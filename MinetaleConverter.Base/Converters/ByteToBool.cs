using MinetaleConverter.Base.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Converters
{
    public class ByteToBool : INbtConverter
    {
        public object? Convert(string value) => ((byte?)NbtHelper.ConvertMapper[TagType.Byte](value) ?? 0) == 1;
    }
}
