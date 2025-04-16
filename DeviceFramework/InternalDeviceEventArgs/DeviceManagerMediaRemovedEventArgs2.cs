using System.Collections.Generic;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework
{
    public class DeviceManagerMediaRemovedEventArgs2 : DeviceManagerMediaDetectedEventArgs2
    {
        public DeviceManagerMediaRemovedEventArgs2(IDevice MyDevice, int intDeviceID, Dictionary<Payloads, object> dicPayload, int deviceFrameworkDeviceID)
            : base(MyDevice, intDeviceID, dicPayload, deviceFrameworkDeviceID)
        {
        }
    }
}