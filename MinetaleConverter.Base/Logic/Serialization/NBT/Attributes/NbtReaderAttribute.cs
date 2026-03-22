using MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Attributes
{
    public class NbtReaderAttribute : Attribute
    {
        public Func<JObject, object?> Converter { get; } = (x) => x;
        public NbtReaderAttribute(Type converterType)
        {
            if (!converterType.GetInterfaces().Any(x => x == typeof(INbtConvertReader)))
                throw new ArgumentException("Converter attribute class must be INbtConverter");

            var obj = (INbtConvertReader?)Activator.CreateInstance(converterType);

            if (obj != null)
                Converter = (x) => obj.Convert(x);
        }
    }
}
