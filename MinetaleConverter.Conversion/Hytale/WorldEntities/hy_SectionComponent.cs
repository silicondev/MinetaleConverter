using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Palette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_SectionComponent
    {
        public hy_Binary Block
        {
            set
            {
                if (value.Version != null)
                    Version = value.Version.Value;

                var bytes = value.Data;
                char[] chars = bytes.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
                var bin = new Binary(bytes);
                //bin.Seek = 1;
                int migrationVer = bin.Read<int>().SwapEndian();
                var paletteType = (HytalePaletteType)bin.Read<byte>();

                if (paletteType == HytalePaletteType.Empty)
                    return;

                int blockCount = 32 * 32 * 32;
                int minBits = 8;
                switch (paletteType)
                {
                    case HytalePaletteType.Half:
                        blockCount /= 2;
                        minBits = 4;
                        break;
                    case HytalePaletteType.Short:
                        blockCount *= 2;
                        minBits = 16;
                        break;
                }

                BlockPalette = new BytePalette<string>(minBits);

                short paletteCount = bin.Read<short>().SwapEndian();
                var palettes = new Dictionary<int, string>();
                for (int i = 0; i < paletteCount; i++)
                {
                    short key = paletteType == HytalePaletteType.Short ? bin.Read<short>().SwapEndian() : bin.Read<byte>();
                    //string name = bin.ReadGivenLength<string, ushort>((x) => x.SwapEndian());
                    ushort nameLen = bin.Read<ushort>().SwapEndian();
                    string name = bin.ReadLength<string>(nameLen);
                    short count = bin.Read<short>().SwapEndian();
                    palettes[key] = name;
                }
                BlockPalette.PalettesDict = palettes;
                bin.Cut();
                bin = new Binary(bin.Subset(blockCount));
                BlockPalette.Data = bin.Bytes;
                chars = BlockPalette.Data.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
                var list = BlockPalette.Decompress(blockCount);
            }
        }

        public int Version { get; set; }
        public BytePalette<string> BlockPalette { get; set; } = new BytePalette<string>();
    }
}
