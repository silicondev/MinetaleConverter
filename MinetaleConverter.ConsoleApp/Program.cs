using MinetaleConverter.Base;
using MinetaleConverter.Base.Logic.Interfaces;
using MinetaleConverter.Base.Logic.Logging;
using MinetaleConverter.Base.Models.Interfaces;
using MinetaleConverter.Hytale.Models;
using MinetaleConverter.Minecraft.Models;

namespace MinetaleConverter.ConsoleApp
{
    public class Program
    {
        public static bool DEBUGMODE = true;
        public static Logger Logger = new ConsoleLogger();

        public Program()
        {

        }

        static void Main(string[] args)
        {
            try
            {
                string argString = string.Join(' ', args).Trim().ToLower();
                var commands = new Dictionary<string, string[]>();
                foreach (string cmd in argString.Split("--").Where(x => !string.IsNullOrEmpty(x)))
                {
                    string[] parts = cmd.Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    if (parts.Length == 0)
                        continue;
                    string[] param = new string[0];
                    if (parts.Length > 1)
                        param = parts[1..];
                    commands.Add(parts[0], param);
                }

                if (!commands.ContainsKey("mode"))
                    throw new ArgumentException("Missing mode!");

                string mode = commands["mode"][0];

                switch (mode)
                {
                    case "analyse":

                        if (commands.ContainsKey("hytale"))
                        {
                            string[] p = commands["hytale"];
                            if (p.Length == 0)
                                throw new ArgumentException("Missing Hytale world path!");
                            Analyse(WorldType.Hytale, p[0]);
                        }
                        else if (commands.ContainsKey("minecraft"))
                        {
                            string[] p = commands["minecraft"];
                            if (p.Length == 0)
                                throw new ArgumentException("Missing Minecraft world path!");
                            Analyse(WorldType.Minecraft, p[0]);
                        }
                        else
                            throw new Exception("Missing world data!");
                        break;

                    case "copy":

                        if (commands.ContainsKey("hytale"))
                        {
                            string[] p = commands["hytale"];
                            if (p.Length < 2)
                                throw new ArgumentException("Missing Hytale world paths!");
                            Copy(WorldType.Hytale, p[0], p[1]);
                        }
                        else if (commands.ContainsKey("minecraft"))
                        {
                            string[] p = commands["minecraft"];
                            if (p.Length < 2)
                                throw new ArgumentException("Missing Minecraft world paths!");
                            Copy(WorldType.Minecraft, p[0], p[1]);
                        }
                        else
                            throw new Exception("Missing world data!");
                        break;

                    case "converttohytale":

                        if (!commands.ContainsKey("minecraft") || commands["minecraft"].Length == 0)
                            throw new ArgumentException("Missing Minecraft source world path!");
                        if (!commands.ContainsKey("hytale") || commands["hytale"].Length == 0)
                            throw new ArgumentException("Missing Hytale destination world path!");

                        Convert(WorldType.Minecraft, commands["minecraft"][0], WorldType.Hytale, commands["hytale"][0]);
                        break;

                    case "converttominecraft":

                        if (!commands.ContainsKey("hytale") || commands["hytale"].Length == 0)
                            throw new ArgumentException("Missing Hytale source world path!");
                        if (!commands.ContainsKey("minecraft") || commands["minecraft"].Length == 0)
                            throw new ArgumentException("Missing Minecraft destination world path!");

                        Convert(WorldType.Hytale, commands["hytale"][0], WorldType.Minecraft, commands["minecraft"][0]);
                        break;

                    case "create":

                        if (commands.ContainsKey("hytale"))
                        {
                            string[] p = commands["hytale"];
                            if (p.Length == 0)
                                throw new ArgumentException("Missing Hytale world path!");
                            Create(WorldType.Hytale, p[0]);
                        }
                        else if (commands.ContainsKey("minecraft"))
                        {
                            string[] p = commands["minecraft"];
                            if (p.Length == 0)
                                throw new ArgumentException("Missing Minecraft world path!");
                            Create(WorldType.Minecraft, p[0]);
                        }
                        else
                            throw new Exception("Missing world data!");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
        }

        static void Copy(WorldType type, string sourcePath, string destinationPath)
        {
            try
            {
                IWorld world;
                if (type == WorldType.Hytale)
                    world = new hy_World();
                else
                    world = new mc_World();

                world.ImportFile(sourcePath, !DEBUGMODE);
                world.ExportFile(destinationPath, !DEBUGMODE);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
        }

        static void Analyse(WorldType type, string worldPath)
        {
            IWorld world;
            try
            {
                if (type == WorldType.Hytale)
                    world = new hy_World();
                else
                    world = new mc_World();

                world.ImportFile(worldPath, !DEBUGMODE);
            }
            catch (Exception e)
            {
                Logger.Fatal($"Error occurred while parsing {Enum.GetName(type)} world.", e);
                return;
            }

            while (true)
            {
                try
                {
                    Console.Write("Enter command (block x y z / biome x y z / column x z / search id / exit): ");
                    string cmd = Console.ReadLine();
                    string[] cmdParts = cmd.Split(' ');
                    if (cmdParts.Length == 0)
                        continue;
                    bool exit = false;
                    switch (cmdParts[0])
                    {
                        case "exit":
                            exit = true;
                            break;
                        case "block":
                        case "biome":
                        case "chunk":
                            if (cmdParts.Length != 4 ||
                                !int.TryParse(cmdParts[1], out int bx) ||
                                !int.TryParse(cmdParts[2], out int by) ||
                                !int.TryParse(cmdParts[3], out int bz))
                            {
                                Console.WriteLine("Invalid input.");
                                break;
                            }

                            switch (cmdParts[0])
                            {
                                case "block":
                                    Console.WriteLine(world.GetBlockId(bx, by, bz) ?? "Not found.");
                                    break;
                                case "biome":
                                    Console.WriteLine(world.GetBiomeId(bx, by, bz) ?? "Not found.");
                                    break;
                                case "chunk":
                                    Console.WriteLine(world.GetChunkId(bx, by, bz) ?? "Not found.");
                                    break;
                            }
                            break;
                        case "column":
                            if (cmdParts.Length != 3 ||
                                !int.TryParse(cmdParts[1], out int cx) ||
                                !int.TryParse(cmdParts[2], out int cz))
                            {
                                Console.WriteLine("Invalid input.");
                                break;
                            }

                            for (int i = world.HeightLevel - 1; i >= world.BedrockLevel; i--)
                            {
                                Console.WriteLine($"[{i:000}] {world.GetBlockId(cx, i, cz)}");
                            }
                            break;
                        case "cross":
                            if (cmdParts.Length != 4 ||
                                !int.TryParse(cmdParts[1], out int ccx) ||
                                !int.TryParse(cmdParts[2], out int ccz) ||
                                !int.TryParse(cmdParts[3], out int cy))
                            {
                                Console.WriteLine("Invalid input.");
                                break;
                            }

                            var chunk = world.GetChunk(ccx, ccz);
                            if (chunk == null)
                            {
                                Console.WriteLine("Chunk not found.");
                                break;
                            }
                            var data = new List<List<string>>();
                            var legend = new Dictionary<string, string>();
                            //var legend = new List<string>();
                            for (int z = 0; z < chunk.Size; z++)
                            {
                                var list = new List<string>();
                                for (int x = 0; x < chunk.Size; x++)
                                {
                                    string? id = chunk.GetBlock(x, cy, z);
                                    if (string.IsNullOrEmpty(id))
                                        id = "";
                                    string toWrite = "";
                                    string[] parts = id.Split('_');
                                    string[] subParts = parts[0].Split(':');
                                    if (subParts.Length > 1)
                                        parts[0] = subParts[1];
                                    foreach (string part in parts.Where(x => !string.IsNullOrEmpty(x)))
                                        toWrite += part[0];
                                    toWrite = toWrite.Trim().ToUpper();

                                    int i = 0;
                                    while (legend.ContainsKey(toWrite + (i == 0 ? "" : i.ToString())) && legend[toWrite + (i == 0 ? "" : i.ToString())] != id)
                                        i++;
                                    toWrite = toWrite + (i == 0 ? "" : i.ToString());
                                    if (!legend.ContainsKey(toWrite))
                                        legend.Add(toWrite, id);

                                    list.Add(toWrite.ToUpper());
                                }
                                data.Add(list);
                            }

                            int max = data.Select(x => x.Select(y => y.Length).Max()).Max();

                            foreach (var line in data)
                                Console.WriteLine(string.Join(',', line.Select(x => x.PadLeft(max))));
                            Console.WriteLine();
                            Console.WriteLine("Legend:");
                            foreach (var line in legend)
                                Console.WriteLine($"{line.Key} - {line.Value}");
                            break;
                        //case "search":
                        //    if (cmdParts.Length != 2)
                        //    {
                        //        Console.WriteLine("Invalid input.");
                        //        break;
                        //    }
                        //    string blockId = cmdParts[1];
                        //    if (world is mc_World)
                        //    {
                        //        var chunks = ((mc_World)world).FullChunks.Values.Combine().Where(x => x.Sections.Any(y => y.BlockStates.Palettes.Any(z => z.Name == blockId)));
                        //        foreach (var chunk in chunks)
                        //        {
                        //            var sections = chunk.Sections.Where(x => x.BlockStates.Palettes.Any(y => y.Name == blockId));
                        //            foreach (var section in sections)
                        //            {
                        //                var rp = section.BlockStates;
                        //                var indices = PaletteHelper.GetIndices(rp, (x) => x.Name == blockId, 4);
                        //                foreach (var index in indices)
                        //                {
                        //                    (int x, int y, int z) = mc_Section.GetBlockCoordsFromIndex(index);
                        //                    int rx = (chunk.xPos * 16) + x;
                        //                    int rz = (chunk.zPos * 16) + z;
                        //                    int ry = (section.Y * 16) + y;
                        //                    Console.WriteLine($"[{rx},{ry},{rz}]");
                        //                }
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //var chunks = ((hy_World)world).Chunks.Cast<hy_Chunk>().Where(x => x.Components.ChunkColumn.Sections.Any(y => y.Components.BlockPalette.Palettes.Any(z => z == blockId)));
                        //        //foreach (var chunk in chunks)
                        //        //{
                        //        //    var sections = chunk.Components.ChunkColumn.Sections.Where(x => x.Components.BlockPalette.Palettes.Any(y => y == blockId));
                        //        //    foreach (var section in sections)
                        //        //    {
                        //        //        var rp = section.Components.BlockPalette;
                        //        //        var indices = PaletteHelper.GetIndices(rp, (x) => x == blockId, 4);
                        //        //        foreach (var index in indices)
                        //        //        {
                        //        //            (int x, int y, int z) = mc_Section.GetBlockCoordsFromIndex(index);
                        //        //            int rx = (chunk.xPos * 32) + x;
                        //        //            int rz = (chunk.zPos * 32) + z;
                        //        //            int ry = (section.Id * 32) + y;
                        //        //            Console.WriteLine($"[{rx},{ry},{rz}]");
                        //        //        }
                        //        //    }
                        //        //}
                        //    }
                        //    break;
                        case "custom":
                            for (int z = 0; z <= 31; z++)
                            {
                                var list = new List<string>();
                                for (int x = 0; x <= 31; x++)
                                {
                                    // Get blocks
                                    string id = world.GetBlockId(x, 79, z);
                                    string toWrite = "";
                                    string[] parts = id.Split('_');
                                    foreach (string part in parts.Where(x => !string.IsNullOrEmpty(x)))
                                        toWrite += part[0];
                                    int pad = 4 - toWrite.Length;
                                    list.Add(toWrite.PadLeft(4));

                                    // Get chunk block ids
                                    //string chunkInfo = world.GetChunkId(x, 79, z);
                                    //string[] parts = chunkInfo.Split(':');
                                    //string coordsStr = parts[2].Trim();
                                    //string[] coordParts = coordsStr.Split(",");
                                    //int ix = int.Parse(coordParts[0]);
                                    //int iy = int.Parse(coordParts[1]);
                                    //int iz = int.Parse(coordParts[2]);
                                    //string sx = ix.ToString("00").PadLeft(3);
                                    //string sz = iz.ToString("00").PadLeft(3);
                                    //list.Add($"{sx},{sz}");

                                    // Get chunk ids
                                    //string chunkInfo = world.GetChunkId(x, 79, z);
                                    //string[] parts = chunkInfo.Split(' ');
                                    //string chunkId = parts[0];
                                    //string[] idParts = chunkId.Split('_');
                                    //string nums = idParts[1];
                                    //string[] numParts = nums.Split('.');
                                    //int ix = int.Parse(numParts[0]);
                                    //int iz = int.Parse(numParts[1]);
                                    //string sx = ix.ToString("0").PadLeft(2);
                                    //string sz = iz.ToString("0").PadLeft(2);
                                    //list.Add($"{sx},{sz}");

                                    // Get indexes
                                    //string chunkInfo = world.GetChunkId(x, 79, z);
                                    //string[] parts = chunkInfo.Split(':');
                                    //string coordsStr = parts[2].Trim();
                                    //string[] coordParts = coordsStr.Split(",");
                                    //int ix = int.Parse(coordParts[0]);
                                    //int iy = int.Parse(coordParts[1]);
                                    //int iz = int.Parse(coordParts[2]);
                                    //int index = hy_Palette.IndexerFunc(ix, iy, iz);
                                    //list.Add(index.ToString());
                                }
                                Console.WriteLine(string.Join('|', list));
                            }
                            break;
                    }
                    if (exit)
                        break;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        static void Convert(WorldType fromType, string sourcePath, WorldType toType, string destinationPath)
        {

        }

        static void Create(WorldType type, string worldPath)
        {

        }
    }

    public enum WorldType
    {
        Minecraft,
        Hytale
    }
}
