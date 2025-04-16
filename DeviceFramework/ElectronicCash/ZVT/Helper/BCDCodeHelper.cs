using System;
using System.Text;
using TIM.Devices.Framework.Common.Extensions;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Helper
{
    /// <summary>
    /// Helps parsing Binary Coded Decimals (BCD)
    /// </summary>
    public static class BCDCodeHelper
    {
        /// <summary>
        /// Converts a decimal number to a bcd data byte-array
        /// </summary>
        /// <param name="lngDec">The decimal number</param>
        /// <returns>A bcd data byte-array</returns>
        public static byte[] Dec2Bcd(long lngDec)
        {
            var bytDigits = ExtractDecDigits(lngDec);

            // Expand to multiple of 2 digits
            if (bytDigits.Length % 2 == 1)
            {
                bytDigits = bytDigits.ExpandPrefixed(bytDigits.Length + 1);
            }

            return DecDigits2Bcd(bytDigits);
        }

        /// <summary>
        /// Converts an array of decimal digits to bcd data byte-array
        /// </summary>
        /// <param name="bytDecDigits">The array of decimal digits</param>
        /// <returns>A bcd data byte-array</returns>
        /// <remarks>The decimal digits array length needs to be multiple of 2</remarks>
        public static byte[] DecDigits2Bcd(byte[] bytDecDigits)
        {
            // Convert pairs of 2 digits to bcd
            var bytBcd = new byte[bytDecDigits.Length / 2];
            for (int i = 0, j = 0; i < bytDecDigits.Length; i += 2, ++j)
            {
                bytBcd[j] = (byte)((bytDecDigits[i] << 4) | bytDecDigits[i + 1]);
            }

            return bytBcd;
        }

        /// <summary>
        /// Converts a bcd data byte-array to a decimal number
        /// </summary>
        /// <param name="bytBcd">The bcd data byte-array</param>
        /// <returns>A decimal number</returns>
        public static long Bcd2Dec(byte[] bytBcd)
        {
            long lngRes = 0;

            foreach (var t in bytBcd)
            {
                var intDigit1 = t >> 4;
                var intDigit2 = t & 0xF;
                if (intDigit1 > 9 || intDigit2 > 9)
                {
                    throw new ArgumentOutOfRangeException("BCD Code only allows hex values in range of 0-9");
                }
                lngRes = (lngRes * 100) + intDigit1 * 10 + intDigit2;
            }

            return lngRes;
        }

        /// <summary>
        /// Converts a bcd data byte-array to a decimal number
        ///
        /// for some values, e.g. PAN/EF-ID, a "F" may be added for unused digits,
        /// these need to be skipped
        /// </summary>
        /// <param name="bytBcd">The bcd data byte-array</param>
        /// <returns>A decimal number</returns>
        public static long Bcd2DecWithFiller(byte[] bytBcd)
        {
            var strResult = new StringBuilder();
            foreach (var t in bytBcd)
            {
                var digit1 = t >> 4;
                var digit2 = t & 0xF;

                if (digit1 < 10)
                {
                    strResult.Append(digit1.ToString());
                }
                if (digit2 < 10)
                {
                    strResult.Append(digit2.ToString());
                }
            }

            long lngRes;
            if (long.TryParse(strResult.ToString(), out lngRes))
            {
                return lngRes;
            }
            throw new InvalidCastException("BCD value with filler could not be parsed to a number");
        }

        public static T Bcd2Dec<T>(byte[] bytData, int intIndex)
        {
            return Bcd2Dec<T>(bytData, intIndex, 1);
        }

        public static T Bcd2Dec<T>(byte[] bytData, int intIndex, int intLength)
        {
            var bytPart = new byte[intLength];
            Array.Copy(bytData, intIndex, bytPart, 0, intLength);
            var lngValue = Bcd2Dec(bytPart);
            return (T)Convert.ChangeType(lngValue, typeof(T));
        }

        /// <summary>
        /// Extract digits of a decimals number
        /// </summary>
        /// <param name="lngDec">The decimal number</param>
        /// <returns>A byte-array containing all digits</returns>
        public static byte[] ExtractDecDigits(long lngDec)
        {
            var intMultipleOfTen = 1;
            var intDigits = 1;
            while (lngDec / intMultipleOfTen >= 10.0f)
            {
                intMultipleOfTen *= 10;
                ++intDigits;
            }
            var digits = new byte[intDigits];
            for (var i = 0; intMultipleOfTen > 0; ++i, intMultipleOfTen /= 10)
            {
                digits[i] = (byte)(lngDec / intMultipleOfTen);
                lngDec -= digits[i] * intMultipleOfTen;
            }

            return digits;
        }
    }
}