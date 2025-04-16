using System;
using System.Collections.Generic;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> Convert<T, TResult>(this IEnumerable<T> MyEnumerable, Func<T, TResult> MyConvertFunc)
        {
            foreach (T MyObj in MyEnumerable)
                yield return MyConvertFunc(MyObj);
        }
    }
}