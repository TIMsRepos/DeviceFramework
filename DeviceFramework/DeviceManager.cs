using System;
using System.Collections.Generic;
using System.Linq;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Extensions;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.Database;

namespace TIM.Devices.Framework
{
    public class DeviceManager
    {
        #region Events

        /// <summary>
        /// Event gets fired if one of the loaded devices has found a media
        /// </summary>
        public event EventHandler<DeviceManagerMediaDetectedEventArgs2> MediaDetected;

        /// <summary>
        /// Event gets fired if a media was removed from one of the loaded devices after it was found
        /// </summary>
        public event EventHandler<DeviceManagerMediaRemovedEventArgs2> MediaRemoved;

        private void OnMediaDetected(object sender, DeviceManagerMediaDetectedEventArgs2 e)
        {
            EventHandler<DeviceManagerMediaDetectedEventArgs2> evtHandler = MediaDetected;
            if (evtHandler != null)
            {
                MediaSearchEnabled = false;
                evtHandler(sender, e);
            }
        }

        private void OnMediaRemoved(object sender, DeviceManagerMediaRemovedEventArgs2 e)
        {
            EventHandler<DeviceManagerMediaRemovedEventArgs2> evtHandler = MediaRemoved;
            if (evtHandler != null)
            {
                MediaSearchEnabled = false;
                evtHandler(sender, e);
            }
        }

        #endregion

        #region Fields

        private static readonly object DevicesLock = new object();
        private readonly SortedList<Enums.ConfigDevices, SortedList<int, IDevice>> _loadedConfigDevices;
        private static readonly object GetDeviceLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Enables/disables the search on all devices for media
        /// </summary>
        /// <remarks>Returns false if at least on one device MediaSearchEnabled is false.</remarks>
        public bool MediaSearchEnabled
        {
            set
            {
                lock (DevicesLock)
                {
                    foreach (IDevice device in Devices)
                        device.MediaSearchEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets the list of loaded devices
        /// </summary>
        public List<IDevice> Devices { get; } = new List<IDevice>();

        public IDevice this[Enums.ConfigDevices configDevices, int intDeviceID] => Devices.FirstOrDefault(t => (int)t.DeviceID == intDeviceID);

        public IDevice[] this[Enums.ConfigDevices configDevices]
        {
            get
            {
                List<IDevice> devices = new List<IDevice>();

                foreach (Enums.ConfigDevices checkConfigDevice in Enum.GetValues(typeof(Enums.ConfigDevices)))
                {
                    if (checkConfigDevice != Enums.ConfigDevices.None &&
                        checkConfigDevice != Enums.ConfigDevices.All)
                    {
                        if (configDevices.Contains(checkConfigDevice))
                        {
                            lock (DevicesLock)
                            {
                                foreach (IDevice device in _loadedConfigDevices[checkConfigDevice].Values)
                                {
                                    if (Devices.Contains(device))
                                        devices.Add(device);
                                }
                            }
                        }
                    }
                }

                if (Devices != null && Devices.Any())
                {
                    foreach (var device in Devices)
                    {
                        devices.Add(device);
                    }
                }

                return devices.Distinct().ToArray();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new DeviceManager and loads devices
        /// </summary>
        public DeviceManager()
        {
            _loadedConfigDevices = new SortedList<Enums.ConfigDevices, SortedList<int, IDevice>>();
            foreach (Enums.ConfigDevices configDevice in Enum.GetValues(typeof(Enums.ConfigDevices)))
            {
                if (configDevice != Enums.ConfigDevices.None)
                    _loadedConfigDevices.Add(configDevice, new SortedList<int, IDevice>());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes all loaded devices after usage
        /// </summary>
        public void Close()
        {
            lock (DevicesLock)
            {
                foreach (IDevice device in Devices)
                {
                    if (device.MediaSearchEnabled)
                        device.MediaSearchEnabled = false;
                    device.Close();
                }
            }
        }

        /// <summary>
        /// Requests all loaded devices that support manual requests
        /// </summary>
        /// <returns>Returns whether any device has a media available that will be requested</returns>
        public bool Request()
        {
            bool blnRet = false;

            lock (DevicesLock)
            {
                foreach (IDevice device in Devices)
                {
                    if (device.SupportsRequest)
                    {
                        if (device.MediaAvailable)
                        {
                            blnRet = true;
                            device.Request();
                        }
                    }
                }
            }
            return blnRet;
        }

        public void AddNewDeviceEvents(int id, IDevice device)
        {
            Devices.Add(device);
            device.MediaDetected += MyDevice_MediaDetected;
            device.MediaRemoved += MyDevice_MediaRemoved;
        }

        private void MyDevice_MediaRemoved(object sender, DeviceMediaRemovedEventArgs2 e)
        {
            OnMediaRemoved(sender, new DeviceManagerMediaRemovedEventArgs2(e.Device, GetDeviceID(e.Device), e.Payload, e.DeviceFrameworkDeviceID));
        }

        private void MyDevice_MediaDetected(object sender, DeviceMediaDetectedEventArgs2 e)
        {
            Notifier.PlayAudio(Enums.AudioSamples.Okay);
            DeviceManagerMediaDetectedEventArgs2 evtArgs = new DeviceManagerMediaDetectedEventArgs2(e.Device, GetDeviceID(e.Device), e.Payload, e.DeviceFrameworkDeviceID);
            OnMediaDetected(sender, evtArgs);
        }

        /// <summary>
        /// Gets the DeviceID by an IDevice by lookup in the MyDevices collection
        /// </summary>
        /// <param name="device">The IDevice to get the DeviceID for</param>
        /// <returns>The DeviceID for the IDevice or -1 if not found</returns>
        /// <remarks>Throws exception automatically if DeviceID wasn't found</remarks>
        private int GetDeviceID(IDevice device)
        {
            int intDeviceID = -1;
            lock (DevicesLock)
            {
                foreach (IDevice currentDevice in Devices)
                {
                    if (currentDevice == device)
                    {
                        intDeviceID = (int)currentDevice.DeviceID;
                        break;
                    }
                }
                if (intDeviceID < 0)
                    throw new FrameworkException("The device throwing the event wasn't found in the loaded device list.");
            }

            return intDeviceID;
        }

        /// <summary>
        /// Gets a device by the given type's fullname
        /// </summary>
        /// <param name="strFullName">The type's fullname</param>
        /// <returns>The created device</returns>
        public static IDevice GetDevice(string strFullName)
        {
            return ObjectManager.Get<IDevice>(strFullName);
        }

        public static IDevice GetDeviceByDeviceDetailID(int intDeviceDetailID, ConfigString configString)
        {
            IDevice devDevice;

            lock (GetDeviceLock)
            {
                using (TIMDataClassesDataContext ctx = DBContext.DataContextRead)
                {
                    var device = (from devDetail in ctx.DevicesDetail
                                  where devDetail.DeviceDetailID == intDeviceDetailID
                                  select new { devDetail.DeviceID, devDetail.RequiredSoftwareComponent }).Single();

                    try
                    {
                        devDevice = GetDevice(device.RequiredSoftwareComponent);
                        devDevice.ConfigString = configString;
                        devDevice.Open();
                    }
                    catch (AssemblyMissingException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        ex.LogWarning();
                        devDevice = null;
                    }
                }
            }

            return devDevice;
        }

        #endregion
    }
}