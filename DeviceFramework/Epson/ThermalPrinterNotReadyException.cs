using System;

namespace TIM.Devices.Framework.Epson
{
    public class ThermalPrinterNotReadyException : ThermalPrinterException
    {
        public ThermalPrinterNotReadyException(string strMessage) : base(strMessage)
        {
        }

        public ThermalPrinterNotReadyException(string strMessage, Exception exInner) : base(strMessage, exInner)
        {
        }
    }
}