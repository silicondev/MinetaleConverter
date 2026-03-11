using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Minecraft.Models
{
    public class mc_Resource
    {
        public string Name { get; internal set; }
        public Dictionary<string, string> Properties { get; internal set; } = new Dictionary<string, string>();
    }
}
