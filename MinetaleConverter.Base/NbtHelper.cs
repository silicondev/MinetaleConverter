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

            foreach (var element in tag)
            {
                if (element == null ||
                    element.Name == null)
                    continue;

                if (!propDict.ContainsKey(element.Name))
                    continue;

                var property = propDict[element.Name];
                bool isStringProperty = property.PropertyType == typeof(string);

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
                        Type[] elementTypes = property.PropertyType.GetGenericArguments();
                        bool isDict = elementTypes.Length > 1;

                        ICollection list;
                        if (isDict)
                        {
                            Type listType = typeof(Dictionary<,>).MakeGenericType(new[] { elementTypes[0], elementTypes[1] });
                            list = (IDictionary)Activator.CreateInstance(listType);
                        }
                        else
                        {
                            Type elementType = elementTypes[0];
                            bool isString = elementType == typeof(string);
                            Type listType = typeof(List<>).MakeGenericType(new[] { elementType });
                            list = (IList)Activator.CreateInstance(listType);
                        }

                        if (listTag.ChildType == TagType.Compound)
                        {
                            if (isDict)
                            {
                                foreach (var el in listTag)
                                {
                                    var elementInstance = Activator.CreateInstance(elementTypes[1]);
                                    FillFromTag(elementInstance, (CompoundTag)el);
                                    ((IDictionary)list).Add(el.Name, elementInstance);
                                }
                            }
                            else
                            {
                                foreach (var el in listTag)
                                {
                                    var elementInstance = Activator.CreateInstance(elementTypes[0]);
                                    FillFromTag(elementInstance, (CompoundTag)el);
                                    ((IList)list).Add(elementInstance);
                                }
                            }
                        }
                        else
                        {
                            var listConverter = property.GetAttribute<NbtConverterAttribute>();
                            if (isDict)
                            {
                                foreach (var el in listTag)
                                {
                                    string listValue = el.Stringify(false);
                                    if (listConverter != null)
                                        ((IDictionary)list).Add(el.Name, listConverter.Converter(listValue));
                                    else
                                        ((IDictionary)list).Add(el.Name, ConvertMapper[el.Type](listValue));
                                }
                            }
                            else
                            {
                                foreach (var el in listTag)
                                {
                                    string listValue = el.Stringify(false);
                                    if (listConverter != null)
                                        ((IList)list).Add(listConverter.Converter(listValue));
                                    else
                                        ((IList)list).Add(ConvertMapper[el.Type](listValue));
                                }
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
                            property.SetValue(obj, ConvertMapper[tagType](value));
                        break;
                }
            }
        }

        public static Dictionary<TagType, Func<string, object?>> ConvertMapper = new Dictionary<TagType, Func<string, object?>>()
        {
            { TagType.String, (x) => x.Replace("\"", "") },
            { TagType.Int, (x) => int.Parse(x.Replace("\"", "")) },
            { TagType.Long, (x) => long.Parse(x.Replace("\"", "").ToLower().Replace("l", "")) },
            { TagType.Float, (x) => float.Parse(x.Replace("\"", "").ToLower().Replace("f", "")) },
            { TagType.Byte, (x) => byte.Parse(x.Replace("\"", "").ToLower().Replace("b", "")) },
            { TagType.Double, (x) => double.Parse(x.Replace("\"", "").ToLower().Replace("d", "")) },
            { TagType.Short, (x) => short.Parse(x.Replace("\"", "")) },
            { TagType.IntArray, (x) => x.Skip(3).SkipLast(1).ToArray((y) => int.Parse(y.Replace("\"", ""))) },
            { TagType.LongArray, (x) => x.Skip(3).SkipLast(1).ToArray((y) => long.Parse(y.Replace("\"", "").ToLower().Replace("l", ""))) },
            { TagType.ByteArray, (x) => x.Skip(3).SkipLast(1).ToArray((y) => byte.Parse(y.Replace("\"", "").ToLower().Replace("b", ""))) }
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
