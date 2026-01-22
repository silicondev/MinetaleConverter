using MinetaleConverter.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion
{
    public class Position
    {
        public Position()
        {
            
        }

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Position(double x, double y, double z) : this(x, y)
        {
            Z = z;
        }

        [NbtProperty("x")]
        public double X { get; internal set; } = 0d;
        [NbtProperty("y")]
        public double Y { get; internal set; } = 0d;
        [NbtProperty("z")]
        public double Z { get; internal set; } = 0d;

        public override string ToString() => $"{X}, {Y}, {Z}";
    }
}
