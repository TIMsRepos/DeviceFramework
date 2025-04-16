using System.Runtime.Serialization;

namespace TIM.Devices.Framework.Communication
{
    [DataContract]
    public class MediaRemovedEventArgs : MediaDetectedEventArgs
    {
        public MediaRemovedEventArgs(int intDeviceID, string strDeviceName, string strMediaID)
            : base(intDeviceID, strDeviceName, strMediaID)
        {
        }
    }
}