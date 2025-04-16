using TIM.Common.CoreStandard;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Blocks
{
    public class ResponseControlField : ControlField
    {
        #region Properties

        public byte CCRC
        {
            get { return FirstByte; }
            set { FirstByte = value; }
        }

        public byte APRC
        {
            get { return SecondByte; }
            set { SecondByte = value; }
        }

        #endregion

        #region Constructors

        public ResponseControlField(byte ccrc, byte aprc)
        {
            FirstByte = ccrc;
            SecondByte = aprc;
        }

        public ResponseControlField(Enums.ZVTCommands zvtCommand)
        {
            BitmapHelper.GetCommandBytes(zvtCommand, out FirstByte, out SecondByte);
        }

        #endregion
    }
}