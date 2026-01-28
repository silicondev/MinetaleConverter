using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_EntityChunk
    {
        public IList<hy_Entity> Entities { get; set; } = new List<hy_Entity>();
    }
}
