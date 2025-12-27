using MinetaleConverter.Base.Attributes;
using SharpNBT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base
{
    public static class NbtHelper
    {
        public static void FillFromTag(object obj, CompoundTag tag)
        {
            var properties = obj.GetType().GetProperties();

            var propDict = new Dictionary<string, PropertyInfo>();

            foreach (var property in properties)
            {
                var nbt = property.GetAttribute<NbtPropertyAttribute>();
                string key = nbt?.PropertyName ?? property.Name;
                propDict.Add(key, property);
            }

            var elements = tag.Keys.Select(x =>
            {
                var type = tag[x].Type;

                switch (type)
                {
                    case TagType.Compound:
                        return tag.Find<CompoundTag>(x);
                    default:
                        return tag[x];
                }
            });

            foreach (var element in elements)
            {
                if (element == null ||
                    element.Name == null)
                    continue;

                if (!propDict.ContainsKey(element.Name))
                    continue;

                var property = propDict[element.Name];

                var tagTypeAttr = property.GetAttribute<NbtTagTypeAttribute>();
                var tagType = tagTypeAttr?.Type ?? element.Type;

                switch (tagType)
                {
                    case TagType.Compound:
                        var compInstance = Activator.CreateInstance(property.PropertyType);
                        FillFromTag(compInstance, (CompoundTag)element);
                        property.SetValue(obj, compInstance);
                        break;
                    case TagType.List:
                        var listTag = (ListTag)element;
                        Type elementType = property.PropertyType.GetGenericArguments().Single();
                        Type listType = typeof(List<>).MakeGenericType(new[] { elementType });
                        IList list = (IList)Activator.CreateInstance(listType);

                        if (listTag.ChildType == TagType.Compound)
                        {
                            foreach (var el in listTag)
                            {
                                var elementInstance = Activator.CreateInstance(elementType);
                                FillFromTag(elementInstance, (CompoundTag)el);
                                list.Add(elementInstance);
                            }
                        }
                        else
                        {
                            var listConverter = property.GetAttribute<NbtConverterAttribute>();
                            foreach (var el in listTag)
                            {
                                string listValue = element.Stringify(false);
                                if (listConverter != null)
                                    list.Add(listConverter.Converter(listValue));
                                else
                                    list.Add(_convertMapper[tagType](listValue));
                            }
                        }
                        property.SetValue(obj, list);
                        break;
                    default:
                        var converter = property.GetAttribute<NbtConverterAttribute>();
                        string value = element.Stringify(false);
                        if (converter != null)
                            property.SetValue(obj, converter.Converter(value));
                        else
                            property.SetValue(obj, _convertMapper[tagType](value));
                        break;
                }
            }
        }

        private static Dictionary<TagType, Func<string, object?>> _convertMapper = new Dictionary<TagType, Func<string, object?>>()
        {
            { TagType.String, (x) => x.Replace("\"", "") },
            { TagType.Int, (x) => int.Parse(x) },
            { TagType.Long, (x) => long.Parse(x.ToLower().Replace("l", "")) },
            { TagType.Float, (x) => float.Parse(x.ToLower().Replace("f", "")) },
            { TagType.Byte, (x) => byte.Parse(x.ToLower().Replace("b", "")) },
            { TagType.Double, (x) => double.Parse(x.ToLower().Replace("d", "")) },
            { TagType.Short, (x) => short.Parse(x) },
            { TagType.IntArray, (x) => x.Skip(3).SkipLast(1).ToArray((y) => int.Parse(y)) },
            { TagType.LongArray, (x) => x.Skip(3).SkipLast(1).ToArray((y) => long.Parse(y.ToLower().Replace("l", ""))) },
            { TagType.ByteArray, (x) => x.Skip(3).SkipLast(1).ToArray((y) => byte.Parse(y.ToLower().Replace("b", ""))) },
            { TagType.List, (x) => x }
        };

        // Not used but keeping just in case. I don't wanna write this again.
        private static Dictionary<TagType, Type> _typeMapper = new Dictionary<TagType, Type>()
        {
            { TagType.String, typeof(string) },
            { TagType.Int, typeof(int) },
            { TagType.Long, typeof(long) },
            { TagType.Float, typeof(float) },
            { TagType.Byte, typeof(byte) },
            { TagType.Double, typeof(double) },
            { TagType.Short, typeof(short) },
            { TagType.IntArray, typeof(int[]) },
            { TagType.LongArray, typeof(long[]) },
            { TagType.ByteArray, typeof(byte[]) }
        };
    }
}
