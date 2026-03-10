using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_Section
    {
        //public int Id { get; set; }
        public hy_SectionComponent Components { get; set; } = new hy_SectionComponent();

        public string? GetBlock(int x, int y, int z) =>
            Components.Block.Get(x, y, z);

        public void SetBlock(string blockId, int x, int y, int z) =>
            Components.Block.Set(blockId, x, y, z);
    }
}
