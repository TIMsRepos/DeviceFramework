using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events
{
    public abstract class StateInfoEventArgs : EventArgs
    {
        public abstract string Result { get; }
        public byte ResultCode { get; protected set; }
        public bool ResultHasError => ResultCode != 0;

        protected StateInfoEventArgs(byte bytResultCode)
        {
            ResultCode = bytResultCode;
        }
    }
}