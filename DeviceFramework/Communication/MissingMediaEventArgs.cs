using System;
using System.Runtime.Serialization;

namespace TIM.Devices.Framework.Communication
{
    [DataContract]
    public class MissingMediaEventArgs : EventArgs
    {
        #region Members

        private string strDeviceName;

        #endregion

        #region Getter

        [DataMember]
        public string DeviceName
        {
            get { return strDeviceName; }
        }

        #endregion

        #region Constructors

        public MissingMediaEventArgs(string strDeviceName)
            : base()
        {
            this.strDeviceName = strDeviceName;
        }

        #endregion
    }
}