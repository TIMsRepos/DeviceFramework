using System.Collections.Generic;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events
{
    public class InterimStateInfoEventArgs : StateInfoEventArgs
    {
        public override string Result => InterimStates.GetMessage(ResultCode);

        public InterimStateInfoEventArgs(IReadOnlyList<byte> bytData)
            : base(bytData[0])
        {
        }
    }
}