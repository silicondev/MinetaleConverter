using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Components : IBsonImporter
    {
        public hy_BlockComponentChunk BlockComponentChunk { get; internal set; } = new hy_BlockComponentChunk();
        public hy_Binary EnvironmentChunk { get; internal set; } = new hy_Binary();
        public hy_ChunkColumn ChunkColumn { get; internal set; } = new hy_ChunkColumn();
        public hy_Binary BlockHealthChunk { get; internal set; } = new hy_Binary();
        public hy_Binary BlockChunk { get; set; } = new hy_Binary();

        public void ImportBson(BsonFile file)
        {
            BlockComponentChunk.ImportBson(file.Get<BsonFile>("BlockComponentChunk"));
            EnvironmentChunk.ImportBson(file.Get<BsonFile>("EnvironmentChunk"));
            ChunkColumn.ImportBson(file.Get<BsonFile>("ChunkColumn"));
            BlockHealthChunk.ImportBson(file.Get<BsonFile>("BlockHealthChunk"));
            BlockChunk.ImportBson(file.Get<BsonFile>("BlockChunk"));
        }
    }
}
