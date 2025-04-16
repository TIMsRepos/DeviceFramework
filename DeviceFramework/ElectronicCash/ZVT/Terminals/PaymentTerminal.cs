using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Extensions;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Exceptions;
using TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals
{
    public class PaymentTerminal : ITerminal
    {
        #region Events

        public event EventHandler<StateInfoEventArgs> StateInfo;

        public event EventHandler<PostStateInfoEventArgs> PostStateInfo;

        public event EventHandler<InterimStateInfoEventArgs> InterimStateInfo;

        #endregion

        #region Properties

        /// <summary>
        /// The transport layer for the underlying physical interface, e.g. RS232
        /// </summary>
        public ITransportLayer TransportLayer { get; }

        /// <summary>
        /// True if the Register-request completed successfully
        /// </summary>
        public bool Registered { get; private set; }

        #endregion

        #region Constructor

        public PaymentTerminal(ITransportLayer transportLayer)
        {
            TransportLayer = transportLayer;
            Registered = false;
        }

        #endregion

        #region Methods

        private static byte[] Currency2Bytes(Enums.Currencies currency)
        {
            var bytCurrency = new byte[2];

            bytCurrency[0] = (byte)((ushort)currency >> 8);
            bytCurrency[1] = (byte)((ushort)currency & 0xFF);

            return bytCurrency;
        }

        private static TerminalException BuildException(string strMessage, ResponseAPDU responseAPDU)
        {
            return
                new TerminalException(
                    $"{strMessage} (Error: {responseAPDU.ErrorMessage}[0x{responseAPDU.ControlField.APRC:X2}])");
        }

        /// <summary>
        /// Connects to the terminal with the given settings
        /// </summary>
        /// <param name="configByte"></param>
        /// <param name="currency"></param>
        /// <param name="password"></param>
        public void Register(Enums.ConfigByte configByte, Enums.Currencies currency, string password)
        {
            TraceHelper.Clear();

            // Validate password. The password consists of 6 digits which are packed as 3 byte BCD.
            int intPassword;
            if (!int.TryParse(password, out intPassword) ||
                intPassword > 999999 ||
                intPassword < 0)
            {
                throw new TerminalWrongConfigException(
                    $"The password needs to be in range '0[00000]' - '999999' (current: {intPassword})");
            }

            // Convert and format password
            var bytPasswordDigits = BCDCodeHelper.ExtractDecDigits(intPassword);
            if (bytPasswordDigits.Length < 6)
            {
                bytPasswordDigits = bytPasswordDigits.ExpandPrefixed(6);
            }
            var bytPassword = BCDCodeHelper.DecDigits2Bcd(bytPasswordDigits);
            var bytCurrency = Currency2Bytes(currency);

            // Write registration command
            var cmdAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.Registration),
                new[]
                {bytPassword[0], bytPassword[1], bytPassword[2], (byte) configByte, bytCurrency[0], bytCurrency[1]});
            TransportLayer.SendAPDU(cmdAPDU);

            // Read response
            var specialCharAcknowledge = Enums.SpecialChars.NotAcknowledged;
            try
            {
                Workflows.InvokeWithTimeout(delegate
                {
                    specialCharAcknowledge = TransportLayer.ReceiveAcknowledge();
                }, 1000);
            }
            catch (TimeoutException)
            {
                throw new TerminalNotAvailableException();
            }

            if (specialCharAcknowledge == Enums.SpecialChars.Acknowledged)
            {
                var responseAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                TransportLayer.SendAcknowledge(true);
                if (responseAPDU.HasError)
                {
                    switch (responseAPDU.ControlField.APRC)
                    {
                        case 0xA0:
                            throw new TerminalNotReadyException();
                        case 0x83:
                            throw new TerminalWrongConfigException("Wrong password");
                    }
                    throw BuildException("Register failed", responseAPDU);
                }

                cmdAPDU = TransportLayer.ReceiveAPDU<CommandAPDU>();
                if (cmdAPDU.ControlField == new CommandControlField(Enums.ZVTCommands.Completion))
                {
                    TransportLayer.SendAcknowledge(true);
                    responseAPDU = new ResponseAPDU(new ResponseControlField(Enums.ZVTCommands.AcknowledgmentResponse), new byte[0]);
                    TransportLayer.SendAPDU(responseAPDU);
                    if (TransportLayer.ReceiveAcknowledge() != Enums.SpecialChars.Acknowledged)
                    {
                        throw new TerminalException("Terminal didn't answer on response to completion of registration");
                    }
                }
                else
                {
                    throw new TerminalException("Register didn't complete");
                }
            }
            else
            {
                throw new TerminalException("Wasn't able to register terminal");
            }

            Registered = true;
        }

        /// <summary>
        /// Disconnects from the terminal
        /// </summary>
        public void LogOff()
        {
            if (!Registered)
            {
                return;
            }
            var cmdAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.LogOff), new byte[0]);
            TransportLayer.SendAPDU(cmdAPDU);
            var specialCharAcknowledge = Enums.SpecialChars.NotAcknowledged;
            try
            {
                Workflows.InvokeWithTimeout(delegate
                {
                    specialCharAcknowledge = TransportLayer.ReceiveAcknowledge();
                }, 1000);
            }
            catch (TimeoutException)
            {
            }
            if (specialCharAcknowledge != Enums.SpecialChars.Acknowledged)
            {
                return;
            }

            var responseAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
            if (responseAPDU.ControlField == new ResponseControlField(Enums.ZVTCommands.AcknowledgmentResponse))
            {
                TransportLayer.SendAcknowledge(true);
                Registered = false;
            }
            else
            {
                throw new TerminalException("LogOff didn't complete");
            }
        }

        /// <summary>
        /// Displays text on the terminal, max. 8 lines with max. 16 characters each
        /// </summary>
        /// <param name="strLines"></param>
        public void DisplayText(string[] strLines)
        {
            if (strLines.Length > 8)
            {
                throw new ArgumentOutOfRangeException("DisplayText can't process more than 8 lines of text");
            }

            var lstLines = new List<byte>();
            for (var i = 0; i < 8; ++i)
            {
                var strLine = strLines.Length > i ? strLines[i] : " ";
                var bytLine = Encoding.ASCII.GetBytes(strLine);
                lstLines.Add((byte)(0xF0 + i + 1));
                lstLines.Add(0xF0);
                lstLines.Add((byte)(0xF0 + (strLine.Length > 15 ? 15 : strLine.Length)));
                lstLines.AddRange(bytLine);
            }
            var cmdAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.DisplayText),
                new byte[] { 0xF0, 0x00 }.Concat(lstLines).ToArray());
            TransportLayer.SendAPDU(cmdAPDU);
            if (TransportLayer.ReceiveAcknowledge() == Enums.SpecialChars.Acknowledged)
            {
                var responseAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                if (responseAPDU.ControlField == new ResponseControlField(Enums.ZVTCommands.AcknowledgmentResponse))
                {
                    TransportLayer.SendAcknowledge(true);
                }
                else
                {
                    throw new TerminalException("DisplayText didn't complete");
                }
            }
            else
            {
                throw new TerminalException("Wasn't able to display text on terminal");
            }
        }

        public void Authenticate(int intAmountCents, Enums.Currencies currency, Enums.PaymentTypes paymentType)
        {
            // Start
            var bytAmountDigits = BCDCodeHelper.Dec2Bcd(intAmountCents);
            bytAmountDigits = bytAmountDigits.ExpandPrefixed(6);
            var bytCurrency = Currency2Bytes(currency);

            var cmdAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.Authorization),
                new byte[] { 0x04 }
                    .Concat(bytAmountDigits)
                    .Concat(new byte[] { 0x49 })
                    .Concat(bytCurrency)
                    .Concat(new byte[] { 0x19 })
                    .Concat((byte)paymentType)
                    .ToArray());
            TransportLayer.SendAPDU(cmdAPDU);

            if (TransportLayer.ReceiveAcknowledge() == Enums.SpecialChars.Acknowledged)
            {
                var responseAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                if (responseAPDU.HasError)
                {
                    throw BuildException("Authenticate failed", responseAPDU);
                }
                TransportLayer.SendAcknowledge(true);

                responseAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                TransportLayer.SendAcknowledge(true);
                while (responseAPDU.ControlField.CCRC == 0x04 &&
                       responseAPDU.ControlField.APRC == 0xFF)
                {
                    var evtInterimArgs = new InterimStateInfoEventArgs(responseAPDU.Data);
                    OnStateInfo(evtInterimArgs);
                    OnInterimStateInfo(evtInterimArgs);

                    /*MyResAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                    TransportLayer.SendAcknowledge(true);*/

                    Thread.Sleep(500);
                    cmdAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.AcknowledgmentResponse), new byte[0]);
                    TransportLayer.SendAPDU(cmdAPDU);
                    TransportLayer.ReceiveAcknowledge();
                }

                // Abort key before payment type selection
                if (responseAPDU.ControlField == new ResponseControlField(Enums.ZVTCommands.Abort))
                {
                    // Send accept of error command
                    cmdAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.AcknowledgmentResponse), new byte[0]);
                    TransportLayer.SendAPDU(cmdAPDU);
                    TransportLayer.ReceiveAcknowledge();

                    switch (responseAPDU.Data[0])
                    {
                        // Abort
                        case 0x6c:
                            throw new TerminalCanceledException();
                        // Card reader doesnt answer (insert card timeout)
                        case 0xe2:
                            throw new TerminalCanceledException();
                    }
                    throw new TerminalException(ErrorMessages.GetMessage(responseAPDU.Data[0]));
                }

                // State after input
                if (responseAPDU.ControlField.CCRC != 0x04 ||
                    responseAPDU.ControlField.APRC != 0x0F)
                {
                    throw new TerminalException("Invalid final interimstate");
                }
                if (responseAPDU.Data[0] != 0x27)
                {
                    throw new TerminalException("Invalid final interimstate body");
                }

                // Transaction complete, event contains all infos
                var evtPostArgs = new PostStateInfoEventArgs(responseAPDU.Data);

                // Finalize to unblock terminal
                if (evtPostArgs.ResultHasError)
                {
                    var cmdErrAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.NotSucceededResponse), new byte[0]);
                    TransportLayer.SendAPDU(cmdErrAPDU);
                    TransportLayer.ReceiveAcknowledge();

                    var responseErrAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                    TransportLayer.SendAcknowledge(true);
                    if (responseErrAPDU.ControlField == new ResponseControlField(Enums.ZVTCommands.Abort))
                    {
                        cmdErrAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.AcknowledgmentResponse), new byte[0]);
                        TransportLayer.SendAPDU(cmdErrAPDU);
                        TransportLayer.ReceiveAcknowledge();
                    }
                }
                else
                {
                    var cmdSuccessAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.AcknowledgmentResponse), new byte[0]);
                    TransportLayer.SendAPDU(cmdSuccessAPDU);
                    TransportLayer.ReceiveAcknowledge();

                    var responseSuccessAPDU = TransportLayer.ReceiveAPDU<ResponseAPDU>();
                    TransportLayer.SendAcknowledge(true);
                    if (responseSuccessAPDU.ControlField == new ResponseControlField(Enums.ZVTCommands.Completion))
                    {
                        cmdSuccessAPDU = new CommandAPDU(new CommandControlField(Enums.ZVTCommands.AcknowledgmentResponse), new byte[0]);
                        TransportLayer.SendAPDU(cmdSuccessAPDU);
                        TransportLayer.ReceiveAcknowledge();
                    }
                }

                // Throw events
                OnStateInfo(evtPostArgs);
                OnPostStateInfo(evtPostArgs);
            }
            else
            {
                throw new TerminalException("Wasn't able to authenticate card");
            }
        }

        #region Events

        protected virtual void OnStateInfo(StateInfoEventArgs evtArgs)
        {
            var evtHandler = StateInfo;
            evtHandler?.Invoke(this, evtArgs);
        }

        protected virtual void OnPostStateInfo(PostStateInfoEventArgs evtArgs)
        {
            var evtHandler = PostStateInfo;
            evtHandler?.Invoke(this, evtArgs);
        }

        protected virtual void OnInterimStateInfo(InterimStateInfoEventArgs evtArgs)
        {
            var evtHandler = InterimStateInfo;
            evtHandler?.Invoke(this, evtArgs);
        }

        #endregion

        #endregion
    }
}