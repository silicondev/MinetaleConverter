using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_PlayerComponents
    {
        public hy_Nameplate Nameplate { get; set; } = new hy_Nameplate();
        public hy_EffectController EffectController { get; set; } = new hy_EffectController();
        public hy_Objective ObjectiveHistory { get; set; } = new hy_Objective();
        public hy_DisplayName DisplayName { get; set; } = new hy_DisplayName();
        public hy_UIComponentList UIComponentList { get; set; } = new hy_UIComponentList();
        public hy_Transform Transform { get; set; } = new hy_Transform();
        public hy_BuilderTools BuilderTools { get; set; } = new hy_BuilderTools();
        public hy_Velocity Velocity { get; set; } = new hy_Velocity();
        public hy_PlayerData Player { get; set; } = new hy_PlayerData();
        // To be continued
    }
}
