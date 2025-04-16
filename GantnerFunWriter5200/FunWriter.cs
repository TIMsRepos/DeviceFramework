using GAT.Comms.Devices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using TIM.Common.CoreStandard;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.GantnerFunWriter5200
{
    public abstract class FunWriter : IDevice
    {
        #region Events

        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        #endregion

        #region Members

        private delegate T OperationDelegate<out T>();

        protected string LastMediaID;
        protected Dictionary<string, object> Values;
        protected object GatWriterLock = new object();
        protected string StrName = null;
        protected string FID;
        protected string SubFID;
        protected string Key;

        #endregion

        #region Properties

        public abstract string Name { get; }

        public Type[] SupportedContentTypes
        {
            get { return new[] { new int().GetType() }; }
        }

        public bool MediaAvailable
        {
            get
            {
                return DoOperation(() => CardInReader);
            }
        }

        public bool IsWriteable => false;

        public bool SupportsCash => false;

        public bool SupportsContent => true;

        public bool SupportsTracking => true;

        public bool SupportsRequest => true;

        public bool MediaSearchEnabled
        {
            get => GatWriterMediaSearchEnabled;
            set => GatWriterMediaSearchEnabled = value;
        }

        protected abstract bool GatWriterMediaSearchEnabled { get; set; }

        protected abstract bool AllowUseTimer { get; }

        public bool SupportsTrigger => false;

        public bool SupportsToggle => false;

        public abstract ConfigString ConfigString { get; set; }

        public abstract ConfigStringDescription[] ConfigStringDescriptions { get; }

        public DeviceCapabilities DeviceCapabilities => DeviceCapabilities.Identification;

        protected abstract bool CardInReader { get; }

        protected abstract string GetCardUniqueNumber { get; }

        public bool DeviceIsOpen { get; set; }

        // setting detail id Key for this device
        protected abstract Enums.SettingDetail SettingDetailIdKey { get; }

        // setting detail id FID for this device
        protected abstract Enums.SettingDetail SettingDetailIdFID { get; }

        // setting detail id SubFID for this device
        protected abstract Enums.SettingDetail SettingDetailIdSubFID { get; }

        public Framework.Common.Enumerations.DeviceIDs DeviceID => FunWriterDeviceID;

        protected abstract Framework.Common.Enumerations.DeviceIDs FunWriterDeviceID { get; }

        public int DeviceFrameworkDeviceID => GATWriterDeviceFrameworkDeviceID;

        protected abstract int GATWriterDeviceFrameworkDeviceID { get; }

        public string Message { get; set; }

        public bool IsConnected => DeviceIsOpen;

        public bool SupportsUnblockLockerKey => true;

        #endregion

        #region Constructors

        protected FunWriter()
        {
            Values = new Dictionary<string, object>();
            LoadConfig();
        }

        #endregion

        #region Methods

        private void LoadConfig()
        {
            FID = SettingsManager.GetValue(SettingDetailIdFID);
            SubFID = SettingsManager.GetValue(SettingDetailIdSubFID);
            Key = SettingsManager.GetValue(SettingDetailIdKey);
        }

        public abstract bool SelfConfig();

        public void Request()
        {
            var mediaID = GetCardUniqueNumber;

            if (string.IsNullOrEmpty(mediaID))
                return;

            Values.Clear();
            // Avoid deadlocks of Client > Server > Client calls
            new Thread(delegate ()
            {
                Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>();
                dicPayload.Add(Payloads.MediaID, mediaID);

                OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload, DeviceFrameworkDeviceID));
            }).Start();
            LastMediaID = mediaID;
        }

        private T[] GetArrayRange<T>(T[] array, int intOffset, int intLength)
        {
            var newArray = new T[intLength];

            for (int k = 0, i = intOffset; i < intOffset + intLength; ++i, ++k)
                newArray[k] = array[i];

            return newArray;
        }

        private T DoOperation<T>(OperationDelegate<T> MyDelegate, bool blnSilent = false)
        {
            var MyObject = default(T);

            try
            {
                if (!blnSilent)
                {
                    SetGreenLightOnOrOff(false);
                    SetRedLightOnOrOff(true);
                }

                MyObject = MyDelegate();

                if (!blnSilent)
                {
                    SetGreenLightOnOrOff(true);
                    SetRedLightOnOrOff(false);
                }
            }
            catch (Exception ex)
            {
                if (!blnSilent)
                {
                    SetGreenLightOnOrOff(false);
                    SetRedLightOnOrOff(true);
                }
                if (ex is MissingMediaException)
                {
                    if (!blnSilent)
                    {
                        SetGreenLightOnOrOff(true);
                        SetRedLightOnOrOff(false);
                    }
                    throw;
                }
                throw new DeviceException("Error while working with device", ex);
            }

            return MyObject;
        }

        public virtual void Open()
        {
            SetGreenLightOnOrOff(true);
            SetRedLightOnOrOff(false);
        }

        public virtual void Close()
        {
            MediaSearchEnabled = false;

            SetGreenLightOnOrOff(false);
            SetRedLightOnOrOff(false);
        }

        public string GetMediaID()
        {
            string strReturn = DoOperation(delegate
            {
                if (!Values.ContainsKey("mediaid"))
                {
                    var tempMediaID = GetCardUniqueNumber.Trim();
                    if (string.IsNullOrEmpty(tempMediaID))
                        throw new MissingMediaException();

                    Values.Add("mediaid", tempMediaID);
                }
                string strMediaID = (string)Values["mediaid"];

                if (!CardInReader)
                    throw new MissingMediaException();

                return strMediaID;
            });

            return strReturn;
        }

        public object GetMediaContent()
        {
            return DoOperation<object>(delegate
            {
                if (!MediaAvailable)
                    throw new MissingMediaException();

                if (!Values.ContainsKey("mediacontent"))
                {
                    byte[] bytUser = ReadSegmentBlock((int)CardReadersHelper.GATSegments.User, 1);
                    string strHex = GAT.Utils.Functions.GetHexStringFromByteArray(GetArrayRange(bytUser, 6, 4));
                    Values.Add("mediacontent", GAT.Utils.NumericFunctions.ConvertBCDHexStringToLong(strHex));
                }
                var lngContent = (ulong)Values["mediacontent"];

                if (!MediaAvailable)
                    throw new MissingMediaException();

                return lngContent;
            });
        }

        public abstract byte[] ReadSegmentBlock(int start, byte count);

        public float GetMediaCash()
        {
            return DoOperation<float>(delegate
            {
                if (!MediaAvailable)
                    throw new MissingMediaException();

                throw new NotImplementedException();
            });
        }

        public void SetMediaContent(object objContent)
        {
            DoOperation<bool>(delegate
            {
                CheckContentType(objContent.GetType());
                if (!MediaAvailable)
                    throw new MissingMediaException();

                throw new NotImplementedException();
            });
        }

        public void SetMediaCash(float fltCash)
        {
            DoOperation<bool>(delegate
            {
                if (!MediaAvailable)
                    throw new MissingMediaException();

                throw new NotImplementedException();
            });
        }

        public void Trigger(TimeSpan tsActive)
        {
            throw new NotSupportedException();
        }

        public void Toggle(TimeSpan tsActive, TimeSpan tsBreak)
        {
            throw new NotSupportedException();
        }

        public void TriggerOrToggle()
        {
            throw new NotSupportedException();
        }

        private void CheckContentType(Type type)
        {
            if (!Array.Exists(SupportedContentTypes, searchType => searchType == type))
            {
                throw new DeviceException("Content type is not supported");
            }
        }

        public abstract void SetGreenLightOnOrOff(bool on);

        public abstract void SetRedLightOnOrOff(bool on);

        public bool AllowOpenDevice(List<DeviceDetail> deviceDetails, List<int> usedDeviceDetailIDs)
        {
            return GATWriterAllowOpenDevice(deviceDetails, usedDeviceDetailIDs);
        }

        protected abstract bool GATWriterAllowOpenDevice(List<DeviceDetail> deviceDetails, List<int> usedDeviceDetailIDs);

        public bool BlockOrUnblockLockerKey(bool unblock)
        {
            return GATWriterUnblockLockerKey(unblock);
        }

        protected abstract bool GATWriterUnblockLockerKey(bool unblock);

        protected abstract string GetLockerInfo();

        internal void GatWriterMediaDetected(Dictionary<Payloads, object> payload)
        {
            OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, payload, DeviceFrameworkDeviceID));
        }

        internal void GatWriterMediaRemoved(Dictionary<Payloads, object> payload)
        {
            OnMediaRemoved(new DeviceMediaRemovedEventArgs2(this, payload, DeviceFrameworkDeviceID));
        }

        #endregion

        #region Event-Fire

        private void OnMediaDetected(DeviceMediaDetectedEventArgs2 e)
        {
            EventHandler<DeviceMediaDetectedEventArgs2> evtHandler = MediaDetected;
            evtHandler?.Invoke(this, e);
        }

        private void OnMediaRemoved(DeviceMediaRemovedEventArgs2 e)
        {
            EventHandler<DeviceMediaRemovedEventArgs2> evtHandler = MediaRemoved;
            evtHandler?.Invoke(this, e);
        }

        #endregion
    }
}