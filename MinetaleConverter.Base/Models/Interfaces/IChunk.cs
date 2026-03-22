using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Models.Interfaces
{
    public interface IChunk
    {
        int Size { get; }
        int xPos { get; }
        int zPos { get; }
        string? GetBlock(int x, int y, int z);
        void SetBlock(string blockId, int x, int y, int z);
        string? GetBiome(int x, int y, int z);
    }
}
