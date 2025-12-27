using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Version
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string Series { get; internal set; }
        [NbtConverter(typeof(ByteToBool))]
        public bool Snapshot { get; internal set; }
    }
}
