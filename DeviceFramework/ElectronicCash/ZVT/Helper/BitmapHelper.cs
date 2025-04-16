using System;
using TIM.Common.CoreStandard;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Helper
{
    /// <summary>
    /// Contains all Command Bitmaps for the ZVT Protocol
    /// </summary>
    public static class BitmapHelper
    {
        public static void GetCommandBytes(Enums.ZVTCommands command, out byte firstByte, out byte secondByte)
        {
            switch (command)
            {
                case Enums.ZVTCommands.Registration:
                    firstByte = 0x06;
                    secondByte = 0x00;
                    break;

                case Enums.ZVTCommands.Completion:
                    firstByte = 0x06;
                    secondByte = 0x0F;
                    break;

                case Enums.ZVTCommands.Authorization:
                    firstByte = 0x06;
                    secondByte = 0x01;
                    break;

                case Enums.ZVTCommands.LogOff:
                    firstByte = 0x06;
                    secondByte = 0x02;
                    break;

                case Enums.ZVTCommands.DisplayText:
                    firstByte = 0x06;
                    secondByte = 0x1E;
                    break;

                case Enums.ZVTCommands.AcknowledgmentResponse:
                    firstByte = 0x80;
                    secondByte = 0x00;
                    break;

                case Enums.ZVTCommands.NotSucceededResponse:
                    firstByte = 0x84;
                    secondByte = 0x01;
                    break;

                case Enums.ZVTCommands.Abort:
                    firstByte = 0x06;
                    secondByte = 0x1E;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }
        }
    }
}