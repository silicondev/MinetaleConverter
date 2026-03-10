using MinetaleConverter.Base;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale
{
    public static class HytalePaletteManager
    {
        public static (Dictionary<ushort, string> palette, ushort[] indices) Parse(byte[] data, int blockCount)
        {
            var bin = new Binary(data, EndianMode.Big);
            int migrationVersion = bin.Read<int>();

            if (migrationVersion != 10)
                throw new Exception($"Error reading migration version: {migrationVersion} was not 10.");

            var paletteType = (HytalePaletteType)bin.Read<byte>();

            if (paletteType == 0)
                return (new Dictionary<ushort, string>(), new ushort[0]);

            ushort paletteCount = bin.Read<ushort>();
            var paletteDict = new Dictionary<ushort, string>();
            for (ushort i = 0; i < paletteCount; i++)
            {
                ushort key = paletteType == HytalePaletteType.Short ? bin.Read<ushort>() : bin.Read<byte>();
                ushort nameLen = bin.Read<ushort>();
                string id = bin.ReadLength<string>(nameLen);
                ushort count = bin.Read<ushort>();
                if (paletteDict.ContainsKey(key))
                    throw new Exception($"Duplicate palette key {key} found!");
                paletteDict.Add(key, id);
            }

            //int payloadByteLength = blockCount;
            int indexLength = 8;

            switch (paletteType)
            {
                case HytalePaletteType.Half:
                    //payloadByteLength /= 2;
                    indexLength /= 2;
                    break;
                case HytalePaletteType.Short:
                    //payloadByteLength *= 2;
                    indexLength *= 2;
                    break;
            }

            bin.Cut();
            byte[] payload = bin.Bytes.ToArray();

            var indices = new List<ushort>();
            var build = new List<bool>();
            for (int i = 0; i < blockCount * indexLength; i++)
            {
                int payloadIndex = i / 8;
                int bitIndex = i % 8;
                bool bit = (payload[payloadIndex] & (1 << bitIndex)) != 0;
                build.Add(bit);
                if (build.Count == indexLength)
                {
                    byte[] indexBytes = new byte[2];
                    for (int j = 0; j < indexLength; j++)
                    {
                        int buildByteIndex = j / 8;
                        int buildBitIndex = j % 8;
                        byte mask = (byte)((build[j] ? 1 : 0) << buildBitIndex);
                        indexBytes[buildByteIndex] = (byte)(indexBytes[buildByteIndex] | mask);
                    }
                    ushort index = BinaryPrimitives.ReadUInt16BigEndian(indexBytes);
                    indices.Add(index);
                }
            }

            return (paletteDict, indices.ToArray());
        }
    }
}
