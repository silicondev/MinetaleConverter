using MinetaleConverter.Compression;
using System;
using MinetaleConverter.Conversion.Minecraft;

namespace MinetaleConverter.ConsoleApp
{
    internal class Program
    {
        public static bool DEBUGMODE = true;

        static async Task Main(string[] args)
        {
            if (args.Length < 2)
                return;

            if (Directory.Exists(args[1]))
            {
                if (DEBUGMODE)
                    Directory.Delete(args[1], true);
                else
                    return;
            }

            Directory.CreateDirectory(args[1]);
            var logger = new ConsoleLogger();
            var world = new MinecraftWorld(logger);
            var success = await world.ImportFile(args[0]);

            if (!success || world == null)
                return;

            for (int i = 0; i < world.bChunkDataNBT.Count; i++)
            {
                var chunk = world.bChunkDataNBT[i];
                if (chunk != null)
                {
                    using (var fs = new FileStream(@$"{args[1]}\chunk-{i}.nbt", FileMode.CreateNew))
                    {
                        foreach (var b in chunk)
                        {
                            fs.WriteByte(b);
                        }
                    }
                }
            }

            while (true)
            {
                Console.Write("Enter coordinates (\"x,y,z\"), or 'quit' to quit: ");
                string? coordStr = Console.ReadLine();
                if (string.IsNullOrEmpty(coordStr))
                {
                    Console.WriteLine("No input given.");
                    continue;
                }
                if (coordStr.Trim().ToLower() == "quit")
                    break;
                var spl = coordStr.Replace(" ", "").Split(",");
                if (spl.Length != 3 ||
                    !int.TryParse(spl[0], out int x) ||
                    !int.TryParse(spl[1], out int y) ||
                    !int.TryParse(spl[2], out int z))
                {
                    Console.WriteLine("Invalid input.");
                    continue;
                }
                Console.WriteLine(world.GetBiomeId(x, y, z));
            }
        }
    }
}
