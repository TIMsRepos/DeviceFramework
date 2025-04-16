using System;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers
{
    public interface ITransportLayer : IDisposable
    {
        void Open();

        void Close();

        void SendAcknowledge(bool blnAck);

        void SendAPDU<T>(T APDUParam) where T : APDU;

        Enums.SpecialChars ReceiveAcknowledge();

        T ReceiveAPDU<T>() where T : APDU;
    }
}