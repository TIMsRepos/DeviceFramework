using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIM.Devices.Framework.Common;
using System.Runtime.InteropServices;
using TIM.Devices.Framework.Common.Settings;

namespace GriauleFingerprintReader
{
    public class FingerprintReader : IDevice
    {
        private const string DLL_NAME = "GrFinger.dll";

        #region P/Invoke
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrInitialize();
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrFinalize();
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrCapInitialize(StatusEventDelegate delStatus);
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrCapFinalize();
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrCapStartCapture(string strSensorID, FingerEventDelegate delFinger, ImageEventDelegate delImage);
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrCapStopCapture(string strSensorID);
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrExtract(IntPtr pRawImage, int intWidth, int intHeight, int intRes, byte[] bytTpt, ref int intTptSize, int intContextID);
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrCreateContext(out int intContextID);
        [DllImport(DLL_NAME)]
        private static extern SuccessCodes GrDestroyContext(int intContextID);
        #endregion

        #region Delegates
        private delegate void StatusEventDelegate(string strSensorID, EventCodes MyEventCode);
        private delegate void FingerEventDelegate(string strSensorID, EventCodes MyEventCode);
        private delegate void ImageEventDelegate(string strSensorID, uint intWidth, uint intHeight, IntPtr pRawImage, int intRes);
        #endregion

        #region Events
        public event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;
        public event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;
        #endregion

        #region Fields
        private string strName;
        private string strLastMediaID;
        #endregion

        #region Properties
        public string Name
        {
            get { return string.Format("Griaule Fingerprint Reader ({0})", strName); }
        }
        public Type[] SupportedContentTypes
        {
            get { throw new NotImplementedException(); }
        }
        public bool MediaAvailable
        {
            get { throw new NotImplementedException(); }
        }
        public bool IsWriteable
        {
            get { throw new NotImplementedException(); }
        }
        public bool SupportsCash
        {
            get { throw new NotImplementedException(); }
        }
        public bool SupportsContent
        {
            get { throw new NotImplementedException(); }
        }
        public bool SupportsTracking
        {
            get { throw new NotImplementedException(); }
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
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
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
        public DeviceCapabilities DeviceCapabilities
        {
            get { return DeviceCapabilities.Identification; }
        }
        #endregion

        #region Constructors
        public FingerprintReader()
        {
        }
        #endregion

        #region Methods
        private void StatusEventCallback(string strSensorID, EventCodes MyEventCode)
        {
            Console.WriteLine(string.Format("STATUS: {0} => {1}", strSensorID, MyEventCode));
            switch (MyEventCode)
            {
                case EventCodes.GR_PLUG:
                    strName = strSensorID;
                    if (GrCapStartCapture(strSensorID, new FingerEventDelegate(FingerEventCallback), new ImageEventDelegate(ImageEventCallback)) != SuccessCodes.GR_OK)
                        throw new DeviceException("Unable to start capturing");
                    break;
                case EventCodes.GR_UNPLUG:
                    if (GrCapStopCapture(strSensorID) != SuccessCodes.GR_OK)
                        throw new DeviceException("Unable to stop capturing");
                    break;
                case EventCodes.GR_FINGER_DOWN:
                    break;
                case EventCodes.GR_FINGER_UP:
                    break;
            }
        }
        private void FingerEventCallback(string strSensorID, EventCodes MyEventCode)
        {
            Console.WriteLine(string.Format("FINGER: {0} => {1}", strSensorID, MyEventCode));
            switch (MyEventCode)
            {
                case EventCodes.GR_PLUG:
                    break;
                case EventCodes.GR_UNPLUG:
                    break;
                case EventCodes.GR_FINGER_DOWN:
                    break;
                case EventCodes.GR_FINGER_UP:
                    break;
            }
        }
        private void ImageEventCallback(string strSensorID, uint intWidth, uint intHeight, IntPtr pRawImage, int intRes)
        {
            Console.WriteLine(string.Format("IMAGE: {0}", strSensorID));
            int intContextID;
            if (GrCreateContext(out intContextID) != SuccessCodes.GR_OK)
                throw new DeviceException("Unable to create context");

            int intTptSize = (int)ImageValues.GR_MAX_SIZE_TEMPLATE;
            byte[] bytTpt = new byte[intTptSize];
            SuccessCodes scQuality = GrExtract(pRawImage, (int)intWidth, (int)intHeight, intRes, bytTpt, ref intTptSize, intContextID);
            switch (scQuality)
            {
                case SuccessCodes.GR_BAD_QUALITY:
                case SuccessCodes.GR_MEDIUM_QUALITY:
                case SuccessCodes.GR_HIGH_QUALITY:
                    {
                        strLastMediaID = Convert.ToBase64String(bytTpt.Take(intTptSize).ToArray());

                        Dictionary<Payloads, object> dicPayload = new Dictionary<Payloads, object>();
                        dicPayload.Add(Payloads.MediaID, strLastMediaID);

                        OnMediaDetected(new DeviceMediaDetectedEventArgs2(this, dicPayload));
                        break;
                    }
                default:
                    throw new DeviceException(string.Format("Extraction failed '{0}'", scQuality));
            }

            if (GrDestroyContext(intContextID) != SuccessCodes.GR_OK)
                throw new DeviceException("Unable to destroy context");
        }

        public bool SelfConfig()
        {
            return true;
        }
        public void Open()
        {
            if (GrInitialize() != SuccessCodes.GR_OK)
                throw new DeviceException("Could not init device");
            if (GrCapInitialize(new StatusEventDelegate(StatusEventCallback)) != SuccessCodes.GR_OK)
                throw new DeviceException("Could not init capturing");
        }
        public void Close()
        {
            if (GrCapFinalize() != SuccessCodes.GR_OK)
                throw new DeviceException("Unable to finalize capturing");
            if (GrFinalize() != SuccessCodes.GR_OK)
                throw new DeviceException("Unable to finalize device");
        }
        public void Request()
        {
            throw new NotSupportedException();
        }
        public string GetMediaID()
        {
            return strLastMediaID;
        }
        public object GetMediaContent()
        {
            throw new NotImplementedException();
        }
        public float GetMediaCash()
        {
            throw new NotImplementedException();
        }
        public void SetMediaContent(object objContent)
        {
            throw new NotImplementedException();
        }
        public void SetMediaCash(float fltCash)
        {
            throw new NotImplementedException();
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
            EventHandler<DeviceMediaDetectedEventArgs2> evtHandler = MediaDetected;
            if (evtHandler != null)
                evtHandler(this, e);
        }
        #endregion
    }
}
