
using MinetaleConverter.Hytale.Logic;

namespace MinetaleConverter.Hytale.Models
{
    public class hy_SectionComponent
    {
        public hy_ChunkSection ChunkSection { get; set; } = new hy_ChunkSection();
        public hy_Binary BlockPhysics { get; set; } = new hy_Binary();
        public hy_Binary Fluid { get; set; } = new hy_Binary();
        public hy_BlockPalette Block { get; set; } = new hy_BlockPalette();
    }
}
