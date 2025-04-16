using System;
using System.ComponentModel;
using System.Threading;

namespace TIM.Devices.Framework.Common
{
    public class EventHelper
    {
        public static void Fire(object MySender, EventHandler MyHandler, EventArgs MyArgs)
        {
            EventHandler MyHandlerTmp = MyHandler;
            if (MyHandlerTmp != null)
                MyHandlerTmp(MySender, MyArgs);
        }

        public static void Fire<T>(object MySender, EventHandler<T> MyHandler, T MyArgs) where T : EventArgs
        {
            EventHandler<T> MyHandlerTmp = MyHandler;
            if (MyHandlerTmp != null)
                MyHandlerTmp(MySender, MyArgs);
        }

        public static void Fire(object MySender, EventHandler MyHandler, EventArgs MyArgs, AsyncOperation MyAsyncOperation)
        {
            MyAsyncOperation.Post(new SendOrPostCallback(delegate (object MyObject)
                {
                    Fire(MySender, MyHandler, MyArgs);
                }), null);
        }

        public static void Fire<T>(object MySender, EventHandler<T> MyHandler, T MyArgs, AsyncOperation MyAsyncOperation) where T : EventArgs
        {
            MyAsyncOperation.Post(new SendOrPostCallback(delegate (object MyObject)
                {
                    Fire<T>(MySender, MyHandler, MyArgs);
                }), null);
        }
    }
}