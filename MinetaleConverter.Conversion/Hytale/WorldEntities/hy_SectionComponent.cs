using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Palette;
using MinetaleConverter.Base.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_SectionComponent
    {
        //public hy_Binary Block
        //{
        //    get
        //    {
        //        var blockObj = new hy_Binary();
        //        blockObj.Version = Version;

        //        var bin = new Binary();
        //        // Migration version
        //        bin.Write(10.SwapEndian());
        //        var paletteType = HytalePaletteType.Full;
        //        short paletteCount = (short)BlockPalette.PalettesDict.Count();
        //        int bitCount = (int)Math.Ceiling(Math.Log2(paletteCount));
        //        if (bitCount > 8)
        //            paletteType = HytalePaletteType.Short;
        //        else if (bitCount <= 4)
        //            paletteType = HytalePaletteType.Half;
        //        if (paletteCount == 0)
        //            paletteType = HytalePaletteType.Empty;
        //        bin.Write((byte)paletteType);
        //        if (paletteType == HytalePaletteType.Empty)
        //        {
        //            blockObj.Data = bin.Bytes.ToArray();
        //            return null;
        //        }
        //        bin.Write(paletteCount.SwapEndian());
        //        foreach (var kvp in BlockPalette.PalettesDict)
        //        {
        //            switch (paletteType)
        //            {
        //                case HytalePaletteType.Full:
        //                case HytalePaletteType.Half:
        //                    bin.Write((byte)kvp.Key);
        //                    break;
        //                case HytalePaletteType.Short:
        //                    bin.Write(((short)kvp.Key).SwapEndian());
        //                    break;
        //            }
        //            bin.Write(((ushort)kvp.Value.Length).SwapEndian());
        //            bin.Write(kvp.Value);
        //            // ADD TO THIS
        //            bin.Write(((short)0).SwapEndian());
        //        }
        //        bin.Write(BlockPalette.Data);
        //        blockObj.Data = bin.Bytes.ToArray();
        //        return blockObj;
        //    }
        //    set
        //    {
        //        if (value.Version != null)
        //            Version = value.Version.Value;

        //        var bytes = value.Data;
        //        char[] chars = bytes.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
        //        var bin = new Binary(bytes);
        //        int migrationVer = bin.Read<int>().SwapEndian();
        //        var paletteType = (HytalePaletteType)bin.Read<byte>();

        //        if (paletteType == HytalePaletteType.Empty)
        //            return;

        //        int blockCount = 32 * 32 * 32;
        //        int minBits = 8;
        //        switch (paletteType)
        //        {
        //            case HytalePaletteType.Half:
        //                blockCount /= 2;
        //                minBits = 4;
        //                break;
        //            case HytalePaletteType.Short:
        //                blockCount *= 2;
        //                minBits = 16;
        //                break;
        //        }

        //        BlockPalette = new BytePalette<string>(minBits);

        //        short paletteCount = bin.Read<short>().SwapEndian();
        //        var palettes = new Dictionary<int, string>();
        //        for (int i = 0; i < paletteCount; i++)
        //        {
        //            short key = paletteType == HytalePaletteType.Short ? bin.Read<short>().SwapEndian() : bin.Read<byte>();
        //            //string name = bin.ReadGivenLength<string, ushort>((x) => x.SwapEndian());
        //            ushort nameLen = bin.Read<ushort>().SwapEndian();
        //            string name = bin.ReadLength<string>(nameLen);
        //            short count = bin.Read<short>().SwapEndian();
        //            palettes[key] = name;
        //        }
        //        BlockPalette.PalettesDict = palettes;
        //        bin.Cut();
        //        bin = new Binary(bin.Subset(blockCount));
        //        BlockPalette.Data = bin.Bytes.ToArray();
        //        chars = BlockPalette.Data.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
        //        var list = BlockPalette.Decompress(blockCount);
        //    }
        //}

        //[JsonIgnore]
        //public int Version { get; set; }
        //[JsonIgnore]
        //public BytePalette<string> BlockPalette { get; set; } = new BytePalette<string>();

        public hy_ChunkSection ChunkSection { get; set; } = new hy_ChunkSection();
        //[JsonConverter(typeof(BsonOptionalClass<hy_Binary>))]
        public hy_Binary BlockPhysics { get; set; } = new hy_Binary();
        public hy_Binary Fluid { get; set; } = new hy_Binary();
        public hy_Palette Block { get; set; } = new hy_Palette();
    }
}
