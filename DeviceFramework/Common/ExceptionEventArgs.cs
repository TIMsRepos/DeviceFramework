using System;

namespace TIM.Devices.Framework.Common
{
    public class ExceptionEventArgs : EventArgs
    {
        private readonly Exception MyException;

        public Exception Exception
        {
            get { return MyException; }
        }

        public ExceptionEventArgs(Exception MyException)
        {
            this.MyException = MyException;
        }
    }
}