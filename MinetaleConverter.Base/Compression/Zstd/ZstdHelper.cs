using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
    }
}
