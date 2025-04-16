using System;
using System.Collections.Generic;
using System.Threading;
using TIM.Devices.Framework.Common.Extensions;

namespace TIM.Devices.Framework.Common.Threading
{
    public class Loader
    {
        #region Events

        public event EventHandler ProcessStarted;

        public event EventHandler ProcessCompleted;

        public event EventHandler<LoaderLoadTaskEventArgs> LoadTaskStarted;

        public event EventHandler<LoaderLoadTaskEventArgs> LoadTaskCompleted;

        #endregion

        #region Fields

        private List<LoadTask> lstLoadTasks;

        #endregion

        #region Constructors

        public Loader()
        {
            lstLoadTasks = new List<LoadTask>();
        }

        #endregion

        #region Methods

        public void AddLoadTask(LoadTask MyLoadTask)
        {
            if (MyLoadTask == null)
                throw new ArgumentNullException("LoadTask");
            if (MyLoadTask.Action == null)
                throw new ArgumentNullException("LoadTask.Action");

            lstLoadTasks.Add(MyLoadTask);
        }

        public void Process()
        {
            OnProcessStarted(EventArgs.Empty);

            foreach (LoadTask MyLoadTask in lstLoadTasks)
            {
                if (MyLoadTask.ForkThread)
                {
                    new Thread(new ParameterizedThreadStart(delegate (object objData)
                        {
                            LoadTask MyLocalLoadTask = (LoadTask)objData;

                            OnLoadTaskStarted(new LoaderLoadTaskEventArgs(MyLocalLoadTask));

                            MyLocalLoadTask.Action();
                            MyLocalLoadTask.ResetEvent.Set();

                            OnLoadTaskCompleted(new LoaderLoadTaskEventArgs(MyLocalLoadTask));
                        })).Start(MyLoadTask);
                }
                else
                {
                    OnLoadTaskStarted(new LoaderLoadTaskEventArgs(MyLoadTask));

                    MyLoadTask.Action();
                    MyLoadTask.ResetEvent.Set();

                    OnLoadTaskCompleted(new LoaderLoadTaskEventArgs(MyLoadTask));
                }
            }

            foreach (LoadTask MyLoadTask in lstLoadTasks)
                MyLoadTask.ResetEvent.WaitOne();

            OnProcessCompleted(EventArgs.Empty);
        }

        #endregion

        #region Event-Fire

        private void OnProcessStarted(EventArgs evtArgs)
        {
            EventHandler evtHandler = ProcessStarted;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        private void OnProcessCompleted(EventArgs evtArgs)
        {
            EventHandler evtHandler = ProcessCompleted;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        private void OnLoadTaskStarted(LoaderLoadTaskEventArgs evtArgs)
        {
            EventHandler<LoaderLoadTaskEventArgs> evtHandler = LoadTaskStarted;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        private void OnLoadTaskCompleted(LoaderLoadTaskEventArgs evtArgs)
        {
            EventHandler<LoaderLoadTaskEventArgs> evtHandler = LoadTaskCompleted;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        #endregion
    }
}