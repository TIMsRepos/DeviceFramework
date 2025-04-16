using System;
using System.DirectoryServices;
using TIM.Common.CoreStandard;
using TIM.Common.Data.Factories;

namespace TIM.Devices.Framework.UserAccountControl
{
    internal static class UACHelper
    {
        #region Fields

        private static Enums.EntryLocation? _currentEntryLocation;
        private static string _activeDirectoryName;

        #endregion

        #region Getter

        public static Enums.EntryLocation CurrentEntryLocation
        {
            get
            {
                if (_currentEntryLocation.HasValue == false)
                    _currentEntryLocation = string.IsNullOrWhiteSpace(ActiveDirectoryName) ? Enums.EntryLocation.LocalMachine : Enums.EntryLocation.ActiveDirectory;

                return _currentEntryLocation.Value;
            }
        }

        private static string ActiveDirectoryName => _activeDirectoryName
                             ?? (_activeDirectoryName = DbControllers.SettingDetails.GetValueFromCache(Enums.SettingDetail.ADName) ?? "");

        public static DirectoryEntry UACDirectoryEntry =>
            CurrentEntryLocation == Enums.EntryLocation.ActiveDirectory
                ? new DirectoryEntry("LDAP://" + ActiveDirectoryName)
                : new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

        #endregion
    }
}