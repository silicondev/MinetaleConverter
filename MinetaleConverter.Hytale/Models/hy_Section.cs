
namespace MinetaleConverter.Hytale.Models
{
    public class hy_Section
    {
        public hy_SectionComponent Components { get; set; } = new hy_SectionComponent();

        public string? GetBlock(int x, int y, int z) =>
            Components.Block.Get(x, y, z);

        public void SetBlock(string blockId, int x, int y, int z) =>
            Components.Block.Set(blockId, x, y, z);
    }
}
