using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1PluginProject
{
    public sealed class MrpSalDocRow
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
        public string PRODUCT { get; set; }
        public int SOSOURCE { get; set; }


        public decimal RunningQty { get; set; }
        public decimal Inv { get; set; }
        public decimal InvRem { get; set; }
        public decimal ReservedPur { get; set; }
        public decimal PurRest { get; set; }
        public decimal IntRest { get; set; }

        public decimal PurInv { get; set; }
        public decimal PurInvRem { get; set; }

        public decimal ReservedOrd { get; set; }

        public decimal ReservedOrdInv { get; set; }
        public decimal ReservedOrdRem { get; set; }
        public string OrdStatus { get; set; }

        public string FinCode { get; set; }
        public DateTime? DelivDate { get; set; }
        public string DistrictName { get; set; }
        public string RouteName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UnitShortcut { get; set; }

        public string CustomerName { get; set; }
        public string MpName { get; set; }
        public string Fason { get; set; }
        public decimal WmsCusQty { get; set; }
        public decimal WmsInAppro { get; set; }
        public string Comments1 { get; set; }

        public string BUNAME { get; set; }

        public decimal StatusPercent { get; set; }

        public decimal TRDR { get; set; }

        public string CCCNPCOMPENTENT { get; set; }



    }

}
