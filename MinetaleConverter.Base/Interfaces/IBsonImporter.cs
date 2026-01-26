using MinetaleConverter.Base.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Interfaces
{
    public interface IBsonImporter
    {
        void ImportBson(BsonFile file);
        void ImportBson(JObject file);
        //void ImportBson(JsonReader reader);
    }
}
