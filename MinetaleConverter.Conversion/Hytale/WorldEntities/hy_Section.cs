using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Section : IBsonImporter
    {
        public hy_Binary BlockPhysics { get; internal set; } = new hy_Binary();
        public hy_Binary Fluid { get; internal set; } = new hy_Binary();
        public hy_Binary Block { get; internal set; } = new hy_Binary();

        public void ImportBson(BsonFile file)
        {
            if (file.Data.ContainsKey("BlockPhysics"))
                BlockPhysics.ImportBson(file.Get<BsonFile>("BlockPhysics"));
            Fluid.ImportBson(file.Get<BsonFile>("Fluid"));
            Block.ImportBson(file.Get<BsonFile>("Block"));
        }
    }
}
