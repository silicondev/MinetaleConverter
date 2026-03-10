using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_PlayerMetadata
    {
        public int BlockIdVersion { get; set; } = 1;
        public string World { get; set; } = "default";
        public string[] KnownRecipes { get; set; } = [];
        public Dictionary<string, hy_PerWorldData> PerWorldData { get; set; } = new Dictionary<string, hy_PerWorldData>();
        public List<string> DiscoveredZones { get; set; } = new List<string>();
        public List<string> DiscoveredInstances { get; set; } = new List<string>();
        public hy_ReputationData ReputationData { get; set; } = new hy_ReputationData();
        public List<string> ActiveObjectiveUUIDs { get; set; } = new List<string>();
    }
}
