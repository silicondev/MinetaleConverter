using MinetaleConverter.Base.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_BlockComponentChunk
    {
        //public object? BlockComponents
        //{
        //    set => setBlockComponents(value);
        //}
        //[JsonIgnore]
        //public Dictionary<string, hy_BlockComponentItem> BlockComponentsDict { get; set; } = new Dictionary<string, hy_BlockComponentItem>();
        //private void setBlockComponents(object? obj)
        //{
        //    BlockComponentsDict = new Dictionary<string, hy_BlockComponentItem>();
        //    if (obj == null)
        //        return;

        //    var jObj = (JObject)obj;
        //    foreach (var child in jObj)
        //    {
        //        var val = child.Value?.ToObject<hy_BlockComponentItem>();
        //        if (val == null)
        //            continue;
        //        BlockComponentsDict.Add(child.Key, val);
        //    }
        //}

        [JsonConverter(typeof(BsonDocumentToDictionary<int, hy_BlockComponentItem>))]
        public Dictionary<int, hy_BlockComponentItem> BlockComponents { get; set; }
    }
}
