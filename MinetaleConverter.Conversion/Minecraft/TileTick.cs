using MinetaleConverter.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class TileTick
    {
        [NbtProperty("i")]
        public string Id { get; internal set; }
        [NbtProperty("p")]
        public string P { get; internal set; }
        [NbtProperty("t")]
        public string T { get; internal set; }
        [NbtProperty("x")]
        public string X { get; internal set; }
        [NbtProperty("y")]
        public string Y { get; internal set; }
        [NbtProperty("z")]
        public string Z { get; internal set; }
    }
}
