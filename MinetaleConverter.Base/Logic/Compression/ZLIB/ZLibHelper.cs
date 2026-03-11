using System.IO;
using System.IO.Compression;

namespace MinetaleConverter.Base.Logic.Compression.Zlib
{
    public static class ZLibHelper
    {
        public static bool Decompress(string inputPath, string outputPath)
        {
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
                return false;

            if (!File.Exists(inputPath) || File.Exists(outputPath))
                return false;

            byte[] compressedBuff = [];

            using (var fs = new FileStream(inputPath, FileMode.Open))
            using (var reader = new BinaryReader(fs))
            {
                compressedBuff = reader.ReadBytes((int)fs.Length);
            }

            bool result = Decompress(compressedBuff, out byte[]? decompressedBuff);

            if (!result || decompressedBuff == null)
                return false;

            using (var fs = new FileStream(outputPath, FileMode.CreateNew))
            {
                foreach (var b in decompressedBuff)
                {
                    fs.WriteByte(b);
                }
            }

            return true;
        }

        public static bool Decompress(byte[] input, out byte[]? output)
        {
            output = [];
            try
            {
                int read = 0;
                int chunkSize = 4096;
                var outputList = new List<byte>();
                using (var memStream = new MemoryStream(input, false))
                using (var deflateStream = new DeflateStream(memStream, CompressionMode.Decompress))
                {
                    do
                    {
                        var chunk = new byte[chunkSize];
                        read = deflateStream.Read(chunk, 0, chunkSize);
                        outputList.AddRange(chunk.ToList().GetRange(0, read));
                    } while (read == chunkSize);
                }
                output = outputList.ToArray();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
