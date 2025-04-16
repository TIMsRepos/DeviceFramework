using System;

namespace TIM.Devices.Framework.Database
{
    public static class DatabaseHelper
    {
        public static string MaxLength(this string value, int maxLength)
        {
            if (String.IsNullOrWhiteSpace(value))
                return "";

            if (value.Length > maxLength)
                return value.Substring(0, maxLength).Trim();

            return value.Trim();
        }
    }
}