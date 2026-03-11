using MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Converters
{
    public class SignedByteToInt : INbtConverter
    {
        public object? Convert(string value)
        {
            var b = (byte?)NbtHelper.ConvertMapper[TagType.Byte](value);
            if (b == null)
                return null;
            // 0 ... 127 +   128 ... 255 -
            int? output = b > 127 ? (256 - b) / -1 : b;
            return output;
        }
    }
}
