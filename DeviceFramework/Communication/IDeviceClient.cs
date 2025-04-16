using System;
using System.ServiceModel;

namespace TIM.Devices.Framework.Communication
{
    [ServiceContract(
        Namespace = "http://schemas.meridianspa.de/TIM/DeviceFramework")]
    public interface IDeviceClient
    {
        event EventHandler<MediaDetectedEventArgs> MediaDetected;

        event EventHandler<MediaDetectedEventArgs2> MediaDetected2;

        event EventHandler<MediaRemovedEventArgs> MediaRemoved;

        event EventHandler<MediaRemovedEventArgs2> MediaRemoved2;

        event EventHandler<MissingMediaEventArgs> MissingMedia;

        string[] GetDeviceNames();

        bool UnblockLockerKey(int deviceFrameworkDeviceID, bool unblock);

        [OperationContract(IsOneWay = true)]
        void FireMediaDetected(MediaDetectedEventArgs e);

        [OperationContract(IsOneWay = true)]
        void FireMediaDetected2(MediaDetectedEventArgs2 e);

        [OperationContract(IsOneWay = true)]
        void FireMediaRemoved(MediaRemovedEventArgs e);

        [OperationContract(IsOneWay = true)]
        void FireMediaRemoved2(MediaRemovedEventArgs2 e);

        [OperationContract(IsOneWay = true)]
        void FireMissingMedia(string strDeviceName);

        [OperationContract(IsOneWay = true)]
        void ReleaseResources();

        [OperationContract]
        bool IsFloating();

        [OperationContract]
        void Pong();
    }
}