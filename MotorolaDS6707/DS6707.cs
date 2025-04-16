using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TIM.Common.CoreStandard;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.MotorolaDS6707.MotoSNAPI;

namespace TIM.Devices.MotorolaDS6707
{
    public class DS6707 : IDevice
    {
        #region Events

        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        #endregion

        #region Members

        private static readonly Regex RegexArchiveDataMatrix = new Regex(Constants.Regex.ArchiveDataMatrix);
        private SnapiScanner _scanner = null;
        private int _lastMediaID = -1;
        private string _lastContent = null;
        private bool _mediaSearchEnabled = false;

        #endregion

        #region Getter & Setter

        public string Name => DeviceIsOpen ? string.Format("Motorola DS6707 ({0})", _scanner.Name) : string.Empty;

        public Type[] SupportedContentTypes
        {
            get { return new Type[0]; }
        }

        public bool MediaAvailable
        {
            get { throw new NotSupportedException(); }
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
            get { return false; }
        }

        public bool SupportsRequest
        {
            get { return false; }
        }

        public bool MediaSearchEnabled
        {
            get { return _mediaSearchEnabled; }
            set
            {
                SetParam((int)Parameters.TriggerMode, (int)(value ? TriggerModes.Blink : TriggerModes.Host));
                _mediaSearchEnabled = value;
            }
        }

        public ConfigString ConfigString
        {
            get { return new ConfigString('=', '|', ConfigStringDescriptions); }
            set { }
        }

        public ConfigStringDescription[] ConfigStringDescriptions
        {
            get { return new ConfigStringDescription[0]; }
        }

        public bool SupportsTrigger
        {
            get { return false; }
        }

        public bool SupportsToggle
        {
            get { return false; }
        }

        public DeviceCapabilities DeviceCapabilities
        {
            get { return DeviceCapabilities.Identification | DeviceCapabilities.Vouchers; }
        }

        public bool DeviceIsOpen { get; set; }

        public Framework.Common.Enumerations.DeviceIDs DeviceID => Framework.Common.Enumerations.DeviceIDs.Motorola;

        public string Message { get; set; }

        public bool IsConnected => DeviceIsOpen;

        public int DeviceFrameworkDeviceID { get; }

        public bool SupportsUnblockLockerKey => false;

        #endregion

        #region Constructor

        // ReSharper disable once UnusedMember.Global
        public DS6707()
        {
        }

        public DS6707(int deviceFrameworkDeviceID)
        {
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion

        #region Methods

        public bool SelfConfig()
        {
            return true;
        }

        public void Open()
        {
            _scanner = null;
            try
            {
                foreach (SnapiScanner MyScannerTmp in SnapiScanner.AttachedScanners)
                {
                    _scanner = MyScannerTmp;
                    DeviceIsOpen = true;
                    break;
                }
            }
            catch (Exception)
            {
                DeviceIsOpen = false;
                throw new DeviceException();
            }

            if (_scanner == null)
            {
                DeviceIsOpen = false;
                throw new DeviceException("Cannot find any attached scanner device!");
            }

            if (!_scanner.Claim())
            {
                DeviceIsOpen = false;
                throw new DeviceException("Could not claim scanner!");
            }

            MediaSearchEnabled = false;
            _scanner.Decode += MyScanner_Decode;
            _scanner.ScannerEnabled = true;
        }

        private void MyScanner_Decode(object sender, EventArgs e)
        {
            DecodeData MyDecodeData = (DecodeData)e;
            switch (MyDecodeData.CodeType)
            {
                case DecodeData.CodeTypes.DataMatrix:
                    {
                        Match MyMatch = RegexArchiveDataMatrix.Match(MyDecodeData.Text);
                        if ((MyDecodeData.Text.Length > 15 && MyDecodeData.Text.Length < 38) &&
                            MyMatch.Success)
                        {
                            _lastContent = MyDecodeData.Text;
                            _lastMediaID = int.Parse(MyMatch.Groups[2].Value);
                            int intDocTypeID = int.Parse(MyMatch.Groups["DocTypeID"].Value);
                            int intID = int.Parse(MyMatch.Groups["ID"].Value);

                            Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>();
                            dicPayload.Add(Payloads.CodeType, MyDecodeData.CodeType.ToString());
                            dicPayload.Add(Payloads.MediaID, _lastContent);
                            dicPayload.Add(Payloads.DocTypeID, intDocTypeID);
                            dicPayload.Add(Payloads.PersonID, intID);
                            if (MyMatch.Groups["Field1"].Success &&
                                MyMatch.Groups["Field1"].Value.Length > 0)
                                dicPayload.Add(Payloads.DataMatrixField1, MyMatch.Groups["Field1"].Value);
                            if (MyMatch.Groups["Field2"].Success &&
                                MyMatch.Groups["Field2"].Value.Length > 0)
                                dicPayload.Add(Payloads.DataMatrixField2, MyMatch.Groups["Field2"].Value);
                            if (MyMatch.Groups["Field3"].Success &&
                                MyMatch.Groups["Field3"].Value.Length > 0)
                                dicPayload.Add(Payloads.DataMatrixField3, MyMatch.Groups["Field3"].Value);
                            if (intDocTypeID == (int)Enums.DocType.Voucher)
                                dicPayload.Add(Payloads.VoucherIDWithCheckDigit, Int64.Parse(dicPayload[Payloads.DataMatrixField1].ToString()));
                            if (intDocTypeID == (int)Enums.DocType.Articles)
                                dicPayload.Add(Payloads.ArticleID, Int64.Parse(dicPayload[Payloads.DataMatrixField1].ToString()));

                            OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload, DeviceFrameworkDeviceID));
                        }
                        break;
                    }
                case DecodeData.CodeTypes.Ean13:
                case DecodeData.CodeTypes.Ean13Plus2:
                case DecodeData.CodeTypes.Ean13Plus5:
                case DecodeData.CodeTypes.Ean8:
                case DecodeData.CodeTypes.Ean8Plus2:
                case DecodeData.CodeTypes.Ean8Plus5:
                case DecodeData.CodeTypes.UpcA:
                case DecodeData.CodeTypes.UpcAPlus2:
                case DecodeData.CodeTypes.UpcAPlus5:
                    {
                        _lastContent = MyDecodeData.Text;

                        // UPC A is compatible with EAN, just add a 0 on the left
                        int lengthUpcA = 12;

                        Int64 lngEan = 0;
                        switch (MyDecodeData.CodeType)
                        {
                            case DecodeData.CodeTypes.Ean13:
                            case DecodeData.CodeTypes.Ean8:
                                lngEan = Int64.Parse(MyDecodeData.Text);
                                break;

                            case DecodeData.CodeTypes.Ean13Plus2:
                                lngEan = Int64.Parse(MyDecodeData.Text.PadLeft(15, '0').Substring(0, 13));
                                break;

                            case DecodeData.CodeTypes.Ean13Plus5:
                                lngEan = Int64.Parse(MyDecodeData.Text.PadLeft(18, '0').Substring(0, 13));
                                break;

                            case DecodeData.CodeTypes.Ean8Plus2:
                                lngEan = Int64.Parse(MyDecodeData.Text.PadLeft(10, '0').Substring(0, 8));
                                break;

                            case DecodeData.CodeTypes.Ean8Plus5:
                                lngEan = Int64.Parse(MyDecodeData.Text.PadLeft(13, '0').Substring(0, 8));
                                break;

                            case DecodeData.CodeTypes.UpcA:
                                lngEan = Int64.Parse(MyDecodeData.Text);
                                break;

                            case DecodeData.CodeTypes.UpcAPlus2:
                                lngEan = Int64.Parse(MyDecodeData.Text.PadLeft(lengthUpcA + 2, '0').Substring(0, lengthUpcA));
                                break;

                            case DecodeData.CodeTypes.UpcAPlus5:
                                lngEan = Int64.Parse(MyDecodeData.Text.PadLeft(lengthUpcA + 5, '0').Substring(0, lengthUpcA));
                                break;
                        }

                        Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>();
                        dicPayload.Add(Payloads.CodeType, MyDecodeData.CodeType.ToString());
                        dicPayload.Add(Payloads.MediaID, MyDecodeData.Text);
                        dicPayload.Add(Payloads.DocTypeID, (int)Enums.DocType.Articles);
                        dicPayload.Add(Payloads.EAN, lngEan);

                        OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload, DeviceFrameworkDeviceID));
                        break;
                    }
                default:
                    break;
            }
        }

        private void SetParam(int intParam, int intValue)
        {
            if (_scanner != null)
            {
                SnapiIntParam MyParam = new SnapiIntParam(intParam, intValue);
                Thread.Sleep(100);
                bool blnResult = _scanner.SetParam(MyParam);
                if (!blnResult)
                    Console.WriteLine(@"Error while setting parameter for trigger mode");
            }
        }

        public void Close()
        {
            if (_scanner != null)
                _scanner.Release();
        }

        public void Request()
        {
            throw new NotSupportedException();
        }

        public string GetMediaID()
        {
            return _lastMediaID < 0 ? null : _lastMediaID.ToString();
        }

        public object GetMediaContent()
        {
            return _lastContent;
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

        protected virtual void OnMediaDetected(DeviceMediaDetectedEventArgs2 e)
        {
            EventHandler<DeviceMediaDetectedEventArgs2> evtHandler = MediaDetected;
            if (evtHandler != null)
                evtHandler(this, e);
        }

        #endregion
    }
}