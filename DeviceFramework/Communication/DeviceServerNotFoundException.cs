using System;

namespace TIM.Devices.Framework.Communication
{
    public class DeviceServerNotFoundException : DeviceClientException
    {
        public DeviceServerNotFoundException(string strMessage, Exception exInner) : base(strMessage, exInner)
        {
        }
    }
}