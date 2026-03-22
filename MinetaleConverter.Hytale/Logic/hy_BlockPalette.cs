using MinetaleConverter.Base.Logic.Compression.Palette;
using MinetaleConverter.Base.Logic.Data;
using MinetaleConverter.Base.Logic.Extensions;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinetaleConverter.Hytale.Logic
{
    public class hy_BlockPalette : DataPalette<ushort, byte, string>
    {
        [JsonIgnore]
        public override string DefaultPalette => "Empty";
        [JsonIgnore]
        public override string ErrorPalette => "Rock_Crystal_Green_Block";
        [JsonIgnore]
        public override Func<int, int, int, int> Indexer => IndexerFunc;
        [JsonIgnore]
        public override int Length
        {
            get => 32 * 32 * 32;
            protected set { }
        }
        

        [JsonIgnore]
        private byte[] _tempHold = new byte[0];
        [JsonIgnore]
        private int _migrationVer = 11;
        [JsonIgnore]
        private hy_PaletteType _blockPaletteType = hy_PaletteType.Empty;
        [JsonIgnore]
        private hy_PaletteType _fillerPaletteType = hy_PaletteType.Empty;
        [JsonIgnore]
        private hy_PaletteType _rotationPaletteType = hy_PaletteType.Empty;
        [JsonIgnore]
        private int _paletteOffset = 0;


        [JsonIgnore]
        public static Func<int, int, int, int> IndexerFunc => (x, y, z) => (y & 31) << 10 | (z & 31) << 5 | (x & 31);

        public override string? GetAtIndex(int index)
        {
            if (PaletteList.Count == 0)
                return null;
            if (PaletteList.Count == 1)
                return PaletteList.First().Value;

            int indexLength = 8;

            switch (_blockPaletteType)
            {
                case hy_PaletteType.Half:
                    indexLength /= 2;
                    break;
                case hy_PaletteType.Short:
                    indexLength *= 2;
                    break;
            }

            var build = new List<bool>();
            for (int i = index * indexLength; i < (index + 1) * indexLength; i++)
            {
                int payloadIndex = i / 8;
                int bitIndex = i % 8;
                bool bit = (Data[payloadIndex + _paletteOffset] & (1 << (7 - bitIndex))) != 0;
                build.Add(bit);
            }

            build.Reverse();
            byte[] indexBytes = { 0, 0 };
            for (int j = 0; j < indexLength; j++)
            {
                if (build[j])
                {
                    int buildByteIndex = j / 8;
                    int buildBitIndex = j % 8;
                    byte mask = (byte)(1 << buildBitIndex);
                    indexBytes[buildByteIndex] = (byte)(indexBytes[buildByteIndex] | mask);
                }
            }
            ushort paletteIndex = BinaryPrimitives.ReadUInt16LittleEndian(indexBytes);
            return PaletteList[paletteIndex];
        }

        public override void SetAtIndex(int index, string value)
        {
            if (!PaletteList.ContainsValue(value))
            {
                var list = DefaultPalette.Stretch(Length);
                list[index] = value;
                Build(list);
                return;
            }

            ushort key = PaletteList.KeyOf(value);
            byte[] keyBytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(keyBytes, key);

            var bits = new List<bool>();
            int indexLength = 8;

            switch (_blockPaletteType)
            {
                case hy_PaletteType.Full:
                    byte bf = keyBytes[1];
                    for (int i = 0; i < 8; i++)
                        bits.Add((bf & 1 << (7 - i)) != 0);
                    break;
                case hy_PaletteType.Half:
                    indexLength /= 2;
                    byte bh = keyBytes[1];
                    for (int i = 0; i < 4; i++)
                        bits.Add((bh & 1 << (3 - i)) != 0);
                    break;
                case hy_PaletteType.Short:
                    indexLength *= 2;
                    for (int b = 0; b < 2; b++)
                    {
                        byte bs = keyBytes[b];
                        for (int i = 0; i < 8; i++)
                            bits.Add((bs & 1 << (7 - i)) != 0);
                    }
                    break;
            }

            for (int i = 0; i < indexLength; i++)
            {
                int indexBit = (index * indexLength) + i;
                int payloadIndex = indexBit / 8;
                int bitIndex = indexBit % 8;

                byte b = Data[payloadIndex + _paletteOffset];
                byte mask = (byte)(1 << (7 - bitIndex));

                if (bits[i])
                    Data[payloadIndex] = (byte)(b | mask);
                else
                    Data[payloadIndex] = (byte)(b & ~mask);
            }
        }

        protected override void populate(bool rebuild = true)
        {
            if (Data == null)
                return;

            var bin = new Binary(Data, EndianMode.Big);
            populateBlockPalette(bin);

            //if (!IsEmpty)
            //{
            //    populateBitset(bin);
            //    populateFillerSection(bin);
            //    populateRotationSection(bin);
            //}

            _paletteOffset = bin.Seek;
            if (rebuild)
            {
                bin.Cut();
                Build(Decompress(bin.Bytes.ToArray()));
            }
        }

        private void populateBlockPalette(Binary bin)
        {
            int migrationVersion = bin.Read<int>();

            if (migrationVersion != _migrationVer)
                throw new Exception($"Error reading migration version: {migrationVersion} was not {_migrationVer}.");

            _blockPaletteType = (hy_PaletteType)bin.Read<byte>();

            if (_blockPaletteType == hy_PaletteType.Empty)
                return;

            PaletteList = extractPalette(bin, _blockPaletteType);
        }

        private void populateBitset(Binary bin)
        {
            ushort tickingBlockCount = bin.Read<ushort>();
            ushort bitsetLen = bin.Read<ushort>();
            ulong[] bitset = new ulong[bitsetLen];
            for (int i = 0; i < bitsetLen; i++)
            {
                bitset[i] = bin.Read<ulong>();
            }
        }

        private void populateFillerSection(Binary bin)
        {
            _fillerPaletteType = (hy_PaletteType)bin.Read<byte>();

            if (_fillerPaletteType == hy_PaletteType.Empty)
                return;

            Dictionary<ushort, string> palette = extractPalette(bin, _fillerPaletteType);
            int k = 0;
        }

        private void populateRotationSection(Binary bin)
        {
            _rotationPaletteType = (hy_PaletteType)bin.Read<byte>();

            if (_rotationPaletteType == hy_PaletteType.Empty)
                return;

            Dictionary<ushort, string> palette = extractPalette(bin, _rotationPaletteType);
            int k = 0;
        }

        private Dictionary<ushort, string> extractPalette(Binary bin, hy_PaletteType paletteType)
        {
            var palette = new Dictionary<ushort, string>();

            char[] chars = bin.Bytes.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();

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

            return palette;
        }

        public override string[] Decompress(byte[] arr)
        {
            if (_blockPaletteType == hy_PaletteType.Empty)
                return new string[0];

            var bin = new Binary(arr, EndianMode.Big);

            int indexLength = 8;
            int payloadLength = Length;

            switch (_blockPaletteType)
            {
                case hy_PaletteType.Half:
                    indexLength /= 2;
                    payloadLength /= 2;
                    break;
                case hy_PaletteType.Short:
                    indexLength *= 2;
                    payloadLength *= 2;
                    break;
            }

            byte[] payload = bin.Subset(payloadLength);
            bin.Cut();
            _tempHold = bin.Bytes.ToArray();

            var indices = new List<ushort>();
            var build = new List<bool>();
            for (int i = 0; i < Length * indexLength; i++)
            {
                int payloadIndex = i / 8;
                int bitIndex = i % 8;
                bool bit = (payload[payloadIndex] & (1 << (7 - bitIndex))) != 0;
                build.Add(bit);
                if (build.Count == indexLength)
                {
                    build.Reverse();
                    byte[] indexBytes = { 0, 0 };
                    for (int j = 0; j < indexLength; j++)
                    {
                        if (build[j])
                        {
                            int buildByteIndex = j / 8;
                            int buildBitIndex = j % 8;
                            byte mask = (byte)(1 << buildBitIndex);
                            indexBytes[buildByteIndex] = (byte)(indexBytes[buildByteIndex] | mask);
                        }
                    }
                    ushort index = BinaryPrimitives.ReadUInt16LittleEndian(indexBytes);
                    indices.Add(index);
                    build.Clear();
                }
            }

            return indices.Select(x => PaletteList[x]).ToArray();
        }

        protected override void Reconstruct(string[] list)
        {
            var bin = new Binary(EndianMode.Big);

            // Block palette

            bin.Write(_migrationVer);

            string[] distinct = list.Distinct().ToArray();

            ushort paletteCount = (ushort)distinct.Length;
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            var paletteType = hy_PaletteType.Full;
            if (bitCount <= 4)
            {
                paletteType = hy_PaletteType.Half;
                bitCount = 4;
            }
            else if (bitCount > 8)
            {
                paletteType = hy_PaletteType.Short;
                bitCount = 16;
            }
            else
                bitCount = 8;

            if (paletteCount == 0)
                paletteType = hy_PaletteType.Empty;

            bin.Write((byte)paletteType);

            if (paletteType == hy_PaletteType.Empty)
                return;

            bin.Write(paletteCount);

            for (ushort i = 0; i < distinct.Length; i++)
            {
                if (paletteType == hy_PaletteType.Short)
                    bin.Write(i);
                else
                {
                    var keyArr = new byte[2];
                    BinaryPrimitives.WriteUInt16BigEndian(keyArr, i);
                    bin.Write(keyArr[1]);
                }
                string value = distinct[i];
                bin.Write((ushort)value.Length);
                bin.Write(value);
                bin.Write((ushort)list.Count(x => x == value));
            }

            var bytes = new List<byte>();
            var bits = new List<bool>();
            foreach (string id in list)
            {
                ushort index = (ushort)distinct.FindNextIndex(x => x == id);
                byte[] arr = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(arr, index);
                switch (paletteType)
                {
                    case hy_PaletteType.Short:
                        bytes.AddRange(arr);
                        break;
                    case hy_PaletteType.Full:
                        bytes.Add(arr[1]);
                        break;
                    case hy_PaletteType.Half:
                        byte b = arr[1];
                        for (int i = 0; i < 4; i++)
                        {
                            byte mask = (byte)(1 << (3 - i));
                            bits.Add((b & mask) != 0);
                        }
                        if (bits.Count == 8)
                        {
                            byte nb = 0;
                            for (int i = 0; i < 8; i++)
                            {
                                byte mask = (byte)((bits[i] ? 1 : 0) << (7 - i));
                                nb = (byte)(nb | mask);
                            }
                            bytes.Add(nb);
                            bits.Clear();
                        }
                        break;
                }
            }
            bin.Bytes.AddRange(bytes);

            // Ticking blocks

            // No ticking blocks
            //bin.Write<ushort>(0);
            //bin.Write<ushort>(0);

            // Filler section

            // No filler
            //bin.Write((byte)HytalePaletteType.Empty);

            // Rotation section

            // No rotation
            //bin.Write((byte)HytalePaletteType.Empty);

            // Light data !! NOT ENOUGH !!

            // No light (recalculated?)
            //bin.Write<ushort>(0);
            //bin.Write<byte>(0);
            //bin.Write<ushort>(0);
            //bin.Write<byte>(0);
            //bin.Write<ushort>(0);
            //bin.Write<ushort>(0);


            bin.Bytes.AddRange(_tempHold);

            _data = bin.Bytes.ToArray();
        }
    }
}
