using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using TIM.Common.CoreStandard;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.FeigIDRWA02
{
    // ReSharper disable once InconsistentNaming
    public class IDRWA02 : IDevice
    {
        #region Events

        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        #endregion

        #region Constants

        private const string FE_RWA_DLL = "FeRwa.dll";
        private const string FE_COM_DLL = "FeCom.dll";
        private const int BUS_ADDRESS = 255;

        #endregion

        #region P/Invokes

        [DllImport(FE_RWA_DLL)]
        private static extern Int32 FERWA_NewReader(Int32 iPortHnd);

        [DllImport(FE_RWA_DLL)]
        private static extern Int32 FERWA_DeleteReader(Int32 iReaderHnd);

        [DllImport(FE_RWA_DLL)]
        private static extern Int32 FERWA_0x01_MultiJobPollAndState(Int32 iReaderHnd, byte cBusAdr, byte[] cPollAction, byte[] cTrAdr, byte[] cSendPollData, StringBuilder cPollValid, byte[] cRecPollData, int iDataType);

        [DllImport(FE_RWA_DLL)]
        private static extern Int32 FERWA_GetErrorText(Int32 iError, StringBuilder cErrorText);

        [DllImport(FE_RWA_DLL)]
        private static extern Int32 FERWA_0x75_SetOutput(Int32 iReaderHnd, byte cBusAdr, byte cOS, byte cOSF, Int32 iOSTime, Int32 iRelay);

        [DllImport(FE_COM_DLL)]
        private static extern Int32 FECOM_OpenPort(byte[] cPortNr);

        [DllImport(FE_COM_DLL)]
        private static extern Int32 FECOM_ClosePort(Int32 iPortHnd);

        [DllImport(FE_COM_DLL)]
        private static extern Int32 FECOM_GetErrorText(Int32 iErrorCode, StringBuilder cErrorText);

        #endregion

        #region Fields

        private int _comPortHandle; // needs to be Int64 when changing to 64 bit (maybe use IntPtr)
        private int _readerHandle; // needs to be Int64 when changing to 64 bit (maybe use IntPtr)
        private readonly System.Timers.Timer _timer;
        private string _lastMediaID;
        private byte _comPort;
        private uint _baud;
        private int _triggerActive;
        private static readonly object Idrwa02Lock = new object();

        #endregion

        #region Properties

        public string Name => $"Feig ID RWA02 (COM{_comPort})";

        public Type[] SupportedContentTypes => Type.EmptyTypes;

        public bool MediaAvailable
        {
            get
            {
                ReadID(false);
                return !string.IsNullOrEmpty(_lastMediaID);
            }
        }

        public bool IsWriteable => false;

        public bool SupportsCash => false;

        public bool SupportsContent => false;

        public bool SupportsTracking => true;

        public bool SupportsRequest => true;

        public bool SupportsTrigger => true;

        public bool SupportsToggle => false;

        public bool MediaSearchEnabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        public ConfigString ConfigString
        {
            get
            {
                ConfigString MyConfigString = new ConfigString('=', '|', ConfigStringDescriptions);
                MyConfigString["COM"] = _comPort.ToString();
                MyConfigString["BAUD"] = _baud.ToString();
                MyConfigString["TRIGGERACTIVE"] = _triggerActive.ToString();
                return MyConfigString;
            }
            set
            {
                _comPort = byte.Parse(value["COM"]);
                _baud = uint.Parse(value["BAUD"]);
                _triggerActive = int.Parse(value["TRIGGERACTIVE"]);
            }
        }

        public ConfigStringDescription[] ConfigStringDescriptions
        {
            get
            {
                return new[]
                {
                    new ConfigStringDescription("COM", typeof(byte), true, true),
                    new ConfigStringDescription("BAUD", typeof(uint), true, false),
                    new ConfigStringDescription("TRIGGERACTIVE", typeof(uint), true, false)
                };
            }
        }

        public DeviceCapabilities DeviceCapabilities => DeviceCapabilities.Identification;

        public bool DeviceIsOpen { get; set; }

        public Framework.Common.Enumerations.DeviceIDs DeviceID => Framework.Common.Enumerations.DeviceIDs.Feig;

        public string Message { get; set; }

        public bool IsConnected => DeviceIsOpen;

        public int DeviceFrameworkDeviceID { get; }

        public bool SupportsUnblockLockerKey => false;

        #endregion

        #region Constructors

        // ReSharper disable once UnusedMember.Global
        public IDRWA02()
        {
            _timer = new System.Timers.Timer();
            _timer.AutoReset = true;
            _timer.Interval = 200;
            _timer.Elapsed += MyTimer_Elapsed;
            _timer.Enabled = false;
        }

        public IDRWA02(int deviceFrameworkDeviceID)
        {
            _timer = new System.Timers.Timer();
            _timer.AutoReset = true;
            _timer.Interval = 200;
            _timer.Elapsed += MyTimer_Elapsed;
            _timer.Enabled = false;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion

        #region Methods

        public bool SelfConfig()
        {
            if (!SettingsManager.EmptyAll(Enums.ComputerSetting.TIM_Devices_FeigIDRWA02_IDRWA02,
                new[] { Enums.ComputerDetailSetting.COM, Enums.ComputerDetailSetting.Baud }))
            {
                _comPort = SettingsManager.GetValue<byte>(Enums.ComputerSetting.TIM_Devices_FeigIDRWA02_IDRWA02, Enums.ComputerDetailSetting.COM);
                _baud = SettingsManager.GetUintOrPredefined(Enums.ComputerSetting.TIM_Devices_FeigIDRWA02_IDRWA02, Enums.ComputerDetailSetting.Baud, 9600);
                _triggerActive = 200;

                return true;
            }

            return false;
        }

        public void Open()
        {
            SelfConfig();

            lock (Idrwa02Lock)
            {
                byte[] bytComPortArr = GetComPortBytes(_comPort);

                // Open com port
                _comPortHandle = FECOM_OpenPort(bytComPortArr);
                var comPortHandleError = DetectError(_comPortHandle);
                if (!string.IsNullOrWhiteSpace(comPortHandleError))
                {
                    Console.WriteLine(comPortHandleError);
                    DeviceIsOpen = false;
                    return;
                }

                // Open reader
                _readerHandle = FERWA_NewReader(_comPortHandle);
                var readerHandleError = DetectError(_readerHandle);
                if (!string.IsNullOrWhiteSpace(readerHandleError))
                {
                    Console.WriteLine(readerHandleError);
                    DeviceIsOpen = false;
                    return;
                }

                ReadID(false);
                DeviceIsOpen = true;
            }
        }

        public void Close()
        {
            lock (Idrwa02Lock)
            {
                MediaSearchEnabled = false;

                if (_readerHandle > -1)
                    FERWA_DeleteReader(_readerHandle);

                if (_comPortHandle > -1)
                    FECOM_ClosePort(_comPortHandle);
            }
        }

        public void Request()
        {
            ReadID(true);
        }

        public string GetMediaID()
        {
            ReadID(false);

            if (string.IsNullOrEmpty(_lastMediaID))
                throw new MissingMediaException();
            return _lastMediaID;
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

        /// <summary>
        /// If tsActive = 0, turnstile will not be triggered
        /// </summary>
        /// <param name="tsActive"></param>
        public void Trigger(TimeSpan tsActive)
        {
            Debug("Try Triggering");

            // do not trigger if tsActive = 0 => only turnstile GAT Access 6200 needs feedback within 3 seconds
            if (tsActive.TotalMilliseconds <= 0)
                return;

            new Thread(delegate ()
            {
                try
                {
                    _timer.Enabled = false;
                    Debug("Try Triggering thread");
                    lock (Idrwa02Lock)
                    {
                        int intRes = 0;

                        for (int i = 0; i < 5; ++i)
                        {
                            Debug("Triggering");
                            intRes = FERWA_0x75_SetOutput(_readerHandle, BUS_ADDRESS, 0, 0, 0, 10);
                            if (intRes == -3030)
                            {
                                Console.WriteLine(@"##### -3030 #####");
                                continue;
                            }
                            break;
                        }
                        DetectError(intRes);
                        Debug("Triggered");
                    }
                }
                finally
                {
                    _timer.Enabled = true;
                }
            }).Start();
        }

        public void Toggle(TimeSpan tsActive, TimeSpan tsBreak)
        {
            throw new NotSupportedException();
        }

        public void TriggerOrToggle()
        {
            Trigger(TimeSpan.FromMilliseconds(_triggerActive));
        }

        private void Debug(string strMessage)
        {
            Console.WriteLine(@"[{0:hh:mm:ss.fff}] (IDRWA02): {1}", DateTime.Now, strMessage);
        }

        private byte[] GetComPortBytes(byte bytComPort)
        {
            string strComPort = bytComPort.ToString();
            char[] chrComPort = new char[4];

            for (int i = 0; i < 4; ++i)
            {
                if (i < strComPort.Length)
                    chrComPort[i] = strComPort[i];
                else
                    chrComPort[i] = '\0';
            }

            return Encoding.ASCII.GetBytes(chrComPort);
        }

        private string DetectError(int intRes)
        {
            StringBuilder sbErrorText = new StringBuilder(256);

            if (intRes < 0 && intRes > -3000)
            {
                FECOM_GetErrorText(intRes, sbErrorText);
                return sbErrorText.ToString();
            }
            if (intRes < -2999)
            {
                FERWA_GetErrorText(intRes, sbErrorText);
                return sbErrorText.ToString();
            }

            return string.Empty;
        }

        private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = string.Format("{1} Pooling ({0})", ConfigString, GetType().Name);

            try
            {
                _timer.Enabled = false;
                lock (Idrwa02Lock)
                {
                    ReadID(true);
                }
            }
            finally
            {
                _timer.Enabled = true;
            }
        }

        private void ReadID(bool blnFireEvents)
        {
            byte[] bytSend = new byte[16];
            byte[] bytRecPollData = new byte[93];
            StringBuilder sbPollValid = new StringBuilder(2);
            byte[] bytReadSerialNumber = { 0x00, 0x02 };
            var intRes = 0;

            for (int i = 0; i < 5; ++i)
            {
                intRes = FERWA_0x01_MultiJobPollAndState(_readerHandle, BUS_ADDRESS, bytReadSerialNumber, new byte[] { 0x00, 0x00 }, bytSend, sbPollValid, bytRecPollData, (int)iDataTypes.String);
                if (intRes == -3030)
                {
                    Console.WriteLine(@"##### -3030 #####");
                    continue;
                }
                break;
            }
            DetectError(intRes);

            // Cut off ANSI-C string null char
            string strMediaID = Encoding.ASCII.GetString(bytRecPollData).Replace("\0", "");
            strMediaID = strMediaID.Length > 0 ? strMediaID.Substring(2) : null;

            if (strMediaID != _lastMediaID)
            {
                if (!string.IsNullOrEmpty(_lastMediaID))
                {
                    // Removed
                    if (blnFireEvents)
                    {
                        Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object> {
                        {
                            Payloads.MediaID, _lastMediaID
                        } };

                        OnMediaRemoved(new DeviceMediaRemovedEventArgs2(this, dicPayload, DeviceFrameworkDeviceID));
                    }
                    Debug("Removed - " + _lastMediaID);
                }

                if (!string.IsNullOrEmpty(strMediaID))
                {
                    // Detected
                    if (blnFireEvents)
                    {
                        Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object> {
                        {
                            Payloads.MediaID, strMediaID
                        } };

                        OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload, DeviceFrameworkDeviceID));
                    }
                    Debug("Detected - " + strMediaID);
                }

                _lastMediaID = strMediaID;
            }
        }

        public bool AllowOpenDevice(List<DeviceDetail> deviceDetails, List<int> usedDeviceDetailIDs)
        {
            foreach (var deviceDetail in deviceDetails.Where(t => t.DeviceID == (int)DeviceID && t.RequiredSoftwareComponent.Contains(GetType().Name)))
            {
                if (usedDeviceDetailIDs.Contains(deviceDetail.DeviceDetailID))
                    return false;
            }

            return true;
        }

        public bool BlockOrUnblockLockerKey(bool unblock)
        {
            return true;
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