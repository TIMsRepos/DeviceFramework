using System;

namespace TIM.Devices.Framework.Common.Threading
{
    public class LoaderLoadTaskEventArgs : EventArgs
    {
        private readonly LoadTask MyLoadTask;

        public LoadTask LoadTask
        {
            get { return MyLoadTask; }
        }

        public LoaderLoadTaskEventArgs(LoadTask MyLoadTask)
        {
            this.MyLoadTask = MyLoadTask;
        }
    }
}