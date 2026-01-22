using MinetaleConverter.Conversion.Hytale.WorldEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Interfaces
{
    public interface IWorld
    {
        List<IChunk> Chunks { get; }
        Task<bool> ImportFile(string path, bool useAsync = true);
        int HeightLevel { get; }
        int BedrockLevel { get; }
        string GetBlockId(int x, int y, int z);
        string GetBiomeId(int x, int y, int z);
    }
}
