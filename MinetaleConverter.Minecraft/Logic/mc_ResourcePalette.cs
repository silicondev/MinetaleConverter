using MinetaleConverter.Base.Logic.Compression.Palette;
using MinetaleConverter.Base.Logic.Extensions;
using MinetaleConverter.Base.Logic.Serialization.NBT.Attributes;
using MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces;
using MinetaleConverter.Minecraft.Models;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Minecraft.Logic
{
    public class mc_ResourcePalette : DataPalette<uint, long, mc_Resource>
    {
        [NbtProperty("data")]
        public override long[] Data
        {
            get => _data;
            set => _data = value;
        }

        [NbtProperty("palette")]
        [NbtConverter(typeof(PrimitivePaletteConverter))]
        public new List<mc_Resource> Palettes
        {
            get
            {
                uint max = PaletteList.Keys.Max();
                mc_Resource[] result = DefaultPalette.Stretch((int)max).ToArray();
                for (uint i = 0; i < max; i++)
                {
                    result[i] = PaletteList[i];
                }
                return result.ToList();
            }
            set
            {
                PaletteList.Clear();
                for (uint i = 0; i < value.Count; i++)
                {
                    PaletteList.Add(i, value.ElementAt((int)i));
                }
            }
        }

        public override mc_Resource DefaultPalette => new mc_Resource()
        {
            Name = "minecraft:air"
        };

        public override mc_Resource ErrorPalette => new mc_Resource()
        {
            Name = "minecraft:bedrock"
        };

        public override Func<int, int, int, int> Indexer => IndexerFunc;

        public override int Length { get; protected set; }

        public static Func<int, int, int, int> IndexerFunc => (x, y, z) => y * 16 * 16 + z * 16 + x;

        private int _minBits = 1;
        private int _bitCount => Math.Min(_minBits, (int)Math.Ceiling(Math.Log2(PaletteList.Count)));
        private int _amountInLong => 64 / _bitCount;

        public mc_ResourcePalette(List<mc_Resource> palettes, long[] data, int minBits = 1)
        {
            Palettes = palettes;
            Data = data;
            _minBits = minBits;
        }

        public mc_ResourcePalette()
        {

        }

        public override mc_Resource[] Decompress(long[] arr)
        {
            throw new NotImplementedException();
        }

        public override mc_Resource? GetAtIndex(int index)
        {
            if (Palettes.Count == 0) 
                return null;
            if (Palettes.Count == 1)
                return Palettes[0];

            int longIndex = index / _amountInLong;
            byte[] bytes = new byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(bytes, Data[longIndex]);

            int paletteIndex = 0;
            for (int i = 0; i < _bitCount; i++)
            {
                int indexInLong = ((index + i) % _amountInLong) * _bitCount;
                int byteIndex = indexInLong / 8;
                int bitIndex = indexInLong % 8;

                byte b = bytes[byteIndex];
                bool v = (b & (1 << (7 - bitIndex))) != 0;
                if (v)
                    paletteIndex |= (1 << (32 - i));
                else
                    paletteIndex &= ~(1 << (32 - i));
            }

            return Palettes[paletteIndex];
        }

        public override void SetAtIndex(int index, mc_Resource value)
        {
            throw new NotImplementedException();
        }

        protected override void populate(bool rebuild = true)
        {
            throw new NotImplementedException();
        }

        protected override void Reconstruct(mc_Resource[] list)
        {
            throw new NotImplementedException();
        }
    }

    internal class PrimitivePaletteConverter : INbtConverter
    {
        public object? Convert(string value)
        {
            var palette = new mc_Resource();
            palette.Name = value.Replace("\"", "");
            return palette;
        }
    }
}
