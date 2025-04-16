using System;

namespace TIM.Devices.Framework.Common
{
    public class GenericEventArgs<T> : EventArgs
    {
        private readonly T MyData;

        public T Data
        {
            get { return MyData; }
        }

        public GenericEventArgs()
            : this(default(T))
        {
        }

        public GenericEventArgs(T MyObj)
        {
            this.MyData = MyObj;
        }
    }
}