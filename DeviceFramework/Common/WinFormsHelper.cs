using System;
using System.Threading;
using System.Windows.Forms;

namespace TIM.Devices.Framework.Common
{
    public static class WinFormsHelper
    {
        public static void WaitForHandle(Control ctlControl)
        {
            while (!ctlControl.IsHandleCreated)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
        }
    }
}