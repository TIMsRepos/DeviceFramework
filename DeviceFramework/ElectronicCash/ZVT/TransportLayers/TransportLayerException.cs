using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers
{
    [Serializable]
    public class TransportLayerException : ZVTException
    {
        public TransportLayerException()
        {
        }

        public TransportLayerException(string strMessage) : base(strMessage)
        {
        }

        public TransportLayerException(string strMessage, Exception exInner) : base(strMessage, exInner)
        {
        }
    }
}