using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_Transform
    {
        public Position Position { get; set; } = new Position();
        public Rotation Rotation { get; set; } = new Rotation();
    }
}
