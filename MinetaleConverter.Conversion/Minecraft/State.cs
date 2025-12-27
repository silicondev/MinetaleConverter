using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class State
    {
        [NbtProperty("data")]
        public long[] Data { get; internal set; }
        [NbtProperty("palette")]
        [NbtConverter(typeof(PrimitivePaletteConverter))]
        public List<Palette> Palettes { get; internal set; } = new List<Palette>();
    }

    internal class PrimitivePaletteConverter : INbtConverter
    {
        public object? Convert(string value)
        {
            var palette = new Palette();
            palette.Name = value;
            return palette;
        }
    }
}
