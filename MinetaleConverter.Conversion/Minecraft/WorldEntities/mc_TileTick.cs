using MinetaleConverter.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft.WorldEntities
{
    public class mc_TileTick
    {
        [NbtProperty("i")]
        public string Id { get; internal set; }
        [NbtProperty("p")]
        public int P { get; internal set; }
        [NbtProperty("t")]
        public int T { get; internal set; }
        [NbtProperty("x")]
        public int X { get; internal set; }
        [NbtProperty("y")]
        public int Y { get; internal set; }
        [NbtProperty("z")]
        public int Z { get; internal set; }
    }
}
