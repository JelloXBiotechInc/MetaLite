using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLite_Viewer.Model
{
    public class Flags
    {
        public const int SIZE_OF_FLAGS = 1;

        public const int AI_ANNOTATION_UPDATE = 0;

        public const int SHOW_AI_ANNOTATION = 0;

        public List<bool> Show { get; private set; }
        public List<bool> Dirty { get; private set; }

        public Flags(List<bool> show = null, List<bool> dirty = null)
        {
            Show = show == null ? Enumerable.Repeat(true, SIZE_OF_FLAGS).ToList() : show;
            Dirty = dirty == null ? Enumerable.Repeat(false, SIZE_OF_FLAGS).ToList() : dirty;
        }
    }
}
