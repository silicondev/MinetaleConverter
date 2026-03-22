using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces
{
    public interface INbtConvertReader
    {
        public object? Convert(JObject reader);
    }
}
