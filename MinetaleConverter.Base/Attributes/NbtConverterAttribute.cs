using MinetaleConverter.Base.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Attributes
{
    public class NbtConverterAttribute : Attribute
    {
        public Func<string, object?> Converter { get; }
        public NbtConverterAttribute(Type converterType)
        {
            if (!converterType.GetInterfaces().Any(x => x == typeof(INbtConverter)))
                throw new ArgumentException("Converter attribute class must be INbtConverter");

            var obj = (INbtConverter?)Activator.CreateInstance(converterType);

            if (obj != null)
                Converter = (x) => obj.Convert(x);
        }
    }
}
