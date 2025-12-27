using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class Level
    {
        [NbtProperty("allowCommands")]
        [NbtConverter(typeof(ByteToBool))]
        public bool AllowCommands { get; internal set; }
        public double BorderCenterX { get; internal set; } = 0d;
        public double BorderCenterZ { get; internal set; } = 0d;
        public double BorderDamagePerBlock { get; internal set; } = 0.2d;
        public double BorderSize { get; internal set; } = 60000000d;
        public double BorderSafeZone { get; internal set; } = 5d;
        public double BorderSizeLerpTarget { get; internal set; } = 60000000d;
        public long BorderSizeLerpTime { get; internal set; } = 0;
        public double BorderWarningBlocks { get; internal set; } = 5d;
        public double BorderWarningTime { get; internal set; } = 15d;
        [NbtProperty("clearWeatherTime")]
        public int ClearWeatherTime { get; internal set; }
        public DataPacks DataPacks { get; internal set; } = new DataPacks();
        public int DataVersion { get; internal set; }
        public long DayTime { get; internal set; }
        public byte Difficulty { get; internal set; }
        [NbtConverter(typeof(ByteToBool))]
        public bool DifficultyLocked { get; internal set; }
        [NbtProperty("enabled_features")]
        public List<string> EnabledFeatures { get; internal set; } = new List<string>();
        public Dictionary<string, bool> GameRules { get; internal set; } = new Dictionary<string, bool>();
        public int GameType { get; internal set; }
        [NbtProperty("hardcore")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Hardcore { get; internal set; }
        [NbtProperty("initialized")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Initialized { get; internal set; }
        public long LastPlayed { get; internal set; }
        public string LevelName { get; internal set; }
        [NbtConverter(typeof(ByteToBool))]
        public bool MapFeatures { get; internal set; } = true;
        [NbtProperty("raining")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Raining { get; internal set; }
        [NbtProperty("rainTime")]
        public int RainTime { get; internal set; }
        public long RandomSeed { get; internal set; }
        public int SpawnX { get; internal set; }
        public int SpawnY { get; internal set; }
        public int SpawnZ { get; internal set; }
        [NbtProperty("thundering")]
        [NbtConverter(typeof(ByteToBool))]
        public bool Thundering { get; internal set; }
        [NbtProperty("thunderingTime")]
        public int ThunderingTime { get; internal set; }
        public long Time { get; internal set; }
        [NbtProperty("version")]
        public int VersionNumber { get; internal set; }
        public Version Version { get; internal set; } = new Version();
        public int[] WanderingTraderId { get; internal set; }
        public int WanderingTraderSpawnChance { get; internal set; }
        public int WanderingTraderSpawnDelay { get; internal set; }
        [NbtConverter(typeof(ByteToBool))]
        public bool WasModded { get; internal set; }
    }
}
