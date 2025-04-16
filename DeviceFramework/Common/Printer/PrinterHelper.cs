using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Management;
using System.Text;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common.Printer.PrinterExceptions;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.Framework.Common.Printer
{
    public static class PrinterHelper
    {
        /// <summary>
        /// Checks whether the printer is a valid printer for this computer and user
        /// </summary>
        /// <param name="printerName">The windows printername</param>
        /// <returns>Whether the printer is valid</returns>
        public static bool IsValidPrinter(string printerName)
        {
            if (String.IsNullOrWhiteSpace(printerName))
                return false;

            PrinterSettings MyPrinterSettings = new PrinterSettings();
            MyPrinterSettings.PrinterName = printerName;
            return MyPrinterSettings.IsValid;
        }

        /// <summary>
        /// Checks whether the printer is a valid printer for this computer and user
        /// </summary>
        /// <returns>Whether the printer is valid</returns>
        public static bool IsValidPrinter(Enums.PrinterTypeFrontend printerType)
        {
            var printerName = SettingsHelper.GetPrinterName(printerType);
            return IsValidPrinter(printerName);
        }

        /// <summary>
        /// Gets a detailed status for the printer
        /// </summary>
        /// <param name="printerName">The windows printername</param>
        /// <returns>The printers status</returns>
        public static Enums.PrinterStatus GetStatus(string printerName)
        {
            string strWmiPath = string.Format("win32_printer.DeviceId='{0}'", printerName);
            ManagementObject wmiPrinter = new ManagementObject(strWmiPath);
            try
            {
                wmiPrinter.Get();
                PropertyDataCollection printerProperties = wmiPrinter.Properties;
                return (Enums.PrinterStatus)Convert.ToInt32(printerProperties["PrinterStatus"].Value);
            }
            catch (ManagementException)
            {
                return Enums.PrinterStatus.Unknown;
            }
        }

        /// <summary>
        /// Gets the portname for the printer
        /// </summary>
        /// <param name="printerName">The windows printername</param>
        /// <returns>The printers port name</returns>
        public static string GetPortName(string printerName)
        {
            string strWmiPath = string.Format("win32_printer.DeviceId='{0}'", printerName);
            ManagementObject wmiPrinter = new ManagementObject(strWmiPath);
            try
            {
                wmiPrinter.Get();
                PropertyDataCollection printerProperties = wmiPrinter.Properties;
                return printerProperties["PortName"].Value.ToString();
            }
            catch (ManagementException)
            {
                return null;
            }
        }

        public static KeyValuePair<UInt16, string>[] GetRawPaperSources(string printerName)
        {
            UInt16[] intBinArray = null;
            string strPortName = GetPortName(printerName);
            int intBinCount = Win32.WinSpool.DeviceCapabilities(printerName, strPortName, Win32.WinSpool.DC_BINS, null, IntPtr.Zero);
            int intRet;
            if (intBinCount > -1)
            {
                intBinArray = new UInt16[intBinCount];
                intRet = Win32.WinSpool.DeviceCapabilitiesA(printerName, strPortName, Win32.WinSpool.DC_BINS, intBinArray, IntPtr.Zero);
                if (intRet > -1)
                {
                    byte[] bytBuffer = new byte[intBinCount * 24];
                    intRet = Win32.WinSpool.DeviceCapabilitiesA(printerName, strPortName, Win32.WinSpool.DC_BINNAMES, bytBuffer, IntPtr.Zero);
                    if (intRet > -1)
                    {
                        // Get all trays in one system-string
                        string strBinNamesRaw = Encoding.ASCII.GetString(bytBuffer, 0, bytBuffer.Length);
                        // Split the null terminated strings into a string array for searching
                        while (strBinNamesRaw.IndexOf("\0\0") > -1)
                            strBinNamesRaw = strBinNamesRaw.Replace("\0\0", "\0");
                        string[] strBinNames = strBinNamesRaw.Split('\0');

                        KeyValuePair<UInt16, string>[] MyBinPairs = new KeyValuePair<UInt16, string>[intBinCount];
                        for (int i = 0; i < intBinCount; ++i)
                            MyBinPairs[i] = new KeyValuePair<UInt16, string>(intBinArray[i], strBinNames[i]);

                        return MyBinPairs;
                    }
                    else
                        throw new PrinterException(string.Format("Couldn't get bin names for '{0}'@'{1}'.", printerName, strPortName));
                }
                else
                    throw new PrinterException(string.Format("Couldn't get bin IDs for '{0}'@'{1}'.", printerName, strPortName));
            }
            else
                throw new PrinterException(string.Format("Couldn't get bin count for '{0}'@'{1}'.", printerName, strPortName));
        }

        /// <summary>
        /// Checks whether the given printer is an epson bonprinter
        /// </summary>
        /// <param name="printerName">The windows pritername</param>
        /// <returns>Whether the given printer is an epson bonprinter</returns>
        public static bool IsBonPrinter(string printerName)
        {
            string strWMIPath = string.Format("win32_printer.DeviceId='{0}'", printerName);
            ManagementObject MyWMIPrinter = new ManagementObject(strWMIPath);
            try
            {
                MyWMIPrinter.Get();
                PropertyDataCollection MyPrinterProperties = MyWMIPrinter.Properties;
                return MyPrinterProperties["DriverName"].Value.ToString().Trim().ToLower().StartsWith("EPSON TM-T88".ToLower());
            }
            catch (ManagementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets windows default printername
        /// </summary>
        /// <returns>The windows printername</returns>
        public static string GetSystemDefaultPrinterName()
        {
            return new PrinterSettings().PrinterName;
        }

        public static void PrintText(string strText, Font fntFont, string strPrinterName)
        {
            string[] strLines = strText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string strCurrentLine = null;
            int intLine = 0;
            float fltYOffset = 0f;
            SizeF szLine;

            PrintDocument prtDoc = new PrintDocument();
            prtDoc.PrintPage += delegate (object sender, PrintPageEventArgs e)
            {
                while (intLine < strLines.Length - 1)
                {
                    if (strCurrentLine == null)
                        strCurrentLine = strLines[intLine];

                    szLine = e.Graphics.MeasureString(strCurrentLine, fntFont, e.MarginBounds.Width);
                    if (string.IsNullOrEmpty(strCurrentLine))
                        szLine = e.Graphics.MeasureString("X", fntFont);
                    if (e.MarginBounds.Top + fltYOffset + szLine.Height > e.MarginBounds.Bottom)
                    {
                        // PAGE BREAK
                        fltYOffset = 0;
                        e.HasMorePages = true;
                        break;
                    }

                    e.Graphics.DrawString(strCurrentLine, fntFont, Brushes.Black, new RectangleF(
                        e.MarginBounds.Left, e.MarginBounds.Top + fltYOffset, e.MarginBounds.Width, e.MarginBounds.Height));
                    strCurrentLine = null;

                    fltYOffset += szLine.Height;
                    if (strCurrentLine == null)
                        ++intLine;
                }
            };
            prtDoc.Print();
        }
    }
}