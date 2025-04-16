using System;
using System.Collections.Generic;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework.Turnstiles
{
    public class TurnstileEventArgs : EventArgs
    {
        #region Fields

        private ITurnstile MyTurnstile;
        private Dictionary<Enums.Payloads, object> dicPayload;

        #endregion

        #region Properties

        public ITurnstile Turnstile
        {
            get { return MyTurnstile; }
            set { MyTurnstile = value; }
        }

        public Dictionary<Enums.Payloads, object> Payload
        {
            get { return dicPayload; }
        }

        #endregion

        #region Constructors

        public TurnstileEventArgs(ITurnstile MyTurnstile, Dictionary<Enums.Payloads, object> dicPayload)
        {
            this.dicPayload = dicPayload;
            this.MyTurnstile = MyTurnstile;
        }

        #endregion
    }
}