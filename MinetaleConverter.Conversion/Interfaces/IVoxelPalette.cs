using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Interfaces
{
    public interface IVoxelPalette<T>
    {
        string DefaultVoxel { get; }
        Func<int, int, int, int> Indexer { get; }
        Dictionary<int, T> PaletteList { get; }
        byte[]? PaletteData { get; }
        int PaletteHeight { get; }
        int PaletteWidth { get; }
        int PaletteDepth { get; }
        T[] Decompress(int payloadSize = -1);
        T Get(int x, int y, int z);
        void Set(T value, int x, int y, int z);
        void Build(T[] list, bool repopulate = true);
    }
}
