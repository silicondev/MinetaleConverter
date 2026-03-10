using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base
{
    public static class Extensions
    {
        public static T[] ToArray<T>(this IEnumerable<char> str, Func<string, T> convert) =>
            new string(str.ToArray()).Replace(" ", "").Split(",").Select(x => convert(x)).ToArray();

        public static T? GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute =>
            (T?)Attribute.GetCustomAttribute(memberInfo, typeof(T));

        public static int SwapEndian(this IEnumerable<byte> arr)
        {
            var list = new List<byte>(arr);
            if (arr.Count() < 4)
                list.Insert(0, 0);
            return BitConverter.ToInt32(list.ToArray()).SwapEndian();
        }

        public static short SwapEndian(this short input)
        {
            unchecked
            {
                return (short)(((ushort)input).SwapEndian());
            }
        }

        public static int SwapEndian(this int input)
        {
            unchecked
            {
                unsafe
                {
                    int j = input;
                    uint i = *(uint*)&j;
                    i = i.SwapEndian();
                    j = *(int*)&i;
                    
                    return j;
                }
                //return (int)(((uint)input).SwapEndian());
                
            }
        }

        public static long SwapEndian(this long input)
        {
            unchecked
            {
                return (long)(((ulong)input).SwapEndian());
            }
        }

        public static ushort SwapEndian(this ushort input) =>
            (ushort)(((input & 0x00ff) << 8) +
                     ((input & 0xff00) >> 8));

        public static uint SwapEndian(this uint input) =>
            ((input & 0x000000ff) << 24) +
            ((input & 0x0000ff00) << 8) +
            ((input & 0x00ff0000) >> 8) +
            ((input & 0xff000000) >> 24);

        public static ulong SwapEndian(this ulong input) =>
            ((input & 0x00000000000000ff) << 56) +
            ((input & 0x000000000000ff00) << 40) +
            ((input & 0x0000000000ff0000) << 24) +
            ((input & 0x00000000ff000000) << 8) +
            ((input & 0x000000ff00000000) >> 8) +
            ((input & 0x0000ff0000000000) >> 24) +
            ((input & 0x00ff000000000000) >> 40) +
            ((input & 0xff00000000000000) >> 56);

        public static T[] Combine<T>(this IEnumerable<T[]> arr)
        {
            var list = new List<T>();
            foreach (var item in arr)
                list.AddRange(item);
            return list.ToArray();
        }

        public static T[] Combine<T>(this IEnumerable<T>[] arr)
        {
            var list = new List<T>();
            foreach (var item in arr)
                list.AddRange(item);
            return list.ToArray();
        }

        public static T[] Combine<T>(this IEnumerable<IEnumerable<T>> arr) => arr.Select(x => x.ToArray()).Combine();

        //public static T[] Combine<T>(this IEnumerable<List<T>> arr) => arr.Select(x => x.ToArray()).Combine();

        public static TValue[] Combine<TKey, TValue>(this Dictionary<TKey, TValue[]>.ValueCollection arr) where TKey : notnull =>
            arr.ToList().Combine();

        public static TValue[] Combine<TKey, TValue>(this Dictionary<TKey, IEnumerable<TValue>>.ValueCollection arr) where TKey : notnull =>
            arr.ToList().Combine();

        public static TValue[] Combine<TKey, TValue>(this Dictionary<TKey, List<TValue>>.ValueCollection arr) where TKey : notnull =>
            arr.ToList().Combine();

        public static (byte, byte) GetNibbles(this byte b)
        {
            byte nibble1 = (byte) (b & 0x0F);
            byte nibble2 = (byte)((b & 0xF0) >> 4);
            return (nibble1, nibble2);
        }

        public static T[] GetTupleArray<T>(this (T, T) tuple) => new T[] { tuple.Item1, tuple.Item2 };

        public static bool[] GetBits(this byte b)
        {
            var arr = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                arr[i] = (b & (1 << i)) != 0;
            }
            return arr;
        }

        public static int ToInt(this bool[] arr, bool bigEndian = false)
        {
            bool[] bits = new bool[32];
            if (bigEndian)
                Array.Copy(arr, 0, bits, 32 - arr.Length, arr.Length);
            else
                Array.Copy(arr, 0, bits, 0, arr.Length);

            byte[] bytes = new byte[4];
            for (int i = 0; i < 32; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = bigEndian ? 7 - (i % 8) : i % 8;
                var mask = (byte)(1 << bitIndex);
                if (bits[i])
                    bytes[byteIndex] |= mask;
                else
                    bytes[byteIndex] &= mask;
            }
            int result = BitConverter.ToInt32(bytes);
            return result;
        }

        public static bool[] FromInt(this int num, int len, bool bigEndian = false)
        {
            var arr = new bool[len];
            if (len > 32)
                len = 32;
            byte[] bytes = BitConverter.GetBytes(num);
            for (int i = 0; i < len; i++)
            {
                int j = bigEndian ? 32 - len + i : i;
                int byteIndex = j / 8;
                int bitIndex = j % 8;
                arr[i] = (bytes[byteIndex] & (1 << bitIndex)) != 0;
            }
            return arr;
        }

        public static Dictionary<int, T> ToNumberedDictionary<T>(this IEnumerable<T> list)
        {
            var dict = new Dictionary<int, T>();
            for (int i = 0; i < list.Count(); i++)
                dict.Add(i, list.ElementAt(i));
            return dict;
        }

        public static int FindNextIndex<T>(this IEnumerable<T> arr, Func<T, bool> predicate, int startIndex = 0)
        {
            for (int i = startIndex; i < arr.Count(); i++)
            {
                if (predicate(arr.ElementAt(i)))
                    return i;
            }
            return -1;
        }

        public static string ToCharString(this int num, char c)
        {
            string str = "";
            for (int i = 0; i < num; i++)
                str += c;
            return str;
        }

        public static string ToArrayString<T>(this IEnumerable<T> arr) => string.Join(", ", arr);

        //public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> list) where TKey : notnull
        //{
        //    var dict = new Dictionary<TKey, TValue>();
        //    foreach (var item in list)
        //        dict.Add(item.Key, item.Value);
        //    return dict;
        //}

        public static T[] Stretch<T>(this T obj, int count)
        {
            var arr = new T[count];
            for (int i = 0; i < count; i++)
                arr[i] = obj;
            return arr;
        }

        public static TKey? KeyOf<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue val) where TKey : notnull
        {
            if (!dict.ContainsValue(val))
                return default;

            foreach (var kvp in dict)
            {
                if (kvp.Value.Equals(val))
                    return kvp.Key;
            }
            return default;
        }
    }
}
