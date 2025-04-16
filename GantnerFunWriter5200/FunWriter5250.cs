using GAT.Comms.Devices;
using GAT.Comms.Devices.CardReader;
using GAT.Comms.Devices.DataCarriers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.DTOs.CheckIn;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.GantnerFunWriter5200
{
    public class FunWriter5250 : FunWriter
    {
        #region Fields

        private GATWriter5250 _gatWriter;
        private const int DEFAULT_COUNTER_TIME_OUT = 10;

        #endregion

        #region Properties

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(StrName))
                    StrName = "GAT Writer 5250";
                return StrName;
            }
        }

        public override ConfigString ConfigString
        {
            get => new ConfigString('=', '|', ConfigStringDescriptions);
            set { }
        }

        public override ConfigStringDescription[] ConfigStringDescriptions => new ConfigStringDescription[0];

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

        protected override bool AllowUseTimer => false;

        protected override bool GatWriterMediaSearchEnabled
        {
            get => _gatWriter?.AutoScanEnabled ?? false;
            set
            {
                if (_gatWriter != null)
                    _gatWriter.AutoScanEnabled = value;
            }
        }

        #endregion

        #region Constructor

        // ReSharper disable once UnusedMember.Global
        public FunWriter5250()
        {
        }

        public FunWriter5250(int deviceFrameworkDeviceID)
        {
            GATWriterDeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion

        #region Methods

        public override bool SelfConfig()
        {
            return true;
        }

        public override void Open()
        {
            _gatWriter = new GATWriter5250
            {
                SubFID = SubFID,
                MasterKey = Key
            };

            _gatWriter.CardDetected += OnCardDetected;
            _gatWriter.CardLost += OnCardLost;

            if (!_gatWriter.OpenDevice())
                throw new DeviceException("Could not open the device");

            _gatWriter.AutoScanEnabled = true;
            _gatWriter.AutoScanInterval = 300;
            base.Open();

            DeviceIsOpen = true;
        }

        private void OnCardDetected(object sender, CardReadersHelper.CardDetectedEventArgs eventargs)
        {
            try
            {
                //At first set LastMediaID to be sure it is always set correct
                LastMediaID = eventargs.CardUniqueNumber;

                if (string.IsNullOrWhiteSpace(LastMediaID))
                    return;

                //Dont throw event if LastMediaID is null or empty
                _gatWriter.CardDetected -= OnCardDetected;
                SetRedLightOnOrOff(true);
                SetGreenLightOnOrOff(false);

                Dictionary<Payloads, object> payload = new Dictionary<Payloads, object>
                {
                    { Payloads.MediaID, LastMediaID },
                    { Payloads.CardData, GetLockerInfo() }
                };

                GatWriterMediaDetected(payload);

                SetRedLightOnOrOff(false);
                SetGreenLightOnOrOff(true);
            }
            catch
            {
                SetRedLightOnOrOff(false);
                SetGreenLightOnOrOff(true);
            }
        }

        private void OnCardLost(object sender)
        {
            try
            {
                //Dont throw event if LastMediaID is null or empty
                if (string.IsNullOrWhiteSpace(LastMediaID))
                    return;

                SetRedLightOnOrOff(true);
                SetGreenLightOnOrOff(false);

                Dictionary<Payloads, object> payload = new Dictionary<Payloads, object>
                {
                    { Payloads.MediaID, LastMediaID }
                };

                GatWriterMediaRemoved(payload);

                SetRedLightOnOrOff(false);
                SetGreenLightOnOrOff(true);
                _gatWriter.CardDetected += OnCardDetected;
            }
            catch
            {
                SetRedLightOnOrOff(false);
                SetGreenLightOnOrOff(true);
                _gatWriter.CardDetected += OnCardDetected;
            }
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