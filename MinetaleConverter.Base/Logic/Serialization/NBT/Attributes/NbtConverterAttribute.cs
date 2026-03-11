using MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces;

namespace MinetaleConverter.Base.Logic.Serialization.NBT.Attributes
{
    public class NbtConverterAttribute : Attribute
    {
        public Func<string, object?> Converter { get; } = (x) => x;
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
