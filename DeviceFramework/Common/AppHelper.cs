using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TIM.Common.CoreStandard;

namespace TIM.Devices.Framework.Common
{
    public class AppHelper
    {
        [DllImport("Dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool blnEnabled);

        private static Nullable<Enums.Applications> MyForceApplication;

        /// <summary>
        /// Force DetectApplication to return this application, for debug of TIM-Gateway!
        /// </summary>
        public static Nullable<Enums.Applications> ForceApplication
        {
            get => MyForceApplication;
            set => MyForceApplication = value;
        }

        public static bool IsCompositionEnabled
        {
            get
            {
                bool blnEnabled;
                DwmIsCompositionEnabled(out blnEnabled);
                return blnEnabled;
            }
        }

        /// <summary>
        /// It returns the absolute directory path of the calling main program .exe file
        /// </summary>
        /// <returns>The program path</returns>
        public static DirectoryInfo GetEntryAssemblyDirectory()
        {
            return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

        public static Enums.Applications DetectApplication()
        {
            if (MyForceApplication.HasValue)
                return MyForceApplication.Value;

            string strExeFileName = Path.GetFileName(Application.ExecutablePath).ToLower();

            // ALL CASES NEED TO BE LOWER CASE
            switch (strExeFileName)
            {
                case "tim-administrator.exe":
                case "displaycustomers.exe":
                    return Enums.Applications.Administrator;

                case "tims-devices.exe":
                    return Enums.Applications.Devices;

                case "tims-frontend.exe":
                    return Enums.Applications.Frontend;

                case "tim-gatdevices.exe":
                    return Enums.Applications.TIMsGATDevices;

                case "tims-signage.exe":
                    return Enums.Applications.TIMsSignage;

                default:
                    if (System.Web.HttpContext.Current != null)
                        return Enums.Applications.TIMsGateway;
                    return Enums.Applications.Other;
            }
        }
    }
}