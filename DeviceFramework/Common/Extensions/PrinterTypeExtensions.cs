using System;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common.Printer.PrinterEnums;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class PrinterTypeExtensions
    {
        public static Enums.ComputerDetailSetting ToComputerDetailSetting(this PrinterTypeFrontend printerTypeFrontend)
        {
            var bOk = Enum.TryParse(printerTypeFrontend.ToString(), out Enums.ComputerDetailSetting computerDetailSetting);
            if (!bOk)
                throw new Exception($"Die Arbeitsplatzeinstellung {printerTypeFrontend.ToString()} konnte nicht gefunden werden.");
            return computerDetailSetting;
        }
    }
}