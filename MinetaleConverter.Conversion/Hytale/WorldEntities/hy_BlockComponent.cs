using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_BlockComponent : IBsonImporter
    {
        public hy_FarmingBlock FarmingBlock { get; internal set; } = new hy_FarmingBlock();

        public void ImportBson(BsonFile file)
        {
            FarmingBlock.SpreadRate = file.Get<BsonFile>("FarmingBlock").Get<double>("SpreadRate");
        }
    }
}
