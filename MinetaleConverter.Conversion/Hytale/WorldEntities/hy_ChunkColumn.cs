using MinetaleConverter.Conversion.Hytale.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.WorldEntities
{
    public class hy_ChunkColumn
    {
        public IList<hy_Section> Sections
        {
            get => _sections;
            set => SetSections(value);
        }
        private IList<hy_Section> _sections = new List<hy_Section>();
        private void SetSections(IList<hy_Section> list)
        {
            _sections = list;
            for (int i = 0; i < _sections.Count; i++)
                _sections[i].Id = i;
        }
    }
}
