using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework
{
    public class DeviceManagerEventArgs : DeviceEventArgs
    {
        #region Members

        private int intDeviceID;

        #endregion

        #region Getter

        public int DeviceID
        {
            get { return intDeviceID; }
        }

        #endregion

        #region Constructors

        public DeviceManagerEventArgs(IDevice MyDevice, int intDeviceID, string strMediaID)
            : base(MyDevice, strMediaID)
        {
            this.intDeviceID = intDeviceID;
        }

        #endregion
    }
}