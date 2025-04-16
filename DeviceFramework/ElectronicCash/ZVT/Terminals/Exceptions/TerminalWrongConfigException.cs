using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Exceptions
{
    [global::System.Serializable]
    public class TerminalWrongConfigException : TerminalException
    {
        public TerminalWrongConfigException(string strMessage) : base(strMessage)
        {
        }
    }
}