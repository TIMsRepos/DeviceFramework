using System;
using System.Collections.Generic;
using System.Linq;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.Helper;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.Database;

namespace TIM.Devices.Framework.Turnstiles
{
    public static class TurnstileHelper
    {
        private static int[] GetDeviceDetailIDs()
        {
            string deviceDetailIDs = SettingsManager.GetValue<string>(Enums.ComputerSetting.TurnStiles, Enums.ComputerDetailSetting.DeviceDetailIDs, false)?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(deviceDetailIDs))
                return new int[] { };

            if (deviceDetailIDs[deviceDetailIDs.Length - 1] == ';')
                deviceDetailIDs = deviceDetailIDs.Substring(0, deviceDetailIDs.Length - 1);
            string[] strDeviceDetailIDsParts = deviceDetailIDs.Split(new[] { ';' }, StringSplitOptions.None);
            int[] intDeviceDetailIDs = strDeviceDetailIDsParts
                .ConvertIEnumerable(int.Parse)
                .ToArray();

            return intDeviceDetailIDs;
        }

        private static string[] GetDeviceSoftwareComponents(int[] intDeviceDetailIDs)
        {
            string[] deviceSoftwareComponents = new string[intDeviceDetailIDs.Length];

            using (TIMDataClassesDataContext MyCtx = DBContext.DataContextRead)
            {
                for (int i = 0; i < intDeviceDetailIDs.Length; ++i)
                {
                    var device = (from MyDevDetail in MyCtx.DevicesDetail
                                  where MyDevDetail.DeviceDetailID == intDeviceDetailIDs[i]
                                  select new { MyDevDetail.RequiredSoftwareComponent }).Single();

                    deviceSoftwareComponents[i] = device.RequiredSoftwareComponent;
                }
            }

            return deviceSoftwareComponents;
        }

        private static IDevice[] GetDeviceInstances(string[] softwareComponents)
        {
            IDevice[] devices = new IDevice[softwareComponents.Length];

            for (int i = 0; i < softwareComponents.Length; ++i)
                devices[i] = DeviceManager.GetDevice(softwareComponents[i]);

            return devices;
        }

        public static IEnumerable<KeyValuePair<int, ConfigString>> GetConfigStrings()
        {
            int[] deviceDetailIDs = GetDeviceDetailIDs();
            string[] deviceSoftwareComponents = GetDeviceSoftwareComponents(deviceDetailIDs);
            IDevice[] deviceInstances = GetDeviceInstances(deviceSoftwareComponents);
            string turnstileConfigStringsRaw = SettingsManager.GetValue<string>(Enums.ComputerSetting.TurnStiles, Enums.ComputerDetailSetting.ConfigStrings, false)?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(turnstileConfigStringsRaw))
                return new List<KeyValuePair<int, ConfigString>>();

            if (turnstileConfigStringsRaw[turnstileConfigStringsRaw.Length - 1] == ';')
            {
                turnstileConfigStringsRaw = turnstileConfigStringsRaw.Substring(0, turnstileConfigStringsRaw.Length - 1);
            }
            string[] turnstileConfigStrings = turnstileConfigStringsRaw.Split(new[] { ';' }, StringSplitOptions.None);

            var list = new List<KeyValuePair<int, ConfigString>>();
            for (int i = 0; i < turnstileConfigStrings.Length; ++i)
            {
                ConfigString MyConfigString = ConfigString.Parse(turnstileConfigStrings[i], '=', '|', deviceInstances[i].ConfigStringDescriptions);
                list.Add(new KeyValuePair<int, ConfigString>(deviceDetailIDs[i], MyConfigString));
            }

            return list;
        }
    }
}