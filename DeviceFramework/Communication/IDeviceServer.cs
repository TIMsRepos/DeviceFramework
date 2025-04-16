using System;
using System.ServiceModel;

namespace TIM.Devices.Framework.Communication
{
    [ServiceContract(
        Namespace = "http://schemas.meridianspa.de/TIM/DeviceFramework",
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IDeviceClient))]
    public interface IDeviceServer
    {
        [OperationContract]
        int AddIDeviceClient(int intConfigDevices, string strName);

        [OperationContract]
        int AddIDeviceClient2(int intConfigDevices, string strName, byte bytVersion);

        [OperationContract]
        string[] GetDeviceNames();

        [Obsolete]
        [OperationContract]
        bool Request();

        [OperationContract]
        bool UnblockLockerKey(int deviceFrameworkDeviceID, bool unblock);
    }
}