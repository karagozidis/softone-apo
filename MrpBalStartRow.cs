using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1PluginProject
{
    public sealed class MrpBalStartRow
    {
        public int MTRL { get; set; }
        public decimal WMS { get; set; }
        public decimal ERPCMD0 { get; set; }
        public decimal AVBAL { get; set; }
    }
}
