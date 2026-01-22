using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using MinetaleConverter.Conversion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Chunk : IBsonImporter, IChunk
    {
        public BsonFile File { get; }
        public hy_Components Components { get; internal set; } = new hy_Components();
        public hy_Chunk(BsonFile file)
        {
            File = file;
            ImportBson(file);
        }

        public void ImportBson(BsonFile file)
        {
            Components.ImportBson(file.Get<BsonFile>("Components"));
        }

        public string GetBlock(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public string GetBiome(int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}
