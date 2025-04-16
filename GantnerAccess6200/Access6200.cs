using GAT.Comms.Devices.FunLine;
using GAT.Comms.Devices.FunLine.EventArguments;
using GAT.Comms.Devices.FunLine.Results;
using GAT.Comms.TCPIP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GAT.Comms.Devices.DataCarriers;
using Newtonsoft.Json;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.DTOs.CheckIn;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Enumerations;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.Turnstiles;

namespace TIM.Devices.GantnerAccess6200
{
    public class Access6200 : ITurnstile
    {
        #region Events

        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        public event EventHandler<TurnstileEventArgs> Passed;

        #endregion

        #region Members

        private string _name;
        private IPEndPoint _ipEndPoint;
        private bool _showTurnstileOpenButton;
        private GATAccess _gatAccess;

        // ReSharper disable once NotAccessedField.Local
        private Task _task;

        private CancellationToken _cancellationToken = CancellationToken.None;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEventSlim _mreMediaDetected;
        private int _triggerActive;
        private readonly AsyncOperation _asyncOperation;
        private DeviceIDs _deviceID;
        private bool _isOpenedByDevices;

        private int? _gatAccess6200MediaDetectTimeout;

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name) && DeviceIsOpen)
                    _name = $"Gantner Access 6200 ({_ipEndPoint.Address}:{_ipEndPoint.Port})";
                return _name ?? string.Empty;
            }
        }

        public Type[] SupportedContentTypes
        {
            get { return new[] { typeof(int) }; }
        }

        public bool MediaAvailable => throw new NotSupportedException();

        public bool IsWriteable => false;

        public bool SupportsCash => false;

        public bool SupportsContent => false;

        public bool SupportsTracking => false;

        public bool SupportsRequest => false;

        public bool SupportsTrigger => true;

        public bool SupportsToggle => false;

        public bool MediaSearchEnabled
        {
            get => true;
            set
            {
            }
        }

        public ConfigString ConfigString
        {
            get
            {
                ConfigString MyConfigString = new ConfigString('=', '|', ConfigStringDescriptions);
                MyConfigString["HOST"] = _ipEndPoint.Address.ToString();
                MyConfigString["PORT"] = _ipEndPoint.Port.ToString();
                MyConfigString["SHOWTURNSTILEOPENBUTTON"] = _showTurnstileOpenButton.ToString();
                return MyConfigString;
            }
            set
            {
                if (_ipEndPoint == null)
                {
                    _ipEndPoint = new IPEndPoint(
                        IPAddress.Parse(value["HOST"]),
                        ushort.Parse(value["PORT"])
                    );
                }
                else
                {
                    _ipEndPoint.Address = IPAddress.Parse(value["HOST"]);
                    _ipEndPoint.Port = ushort.Parse(value["PORT"]);
                }

                var showTurnstileOpenButtonValue = value["SHOWTURNSTILEOPENBUTTON"];
                if (string.IsNullOrWhiteSpace(showTurnstileOpenButtonValue))
                    return;

                if (bool.TryParse(value["SHOWTURNSTILEOPENBUTTON"], out bool result))
                    _showTurnstileOpenButton = result;
                else if (showTurnstileOpenButtonValue == "1")
                    _showTurnstileOpenButton = true;
                else
                    _showTurnstileOpenButton = false;
            }
        }

        public ConfigStringDescription[] ConfigStringDescriptions
        {
            get
            {
                return new[]
                    {
                        new ConfigStringDescription("HOST", typeof(string), true, true),
                        new ConfigStringDescription("PORT", typeof(ushort), true, true),
                        new ConfigStringDescription("SHOWTURNSTILEOPENBUTTON", typeof(bool), false, true)
                    };
            }
        }

        public DeviceCapabilities DeviceCapabilities => DeviceCapabilities.Identification;

        public bool SupportsPassed => true;

        public bool DeviceIsOpen { get; set; }

        /// <summary>
        /// The DeviceID will be set in the CardIdentificationRequested
        /// </summary>
        public DeviceIDs DeviceID => _deviceID;

        public string Message { get; set; }

        public bool AlwaysTrigger { get; set; }

        public bool IsConnected => _gatAccess.IsConnected;

        public bool ShowTurnstileOpenButton => _showTurnstileOpenButton;

        public int DeviceFrameworkDeviceID { get; }

        public bool SupportsUnblockLockerKey => false;

        private int GatAccess6200MediaDetectTimeout
        {
            get
            {
                if (!_gatAccess6200MediaDetectTimeout.HasValue)
                {
                    _gatAccess6200MediaDetectTimeout = SettingsManager.GatAccess6200MediaDetectTimeout;
                }

                return _gatAccess6200MediaDetectTimeout.Value;
            }
        }

        #endregion

        #region Constructors

        // ReSharper disable once UnusedMember.Global
        // Need for InstanceBuilder...
        public Access6200()
        {
            _asyncOperation = AsyncOperationManager.CreateOperation(null);
        }

        // ReSharper disable once UnusedMember.Global
        //Is needed for turnstiles
        public Access6200(int deviceFrameworkDeviceID)
        {
            _asyncOperation = AsyncOperationManager.CreateOperation(null);
            AlwaysTrigger = false;

            //At this point it is not clear, which DeviceID is correct
            _deviceID = DeviceIDs.GantnerFunWriterLegic;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        public Access6200(int deviceFrameworkDeviceID, bool alwaysTrigger = false)
        {
            _asyncOperation = AsyncOperationManager.CreateOperation(null);
            AlwaysTrigger = alwaysTrigger;

            //At this point it is not clear, which DeviceID is correct
            _deviceID = DeviceIDs.GantnerFunWriterLegic;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion

        #region Methods

        public bool SelfConfig()
        {
            if (SettingsManager.ExistsAll(Enums.ComputerSetting.TIM_Devices_GantnerAccess6200_Access6200,
                new[] { Enums.ComputerDetailSetting.Host, Enums.ComputerDetailSetting.Port }))
            {
                var host = SettingsManager.GetValue<string>(Enums.ComputerSetting.TIM_Devices_GantnerAccess6200_Access6200, Enums.ComputerDetailSetting.Host);
                var port = SettingsManager.GetValue<ushort>(Enums.ComputerSetting.TIM_Devices_GantnerAccess6200_Access6200, Enums.ComputerDetailSetting.Port);

                if (_ipEndPoint == null)
                {
                    _ipEndPoint = new IPEndPoint(
                        IPAddress.Parse(host),
                        port
                    );
                }

                return true;
            }
            return false;
        }

        public void Open()
        {
            if (!SelfConfig() && _ipEndPoint == null)
            {
                DeviceIsOpen = false;
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _task = Task.Factory.StartNew(() =>
            {
                TCP_Client MyCommChannel = new TCP_Client
                {
                    Port = _ipEndPoint.Port,
                    RemoteLocation = _ipEndPoint.Address.ToString()
                };

                _gatAccess = new GATAccess(MyCommChannel, true);
                _gatAccess.CardIdentificationRequested +=
                    MyGATAccess_CardIdentificationRequested;
                _gatAccess.ActionStarted += MyGATAccess_ActionStarted;
                _gatAccess.AutoReconnect = true;
                _gatAccess.AutoReconnectInactivityInterval = 5000; // time before reconnect when ip connection was interrupted
                _gatAccess.AutoReconnectInterval = 5000; // time before reconnect when ip connection was interrupted

                int i = 0;
                while (true)
                {
                    if (_gatAccess.Start() || i++ > 5)
                        break;

                    Thread.Sleep(1000);
                }

                // Work loop
                while (true)
                {
                    try
                    {
                        Thread.Sleep(250);

                        if (_cancellationToken.IsCancellationRequested)
                        {
                            // Clean up
                            if (_gatAccess != null)
                            {
                                _gatAccess.Stop();
                                _gatAccess = null;
                            }
                            _task = null;

                            _cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, _cancellationToken);
            DeviceIsOpen = true;
        }

        private void MyGATAccess_ActionStarted(object sender, ActionStartedGATAccessEventArguments eventArguments)
        {
            if (eventArguments.IsUsed) // in gantner settings: Functions = Entrance
            {
                // todo: check that CardNumber is correct (cnf sunbeds)
                Dictionary<Enums.Payloads, object> dicPayload = new Dictionary<Enums.Payloads, object>();
                dicPayload.Add(Enums.Payloads.MediaID, eventArguments.CardNumber.Trim());

                TurnstileEventArgs evtArgs = new TurnstileEventArgs(this, dicPayload);

                OnPassed(evtArgs);
            }
        }

        private CardIdentificationGATAccessResult MyGATAccess_CardIdentificationRequested(object sender, CardIdentificationGATAccessEventArguments eventArguments)
        {
            switch (eventArguments.ReaderType)
            {
                case FunLineHelper.TerminalReaderType.Legic:
                    _deviceID = DeviceIDs.GantnerFunWriterLegic;
                    break;

                case FunLineHelper.TerminalReaderType.Mifare:
                    _deviceID = DeviceIDs.GantnerFunWriterMifare;
                    break;

                default:
                    throw new Exception("Unknown readertype");
            }

            _mreMediaDetected = new ManualResetEventSlim();

            var cardDataDto = new CardDataDto
            {
                LockerData = new List<LockerDto>()
            };

            if (eventArguments.CardData != null)
            {
                foreach (var cardData in eventArguments.CardData)
                {
                    if (cardData is MifareLockerInfo mifareLockerInfo)
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
                    else if (cardData is LegicLockerInfo legicLockerInfo)
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
            }

            var serializedCardData = JsonConvert.SerializeObject(cardDataDto);

            Dictionary<Payloads, object> dicPayloads = new Dictionary<Payloads, object>
            {
                { Payloads.MediaID, eventArguments.CardNumber.Trim() },
                { Payloads.CardData, serializedCardData }
            };
            _asyncOperation.Post(delegate
            {
                OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayloads, DeviceFrameworkDeviceID));
            }, null);

            if (AlwaysTrigger)
            {
                return new CardIdentificationGATAccessResult
                {
                    IsAuthorized = true,
                    Texts = "OK;"
                };
            }

            // method Trigger() needs to be called, if not called within 3 sec then not autorized
            // otherwise autorized if tsActive > 0 (value of tsActive does not have consequences)
            // this is neede because connection to terminal is synchrone and connection to DF is asynchrone
            if (_mreMediaDetected.Wait(GatAccess6200MediaDetectTimeout)) // max 3000, otherwise timeout on device may occur
            {
                if (_triggerActive > 0)
                {
                    // Access allowed
                    return new CardIdentificationGATAccessResult
                    {
                        IsAuthorized = true,
                        Texts = Message
                    };
                }

                // Access denied
                return new CardIdentificationGATAccessResult
                {
                    IsAuthorized = false,
                    Texts = Message
                };
            }

            // Failure
            return new CardIdentificationGATAccessResult
            {
                IsAuthorized = false
            };
        }

        public void Close()
        {
            if (_cancellationToken != CancellationToken.None)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public void Request()
        {
            throw new NotSupportedException();
        }

        public string GetMediaID()
        {
            throw new NotSupportedException();
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
            // tsActive = 301 ms, then the button "Open" was pressed without a card event
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (tsActive.TotalMilliseconds == 301.0)
            {
                if (_gatAccess != null)
                    _gatAccess.ExecuteDirectAccessControl();

                return;
            }

            // default behavior
            _triggerActive = (int)tsActive.TotalMilliseconds;
            if (_mreMediaDetected != null)
                _mreMediaDetected.Set();
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
            var configStrings = TurnstileHelper.GetConfigStrings().Select(p => p.Value).ToList();
            string host = SettingsManager.GetValue<string>(Enums.ComputerSetting.TIM_Devices_GantnerAccess6200_Access6200, Enums.ComputerDetailSetting.Host, false);

            foreach (var configString in configStrings)
            {
                if (!configString.IsEmpty && configString.EntryExists("HOST") && configString["HOST"] == host)
                {
                    _isOpenedByDevices = false;
                    return false;
                }
            }

            _isOpenedByDevices = true;
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

        protected virtual void OnPassed(TurnstileEventArgs e)
        {
            EventHandler<TurnstileEventArgs> evtHandler = Passed;
            if (evtHandler != null)
                evtHandler(this, e);
        }

        #endregion
    }
}