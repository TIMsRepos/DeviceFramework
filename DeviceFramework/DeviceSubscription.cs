using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Communication;

namespace TIM.Devices.Framework
{
    public class DeviceSubscription
    {
        private IDeviceClient MyDeviceClient;
        private Enums.ConfigDevices MyConfigDevices;
        private string strSignature;
        private object objDeviceClientLock;
        private byte bytVersion;

        public IDeviceClient DeviceClient
        {
            get { return MyDeviceClient; }
        }

        public Enums.ConfigDevices ConfigDevices
        {
            get { return MyConfigDevices; }
        }

        public string Signature
        {
            get { return strSignature; }
        }

        public object DeviceClientLock
        {
            get { return objDeviceClientLock; }
        }

        public byte Version
        {
            get { return bytVersion; }
        }

        public DeviceSubscription(IDeviceClient MyDeviceClient, Enums.ConfigDevices MyConfigDevices, string strSignature, byte bytVersion)
        {
            this.MyDeviceClient = MyDeviceClient;
            this.MyConfigDevices = MyConfigDevices;
            this.strSignature = strSignature;
            this.objDeviceClientLock = new object();
            this.bytVersion = bytVersion;
        }
    }
}