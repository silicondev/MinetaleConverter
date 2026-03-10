using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_DisplayNameDetails
    {
        public string RawText { get; set; }
        public bool? Bold { get; set; } = null;
        public bool? Italic { get; set; } = null;
        public bool? Monospace { get; set; } = null;
        public bool? Underline { get; set; } = null;
    }
}
