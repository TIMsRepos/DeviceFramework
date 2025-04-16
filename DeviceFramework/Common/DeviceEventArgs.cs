using System;

namespace TIM.Devices.Framework.Common
{
    public class DeviceEventArgs : EventArgs
    {
        #region Members

        private IDevice MyDevice;
        private string strMediaID;

        #endregion

        #region Getter & Setter

        /// <summary>
        /// The device throwing the event
        /// </summary>
        public IDevice Device
        {
            get { return MyDevice; }
        }

        public string MediaID
        {
            get { return strMediaID; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new DeviceEventArgs object
        /// </summary>
        /// <param name="MyDevice">The device throwing this event</param>
        public DeviceEventArgs(IDevice MyDevice, string strMediaID)
            : base()
        {
            this.MyDevice = MyDevice;
            this.strMediaID = strMediaID;
        }

        #endregion
    }
}