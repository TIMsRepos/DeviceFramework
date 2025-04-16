namespace TIM.Devices.Framework.ElectronicCash.ZVT.Helper
{
    public static class LVarHelper
    {
        public static byte LLVar(byte[] bytData, int intOffset)
        {
            return (byte)(10 * (bytData[intOffset] - 0xF0) + (bytData[intOffset + 1] - 0xF0));
        }

        public static ushort LLLVar(byte[] bytData, int intOffset)
        {
            return (byte)(100 * (bytData[intOffset] - 0xF0) + 10 * (bytData[intOffset + 1] - 0xF0) + (bytData[intOffset + 2] - 0xF0));
        }
    }
}