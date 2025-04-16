using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Helper
{
    public static class TraceHelper
    {
        #region Fields

        private static readonly List<TraceEntry> Entries = new List<TraceEntry>();

        #endregion

        #region Properties

        public static bool DirectDumpEnabled { get; set; }
        public static TextWriter DirectDumpTextWriter { get; set; }

        #endregion

        #region Constructor

        static TraceHelper()
        {
            DirectDumpEnabled = false;
            DirectDumpTextWriter = Console.Out;
        }

        #endregion

        #region Methods

        public static void Add(bool blnSent, params byte[] bytData)
        {
            var entry = new TraceEntry(blnSent, bytData);
            Entries.Add(entry);

            if (DirectDumpEnabled)
            {
                DirectDumpTextWriter.WriteLine(FormatLine(entry));
            }
        }

        public static string Dump()
        {
            var builder = new StringBuilder();

            foreach (var entry in Entries)
            {
                builder.AppendLine(FormatLine(entry));
            }

            return builder.ToString();
        }

        private static string FormatLine(TraceEntry entry)
        {
            return $"[{entry.Time.ToLongTimeString()}] ({(entry.Sent ? "Sent" : "Recv")}): {Bytes2Hex(entry.Data)}";
        }

        public static void Clear()
        {
            Entries.Clear();
        }

        public static string Bytes2Hex(params byte[] bytData)
        {
            var builder = new StringBuilder();
            foreach (var t in bytData)
            {
                builder.Append($"0x{t:X2} ");
            }
            return builder.ToString();
        }

        #endregion
    }
}