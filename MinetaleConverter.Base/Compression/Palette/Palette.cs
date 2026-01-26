using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Compression.Palette
{
    public abstract class Palette<T>
    {
        public List<T> Palettes
        {
            get
            {
                if (PalettesDict.Count() == 0)
                    return new List<T>();

                int max = PalettesDict.Keys.Max();
                var arr = new T[max + 1];
                foreach (var kvp in PalettesDict)
                    arr[kvp.Key] = kvp.Value;
                return arr.ToList();
            }
            set
            {
                PalettesDict = new Dictionary<int, T>();
                for (int i = 0; i < value.Count; i++)
                {
                    PalettesDict[i] = value[i];
                }
            }
        }
        public Dictionary<int, T> PalettesDict { get; set; } = new Dictionary<int, T>();
        protected int _minBits = 1;

        protected Palette(IEnumerable<T> palettes, int minBits = 1)
        {
            Palettes = palettes.ToList();
            _minBits = minBits;
        }

        protected Palette(int minBits = 1)
        {
            _minBits = minBits;
        }

        public abstract T? GetAtIndex(int index);
        /// <summary>
        /// Decompresses the palette data into a list of the palette names in the same order.
        /// </summary>
        /// <param name="payloadSize">Size of the expected payload in index bit count.</param>
        public abstract List<T> Decompress(int payloadSize = -1);
    }
}
