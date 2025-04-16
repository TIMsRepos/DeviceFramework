using GAT.Comms.Devices;
using GAT.Comms.Devices.CardReader;
using GAT.Comms.Devices.DataCarriers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.DTOs.CheckIn;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.GantnerFunWriter5200
{
    public class FunWriter5200 : FunWriter
    {
        #region Fields

        private byte _comPort;
        private uint _baud;
        private GATWriter5200 _gatWriter;
        private readonly System.Timers.Timer _timer;
        private const int DEFAULT_COUNTER_TIME_OUT = 10;

        #endregion

        #region Properties

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(StrName))
                    StrName = $"GAT Writer 5200 ({_gatWriter.DeviceSerialNumber})";
                return StrName;
            }
        }

        public override ConfigString ConfigString
        {
            get
            {
                ConfigString configString = new ConfigString('=', '|', ConfigStringDescriptions)
                {
                    ["COM"] = _comPort.ToString(),
                    ["BAUD"] = _baud.ToString()
                };
                return configString;
            }
            set
            {
                _comPort = byte.Parse(value["COM"]);
                _baud = uint.Parse(value["BAUD"]);
            }
        }

        public override ConfigStringDescription[] ConfigStringDescriptions
        {
            get
            {
                return new[]
                {
                    new ConfigStringDescription("COM", typeof(byte), true, true),
                    new ConfigStringDescription("BAUD", typeof(uint), true, false)
                };
            }
        }

        protected override bool CardInReader => _gatWriter.CardInReader;

        protected override string GetCardUniqueNumber => _gatWriter.GetCardUniqueNumber();

        // setting detail id Key for this device
        protected override Enums.SettingDetail SettingDetailIdKey => Enums.SettingDetail.GATWriterLegicMasterKey;

        // setting detail id FID for this device
        protected override Enums.SettingDetail SettingDetailIdFID => Enums.SettingDetail.GATWriterFID;

        // setting detail id SubFID for this device
        protected override Enums.SettingDetail SettingDetailIdSubFID => Enums.SettingDetail.GATWriterSubFID;

        protected override Framework.Common.Enumerations.DeviceIDs FunWriterDeviceID => Framework.Common.Enumerations.DeviceIDs.GantnerFunWriterLegic;

        protected override int GATWriterDeviceFrameworkDeviceID { get; }

        protected override bool AllowUseTimer => true;

        protected override bool GatWriterMediaSearchEnabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        #endregion

        #region Constructor

        // ReSharper disable once UnusedMember.Global
        public FunWriter5200()
        {
        }

        public FunWriter5200(int deviceFrameworkDeviceID)
        {
            GATWriterDeviceFrameworkDeviceID = deviceFrameworkDeviceID;

            _timer = new System.Timers.Timer { AutoReset = true, Interval = 200 };
            _timer.Elapsed += MyTimer_Elapsed;
            _timer.Enabled = false;
        }

        #endregion

        #region Methods

        private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!DeviceIsOpen)
            {
                _timer.Enabled = false;
                return;
            }

            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = string.Format("{1} Pooling ({0})", ConfigString, GetType().Name);

            lock (GatWriterLock)
            {
                try
                {
                    _timer.Enabled = false;

                    string mediaID = GetCardUniqueNumber;

                    //if (mediaID != LastMediaID && !string.IsNullOrWhiteSpace(mediaID))
                    if (mediaID != LastMediaID)
                    {
                        if (!string.IsNullOrEmpty(LastMediaID))
                        {
                            // Removed
                            SetGreenLightOnOrOff(false);
                            SetRedLightOnOrOff(true);

                            Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>
                            {
                                { Payloads.MediaID, LastMediaID }
                            };

                            GatWriterMediaRemoved(dicPayload);
                        }

                        if (!string.IsNullOrEmpty(mediaID))
                        {
                            // Detected
                            SetGreenLightOnOrOff(false);
                            SetRedLightOnOrOff(true);

                            Values.Clear();
                            Values.Add("mediaid", mediaID);

                            Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>
                            {
                                { Payloads.MediaID, mediaID },
                                { Payloads.CardData, GetLockerInfo() }
                            };

                            GatWriterMediaDetected(dicPayload);
                        }

                        LastMediaID = mediaID;
                        SetGreenLightOnOrOff(true);
                        SetRedLightOnOrOff(false);
                    }
                }
                finally
                {
                    _timer.Enabled = true;
                }
            }
        }

        public override sealed bool SelfConfig()
        {
            if (SettingsManager.EmptyAll(Enums.ComputerSetting.TIM_Devices_GantnerFunWriter5200_FunWriter5200,
                new[] { Enums.ComputerDetailSetting.COM, Enums.ComputerDetailSetting.Baud }))
            {
                _comPort = 8;
                _baud = 9600;
                return false;
            }

            _comPort = SettingsManager.GetValue<byte>(Enums.ComputerSetting.TIM_Devices_GantnerFunWriter5200_FunWriter5200, Enums.ComputerDetailSetting.COM);
            _baud = SettingsManager.GetValue<uint>(Enums.ComputerSetting.TIM_Devices_GantnerFunWriter5200_FunWriter5200, Enums.ComputerDetailSetting.Baud);

            return true;
        }

        public override void Open()
        {
            SelfConfig();

            _gatWriter = new GATWriter5200(_comPort, _baud)
            {
                SubFID = SubFID,
                MasterKey = Key
            };

            if (!_gatWriter.OpenDevice())
                throw new DeviceException("Could not open the device");

            base.Open();
            DeviceIsOpen = true;
        }

        public override byte[] ReadSegmentBlock(int start, byte count)
        {
            return _gatWriter.ReadSegmentBlock(CardReadersHelper.GATSegments.User, count, true);
        }

        public override void SetGreenLightOnOrOff(bool on)
        {
            if (_gatWriter == null)
                return;

            if (on)
                _gatWriter.SetGreenLEDOn();
            else
                _gatWriter.SetGreenLEDOff();
        }

        public override void SetRedLightOnOrOff(bool on)
        {
            if (_gatWriter == null)
                return;

            if (on)
                _gatWriter.SetRedLEDOn();
            else
                _gatWriter.SetRedLEDOff();
        }

        protected override bool GATWriterAllowOpenDevice(List<DeviceDetail> deviceDetails, List<int> usedDeviceDetailIDs)
        {
            foreach (var deviceDetail in deviceDetails.Where(t => t.DeviceID == (int)DeviceID && t.RequiredSoftwareComponent.Contains(GetType().Name)))
            {
                if (usedDeviceDetailIDs.Contains(deviceDetail.DeviceDetailID))
                    return false;
            }

            return true;
        }

        protected override bool GATWriterUnblockLockerKey(bool unblock)
        {
            try
            {
                var lockerInfo = (LegicLockerInfo)_gatWriter.GetLockerInfo();

                lockerInfo = unblock ? SetLockerInfo(lockerInfo) : SetLockerInfoToDefault(lockerInfo);

                var timeoutCounter = 0;

                while (timeoutCounter < DEFAULT_COUNTER_TIME_OUT)
                {
                    if (_gatWriter.WriteLockerInfo(lockerInfo))
                        return true;

                    timeoutCounter++;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private LegicLockerInfo SetLockerInfo(LegicLockerInfo lockerInfo)
        {
            if (lockerInfo == null)
                return null;

            var expiryDate = SettingsManager.GetValue(Enums.SettingDetail.UnblockChipDuration);

            int.TryParse(expiryDate, out var expiryDuration);
            lockerInfo.ExpiryDate = DateTime.Now.AddHours(expiryDuration);

            lockerInfo.LockerOneInUse = false;
            lockerInfo.LockerOneNumber = 0;

            return lockerInfo;
        }

        private LegicLockerInfo SetLockerInfoToDefault(LegicLockerInfo lockerInfo)
        {
            if (lockerInfo == null)
                return null;

            lockerInfo.ExpiryDate = DateTime.Now.AddYears(-2);

            lockerInfo.LockerOneInUse = false;
            lockerInfo.LockerOneNumber = 0;

            return lockerInfo;
        }

        protected override string GetLockerInfo()
        {
            LockerInfo lockerInfo;
            var cardDataDto = new CardDataDto
            {
                LockerData = new List<LockerDto>()
            };
            try
            {
                lockerInfo = _gatWriter.GetLockerInfo();

                if (lockerInfo is LegicLockerInfo legicLockerInfo)
                {
                    cardDataDto.LockerData.Add(new LockerDto
                    {
                        LockerInUse = legicLockerInfo.LockerOneInUse,
                        LockerNumber = legicLockerInfo.LockerOneNumber
                    });
                    cardDataDto.LockerData.Add(new LockerDto
                    {
                        LockerInUse = legicLockerInfo.LockerTwoInUse,
                        LockerNumber = legicLockerInfo.LockerTwoNumber
                    });
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return lockerInfo == null ? string.Empty : JsonConvert.SerializeObject(cardDataDto);
        }

        #endregion
    }
}