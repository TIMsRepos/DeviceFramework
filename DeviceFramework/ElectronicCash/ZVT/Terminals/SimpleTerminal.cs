using System;
using System.Diagnostics;
using System.Threading;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Extensions;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Exceptions;
using TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.Terminals
{
    public class SimpleTerminal<TTerminal, TTransport> : IDisposable
        where TTransport : ITransportLayer
        where TTerminal : ITerminal
    {
        public event EventHandler<PostStateInfoEventArgs> PaymentCompleted;

        public event EventHandler<ExceptionEventArgs> ExceptionThrown;

        private ITransportLayer MyTransportLayer = null;
        private ITerminal MyTerminal = null;

        public ITerminal Terminal { get { return MyTerminal; } }

        public SimpleTerminal()
        {
            MyTransportLayer =
                (ITransportLayer)typeof(TTransport).
                GetConstructor(Type.EmptyTypes).
                Invoke(null);
            MyTerminal =
                (ITerminal)typeof(TTerminal).
                GetConstructor(new Type[] { typeof(TTransport) }).
                Invoke(new object[] { MyTransportLayer });
            MyTerminal.PostStateInfo += new EventHandler<PostStateInfoEventArgs>(MyComTerminal_PostStateInfo);

            MyTransportLayer.Open();
        }

        public SimpleTerminal(ITransportLayer MyTransportLayer)
        {
            this.MyTransportLayer = MyTransportLayer;

            MyTerminal =
                (ITerminal)typeof(TTerminal).
                GetConstructor(new Type[] { typeof(TTransport) }).
                Invoke(new object[] { MyTransportLayer });
            MyTerminal.PostStateInfo += new EventHandler<PostStateInfoEventArgs>(MyComTerminal_PostStateInfo);

            MyTransportLayer.Open();
        }

        /// <summary>
        /// Starts the payment process
        /// </summary>
        /// <param name="MyCurrency">The currency</param>
        /// <param name="intCents">The amount in cents</param>
        /// <param name="blnTerminalPrintsReceipt">Defines whether the terminal should print the receipt on its own</param>
        /// <param name="MyPaymentType">The payment method</param>
        public void Payment(Enums.Currencies MyCurrency, int intCents, bool blnTerminalPrintsReceipt, Enums.PaymentTypes MyPaymentType, Enums.ConfigByte MyAddConfigByte = Enums.ConfigByte.None)
        {
            new Thread(new ThreadStart(delegate ()
                {
                    try
                    {
                        Enums.ConfigByte MyConfigByte = Enums.ConfigByte.POSControlsPayment;
                        if (!blnTerminalPrintsReceipt)
                            MyConfigByte |= Enums.ConfigByte.POSPrintsPayment;
                        MyConfigByte |= MyAddConfigByte;

                        if (SettingsManager.Empty(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Password))
                            throw new TerminalWrongConfigException("Password is missing");

                        MyTerminal.Register(MyConfigByte, MyCurrency, SettingsManager.GetValue<string>(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Password));
                        MyTerminal.Authenticate(intCents, MyCurrency, MyPaymentType);

#if DEBUG
                        Debug.WriteLine(TraceHelper.Dump());
#endif
                    }
                    catch (Exception ex)
                    {
                        OnExceptionThrown(new ExceptionEventArgs(ex));
                    }
                })).Start();
        }

        public void Dispose()
        {
            MyTerminal.LogOff();

            if (MyTransportLayer != null)
                MyTransportLayer.Dispose();
        }

        /// <summary>
        /// Very SLOW! Can take several seconds!
        /// </summary>
        /// <returns>Whether the terminal is configured and connected correctly</returns>
        public static bool IsReady()
        {
            bool blnReady = false;
            bool blnExceptionThrown = false;

            try
            {
                using (SimpleTerminal<TTerminal, TTransport> MyTerminal = new SimpleTerminal<TTerminal, TTransport>())
                {
                    MyTerminal.ExceptionThrown += delegate (object sender, ExceptionEventArgs evtArgs)
                    {
                        blnExceptionThrown = true;
                    };

                    MyTerminal.Terminal.Register(
                        Enums.ConfigByte.POSControlsPayment | Enums.ConfigByte.POSPrintsPayment,
                        Enums.Currencies.Eur,
                        SettingsManager.GetValue<string>(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Password));
                    MyTerminal.Terminal.LogOff();
                }

                blnReady = true;
            }
            catch (Exception ex)
            {
                new TransportLayerException("Connection to EC Terminal failed. Remove connection settings if no device is connected.", ex).LogInfo();
            }

            return blnReady && !blnExceptionThrown;
        }

        protected virtual void OnPaymentCompleted(PostStateInfoEventArgs evtArgs)
        {
            EventHandler<PostStateInfoEventArgs> evtHandler = PaymentCompleted;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnExceptionThrown(ExceptionEventArgs evtArgs)
        {
            EventHandler<ExceptionEventArgs> evtHandler = ExceptionThrown;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        private void MyComTerminal_PostStateInfo(object sender, PostStateInfoEventArgs e)
        {
            OnPaymentCompleted(e);
        }
    }
}