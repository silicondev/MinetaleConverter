using MinetaleConverter.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Compression.Palette
{
    public static class PaletteHelper
    {
        public static T? GetAtIndex<T>(IEnumerable<T> palette, long[] data, int index, int minBits = 1)
        {
            if (palette.Count() == 0)
                return default;
            else if (palette.Count() == 1)
                return palette.First();

            int paletteCount = palette.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < minBits)
                bitCount = minBits;
            var divCount = Math.Floor(64d / bitCount);

            int dataIndex = (int)Math.Floor(index / divCount);
            int longIndex = index - dataIndex * (int)divCount;

            var bits = BitConverter.GetBytes(data[dataIndex]).Select(x => x.GetBits()).Combine();
            int ind = longIndex * bitCount;
            var intBits = bits[ind..(ind + bitCount)];

            return palette.ElementAt(intBits.ToInt());
        }

        public static int[] GetIndices<T>(IEnumerable<T> palette, long[] data, Func<T, bool> predicate, int minBits = 1)
        {
            //if (palette.Count() == 0)
            //    return [];
            //else if (palette.Count() == 1)
            //    return [0];

            //int paletteCount = palette.Count();
            //int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            //if (bitCount < minBits)
            //    bitCount = minBits;
            //var divCount = Math.Floor(64d / bitCount);

            //var list = new List<int>();
            //int li = 0;
            //int d = 0;
            //for (int i = 0; i < divCount * data.Length; i++)
            //{
            //    if (d >= divCount)
            //    {
            //        d = 0;
            //        li++;
            //    }

            //    var bits = BitConverter.GetBytes(data[li]).Select(x => x.GetBits()).Combine();

            //    int ind = d * bitCount;
            //    var intBits = bits[ind..(ind + bitCount)];

            //    if (predicate(palette.ElementAt(intBits.ToInt())))
            //        list.Add(i);
            //    d++;
            //}

            //return list.ToArray();
            return Decompress(palette, data, minBits).ToNumberedDictionary().Where(x => predicate(x.Value)).Select(x => x.Key).ToArray();
        }

        public static List<T> Decompress<T>(IEnumerable<T> palette, long[] data, int minBits = 1)
        {
            var list = new List<T>();
            if (palette.Count() == 0)
                return list;

            int paletteCount = palette.Count();
            int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
            if (bitCount < minBits)
                bitCount = minBits;
            var divCount = Math.Floor(64d / bitCount);

            int li = 0;
            int d = 0;
            for (int i = 0; i < divCount * data.Length; i++)
            {
                if (d >= divCount)
                {
                    d = 0;
                    li++;
                }

                var bits = BitConverter.GetBytes(data[li]).Select(x => x.GetBits()).Combine();

                int ind = d * bitCount;
                var intBits = bits[ind..(ind + bitCount)];

                list.Add(palette.ElementAt(intBits.ToInt()));
                d++;
            }
            return list;
        }
    }
}
