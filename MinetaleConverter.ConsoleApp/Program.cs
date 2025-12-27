using MinetaleConverter.Compression;
using MinetaleConverter.Conversion.Minecraft;
using System;

namespace MinetaleConverter.ConsoleApp
{
    internal class Program
    {
        public static bool DEBUGMODE = true;

        static void Main(string[] args)
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

            var success = MinecraftWorld.FromFile(args[0], out MinecraftWorld? world);

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
        }
    }
}
