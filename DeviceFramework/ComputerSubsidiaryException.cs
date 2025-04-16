using System;

namespace TIM.Devices.Framework
{
    [global::System.Serializable]
    public class ComputerSubsidiaryException : Exception
    {
        public ComputerSubsidiaryException(string strMessage) : base(strMessage)
        {
        }
    }
}