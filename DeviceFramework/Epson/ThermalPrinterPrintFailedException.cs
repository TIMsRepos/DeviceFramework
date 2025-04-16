using System;

namespace TIM.Devices.Framework.Epson
{
    public class ThermalPrinterPrintFailedException : ThermalPrinterException
    {
        public ThermalPrinterPrintFailedException() : base()
        {
        }

        public ThermalPrinterPrintFailedException(string strMessage) : base(strMessage)
        {
        }
    }
}