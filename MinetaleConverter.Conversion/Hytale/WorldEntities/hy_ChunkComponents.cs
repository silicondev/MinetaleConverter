using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_ChunkComponents
    {
        public hy_ChunkColumn ChunkColumn { get; set; } = new hy_ChunkColumn();
        public hy_BlockComponentChunk BlockComponentChunk { get; set; } = new hy_BlockComponentChunk();
        public hy_EnvironmentChunk EnvironmentChunk { get; set; } = new hy_EnvironmentChunk();
        public hy_Binary BlockChunk { get; set; } = new hy_Binary();
        public hy_Binary BlockHealthChunk { get; set; } = new hy_Binary();
        public hy_WorldChunk WorldChunk { get; set; } = new hy_WorldChunk();
        public hy_EntityChunk EntityChunk { get; set; } = new hy_EntityChunk();

    }
}
