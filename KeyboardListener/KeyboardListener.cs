using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TIM.Common.CoreStandard;
using TIM.Common.Data.Entities;
using TIM.Common.Data.Factories;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.KeyboardListener
{
    [StructLayout(LayoutKind.Sequential)]
    public class KBDLLHOOKSTRUCT
    {
        public UInt32 vkCode;
        public UInt32 scanCode;
        public UInt32 flags;
        public UInt32 time;
        public IntPtr dwExtraInfo;

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", vkCode, scanCode, flags, time);
        }
    }

    public enum HookType : int
    {
        WH_KEYBOARD_LL = 13,
    }

    public class KeyboardListener : IDevice
    {
        private delegate int HookProc(int code, IntPtr wParam, KBDLLHOOKSTRUCT lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(HookType hook, HookProc callback, int hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);

        #region Events

        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        #endregion

        #region Fields

        private RingBuffer _ringBuffer;
        private readonly HookProc _hookProc;
        private IntPtr _pHook;
        private DateTime _lastKey;
        private bool _mediaSearchEnabled;
        private string _lastID = null;

        private Regex _regex;
        private bool _tooFastForHumanInput = false;
        private int _maximumInputTime = 25;

        #endregion Fields

        #region Properties

        public string Name => "KeyboardListener";

        public Type[] SupportedContentTypes => new Type[] { };

        public bool MediaAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsWriteable => false;

        public bool SupportsCash => false;

        public bool SupportsContent => false;

        public bool SupportsTracking => false;

        public bool SupportsRequest => false;

        public bool MediaSearchEnabled
        {
            get { return _mediaSearchEnabled; }
            set
            {
                if (value && DeviceIsOpen)
                {
                    _pHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL,
                        _hookProc,
                        GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName).ToInt32(),
                        0);
                    if (_pHook == IntPtr.Zero)
                        throw new DeviceException("Can't create windows hook for keyboard");
                }
                else
                {
                    UnhookWindowsHookEx(_pHook);
                }
                _mediaSearchEnabled = value && DeviceIsOpen;
            }
        }

        public bool SupportsTrigger => false;

        public bool SupportsToggle => false;

        public ConfigString ConfigString
        {
            get { return new ConfigString('=', '|', ConfigStringDescriptions); }
            set { }
        }

        public ConfigStringDescription[] ConfigStringDescriptions => new ConfigStringDescription[0];

        public DeviceCapabilities DeviceCapabilities => DeviceCapabilities.Identification | DeviceCapabilities.Vouchers;

        public bool DeviceIsOpen { get; set; }

        public Framework.Common.Enumerations.DeviceIDs DeviceID => Framework.Common.Enumerations.DeviceIDs.KeyboardListener;

        public string Message { get; set; }

        public bool IsConnected => DeviceIsOpen;

        public int DeviceFrameworkDeviceID { get; }

        public bool SupportsUnblockLockerKey => false;

        #endregion Properties

        #region Constructor

        // ReSharper disable once UnusedMember.Global
        public KeyboardListener()
        {
        }

        public KeyboardListener(int deviceFrameworkDeviceID)
        {
            _hookProc = KeyStroke;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;
        }

        #endregion Constructor

        #region Methods

        public bool SelfConfig()
        {
            return true;
        }

        public void Open()
        {
            //Load and check Regex from config
            var regexFromConfig = DbControllers.SettingDetails.GetValueFromCache(false, Enums.SettingDetail.Regex)?.Trim();
            _regex = IsValidRegex(regexFromConfig) ? new Regex(regexFromConfig) : null;

            //Init timer if no regex is found
            if (_regex == null)
            {
                DeviceIsOpen = false;
                Console.WriteLine("Setting for KeyboardListener not found");
                return;
            }

            _ringBuffer = new RingBuffer();
            _lastKey = DateTime.MinValue;

            DeviceIsOpen = true;
        }

        public void Close()
        {
            DeviceIsOpen = false;
        }

        public void Request()
        {
            throw new NotSupportedException();
        }

        public string GetMediaID()
        {
            return _lastID;
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

        private int KeyStroke(int code, IntPtr wParam, KBDLLHOOKSTRUCT lParam)
        {
            //Check if keystroke is keydown
            if (lParam.flags != 0) return 0;

            // Check input speed
            _tooFastForHumanInput = DateTime.Now.Subtract(_lastKey).TotalMilliseconds <= _maximumInputTime;
            _lastKey = DateTime.Now;

            // Delete all buffered input if more than _maximumInputTime passed since last keystroke
            if (!_tooFastForHumanInput)
                _ringBuffer.Clear();

            // Allow keys 0-9 and A-Z, else clear buffer
            if ((lParam.vkCode > 47 && lParam.vkCode < 91)) //TODO DK find out what are correct symbols
            {
                //Add keystroke to buffer
                _ringBuffer.Append((char)lParam.vkCode);

                //Check if input fits the regex.
                CheckMediaDetected();
            }
            else
            {
                //Clear buffer if you can check the regex. Save everything if you wait for the whole input.
                /*if (_regexFlag)
                    _ringBuffer.Clear();*/
                return 0;
            }

            return !_tooFastForHumanInput ? 0 : -1;
        }

        private void CheckMediaDetected()
        {
            //Check if input matches regex. If no regex is defined checks against amount.
            if (_ringBuffer.IsMatch(_regex))
            {
                _lastID = _ringBuffer.GetIDWhichFitsRegex(_regex);
                //return if last Id is invalid
                if (_lastID == null)
                    return;

                var dicPayload = new Dictionary<Payloads, object>
                    {
                        {Payloads.MediaID, _lastID}
                    };

                OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload, DeviceFrameworkDeviceID));
                _ringBuffer.Clear();
            }
            else
                _lastID = null;
        }

        /// <summary>
        /// Checks if the given regex pattern is valid
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        protected virtual void OnMediaDetected(DeviceMediaDetectedEventArgs2 e)
        {
            var evtHandler = MediaDetected;
            evtHandler?.Invoke(this, e);
        }

        public bool AllowOpenDevice(List<DeviceDetail> deviceDetails, List<int> usedDeviceDetailIDs)
        {
            return true;
        }

        public bool BlockOrUnblockLockerKey(bool unblock)
        {
            return true;
        }

        #endregion Methods
    }
}