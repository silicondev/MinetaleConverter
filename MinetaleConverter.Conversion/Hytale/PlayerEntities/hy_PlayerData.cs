using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_PlayerData
    {
        public int Version { get; set; } = 5;
        public hy_Inventory Inventory { get; set; } = new hy_Inventory();
        public hy_PlayerMetadata PlayerData { get; set; } = new hy_PlayerMetadata();
        public hy_HotbarManager HotbarManager { get; set; } = new hy_HotbarManager();
        public string GameMode { get; set; } = "Adventure";
    }
}
