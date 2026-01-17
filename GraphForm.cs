using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Softone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S1PluginProject
{
    public partial class GraphForm : Form
    {
        

        private readonly string _payloadJson;

        public GraphForm(string payloadJson)
        {
            InitializeComponent();
            _payloadJson = payloadJson;
            //this.Load += GraphForm_Load;
        }

        public class ClientMessage
        {
            public string action { get; set; }
            public int findoc { get; set; }
            public int mtrl { get; set; }
        }

        //private async void GraphForm_Load(object sender, EventArgs e)
        //{
        //    await webView21.EnsureCoreWebView2Async();
        //    webView21.CoreWebView2.Settings.IsWebMessageEnabled = true;

        // //   webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

        //    string htmlPath = Path.Combine(Application.StartupPath, "graph.html");

        //    // 1) Πρώτα κάνουμε navigate
        //    webView21.CoreWebView2.Navigate(htmlPath);

        //    // 2) Μετά περιμένουμε να ολοκληρωθεί η φόρτωση
        // //   webView21.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
        //}

        private async void GraphForm_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();

            webView21.CoreWebView2.Settings.IsWebMessageEnabled = true;

            webView21.CoreWebView2.WebMessageReceived += GraphWebMessageReceived;


            string graphPath = ExtractEmbeddedFileToDisk(
               "S1PluginProject.graph.html", // ⚠️ άλλαξέ το αν το namespace/folder διαφέρει
               "graph.html"
           );
            
            webView21.CoreWebView2.Navigate(new Uri(graphPath).AbsoluteUri);

           // string htmlPath = Path.Combine(Application.StartupPath, "graph.html");
           // webView21.CoreWebView2.Navigate(htmlPath);

            webView21.CoreWebView2.NavigationCompleted += (s, ev) =>
            {
                webView21.CoreWebView2.PostWebMessageAsString(_payloadJson);
            };
        }


        private async void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {

            //var json = await Task.Run(() =>
            //{
            //    var list = MrpCalculator.LoadAll();
            //    return Newtonsoft.Json.JsonConvert.SerializeObject(list,
            //        new Newtonsoft.Json.JsonSerializerSettings
            //        {
            //            DateFormatString = "yyyy-MM-dd"
            //        });
            //});

            //// 2) Στέλνουμε το μήνυμα στην WebView2 ΧΩΡΙΣ καν να μπλοκάρουμε το UI thread
            //this.BeginInvoke(new Action(() =>
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

        private void GraphWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                var msg = JsonConvert.DeserializeObject<ClientMessage>(json);
                if (msg == null) return;

                // ΔΙΠΛΟ ΚΛΙΚ ΣΕ ΕΙΔΟΣ
                if (msg.action == "view-mtrl")
                {
                    int mtrl = msg.mtrl;
                    string cmd = $"ITEM[FORM='AgroHellas Pro Lev1',AUTOLOCATE={mtrl}]";

                    this.BeginInvoke(new Action(() =>
                    {
                        ApoForm.XSupport.ExecS1Command(cmd, null);
                    }));
                    return;
                }

                // ΔΙΠΛΟ ΚΛΙΚ ΣΕ ΠΑΡΑΣΤΑΤΙΚΟ
                if (msg.action == "view-doc")
                {
                    int findoc = msg.findoc;

                    // ίδια λογική με sosource όπως στο ApoForm
                    XTable t = ApoForm.XSupport.GetSQLDataSet($"SELECT SOSOURCE FROM FINDOC WHERE FINDOC={findoc}", null);
                    int sosource = Convert.ToInt32(t[0, "SOSOURCE"]);

                    string cmd = null;
                    if (sosource == 1351) cmd = $"SALDOC[FORM='Agrohellas Lev0',AUTOLOCATE={findoc}]";
                    if (sosource == 1171) cmd = $"PRDORDDOC[FORM='Agrohellas Pro',AUTOLOCATE={findoc}]";
                    if (sosource == 1151) cmd = $"ITEDOC[FORM='Agrohellas',AUTOLOCATE={findoc}]";

                    if (cmd == null) return;

                    this.BeginInvoke(new Action(() =>
                    {
                        ApoForm.XSupport.ExecS1Command(cmd, null);
                    }));
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Graph WebMessage error: " + ex.Message);
            }
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
