using MinetaleConverter.Base;
using MinetaleConverter.Conversion.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Palette_old : hy_Binary, IVoxelPalette<string>
    {
        public override byte[]? Data
        {
            get => base.Data;
            set
            {
                base.Data = value;
                populatePalette();
            }
        }

        [JsonIgnore]
        public Dictionary<int, string> PaletteList { get; private set; } = new Dictionary<int, string>();
        [JsonIgnore]
        public byte[]? PaletteData { get; private set; } = null;

        [JsonIgnore]
        public Func<int, int, int, int> Indexer => (x, y, z) => (y & 31) << 10 | (z & 31) << 5 | x & 31;
        [JsonIgnore]
        public string DefaultVoxel => "Empty";

        [JsonIgnore]
        private int _minBits = -1;

        [JsonIgnore]
        public int PaletteHeight => 32;
        [JsonIgnore]
        public int PaletteWidth => 32;
        [JsonIgnore]
        public int PaletteDepth => 32;

        [JsonIgnore]
        private int _blockCount => PaletteWidth * PaletteHeight * PaletteDepth;

        private void populatePalette()
        {
            if (Data == null)
                return;

            char[] chars = Data.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
            var bin = new Binary(Data, EndianMode.Big);
            //int migrationVer = bin.Read<int>().SwapEndian();
            bin.Seek += 4;
            var paletteType = (HytalePaletteType)bin.Read<byte>();

            if (paletteType == HytalePaletteType.Empty)
                return;

            int indexCount = _blockCount;
            _minBits = 8;
            switch (paletteType)
            {
                case HytalePaletteType.Half:
                    indexCount /= 2;
                    _minBits = 4;
                    break;
                case HytalePaletteType.Short:
                    indexCount *= 2;
                    _minBits = 16;
                    break;
            }

            PaletteList = new Dictionary<int, string>();

            ushort paletteCount = bin.Read<ushort>();
            var palettes = new Dictionary<int, string>();
            for (int i = 0; i < paletteCount; i++)
            {
                ushort key = paletteType == HytalePaletteType.Short ? bin.Read<ushort>() : bin.Read<byte>();
                //string name = bin.ReadGivenLength<string, ushort>((x) => x.SwapEndian());
                ushort nameLen = bin.Read<ushort>();
                string name = bin.ReadLength<string>(nameLen);
                ushort count = bin.Read<ushort>();
                palettes[key] = name;
            }
            PaletteList = palettes;

            if (paletteCount != PaletteList.Count)
            {
                int z = 0;
            }
            for (int i = 0; i < PaletteList.Keys.Max(); i++)
            {
                if (!PaletteList.ContainsKey(i))
                {
                    int z = 0;
                }
            }

            bin.Cut();
            bin = new Binary(bin.Subset(indexCount));
            PaletteData = bin.Bytes.ToArray();
            //chars = PaletteData.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
            //var list = Decompress(_blockCount);
            Build(Decompress(_blockCount), false);
        }

        public string[] Decompress(int payloadSize = -1)
        {
            if (PaletteData == null)
                return [];
            if (PaletteList.Count < 2)
            {
                if (payloadSize == -1 || PaletteList.Count == 0)
                    return [];
                return PaletteList[0].Stretch(payloadSize);
            }

            int paletteCount = PaletteList.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < _minBits)
                bitCount = _minBits;

            if (payloadSize == -1)
                payloadSize = PaletteData.Length / bitCount;

            var list = new List<string>();
            for (int i = 0; i < payloadSize; i++)
            {
                var intBits = new bool[bitCount];
                for (int j = i * bitCount; j < (i + 1) * bitCount; j++)
                {
                    //int byteIndex = (int)Math.Floor(j / 8d);
                    //int bitIndex = j - (byteIndex * 8);
                    int byteIndex = j / 8;
                    int bitIndex = j % 8;
                    byte b = PaletteData[byteIndex];
                    int k = j - (i * bitCount);
                    intBits[k] = (b & (1 << bitIndex)) != 0;
                }
                int index = intBits.ToInt(true).SwapEndian();
                string key = "";
                if (PaletteList.ContainsKey(index))
                    key = PaletteList[index];
                else
                {
                    // Stop a crash, fill errored block.
                    //for (int k = 0; k < paletteCount; k++)
                    //{
                    //    if (PaletteList.ContainsKey(k))
                    //    {
                    //        key = PaletteList[k];
                    //        break;
                    //    }
                    //}
                    if (string.IsNullOrEmpty(key))
                    {
                        key = "Rock_Crystal_Green_Block";
                        PaletteList.Add(index, key);
                    }
                        
                }
                list.Add(key);
            }

            var dist = list.Distinct();
            foreach (var p in PaletteList.Values)
            {
                if (!dist.Contains(p))
                {
                    //Huh???
                    int k = 0;
                }
            }

            return list.ToArray();
        }

        public string Get(int x, int y, int z)
        {
            int paletteCount = PaletteList.Count();
            if (paletteCount == 1)
                return PaletteList[0];

            if (PaletteData == null)
                return DefaultVoxel;

            int index = Indexer(x, y, z);
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < _minBits)
                bitCount = _minBits;

            int payloadSize = PaletteData.Length / bitCount;

            var intBits = new bool[bitCount];
            for (int j = index * bitCount; j < (index + 1) * bitCount; j++)
            {
                int byteIndex = (int)Math.Floor(j / 8d);
                int bitIndex = j - (byteIndex * 8);
                byte b = PaletteData[byteIndex];
                int k = j - (index * bitCount);
                intBits[k] = (b & (1 << bitIndex)) != 0;
            }
            int paletteIndex = intBits.ToInt();
            return PaletteList[paletteIndex];
        }

        public void Set(string value, int x, int y, int z)
        {
            int index = Indexer(x, y, z);

            if (!PaletteList.ContainsValue(value))
            {
                var list = PaletteData == null || PaletteData.Length == 0 ? DefaultVoxel.Stretch(_blockCount) : Decompress();
                list[index] = value;
                Build(list);
            }
            else
            {
                int paletteCount = PaletteList.Count();
                int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
                if (bitCount < _minBits)
                    bitCount = _minBits;

                int paletteIndex = PaletteList.KeyOf(value);
                bool[] bits = paletteIndex.FromInt(bitCount).Reverse().ToArray();

                int offset = index * bitCount;

                for (int i = 0; i < bitCount; i++)
                {
                    int j = offset + i;
                    int byteIndex = j / 8;
                    int bitIndex = j % 8;
                    byte mask = (byte)(1 << j);
                    if (bits[i])
                        PaletteData[byteIndex] |= mask;
                    else
                        PaletteData[byteIndex] &= mask;
                }
            }
        }

        public void Build(string[] list, bool repopulate = true)
        {
            string[] palette = list.Distinct().ToArray();
            short paletteCount = (short)palette.Length;
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
            
            var bin = new Binary(EndianMode.Big);
            // migration ver
            bin.Write(10);
            bin.Write((byte)paletteType);
            bin.Write(paletteCount);

            for (short i = 0; i < paletteCount; i++)
            {
                if (paletteType == HytalePaletteType.Short)
                    bin.Write(i);
                else
                    bin.Write(BitConverter.GetBytes(i)[1]);

                string name = palette[i];
                bin.Write((ushort)name.Length);
                bin.Write(name);
                // Come back to this
                bin.Write((short)0);
            }

            var bits = new List<bool>();
            foreach (string item in list)
            {
                int i = palette.FindNextIndex(x => x == item);
                bool[] newBits = i.FromInt(bitCount).Reverse().ToArray();
                for (int j = 0; j < bitCount; j++)
                {
                    bits.Add(newBits[j]);
                    if (bits.Count == 8)
                    {
                        byte b = 0;
                        for (int k = 0; k < bitCount; k++)
                        {
                            byte mask = (byte)(1 << k);
                            if (bits[k])
                                b |= mask;
                            else
                                b &= mask;
                        }
                        bin.Write(b);
                        bits.Clear();
                    }
                }
            }

            if (repopulate)
                Data = bin.Bytes.ToArray();
            else
                base.Data = bin.Bytes.ToArray();
        }
    }
}
