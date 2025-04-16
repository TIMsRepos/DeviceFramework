using TIM.Common.CoreStandard;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Blocks
{
    public class CommandControlField : ControlField
    {
        #region Properties

        public byte CLASS
        {
            get { return FirstByte; }
            set { FirstByte = value; }
        }

        public byte INSTR
        {
            get { return SecondByte; }
            set { SecondByte = value; }
        }

        #endregion

        #region Constructors

        public CommandControlField(byte @class, byte instr)
        {
            FirstByte = @class;
            SecondByte = instr;
        }

        public CommandControlField(Enums.ZVTCommands zvtCommand)
        {
            BitmapHelper.GetCommandBytes(zvtCommand, out FirstByte, out SecondByte);
        }

        #endregion
    }
}