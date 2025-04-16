using System;
using System.Collections.Generic;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events
{
    /// <summary>
    /// Reads the status information bytes by looking for the available hex bitmaps (= parameters) and getting their corresponding values.
    ///
    /// Control-field according to zvt interface documentation:
    /// CLASS: 04
    /// INSTR: 0F
    /// Length: xx (variable)
    /// </summary>
    public class PostStateInfoEventArgs : StateInfoEventArgs
    {
        #region Properties

        public override string Result => ErrorMessages.GetMessage(ResultCode);

        public int? Amount { get; private set; }
        public int? TraceNumber { get; private set; }
        public int? OriginalTraceNumber { get; private set; }
        public DateTime? DateTime { get; private set; }
        public DateTime? ExperationDate { get; private set; }
        public short? SequenceNumber { get; private set; }
        public Enums.PaymentConfigTypes? PaymentType { get; private set; }
        public int? TerminalID { get; private set; }
        public Enums.Currencies? Currency { get; private set; }
        public byte? CardType { get; private set; }
        public ushort? ReceiptNumber { get; private set; }
        public int? TurnoverNumber { get; private set; }
        public string CardName { get; private set; }
        public string PAN { get; private set; }
        public string AID { get; private set; }
        public string VUNumber { get; private set; }

        #endregion

        /// <summary>
        /// Reads the status information bytes by looking for the available hex bitmaps (= parameters) and getting their corresponding values.
        ///
        /// Control-field according to zvt interface documentation:
        /// CLASS: 04
        /// INSTR: 0F
        /// Length: xx (variable)
        ///
        /// This constructor skips the first byte, because it always contains the result code bitmap (0x27), to directly retrieve the first value.s
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        public PostStateInfoEventArgs(byte[] transferData)
            : base(transferData[1])
        {
            // Init properties
            // first info of the data block is the result code (prefaced with bitmap: 0x27)
            ResultCode = transferData[1];
            Amount = null;
            TraceNumber = null;
            OriginalTraceNumber = null;
            DateTime = null;
            ExperationDate = null;
            SequenceNumber = null;
            PaymentType = null;
            TerminalID = null;
            Currency = null;
            CardType = null;
            CardName = null;
            ReceiptNumber = null;
            TurnoverNumber = null;
            CardName = null;
            PAN = null;
            AID = null;

            // read status information by bitmap
            var offset = 2;
            while (offset < transferData.Length)
            {
                switch (transferData[offset])
                {
                    case 0x04:
                        offset += HandleAmount(transferData, offset);
                        break;

                    case 0x0b:
                        offset += HandleTraceNumber(transferData, offset);
                        break;

                    case 0x37:
                        offset += HandleOriginalTraceNumber(transferData, offset);
                        break;

                    case 0x0c:
                        offset += HandleTime(transferData, offset);
                        break;

                    case 0x0d:
                        offset += HandleDate(transferData, offset);
                        break;

                    case 0x0e:
                        offset += HandleExpirationDate(transferData, offset);
                        break;

                    case 0x17:
                        offset += HandleSequenceNumber(transferData, offset);
                        break;

                    case 0x19:
                        offset += HandlePaymentType(transferData, offset);
                        break;

                    case 0x22:
                        offset += HandlePAN(transferData, offset);
                        break;

                    /*
                     * List of blocked goods-groups
                     * LLVAR (2 byte counter [FxFy], data BCD packed)
                     *
                     * Structure:
                     *  -   2       LLVAR
                     *  -   3       product-code according to goods-groups-table in PT,
                     *  -   BCD     encoded with leading zeros
                     *  -   ...     ...
                     *  -   3       product-code according to goods-groups-table in PT,
                     *  -   BCD     encoded with leading zeros
                     */
                    case 0x4c:
                        byte valueLength = LVarHelper.LLVar(transferData, offset + 1);
                        offset += 2 + valueLength;
                        break;

                    case 0x29:
                        offset += HandleTerminalID(transferData, offset);
                        break;

                    /*
                     * authorisation-attribute. The length of the bitmaps is always 8 byte.
                     *
                     * contents:
                     *  1.) ec-cards, bank-customer-card/service-card, Geldkarte (BMP8A = 2 or 30):
                     *      used-data max. 4 byte binary. The bitmap is filled with leading zeros.
                     *      Only for ec-Cash magnet-stripe and ec-Cash chip online.
                     *      For ec-Cash chip offline see BMP 92.
                     *
                     *  2.) Maestro-cards (BMP8A = 46):
                     *      used-data max. 6 byte ASCII. The bitmap is filled with trailing zeros.
                     *
                     *  3). Girocard-cards (ectrack2, ecEMV online/offline):
                     *      8 byte ASCII padded with trailing zeros.
                     *
                     *  4.) other cards:
                     *      used-data max. 8 byte ASCII. The bitmap is filled, where possible, with
                     *      trailing zeros.
                     *
                     */
                    case 0x3b:
                        offset += 8; //HandleAID(transferData, offset);
                        break;

                    case 0x49:
                        offset += HandleCurrency(transferData, offset);
                        break;

                    case 0x87:
                        offset += HandleReceiptNumber(transferData, offset);
                        break;

                    case 0x8a:
                        offset += HandleCardType(transferData, offset);
                        break;

                    /*
                     * card-type-ID of the network operator; 1 byte binary.
                     *
                     * If the network operator card-type ID is larger than decimal 255 then
                     * BMP 8C should contain ‘FF’ and tag 49 should be used (see chapter
                     * TLV-container), providing the network operator card-type ID is to be
                     * sent to the ECR. Alternatively BMP 8C can be omitted.
                     */
                    case 0x8c:
                        offset += 1;
                        break;

                    /*
                     * card-type-ID of the network operator; 1 byte binary.
                     *
                     * If the network operator card-type ID is larger than decimal 255 then
                     * BMP 8C should contain ‘FF’ and tag 49 should be used (see chapter
                     * TLV-container), providing the network operator card-type ID is to be
                     * sent to the ECR. Alternatively BMP 8C can be omitted.
                     *
                     * Structure:
                     *  -   3   LLLVAR
                     *  -   17  EF_INFO
                     *  -   24  EF_ID
                     *  -   2   EF_SEQ
                     *  -   2   KID (= log. key-number from reduce_ec) and
                     *          KV  (key-version from reduce_ec) of KZert
                     *  -   8   certificate with KZert
                     */
                    case 0x92:
                        offset += 3 + 17 + 24 + 2 + 2 + 8;
                        break;

                    /*
                     * LLLVAR payment-record from Geldkarte with certificate according to
                     * specification for the ec-card with chip – Version 3.0. 100 bytes binary
                     * (103 byte incl. LLLVAR); (only for Geldkarte)
                     *
                     * Structure:
                     *  -   3       LLLVAR, always: F1 F0 F0
                     *  -   100     payment-record according to specification Geldkarte 3.0
                     */
                    case 0x9a:
                        offset += 3 + 100;
                        break;

                    /*
                     * AID-parameter, 5 byte binary (only for ec-cards, BMP 8A = 2, 30)
                     *
                     * Only for ec-Cash magnet-stripe and ec-Cash Chip online (see also BMP AF).
                     * Not for girocard (ecTrack2, ecEMV online/offline).
                     * For ec-Cash Chip offline see BMP 92.
                     */
                    case 0xba:
                        offset += 5; //HandleAID2(transferData, offset);
                        break;

                    /*
                     * LLLVAR for ec-Cash with Chip Online-payments. Datalength 17 byte
                     * (incl. LLLVAR 20 byte) according to specification for the ec-card with
                     * Chip – Version 3.0 / 5.2.
                     *
                     * Only for ec-Cash chip online (see also BMP BA). For ec-Cash chip offline
                     * see BMP 92.
                     * Not for girocard (ecTrack2, ecEMV online/offline).
                     *
                     * receipt-data:
                     * account-number: byte 1-5
                     * card sequence-number: byte 9-10
                     * BLZ: byte 11-14
                     */
                    case 0xaf:
                        offset += 3 + 17;
                        break;

                    case 0x2a:
                        offset += HandleVUNumber(transferData, offset);
                        break;

                    // additional text for credit-cards, LLLVAR, ASCII, not null-terminated.
                    case 0x3c:
                        ushort shrLen = LVarHelper.LLLVar(transferData, offset + 1);
                        offset += 3 + shrLen;
                        break;

                    // the result-code, the AS is set if the host sends a result-code which can't
                    // be encoded in BCD. 1 byte, binary.
                    case 0xa0:
                        offset += 1;
                        break;

                    case 0x88:
                        offset += HandleTurnoverNumber(transferData, offset);
                        break;

                    case 0x8b:
                        offset += HandleCardName(transferData, offset);
                        break;
                }
                ++offset;
            }
        }

        #region Handle methods for status information

        /// <summary>
        /// contract-number for credit-cards, 15 byte, ASCII, not null-terminated.
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (15)</returns>
        private int HandleVUNumber(IReadOnlyList<byte> transferData, int offset)
        {
            var text = new char[15];
            for (var i = 0; i < text.Length; ++i)
            {
                text[i] = (char)transferData[offset + 1 + i];
            }
            VUNumber = new string(text).Trim();
            return 15;
        }

        /// <summary>
        /// PAN for magnet-stripe or EF_ID for ec chip,
        ///
        /// LLVAR(2 byte counter [FxFy], data BCD packed, D = separator),
        /// e.g.F0 F3 01 23 45 (F0 F3 means 3 bytes follow)
        ///
        /// receipt-data of the EF_ID:
        ///     - card-number: byte 5-9 from EF_ID
        ///     - expiry-date: byte 11-12 from EF_ID
        ///
        /// The transfer of the PAN for girocard transactions(ecTrack2, ecEMV
        /// online/offline) is in BCD format(analogous to credit card payments).        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed</returns>
        private int HandlePAN(byte[] transferData, int offset)
        {
            var valueLength = LVarHelper.LLVar(transferData, offset + 1);
            var value = new byte[valueLength];
            Array.Copy(transferData, offset + 1 + 2, value, 0, valueLength);

            // for some shorter numbers, e.g. Amex with 15, a "F" or may be added for unused digits. These need to be skipped
            PAN += BCDCodeHelper.Bcd2DecWithFiller(value).ToString();
            return 2 + valueLength;
        }

        /// <summary>
        /// analogous to receipt-number, <turnover-nr./> is hovever valid for all
        /// transactions. 3 byte BCD-packed.Not supported by all terminals.
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (3)</returns>
        private int HandleTurnoverNumber(byte[] transferData, int offset)
        {
            TurnoverNumber = BCDCodeHelper.Bcd2Dec<int>(transferData, offset + 1, 3);
            return 3;
        }

        /// <summary>
        /// receipt-number, 2 byte BCD packed. Valid only for non-Geldkarte transactions.
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns></returns>
        private int HandleReceiptNumber(byte[] transferData, int offset)
        {
            ReceiptNumber = BCDCodeHelper.Bcd2Dec<ushort>(transferData, offset + 1, 2);
            return 2;
        }

        /// <summary>
        /// name of the card-type, LLVAR, ASCII, null-terminated.
        ///
        /// For EMV-applications the product name is entered here.This must be
        /// printed on the receipt.
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed</returns>
        private int HandleCardName(byte[] transferData, int offset)
        {
            var valueLength = LVarHelper.LLVar(transferData, offset + 1);
            var text = new char[transferData.Length];
            for (var i = 0; i < text.Length; ++i)
            {
                text[i] = (char)transferData[i];
            }
            CardName = new string(text, offset + 1 + 2, valueLength - 1);
            return 2 + valueLength;
        }

        /// <summary>
        /// card-type (= ZVT card-type ID), 1 byte binary; see chapter ZVT-cardtype-ID.
        ///
        /// Via BMP 8A can only cards within the first 255 card-type-IDs be
        /// transferred.For cards ID 256 upwards tag 41 must be used.
        ///
        /// If the ZVT card-type ID is larger than decimal 255 then BMP 8A should
        /// contain ‘FF’ and tag 41 should be used(see chapter TLV-container),
        /// providing the ZVT Card-Type ID is to be sent to the ECR.Alternatively
        /// BMP 8A can be omitted.
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (1)</returns>
        private int HandleCardType(IReadOnlyList<byte> transferData, int offset)
        {
            var cardType = transferData[offset + 1];
            CardType = cardType;
            return 1;
        }

        /// <summary>
        /// 2 byte BCD packed. Value: 09 78 = EUR
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (2)</returns>
        private int HandleCurrency(IReadOnlyList<byte> transferData, int offset)
        {
            var currency = (ushort)((transferData[offset + 1] << 8) | transferData[offset + 2]);
            Currency = (Enums.Currencies)currency;
            return 2;
        }

        /// <summary>
        /// terminal-ID, 4 byte BCD packed
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (4)</returns>
        private int HandleTerminalID(byte[] transferData, int offset)
        {
            TerminalID = BCDCodeHelper.Bcd2Dec<int>(transferData, offset + 1, 4);
            return 4;
        }

        /// <summary>
        /// payment-type:
        ///     -   40 = offline
        ///     -   50 = card in terminal checked positively, but no Authorization carried out
        ///     -   60 = online
        ///     -   70 = PIN-payment(also possible for EMV-processing, i.e.credit cards, ecTrack2, ecEMV online/offline).
        ///
        /// If the TLV-container is active, this information can be specified in tag 2F
        /// (see chapter TLV-containerin zvt documentation).
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (1)</returns>
        private int HandlePaymentType(byte[] transferData, int offset)
        {
            var paymentType = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 1);
            PaymentType = (Enums.PaymentConfigTypes)paymentType;
            return 1;
        }

        /// <summary>
        /// card sequence-number, 2 byte BCD packed (only for ec-cards)
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (2)</returns>
        private int HandleSequenceNumber(byte[] transferData, int offset)
        {
            SequenceNumber = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 1, 2);
            return 2;
        }

        /// <summary>
        /// expiry-date, 2 byte BCD in Format YYMM
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (2)</returns>
        private int HandleExpirationDate(byte[] transferData, int offset)
        {
            var y = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 1);
            var m = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 2);
            ExperationDate = new DateTime(2000 + y, m, System.DateTime.DaysInMonth(2000 + y, m));
            return 2;
        }

        /// <summary>
        /// 2 byte BCD MMDD
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (2)</returns>
        private int HandleDate(byte[] transferData, int offset)
        {
            var m = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 1);
            var d = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 2);
            if (DateTime.HasValue)
            {
                DateTime = new DateTime(DateTime.Value.Year, m, d, DateTime.Value.Hour, DateTime.Value.Minute,
                    DateTime.Value.Second);
            }
            else
            {
                DateTime = new DateTime(System.DateTime.Today.Year, m, d);
            }
            return 2;
        }

        /// <summary>
        /// 3 byte BCD HHMMSS
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns> the number of bytes that have been processed (3)</returns>
        private int HandleTime(byte[] transferData, int offset)
        {
            var h = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 1);
            var m = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 2);
            var s = BCDCodeHelper.Bcd2Dec<byte>(transferData, offset + 3);
            if (DateTime.HasValue)
            {
                DateTime.Value.AddHours(h);
                DateTime.Value.AddMinutes(m);
                DateTime.Value.AddSeconds(s);
            }
            else
            {
                DateTime = new DateTime(System.DateTime.Today.Year, System.DateTime.Today.Month,
                    System.DateTime.Today.Day, h, m, s);
            }
            return 3;
        }

        /// <summary>
        /// only for Reversal: Trace-number of the original payment, 3 byte BCD
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed</returns>
        private int HandleOriginalTraceNumber(byte[] transferData, int offset)
        {
            int value;
            var valueLength = Handle3ByteBCD(transferData, offset + 1, out value);
            OriginalTraceNumber = value;
            return valueLength;
        }

        /// <summary>
        /// trace-number, 3 byte BCD, for long trace numbers with more than 6 digits,
        /// the bitmap is set to 000000 and TLV tag 1F2B is used instead.        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed</returns>
        private int HandleTraceNumber(byte[] transferData, int offset)
        {
            int value;
            var valueLength = Handle3ByteBCD(transferData, offset + 1, out value);
            TraceNumber = value;
            return valueLength;
        }

        /// <summary>
        /// Converts the byte value at the given offset to a integer
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <param name="intValue"></param>
        /// <returns>the number of bytes that have been processed</returns>
        private static int Handle3ByteBCD(byte[] transferData, int offset, out int intValue)
        {
            var value = new byte[3];
            Array.Copy(transferData, offset, value, 0, 3);
            var lngValue = BCDCodeHelper.Bcd2Dec(value);
            intValue = (int)lngValue;
            return 3;
        }

        /// <summary>
        /// Transaction amount
        /// 6 byte BCD packed (payment-amount or total of the End-of-Day)
        /// </summary>
        /// <param name="transferData">a byte array of hex values containing the bitmap parameters and their corresponding values</param>
        /// <param name="offset">the starting position of the value in the byte array</param>
        /// <returns>the number of bytes that have been processed (6)</returns>
        private int HandleAmount(byte[] transferData, int offset)
        {
            var amount = new byte[6];
            Array.Copy(transferData, offset + 1, amount, 0, 6);
            var lngAmount = BCDCodeHelper.Bcd2Dec(amount);
            Amount = (int)lngAmount;
            return 6;
        }
        
        #endregion
    }
}