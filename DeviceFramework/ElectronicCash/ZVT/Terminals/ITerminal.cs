using System;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events;
using TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals
{
    public interface ITerminal
    {
        event EventHandler<StateInfoEventArgs> StateInfo;

        event EventHandler<PostStateInfoEventArgs> PostStateInfo;

        event EventHandler<InterimStateInfoEventArgs> InterimStateInfo;

        /// <summary>
        /// The transport layer for the underlying physical interface, e.g. RS232
        /// </summary>
        ITransportLayer TransportLayer { get; }

        /// <summary>
        /// Connects to the terminal with the given settings
        /// </summary>
        /// <param name="configByte"></param>
        /// <param name="currency"></param>
        /// <param name="password"></param>
        void Register(Enums.ConfigByte configByte, Enums.Currencies currency, string password);

        /// <summary>
        /// Disconnects from the terminal
        /// </summary>
        void LogOff();

        /// <summary>
        /// Displays text on the terminal, max. 8 lines with max. 16 characters each
        /// </summary>
        /// <param name="strLines"></param>
        void DisplayText(string[] strLines);

        void Authenticate(int intAmountCents, Enums.Currencies currency, Enums.PaymentTypes paymentType);
    }
}