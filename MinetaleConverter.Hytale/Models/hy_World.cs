using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Zstd;
using MinetaleConverter.Base.Logic.Data;
using MinetaleConverter.Base.Logic.Extensions;
using MinetaleConverter.Base.Logic.Interfaces;
using MinetaleConverter.Base.Logic.Logging;
using MinetaleConverter.Base.Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace MinetaleConverter.Hytale.Models
{
    public class hy_World : IWorld
    {
        private WorldLogger? _logger;

        public string LevelName { get; private set; } = "unknown";
        public Dictionary<string, List<IChunk>> Chunks { get; } = new Dictionary<string, List<IChunk>>();

        public int HeightLevel => 319;

        public int BedrockLevel => 0;

        public async Task<bool> ImportFile(string path, bool useAsync = true)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Hytale world folder not found.");

            string configFilePath = $@"{path}\config.json";
            if (!File.Exists(configFilePath))
                throw new ArgumentException("Hytale world folder does not contain a config.json file.");

            // Parse config here...

            _logger = new WorldLogger(this, new ConsoleLogger());

            try
            {
                var regionFiles = Directory.GetFiles($@"{path}\universe\worlds\default\chunks", "*.region.bin");

                if (regionFiles.Length == 0)
                    throw new ArgumentException($"");

                int failed = 0;
                int success = 0;
                if (useAsync)
                {
                    //var tasks = regionFiles.Select(x => Task.Factory.StartNew(() => (Path.GetFileName(x), parseRegion(x, false))));
                    var tasks = regionFiles.Select(x => Task.Factory.StartNew(() =>
                    {
                        bool result = parseRegion(x, out List<hy_Chunk> chunks, false);
                        return (result, Path.GetFileName(x), chunks);
                    }));

                    var results = await Task.WhenAll(tasks);
                    failed = results.Count(x => !x.result);
                    success = results.Count(x => x.result);

                    foreach (var result in results.Where(x => x.result))
                    {
                        Chunks.Add(result.Item2, result.Item3.Cast<IChunk>().ToList());
                    }
                }
                else
                {
                    foreach (var regionFile in regionFiles)
                    {
                        bool result = parseRegion(regionFile, out List<hy_Chunk> chunks);
                        if (result)
                        {
                            success++;
                            _logger.Info($"{Path.GetFileName(regionFile)} was successfully decoded with {chunks.Count} successful chunks!");
                            Chunks.Add(Path.GetFileName(regionFile), chunks.Cast<IChunk>().ToList());
                        }
                        else
                            failed++;
                    }
                }
                _logger.Info($"{success} Region files processed. {failed} failed to process.");

                return success > 0;
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
                return false;
            }
        }

        public async Task<bool> ExportFile(string path, bool useAsync = true)
        {
            throw new NotImplementedException();
        }

        public string? GetBiomeId(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public string? GetBlockId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 32d);
            int zChunkPos = (int)Math.Floor(z / 32d);

            var chunk = GetChunk(xChunkPos, zChunkPos);
            if (chunk == null)
                return null;

            return chunk.GetBlock(x - (xChunkPos * 32), y, z - (zChunkPos * 32));
        }

        public string? GetChunkId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 32d);
            int zChunkPos = (int)Math.Floor(z / 32d);
            int sectionId = (int)Math.Floor(y / 32d);

            var chunk = GetChunk(xChunkPos, zChunkPos);

            if (chunk == null)
                return $"No chunk found at {xChunkPos},{zChunkPos}";

            return $"Chunk_{chunk.xPos}.{chunk.zPos} Section: {sectionId} In Chunk: {x - (chunk.xPos * 32)},{y - sectionId * 32},{z - (chunk.zPos * 32)}";
        }

        public void SetBlockId(string blockId, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public IChunk? GetChunk(int chunkX, int chunkZ) =>
            Chunks.Values.Combine().FirstOrDefault(c => c.xPos == chunkX && c.zPos == chunkZ);

        private bool parseRegion(string regionPath, out List<hy_Chunk> chunks, bool log = true)
        {
            string fileName = Path.GetFileName(regionPath);
            string[] parts = fileName.Split('.');
            int regionX = int.Parse(parts[0]);
            int regionZ = int.Parse(parts[1]);

            chunks = new List<hy_Chunk>();

            var bin = new Binary(File.ReadAllBytes(regionPath), EndianMode.Big);

            string magic = bin.ReadLength<string>(20);
            if (magic != "HytaleIndexedStorage")
            {
                if (log) _logger?.Error($"[{fileName}] >FAIL< Region file does not start with correct magic text: {magic}");
                return false;
            }

            int version = bin.Read<int>();
            if (version < 0 || version > 1)
            {
                if (log) _logger?.Error($"[{fileName}] >FAIL< Region is not a supported version: {version}");
                return false;
            }

            int blobCount = bin.Read<int>();
            int segmentSize = bin.Read<int>();

            var blobIndexes = new List<int>();
            for (int i = 0; i < blobCount; i++)
                blobIndexes.Add(bin.Read<int>());

            int errorChunks = 0;

            for (int i = 0; i < blobCount; i++)
            {
                try
                {
                    int segmentIndex = blobIndexes[i];

                    if (segmentIndex == 0)
                        continue;

                    bin.Seek = ((segmentIndex - 1) * segmentSize) + (blobCount * 4) + 32;

                    (int chunkX, int chunkZ) = getChunkCoordinates(i, regionX, regionZ);

                    int srcLength = bin.Read<int>();
                    int compLength = bin.Read<int>();

                    byte[] compressed = bin.Subset(compLength);

                    if (compressed.Length != compLength)
                    {
                        if (log) _logger?.Warn($"[{fileName}] Chunk#{i} Compressed data out of bounds of file.");
                        errorChunks++;
                        continue;
                    }

                    byte[] bsonData = ZstdHelper.Decompress(compressed, srcLength);

                    hy_Chunk? chunk = null;
                    using (var memStream = new MemoryStream(bsonData))
                    using (var reader = new BsonDataReader(memStream))
                    {
                        var serializer = new JsonSerializer();
                        chunk = serializer.Deserialize<hy_Chunk>(reader);
                    }

                    if (chunk != null)
                    {
                        chunk.xPos = chunkX;
                        chunk.zPos = chunkZ;
                        chunks.Add(chunk);
                    }
                }
                catch (Exception e)
                {
                    if (log) _logger?.Error($"[{fileName}] Chunk#{i} failed to parse: {e.Message}");
                    errorChunks++;
                }
            }
            
            return true;
        }

        private (int chunkX, int chunkZ) getChunkCoordinates(int blobIndex, int regionX, int regionZ)
        {
            int localX = blobIndex % 32;
            int localZ = blobIndex / 32;
            int chunkX = (regionX * 32) + localX;
            int chunkZ = (regionZ * 32) + localZ;
            return (chunkX, chunkZ);
        }
    }
}
