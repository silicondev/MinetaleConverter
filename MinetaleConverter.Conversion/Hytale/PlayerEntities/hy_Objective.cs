using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_Objective
    {
        public hy_ObjectiveHistory ObjectiveHistory { get; set; } = new hy_ObjectiveHistory();
        public hy_ObjectiveLineHistory ObjectiveLineHistory { get; set; } = new hy_ObjectiveLineHistory();
    }
}
