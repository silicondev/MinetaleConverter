using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_ChunkColumn : IBsonImporter
    {
        public hy_Section[] Sections { get; internal set; }

        public void ImportBson(BsonFile file)
        {
            var files = file.Get<object[]>("Sections");
            var list = new List<hy_Section>();
            foreach (var f in files.Select(x => (BsonFile)((((BsonType type, object fileObj))x).fileObj)))
            {
                var section = new hy_Section();
                section.ImportBson(f.Get<BsonFile>("Components"));
                list.Add(section);
            }
            Sections = list.ToArray();
        }
    }
}
