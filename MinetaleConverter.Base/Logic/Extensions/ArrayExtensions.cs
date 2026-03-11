using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] ToArray<T>(this IEnumerable<char> str, Func<string, T> convert) =>
            new string(str.ToArray()).Replace(" ", "").Split(",").Select(x => convert(x)).ToArray();

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

        public static T[] Combine<T>(this IEnumerable<List<T>> arr) => arr.Select(x => x.ToArray()).Combine();

        public static TValue[] Combine<TKey, TValue>(this Dictionary<TKey, TValue[]>.ValueCollection arr) where TKey : notnull =>
            arr.ToList().Combine();

        public static TValue[] Combine<TKey, TValue>(this Dictionary<TKey, IEnumerable<TValue>>.ValueCollection arr) where TKey : notnull =>
            arr.ToList().Combine();

        public static TValue[] Combine<TKey, TValue>(this Dictionary<TKey, List<TValue>>.ValueCollection arr) where TKey : notnull =>
            arr.ToList().Combine();

        public static T[] GetTupleArray<T>(this (T, T) tuple) => new T[] { tuple.Item1, tuple.Item2 };

        public static Dictionary<int, T> ToNumberedDictionary<T>(this IEnumerable<T> list)
        {
            var dict = new Dictionary<int, T>();
            for (int i = 0; i < list.Count(); i++)
                dict.Add(i, list.ElementAt(i));
            return dict;
        }

        public static string ToArrayString<T>(this IEnumerable<T> arr) => string.Join(", ", arr);

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

        public static int FindNextIndex<T>(this IEnumerable<T> arr, Func<T, bool> predicate, int startIndex = 0)
        {
            for (int i = startIndex; i < arr.Count(); i++)
            {
                if (predicate(arr.ElementAt(i)))
                    return i;
            }
            return -1;
        }
    }
}
