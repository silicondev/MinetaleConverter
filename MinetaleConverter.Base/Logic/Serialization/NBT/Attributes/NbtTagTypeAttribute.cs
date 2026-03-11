using SharpNBT;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Attributes
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
