using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Helper
{
    public static class CRCHelper
    {
        public static byte[] CalculateCRCBytes(byte[] data)
        {
            var crc = CalculateCRC(data);
            return BitConverter.GetBytes(crc);
        }

        public static ushort CalculateCRC(byte[] data)
        {
            ushort crc = 0;
            foreach (var b in data)
            {
                CRC(b, ref crc);
            }
            return crc;
        }

        private static void CRC(byte b, ref ushort crc)
        {
            for (var i = 0; i < 8; i++, b >>= 1)
            {
                var c1 = (byte)(b ^ (byte)crc);
                crc >>= 1;
                if ((c1 & 1) != 0)
                {
                    crc ^= 0x8408;
                }
            }
        }
    }
}