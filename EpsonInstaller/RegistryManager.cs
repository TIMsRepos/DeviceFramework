using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework
{
    public class RegistryManager
    {
        private const string strCompanyPath = @"SOFTWARE\MeridianSpa GmbH\";
        private const string strMembersReloaded_Settings = @"Members Reloaded\Settings";
        private const string strMembersReloaded_Settings_Printer = strMembersReloaded_Settings + @"\Printer";
        private const string strTIMsFrontend_Settings = @"TIMs-Frontend\Settings";

        //public static string CompanyPath { get { return strCompanyPath; } }
        //public static string MembersReloaded_Settings { get { return strMembersReloaded_Settings; } }
        //public static string MembersReloaded_Settings_Printer { get { return strMembersReloaded_Settings_Printer; } }
        //public static string TIMsFrontend_Settings { get { return strTIMsFrontend_Settings; } }

        /// <summary>
        /// Takes an installation path of one TIM related application and tries
        /// to guess parallel other application installation paths
        /// </summary>
        /// <param name="dirSample">The install path of one TIM related application</param>
        public static IEnumerable<DirectoryInfo> GuessInstallPaths(DirectoryInfo dirSample)
        {
            DirectoryInfo dirParent = dirSample.Parent;
            foreach (DirectoryInfo dirSub in dirParent.GetDirectories())
            {
                if (dirSub == dirSample)
                    continue;

                FileInfo[] files = dirSub.GetFiles("TIM-Administrator.exe.config");
                yield return dirSub;
            }
        }

        public static DirectoryInfo GetInstallPath(Applications MyApp)
        {
            switch (MyApp)
            {
                case Applications.Administrator:
                    {
                        string strPath = RegistryManager.GetValue<string>(strCompanyPath + @"\Members Reloaded", "TargetDir", Registry.LocalMachine);
                        return string.IsNullOrEmpty(strPath) ? null : new DirectoryInfo(strPath);
                    }
                case Applications.Frontend:
                    {
                        string strPath = RegistryManager.GetValue<string>(strCompanyPath + @"\TIMs-Frontend", "TargetDir", Registry.LocalMachine);
                        return string.IsNullOrEmpty(strPath) ? null : new DirectoryInfo(strPath);
                    }
                case Applications.Devices:
                    {
                        string strPath = RegistryManager.GetValue<string>(strCompanyPath + @"\TIMs-Devices", "TargetDir", Registry.LocalMachine);
                        return string.IsNullOrEmpty(strPath) ? null : new DirectoryInfo(strPath);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool IsInstalled(Applications MyApp)
        {
            return GetInstallPath(MyApp) != null;
        }

        [Obsolete("USE GetInstallPath(Applications) INSTEAD")]
        public static DirectoryInfo InstallPath
        {
            get
            {
                string strTargetDir = RegistryManager.GetValue<string>(strCompanyPath + @"\TIMs-Devices", "TargetDir", Registry.LocalMachine);
                if (strTargetDir == null)
                    return null;
                else
                    return new DirectoryInfo(strTargetDir);
            }
        }

        public static bool BalloonTips
        {
            get
            {
                try
                {
                    RegistryKey regBranch = Registry.CurrentUser;
                    regBranch = regBranch.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", false);
                    return Convert.ToBoolean(regBranch.GetValue("EnableBalloonTips", false));
                }
                catch
                {
                    return false;
                }
            }
        }

        public static T GetValue<T>(string strRegPath, string strRegValueName, RegistryKey regBranch)
        {
            try
            {
                regBranch = regBranch.OpenSubKey(strRegPath, false);
                object tmp = regBranch.GetValue(strRegValueName);
                if (tmp == null)
                    return default(T);
                return (T)Convert.ChangeType(tmp, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static bool SetValue<T>(string strRegPath, string strRegValueName, RegistryKey regBranch, T objRegValue)
        {
            try
            {
                regBranch = regBranch.CreateSubKey(strRegPath);
                regBranch.SetValue(strRegValueName, objRegValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetBonPrinterName()
        {
            return GetValue<string>(strCompanyPath + strMembersReloaded_Settings_Printer, "BonPrinter_Name", Registry.LocalMachine);
        }

        public static bool SetValueForFrontend<T>(string strRegValueName, T objRegValue)
        {
            return SetValue<T>(strCompanyPath + strTIMsFrontend_Settings, strRegValueName, Registry.CurrentUser, objRegValue);
        }

        public static T GetValueForFrontend<T>(string strRegValueName)
        {
            return GetValue<T>(strCompanyPath + strTIMsFrontend_Settings, strRegValueName, Registry.CurrentUser);
        }
    }
}