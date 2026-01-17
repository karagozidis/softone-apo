using Softone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1PluginProject
{
    public static class MrpCalculator
    {
        static public XSupport XSupport;

        public static List<MrpSalDocRow> LoadAll()
        {
            var salList = LoadSalDoc();
            var balList = LoadBalStart();


            var grouped = salList
                .GroupBy(x => x.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.FINDOC)
                          .ThenBy(x => x.MTRLINES)
                          .ToList()
                );

            foreach (var kv in grouped)
            {
                int mtrl = kv.Key;
                var rows = kv.Value;

                decimal startBal = balList.FirstOrDefault(b => b.MTRL == mtrl)?.AVBAL ?? 0m;
                decimal cumulative = startBal;

                foreach (var r in rows)
                {
                    cumulative -= r.QTY1NCOV;
                    r.RunningQty = cumulative;
                }
            }


            Parallel.ForEach(salList, r =>
            {
                decimal qty = r.QTY1NCOV;
                decimal run = r.RunningQty;

                if (run >= 0)
                    r.Inv = qty;
                else if ((qty + run) > 0)
                    r.Inv = qty + run;
                else
                    r.Inv = 0;
            });


            ApplyPurchaseInfo(salList);

            ApplyPurchaseInvUsage(salList);

            ApplyOrderReservation(salList);

            ApplyOrderInvUsage(salList);

            ApplyOrdStatus(salList);

            ApplyStatusPercent(salList);

            foreach (var r in salList)
            {
                r.QTY1NCOV = Math.Round(r.QTY1NCOV, 3);
                r.Inv = Math.Round(r.Inv, 3);
                r.InvRem = Math.Round(r.InvRem, 3);
                r.RunningQty = Math.Round(r.RunningQty, 3);
                r.PurRest = Math.Round(r.PurRest, 3);
                r.IntRest = Math.Round(r.IntRest, 3);
                r.ReservedPur = Math.Round(r.ReservedPur, 3);
                r.PurInv = Math.Round(r.PurInv, 3);
                r.PurInvRem = Math.Round(r.PurInvRem, 3);
                r.ReservedOrd = Math.Round(r.ReservedOrd, 3);
                r.ReservedOrdInv = Math.Round(r.ReservedOrdInv, 3);
                r.ReservedOrdRem = Math.Round(r.ReservedOrdRem, 3);
            }


            return salList;
        }

        private static List<MrpSalDocRow> LoadSalDoc()
        {
            XTable table = XSupport.GetSQLDataSet("WITH WMSCUS AS ( " +
                                                "     SELECT      AW.MTRL, " +
                                                "                       AW.CUSCODE," +
                                                "                       SUM(AW.TONOIFREE - AW.TONOIDESTROY) AS WMSCUSQTY " +
                                                "     FROM APO_SynolikoApothema_WCUSCODE AW    " +
                                                "     GROUP BY AW.MTRL, AW.CUSCODE), " +
                                                " WMSAPPRO AS (  " +
                                                "     SELECT   CK.MTRL,    " +
                                                "     ISNULL((SELECT TOP 1 ISNULL(CK2.TONOIDESTROY,0) FROM APO_SynolikoApothema CK2 WHERE CK2.PRD_ID = CK.PRD_ID ),0) AS WMSINAPPRO    " +
                                                "     FROM APO_SynolikoApothema CK   " +
                                                "     WHERE ISNULL(CK.Apothetis, '') = N'Αγχίαλος ' " +
                                                "    GROUP BY CK.MTRL,CK.PRD_ID) " +
                                                " SELECT    MS.FINDOC,    " +
                                                " MS.FINCODE, " +
                                                " MT.DELIVDATE, " +
                                                " DS.NAME AS DISTRICTNAME, " +
                                                " CR.NAME AS ROUTENAME, " +
                                                " MS.MTRLINES,   " +
                                                " MS.TRNDATE,    " +
                                                " MS.MTRL,  " +
                                                " M.CODE AS ITEMCODE , " +
                                                " M.NAME AS ITEMNAME, " +
                                                " MRU.SHORTCUT,   " +
                                                " MS.COMPANY,    " +
                                                " MS.WHOUSE,    " +
                                                " MS.QTY1,  " +
                                                " MS.QTY1CANC,   " +
                                                " MS.QTY1COV,  " +
                                                " MS.QTY1NCOV,    " +
                                                " MS.PRODUCT,    " +
                                                " MS.BUSUNITS,    " +
                                                " ISNULL(T.TRDR,0) AS TRDR, " +
                                                " MS.SOSOURCE,    " +
                                                " ISNULL(F.CCCNPCOMPENTENT,'') AS CCCNPCOMPENTENT, " +
                                                " BU.NAME AS BUNAME,    " +
                                                " T.NAME AS CUSTOMERNAME, " +
                                                " (SELECT ISNULL(TT.NAME,'') FROM TRDR TT WHERE TT.TRDR = T.CCCNPGROUPTRDR) AS MP,  " +
                                                " CASE WHEN F.CCCNPFASONACT=1 THEN 'ΝΑΙ' ELSE 'ΟΧΙ' END AS FASON,  " +
                                                " ISNULL(WMSCUS.WMSCUSQTY, 0) AS WMSCUSQTY,   " +
                                                " ISNULL(WMSAPPRO.WMSINAPPRO, 0) AS WMSINAPPRO, " +
                                                " F.COMMENTS1 FROM MRPSALDOC MS " +
                                                " LEFT JOIN FINDOC F ON F.FINDOC = MS.FINDOC " +
                                                " LEFT JOIN MTRDOC MT ON MT.FINDOC =F.FINDOC " +
                                                " LEFT JOIN CCCNPROUTER CR ON CR.CCCNPROUTER = F.CCCNPROUTER  " +
                                                " LEFT JOIN DISTRICT DS ON DS.DISTRICT = MT.DISTRICT " +
                                                " LEFT JOIN TRDR T ON T.TRDR = F.TRDR " +
                                                " INNER JOIN MTRL M ON M.MTRL = MS.MTRL " +
                                                " LEFT JOIN MTRUNIT MRU ON MRU.MTRUNIT = M.MTRUNIT1  AND MRU.COMPANY = M.COMPANY " +
                                                " LEFT JOIN BUSUNITS BU ON BU.BUSUNITS = M.BUSUNITS AND BU.COMPANY = M.COMPANY " +
                                                " LEFT JOIN WMSCUS ON    WMSCUS.MTRL = MS.MTRL    AND WMSCUS.CUSCODE COLLATE Greek_CI_AI = T.CODE COLLATE Greek_CI_AI " +
                                                " LEFT JOIN WMSAPPRO ON WMSAPPRO.MTRL = MS.MTRL  ORDER BY MS.FINDOC,MS.MTRLINES", null);
            var list = new List<MrpSalDocRow>(table.Count);


            for (int i = 0; i < table.Count; i++)
            {
                list.Add(new MrpSalDocRow
                {
                    FINDOC = Convert.ToInt32(table[i, "FINDOC"]),
                    MTRLINES = Convert.ToInt32(table[i, "MTRLINES"]),
                    TRNDATE = Convert.ToDateTime(table[i, "TRNDATE"]),
                    MTRL = Convert.ToInt32(table[i, "MTRL"]),
                    COMPANY = Convert.ToInt32(table[i, "COMPANY"]),
                    WHOUSE = Convert.ToInt32(table[i, "WHOUSE"]),
                    QTY1 = Convert.ToDecimal(table[i, "QTY1"]),
                    QTY1CANC = Convert.ToDecimal(table[i, "QTY1CANC"]),
                    QTY1COV = Convert.ToDecimal(table[i, "QTY1COV"]),
                    QTY1NCOV = Convert.ToDecimal(table[i, "QTY1NCOV"]),
                    PRODUCT = table[i, "PRODUCT"].ToString(),
                    SOSOURCE = Convert.ToInt32(table[i, "SOSOURCE"]),

                    FinCode = table[i, "FINCODE"].ToString(),
                    DelivDate = table[i, "DELIVDATE"] is DBNull ? (DateTime?)null : Convert.ToDateTime(table[i, "DELIVDATE"]),
                    DistrictName = table[i, "DISTRICTNAME"].ToString(),
                    RouteName = table[i, "ROUTENAME"].ToString(),
                    ItemCode = table[i, "ITEMCODE"].ToString(),
                    ItemName = table[i, "ITEMNAME"].ToString(),
                    UnitShortcut = table[i, "SHORTCUT"].ToString(),

                    CustomerName = table[i, "CUSTOMERNAME"].ToString(),
                    MpName = table[i, "MP"].ToString(),
                    Fason = table[i, "FASON"].ToString(),
                    BUNAME = table[i, "BUNAME"].ToString(),
                    WmsCusQty = Convert.ToDecimal(table[i, "WMSCUSQTY"]),
                    WmsInAppro = Convert.ToDecimal(table[i, "WMSINAPPRO"]),
                    Comments1 = table[i, "COMMENTS1"].ToString(),
                    TRDR = Convert.ToInt32(table[i, "TRDR"]),
                    CCCNPCOMPENTENT = table[i, "CCCNPCOMPENTENT"].ToString()
                });
            }


            return list;
        }

        private static List<MrpBalStartRow> LoadBalStart()
        {
            XTable table = XSupport.GetSQLDataSet("SELECT * FROM MRPBALSTART", null);
            var list = new List<MrpBalStartRow>(table.Count);


            for (int i = 0; i < table.Count; i++)
            {
                list.Add(new MrpBalStartRow
                {
                    MTRL = Convert.ToInt32(table[i, "MTRL"]),
                    WMS = Convert.ToDecimal(table[i, "WMS"]),
                    ERPCMD0 = Convert.ToDecimal(table[i, "ERPCMD0"]),
                    AVBAL = Convert.ToDecimal(table[i, "AVBAL"])
                });
            }


            return list;
        }

        private static List<MrpPurDocRow> LoadPurDoc()
        {
            // Παίρνω τις γραμμές από το view MRPPURDOC
            XTable table = XSupport.GetSQLDataSet("SELECT * FROM MRPPURDOC", null);
            var list = new List<MrpPurDocRow>(table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                list.Add(new MrpPurDocRow
                {
                    FINDOC = Convert.ToInt32(table[i, "FINDOC"]),
                    MTRLINES = Convert.ToInt32(table[i, "MTRLINES"]),
                    TRNDATE = Convert.ToDateTime(table[i, "TRNDATE"]),
                    MTRL = Convert.ToInt32(table[i, "MTRL"]),
                    COMPANY = Convert.ToInt32(table[i, "COMPANY"]),
                    WHOUSE = Convert.ToInt32(table[i, "WHOUSE"]),

                    QTY1 = Convert.ToDecimal(table[i, "QTY1"]),
                    QTY1CANC = Convert.ToDecimal(table[i, "QTY1CANC"]),
                    QTY1COV = Convert.ToDecimal(table[i, "QTY1COV"]),
                    QTY1NCOV = Convert.ToDecimal(table[i, "QTY1NCOV"]),

                    SOSOURCE = Convert.ToInt32(table[i, "SOSOURCE"])
                });
            }

            return list;
        }


        private static void ApplyPurchaseInfo(List<MrpSalDocRow> salList)
        {

            var purList = LoadPurDoc();

            var totalPurByMtrl = purList
                .GroupBy(p => p.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.QTY1NCOV)
                );

            // 3) υπολογίζω το PURREST , δλδ την ποσότητα που περιμένουμε απο προμηθευτή
            var purRestByMtrl = purList
                .Where(p => p.SOSOURCE != 1151)
                .GroupBy(p => p.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.QTY1NCOV)
                );

            // 4) υπολογίζω το INTREST , δλδ την ποσότητα που περιμένουμε απο ενδοδιακίνηση 
            var intRestByMtrl = purList
                .Where(p => p.SOSOURCE == 1151)
                .GroupBy(p => p.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.QTY1NCOV)
                );

            // 5) Για να κάνουμε το window SUM (OVER PARTITION BY MTRL ORDER BY FINDOC,MTRLINES)
            //    ξαναομαδοποιούμε τις πωλήσεις ανά MTRL
            var grouped = salList
                .GroupBy(x => x.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.FINDOC)
                          .ThenBy(x => x.MTRLINES)
                          .ToList()
                );

            foreach (var kv in grouped)
            {
                int mtrl = kv.Key;
                var rows = kv.Value;

                // sum αγορών και ενδοδιακινήσεων για καθε MTRL
                decimal totalPur = totalPurByMtrl.TryGetValue(mtrl, out var t) ? t : 0m;

                // για το προοδευτικο αποθεμα
                decimal runningInvRemSum = 0m;

                foreach (var r in rows)
                {

                    r.InvRem = r.QTY1NCOV - r.Inv;

                    runningInvRemSum += r.InvRem;


                    r.ReservedPur = totalPur - runningInvRemSum;


                    r.PurRest = purRestByMtrl.TryGetValue(mtrl, out var pr) ? pr : 0m;
                    r.IntRest = intRestByMtrl.TryGetValue(mtrl, out var ir) ? ir : 0m;
                }
            }
        }

        //αυτο υπολογίζει πόση ποσότητα απο αυτά που περιμένω μπορώ να χρησιμοποιήσω και τι μένει ακόμα ακάλυπτο ακόμα και μετά την χρήση και των αναμενόμενων 
        private static void ApplyPurchaseInvUsage(List<MrpSalDocRow> salList)
        {
            foreach (var r in salList)
            {
                decimal invRem = r.InvRem;
                decimal reservedPur = r.ReservedPur;

                decimal purInv;

                if (invRem <= 0)
                {
                    purInv = 0m;
                }
                else if (invRem > 0 && reservedPur >= 0)
                {
                    purInv = invRem;
                }
                else if (invRem > 0 && reservedPur < 0 && (invRem + reservedPur) >= 0)
                {
                    purInv = invRem + reservedPur;
                }
                else
                {
                    purInv = 0m;
                }

                r.PurInv = purInv;
                r.PurInvRem = invRem - purInv;
            }
        }


        private static List<MrpPurDocOrdRow> LoadPurDocOrd()
        {


            XTable table = XSupport.GetSQLDataSet("SELECT MTRL, QTY1NCOV AS QTY FROM MRPPURDOCORD", null);

            var list = new List<MrpPurDocOrdRow>(table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                list.Add(new MrpPurDocOrdRow
                {
                    MTRL = Convert.ToInt32(table[i, "MTRL"]),
                    QTY = Convert.ToDecimal(table[i, "QTY"])
                });
            }

            return list;
        }

        private static void ApplyOrderReservation(List<MrpSalDocRow> salList)
        {

            var ordList = LoadPurDocOrd();


            var totalOrdByMtrl = ordList
                .GroupBy(o => o.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.QTY)
                );


            var grouped = salList
                .GroupBy(x => x.MTRL)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.FINDOC)
                          .ThenBy(x => x.MTRLINES)
                          .ToList()
                );

            foreach (var kv in grouped)
            {
                int mtrl = kv.Key;
                var rows = kv.Value;

                decimal totalOrd = totalOrdByMtrl.TryGetValue(mtrl, out var t) ? t : 0m;
                decimal runningPurInvRemSum = 0m;

                foreach (var r in rows)
                {

                    runningPurInvRemSum += r.PurInvRem;


                    r.ReservedOrd = totalOrd - runningPurInvRemSum;
                }
            }
        }


        private static void ApplyOrderInvUsage(List<MrpSalDocRow> salList)
        {
            foreach (var r in salList)
            {
                decimal purInvRem = r.PurInvRem;
                decimal reservedOrd = r.ReservedOrd;

                decimal reservedOrdInv;

                if (purInvRem <= 0)
                {
                    reservedOrdInv = 0m;
                }
                else if (purInvRem > 0 && reservedOrd >= 0)
                {
                    reservedOrdInv = purInvRem;
                }
                else if (purInvRem > 0 && reservedOrd < 0 && (purInvRem + reservedOrd) >= 0)
                {
                    reservedOrdInv = purInvRem + reservedOrd;
                }
                else
                {
                    reservedOrdInv = 0m;
                }

                r.ReservedOrdInv = reservedOrdInv;
                r.ReservedOrdRem = purInvRem - reservedOrdInv;
            }
        }


        private static Dictionary<int, int> LoadMtrlTypes()
        {
            XTable table = XSupport.GetSQLDataSet(
                "SELECT MTRL, MTRTYPE1 " +
                "FROM MTRL " +
                "WHERE SODTYPE = 51 AND COMPANY = 1001 AND ISACTIVE = 1",
                null
            );

            var dict = new Dictionary<int, int>(table.Count);

            for (int i = 0; i < table.Count; i++)
            {
                int mtrl = Convert.ToInt32(table[i, "MTRL"]);
                int type = Convert.ToInt32(table[i, "MTRTYPE1"]);
                dict[mtrl] = type;
            }

            return dict;
        }

        private static void ApplyOrdStatus(List<MrpSalDocRow> salList)
        {
            var mtrlTypes = LoadMtrlTypes();

            foreach (var r in salList)
            {
                int mtrType1 = 0;

                if (!mtrlTypes.TryGetValue(r.MTRL, out mtrType1))
                {

                    mtrType1 = 0;
                }

                if (r.RunningQty >= 0 && r.SOSOURCE == 1351)
                {
                    r.OrdStatus = "Διαθέσιμο προς δρομολόγηση";
                }
                else if (r.RunningQty < 0 && mtrType1 != 1)
                {
                    r.OrdStatus = "Προς Αγορά";
                }
                else if (r.RunningQty >= 0 && r.SOSOURCE == 1151)
                {
                    r.OrdStatus = "Διαθέσιμο προς δρομολόγηση";
                }

                else if (r.RunningQty >= 0 && r.SOSOURCE == 1171)
                {
                    r.OrdStatus = "Διαθέσιμο για παραγωγή";
                }
                else if (r.RunningQty < 0 && mtrType1 == 1)
                {
                    r.OrdStatus = "Προς Παραγωγή";
                }
                else
                {
                    r.OrdStatus = "joker";
                }
            }
        }


        private static void ApplyStatusPercent(List<MrpSalDocRow> rows)
        {
            foreach (var r in rows)
            {
                if (r.RunningQty >= 0)
                {
                    r.StatusPercent = 100m;
                }
                else if (r.QTY1NCOV > 0)
                {
                    decimal pct = (r.Inv / r.QTY1NCOV) * 100m;

                    if (pct < 0m) pct = 0m;
                    if (pct > 100m) pct = 100m;

                    r.StatusPercent = Math.Round(pct, 2);
                }
                else
                {

                    r.StatusPercent = 0m;
                }
            }
        }




    }
}
