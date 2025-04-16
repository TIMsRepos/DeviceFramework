using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using TIM.Devices.Framework.Communication;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Description;

namespace TIMs_Devices
{
    public partial class TIMsDevices : ServiceBase
    {
        #region Members
        private ServiceHost MyServiceHost; 
	    #endregion

        public TIMsDevices()
        {
            InitializeComponent();
        }

        public void OnStart()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            /*ServiceMetadataBehavior MyServiceMetadataBehavior = new ServiceMetadataBehavior();
            MyServiceMetadataBehavior.HttpGetEnabled = true;

            MyServiceHost = new ServiceHost(
                typeof(DeviceServer),
                new Uri("http://localhost:54321/DeviceFramework"));
            MyServiceHost.AddServiceEndpoint(typeof(IDeviceServer),
                new NetTcpBinding(),
                "net.tcp://localhost:54320/DeviceFramework");
            MyServiceHost.Description.Behaviors.Add(MyServiceMetadataBehavior);
            MyServiceHost.Open();*/

            /*Console.WriteLine("Listening...");
            Console.ReadLine();*/

            //MyServiceHost.Close();
            //Thread.Sleep(Timeout.Infinite);
                        
            //DeviceServer MyDeviceServer = new DeviceServer();
            OnContinue();
        }

        protected override void OnStop()
        {
            OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();

            ServiceMetadataBehavior MyServiceMetadataBehavior = new ServiceMetadataBehavior();
            MyServiceMetadataBehavior.HttpGetEnabled = true;
            ServiceDebugBehavior MyServiceDebugBehavior = new ServiceDebugBehavior();
            MyServiceDebugBehavior.IncludeExceptionDetailInFaults = true;

            MyServiceHost = new ServiceHost(
                typeof(DeviceServer),
                new Uri("http://localhost:54321/DeviceFramework"));
            NetTcpBinding MyNetTcpBinding = new NetTcpBinding();
            MyNetTcpBinding.SendTimeout = MyNetTcpBinding.OpenTimeout = MyNetTcpBinding.CloseTimeout = MyNetTcpBinding.ReceiveTimeout =
                new TimeSpan(0, 0, 5);
            MyServiceHost.AddServiceEndpoint(typeof(IDeviceServer),
                MyNetTcpBinding,
                "net.tcp://localhost:54320/DeviceFramework");
            MyServiceHost.Description.Behaviors.Add(MyServiceMetadataBehavior);
            MyServiceHost.Description.Behaviors.Add(MyServiceDebugBehavior);
            MyServiceHost.Open();
        }

        protected override void OnPause()
        {
            base.OnPause();

            MyServiceHost.Close();
        }
    }
}
