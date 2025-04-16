using System;

namespace TIM.Devices.Framework.Common
{
    [Flags]
    public enum DeviceCapabilities : int
    {
        None = 0x0,
        Identification = 0x1,
        Vouchers = 0x2
    }
}