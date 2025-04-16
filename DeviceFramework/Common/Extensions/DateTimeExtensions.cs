using System;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns a string that contains all part of the DateTime struct, from the year down to the seven-digit microseconds
        /// </summary>
        /// <param name="dtDateTime"></param>
        /// <returns></returns>
        public static string ToFullString(this DateTime dtDateTime)
        {
            return dtDateTime.ToString("dd.MM.yyyy HH:mm:ss.fffffff");
        }

        public static string ToFullTimeString(this DateTime dtDateTime)
        {
            return dtDateTime.ToString("HH:mm:ss.fffffff");
        }

        /// <summary>
        /// Adds the specified number of weeks to the value of this instance.
        /// </summary>
        /// <param name="dtDateTime">The DateTime struct to operate on</param>
        /// <param name="dblWeeks">A number of whole and fractional weeks. The value parameter can be negative or positive.</param>
        /// <returns>A DateTime whose value is the sum of the date and time represented by this instance and the number of weeks represented by value.</returns>
        public static DateTime AddWeeks(this DateTime dtDateTime, double dblWeeks)
        {
            return dtDateTime.AddDays(dblWeeks * 7f);
        }
    }
}