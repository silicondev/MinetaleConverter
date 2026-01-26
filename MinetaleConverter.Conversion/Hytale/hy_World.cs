using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Zstd;
using MinetaleConverter.Base.Logging;
using MinetaleConverter.Conversion.Hytale.WorldEntities;
using MinetaleConverter.Conversion.Interfaces;
using MinetaleConverter.Conversion.Minecraft.WorldEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale
{
    public class hy_World : IWorld
    {
        private ILogger _logger;
        public List<IChunk> Chunks { get; private set; } = new List<IChunk>();
        public List<string> ChunkBsonFiles { get; private set; } = new List<string>();

        public hy_World(ILogger logger)
        {
            _logger = logger;
        }

        public int HeightLevel => 320;
        public int BedrockLevel => 0;

        public async Task<bool> ImportFile(string path, bool useAsync = true)
        {
            try
            {
                string configJsonPath = $@"{path}\config.json";
                if (!File.Exists(configJsonPath))
                    throw new ArgumentException("Hytale world folder does not contain a config.json file.");

                string levelName = "unknown";
                _logger.Info($"[{levelName}] Processing world...");

                var regionFiles = Directory.GetFiles($@"{path}\universe\worlds\default\chunks", "*.region.bin");

                if (regionFiles.Length == 0)
                    throw new ArgumentException($"[{levelName}] Hytale world folder contains no region files.");

                _logger.Info($"[{levelName}] Found {regionFiles.Length} region files.");

                int failed = 0;
                int success = 0;
                if (useAsync)
                {
                    var tasks = regionFiles.Select(x => Task.Factory.StartNew(() => ParseRegion(x, false)));

                    var results = await Task.WhenAll(tasks);
                    failed = results.Count(x => x.Count() == 0);
                    success = results.Count(x => x.Count() > 0);

                    Chunks.AddRange(results.Combine());
                }
                else
                {
                    foreach (var regionFile in regionFiles)
                    {
                        var chunks = ParseRegion(regionFile);
                        if (chunks.Count() > 0)
                            success++;
                        else
                            failed++;
                        Chunks.AddRange(chunks);
                    }
                }
                _logger.Info($"[{levelName}] {success} Region files processed. {failed} failed to process.");

                return success > 0;
            }
            catch (Exception e)
            {
                _logger.Critical(e);
                return false;
            }
        }

        public List<hy_Chunk> ParseRegion(string regionPath, bool log = true)
        {
            var chunks = new List<hy_Chunk>();

            string fileName = Path.GetFileName(regionPath);
            string[] fileNameParts = fileName.Split('.');
            int regionX = int.Parse(fileNameParts[0]);
            int regionZ = int.Parse(fileNameParts[1]);

            Binary bin = new Binary(File.ReadAllBytes(regionPath));
            string magic = bin.ReadLength<string>(20);
            if (magic != "HytaleIndexedStorage")
            {
                _logger.Error($"[{fileName}] >FAIL< Region file does not start with correct magic text.");
                return chunks;
            }
            int version = bin.Read<int>().SwapEndian();
            if (version < 0 || version > 1)
            {
                _logger.Error($"[{fileName}] >FAIL< Region file is not a supported verion. (Found {version}. Should be 0 or 1)");
                return chunks;
            }
            int blobCount = bin.Read<int>().SwapEndian();
            int segmentSize = bin.Read<int>().SwapEndian();
            var blobIndexes = new List<int>();
            //for (int i = 0; i < blobCount; i++)
            //{
            //    int index = indexesBin.Read<int>().SwapEndian();
            //    if (index != 0)
            //        blobIndexes.Add(index);
            //}

            for (int i = 0; i < blobCount; i++)
                blobIndexes.Add(bin.Read<int>().SwapEndian());

            int errorChunks = 0;

            for (int i = 0; i < blobCount; i++)
            {
                try
                {
                    int firstSegmentIndex = blobIndexes[i];
                    if (firstSegmentIndex == 0)
                        continue;

                    bin.Seek = ((firstSegmentIndex - 1) * segmentSize) + 32 + (blobCount * 4);

                    int srcLength = bin.Read<int>().SwapEndian();
                    int compLength = bin.Read<int>().SwapEndian();
                    byte[] compressedData = bin.Subset(compLength);
                    if (compressedData.Length != compLength)
                    {
                        _logger.Warn($"[{fileName}] Chunk#{i} Compressed data out of bounds of file.");
                        errorChunks++;
                        continue;
                    }
                    byte[] bsonData = ZstdHelper.Decompress(compressedData, srcLength);
                    (int chunkX, int chunkZ) = getChunkCoordinates(i, regionX, regionZ);

                    using (var memStream = new MemoryStream(bsonData))
                    using (var reader = new BsonDataReader(memStream))
                    {
                        var serializer = new JsonSerializer();
                        var obj = serializer.Deserialize(reader);
                        string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        ChunkBsonFiles.Add(json);
                    }

                    hy_Chunk? chunk = null;
                    using (var memStream = new MemoryStream(bsonData))
                    using (var reader = new BsonDataReader(memStream))
                    {
                        var serializer = new JsonSerializer();
                        chunk = serializer.Deserialize<hy_Chunk>(reader);
                    }
                    if (chunk != null)
                    {
                        chunk.Populate(chunkX, chunkZ);
                        chunks.Add(chunk);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"[{fileName}] Chunk#{i} failed to parse: {e.Message}");
                    errorChunks++;
                }
            }

            _logger.Info($"[{fileName}] >SUCCESS< Found {chunks.Count()} successful chunks." + (errorChunks > 0 ? $" (and {errorChunks} failed one(s).)" : ""));

            return chunks;
        }

        private (int chunkX, int chunkZ) getChunkCoordinates(int blobIndex, int regionX, int regionZ)
        {
            int localX = blobIndex % 32;
            int localZ = blobIndex / 32;
            int chunkX = regionX << 5 | localX;
            int chunkZ = regionZ << 5 | localZ;
            return (chunkX, chunkZ);
        }

        public string GetBlockId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 32d);
            int zChunkPos = (int)Math.Floor(z / 32d);

            var chunk = GetChunk(xChunkPos, zChunkPos);
            if (chunk == null)
                return "";

            return chunk.GetBlock(x - (xChunkPos * 32), y, z - (zChunkPos * 32));
        }

        public string GetBiomeId(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public IChunk? GetChunk(int x, int z) =>
            Chunks.FirstOrDefault(c => c.xPos == x && c.zPos == z);
    }
}
