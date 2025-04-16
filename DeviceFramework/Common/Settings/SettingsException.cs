namespace TIM.Devices.Framework.Common.Settings
{
    [global::System.Serializable]
    public class SettingsException : FrameworkException
    {
        public SettingsException(string strMessage) : base(strMessage)
        {
        }
    }
}