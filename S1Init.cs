using Softone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S1PluginProject
{
    public class S1Init : TXCode
    {
        public override void Initialize()
        {
            Form1.XSupport = XSupport;
            ApoForm.XSupport = XSupport;
            MrpCalculator.XSupport = XSupport;

            // Example.Initialize();
           // MessageBox.Show("Start Demon!");
        }

    }
}
