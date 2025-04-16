namespace TIM.Devices.Framework
{
    /// <summary>
    /// Specifies the data source for different managers (e.g. ObjectManager for TypeCatalog, SettingsManager)
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// Defines the source as a xml file
        /// </summary>
        XML,

        /// <summary>
        /// Defines the source as a database
        /// </summary>
        DB
    }
}