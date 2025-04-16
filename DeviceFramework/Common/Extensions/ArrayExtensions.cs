using System;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] ExpandPrefixed<T>(this T[] MyArray, int intNewLength)
        {
            if (intNewLength < MyArray.Length)
                throw new ArgumentOutOfRangeException("NewLength paramter needs equal or greater than the existing array lengh");

            if (intNewLength == MyArray.Length)
                return MyArray;

            T[] objTmp = new T[intNewLength];
            for (int i = MyArray.Length - 1, j = objTmp.Length - 1; i > -1; --i, --j)
                objTmp[j] = MyArray[i];

            return objTmp;
        }

        public static T[] Concat<T>(this T[] MyArray, params T[] MyAddArray)
        {
            T[] objTmp = new T[MyArray.Length + MyAddArray.Length];

            for (int i = 0; i < MyArray.Length; ++i)
                objTmp[i] = MyArray[i];
            for (int i = MyArray.Length, j = 0; j < MyAddArray.Length; ++i, ++j)
                objTmp[i] = MyAddArray[j];

            return objTmp;
        }
    }
}