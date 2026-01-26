using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Palette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_EnvironmentChunk
    {
        public byte[] Data
        {
            set
            {
                char[] chars = value.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
                var bin = new Binary(value);
                int checkInt = bin.Read<int>().SwapEndian();

                int minBits = 32;
                BiomePalette = new BytePalette<string>(minBits);

                var palettes = new Dictionary<int, string>();
                for (int i = 0; true; i++)
                {
                    int check = bin.Read<int>().SwapEndian();
                    if (check == checkInt)
                        break;
                    ushort nameLen = bin.Read<ushort>().SwapEndian();
                    string name = bin.ReadLength<string>(nameLen);
                    palettes.Add(i, name);
                }
                BiomePalette.PalettesDict = palettes;
                bin.Seek += 4;
                bin.Cut();
                var bigList = new List<List<uint>>();
                var list = new List<uint>();
                while (!bin.EOF)
                {
                    uint read = bin.Read<uint>().SwapEndian();
                    if (read == uint.MaxValue)
                    {
                        bigList.Add(new List<uint>(list));
                        list.Clear();
                    }
                    else
                        list.Add(read);
                }
                int k = 0;
                // This data currently makes no sense. More investigation is required.
            }
        }

        public BytePalette<string> BiomePalette { get; set; } = new BytePalette<string>();
    }
}
