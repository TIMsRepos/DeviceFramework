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
using TIM.Devices.Framework.Common.Extensions;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.GantnerFunWriter5200
{
    public class FunWriter6000 : FunWriter
    {
        #region Fields

        private GATWriter6000F _gatWriter;
        private const int DEFAULT_COUNTER_TIME_OUT = 10;

        #endregion

        #region Properties

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(StrName))
                    StrName = "GAT Writer 6000";
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
        protected override Enums.SettingDetail SettingDetailIdKey => Enums.SettingDetail.GATWriterMifareMasterKey;
        protected override Enums.SettingDetail SettingDetailIdFID => Enums.SettingDetail.GATWriterFID;
        protected override Enums.SettingDetail SettingDetailIdSubFID => Enums.SettingDetail.GATWriterSubFID;

        protected override Framework.Common.Enumerations.DeviceIDs FunWriterDeviceID => Framework.Common.Enumerations.DeviceIDs.GantnerFunWriterMifare;

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
        public FunWriter6000()
        {
        }

        public FunWriter6000(int deviceFrameworkDeviceID)
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
            try
            {
                _gatWriter = new GATWriter6000F();
            }
            catch (Exception ex)
            {
                ex.LogWarning();
                throw new DeviceException("Could not open the device");
            }

            _gatWriter.FID = FID;
            _gatWriter.SubFID = SubFID;
            _gatWriter.MasterKey = Key;

            _gatWriter.CardDetected += OnCardDetected;
            _gatWriter.CardLost += OnCardLost;

            if (!_gatWriter.OpenDevice())
            {
                DeviceIsOpen = false;
                throw new DeviceException("Could not open the device");
            }

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

                //Dont throw event if LastMediaID is null or empty
                if (string.IsNullOrWhiteSpace(LastMediaID))
                    return;

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

        public override void Close()
        {
            base.Close();

            if (_gatWriter != null && !_gatWriter.CloseDevice())
                throw new DeviceException("Could not close the device");
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

        public override byte[] ReadSegmentBlock(int start, byte count)
        {
            return _gatWriter.ReadSegmentBlock(start, count);
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
                var lockerInfo = (MifareLockerInfo)_gatWriter.GetLockerInfo();

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

        private MifareLockerInfo SetLockerInfo(MifareLockerInfo lockerInfo)
        {
            if (lockerInfo == null)
                return null;

            lockerInfo.Config = 0x00;
            var expiryDate = SettingsManager.GetValue(Enums.SettingDetail.UnblockChipDuration);

            int.TryParse(expiryDate, out var expiryDuration);
            lockerInfo.ExpiryDate = DateTime.Now.AddHours(expiryDuration);

            lockerInfo.LockerOneInUse = false;
            lockerInfo.LockerOneNumber = 0;

            return lockerInfo;
        }

        private MifareLockerInfo SetLockerInfoToDefault(MifareLockerInfo lockerInfo)
        {
            if (lockerInfo == null)
                return null;

            lockerInfo.ExpiryDate = DateTime.Now.AddYears(-2);

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

                if (lockerInfo is MifareLockerInfo mifareLockerInfo)
                {
                    cardDataDto.LockerData.Add(new LockerDto
                    {
                        LockerInUse = mifareLockerInfo.LockerOneInUse,
                        LockerNumber = mifareLockerInfo.LockerOneNumber
                    });
                    cardDataDto.LockerData.Add(new LockerDto
                    {
                        LockerInUse = mifareLockerInfo.LockerTwoInUse,
                        LockerNumber = mifareLockerInfo.LockerTwoNumber
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