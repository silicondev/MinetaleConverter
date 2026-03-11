
using ZstdSharp;

namespace MinetaleConverter.Base.Compression.Zstd
{
    public static class ZstdHelper
    {
        public static byte[] Decompress(byte[] compressedData, int srcSize)
        {
            var decomp = new Decompressor();
            byte[] output = new byte[srcSize];
            int result = decomp.Unwrap(compressedData, output, 0);
            return output;
        }

        public static byte[] Compress(byte[] srcData)
        {
            var comp = new Compressor();
            var result = comp.Wrap(srcData);
            return result.ToArray();
        }
    }
}
