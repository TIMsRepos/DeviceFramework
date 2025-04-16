using System;
using System.Threading;

namespace TIM.Devices.Framework.Common
{
    public static class Workflows
    {
        /// <summary>
        /// Invokes the given Action with a timeout, when timeout is reached before finishing it throws a TimeoutException
        /// </summary>
        /// <param name="MyAction">The Action to invoke</param>
        /// <param name="intTimeout">The timeout in milliseconds</param>
        /// <exception cref="TimeoutException">Gets thrown when timeout is reached before the invoked Action finished</exception>
        public static void InvokeWithTimeout(Action MyAction, int intTimeout)
        {
            IAsyncResult MyAsyncResult = MyAction.BeginInvoke(null, null);
            if (MyAsyncResult.AsyncWaitHandle.WaitOne(intTimeout))
                MyAction.EndInvoke(MyAsyncResult);
            else
            {
                throw new TimeoutException(string.Format(
                    "Execution of '{0}' took longer than '{1}'",
                    MyAction.Method.DeclaringType.FullName + "." + MyAction.Method.Name,
                    new TimeSpan(0, 0, 0, 0, intTimeout).ToString()));
            }
        }

        public static TResult TryOrPredefined<TResult>(Func<TResult> MyFunc, TResult MyPredefinedValue)
        {
            try
            {
                return MyFunc();
            }
            catch
            {
                return MyPredefinedValue;
            }
        }
    }
}