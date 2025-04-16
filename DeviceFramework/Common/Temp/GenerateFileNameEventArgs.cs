using System;
using System.IO;

namespace TIM.Devices.Framework.Common.Temp
{
    public class GenerateFileNameEventArgs : EventArgs
    {
        public DirectoryInfo Directory { get; private set; }
        public string Extension { get; private set; }
        public object Data { get; private set; }
        public string FileName { get; set; }

        public GenerateFileNameEventArgs(DirectoryInfo dirDirectory, string strExtension, object objData)
        {
            Directory = dirDirectory;
            Extension = strExtension;
            Data = objData;
        }
    }
}