
using MinetaleConverter.Base.Logic.Extensions;

namespace MinetaleConverter.Base.Logic.Compression.Palette
{
    public abstract class DataPalette<TIndex, TDataType, TPalette> where TIndex : notnull
    {
        protected TDataType[] _data = new TDataType[0];
        public virtual TDataType[] Data
        {
            get => _data;
            set
            {
                _data = value;
                populate();
            }
        }
        public Dictionary<TIndex, TPalette> PaletteList = new Dictionary<TIndex, TPalette>();
        public abstract TPalette DefaultPalette { get; }
        public abstract TPalette ErrorPalette { get; }
        public abstract Func<int, int, int, int> Indexer { get; }
        public abstract int Length { get; protected set; }
        public bool IsEmpty => PaletteList.Count == 0;
        protected abstract void populate(bool rebuild = true);
        protected abstract void Reconstruct(TPalette[] list);
        public abstract TPalette[] Decompress(TDataType[] arr);
        public abstract TPalette? GetAtIndex(int index);
        public abstract void SetAtIndex(int index, TPalette value);
        public TPalette Get(int x, int y, int z)
        {
            if (IsEmpty)
                return DefaultPalette;

            int paletteCount = PaletteList.Count();
            if (paletteCount == 1)
                return PaletteList.First().Value;

            if (Data == null || Data.Length == 0)
                return DefaultPalette;

            int index = Indexer(x, y, z);
            return GetAtIndex(index) ?? ErrorPalette;
        }

        public void Set(TPalette value, int x, int y, int z)
        {
            int index = Indexer(x, y, z);

            if (Data == null || Data.Length == 0 || PaletteList == null || PaletteList.Count == 0 || !PaletteList.ContainsValue(value))
            {
                var list = DefaultPalette.Stretch(Length);
                list[index] = value;
                Build(list);
            }
            else
                SetAtIndex(index, value);
        }

        

        public void Build(TPalette[] list, bool repopulate = true)
        {
            Reconstruct(list);

            if (repopulate)
                populate(false);
        }
    }
}
