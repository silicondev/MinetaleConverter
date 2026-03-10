using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Zstd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Blob
{
    public class IndexedStorageFile
    {
        private string _magicString = "";
        private int _headerLength => _magicString.Length + 12;
        private int _metaLength => _headerLength + (BlobCount * 4);

        public Binary? Binary { get; private set; }
        public int FileVersion { get; private set; } = -1;
        public int BlobCount { get; private set; } = 0;
        public int SegmentSize { get; private set; } = 0;

        public List<int> BlobIndexes { get; private set; } = new List<int>();

        public bool Ready => Binary != null && BlobCount > 0;

        public IndexedStorageFile(string fileName, string magicString = "") : this(magicString)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Could not create an Indexed Storage File: File cannot be found.", fileName);
            Import(File.ReadAllBytes(fileName));
        }

        public IndexedStorageFile(byte[] bytes, string magicString = "") : this(magicString)
        {
            Import(bytes);
        }

        public IndexedStorageFile(string magicString = "")
        {
            _magicString = magicString;
        }

        public bool Import(byte[] bytes, bool includeInvalidIndexes = false)
        {
            Binary = new Binary(bytes, EndianMode.Big);
            Binary.Seek = 0;
            byte[] header = Binary.Subset(_headerLength);
            if (header.Length < _headerLength)
                return false;
            var headerBin = new Binary(header);
            string magic = headerBin.ReadLength<string>(_magicString.Length);
            if (magic != _magicString)
                return false;
            FileVersion = headerBin.Read<int>();
            if (FileVersion < 0 || FileVersion > 1)
                return false;
            BlobCount = headerBin.Read<int>();
            SegmentSize = headerBin.Read<int>();

            var indexesBin = new Binary(Binary.Subset(BlobCount * 4));
            for (int i = 0; i < BlobCount; i++)
            {
                int index = indexesBin.Read<int>();
                if (index != 0 || includeInvalidIndexes)
                    BlobIndexes.Add(index);
            }

            return Ready;
        }

        public byte[] ReadBlob(int index)
        {
            if (!Ready)
                throw new Exception("File has not been correctly imported.");

            if (index <= 0)
                throw new ArgumentOutOfRangeException();

            int segIndex = getSegmentIndex(index);
            Binary!.Seek = segIndex;
            int srcLength = Binary.Read<int>();
            int compLength = Binary.Read<int>();
            byte[] compressedData = Binary.Subset(compLength, false);
            if (compressedData.Length != compLength)
                throw new Exception("Compressed data out of bounds of file.");
            byte[] decompData = ZstdHelper.Decompress(compressedData, srcLength);
            return decompData;
        }

        private int getSegmentIndex(int index) => ((index - 1) * SegmentSize) + _metaLength;
    }
}
