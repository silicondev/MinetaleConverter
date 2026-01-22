using MinetaleConverter.Compression;
using System;
using MinetaleConverter.Conversion.Minecraft;
using MinetaleConverter.Compression.Palette;
using MinetaleConverter.Conversion.Minecraft.WorldEntities;
using MinetaleConverter.Conversion.Hytale;
using MinetaleConverter.Conversion.Interfaces;

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

                    if (Directory.Exists(outputPath))
                    {
                        if (Directory.GetFiles(outputPath).Length > 0)
                        {
                            if (DEBUGMODE)
                                Directory.Delete(outputPath, true);
                            else
                                throw new Exception("Output folder exists and has data!");
                        }
                    }
                    else
                        Directory.CreateDirectory(outputPath);

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
                        world = new hy_World(logger);
                    }
                    else
                        throw new ArgumentException("Analyze requires either a Minecraft or Hytale world path");

                    bool success = await world.ImportFile(worldPath, !DEBUGMODE);

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
                                if (cmdParts.Length != 4 ||
                                    !int.TryParse(cmdParts[1], out int bx) ||
                                    !int.TryParse(cmdParts[2], out int by) ||
                                    !int.TryParse(cmdParts[3], out int bz))
                                {
                                    Console.WriteLine("Invalid input.");
                                    break;
                                }

                                if (cmdParts[0] == "block")
                                    Console.WriteLine(world.GetBlockId(bx, by, bz));
                                else
                                    Console.WriteLine(world.GetBiomeId(bx, by, bz));
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
                                    var chunks = ((mc_World)world).FullChunks.Where(x => x.Sections.Any(y => y.BlockStates.Palettes.Any(z => z.Name == blockId)));
                                    foreach (var chunk in chunks)
                                    {
                                        var sections = chunk.Sections.Where(x => x.BlockStates.Palettes.Any(y => y.Name == blockId));
                                        foreach (var section in sections)
                                        {
                                            var rp = section.BlockStates;
                                            var indices = PaletteHelper.GetIndices(rp.Palettes, rp.Data, (x) => x.Name == blockId, 4);
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
                default:
                    throw new ArgumentException("Unrecognized mode.");
            }
        }
    }
}
