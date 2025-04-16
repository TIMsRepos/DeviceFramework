using System;

namespace TIM.Devices.Framework.Common.Printer.PrinterExceptions
{
    [Serializable]
    public class PrinterException : FrameworkException
    {
        public PrinterException(string message) : base(message)
        {
        }
    }
}