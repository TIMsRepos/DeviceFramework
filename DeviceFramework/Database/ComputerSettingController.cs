using System;
using System.Linq;
using TIM.Common.CoreStandard.Helper;

namespace TIM.Devices.Framework.Database
{
    public static class ComputerSettingController
    {
        public static void SaveComputerSetting(string computerNameID, string computerSettingID, string computerDetailSettingID, string value)
        {
            using (var ctx = DBContext.DataContextEdit)
            {
                var setting = ctx
                    .ComputerSettings
                    .FirstOrDefault(s =>
                        s.ComputerNameID == computerNameID &&
                        s.ComputerSettingID == computerSettingID &&
                        s.ComputerDetailSettingID == computerDetailSettingID);

                if (setting == null)
                {
                    setting = new ComputerSetting();
                    setting.ComputerNameID = computerNameID;
                    setting.ComputerSettingID = computerSettingID;
                    setting.ComputerDetailSettingID = computerDetailSettingID;
                    setting.Value = !String.IsNullOrWhiteSpace(value) ? value.Trim().MaxLength(100) : "";
                    ctx.ComputerSettings.InsertOnSubmit(setting);
                }
                else
                {
                    setting.Value = !String.IsNullOrWhiteSpace(value) ? value.Trim().MaxLength(100) : "";
                }
                DBContext.SubmitChanges(ctx);
            }
        }
    }
}