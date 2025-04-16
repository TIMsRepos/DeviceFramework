namespace TIM.Devices.Framework.Common.Settings
{
    [global::System.Serializable]
    public class SettingNotFoundException : SettingsException
    {
        public SettingNotFoundException(string message) : base(message)
        {
        }
    }
}