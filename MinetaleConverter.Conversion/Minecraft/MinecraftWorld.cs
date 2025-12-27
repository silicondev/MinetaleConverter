using MinetaleConverter.Base;
using MinetaleConverter.Base.Entities;
using MinetaleConverter.Base.Logging;
using MinetaleConverter.Compression;
using SharpNBT;
using System.IO;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class MinecraftWorld
    {
        private ILogger _logger;

        public Level Level { get; private set; } = new Level();
        public List<byte[]> bChunkDataNBT { get; private set; } = new List<byte[]>();
        public List<Chunk> Chunks { get; private set; } = new List<Chunk>();
        public List<Chunk> FullChunks => Chunks.Where(x => x.Status == "minecraft:full").ToList();

        public MinecraftWorld(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> ImportFile(string path)
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

                var tasks = regionFiles.Select(x => Task.Factory.StartNew(() => ParseRegion(x, false)));

                var results = await Task.WhenAll(tasks);
                int failed = results.Count(x => x.Count() == 0);
                int success = results.Count(x => x.Count() > 0);

                _logger.Info($"[{Level.LevelName}] {success} Region files processed. {failed} failed to process.");

                Chunks.AddRange(results.Combine());
                return success > 0;
            }
            catch (Exception e)
            {
                _logger.Critical(e);
                return false;
            }
        }

        public List<Chunk> ParseRegion(string regionPath, bool log = true)
        {
            var chunks = new List<Chunk>();

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
                    bool decompSuccess = ZipHelper.Decompress(compressedData, out byte[]? decompChunk, (CompressionMethod)compType);
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
                        if (log) _logger.Error($"[{fileName}] Chunk#{offset} Could not read NBT tag.");
                        errorChunks++;
                        continue;
                    }

                    // Fill chunk tag into data structure

                    var chunk = new Chunk();
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
                return new List<Chunk>();
            }
        }

        public string GetBlockId(int x, int y, int z)
        {
            int xChunkPos = (int)(x / 16d);
            int zChunkPos = (int)(z / 16d);

            var chunk = GetChunk(xChunkPos, zChunkPos);
            if (chunk == null)
                return "";

            var palette = chunk.GetBlock(x - (xChunkPos * 16), y, z - (zChunkPos * 16));
            return palette?.Name ?? "minecraft:air";
        }

        public Chunk? GetChunk(int x, int z) =>
            FullChunks.FirstOrDefault(c => c.xPos == x && c.zPos == z);
    }
}
