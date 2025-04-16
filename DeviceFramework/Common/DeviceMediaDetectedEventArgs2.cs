using System;
using System.Collections.Generic;

namespace TIM.Devices.Framework.Common
{
    public class DeviceMediaDetectedEventArgs2 : EventArgs
    {
        #region Properties

        public IDevice Device { get; set; }

        public Dictionary<Payloads, object> Payload { get; }

        public int DeviceFrameworkDeviceID { get; }

        #endregion

        #region Constructors

        public DeviceMediaDetectedEventArgs2(IDevice MyDevice, Dictionary<Payloads, object> dicPayload, int deviceFrameworkDeviceID)
        {
            Payload = dicPayload;
            Device = MyDevice;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion
    }
}