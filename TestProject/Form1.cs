using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading;
using System.Windows.Forms;
using com.epson.pos.driver;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Communication;
using TIM.Devices.Framework.Epson;
using MediaDetectedEventArgs = TIM.Devices.Framework.TIMsDevices.MediaDetectedEventArgs;

namespace TestProject
{
    public partial class Form1 : Form
    {
        DeviceClient c;
        StatusAPI api;

        public Form1()
        {
            InitializeComponent();

            //textBox1.Text = LogManager.GetFormattedSummary(new Exception("Test"), new Exception("Test Conn"), LogLevel.Error);

            /*IDevice kl = new KeyboardListener.KeyboardListener();
            kl.MediaFound += new EventHandler<DeviceEventArgs>(kl_MediaFound);
            kl.Open();
            kl.MediaSearchEnabled = true;*/
            /*c = new DeviceClient(null, TIM.Devices.Framework.ConfigDevices.All);
            c.MediaDetected += new EventHandler<TIM.Devices.Framework.TIMsDevices.MediaDetectedEventArgs>(c_MediaDetected);
            c.ScopeCheckDelegate = new Func<bool>(delegate() { return true; });*/

            /*api = new com.epson.pos.driver.StatusAPI();
            Console.WriteLine(api.OpenMonPrinter(com.epson.pos.driver.OpenType.TYPE_PRINTER, "EPSON TM-T88IV Receipt"));*/
            //System.Windows.Forms.Timer tmr = new System.Windows.Forms.Timer();
            //tmr.Interval = 500;
            //tmr.Tick += new EventHandler(tmr_Tick);
            //tmr.Enabled = true;
            /*api.StatusCallback += new com.epson.pos.driver.StatusAPI.StatusCallbackHandler(api_StatusCallback);            
            api.SetStatusBack();*/
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            /*com.epson.pos.driver.ASB asb;
            api.GetRealStatus(out asb);
            Console.WriteLine(asb);*/
            //Console.WriteLine(api.Status);
            Stopwatch sw = Stopwatch.StartNew();
            //Console.WriteLine(ThermalPrinter.DrawerAvailable);
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        void api_StatusCallback(ASB asb)
        {
            Console.WriteLine(asb);
        }

        void c_MediaDetected(object sender, MediaDetectedEventArgs e)
        {
            Console.WriteLine(e.CheckInID.HasValue ? e.CheckInID.Value.ToString() : "X");
        }

        void kl_MediaFound(object sender, DeviceEventArgs e)
        {
            Console.WriteLine(e.Device.GetMediaID());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //c.Request();

            ThermalPrinter tp = new ThermalPrinter();
            tp.DrawerOpened += new EventHandler(tp_DrawerOpened);
            tp.DrawerClosed += new EventHandler(tp_DrawerClosed);
            tp.PrintPage += new EventHandler<GenericEventArgs<Container<PrintPageEventArgs, object>>>(tp_PrintPage);
            //tp.Execute(true, null);
        }

        void tp_PrintPage(object sender, GenericEventArgs<Container<PrintPageEventArgs, object>> e)
        {
            e.Data.Value1.Graphics.DrawString("Hello World", new Font(FontFamily.GenericMonospace, 12f), Brushes.Black, 10, 10);
        }

        void tp_DrawerClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Close" + Thread.CurrentThread.ManagedThreadId.ToString());
            Text = "Close";
        }

        void tp_DrawerOpened(object sender, EventArgs e)
        {
            Console.WriteLine("Open" + Thread.CurrentThread.ManagedThreadId.ToString());
            Text = "Open";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine(api.OpenMonPrinter(OpenType.TYPE_PRINTER, "EPSON TM-T88IV Receipt"));
            /*ASB asb;
            api.GetRealStatus(out asb);
            Console.WriteLine(asb);*/
            Console.WriteLine(api.Status);
            Console.WriteLine(api.CloseMonPrinter());
        }
    }
}
