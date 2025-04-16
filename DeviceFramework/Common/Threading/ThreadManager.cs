using System;
using System.Threading;

namespace TIM.Devices.Framework.Common.Threading
{
    public class ThreadManager
    {
        private static SynchronizationContext MyGUIContext = null;
        private static ManualResetEventSlim mreGlobalAbort = null;

        public static SynchronizationContext GUIContext
        {
            get { return MyGUIContext; }
            set
            {
                if (MyGUIContext != null)
                    throw new InvalidOperationException("GUIContext can only be set once!");
                MyGUIContext = value;
            }
        }

        public static ManualResetEventSlim GlobalAbort
        {
            get { return mreGlobalAbort; }
        }

        static ThreadManager()
        {
            mreGlobalAbort = new ManualResetEventSlim();
        }

        public static void DispatchToContext(Action MyAction, SynchronizationContext MyCtx)
        {
            MyCtx.Post(new SendOrPostCallback(delegate (object objState)
                {
                    MyAction();
                }), null);
        }

        public static void DispatchToGUI(Action MyAction)
        {
            DispatchToContext(MyAction, GUIContext);
        }

        public static void DispatchToGUIThreaded(Action MyAction)
        {
            new Thread(new ThreadStart(delegate ()
                {
                    DispatchToGUI(MyAction);
                })).Start();
        }
    }
}