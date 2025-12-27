using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Chunk
    {
        public int DataVersion { get; internal set; }
        public int xPos { get; internal set; }
        public int zPos { get; internal set; }
        public int yPos { get; internal set; }
        public string Status { get; internal set; }
        [NbtConverter(typeof(DateConverter))]
        public DateTime LastUpdate { get; internal set; }
        [NbtProperty("sections")]
        public List<Section> Sections { get; internal set; } = new List<Section>();
        public Heightmaps Heightmaps { get; internal set; } = new Heightmaps();
        internal byte[] data { get; set; }
    }

    internal class DateConverter : INbtConverter
    {
        public object? Convert(string value) =>
            new DateTime(long.Parse(value.ToLower().Replace("l", "")));
    }
}
