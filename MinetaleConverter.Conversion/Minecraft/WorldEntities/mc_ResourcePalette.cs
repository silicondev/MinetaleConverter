using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using MinetaleConverter.Compression.Palette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft.WorldEntities
{
    public class mc_ResourcePalette : Palette<mc_Resource>
    {
        [NbtProperty("data")]
        public new long[] Data
        {
            get => base.Data;
            set => base.Data = value;
        }
        [NbtProperty("palette")]
        [NbtConverter(typeof(PrimitivePaletteConverter))]
        public new List<mc_Resource> Palettes
        {
            get => base.Palettes;
            set => base.Palettes = value;
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
