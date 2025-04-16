using System;

namespace TIM.Devices.Framework.Epson
{
    [global::System.Serializable]
    public class ThermalPrinterException : FrameworkException
    {
        public ThermalPrinterException() : base()
        {
        }

        public ThermalPrinterException(string strMessage) : base(strMessage)
        {
        }

        public ThermalPrinterException(string strMessage, Exception exInner) : base(strMessage, exInner)
        {
        }
    }
}