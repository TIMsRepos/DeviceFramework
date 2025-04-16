using System;

namespace TIM.Devices.Framework
{
    [Flags()]
    public enum ConfigDevices : uint
    {
        None = 0,
        External = 1,
        InHouse = 2,
        Employees = 4,
        All = ConfigDevices.External | ConfigDevices.InHouse | ConfigDevices.Employees,
        ExternalOrInHouse = ConfigDevices.External | ConfigDevices.InHouse
    }
}