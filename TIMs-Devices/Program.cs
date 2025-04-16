using System;
using System.Windows.Forms;
using TIM.Common.Data.Helper;
using TIM.Devices.Framework.Common;

namespace TIMs_Devices
{
    internal static class Program
    {
        public static FrmDevices DevicesForm { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (ProcessHelper.FindRunningInstanceWithSameName() != null)
            {
                MessageBox.Show(
                    @"TIMs-Devices wurde bereits gestartet, ein Mehrfachstart ist nicht möglich!",
                    @"Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Application.EnableVisualStyles();

                // load config data
                var result = AppSettingHelper.InitAppSettingData(Environment.UserName);
                if (!result.Success)
                {
                    MessageBox.Show(
                    result.Message,
                    @"Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DevicesForm = new FrmDevices();
                Application.Run(DevicesForm);
            }
        }
    }
}