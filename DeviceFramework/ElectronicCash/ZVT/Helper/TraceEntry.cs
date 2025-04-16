using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Helper
{
    /// <summary>
    /// Single communication entry either of the terminal or the
    /// </summary>
    public class TraceEntry
    {
        #region Properties

        public DateTime Time { get; set; }
        public bool Sent { get; set; }
        public byte[] Data { get; set; }

        #endregion

        #region Constructor

        public TraceEntry(bool blnSent, byte[] bytData)
        {
            Time = DateTime.Now;
            Sent = blnSent;
            Data = bytData;
        }

        #endregion
    }
}