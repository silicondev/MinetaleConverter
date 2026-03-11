using SharpNBT;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Attributes
{
    public class NbtListTagTypeAttribute : Attribute
    {
        public TagType Type { get; }
        public NbtListTagTypeAttribute(TagType type)
        {
            Type = type;
        }
    }
}
