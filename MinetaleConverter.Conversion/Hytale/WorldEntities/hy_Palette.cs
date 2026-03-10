using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Palette;
using MinetaleConverter.Conversion.Interfaces;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Palette : hy_Binary//, IVoxelPalette<string>
    {
        public override byte[]? Data
        {
            get => base.Data;
            set
            {
                base.Data = value;
                //populatePalette();
                populate();
            }
        }

        [JsonIgnore]
        public Dictionary<ushort, string> PaletteList { get; private set; } = new Dictionary<ushort, string>();
        [JsonIgnore]
        public ushort[]? PaletteData { get; private set; } = null;

        [JsonIgnore]
        public Func<int, int, int, int> Indexer => IndexerFunc;

        [JsonIgnore]
        public string DefaultVoxel => "Empty";
        [JsonIgnore]
        public string ErrorVoxel => "Rock_Crystal_Green_Block";

        [JsonIgnore]
        private int _minBits = -1;

        [JsonIgnore]
        public int PaletteHeight => 32;
        [JsonIgnore]
        public int PaletteWidth => 32;
        [JsonIgnore]
        public int PaletteDepth => 32;
        [JsonIgnore]
        public bool IsEmpty { get; private set; } = true;

        [JsonIgnore]
        private int _blockCount => PaletteWidth * PaletteHeight * PaletteDepth;

        private byte[] _tempHold = new byte[0];


        [JsonIgnore]
        //public static Func<int, int, int, int> IndexerFunc => (x, y, z) => y * 32 * 32 + z * 32 + x;
        public static Func<int, int, int, int> IndexerFunc => (x, y, z) => (y & 31) << 10 | (z & 31) << 5 | (x & 31);

        private void populate(bool rebuild = true)
        {
            if (Data == null)
                return;

            var bin = new Binary(Data, EndianMode.Big);
            populateBlockPalette(bin);

            bin.Cut();
            _tempHold = bin.Bytes.ToArray();

            //if (!IsEmpty)
            //{
            //    populateBitset(bin);
            //    populateFillerSection(bin);
            //    populateRotationSection(bin);
            //}

            if (rebuild)
                Build(Decompress());
        }

        private void populateBlockPalette(Binary bin)
        {
            int migrationVersion = bin.Read<int>();

            if (migrationVersion != 11)
                throw new Exception($"Error reading migration version: {migrationVersion} was not 11.");

            (Dictionary<ushort, string> palette, ushort[] data) = extractPalette(bin, _blockCount);
            PaletteList = palette;
            PaletteData = data;
            IsEmpty = PaletteList == null || PaletteList.Count == 0;
        }

        private void populateBitset(Binary bin)
        {
            int tickingBlockCount = bin.Read<int>();
            int bitsetLen = bin.Read<int>() / 8;
            ulong[] bitset = new ulong[bitsetLen];
            for (int i = 0; i < bitsetLen; i++)
            {
                bitset[i] = bin.Read<ulong>();
            }
        }

        private void populateFillerSection(Binary bin)
        {
            (Dictionary<ushort, string> palette, ushort[] data) = extractPalette(bin, _blockCount);
            int k = 0;
        }

        private void populateRotationSection(Binary bin)
        {
            (Dictionary<ushort, string> palette, ushort[] data) = extractPalette(bin, _blockCount);
            int k = 0;
        }

        private static (Dictionary<ushort, string> palette, ushort[] data) extractPalette(Binary bin, int blockCount)
        {
            var palette = new Dictionary<ushort, string>();
            ushort[] data = new ushort[0];

            var paletteType = (HytalePaletteType)bin.Read<byte>();

            if (paletteType == HytalePaletteType.Empty)
                return (palette, data);

            ushort paletteCount = bin.Read<ushort>();
            for (ushort i = 0; i < paletteCount; i++)
            {
                ushort key = paletteType == HytalePaletteType.Short ? bin.Read<ushort>() : bin.Read<byte>();
                ushort nameLen = bin.Read<ushort>();
                string id = bin.ReadLength<string>(nameLen);
                ushort count = bin.Read<ushort>();
                if (palette.ContainsKey(key))
                    throw new Exception($"Duplicate palette key {key} found!");
                palette.Add(key, id);
            }

            int indexLength = 8;
            int payloadLength = blockCount;

            switch (paletteType)
            {
                case HytalePaletteType.Half:
                    indexLength /= 2;
                    payloadLength /= 2;
                    break;
                case HytalePaletteType.Short:
                    indexLength *= 2;
                    payloadLength *= 2;
                    break;
            }

            byte[] payload = bin.Subset(payloadLength);

            var indices = new List<ushort>();
            var build = new List<bool>();
            for (int i = 0; i < blockCount * indexLength; i++)
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

            data = indices.ToArray();
            return (palette, data);
        }

        //private void populatePalette(bool rebuild = true)
        //{
        //    if (Data == null)
        //        return;

        //    PaletteList.Clear();

        //    var bin = new Binary(Data, EndianMode.Big);
        //    int migrationVersion = bin.Read<int>();

        //    if (migrationVersion != 11)
        //        throw new Exception($"Error reading migration version: {migrationVersion} was not 11.");

        //    var paletteType = (HytalePaletteType)bin.Read<byte>();

        //    if (paletteType == HytalePaletteType.Empty)
        //    {
        //        IsEmpty = true;
        //        return;
        //    }

        //    ushort paletteCount = bin.Read<ushort>();
        //    for (ushort i = 0; i < paletteCount; i++)
        //    {
        //        ushort key = paletteType == HytalePaletteType.Short ? bin.Read<ushort>() : bin.Read<byte>();
        //        ushort nameLen = bin.Read<ushort>();
        //        string id = bin.ReadLength<string>(nameLen);
        //        ushort count = bin.Read<ushort>();
        //        if (PaletteList.ContainsKey(key))
        //            throw new Exception($"Duplicate palette key {key} found!");
        //        PaletteList.Add(key, id);
        //    }

        //    int indexLength = 8;
        //    int payloadLength = _blockCount;

        //    switch (paletteType)
        //    {
        //        case HytalePaletteType.Half:
        //            indexLength /= 2;
        //            payloadLength /= 2;
        //            break;
        //        case HytalePaletteType.Short:
        //            indexLength *= 2;
        //            payloadLength *= 2;
        //            break;
        //    }

        //    //bin.Cut();
        //    //byte[] payload = bin.Bytes.ToArray();
        //    byte[] payload = bin.Subset(payloadLength);

        //    var indices = new List<ushort>();
        //    var build = new List<bool>();
        //    for (int i = 0; i < _blockCount * indexLength; i++)
        //    {
        //        int payloadIndex = i / 8;
        //        int bitIndex = i % 8;
        //        bool bit = (payload[payloadIndex] & (1 << (7 - bitIndex))) != 0;
        //        build.Add(bit);
        //        if (build.Count == indexLength)
        //        {
        //            build.Reverse();
        //            byte[] indexBytes = { 0, 0 };
        //            for (int j = 0; j < indexLength; j++)
        //            {
        //                if (build[j])
        //                {
        //                    int buildByteIndex = j / 8;
        //                    int buildBitIndex = j % 8;
        //                    byte mask = (byte)(1 << buildBitIndex);
        //                    indexBytes[buildByteIndex] = (byte)(indexBytes[buildByteIndex] | mask);
        //                }
        //            }
        //            ushort index = BinaryPrimitives.ReadUInt16LittleEndian(indexBytes);
        //            indices.Add(index);
        //            build.Clear();
        //        }
        //    }

        //    PaletteData = indices.ToArray();

        //    bin.Cut();
        //    _tempHold = bin.Bytes.ToArray();

        //    char[] chars = _tempHold.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();

        //    int tickingBlockCount = bin.Read<int>();
        //    int bitsetLen = bin.Read<int>();
        //    long[] bitset = new long[bitsetLen];
        //    for (int i = 0; i < bitsetLen; i++)
        //    {
        //        bitset[i] = bin.Read<long>();
        //    }

        //    var fillerSectionType = (HytalePaletteType)bin.Read<byte>();
        //    ushort fillerSectionPaletteCount = bin.Read<ushort>();


        //    // DEBUG
        //    if (rebuild)
        //        Build(Decompress());

        //    IsEmpty = false;
        //}

        public string[] Decompress()
        {
            var list = new List<string>();
            for (int i = 0; i < PaletteData.Length; i++)
            {
                ushort key = PaletteData[i];
                if (!PaletteList.ContainsKey(key))
                    list.Add(ErrorVoxel);
                else
                    list.Add(PaletteList[key]);
            }
            return list.ToArray();
        }

        public string Get(int x, int y, int z)
        {
            if (IsEmpty)
                return DefaultVoxel;

            int paletteCount = PaletteList.Count();
            if (paletteCount == 1)
                return PaletteList[0];

            if (PaletteData == null)
                return DefaultVoxel;

            int index = Indexer(x, y, z);

            // DEBUG
            //index += 1;

            ushort key = PaletteData[index];
            if (!PaletteList.ContainsKey(key))
                return ErrorVoxel;
            else
                return PaletteList[key];
        }

        public void Set(string value, int x, int y, int z)
        {
            int index = Indexer(x, y, z);

            if (PaletteData == null || PaletteList == null || PaletteList.Count == 0 || !PaletteList.ContainsValue(value))
            {
                var list = DefaultVoxel.Stretch(_blockCount);
                list[index] = value;
                Build(list);
            }
            else
            {
                ushort key = PaletteList.KeyOf(value);
                PaletteData[index] = key;
            }
        }

        public void Build(string[] list, bool repopulate = true)
        {
            var bin = new Binary(EndianMode.Big);
            bin.Write(11);

            string[] distinct = list.Distinct().ToArray();

            ushort paletteCount = (ushort)distinct.Length;
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            var paletteType = HytalePaletteType.Full;
            if (bitCount <= 4)
            {
                paletteType = HytalePaletteType.Half;
                bitCount = 4;
            }
            else if (bitCount > 8)
            {
                paletteType = HytalePaletteType.Short;
                bitCount = 16;
            }
            else
                bitCount = 8;

            if (paletteCount == 0)
                paletteType = HytalePaletteType.Empty;

            bin.Write((byte)paletteType);

            if (paletteType == HytalePaletteType.Empty)
            {
                if (repopulate)
                    populate(false);
                return;
            }

            bin.Write(paletteCount);

            for (ushort i = 0; i < distinct.Length; i++)
            {
                if (paletteType == HytalePaletteType.Short)
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
                    case HytalePaletteType.Short:
                        bytes.AddRange(arr);
                        break;
                    case HytalePaletteType.Full:
                        bytes.Add(arr[1]);
                        break;
                    case HytalePaletteType.Half:
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

            bin.Bytes.AddRange(_tempHold);

            base.Data = bin.Bytes.ToArray();

            if (repopulate)
                populate(false);
                //populatePalette(false);
        }
    }
}
