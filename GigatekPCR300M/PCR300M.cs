using PCR300XLib;
using System;
using System.Collections.Generic;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.GigatekPCR300M
{
    public class PCR300M : IDevice
    {
        #region Events

        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        #endregion

        #region Members

        private AxPCR300XLib.AxPCR300x MyAxPCR300x;
        private frmOCXHelper MyFrmOCXHelper;
        private string strLastID;
        private byte bytCOMPort;
        private bool blnMediaSearchEnabled;

        #endregion

        #region Properties

        public string Name
        {
            get { return string.Format("Gigatek Promag PCR300M (COM{0})", bytCOMPort); }
        }

        public Type[] SupportedContentTypes
        {
            get { return Type.EmptyTypes; }
        }

        public bool MediaAvailable
        {
            get { return false; }
        }

        public bool IsWriteable
        {
            get { return false; }
        }

        public bool SupportsCash
        {
            get { return false; }
        }

        public bool SupportsContent
        {
            get { return false; }
        }

        public bool SupportsTracking
        {
            get { return true; }
        }

        public bool SupportsRequest
        {
            get { return false; }
        }

        public bool SupportsTrigger
        {
            get { return false; }
        }

        public bool SupportsToggle
        {
            get { return false; }
        }

        public bool MediaSearchEnabled
        {
            get { return blnMediaSearchEnabled; }
            set { blnMediaSearchEnabled = value; }
        }

        public ConfigString ConfigString
        {
            get
            {
                ConfigString MyConfigString = new ConfigString('=', '|', ConfigStringDescriptions);
                MyConfigString["COM"] = bytCOMPort.ToString();
                return MyConfigString;
            }
            set
            {
                bytCOMPort = byte.Parse(value["COM"]);
            }
        }

        public ConfigStringDescription[] ConfigStringDescriptions
        {
            get
            {
                return new ConfigStringDescription[]
                {
                    new ConfigStringDescription("COM", typeof(byte), true, true)
                };
            }
        }

        public DeviceCapabilities DeviceCapabilities
        {
            get { return DeviceCapabilities.Identification; }
        }

        public bool DeviceIsOpen { get; set; }

        #endregion

        #region Constructors

        public PCR300M()
        {
            MyFrmOCXHelper = new frmOCXHelper();
            MyAxPCR300x = MyFrmOCXHelper.AxPCR300x;
            MyAxPCR300x.OnIdChange += new AxPCR300XLib._DPCR300xEvents_OnIdChangeEventHandler(MyAxPCR300x_OnIdChange);
            MyAxPCR300x.StatusChange += new AxPCR300XLib._DPCR300xEvents_StatusChangeEventHandler(MyAxPCR300x_StatusChange);
        }

        #endregion

        #region Methods

        private void MyAxPCR300x_OnIdChange(object sender, AxPCR300XLib._DPCR300xEvents_OnIdChangeEvent e)
        {
            strLastID = e.code;

            Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>();
            dicPayload.Add(Payloads.MediaID, strLastID);

            OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload));
        }

        private void MyAxPCR300x_StatusChange(object sender, AxPCR300XLib._DPCR300xEvents_StatusChangeEvent e)
        {
            Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>();
            dicPayload.Add(Payloads.MediaID, strLastID);

            OnMediaRemoved(new DeviceMediaRemovedEventArgs2(this, dicPayload));
        }

        public bool SelfConfig()
        {
            if (!SettingsManager.EmptyAll(Enums.ComputerSetting.TIM_Devices_GigatekPCR300M_PCR300M,
                new Enums.ComputerDetailSetting[] { Enums.ComputerDetailSetting.COM }))
            {
                bytCOMPort = SettingsManager.GetValue<byte>(Enums.ComputerSetting.TIM_Devices_GigatekPCR300M_PCR300M, Enums.ComputerDetailSetting.COM);

                return true;
            }
            else
                return false;
        }

        public void Open()
        {
            if (bytCOMPort < 1 || bytCOMPort > 4)
                throw new DeviceException("COM Port setting needs to be in range 1-4!");
            MyAxPCR300x.CommPort = (COMPORT)Enum.Parse(typeof(COMPORT), "COM" + bytCOMPort.ToString());

            MyAxPCR300x.PortOpen = true;
        }

        public void Close()
        {
            MyAxPCR300x.PortOpen = false;
        }

        public void Request()
        {
            throw new NotSupportedException();
        }

        public string GetMediaID()
        {
            return strLastID;
        }

        public object GetMediaContent()
        {
            throw new NotSupportedException();
        }

        public float GetMediaCash()
        {
            throw new NotSupportedException();
        }

        public void SetMediaContent(object objContent)
        {
            throw new NotSupportedException();
        }

        public void SetMediaCash(float fltCash)
        {
            throw new NotSupportedException();
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

        protected virtual void OnMediaDetected(DeviceMediaDetectedEventArgs2 e)
        {
            if (blnMediaSearchEnabled)
            {
                EventHandler<DeviceMediaDetectedEventArgs2> evtHandler = MediaDetected;
                if (evtHandler != null)
                    evtHandler(this, e);
            }
        }

        protected virtual void OnMediaRemoved(DeviceMediaRemovedEventArgs2 e)
        {
            EventHandler<DeviceMediaRemovedEventArgs2> evtHandler = MediaRemoved;
            if (evtHandler != null)
                evtHandler(this, e);
        }

        #endregion
    }
}