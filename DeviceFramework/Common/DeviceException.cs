using System;

namespace TIM.Devices.Framework.Common
{
    public class DeviceException : FrameworkException
    {
        /// <summary>
        /// Creates a new empty DeviceException
        /// </summary>
        public DeviceException()
            : base()
        {
        }

        /// <summary>
        /// Creates a new DeviceException with an error message
        /// </summary>
        /// <param name="strMessage">The error message</param>
        public DeviceException(string strMessage)
            : base(strMessage)
        {
        }

        /// <summary>
        /// Creates a new DeviceException with an error message and an inner exception
        /// </summary>
        /// <param name="strMessage">The error message</param>
        /// <param name="exInner">The inner exception</param>
        public DeviceException(string strMessage, Exception exInner)
            : base(strMessage, exInner)
        {
        }
    }
}