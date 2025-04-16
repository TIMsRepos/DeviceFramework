using System.Linq;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Blocks
{
    public class ResponseAPDU : APDU
    {
        #region Properties

        public ResponseControlField ControlField { get; set; }

        public byte[] CRCData => ControlField.Bytes.Concat(new[] { Length }).Concat(Data).ToArray();

        public string ErrorMessage => ErrorMessages.GetMessage(ControlField.APRC);

        public bool HasError => ControlField.CCRC != 0x80 && !(ControlField.CCRC == 0x84 && ControlField.APRC == 0x00);

        #endregion

        #region Constructor

        public ResponseAPDU(ResponseControlField controlField, byte[] bytData) :
            base(bytData)
        {
            this.ControlField = controlField;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return
                $"[{GetType().FullName}] {TraceHelper.Bytes2Hex(ControlField.Bytes.Concat(new[] { Length }).Concat(Data).ToArray())}";
        }

        #endregion
    }
}