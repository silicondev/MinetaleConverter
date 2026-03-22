using MinetaleConverter.Base.Logic.Serialization.NBT.Attributes;
using MinetaleConverter.Base.Logic.Serialization.NBT.Converters;
using MinetaleConverter.Minecraft.Logic;
using SharpNBT;

namespace MinetaleConverter.Minecraft.Models
{
    public class mc_Section
    {
        [NbtConverter(typeof(SignedByteToInt))]
        public int Y { get; internal set; }
        [NbtProperty("block_states")]
        [NbtTagType(TagType.Compound)]
        [NbtReader(typeof(mc_BlockPaletterReader))]
        public mc_ResourcePalette BlockStates
        {
            get => _blockStates;
            internal set
            {
                _blockStates = new mc_ResourcePalette(value.Palettes, value.Data, 4);
            }
        }
        [NbtProperty("biomes")]
        [NbtTagType(TagType.Compound)]
        [NbtReader(typeof(mc_BiomePaletterReader))]
        public mc_ResourcePalette Biomes
        {
            get => _biomes;
            internal set
            {
                _biomes = new mc_ResourcePalette(value.Palettes, value.Data);
            }
        }
        public byte[] BlockLight { get; internal set; }
        public byte[] SkyLight { get; internal set; }

        private mc_ResourcePalette? _blockStates;
        private mc_ResourcePalette? _biomes;

        public mc_Resource? GetBlock(int x, int y, int z) =>
            BlockStates.GetAtIndex(GetBlockIndexFromCoords(x, y, z));

        public mc_Resource? GetBiome(int x, int y, int z) =>
            Biomes.GetAtIndex(GetBiomeIndexFromCoords(x, y, z));

        public int GetBlockIndexFromCoords(int x, int y, int z) =>
            BlockStates.Indexer(x, y, z);

        public static int GetBiomeIndexFromCoords(int x, int y, int z) =>
            y / 4 * 4 * 4 + z / 4 * 4 + x / 4;

        public (int x, int y, int z) GetBlockCoordsFromIndex(int index)
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
