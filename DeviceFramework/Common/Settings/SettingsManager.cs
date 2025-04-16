using System;
using System.Windows.Forms;
using TIM.Common.CoreStandard;
using TIM.Common.Data.Factories;
using Subsidiary = TIM.Common.Data.Entities.Subsidiary;

namespace TIM.Devices.Framework.Common.Settings
{
    /// <summary>
    /// Helps loading global and computer specific settings
    /// </summary>
    public class SettingsManager
    {
        #region Fields

        private static int? _gatAccess6200MediaDetectTimeout;

        #endregion

        #region Properties

        public static Enums.Architecture DetectArchitecture()
        {
            if (IntPtr.Size == 4)
                return Enums.Architecture.x86;
            else if (IntPtr.Size == 8)
                return Enums.Architecture.x64;
            else
                throw new SettingsException("Unknown architecture detected!");
        }

        public static string ComputerName => SystemInformation.ComputerName;

        public static string WindowsUserName => SystemInformation.UserName + "@" + SystemInformation.UserDomainName;

        public static int GatAccess6200MediaDetectTimeout => GetGatAccess6200MediaDetectTimeout();

        #endregion

        #region Methods

        /// <summary>
        /// Converts a value to its target type
        /// </summary>
        /// <typeparam name = "T" > The target type to convert to</typeparam>
        /// <param name = "objValue" > The object to convert</param>
        /// <param name="throwExceptionIfEmpty"></param>
        /// <returns>The converted object as an object of type of the target type</returns>
        public static T Convert<T>(object objValue, bool throwExceptionIfEmpty)
        {
            if (objValue == null && throwExceptionIfEmpty)
                throw new NullReferenceException();

            if (typeof(T) == typeof(bool))
            {
                if (objValue == null ||
                    string.IsNullOrEmpty(objValue.ToString()))
                    return (T)(object)false;
                return (T)(object)BitConverter.ToBoolean(new byte[] { byte.Parse(objValue.ToString()) }, 0);
            }
            else
                return (T)System.Convert.ChangeType(objValue, typeof(T));
        }

        /// <summary>
        /// Get Computer detail setting value as bool, if the entry is not found, the predefined will be returned
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <param name="predefined"></param>
        /// <returns></returns>
        public static bool GetBoolOrPredefined(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting computerDetailSettingID, bool predefined)
        {
            var bOkSetting = DbControllers.ComputerSettings.TryGetValue(computerSettingID, computerDetailSettingID, ComputerName, out var stringValue);
            return (bOkSetting && stringValue.Trim() == "1") || predefined;
        }

        /// <summary>
        /// Check if the computer detail setting is empty
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <returns></returns>
        public static bool Empty(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting computerDetailSettingID)
        {
            var bOk = TryGet(computerSettingID, computerDetailSettingID, out var stringValue);
            return !bOk || string.IsNullOrWhiteSpace(stringValue);
        }

        /// <summary>
        /// Check if all computer detail settings are empty
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingIDs"></param>
        /// <returns></returns>
        public static bool EmptyAll(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting[] computerDetailSettingIDs)
        {
            bool blnEmptyAll = true;

            foreach (var computerDetailSettingID in computerDetailSettingIDs)
            {
                if (!Empty(computerSettingID, computerDetailSettingID))
                {
                    blnEmptyAll = false;
                    break;
                }
            }

            return blnEmptyAll;
        }

        /// <summary>
        /// Check if one of the computer settings is empty
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingIDs"></param>
        /// <returns></returns>
        public static bool EmptyAny(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting[] computerDetailSettingIDs)
        {
            foreach (var computerDetailSettingID in computerDetailSettingIDs)
            {
                if (Empty(computerSettingID, computerDetailSettingID))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get computer detail setting value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <returns></returns>
        public static string GetValue(Enums.SettingDetail settingDetailID)
        {
            return DbControllers.SettingDetails.GetValueFromCache(settingDetailID);
        }

        /// <summary>
        /// Get computer detail setting value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <param name="throwExceptionIfEmpty"></param>
        /// <returns></returns>
        public static T GetValue<T>(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting computerDetailSettingID, bool throwExceptionIfEmpty = true)
        {
            var stringValue = DbControllers.ComputerSettings.GetValue(throwExceptionIfEmpty, computerSettingID, computerDetailSettingID, ComputerName);
            return Convert<T>(stringValue, throwExceptionIfEmpty);
        }

        public static Subsidiary ComputerSubsidiary
        {
            get
            {
                var bOkSubsidiaryID = DbControllers.ComputerSettings.TryGetValue(Enums.ComputerSetting.ComputerLocation, Enums.ComputerDetailSetting.SubsidiaryID,
                    ComputerName, out var stringSubsidiaryID);
                if (!bOkSubsidiaryID)
                    throw new Exception("Es wurde keine Anlage für diesen Computer hinterlegt.");
                if (int.TryParse(stringSubsidiaryID, out var intSubsidiaryID) == false
                    || intSubsidiaryID < 0)
                    throw new Exception("Die ID der Anlage dieses Computers ist ungültig.");

                var MyComputerSubsidiary = DbControllers.Subsidiaries.GetByID(intSubsidiaryID);
                if (MyComputerSubsidiary == null)
                    throw new Exception("Die Anlage dieses Computers ist ungültig.");
                return MyComputerSubsidiary;
            }
        }

        /// <summary>
        /// Get Computer detail setting value as uint, if the entry is not found, the predefined will be returned
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <param name="predefined"></param>
        /// <returns></returns>
        public static uint GetUintOrPredefined(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting computerDetailSettingID, uint predefined)
        {
            var stringValue = GetValue<string>(computerSettingID, computerDetailSettingID);
            var bOk = uint.TryParse(stringValue, out var uintValue);
            return bOk
                ? uintValue
                : predefined;
        }

        /// <summary>
        /// Try get computer detail setting
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGet(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting computerDetailSettingID, out string value)
        {
            return DbControllers.ComputerSettings.TryGetValue(computerSettingID, computerDetailSettingID, ComputerName, out value);
        }

        /// <summary>
        /// Try get setting detail
        /// </summary>
        /// <param name="settingDetailID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGet(Enums.SettingDetail settingDetailID, out string value)
        {
            return DbControllers.SettingDetails.TryGetValueFromCache(settingDetailID, out value);
        }

        /// <summary>
        /// Try get setting detail
        /// </summary>
        /// <param name="settingDetailID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGet(Enums.SettingDetail settingDetailID, out int value)
        {
            var bOk = DbControllers.SettingDetails.TryGetValueFromCache(settingDetailID, out var stringValue);
            if (!bOk)
            {
                value = 0;
                return false;
            }

            var bOkInt = int.TryParse(stringValue, out value);
            return bOkInt;
        }

        /// <summary>
        /// Check if a computer setting id exists
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingID"></param>
        /// <returns></returns>
        public static bool Exists(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting computerDetailSettingID)
        {
            var bOK = DbControllers.ComputerSettings.TryGetValue(computerSettingID, computerDetailSettingID, ComputerName, out var value);
            return bOK && !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Check if all given computer setting ids exists
        /// </summary>
        /// <param name="computerSettingID"></param>
        /// <param name="computerDetailSettingIDs"></param>
        /// <returns></returns>
        public static bool ExistsAll(Enums.ComputerSetting computerSettingID, Enums.ComputerDetailSetting[] computerDetailSettingIDs)
        {
            bool blnExistsAll = true;

            foreach (var computerDetailSettingID in computerDetailSettingIDs)
            {
                if (!Exists(computerSettingID, computerDetailSettingID))
                {
                    blnExistsAll = false;
                    break;
                }
            }

            return blnExistsAll;
        }

        private static int GetGatAccess6200MediaDetectTimeout()
        {
            if (!_gatAccess6200MediaDetectTimeout.HasValue)
            {
                var defaultTimeout = 3000;
                var result = TryGet(Enums.ComputerSetting.TIM_Devices_GantnerAccess6200_Access6200, Enums.ComputerDetailSetting.GatAccess6200MediaDetectTimeout, out var timeoutValue);
                if (!result ||
                    string.IsNullOrWhiteSpace(timeoutValue))
                    _gatAccess6200MediaDetectTimeout = defaultTimeout;
                else
                {
                    var resultTryParse = int.TryParse(timeoutValue, out var timeout);
                    _gatAccess6200MediaDetectTimeout = !resultTryParse || timeout < 1
                        ? defaultTimeout
                        : timeout * 1000;
                }
            }

            return _gatAccess6200MediaDetectTimeout.Value;
        }

        #endregion
    }
}