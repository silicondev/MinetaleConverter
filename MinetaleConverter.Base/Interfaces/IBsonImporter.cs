using MinetaleConverter.Base.Bson;
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
    }
}
