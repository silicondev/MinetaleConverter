using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Conversion.Minecraft.PlayerEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class mc_Player
    {
        [NbtProperty("abilities")]
        public mc_Abilities Abilities { get; internal set; }
        public int DataVersion { get; internal set; }
        [NbtProperty("current_explosion_impact_pos")]
        public Position CurrentExplosionImpactPos { get; internal set; } = new Position();
        public string Dimension { get; internal set; }
        [NbtProperty("entered_nether_pos")]
        public Position EnteredNetherPos { get; internal set; } = new Position();
    }
}
