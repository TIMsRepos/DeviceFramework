using System.Configuration;

namespace TIM.Devices.Framework.Common.Settings
{
    public static class ConfigManager
    {
        public static bool ExistsAppSetting(string strName, Configuration MyConfig = null)
        {
            if (MyConfig == null)
                MyConfig = TIM.Devices.Framework.Database.DBContext.GetDefaultConfiguration();

            KeyValueConfigurationElement MyConfigElement = MyConfig.AppSettings.Settings[strName];
            return MyConfig != null;
        }

        public static T GetAppSetting<T>(string strName, Configuration MyConfig = null)
        {
            if (MyConfig == null)
                MyConfig = TIM.Devices.Framework.Database.DBContext.GetDefaultConfiguration();

            KeyValueConfigurationElement MyConfigElement = MyConfig.AppSettings.Settings[strName];
            if (MyConfig == null)
            {
                return default(T);
            }
            else
            {
                T MyValue = SettingsManager.Convert<T>(MyConfigElement.Value);
                return MyValue;
            }
        }
    }
}