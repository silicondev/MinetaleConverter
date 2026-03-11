using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Models.Interfaces
{
    public interface IWorld
    {
        string LevelName { get; }
        Dictionary<string, List<IChunk>> Chunks { get; }
        Task<bool> ImportFile(string path, bool useAsync = true);
        Task<bool> ExportFile(string path, bool useAsync = true);
        int HeightLevel { get; }
        int BedrockLevel { get; }
        string GetBlockId(int x, int y, int z);
        void SetBlockId(string blockId, int x, int y, int z);
        string GetBiomeId(int x, int y, int z);
        string GetChunkId(int x, int y, int z);
    }
}
