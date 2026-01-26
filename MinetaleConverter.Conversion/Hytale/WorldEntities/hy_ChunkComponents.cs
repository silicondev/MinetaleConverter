using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_ChunkComponents
    {
        public hy_ChunkColumn ChunkColumn { get; set; }
        public hy_BlockComponentChunk BlockComponentChunk { get; set; }
        public hy_EnvironmentChunk EnvironmentChunk { get; set; }
        public hy_BlockChunk BlockChunk { get; set; }
    }
}
