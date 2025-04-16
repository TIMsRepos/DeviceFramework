using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.Helper;
using TIM.Common.Data.Factories;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework.Communication
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true)]
    public class DeviceServer : IDeviceServer, IDisposable
    {
        #region Events

        public event EventHandler ClientsChanged;

        public event EventHandler ClientAdded;

        public event EventHandler ClientRemoved;

        #endregion

        #region Fields

        private DeviceManager _deviceManager;
        private readonly LinkedList<DeviceSubscription> _subscriptions;
        private readonly object _subscription;
        private readonly System.Timers.Timer _tmrGc;
        private readonly System.Timers.Timer _tmrPong;

        #endregion

        #region Properties

        public DeviceManager DeviceManager
        {
            get
            {
                if (_deviceManager == null)
                {
                    try
                    {
                        _deviceManager = new DeviceManager();
                        _deviceManager.MediaDetected += MyDeviceManager_MediaDetected;
                        _deviceManager.MediaRemoved += MyDeviceManager_MediaRemoved;
                        _deviceManager.MediaSearchEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        MyExceptions.Add(ex);
                        throw;
                    }
                }

                return _deviceManager;
            }
        }

        public int ClientCount => _subscriptions.Count;

        public static List<Exception> MyExceptions { get; } = new List<Exception>();

        #endregion

        #region Constructors

        public DeviceServer()
        {
            _subscriptions = new LinkedList<DeviceSubscription>();
            _subscription = new object();

            _tmrGc = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = 1000 * DbControllers.SettingDetails.GetValueFromCache(Enums.SettingDetail.DeviceServer_GarbageCollectionInterval).ToInteger(15)
            };
            _tmrGc.Elapsed += tmrGC_Elapsed;
            _tmrGc.Enabled = true;

            _tmrPong = new System.Timers.Timer { AutoReset = true, Interval = 1000 };
            _tmrPong.Elapsed += tmrPong_Elapsed;
            _tmrPong.Enabled = true;
        }

        #endregion

        #region Methods

        private void IterateClientsWithLocks(Action<LinkedListNode<DeviceSubscription>> action)
        {
            Monitor.Enter(_subscription);

            LinkedList<DeviceSubscription> subs = new LinkedList<DeviceSubscription>(_subscriptions);

            Monitor.Exit(_subscription);

            while (subs.Count > 0)
            {
                LinkedListNode<DeviceSubscription> node;
                for (node = subs.First; node != null; node = node.Next)
                {
                    var locked = false;
                    try
                    {
                        locked = Monitor.TryEnter(node.Value.DeviceClientLock);
                        if (locked)
                        {
                            action(node);
                            subs.Remove(node);
                            break;
                        }
                    }
                    catch (TimeoutException)
                    {
                        RemoveClient(node);
                        subs.Remove(node);
                    }
                    catch (ObjectDisposedException)
                    {
                        RemoveClient(node);
                        subs.Remove(node);
                    }
                    catch (CommunicationException)
                    {
                        RemoveClient(node);
                        subs.Remove(node);
                    }
                    finally
                    {
                        if (locked)
                            Monitor.Exit(node.Value.DeviceClientLock);
                    }
                }
            }
        }

        public static void ClearExceptions()
        {
            MyExceptions.Clear();
        }

        private void tmrPong_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _tmrPong.Enabled = false;

                IterateClientsWithLocks(delegate (LinkedListNode<DeviceSubscription> node)
                {
                    try
                    {
                        node.Value.DeviceClient.Pong();
                    }
                    catch (ActionNotSupportedException)
                    {
                    }
                });
            }
            finally
            {
                _tmrPong.Enabled = true;
            }
        }

        private void tmrGC_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _tmrGc.Enabled = false;

                IterateClientsWithLocks(delegate (LinkedListNode<DeviceSubscription> node)
                {
                    if (node.Value.DeviceClient.IsFloating())
                    {
                        node.Value.DeviceClient.ReleaseResources();
                        RemoveClient(node);
                    }
                });
            }
            finally
            {
                _tmrGc.Enabled = true;
            }
        }

        private void MyDeviceManager_MediaDetected(object sender, DeviceManagerMediaDetectedEventArgs2 e)
        {
            bool blnDetected = true;

            MediaDetectedEventArgs evtArgs = null;
            MediaDetectedEventArgs2 evtArgs2 = null;

            try
            {
                evtArgs = new MediaDetectedEventArgs(new DeviceManagerEventArgs(e.Device, e.DeviceID, e.Payload[Payloads.MediaID].ToString()));
                Console.WriteLine(@"D: {0} => {1}", DateTime.FromBinary(evtArgs.Date), evtArgs.ECheckInID);
            }
            catch (MissingMediaException)
            {
                blnDetected = false;
            }
            try
            {
                evtArgs2 = new MediaDetectedEventArgs2(e);
                Console.WriteLine(@"D: {0} => {1}", DateTime.FromBinary(evtArgs2.Date), evtArgs2.ECheckInID);
            }
            catch (MissingMediaException)
            {
                blnDetected = false;
            }

            IterateClientsWithLocks(delegate (LinkedListNode<DeviceSubscription> node)
            {
                if (e.Device != null)
                {
                    if (blnDetected)
                    {
                        switch (node.Value.Version)
                        {
                            case 1:
                                node.Value.DeviceClient.FireMediaDetected(evtArgs);
                                Console.WriteLine(@"DeviceServer: FireMediaDetected");
                                break;

                            case 2:
                                node.Value.DeviceClient.FireMediaDetected2(evtArgs2);
                                Console.WriteLine(@"DeviceServer: FireMediaDetected2");
                                break;
                        }
                    }
                    else
                        node.Value.DeviceClient.FireMissingMedia(e.Device.Name);
                }
            });

            DeviceManager.MediaSearchEnabled = true;
            e.Device.MediaSearchEnabled = true;
        }

        private void MyDeviceManager_MediaRemoved(object sender, DeviceManagerMediaRemovedEventArgs2 e)
        {
            MediaRemovedEventArgs evtArgs = new MediaRemovedEventArgs(e.DeviceID, e.Device.Name, e.Payload[Payloads.MediaID].ToString());
            MediaRemovedEventArgs2 evtArgs2 = new MediaRemovedEventArgs2(e);

            Console.WriteLine(@"R: {0} => {1}", DateTime.FromBinary(evtArgs2.Date), evtArgs2.ECheckInID);

            IterateClientsWithLocks(delegate (LinkedListNode<DeviceSubscription> node)
            {
                if (DeviceManager[node.Value.ConfigDevices, e.DeviceID] != null)
                {
                    bool blnFired = false;
                    try
                    {
                        node.Value.DeviceClient.FireMediaRemoved2(evtArgs2);
                        blnFired = true;
                    }
                    catch
                    {
                        //ignored
                    }
                    if (!blnFired)
                        node.Value.DeviceClient.FireMediaRemoved(evtArgs);
                }
            });

            DeviceManager.MediaSearchEnabled = true;
        }

        private void RemoveClient(LinkedListNode<DeviceSubscription> node)
        {
            Monitor.Enter(_subscription);

            string strName = node.Value.Signature;
            _subscriptions.Remove(node.Value);
            OnClientRemoved(EventArgs.Empty);
            OnClientsChanged(EventArgs.Empty);
#if DEBUG
            Console.WriteLine(@"{1} :: Client removed '{0}'", strName, DateTime.Now.ToFullTimeString());
#endif

            Monitor.Exit(_subscription);
        }

        public int AddIDeviceClient(int intConfigDevices, string strName)
        {
            return AddIDeviceClient2(intConfigDevices, strName, 1);
        }

        public int AddIDeviceClient2(int intConfigDevices, string strName, byte bytVersion)
        {
            Monitor.Enter(_subscription);

            DeviceSubscription deviceSubscription =
                new DeviceSubscription(OperationContext.Current.GetCallbackChannel<IDeviceClient>(),
                    (Enums.ConfigDevices)intConfigDevices, strName, bytVersion);

            if (!_subscriptions.Contains(deviceSubscription))
            {
                _subscriptions.AddFirst(deviceSubscription);
                OnClientAdded(EventArgs.Empty);
                OnClientsChanged(EventArgs.Empty);
#if DEBUG
                Console.WriteLine(@"{1} :: Client added   '{0}' Version {2}", strName, DateTime.Now.ToFullTimeString(), bytVersion);
#endif
            }

            Monitor.Exit(_subscription);

            return _subscriptions.Count;
        }

        public string[] GetDeviceNames()
        {
            Monitor.Enter(_subscription);
            string[] strNames = null;
            if (_subscriptions.Any())
            {
                DeviceSubscription deviceSubscription = _subscriptions.Single(pair => pair.DeviceClient == OperationContext.Current.GetCallbackChannel<IDeviceClient>());
                Enums.ConfigDevices configDevices = deviceSubscription.ConfigDevices;
                IDevice[] devices = DeviceManager[configDevices];
                strNames = new string[devices.Length];

                for (var i = 0; i < devices.Length; ++i)
                {
                    strNames[i] = devices[i].Name;
                }
            }
            Monitor.Exit(_subscription);

            return strNames;
        }

        public bool Request()
        {
            return DeviceManager.Request();
        }

        public void Dispose()
        {
            DeviceManager?.Close();
        }

        protected virtual void OnClientsChanged(EventArgs e)
        {
            EventHandler evtHandler = ClientsChanged;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnClientAdded(EventArgs e)
        {
            EventHandler evtHandler = ClientAdded;
            evtHandler?.Invoke(this, e);
        }

        protected virtual void OnClientRemoved(EventArgs e)
        {
            EventHandler evtHandler = ClientRemoved;
            evtHandler?.Invoke(this, e);
        }

        public bool UnblockLockerKey(int deviceFrameworkDeviceID, bool unblock)
        {
            var device = DeviceManager.Devices.FirstOrDefault(t => t.DeviceFrameworkDeviceID == deviceFrameworkDeviceID);

            if (device == null)
                return false;

            return device.BlockOrUnblockLockerKey(unblock);
        }

        #endregion
    }
}