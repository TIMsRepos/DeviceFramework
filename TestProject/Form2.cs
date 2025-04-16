using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Threading;

namespace TestProject
{
    public partial class Form2 : Form
    {
        private List<Thread> workers = new List<Thread>();
        private List<IDevice> devs = new List<IDevice>();

        public Form2()
        {
            InitializeComponent();

            ThreadManager.GUIContext = SynchronizationContext.Current;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            string[] configStrings = new string[] {
                "COM=11|Baud=9600|TriggerActive=200",
                "COM=9|Baud=9600|TriggerActive=200" 
            };

            for(int c = 0; c < configStrings.Length; ++c)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(delegate(object obj)
                    {
                        // string configString = "COM=9|Baud=9600";
                        //   int id = 8;

                        /*ConfigString cfg = ConfigString.Parse(configStrings[((tparams)obj).C], '=', '|');

                        IDevice dev = DeviceManager.GetDevice(8, 7, cfg);
                        dev.MediaFound += new EventHandler<DeviceEventArgs>(dev_MediaFound);
                        dev.MediaRemoved += new EventHandler<DeviceMediaRemovedEventArgs>(dev_MediaRemoved);
                        //dev.MediaSearchEnabled = true;

                        devs.Add(dev);*/

                        /// ...

                        /*dev.MediaSearchEnabled = false;
                        dev.Close();*/
                    }));
                workers.Add(thread);
            }

            for (int i = 0; i < workers.Count; ++i)
            {
                workers[i].Start(new tparams(i));
                workers[i].Join();
            }

            for (; ; )
            {
                StringBuilder sb = new StringBuilder();
                foreach (IDevice dev in devs)
                {
                    try
                    {
                        sb.Append(dev.GetMediaID() + "; ");
                    }
                    catch (MissingMediaException)
                    {
                        sb.Append("; ");
                    }
                }
                Console.WriteLine(sb.ToString());
                Thread.Sleep(100);
            }
        }

        class tparams
        {
            public int C { get; set; }

            public tparams(int c)
            {
                C = c;
            }
        }

        void dev_MediaRemoved(object sender, DeviceMediaRemovedEventArgs e)
        {
            ThreadManager.DispatchToGUIThreaded(delegate()
            {
                Text = string.Format("REMOVED [{0}]: {1}", e.Device.ConfigString, e.MediaID.ToString());
            });   
        }

        void dev_MediaFound(object sender, DeviceEventArgs e)
        {
            ThreadManager.DispatchToGUIThreaded(delegate()
            {
                Text = string.Format("FOUND [{0}]: {1}", e.Device.ConfigString, e.MediaID.ToString());
            });
        }
    }
}
