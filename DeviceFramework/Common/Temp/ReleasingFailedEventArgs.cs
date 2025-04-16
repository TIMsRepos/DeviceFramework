using System;
using System.IO;

namespace TIM.Devices.Framework.Common.Temp
{
    public class ReleasingFailedEventArgs : EventArgs
    {
        public FileInfo File { get; private set; }
        public Exception Exception { get; private set; }
        public bool Remove { get; set; }

        public ReleasingFailedEventArgs(FileInfo MyFile, Exception ex)
        {
            File = MyFile;
            Exception = ex;
            Remove = false;
        }
    }
}