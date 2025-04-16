using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Database;
using TIM.Devices.Framework.UserAccountControl.ActiveDirectory;

namespace TIM.Devices.Framework.UserAccountControl
{
    internal static class UACEntry
    {
        #region Static methods

        /// <summary>
        /// Returns DisplayName if AD is used, else the StandardName
        /// </summary>
        /// <param name="adLoginName"></param>
        /// <returns></returns>
        public static string GetFullName(string adLoginName)
        {
            switch (UACHelper.CurrentEntryLocation)
            {
                case Enums.EntryLocation.ActiveDirectory:
                    return ADEntry.GetDisplayName(adLoginName);

                case Enums.EntryLocation.LocalMachine:
                    {
                        var user = QryServiceEmployeeController.GetByADLoginName(adLoginName);
                        return user == null ? "" : user.StandardName ?? "";
                    }
                default:
                    return "";
            }
        }

        #endregion
    }
}