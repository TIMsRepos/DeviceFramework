using System;

namespace TIM.Devices.Framework.ElectronicCash.ZVT
{
    public class ZVTEnums
    {
        public enum ZVTCommands
        {
            /// <summary>
            /// Using the command Registration the Electronic Cash Register can set up different configurations on the PT and also control the
            /// current status of the Payment-Terminal.
            /// </summary>
            Registration,

            /// <summary>
            /// This command initiates a payment process and transmits the amount from the Electronic Cash Register to Payment-Terminal. The result of the
            /// payment process is reported to the Electronic Cash Register after completion of the booking process.
            /// </summary>
            Authorization,

            /// <summary>
            /// Certain commands must be completed with a separate command.
            /// Commands which require Completion are explicitly noted within the command description.
            /// </summary>
            Completion,

            /// <summary>
            /// With this command the Electronic Cash Register can cause the Payment-Terminal to display a certain text on the Payment-Terminal-display.
            /// </summary>
            DisplayText,

            /// <summary>
            /// The command Log-Off has the following consequences:
            ///      - the Payment-Terminal resets the Registration config-byte to 86
            ///      - the Payment-Terminal may not send any more TLV-containers
            /// </summary>
            LogOff,

            /// <summary>
            /// Default response which acknowledges a previous command
            /// </summary>
            AcknowledgmentResponse,

            /// <summary>
            /// A response 84-yy-xx-xx with ‚yy‘ not equal to ‚00‘ and not equal to ‚9C‘ implies that the Issue-
            /// of-Goods has not succeeded. If Issue-of-Goods did not succeed the PT reverses the payment.
            /// </summary>
            NotSucceededResponse,

            /// <summary>
            /// If a command was not successfully terminated the PT sends command Abort to the Electronic Cash Register.
            /// </summary>
            Abort
        }

        public enum SpecialChars : byte
        {
            /// <summary>
            /// DLE
            /// </summary>
            DataLineEscape = 0x10,

            /// <summary>
            /// STX
            /// </summary>
            StartOfText = 0x02,

            /// <summary>
            /// ETX
            /// </summary>
            EndOfText = 0x03,

            /// <summary>
            /// ACK
            /// </summary>
            Acknowledged = 0x06,

            /// <summary>
            /// NAK
            /// </summary>
            NotAcknowledged = 0x15,

            /// <summary>
            /// CR
            /// </summary>
            CarriageReturn = 0x0d,

            /// <summary>
            /// LF
            /// </summary>
            LineFeed = 0x0a
        }

        [Flags]
        public enum PaymentTypes : byte
        {
            Reserved1 = 0x1,
            PT_UsePreviousOrError = 0x2,
            PrinterReady = 0x4,
            TippableTransaction = 0x8,
            ELV = 0x0,
            MoneyCard = 0x10,
            OnlineWithoutPin = 0x20,
            Pin = 0x30,
            PT_Decission = 0x40
        }

        public enum PaymentConfigTypes : byte
        {
            Offline = 40,
            CardPositiveButNoAuth = 50,
            Online = 60,
            PINPayment = 70
        }

        /// <summary>
        /// The Currency Code (CC) has a length of 2 bytes.
        ///
        /// The Currency-Code is checked by the PT as follows to ensure maximum compatibility:
        ///     · no CC             OK(interpreted as amount in currency ‚EUR‘)
        ///     · CC = 09 78        OK(= ‚EUR‘)
        ///     · All other CCs     OK if PT supports multiple currencies otherwise error
        /// The Payment-Terminal only sends a Currency-Code to the Electronic Cash Register,
        /// if the Electronic Cash Register had also sent a Currency-Code in its request.
        /// </summary>
        public enum Currencies : ushort
        {
            Eur = 0x0978
        }

        [Flags]
        public enum ConfigByte : byte
        {
            None = 0,
            Reserved1 = 0x1,
            POSPrintsPayment = 0x2,
            POSPrintsAdministration = 0x4,
            ECRRequiresIntermediateStatusInfo = 0x8,
            POSControlsPayment = 0x10,
            POSControlsAdministration = 0x20,
            Reserved2 = 0x40,
            POSCustomReceipt = 0x80
        }
    }
}