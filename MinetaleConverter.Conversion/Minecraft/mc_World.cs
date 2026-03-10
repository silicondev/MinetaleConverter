using MinetaleConverter.Base;
using MinetaleConverter.Base.Logging;
using MinetaleConverter.Compression.ZLib;
using MinetaleConverter.Conversion.Interfaces;
using MinetaleConverter.Conversion.Minecraft.WorldEntities;
using SharpNBT;
using System.IO;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class mc_World : IWorld
    {
        private ILogger _logger;

        public mc_Level Level { get; private set; } = new mc_Level();
        public List<byte[]> bChunkDataNBT { get; private set; } = new List<byte[]>();
        public Dictionary<string, List<IChunk>> Chunks { get; private set; } = new Dictionary<string, List<IChunk>>();
        public Dictionary<string, List<mc_Chunk>> FullChunks =>
            Chunks.Select(x => new KeyValuePair<string, List<mc_Chunk>>(x.Key, x.Value.Cast<mc_Chunk>().Where(y => y.Status == "minecraft:full").ToList())).Where(x => x.Value.Any()).ToDictionary();

        public mc_World(ILogger logger)
        {
            _logger = logger;
        }

        public int HeightLevel => 320;
        public int BedrockLevel => -64;

        public async Task<bool> ImportFile(string path, bool useAsync = true)
        {
            try
            {
                string levelDatPath = $@"{path}\level.dat";
                if (!File.Exists(levelDatPath))
                    throw new ArgumentException("Minecraft world folder does not contain a level.dat file.");

                var levelTag = NbtFile.Read(levelDatPath, FormatOptions.Java);
                NbtHelper.FillFromTag(Level, levelTag.Find<CompoundTag>("Data"));

                if (!Level.Initialized)
                    throw new ArgumentException("Minecraft world is not initialized.");

                _logger.Info($"[{Level.LevelName}] Processing world...");

                var regionFiles = Directory.GetFiles($@"{path}\region", "*.mca");

                if (regionFiles.Length == 0)
                    throw new ArgumentException($"[{Level.LevelName}] Minecraft world folder contains no region files.");

                _logger.Info($"[{Level.LevelName}] Found {regionFiles.Length} region files.");

                int failed = 0;
                int success = 0;
                if (useAsync)
                {
                    var tasks = regionFiles.Select(x => Task.Factory.StartNew(() => (Path.GetFileName(x), ParseRegion(x, false))));

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
                        var chunks = ParseRegion(regionFile);
                        if (chunks.Count() > 0)
                            success++;
                        else
                            failed++;
                        Chunks.Add(Path.GetFileName(regionFile), chunks.Cast<IChunk>().ToList());
                    }
                }
                _logger.Info($"[{Level.LevelName}] {success} Region files processed. {failed} failed to process.");

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
            return false;
        }

        public List<mc_Chunk> ParseRegion(string regionPath, bool log = true)
        {
            var chunks = new List<mc_Chunk>();

            // Sanity check and setup

            if (string.IsNullOrEmpty(regionPath))
                return chunks;

            string fileName = Path.GetFileName(regionPath).ToLower();

            if (!fileName.EndsWith(".mca"))
                return chunks;

            try
            {
                // Binary extraction

                byte[] data = [];

                using (var fs = new FileStream(regionPath, FileMode.Open))
                using (var reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }

                if (data.Length == 0)
                    return chunks;

                if (data.Length < 8192)
                {
                    _logger.Warn($"[{fileName}] >FAIL< Region file does not contain enough data to support chunk decompression. Found {data.Length} (< 8192) bytes.");
                    return chunks;
                }

                byte[] locations = data[0..4096];
                byte[] timestamps = data[4096..8192];
                byte[] payload = data[8192..];

                // Chunking payload data into 4kb chunks

                var byteChunks = new List<byte[]>();

                while (true)
                {
                    if (payload.Length == 0)
                        break;
                    if (payload.Length < 4096)
                    {
                        byteChunks.Add(payload);
                        break;
                    }
                    byteChunks.Add(payload[0..4096]);
                    payload = payload[4096..];
                }

                // Loop through each location
                int errorChunks = 0;

                for (int i = 0; i < locations.Length; i += 4)
                {
                    // Setup offset and length

                    byte[] bOffset = locations[i..(i + 3)];
                    byte len = locations[i + 3];

                    if (len == 0)
                        continue;

                    int offset = bOffset.SwapEndian() - 2;

                    if (offset < 0)
                        continue;

                    // Find chunk payload data and decompress

                    byte compType = byteChunks[offset][4];
                    // Skipping 2 bytes for the data (5 > 7) so it... works?
                    byte[] compressedData = byteChunks[offset..(offset + len)].Combine()[7..];
                    var method = (CompressionMethod)compType;
                    bool decompSuccess = false;
                    byte[]? decompChunk = null;
                    switch (method)
                    {
                        case CompressionMethod.ZLIB:
                            decompSuccess = ZLibHelper.Decompress(compressedData, out decompChunk);
                            break;
                        default:
                            _logger.Error($"[{fileName}] Chunk#{offset} Unrecognised compression method: {compType}.");
                            continue;
                    }
                    if (!decompSuccess || decompChunk == null)
                    {
                        if (log) _logger.Error($"[{fileName}] Chunk#{offset} Decompression failed.");
                        errorChunks++;
                        continue;
                    }

                    // Read compound chunk tag

                    CompoundTag? tag = null;
                    try
                    {
                        using (var memStream = new MemoryStream(decompChunk))
                        {
                            var tagReader = new TagReader(memStream, FormatOptions.Java);
                            tag = tagReader.ReadTag<CompoundTag>();
                        }
                    }
                    catch (Exception e)
                    {
                        tag = null;
                    }

                    if (tag == null)
                    {
                        if (log) _logger.Warn($"[{fileName}] Chunk#{offset} Could not read NBT tag.");
                        errorChunks++;
                        continue;
                    }

                    // Fill chunk tag into data structure

                    var chunk = new mc_Chunk();
                    NbtHelper.FillFromTag(chunk, tag);
                    chunk.NbtData = decompChunk;
                    chunks.Add(chunk);
                }

                _logger.Info($"[{fileName}] >SUCCESS< Found {chunks.Count()} successful chunks." + (errorChunks > 0 ? $" (and {errorChunks} failed one(s).)" : ""));
                return chunks;
            }
            catch (Exception e)
            {
                _logger.Critical($"[{fileName}] >FAIL< Error parsing region file.", e);
                return new List<mc_Chunk>();
            }
        }

        public string GetBlockId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 16d);
            int zChunkPos = (int)Math.Floor(z / 16d);

            var chunk = GetChunk(xChunkPos, zChunkPos);
            if (chunk == null)
                return "";

            return chunk.GetBlock(x - (xChunkPos * 16), y, z - (zChunkPos * 16));
        }

        public string GetBiomeId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 16d);
            int zChunkPos = (int)Math.Floor(z / 16d);

            var chunk = GetChunk(xChunkPos, zChunkPos);
            if (chunk == null)
                return "";

            return chunk.GetBiome(x - (xChunkPos * 16), y, z - (zChunkPos * 16));
        }

        public string GetChunkId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 16d);
            int zChunkPos = (int)Math.Floor(z / 16d);
            int sectionId = (int)Math.Floor(y / 16d);

            var chunk = GetChunk(xChunkPos, zChunkPos);

            if (chunk == null)
                return $"No chunk found at {xChunkPos},{zChunkPos}";

            return $"Chunk_{chunk.xPos}.{chunk.zPos} Section {sectionId}";
        }

        public mc_Chunk? GetChunk(int x, int z) =>
            FullChunks.Values.Combine().FirstOrDefault(c => c.xPos == x && c.zPos == z);

        public void SetBlockId(string blockId, int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}
