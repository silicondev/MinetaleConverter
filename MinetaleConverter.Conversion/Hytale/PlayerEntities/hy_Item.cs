using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_Item
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public double Durability { get; set; } = 0.0d;
        public double MaxDurability { get; set; } = 0.0d;
        public bool OverrideDroppedItemAnimation { get; set; } = false;
    }
}
