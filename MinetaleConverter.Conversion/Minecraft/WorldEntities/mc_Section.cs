using MinetaleConverter.Base;
using MinetaleConverter.Base.Attributes;
using MinetaleConverter.Base.Converters;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft.WorldEntities
{
    public class mc_Section
    {
        [NbtConverter(typeof(SignedByteToInt))]
        public int Y { get; internal set; }
        [NbtProperty("block_states")]
        [NbtTagType(TagType.Compound)]
        public mc_ResourcePalette BlockStates { get; internal set; }
        [NbtProperty("biomes")]
        [NbtTagType(TagType.Compound)]
        public mc_ResourcePalette Biomes { get; internal set; }
        public byte[] BlockLight { get; internal set; }
        public byte[] SkyLight { get; internal set; }

        public mc_Resource? GetBlock(int x, int y, int z) =>
            BlockStates.GetPalette(GetBlockIndexFromCoords(x, y, z), 4);

        public mc_Resource? GetBiome(int x, int y, int z) =>
            Biomes.GetPalette(GetBiomeIndexFromCoords(x, y, z));

        public static int GetBlockIndexFromCoords(int x, int y, int z) =>
            y * 16 * 16 + z * 16 + x;

        public static int GetBiomeIndexFromCoords(int x, int y, int z) =>
            y / 4 * 4 * 4 + z / 4 * 4 + x / 4;

        public static (int x, int y, int z) GetBlockCoordsFromIndex(int index)
        {
            for (int x = 0; x < 16; x++)
                for (int z = 0; z < 16; z++)
                    for (int y = 0; y < 16; y++)
                        if (GetBlockIndexFromCoords(x, y, z) == index)
                            return (x, y, z);
            return (-1, -1, -1);
        }

        public static (int x, int y, int z) GetBiomeCoordsFromIndex(int index)
        {
            for (int x = 0; x < 4; x++)
                for (int z = 0; z < 4; z++)
                    for (int y = 0; y < 4; y++)
                        if (GetBiomeIndexFromCoords(x, y, z) == index)
                            return (x * 4, y * 4, z * 4);
            return (-1, -1, -1);
        }
    }
}
