using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Compression.Palette
{
    public class Palette<T>
    {
        public long[] Data { get; set; } = [];
        public List<T> Palettes { get; set; } = new List<T>();

        public T? GetPalette(int index, int minBit = 1) =>
            PaletteHelper.GetAtIndex(Palettes, Data, index, minBit);
    }
}
