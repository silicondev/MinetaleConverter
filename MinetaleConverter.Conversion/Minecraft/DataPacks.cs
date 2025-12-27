using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Minecraft
{
    public class DataPacks
    {
        public List<string> Enabled { get; internal set; } = new List<string>();
        public List<string> Disabled { get; internal set; } = new List<string>();
    }
}
