using MinetaleConverter.Compression;
using System;
using MinetaleConverter.Conversion.Minecraft;
using MinetaleConverter.Conversion.Minecraft.WorldEntities;
using MinetaleConverter.Conversion.Hytale;
using MinetaleConverter.Conversion.Interfaces;
using MinetaleConverter.Base.Compression.Palette;
using MinetaleConverter.Conversion.Hytale.WorldEntities;
using MinetaleConverter.Base;

namespace MinetaleConverter.ConsoleApp
{
    internal class Program
    {
        public static bool DEBUGMODE = true;

        static async Task Main(string[] args)
        {
            string argString = string.Join(' ', args);
            string[] param = argString.Split("--").Select(x => x.Trim().ToLower()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var commandDict = new Dictionary<string, string>();
            foreach (var p in param)
            {
                string[] parts = p.Split(' ');
                if (parts.Length == 0)
                    continue;
                string key = parts[0];
                string val = parts.Length > 1 ? string.Join(' ', parts[1..]) : "";
                commandDict[key] = val;
            }

            if (!commandDict.ContainsKey("mode"))
                throw new ArgumentException("Missing mode!");

            string mode = commandDict["mode"];

            var logger = new ConsoleLogger();

            switch (mode)
            {
                case "analyze":
                    if (!commandDict.ContainsKey("output"))
                        throw new ArgumentException("Requires output path for analysis files.");

                    string outputPath = commandDict["output"];

                    string worldPath;
                    IWorld world;
                    if (commandDict.ContainsKey("minecraft"))
                    {
                        worldPath = commandDict["minecraft"];
                        world = new mc_World(logger);
                    }
                    else if (commandDict.ContainsKey("hytale"))
                    {
                        worldPath = commandDict["hytale"];
                        var hyWorld = new hy_World(logger);
                        hyWorld.DebugType = DebugType.Export;
                        world = hyWorld;
                    }
                    else
                        throw new ArgumentException("Analyze requires either a Minecraft or Hytale world path");

                    bool success = await world.ImportFile(worldPath, !DEBUGMODE);

                    if (Directory.Exists(outputPath))
                    {
                        if (Directory.GetFiles(outputPath).Length > 0)
                        {
                            if (DEBUGMODE)
                            {
                                Directory.Delete(outputPath, true);
                                Directory.CreateDirectory(outputPath);
                            }
                            else
                                throw new Exception("Output folder exists and has data!");
                        }
                    }
                    else
                        Directory.CreateDirectory(outputPath);

                    if (world is mc_World)
                    {
                        var nbt = ((mc_World)world).bChunkDataNBT;
                        for (int i = 0; i < nbt.Count; i++)
                        {
                            var chunk = nbt[i];
                            if (chunk != null)
                            {
                                using (var fs = new FileStream(@$"{outputPath}\chunk-{i}.nbt", FileMode.CreateNew))
                                {
                                    foreach (var b in chunk)
                                    {
                                        fs.WriteByte(b);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        copyDir(worldPath, outputPath);
                        string worldFolder = Path.Combine(outputPath, @"universe\worlds\default\chunks");
                        Directory.Delete(worldFolder, true);
                        Directory.CreateDirectory(worldFolder);
                        world.ExportFile(worldFolder, !DEBUGMODE);

                        var bson = ((hy_World)world).ChunkBsonFiles;
                        if (bson.Any())
                        {
                            if (!Directory.Exists($"{outputPath}-chunks"))
                                Directory.CreateDirectory($"{outputPath}-chunks");
                            else
                            {
                                Directory.Delete($"{outputPath}-chunks", true);
                                Directory.CreateDirectory($"{outputPath}-chunks");
                            }
                            for (int i = 0; i < bson.Count; i++)
                            {
                                var chunk = bson[i];
                                if (chunk != null)
                                {
                                    using (var fs = new FileStream(@$"{outputPath}-chunks\chunk-{i}.json", FileMode.CreateNew))
                                    using (var writer = new StreamWriter(fs))
                                    {
                                        writer.WriteLine(chunk);
                                    }
                                }
                            }
                        }
                    }

                    while (true)
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
                                        Console.WriteLine(world.GetBlockId(bx, by, bz));
                                        break;
                                    case "biome":
                                        Console.WriteLine(world.GetBiomeId(bx, by, bz));
                                        break;
                                    case "chunk":
                                        Console.WriteLine(world.GetChunkId(bx, by, bz));
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

                                for (int i = world.HeightLevel; i >= world.BedrockLevel; i--)
                                {
                                    Console.WriteLine($"[{i:000}] {world.GetBlockId(cx, i, cz)}");
                                }
                                break;
                            case "search":
                                if (cmdParts.Length != 2)
                                {
                                    Console.WriteLine("Invalid input.");
                                    break;
                                }
                                string blockId = cmdParts[1];
                                if (world is mc_World)
                                {
                                    var chunks = ((mc_World)world).FullChunks.Values.Combine().Where(x => x.Sections.Any(y => y.BlockStates.Palettes.Any(z => z.Name == blockId)));
                                    foreach (var chunk in chunks)
                                    {
                                        var sections = chunk.Sections.Where(x => x.BlockStates.Palettes.Any(y => y.Name == blockId));
                                        foreach (var section in sections)
                                        {
                                            var rp = section.BlockStates;
                                            var indices = PaletteHelper.GetIndices(rp, (x) => x.Name == blockId, 4);
                                            foreach (var index in indices)
                                            {
                                                (int x, int y, int z) = mc_Section.GetBlockCoordsFromIndex(index);
                                                int rx = (chunk.xPos * 16) + x;
                                                int rz = (chunk.zPos * 16) + z;
                                                int ry = (section.Y * 16) + y;
                                                Console.WriteLine($"[{rx},{ry},{rz}]");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //var chunks = ((hy_World)world).Chunks.Cast<hy_Chunk>().Where(x => x.Components.ChunkColumn.Sections.Any(y => y.Components.BlockPalette.Palettes.Any(z => z == blockId)));
                                    //foreach (var chunk in chunks)
                                    //{
                                    //    var sections = chunk.Components.ChunkColumn.Sections.Where(x => x.Components.BlockPalette.Palettes.Any(y => y == blockId));
                                    //    foreach (var section in sections)
                                    //    {
                                    //        var rp = section.Components.BlockPalette;
                                    //        var indices = PaletteHelper.GetIndices(rp, (x) => x == blockId, 4);
                                    //        foreach (var index in indices)
                                    //        {
                                    //            (int x, int y, int z) = mc_Section.GetBlockCoordsFromIndex(index);
                                    //            int rx = (chunk.xPos * 32) + x;
                                    //            int rz = (chunk.zPos * 32) + z;
                                    //            int ry = (section.Id * 32) + y;
                                    //            Console.WriteLine($"[{rx},{ry},{rz}]");
                                    //        }
                                    //    }
                                    //}
                                }
                                break;
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

                    break;
                case "converttohytale":
                    break;
                case "converttominecraft":
                    break;
                case "create":
                    string createWorldPath;
                    IWorld createWorld;
                    if (commandDict.ContainsKey("minecraft"))
                    {
                        createWorldPath = commandDict["minecraft"];
                        createWorld = new mc_World(logger);
                    }
                    else if (commandDict.ContainsKey("hytale"))
                    {
                        createWorldPath = commandDict["hytale"];
                        var hyWorld = new hy_World(logger);
                        hyWorld.DebugType = DebugType.Export;
                        createWorld = hyWorld;

                        double progress = 0;
                        double total = 1000 * 1000;
                        for (int z = 0; z < 1000; z++)
                        {
                            for (int x = 0; x < 1000; x++)
                            {
                                createWorld.SetBlockId("Rock_Stone", x, 150, z);
                                progress++;
                                Console.Write($"\r{(progress / total) * 100d:N3}%                                                  ");
                            }
                        }
                    }
                    else
                        throw new ArgumentException("Analyze requires either a Minecraft or Hytale world path");

                    if (!Directory.Exists(createWorldPath))
                        Directory.CreateDirectory(createWorldPath);
                    else
                    {
                        Directory.Delete(createWorldPath, true);
                        Directory.CreateDirectory(createWorldPath);
                    }

                    await createWorld.ExportFile(createWorldPath, !DEBUGMODE);
                    break;
                default:
                    throw new ArgumentException("Unrecognized mode.");
            }
        }

        private static void copyDir(string input, string output)
        {
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            foreach (var file in Directory.GetFiles(input))
            {
                string fileName = Path.GetFileName(file);
                string newFile = Path.Combine(output, fileName);
                File.Copy(file, newFile);
            }

            foreach (var dir in Directory.GetDirectories(input))
            {
                string dirName = Path.GetFileName(dir);
                string newDir = Path.Combine(output, dirName);
                copyDir(dir, newDir);
            }
        }
    }
}
