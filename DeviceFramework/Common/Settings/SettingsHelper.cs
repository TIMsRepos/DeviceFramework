using System;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.Helper;
using TIM.Devices.Framework.Common.Printer;
using TIM.Devices.Framework.Database;

namespace TIM.Devices.Framework.Common.Settings
{
    public static class SettingsHelper
    {
        private const string ComputerDetailSettingWebcam = "WebcamSettings";

        /// <summary>
        /// Get printer name from computer settings (Frontend only)
        /// </summary>
        /// <returns>Printer name or NULL</returns>
        public static string GetPrinterName(Enums.PrinterTypeFrontend printerTypeFrontend)
        {
            string printerName;
            if (printerTypeFrontend == Enums.PrinterTypeFrontend.SystemDefaultPrinter)
            {
                printerName = PrinterHelper.GetSystemDefaultPrinterName();
            }
            else
            {
                var ok = SettingsManager.TryGet(Enums.ComputerSetting.Printer, printerTypeFrontend.ToComputerDetailSetting(), out printerName);
                if (!ok || String.IsNullOrWhiteSpace(printerName))
                    printerName = "";
            }

            //if (!PrinterHelper.IsValid(printerName))
            //    throw new PrinterInvalidException();

            return printerName;
        }

        /// <summary>
        /// Get printer name or paper source from computer settings (Admin only)
        /// </summary>
        /// <returns>Printer name or NULL</returns>
        public static string GetPrinter(Enums.PrinterTypeAdmin printerTypeAdmin, Enums.PrinterSettingDetail printerSettingDetail)
        {
            string computerDetailSettingID = String.Format("{0}Printer_{1}", printerTypeAdmin, printerSettingDetail);
            var bOk = Enum.TryParse(computerDetailSettingID, out Enums.ComputerDetailSetting printerSettingDetailID);
            if (!bOk)
                return null;
            string value;
            var ok = SettingsManager.TryGet(Enums.ComputerSetting.PrinterAdmin, printerSettingDetailID, out value);
            return (!ok || String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
        }

        /// <summary>
        /// Save printer name or paper source in computer settings (Admin only)
        /// </summary>
        public static void SetPrinter(Enums.PrinterTypeAdmin printerTypeAdmin, Enums.PrinterSettingDetail printerSettingDetail, string value)
        {
            string computerDetailSettingID = String.Format("{0}Printer_{1}", printerTypeAdmin, printerSettingDetail);
            ComputerSettingController.SaveComputerSetting(SettingsManager.ComputerName, Enums.ComputerSetting.PrinterAdmin.ToString(), computerDetailSettingID, value);
        }

        /// <summary>
        /// Get webcam device number from computer settings
        /// </summary>
        /// <returns>Webcam device number or NULL</returns>
        public static string GetWebcamDeviceNumber()
        {
            string value;
            var ok = SettingsManager.TryGet(Enums.ComputerSetting.Webcam, Enums.ComputerDetailSetting.WebcamSettings, out value);
            return (!ok || String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
        }

        /// <summary>
        /// Save webcam device number in computer settings
        /// </summary>
        /// <param name="value"></param>
        public static void SetWebcamDeviceNumber(string value)
        {
            ComputerSettingController.SaveComputerSetting(SettingsManager.ComputerName, Enums.ComputerSetting.Webcam.ToString(), ComputerDetailSettingWebcam, value);
        }
    }
}