using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using MinetaleConverter.Base;
using MinetaleConverter.Base.Logging;
using MinetaleConverter.Base.Blob;
using MinetaleConverter.Base.Bson;
using MinetaleConverter.Conversion.Hytale.WorldEntities;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using MinetaleConverter.Conversion.Interfaces;

namespace MinetaleConverter.Conversion.Hytale
{
    public class hy_World : IWorld
    {
        private ILogger _logger;
        public List<IChunk> Chunks { get; private set; } = new List<IChunk>();

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
            string[] fileNameParts = Path.GetFileName(regionPath).Split('.');
            int regionX = int.Parse(fileNameParts[0]);
            int regionY = int.Parse(fileNameParts[1]);
            var file = new IndexedStorageFile("HytaleIndexedStorage");
            file.Import(File.ReadAllBytes(regionPath));
            var chunks = new List<hy_Chunk>();

            foreach (var index in file.BlobIndexes)
            {
                try
                {
                    (int chunkX, int chunkY) = getChunkCoordinates(index, regionX, regionY);
                    byte[] chunkData = file.ReadBlob(index);
                    var bsonFile = new BsonFile(chunkData);
                    chunks.Add(new hy_Chunk(bsonFile));
                }
                catch (Exception e) { }
            }

            return chunks;
        }

        private (int chunkX, int chunkY) getChunkCoordinates(int blobIndex, int regionX, int regionY)
        {
            int localX = blobIndex % 32;
            int localY = blobIndex / 32;
            int chunkX = regionX << 5 | localX;
            int chunkY = regionY << 5 | localY;
            return (chunkX, chunkY);
        }

        public string GetBlockId(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public string GetBiomeId(int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}
