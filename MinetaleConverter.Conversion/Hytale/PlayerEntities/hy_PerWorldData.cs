using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_PerWorldData
    {
        public hy_CompletePositon LastPosition { get; set; } = new hy_CompletePositon();
        public hy_LastMovementStates LastMovementStates { get; set; } = new hy_LastMovementStates();
        public bool FirstSpawn { get; set; } = true;
        public List<Position> DeathPositions { get; set; } = new List<Position>();
    }
}
