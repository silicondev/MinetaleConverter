
using MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces;
using SharpNBT;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Converters
{
    public class ByteToBool : INbtConverter
    {
        public object? Convert(string value) => ((byte?)NbtHelper.ConvertMapper[TagType.Byte](value) ?? 0) == 1;
    }
}
