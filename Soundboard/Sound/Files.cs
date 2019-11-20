using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundboard.Sound
{
    public class Files:EventArgs
    {
        public string NameSound { get; set; }
        public string InputKey { get; set; }
        public string FileLocation { get; set; }
    }
}
