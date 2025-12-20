using Softone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace S1PluginProject
{
    public partial class Form1 : Form
    {

        static public XSupport XSupport;
        static public XTable myData;

        public Form1()
        {
            InitializeComponent();
            Form1_Load();

            myData = XSupport.GetSQLDataSet("SELECT TOP 10 CODE, NAME FROM TRDR", null);
            MessageBox.Show("Hello from Server code");
        }

        private async void Form1_Load()
        {
            await webView21.EnsureCoreWebView2Async(null);
            webView21.Source = new Uri("https://shipintime.gr/signin-up");
        }

        private void webView21_Click(object sender, EventArgs e)
        {
/*            myData = XSupport.GetSQLDataSet("SELECT TOP 10 CODE, NAME FROM TRDR", null);

            MessageBox.Show("Hello from Server code");*/
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
/*            myData = XSupport.GetSQLDataSet("SELECT TOP 10 CODE, NAME FROM TRDR", null);

            MessageBox.Show("Hello from Server code");*/
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
  /*          myData = XSupport.GetSQLDataSet("SELECT TOP 10 CODE, NAME FROM TRDR", null);

            MessageBox.Show("Hello from Server code");*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
/*            myData = XSupport.GetSQLDataSet("SELECT TOP 10 CODE, NAME FROM TRDR", null);

            MessageBox.Show("Hello from Server code");*/
        }
    }

/*    public class S1Init : TXCode
    {
        public override void Initialize()
        {
            Form1.XSupport = XSupport;
           // Example.Initialize();
            MessageBox.Show("Start Demon!");
        }

    }*/

}
