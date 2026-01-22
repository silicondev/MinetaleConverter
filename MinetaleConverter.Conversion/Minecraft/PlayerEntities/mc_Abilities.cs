using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft.PlayerEntities
{
    public class mc_Abilities
    {
        [NbtProperty("flying")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Flying { get; internal set; }
        [NbtProperty("flySpeed")]
        public float FlySpeed { get; internal set; } = 0.05f;
        [NbtProperty("instabuild")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Instabuild { get; internal set; }
        [NbtProperty("invulnerable")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Invulnerable { get; internal set; }
        [NbtProperty("mayBuild")]
        [NbtConverter(typeof(ByteToBool))]
        public bool MayBuild { get; internal set; }
        [NbtProperty("mayFly")]
        [NbtConverter(typeof(ByteToBool))]
        public bool MayFly { get; internal set; }
        [NbtProperty("walkSpeed")]
        public float WalkSpeed { get; internal set; } = 0.1f;
    }
}
