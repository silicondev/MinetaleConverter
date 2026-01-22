using MinetaleConverter.Base.Bson;
using MinetaleConverter.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Binary : IBsonImporter
    {
        public int Version { get; internal set; } = -1;
        public byte[] Data { get; internal set; }

        public void ImportBson(BsonFile file)
        {
            if (file.Data.ContainsKey("Version"))
                Version = file.Get<int>("Version");
            Data = file.Get<byte[]>("Data");
        }
    }
}
