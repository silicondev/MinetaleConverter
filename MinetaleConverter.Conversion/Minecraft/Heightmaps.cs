using MinetaleConverter.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Heightmaps
    {
        public long[] MOTION_BLOCKING { get; internal set; }
        public long[] MOTION_BLOCKING_NO_LEAVES { get; internal set; }
        public long[] OCEAN_FLOOR { get; internal set; }
        public long[] OCEAN_FLOOR_WG { get; internal set; }
        public long[] WORLD_SURFACE { get; internal set; }
        public long[] WORLD_SURFACE_WG { get; internal set; }
    }
}
