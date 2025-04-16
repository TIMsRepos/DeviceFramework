using System.Collections.Generic;

namespace TIM.Devices.Framework.Common
{
    public class DeviceMediaRemovedEventArgs2 : DeviceMediaDetectedEventArgs2
    {
        public DeviceMediaRemovedEventArgs2(IDevice MyDevice, Dictionary<Payloads, object> dicPayload, int deviceFrameworkDeviceID)
            : base(MyDevice, dicPayload, deviceFrameworkDeviceID)
        {
        }
    }
}