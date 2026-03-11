using MinetaleConverter.Base.Logic.Compression.Palette;
using MinetaleConverter.Base.Logic.Data;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Hytale.Logic
{
    public class hy_BiomePalette : DataPalette<uint, byte, string>
    {
        [JsonIgnore]
        public override string DefaultPalette => "";

        [JsonIgnore]
        public override string ErrorPalette => "";

        [JsonIgnore]
        public override int Length { get; protected set; }

        [JsonIgnore]
        public override Func<int, int, int, int> Indexer => throw new NotImplementedException();

        private int _migrationVer = 11;
        private byte[] _tempHold = new byte[0];
        private int _bitCount = 1;

        protected override void populate(bool rebuild = true)
        {
            if (Data == null)
                return;

            var bin = new Binary(Data, EndianMode.Big);
            populateBiomePalette(bin);

            if (rebuild)
            {
                bin.Cut();
                _tempHold = bin.Bytes.ToArray();
                Build(Decompress(_tempHold));
            }
        }

        protected override void Reconstruct(string[] list)
        {

        }

        private void populateBiomePalette(Binary bin)
        {
            int migrationVersion = bin.Read<int>();

            if (migrationVersion != _migrationVer)
                throw new Exception($"Error reading migration version: {migrationVersion} was not {_migrationVer}.");

            (Dictionary<uint, string> palette, uint[] data) = extractPalette(bin, Length);
            PaletteList = palette;
            //PaletteData = data;
        }

        private (Dictionary<uint, string> palette, uint[] data) extractPalette(Binary bin, int indexCount)
        {
            var palette = new Dictionary<uint, string>();
            uint[] data = new uint[0];

            char[] chars = bin.Bytes.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();

            var paletteType = (hy_PaletteType)bin.Read<byte>();

            if (paletteType == hy_PaletteType.Empty)
                return (palette, data);

            ushort paletteCount = bin.Read<ushort>();
            for (ushort i = 0; i < paletteCount; i++)
            {
                ushort key = paletteType == hy_PaletteType.Short ? bin.Read<ushort>() : bin.Read<byte>();
                ushort nameLen = bin.Read<ushort>();
                string id = bin.ReadLength<string>(nameLen);
                ushort count = bin.Read<ushort>();
                if (palette.ContainsKey(key))
                    throw new Exception($"Duplicate palette key {key} found!");
                palette.Add(key, id);
            }

            _bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));

            int payloadLength = indexCount;

            switch (paletteType)
            {
                case hy_PaletteType.Half:
                    payloadLength /= 2;
                    break;
                case hy_PaletteType.Short:
                    payloadLength *= 2;
                    break;
            }

            byte[] payload = bin.Subset(payloadLength);

            var indices = new List<uint>();
            var build = new List<bool>();
            for (int i = 0; i < indexCount * _bitCount; i++)
            {
                int payloadIndex = i / 8;
                int bitIndex = i % 8;
                bool bit = (payload[payloadIndex] & (1 << (7 - bitIndex))) != 0;
                build.Add(bit);
                if (build.Count == _bitCount)
                {
                    build.Reverse();
                    byte[] indexBytes = { 0, 0, 0, 0 };
                    for (int j = 0; j < _bitCount; j++)
                    {
                        if (build[j])
                        {
                            int buildByteIndex = j / 8;
                            int buildBitIndex = j % 8;
                            byte mask = (byte)(1 << buildBitIndex);
                            indexBytes[buildByteIndex] = (byte)(indexBytes[buildByteIndex] | mask);
                        }
                    }
                    uint index = BinaryPrimitives.ReadUInt32LittleEndian(indexBytes);
                    indices.Add(index);
                    build.Clear();
                }
            }

            data = indices.ToArray();
            return (palette, data);
        }

        public override string[] Decompress(byte[] arr)
        {
            throw new NotImplementedException();
        }

        public override string? GetAtIndex(int index)
        {
            throw new NotImplementedException();
        }

        public override void SetAtIndex(int index, string value)
        {
            throw new NotImplementedException();
        }
    }
}
