using System.Collections.Generic;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework
{
    public class DeviceManagerMediaDetectedEventArgs2 : DeviceMediaDetectedEventArgs2
    {
        #region Getter

        public int DeviceID { get; }

        public int DeviceFrameworkDeviceID { get; }

        #endregion

        #region Constructors

        public DeviceManagerMediaDetectedEventArgs2(IDevice MyDevice, int intDeviceID, Dictionary<Payloads, object> dicPayload, int deviceFrameworkDeviceID)
            : base(MyDevice, dicPayload, deviceFrameworkDeviceID)
        {
            DeviceID = intDeviceID;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion
    }
}