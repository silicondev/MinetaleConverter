using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Palette;
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
        public Dictionary<string, List<IChunk>> Chunks { get; private set; } = new Dictionary<string, List<IChunk>>();
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
                    var tasks = regionFiles.Select(x => Task.Factory.StartNew(() => (Path.GetFileName(x), parseRegion(x, false))));

                    var results = await Task.WhenAll(tasks);
                    failed = results.Count(x => x.Item2.Count() == 0);
                    success = results.Count(x => x.Item2.Count() > 0);

                    foreach (var result in results)
                    {
                        Chunks.Add(result.Item1, result.Item2.Cast<IChunk>().ToList());
                    }
                }
                else
                {
                    foreach (var regionFile in regionFiles)
                    {
                        var chunks = parseRegion(regionFile);
                        if (chunks.Count() > 0)
                            success++;
                        else
                            failed++;
                        Chunks.Add(Path.GetFileName(regionFile), chunks.Cast<IChunk>().ToList());
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

        public async Task<bool> ExportFile(string path, bool useAsync = true)
        {
            try
            {
                if (useAsync)
                {

                }
                else
                {
                    foreach (var kvp in Chunks)
                    {
                        string regionPath = Path.Combine(path, kvp.Key);
                        exportRegion(kvp.Key, regionPath);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Critical(e);
                return false;
            }
        }

        private List<hy_Chunk> parseRegion(string regionPath, bool log = true)
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

        private bool exportRegion(string regionFileName, string outputPath, bool log = true)
        {
            if (!Chunks.ContainsKey(regionFileName))
            {
                _logger.Error($"Could not find chunk data for {regionFileName}");
                return false;
            }

            string[] fileNameParts = regionFileName.Split('.');
            int regionX = int.Parse(fileNameParts[0]);
            int regionZ = int.Parse(fileNameParts[1]);

            var chunks = Chunks[regionFileName].Cast<hy_Chunk>().ToArray();
            using (var fileStream = File.OpenWrite(outputPath))
            using (var writer = new BinaryWriter(fileStream))
            {
                // Magic
                writer.Write(Encoding.UTF8.GetBytes("HytaleIndexedStorage"));
                // Version
                writer.Write(1.SwapEndian());

                var dict = new Dictionary<int, byte[]>();

                foreach (var chunk in chunks)
                {
                    var chunkBin = new Binary();
                    byte[] srcBytes;
                    using (var memStream = new MemoryStream())
                    {
                        using (var bsonWriter = new BsonDataWriter(memStream))
                        {
                            bsonWriter.AutoCompleteOnClose = true;
                            var serializer = new JsonSerializer();
                            serializer.NullValueHandling = NullValueHandling.Ignore;
                            serializer.Serialize(bsonWriter, chunk);
                        }
                        srcBytes = memStream.ToArray();
                    }

                    using (var memStream = new MemoryStream(srcBytes))
                    using (var bsonReader = new BsonDataReader(memStream))
                    {
                        var serializer = new JsonSerializer();
                        object? obj = serializer.Deserialize(bsonReader);
                        int k = 0;
                    }
                    // Src length
                    chunkBin.Write(srcBytes.Length.SwapEndian());

                    byte[] compressedBytes = ZstdHelper.Compress(srcBytes);

                    // Compressed length
                    chunkBin.Write(compressedBytes.Length.SwapEndian());
                    chunkBin.Write(compressedBytes);

                    dict.Add(getChunkIndex(chunk.xPos, chunk.zPos, regionX, regionZ), chunkBin.Bytes.ToArray());
                }


                //int blobCount = dict.Keys.Max() + 1;
                int blobCount = 1024;
                // Blob count
                writer.Write(blobCount.SwapEndian());

                int segmentSize = dict.Values.Select(x => x.Length).Max();
                writer.Write(segmentSize.SwapEndian());

                int[] blobIndexes = new int[blobCount];
                int segment = 1;
                for (int i = 0; i < blobCount; i++)
                {
                    if (dict.ContainsKey(i))
                    {
                        blobIndexes[i] = segment;
                        segment++;
                    }
                    else
                        blobIndexes[i] = 0;
                }

                for (int i = 0; i < blobCount; i++)
                    writer.Write(blobIndexes[i].SwapEndian());

                for (int i = 0; i < blobCount; i++)
                {
                    int firstSegmentIndex = blobIndexes[i];
                    if (firstSegmentIndex > 0)
                    {
                        writer.Seek((firstSegmentIndex - 1) * segmentSize + 32 + (blobCount * 4), SeekOrigin.Begin);
                        writer.Write(dict[i]);
                    }
                }
            }
            /*
            var bin = new Binary();
            // Magic
            bin.Write("HytaleIndexedStorage");
            // Version
            bin.Write(1.SwapEndian());
            // Blob count
            bin.Write(chunks.Length.SwapEndian());

            var dict = new Dictionary<int, byte[]>();

            foreach (var chunk in chunks)
            {
                var chunkBin = new Binary();
                byte[] srcBytes;
                using (var memStream = new MemoryStream())
                using (var writer = new BsonDataWriter(memStream))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, chunk);
                    srcBytes = memStream.ToArray();
                }
                // Src length
                chunkBin.Write(srcBytes.Length.SwapEndian());

                byte[] compressedBytes = ZstdHelper.Compress(srcBytes);

                // Compressed length
                chunkBin.Write(compressedBytes.Length.SwapEndian());
                chunkBin.Write(compressedBytes);

                dict.Add(getChunkIndex(chunk.xPos, chunk.zPos, regionX, regionZ), chunkBin.Bytes.ToArray());
            }

            int segmentSize = dict.Values.Select(x => x.Length).Max();
            bin.Write(segmentSize.SwapEndian());

            int maxIndex = dict.Keys.Max();

            for (int i = 0; i <= maxIndex; i++)
            {
                // Blob indexes
                if (!dict.ContainsKey(i))
                    bin.Write(0);
                else
                    bin.Write(((i + 1) * segmentSize) + 32 + (maxIndex * 4));
            }

            for (int i = 0; i <= maxIndex; i++)
            {
                // Blob data write
                if (!dict.ContainsKey(i))
                    continue;
                else
                {
                    bin.Seek = ((i + 1) * segmentSize) + 32 + (maxIndex * 4);
                    bin.Write(dict[i]);
                }
            }

            using (var fileStream = File.OpenWrite(outputPath))
            {
                var bytes = bin.Bytes;
                foreach (byte b in bytes)
                    fileStream.WriteByte(b);
            }
            */
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

        private int getChunkIndex(int chunkX, int chunkZ, int regionX, int regionZ)
        {
            int localX = chunkX - (regionX * 32);
            int localZ = chunkZ - (regionZ * 32);
            return localX + (localZ * 32);
        }

        //private int getChunkIndex(int chunkX, int chunkZ, int regionX, int regionZ)
        //{
        //    int localX = chunkX & 31;
        //    int localZ = chunkZ & 31;
        //    return ((int)localX << 32) | (localZ & 0xFFFF);
        //}

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
            Chunks.Values.Combine().FirstOrDefault(c => c.xPos == x && c.zPos == z);
    }
}
