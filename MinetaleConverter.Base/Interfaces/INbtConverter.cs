using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Interfaces
{
    public interface INbtConverter
    {
        object? Convert(string value);
    }
}
