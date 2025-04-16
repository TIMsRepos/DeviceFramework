using System.Linq;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Blocks
{
    public class CommandAPDU : APDU
    {
        #region Properties

        public CommandControlField ControlField { get; set; }

        public byte[] CRCData => ControlField.Bytes.Concat(new[] { Length }).Concat(Data).ToArray();

        #endregion

        #region Constructor

        public CommandAPDU(CommandControlField controlField, byte[] bytData) :
            base(bytData)
        {
            ControlField = controlField;
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