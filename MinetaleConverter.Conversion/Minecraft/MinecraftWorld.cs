using MinetaleConverter.Base;
using MinetaleConverter.Base.Entities;
using MinetaleConverter.Compression;
using SharpNBT;
using System.IO;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class MinecraftWorld
    {
        public byte[] bLocations { get; private set; }
        public byte[] bTimestamps { get; private set; }
        public List<byte[]> bChunkDataNBT { get; private set; } = new List<byte[]>();
        public List<Chunk> Chunks { get; private set; } = new List<Chunk>();
        public List<Chunk> FullChunks => Chunks.Where(x => x.Status == "minecraft:full").ToList();

        public static bool FromFile(string path, out MinecraftWorld? world)
        {
            world = new MinecraftWorld();

            var regionFiles = Directory.GetFiles($@"{path}\region", "*.mca");
            foreach (var regionFile in regionFiles)
            {
                var success = world.ParseRegion(regionFile);
            }

            return true;
        }

        public bool ParseRegion(string regionPath)
        {
            // Sanity check and setup

            if (string.IsNullOrEmpty(regionPath))
                return false;

            if (!regionPath.ToLower().EndsWith(".mca"))
                return false;

            try
            {
                // Binary extraction

                byte[] data = [];

                using (var fs = new FileStream(regionPath, FileMode.Open))
                using (var reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }

                byte[] locations = data[0..4096];
                byte[] timestamps = data[4096..8192];
                byte[] payload = data[8192..];

                // Chunking payload data into 4kb chunks

                var chunks = new List<byte[]>();

                while (true)
                {
                    chunks.Add(payload[0..4096]);
                    if (payload.Length > 4096)
                        payload = payload[4096..];
                    else
                        break;
                }

                // Loop through each location

                for (int i = 0; i < locations.Length; i += 4)
                {
                    // Setup offset and length

                    byte[] bOffset = locations[i..(i + 3)];
                    byte len = locations[i + 3];

                    int offset = swapEndian(bOffset) - 2;

                    if (offset < 0)
                        continue;

                    // Find chunk payload data and decompress

                    byte compType = chunks[offset][4];
                    // Skipping 2 bytes for the data (5 > 7) so it... works?
                    byte[] compressedData = Combine(chunks[offset..(offset + len)])[7..];
                    bool decompSuccess = ZipHelper.Decompress(compressedData, out byte[]? decompChunk, (CompressionMethod)compType);
                    if (!decompSuccess || decompChunk == null)
                        continue;
                    bChunkDataNBT.Add(decompChunk);

                    // Read compound chunk tag

                    CompoundTag? tag;
                    using (var memStream = new MemoryStream(decompChunk))
                    {
                        var tagReader = new TagReader(memStream, FormatOptions.Java);
                        tag = tagReader.ReadTag<CompoundTag>();
                    }

                    if (tag == null)
                        continue;

                    // Fill chunk tag into data structure

                    var chunk = new Chunk();
                    NbtHelper.FillFromTag(chunk, tag);
                    Chunks.Add(chunk);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static int swapEndian(byte[] arr)
        {
            var list = new List<byte>(arr);
            if (arr.Length < 4)
                list.Insert(0, 0);
            return swapEndian(BitConverter.ToInt32(list.ToArray()));
        }

        private static int swapEndian(int input)
        {
            unchecked
            {
                return (int)swapEndian((uint)input);
            }
        }

        private static uint swapEndian(uint input) =>
            ((input & 0x000000ff) << 24) +
            ((input & 0x0000ff00) << 8) +
            ((input & 0x00ff0000) >> 8) +
            ((input & 0xff000000) >> 24);

        private static T[] Combine<T>(IEnumerable<T[]> arr)
        {
            var list = new List<T>();
            foreach (var item in arr)
                list.AddRange(item);
            return list.ToArray();
        }
    }
}
