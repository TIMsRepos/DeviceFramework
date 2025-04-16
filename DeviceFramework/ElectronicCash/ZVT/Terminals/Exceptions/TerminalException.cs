using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Exceptions
{
    [global::System.Serializable]
    public class TerminalException : ZVTException
    {
        public TerminalException() : base()
        {
        }

        public TerminalException(string strMessage) : base(strMessage)
        {
        }
    }
}