using System;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.FeigIDRWA02
{
    [global::System.Serializable]
    public class IDRWA02Exception : DeviceException
    {
        public IDRWA02Exception(string strMessage) : base(strMessage)
        {
        }
    }
}