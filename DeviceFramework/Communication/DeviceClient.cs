using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework.Communication
{
    public class DeviceClient : TIMsDevices.IDeviceServerCallback
    {
        #region Events

        /// <summary>
        /// Fires when a media was detected by one of the devices the client is listening to
        /// </summary>
        public event EventHandler<TIMsDevices.MediaDetectedEventArgs> MediaDetected;

        public event EventHandler<TIMsDevices.MediaRemovedEventArgs> MediaRemoved;

        public event EventHandler<TIMsDevices.MediaDetectedEventArgs2> MediaDetected2;

        public event EventHandler<TIMsDevices.MediaRemovedEventArgs2> MediaRemoved2;

        /// <summary>
        /// Fires when a media is missing while reading data
        /// </summary>
        public event EventHandler<MissingMediaEventArgs> MissingMedia;

        public event EventHandler ConnectionLost;

        public event EventHandler Ponged;

        protected virtual void OnMediaDetected(TIMsDevices.MediaDetectedEventArgs e)
        {
            EventHandler<TIMsDevices.MediaDetectedEventArgs> evtHandler = MediaDetected;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnMediaRemoved(TIMsDevices.MediaRemovedEventArgs e)
        {
            EventHandler<TIMsDevices.MediaRemovedEventArgs> evtHandler = MediaRemoved;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnMediaDetected2(TIMsDevices.MediaDetectedEventArgs2 e)
        {
            EventHandler<TIMsDevices.MediaDetectedEventArgs2> evtHandler = MediaDetected2;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnMediaRemoved2(TIMsDevices.MediaRemovedEventArgs2 e)
        {
            EventHandler<TIMsDevices.MediaRemovedEventArgs2> evtHandler = MediaRemoved2;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnMissingMedia(MissingMediaEventArgs e)
        {
            EventHandler<MissingMediaEventArgs> evtHandler = MissingMedia;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnConnectionLost(EventArgs e)
        {
            EventHandler evtHandler = ConnectionLost;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnPonged(EventArgs e)
        {
            EventHandler evtHandler = Ponged;
            evtHandler?.Invoke(this, e);
        }

        #endregion

        #region Fields

        private static bool blnServerNotFoundAlerted = false;
        private ConfigDevices _configDevices;
        private TIMsDevices.DeviceServerClient _client;
        private Control _scope;
        private readonly Func<bool> _scopeCheck = null;
        private bool _enabled;
        private readonly string _name;

        #endregion

        #region Properties

        public static bool ServerNotFoundAlerted => blnServerNotFoundAlerted;

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        /// Checks the client scope conditions
        /// </summary>
        public bool ScopeActive
        {
            get
            {
                bool blnRes;
                if (_scopeCheck == null)
                {
                    Form frmSuper = _scope == null ? null : GetSuperForm(_scope);
                    bool blnSuperNull = frmSuper != null;
                    bool blnSuperContains = frmSuper != null && frmSuper.ContainsFocus;
                    bool blnScopeVisible = _scope != null && _scope.Visible;
                    bool blnScopeEnabled = _scope != null && _scope.Enabled;
                    blnRes = _scope == null ||
                             blnSuperNull && blnSuperContains && blnScopeVisible && blnScopeEnabled;
                }
                else
                    blnRes = _scopeCheck();
                return _enabled && blnRes;
            }
        }

        #endregion

        #region Constructors

        public DeviceClient()
            : this(null, ConfigDevices.All)
        {
        }

        public DeviceClient(Control scope, ConfigDevices configDevices, bool blnEnabled = false)
        {
            if (scope is Form)
                throw new NotSupportedException("ctlScope ist eine Form");

            _enabled = blnEnabled;
            _scope = scope;
            _configDevices = configDevices;

#if DEBUG
            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();
            if (frames != null)
            {
                MethodBase methodBase = frames[3].GetMethod();
                if (methodBase.DeclaringType != null) _name = $"{methodBase.DeclaringType.FullName}.{methodBase.Name}";
            }

#endif

            Workflows.InvokeWithTimeout(TIMsDevicesHelper.StartAndWaitForServer, 15 * 1000);

            var netTcpBinding = new NetTcpBinding(SecurityMode.None, true)
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                ReliableSession = { InactivityTimeout = TimeSpan.MaxValue }
            };

            _client =
                new TIMsDevices.DeviceServerClient(
                    new InstanceContext(this),
                    netTcpBinding,
                    new EndpointAddress("net.tcp://localhost:54320/DeviceFramework"));

            Connect();

            if (scope != null)
            {
                _scope.Disposed += delegate
                {
                    _client.Abort();
                };
            }
        }

        #endregion

        #region Methods

        private void Connect()
        {
            try
            {
                try
                {
                    _client.AddIDeviceClient2((int)_configDevices, _name, 2);
                }
                catch (FaultException)
                {
                    OnConnectionLost(EventArgs.Empty);
                }
                catch (CommunicationObjectFaultedException)
                {
                    OnConnectionLost(EventArgs.Empty);
                }
            }
            catch (EndpointNotFoundException ex)
            {
                if (!blnServerNotFoundAlerted)
                {
                    blnServerNotFoundAlerted = true;
                    throw new DeviceServerNotFoundException("Server not found!", ex);
                }
            }
        }

        private Form GetSuperForm(Control controlBase)
        {
            if (controlBase is Form form)
                return form;
            if (controlBase.Parent == null)
                return null;

            Control control = controlBase;

            while (!(control.Parent is Form))
                control = control.Parent;

            return control.Parent as Form;
        }

        public string[] GetDeviceNames()
        {
            if (blnServerNotFoundAlerted)
                return new string[0];
            return _client.GetDeviceNames();
        }

        public void ReleaseResources()
        {
            if (_scope != null)
            {
                _scope.Dispose();
                _scope = null;
            }
            _client.Abort();
            _client = null;
        }

        public bool IsFloating()
        {
            if (_scope == null)
                return false;
            return _scope.Parent == null;
        }

        /// <summary>
        /// Fires the <see cref="MediaDetected"/> event. For internal WCF usage only!
        /// </summary>
        /// <param name="e"></param>
        public void FireMediaDetected(TIMsDevices.MediaDetectedEventArgs e)
        {
            if (ScopeActive)
                OnMediaDetected(e);
        }

        public void FireMediaDetected2(TIMsDevices.MediaDetectedEventArgs2 e)
        {
            if (ScopeActive)
            {
                OnMediaDetected2(e);
            }
        }

        /// <summary>
        /// Fires the <see cref="MissingMedia"/> event. For internal WCF usage only!
        /// </summary>
        /// <param name="strDeviceName"></param>
        public void FireMissingMedia(string strDeviceName)
        {
            if (ScopeActive)
                OnMissingMedia(new MissingMediaEventArgs(strDeviceName));
        }

        public void FireMediaRemoved(TIMsDevices.MediaRemovedEventArgs e)
        {
            if (ScopeActive)
                OnMediaRemoved(e);
        }

        public void FireMediaRemoved2(TIMsDevices.MediaRemovedEventArgs2 e)
        {
            if (ScopeActive)
                OnMediaRemoved2(e);
        }

        public void Pong()
        {
            OnPonged(EventArgs.Empty);
        }

        public bool UnblockLockerKey(int deviceFrameworkDeviceID, bool unblock)
        {
            if (blnServerNotFoundAlerted)
                return false;

            return _client.UnblockLockerKey(deviceFrameworkDeviceID, unblock);
        }

        #endregion
    }
}