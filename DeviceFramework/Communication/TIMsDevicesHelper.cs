using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TIM.Devices.Framework.Common;
using Enums = TIM.Common.CoreStandard.Enums;
using RegistryHelper = TIM.Common.CoreStandard.Helper.RegistryHelper;

namespace TIM.Devices.Framework.Communication
{
    public class TIMsDevicesHelper
    {
        private static bool blnStarting;

        public static void StartAndWaitForServer()
        {
            // Is DF installed
            if (RegistryHelper.GetInstallPath(Enums.Applications.Devices) == null)
                return;

            if (!blnStarting)
            {
                blnStarting = true;

                if (IsVisualStudioRunningWithDF())
                {
                    // VS running
                    try
                    {
                        Workflows.InvokeWithTimeout(() => WaitForWebService(), 2000);
                    }
                    catch (TimeoutException)
                    {
                        // VS running but no DF -> start DF
                        StartAndWaitForExecutable();
                        WaitForWebService();
                    }
                }
                else
                {
                    // VS not running
                    StartAndWaitForExecutable();
                    WaitForWebService();
                }
            }
        }

        private static bool IsVisualStudioRunningWithDF()
        {
            string strExe = "TIMs-Devices.vshost";
            Process[] MyProcesses = Process.GetProcessesByName(strExe);
            return MyProcesses.Any(p => p.ProcessName == strExe);
        }

        private static void StartAndWaitForExecutable()
        {
            bool blnExeStarted = false;
            Process[] MyProcesses = Process.GetProcessesByName("TIMs-Devices");
            while (!MyProcesses.Any(p => p.ProcessName == "TIMs-Devices"))
            {
                if (!blnExeStarted)
                {
                    Process.Start(RegistryHelper.GetInstallPath(Enums.Applications.Devices).FullName + "\\TIMs-Devices.exe");
                    blnExeStarted = true;
                }

                Thread.Sleep(500);
                MyProcesses = Process.GetProcessesByName("TIMs-Devices");
            }
        }

        private static void WaitForWebService()
        {
            bool blnConnectFailed = true;
            while (blnConnectFailed)
            {
                try
                {
                    TcpClient MyTcpClient = new TcpClient();
                    MyTcpClient.ReceiveTimeout = MyTcpClient.SendTimeout = 500;
                    MyTcpClient.Connect(new IPAddress(new byte[] { 127, 0, 0, 1 }), 54321);
                    MyTcpClient.Close();

                    blnConnectFailed = false;
                }
                catch (SocketException)
                {
                    blnConnectFailed = true;
                    Thread.Sleep(500);
                }
            }
        }
    }
}