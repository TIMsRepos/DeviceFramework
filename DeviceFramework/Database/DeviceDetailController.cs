using System.Collections.Generic;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.Helper;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.Framework.Database
{
    public static class DeviceDetailController
    {
        public static List<DeviceDetail> GetDeviceDetails()
        {
            return TIM.Common.Data.Factories.DbControllersCache.DevicesDetail.GetAll();
        }

        public static List<int> GetUsedDeviceDetailIDs()
        {
            var usedDeviceDetailIDs = SettingsManager.GetValue<string>(Enums.ComputerSetting.TurnStiles, Enums.ComputerDetailSetting.DeviceDetailIDs, false);

            var convertedUsedDeviceDetailIDs = new List<int>();

            foreach (var usedDeviceDetailsID in usedDeviceDetailIDs.Separate(";", false))
            {
                if (int.TryParse(usedDeviceDetailsID, out int result))
                    convertedUsedDeviceDetailIDs.Add(result);
            }

            return convertedUsedDeviceDetailIDs;
        }
    }
}