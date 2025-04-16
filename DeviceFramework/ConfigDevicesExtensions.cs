using TIM.Common.CoreStandard;

namespace TIM.Devices.Framework
{
    public static class ConfigDevicesExtensions
    {
        public static bool Contains(this Enums.ConfigDevices MyConfigDevices, Enums.ConfigDevices MyConfigDevice)
        {
            return ((MyConfigDevices & MyConfigDevice) == MyConfigDevice);
        }
    }
}