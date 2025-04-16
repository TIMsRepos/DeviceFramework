namespace TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract
{
    /// <summary>
    /// Application Protocol Data Unit (= a complete request or response)
    /// </summary>
    public abstract class APDU
    {
        public byte Length => (byte)Data.Length;

        public byte[] Data { get; set; }

        protected APDU(byte[] bytData)
        {
            Data = bytData;
        }
    }
}