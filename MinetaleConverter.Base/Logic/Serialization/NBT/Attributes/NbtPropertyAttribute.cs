using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Attributes
{
    public class NbtPropertyAttribute : Attribute
    {
        public string PropertyName { get; }
        public NbtPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
