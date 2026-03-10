using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_HotbarManager
    {
        public List<string?> SavedHotbars { get; set; } = new List<string?>()
        {
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        };
        public int CurrentHotbar { get; set; } = 0;
    }
}
