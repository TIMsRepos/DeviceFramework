using System;
using System.Windows.Forms;
using TIM.Common.Data.Factories;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.UserAccountControl;
using Enums = TIM.Common.CoreStandard.Enums;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class ExceptionExtensions
    {
        private static string CurrentUserName => UACEntry.GetFullName(SystemInformation.UserName.Trim());

        public static void LogInfo(this Exception ex)
        {
            Log(ex, Enums.LogLevel.Info);
        }

        public static void LogWarning(this Exception ex)
        {
            Log(ex, Enums.LogLevel.Warning);
        }

        private static void Log(Exception ex, Enums.LogLevel level)
        {
            var subsidiaryName = Workflows.TryOrPredefined(() => SettingsManager.ComputerSubsidiary.SubsidiaryName, "ERROR");
            var currentUser = Workflows.TryOrPredefined(() => CurrentUserName, SettingsManager.WindowsUserName + "(FALLBACK)");
            switch (level)
            {
                case Enums.LogLevel.Info:
                    DbControllers.Crashes.LogInfo(ex, currentUser, subsidiaryName);
                    break;

                case Enums.LogLevel.Warning:
                    DbControllers.Crashes.LogWarning(ex, currentUser, subsidiaryName);
                    break;

                case Enums.LogLevel.Error:
                    DbControllers.Crashes.LogError(ex, currentUser, subsidiaryName);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}