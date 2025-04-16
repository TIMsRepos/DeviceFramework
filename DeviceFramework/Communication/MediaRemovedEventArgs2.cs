using System.Runtime.Serialization;

namespace TIM.Devices.Framework.Communication
{
    [DataContract]
    public class MediaRemovedEventArgs2 : MediaDetectedEventArgs2
    {
        public MediaRemovedEventArgs2(DeviceManagerMediaRemovedEventArgs2 e)
            : base(e)
        {
        }
    }
}