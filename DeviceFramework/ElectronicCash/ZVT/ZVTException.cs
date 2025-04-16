using System;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT
{
    [global::System.Serializable]
    public class ZVTException : FrameworkException
    {
        public ZVTException()
        {
        }

        public ZVTException(string strMessage) : base(strMessage + "\r\n\r\nZVT-Trace:\r\n" + TraceHelper.Dump())
        {
        }

        public ZVTException(string strMessage, Exception exInner) : base(strMessage + "\r\n\r\nZVT-Trace:\r\n" + TraceHelper.Dump(), exInner)
        {
        }
    }
}