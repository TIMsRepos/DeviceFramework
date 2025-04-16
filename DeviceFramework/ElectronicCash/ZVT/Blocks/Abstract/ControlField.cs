using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract
{
    public abstract class ControlField
    {
        #region Properties

        protected byte FirstByte;
        protected byte SecondByte;

        public byte[] Bytes => new[] { FirstByte, SecondByte };

        #endregion

        #region Methods

        public static bool operator ==(ControlField firstControlField, ControlField secondControlField)
        {
            return firstControlField?.FirstByte == secondControlField?.FirstByte &&
                   firstControlField?.SecondByte == secondControlField?.SecondByte;
        }

        public static bool operator !=(ControlField firstControlField, ControlField secondControlField)
        {
            return firstControlField?.FirstByte != secondControlField?.FirstByte ||
                   firstControlField?.SecondByte != secondControlField?.SecondByte;
        }

        public override string ToString()
        {
            return $"[{GetType().FullName}] {TraceHelper.Bytes2Hex(FirstByte, SecondByte)}";
        }

        #endregion
    }
}