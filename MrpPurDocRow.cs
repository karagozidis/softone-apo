using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1PluginProject
{
    public sealed class MrpPurDocRow
    {
        public int FINDOC { get; set; }
        public int MTRLINES { get; set; }
        public DateTime TRNDATE { get; set; }
        public int MTRL { get; set; }
        public int COMPANY { get; set; }
        public int WHOUSE { get; set; }

        public decimal QTY1 { get; set; }
        public decimal QTY1CANC { get; set; }
        public decimal QTY1COV { get; set; }
        public decimal QTY1NCOV { get; set; }

        public int SOSOURCE { get; set; }
    }
}