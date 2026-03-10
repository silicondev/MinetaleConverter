using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Interfaces
{
    public interface IChunk
    {
        int xPos { get; }
        int zPos { get; }
        string GetBlock(int x, int y, int z);
        void SetBlock(string blockId, int x, int y, int z);
        string GetBiome(int x, int y, int z);
    }
}
