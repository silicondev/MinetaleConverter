using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Section
    {
        //public int Id { get; set; }
        public hy_SectionComponent Components { get; set; } = new hy_SectionComponent();

        //public string? GetBlock(int x, int y, int z) =>
        //    Components.BlockPalette.GetAtIndex(GetBlockIndexFromCoords(x, y, z));

        //public static int GetBlockIndexFromCoords(int x, int y, int z) => (y & 31) << 10 | (z & 31) << 5 | x & 31;

        //public static (int x, int y, int z) GetBlockCoordsFromIndex(int index)
        //{
        //    for (int x = 0; x < 32; x++)
        //        for (int z = 0; z < 32; z++)
        //            for (int y = 0; y < 32; y++)
        //                if (GetBlockIndexFromCoords(x, y, z) == index)
        //                    return (x, y, z);
        //    return (-1, -1, -1);
        //}
    }
}
