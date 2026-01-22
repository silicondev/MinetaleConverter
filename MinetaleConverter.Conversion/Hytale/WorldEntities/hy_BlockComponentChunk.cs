using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_BlockComponentChunk : IBsonImporter
    {
        public Dictionary<int, hy_BlockComponent> Components { get; internal set; } = new Dictionary<int, hy_BlockComponent>();

        public void ImportBson(BsonFile file)
        {
            var blockComponents = file.Get<BsonFile>("BlockComponents");
            foreach (var kvp in blockComponents.Data)
            {
                (_, object data) = kvp.Value;
                var blockCompBson = (BsonFile)data;
                var comp = new hy_BlockComponent();
                comp.ImportBson(blockCompBson.Get<BsonFile>("Components"));
                Components.Add(int.Parse(kvp.Key), comp);
            }
        }
    }
}
