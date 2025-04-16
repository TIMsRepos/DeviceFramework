using System;
using System.Threading;

namespace TIM.Devices.Framework.Common.Threading
{
    public class LoadTask
    {
        #region Fields

        private string strName;
        private Action MyAction;
        private bool blnForkThread;
        private ManualResetEvent mreResetEvent;

        #endregion

        #region Properties

        public string Name
        {
            get { return strName; }
        }

        public Action Action
        {
            get { return MyAction; }
        }

        public bool ForkThread
        {
            get { return blnForkThread; }
        }

        public ManualResetEvent ResetEvent
        {
            get { return mreResetEvent; }
        }

        #endregion

        #region Constructors

        public LoadTask(string strName, bool blnForkThread, Action MyAction)
        {
            this.strName = strName;
            this.blnForkThread = blnForkThread;
            this.MyAction = MyAction;
            this.mreResetEvent = new ManualResetEvent(false);
        }

        #endregion
    }
}