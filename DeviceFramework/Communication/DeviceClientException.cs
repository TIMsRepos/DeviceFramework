using System;

namespace TIM.Devices.Framework.Communication
{
    [global::System.Serializable]
    public class DeviceClientException : FrameworkException
    {
        public DeviceClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}