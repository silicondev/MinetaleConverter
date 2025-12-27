using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base
{
    public class NbtTagTypeAttribute : Attribute
    {
        public TagType Type { get; }
        public NbtTagTypeAttribute(TagType type)
        {
            Type = type;
        }
    }
}
