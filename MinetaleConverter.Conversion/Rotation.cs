using MinetaleConverter.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion
{
    public class Rotation
    {
        public Rotation()
        {

        }

        public Rotation(double pitch, double yaw, double roll)
        {
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
        }

        public double Pitch { get; internal set; } = 0d;
        public double Yaw { get; internal set; } = 0d;
        public double Roll { get; internal set; } = 0d;

        public override string ToString() => $"{Pitch}, {Yaw}, {Roll}";
    }
}
