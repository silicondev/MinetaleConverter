using MinetaleConverter.Base;
using MinetaleConverter.Base.Compression.Palette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_BlockChunk
    {
        public byte[] Data
        {
            set
            {
                char[] chars = value.Select(x => Encoding.ASCII.GetString([x])[0]).ToArray();
                var bin = new Binary(value);
                // This data currently makes no sense. More investigation is required.
            }
        }
    }
}
