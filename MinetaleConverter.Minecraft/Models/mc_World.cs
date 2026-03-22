using MinetaleConverter.Base.Logic.Compression.Zlib;
using MinetaleConverter.Base.Logic.Data;
using MinetaleConverter.Base.Logic.Extensions;
using MinetaleConverter.Base.Logic.Logging;
using MinetaleConverter.Base.Logic.Serialization.NBT;
using MinetaleConverter.Base.Models.Interfaces;
using SharpNBT;
using System.Buffers.Binary;
using System.Reflection.Emit;

namespace MinetaleConverter.Minecraft.Models
{
    public class mc_World : IWorld
    {
        private WorldLogger? _logger;

        public string LevelName { get; private set; } = "unknown";
        public Dictionary<string, List<IChunk>> Chunks { get; } = new Dictionary<string, List<IChunk>>();

        public int HeightLevel => 319;

        public int BedrockLevel => -64;

        public async Task<bool> ImportFile(string path, bool useAsync = true)
        {
            try
            {
                string levelDatPath = $@"{path}\level.dat";
                if (!File.Exists(levelDatPath))
                    throw new ArgumentException("Minecraft world folder does not contain a level.dat file.");

                var levelTag = NbtFile.Read(levelDatPath, FormatOptions.Java);
                //NbtHelper.FillFromTag(Level, levelTag.Find<CompoundTag>("Data"));

                //if (!Level.Initialized)
                //    throw new ArgumentException("Minecraft world is not initialized.");

                //LevelName = Level.LevelName;

                _logger = new WorldLogger(this, new ConsoleLogger());

                _logger.Info($"[{LevelName}] Processing world...");

                var regionFiles = Directory.GetFiles($@"{path}\region", "*.mca");

                if (regionFiles.Length == 0)
                    throw new ArgumentException($"[{LevelName}] Minecraft world folder contains no region files.");

                _logger.Info($"[{LevelName}] Found {regionFiles.Length} region files.");

                int failed = 0;
                int success = 0;
                if (useAsync)
                {
                    var tasks = regionFiles.Select(x => Task.Factory.StartNew(() =>
                    {
                        bool result = parseRegion(x, out List<mc_Chunk> chunks, false);
                        return (result, Path.GetFileName(x), chunks);
                    }));

                    var results = await Task.WhenAll(tasks);
                    failed = results.Count(x => x.Item2.Count() == 0);
                    success = results.Count(x => x.Item2.Count() > 0);

                    foreach (var result in results)
                    {
                        Chunks.Add(result.Item2, result.Item3.Cast<IChunk>().ToList());
                    }
                }
                else
                {
                    foreach (var regionFile in regionFiles)
                    {
                        bool result = parseRegion(regionFile, out List<mc_Chunk> chunks);
                        if (result && chunks.Count > 0)
                        {
                            success++;
                            Chunks.Add(Path.GetFileName(regionFile), chunks.Cast<IChunk>().ToList());
                        }
                        else
                            failed++;
                    }
                }
                _logger.Info($"[{LevelName}] {success} Region files processed. {failed} failed to process.");

                return success > 0;
            }
            catch (Exception e)
            {
                _logger?.Fatal(e);
                return false;
            }
        }

        public Task<bool> ExportFile(string path, bool useAsync = true)
        {
            throw new NotImplementedException();
        }

        public string GetBiomeId(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public string GetBlockId(int x, int y, int z)
        {
            int xChunkPos = (int)Math.Floor(x / 16d);
            int zChunkPos = (int)Math.Floor(z / 16d);

            var chunk = GetChunk(xChunkPos, zChunkPos);
            if (chunk == null)
                return "Chunk not found.";

            return chunk.GetBlock(x - (xChunkPos * 16), y, z - (zChunkPos * 16));
        }

        public string GetChunkId(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void SetBlockId(string blockId, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        private bool parseRegion(string regionPath, out List<mc_Chunk> chunks, bool log = true)
        {
            chunks = new List<mc_Chunk>();

            if (string.IsNullOrEmpty(regionPath) || !File.Exists(regionPath))
            {
                if (log) _logger?.Error($"Region file {regionPath} does not exist.");
                return false;
            }

            string fileName = Path.GetFileName(regionPath).ToLower();

            if (!fileName.EndsWith(".mca"))
            {
                if (log) _logger?.Error($"{fileName} does not end with .mca");
                return false;
            }

            try
            {
                byte[] data = File.ReadAllBytes(regionPath);

                if (data.Length < 8192)
                {
                    //if (log) _logger?.Error($"[{fileName}] >FAIL< Region file does not contain enough data to support chunk decompression. Found {data.Length} (< 8192) bytes.");
                    return false;
                }

                var sectors = new List<byte[]>();
                while (true)
                {
                    if (data.Length == 0)
                        break;
                    if (data.Length < 4096)
                    {
                        sectors.Add(data);
                        break;
                    }
                    sectors.Add(data[..4096]);
                    data = data[4096..];
                }

                int errorChunks = 0;

                for (int i = 0; i < 4096; i += 4)
                {
                    byte[] bOffset = { 0, 0, 0, 0 };
                    sectors[0][i..(i + 3)].CopyTo(bOffset, 1);
                    bOffset = bOffset.Reverse().ToArray();
                    byte len = sectors[0][i + 3];

                    if (len == 0)
                        continue;

                    int offset = BinaryPrimitives.ReadInt32LittleEndian(bOffset);
                    byte[] compressedData = sectors[offset..(offset + len)].Combine()[7..];
                    var method = (CompressionMethod)sectors[offset][4];

                    bool decompSuccess = false;
                    byte[]? decompChunk = null;
                    switch (method)
                    {
                        case CompressionMethod.ZLIB:
                            decompSuccess = ZLibHelper.Decompress(compressedData, out decompChunk);
                            break;
                        default:
                            if (log) _logger?.Error($"[{fileName}] Chunk#{offset} Unrecognised compression method: {Enum.GetName(method)}");
                            errorChunks++;
                            continue;
                    }

                    if (!decompSuccess || decompChunk == null)
                    {
                        if (log) _logger?.Error($"[{fileName}] Chunk#{offset} Decompression failed.");
                        errorChunks++;
                        continue;
                    }

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
                        if (log) _logger?.Error($"[{fileName}] Chunk#{offset} Could not read NBT tag.");
                        errorChunks++;
                        continue;
                    }

                    var chunk = new mc_Chunk();
                    NbtHelper.FillFromTag(chunk, tag);
                    chunk.NbtData = decompChunk;
                    chunks.Add(chunk);
                }

                _logger?.Info($"[{fileName}] >SUCCESS< Found {chunks.Count()} successful chunks." + (errorChunks > 0 ? $" (and {errorChunks} failed one(s).)" : ""));
                return true;
            }
            catch (Exception e)
            {
                if (log) _logger?.Error(e);
                return false;
            }
        }

        public IChunk? GetChunk(int chunkX, int chunkZ) =>
            Chunks.Values.Combine().FirstOrDefault(c => c.xPos == chunkX && c.zPos == chunkZ);
    }

    public enum CompressionMethod
    {
        GZIP = 0x1,
        ZLIB = 0x2,
        Uncompressed = 0x3
    }
}
