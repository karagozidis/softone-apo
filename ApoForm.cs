using Newtonsoft.Json;
using Softone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using System.Reflection;


namespace S1PluginProject
{
    public partial class ApoForm : Form
    {
        static public XSupport XSupport;
        static public XTable FindocSosource;
        public class ClientMessage
        {
            public string action { get; set; }
            public int findoc { get; set; }

            public int trdr { get; set; }

            public int mtrl { get; set; }
            

        }
        public class GraphRowDto
        {
            public int findoc { get; set; }
            public string finCode { get; set; }
            public decimal qtyNeed { get; set; }
            public decimal runningQty { get; set; }
        }

        public class GraphItemMessage
        {
            public string action { get; set; }
            public int mtrl { get; set; }
            public string itemCode { get; set; }
            public string itemName { get; set; }
            public List<GraphRowDto> rows { get; set; }
        }

        public ApoForm()
        {
            InitializeComponent();

            // MessageBox.Show(cmd); 

          


            /*            List<MrpSalDocRow> list = MrpCalculator.LoadAll();
                        string json = JsonConvert.SerializeObject(list);


                        string htmlPath = Path.Combine(Application.StartupPath, "mrp.html");
                        webView21.CoreWebView2.Navigate(htmlPath);
                        webView21.CoreWebView2.PostWebMessageAsString(json);

                        //  MessageBox.Show("Executed");*/
        }



        private async void Form2_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.Settings.IsWebMessageEnabled = true;

            webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            string mrpPath = ExtractEmbeddedFileToDisk(
                "S1PluginProject.mrp.html", // ⚠️ άλλαξέ το αν το namespace/folder διαφέρει
                "mrp.html"
            );

            webView21.CoreWebView2.Navigate(new Uri(mrpPath).AbsoluteUri);

            // string htmlPath = Path.Combine(Application.StartupPath, "mrp.html");

            // 1) Πρώτα κάνουμε navigate
            //  webView21.CoreWebView2.Navigate(htmlPath);

            // 2) Μετά περιμένουμε να ολοκληρωθεί η φόρτωση
            webView21.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
        }

        private async void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {


            SendMrpDataToWeb();


           // var json = await Task.Run(() =>
           // {
           //     var list = MrpCalculator.LoadAll();
           //     return Newtonsoft.Json.JsonConvert.SerializeObject(list,
           //         new Newtonsoft.Json.JsonSerializerSettings
           //         {
           //             DateFormatString = "yyyy-MM-dd"
           //         });
           // });

           // // 2) Στέλνουμε το μήνυμα στην WebView2 ΧΩΡΙΣ καν να μπλοκάρουμε το UI thread
           // this.BeginInvoke(new Action(() =>
           //{
           //    webView21.CoreWebView2.PostWebMessageAsString(json);
           //}));


            // 3) ΤΩΡΑ και μόνο τώρα στέλνουμε τα δεδομένα
            //var list = MrpCalculator.LoadAll();
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(list,
            //    new Newtonsoft.Json.JsonSerializerSettings
            //    {
            //        DateFormatString = "yyyy-MM-dd"
            //    });

            //webView21.CoreWebView2.PostWebMessageAsString(json);
        }


        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            //try
            //{
            //    // Παίρνουμε το JSON που έστειλε το JavaScript
            //    var json = e.WebMessageAsJson;

            //    var msg = JsonConvert.DeserializeObject<ClientMessage>(json);
            //    if (msg == null) return;

            //    if (msg.action == "view-doc")
            //    {
            //        int findoc = msg.findoc;



            //      //  MessageBox.Show($"Προβολή παραστατικού FINDOC = {findoc}");


            //        string cmd = $"SALDOC[FORM='Agrohellas Pro Lev1',AUTOLOCATE={findoc}]";

            //       // MessageBox.Show(cmd); 

            //        Form2.XSupport.ExecS1Command(cmd, null);
            //      //  Form2.XModule.Exec("XCMD:SOCALL[STYLE=MODELESS]", null);


            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("WebMessage error: " + ex.Message);
            //}

            try
            {
                var json = e.WebMessageAsJson;
                var msg = JsonConvert.DeserializeObject<ClientMessage>(json);

                var baseMsg = JsonConvert.DeserializeObject<dynamic>(json);
                string action = baseMsg?.action;
                if (string.IsNullOrWhiteSpace(action)) return;


                if (msg == null) return;

                if (msg.action == "view-doc")
                {
                    int findoc = msg.findoc;
                    
                    int sosource = 0;
                    XTable FindocSosource = XSupport.GetSQLDataSet($"SELECT SOSOURCE FROM FINDOC  WHERE FINDOC ={findoc}", null);
                    // Χτίζουμε την εντολή XCMD

                    sosource = Convert.ToInt32(FindocSosource[0, "SOSOURCE"]);

                    if (sosource == 1351)
                    {
                        string cmd = $"SALDOC[FORM='Agrohellas Lev0',AUTOLOCATE={findoc}]";

                       
                        // ΤΗΝ ΕΚΤΕΛΟΥΜΕ ΣΤΟ UI THREAD
                        this.BeginInvoke(new Action(() =>
                        {
                           
                            ApoForm.XSupport.ExecS1Command(cmd, null);
                        }));
                    }

                    if (sosource == 1171)
                    {
                        string cmd = $"PRDORDDOC[FORM='Agrohellas Pro',AUTOLOCATE={findoc}]";

                        // ΤΗΝ ΕΚΤΕΛΟΥΜΕ ΣΤΟ UI THREAD
                        this.BeginInvoke(new Action(() =>
                        {
                            ApoForm.XSupport.ExecS1Command(cmd, null);
                        }));
                    }
        
                    if (sosource == 1151)
                    {
                        string cmd = $"ITEDOC[FORM='Agrohellas',AUTOLOCATE={findoc}]";

                        // ΤΗΝ ΕΚΤΕΛΟΥΜΕ ΣΤΟ UI THREAD
                        this.BeginInvoke(new Action(() =>
                        {
                            ApoForm.XSupport.ExecS1Command(cmd, null);
                        }));
                    }

                }
                if (msg.action == "view-graph-item")
                {

                    var gmsg = JsonConvert.DeserializeObject<GraphItemMessage>(json);
                    if (gmsg == null) return;

                    this.BeginInvoke(new Action(() =>
                    {
                        // Στέλνουμε το json στο GraphForm για να το περάσει στο graph.html
                        GraphForm f = new GraphForm(json);
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.ShowDialog();
                    }));

                    return;
                }


                if (msg.action == "view-customer")
                {
                    int trdr = msg.trdr;
                    string cmd = $"CUSTOMER[FORM='Agrohellas Lev1',AUTOLOCATE={trdr}]";

                    // ΤΗΝ ΕΚΤΕΛΟΥΜΕ ΣΤΟ UI THREAD
                    this.BeginInvoke(new Action(() =>
                    {
                        ApoForm.XSupport.ExecS1Command(cmd, null);
                    }));
                }

                if (msg.action == "view-mtrl")
                {
                    int mtrl = msg.mtrl;
                    string cmd = $"ITEM[FORM='AgroHellas Pro Lev1',AUTOLOCATE={mtrl}]";

                    // ΤΗΝ ΕΚΤΕΛΟΥΜΕ ΣΤΟ UI THREAD
                    this.BeginInvoke(new Action(() =>
                    {
                        ApoForm.XSupport.ExecS1Command(cmd, null);
                    }));
                }

 

                if (msg.action == "view-updstatus")
                {
                    int findoc = msg.findoc;

                    var sysTable = ApoForm.XSupport.GetMemoryTable("SYS");
                    string currentUserName = sysTable[0, "USERNAME"].ToString();

                    string sqlupd = $"UPDATE FINDOC SET CCCNPCOMPENTENT = 'Δημιουργήκε απο: {currentUserName}'  WHERE FINDOC = {findoc}";

                    // ΤΗΝ ΕΚΤΕΛΟΥΜΕ ΣΤΟ UI THREAD
                    this.BeginInvoke(new Action(() =>
                    {
                        ApoForm.XSupport.ExecuteSQL(sqlupd);
                    }));
                }

                if (msg.action == "refresh")
                {

                    SendMrpDataToWeb();
                    return;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("WebMessage error: " + ex.Message);
            }

        }


        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private async void SendMrpDataToWeb()
        {
            var json = await Task.Run(() =>
            {
                var list = MrpCalculator.LoadAll();
                return JsonConvert.SerializeObject(
                    list,
                    new JsonSerializerSettings
                    {
                        DateFormatString = "yyyy-MM-dd"
                    });
            });

            this.BeginInvoke(new Action(() =>
            {
                if (webView21.CoreWebView2 != null)
                {
                    webView21.CoreWebView2.PostWebMessageAsString(json);
                }
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ApoForm.XSupport.ExecS1Command("SALDOC[FORM='Agrohellas Pro Lev1',AUTOLOCATE=1783757]", null);
        }

        private string ExtractEmbeddedFileToDisk(string resourceName, string outputFileName)
        {
            // Προτείνω temp folder για να μη χρειάζεσαι δικαιώματα στο Program Files
            string baseDir = Path.Combine(Path.GetTempPath(), "S1PluginProject", "Apo");
            Directory.CreateDirectory(baseDir);

            string outPath = Path.Combine(baseDir, outputFileName);

            var asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(resourceName))
            {
                if (s == null)
                {
                    // χρήσιμο για debug: να δεις τα διαθέσιμα resource names
                    var all = string.Join("\n", asm.GetManifestResourceNames());
                    throw new Exception($"Resource not found: {resourceName}\n\nAvailable:\n{all}");
                }

                using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    s.CopyTo(fs); // πάντα replace
                }
            }

            return outPath;
        }
    }
}
