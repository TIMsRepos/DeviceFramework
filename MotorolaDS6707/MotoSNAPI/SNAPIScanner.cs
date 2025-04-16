using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TIM.Devices.MotorolaDS6707.MotoSNAPI
{
    /// <summary>
    /// Decode data recieved from the scanner
    /// </summary>
    public class DecodeData : EventArgs
    {
        /// <summary>
        /// Decode barcode identifier values returned by SNAPI library.
        /// </summary>
        public enum CodeTypes
        {
            ///<summary>An unknown, with respect to this enumeration, code type</summary>
            Unknown = 0,

            ///<summary>Code 39 symbology</summary>
            Code39 = 1,

            ///<summary>Codabar symbology</summary>
            Codabar = 2,

            ///<summary>Code 128 symbology</summary>
            Code128 = 3,

            ///<summary>Dinscrete 2 of 5 symbology</summary>
            Discrete2of5 = 4,

            ///<summary>IATA symbology</summary>
            Iata = 5,

            ///<summary>Interleaved 2 of 5 symbology</summary>
            Interleaved2of5 = 6,

            ///<summary>Code 93 symbology</summary>
            Code93 = 7,

            ///<summary>UPC-A symbology</summary>
            UpcA = 8,

            ///<summary>UPC-E0 symbology</summary>
            UpcE0 = 9,

            ///<summary>EAN-8 symbology</summary>
            Ean8 = 10,

            ///<summary>EAN-13 symbology</summary>
            Ean13 = 11,

            ///<summary>Code 11symbology</summary>
            Code11 = 12,

            ///<summary>Code 49symbology</summary>
            Code49 = 13,

            ///<summary>MSI Plessey symbology</summary>
            Msi = 14,

            ///<summary>EAN-128 symbology</summary>
            Ean128 = 15,

            ///<summary>UPC-E1 symbology</summary>
            UpcE1 = 16,

            ///<summary>PDF-417 symbology</summary>
            Pdf417 = 17,

            ///<summary>Code 16K symbology</summary>
            Code16K = 18,

            ///<summary>Code 39 symbology, with full-ASCII expansion applied</summary>
            Code39FullAscii = 19,

            ///<summary>UPC-D symbology</summary>
            UpcD = 20,

            ///<summary>symbology</summary>
            Code39Trioptic = 21,

            ///<summary>symbology</summary>
            Bookland = 22,

            ///<summary>symbology</summary>
            CouponCode = 23,

            ///<summary>symbology</summary>
            Nw7 = 24,

            ///<summary>symbology</summary>
            Isbt128 = 25,

            ///<summary>symbology</summary>
            Micro_Pdf = 26,

            ///<summary>symbology</summary>
            DataMatrix = 27,

            ///<summary>symbology</summary>
            QrCode = 28,

            ///<summary>symbology</summary>
            MicroPdfCca = 29,

            ///<summary>symbology</summary>
            PostNetUS = 30,

            ///<summary>symbology</summary>
            PlanetCode = 31,

            ///<summary>symbology</summary>
            Code32 = 32,

            ///<summary>symbology</summary>
            Isbt128Con = 33,

            ///<summary>symbology</summary>
            JapanPostal = 34,

            ///<summary>symbology</summary>
            AustralianPostal = 35,

            ///<summary>symbology</summary>
            DutchPostal = 36,

            ///<summary>symbology</summary>
            MaxiCode = 37,

            ///<summary>symbology</summary>
            CanadianPostal = 38,

            ///<summary>symbology</summary>
            UkPostal = 39,

            ///<summary>symbology</summary>
            MacroPdf = 40,

            ///<summary>symbology</summary>
            Aztec = 45,

            ///<summary>symbology</summary>
            Rss14 = 48,

            ///<summary>symbology</summary>
            RssLimited = 49,

            ///<summary>symbology</summary>
            RssExpanded = 50,

            ///<summary>symbology</summary>
            Scanlet = 55,

            ///<summary>symbology</summary>
            UpcAPlus2 = 72,

            ///<summary>symbology</summary>
            UpcE0Plus2 = 73,

            ///<summary>symbology</summary>
            Ean8Plus2 = 74,

            ///<summary>symbology</summary>
            Ean13Plus2 = 75,

            ///<summary>symbology</summary>
            UpcE1Plus2 = 80,

            ///<summary>symbology</summary>
            CcaEAN128 = 81,

            ///<summary>symbology</summary>
            CcaEAN13 = 82,

            ///<summary>symbology</summary>
            CcaEAN8 = 83,

            ///<summary>symbology</summary>
            CcaRssExpanded = 84,

            ///<summary>symbology</summary>
            CcaRssLimited = 85,

            ///<summary>symbology</summary>
            CcaRss14 = 86,

            ///<summary>symbology</summary>
            CcaUpcA = 87,

            ///<summary>symbology</summary>
            CcaUpcE = 88,

            ///<summary>symbology</summary>
            CccEAN128 = 89,

            ///<summary>symbology</summary>
            Tlc39 = 90,

            ///<summary>symbology</summary>
            CcbEan128 = 97,

            ///<summary>symbology</summary>
            CcbEan13 = 98,

            ///<summary>symbology</summary>
            CcbEan8 = 99,

            ///<summary>symbology</summary>
            CcbRssExpanded = 100,

            ///<summary>symbology</summary>
            CcbRssLimited = 101,

            ///<summary>symbology</summary>
            CcbRss14 = 102,

            ///<summary>symbology</summary>
            CcbUpcA = 103,

            ///<summary>symbology</summary>
            CcbUpcE = 104,

            ///<summary>symbology</summary>
            SignatureCapture = 105,

            ///<summary>symbology</summary>
            Matrix2of5 = 113,

            ///<summary>symbology</summary>
            Chinese2of5 = 114,

            ///<summary>symbology</summary>
            UpcAPlus5 = 136,

            ///<summary>symbology</summary>
            UpcE0Plus5 = 137,

            ///<summary>symbology</summary>
            Ean8Plus5 = 138,

            ///<summary>symbology</summary>
            Ean13Plus5 = 139,

            ///<summary>symbology</summary>
            UpcE1Plus5 = 144,

            ///<summary>symbology</summary>
            MacroMicroPDF = 154,

            ///<summary>The no-decode symbol</summary>
            NoDecode = 255,
        }

        /// <summary>
        /// Construct a new decode data object
        /// </summary>
        /// <param name="codeType">The barcode symbology type the data represents</param>
        /// <param name="data">The raw decode data</param>
        public DecodeData(CodeTypes codeType, byte[] data)
        {
            this.codeType = codeType;
            this.data = data;
        }

        /// <summary>
        /// Return the barcode symbology type
        /// </summary>
        public CodeTypes CodeType
        {
            get { return codeType; }
        }

        /// <summary>
        /// Return the decoded data as an ASCII string
        /// </summary>
        public String Text
        {
            get { return System.Text.Encoding.ASCII.GetString(data); }
        }

        private CodeTypes codeType;
        private byte[] data;
    }

    /// <summary>
    /// Image data recieved from the scanner
    /// </summary>
    public class ImageData : System.EventArgs
    {
        protected ImageData(byte[] rawData, int imageSize)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(rawData);
            image = System.Drawing.Image.FromStream(ms);
            this.imageSize = imageSize;
        }

        private System.Drawing.Image image;
        private int imageSize;
    }

    /// <summary>
    /// Decode data recieved from the scanner
    /// </summary>
    internal class ImageDataFactory : ImageData
    {
        public ImageDataFactory(byte[] rawData, int imageSize)
            : base(rawData, imageSize)
        {
        }

        static public ImageData CreateImageData(byte[] rawData, int imageSize)
        {
            return new ImageDataFactory(rawData, imageSize);
        }
    }

    /// <summary>
    /// Version data recieved from the scanner
    /// </summary>
    public class VersionData : EventArgs
    {
        public VersionData(byte[] data, int length)
        {
            if (length > 1)
                this.versionString = Encoding.ASCII.GetString(data, 0, length - 1);

            if ((versionString.IndexOf(" ") > 0) && (versionString.Length - versionString.IndexOf(" ") - 1 > 0))
            {
                this.softwareRevision = versionString.Substring(0, versionString.IndexOf(" "));
                this.versionString = versionString.Substring(versionString.IndexOf(" ") + 1, versionString.Length - versionString.IndexOf(" ") - 1);
            }
            if ((versionString.IndexOf(" ") > 0) && (versionString.Length - versionString.IndexOf(" ") - 1 > 0))
            {
                this.boardType = versionString.Substring(0, versionString.IndexOf(" "));
                this.versionString = versionString.Substring(versionString.IndexOf(" ") + 1, versionString.Length - versionString.IndexOf(" ") - 1);
            }
            if ((versionString.IndexOf(" ") > 0) && (versionString.Length - versionString.IndexOf(" ") - 1 > 0))
            {
                this.engineCode = versionString.Substring(0, versionString.IndexOf(" "));
                this.classCode = versionString.Substring(versionString.IndexOf(" ") + 1, versionString.Length - versionString.IndexOf(" ") - 1);
            }
        }

        protected string versionString = "";
        protected string softwareRevision = "";
        protected string boardType = "";
        protected string engineCode = "";
        protected string classCode = "";
    }

    /// <summary>
    /// Firmware update data recieved from the scanner
    /// </summary>
    public class FirmwareUpdateData : EventArgs
    {
        public FirmwareUpdateData(bool isStart, int length)
        {
            this.isStart = isStart;
            this.length = length;
        }

        protected bool isStart;
        protected int length;
    }

    /// <summary>
    /// Capabilities data recieved from the scanner
    /// </summary>
    public class CapabilitiesData : EventArgs
    {
        public CapabilitiesData(byte[] data, int length)
        {
            this.data = data;
            this.length = length;
        }

        protected byte[] data;
        protected int length;
    }

    /// <summary>
    /// Parameter data recieved from the scanner
    /// </summary>
    public class ParamData : EventArgs
    {
        public ParamData(byte[] data, int length)
        {
            this.data = data;
        }

        protected byte[] data;
    }

    public class ScannerInvalidException : System.Exception
    {
        public ScannerInvalidException()
            : base("This scanner instance is either no longer attached or has not been claimed")
        {
        }
    }

    /// <summary>
    /// A USB scanner that supports imaging using the SNAPI protocol
    /// </summary>
    public class SnapiScanner
    {
        /// <summary>
        /// Occurs when a decode message is received from the scanner. The message type received is DecodeData
        /// </summary>
        public event System.EventHandler Decode;

        /// <summary>
        /// Occurs when an image is recieved from the scanner.
        /// </summary>
        public event System.EventHandler Image;

        /// <summary>
        /// Occurs when a video frame is received from the scanner
        /// </summary>
        public event System.EventHandler Video;

        /// <summary>
        /// Occurs when a version data is received from the scanner
        /// </summary>
        public event System.EventHandler Version;

        /// <summary>
        /// Occurs when a parameter data is received from the scanner
        /// </summary>
        public event System.EventHandler Params;

        /// <summary>
        /// Occurs when a capabilities data is received from the scanner
        /// </summary>
        public event System.EventHandler Capabilities;

        /// <summary>
        /// Occurs when a firmware update data is received from the scanner
        /// </summary>
        public event System.EventHandler FirmwareUpdate;

        /// <summary>
        /// Occurs when the claimed scanner is removed from the system; we receieve a PnP disconnect event.
        /// </summary>
        public static event SnapiScanner.AttachHandler Disconnect;

        /// <summary>
        /// Occurs when a new SNAPI scanner has been attached to the system
        /// </summary>
        public static event SnapiScanner.AttachHandler Attached;

        /// <summary>
        /// A collection of all known, SNAPI scanners, attached to the system
        /// </summary>
        public static SnapiScannerCollection AttachedScanners = new SnapiScannerCollection();

        /// <summary>
        /// Handler for events which detect newly connected scanners to the system
        /// </summary>
        /// <param name="scanner">The scanner that has connect to the system</param>
        public delegate void AttachHandler(SnapiScanner scanner);

        /// <summary>
        /// Handler for events which for disconnected scanners from the system
        /// </summary>
        /// <param name="scanner"></param>
        public delegate void DetachHandler(SnapiScanner scanner);

        /// <summary>
        /// Handle a bug in the SNAPI library that prematurely signals the attachement of a new scanner
        /// before it is able to be claimed. This only occurs when the scanner attaching supports the
        /// imaging channel.
        /// <para>
        /// If your scanners do not support "SNAPI with Imaging" then you can set this to false.
        /// </para>
        /// </summary>
        public static bool AttachmentBug = true;

        private static int prevHandleCon = 0;
        private static int prevHandleDis = 0;

        /// <summary>
        /// A unique name that identifies the scanner within the context of the application
        /// </summary>
        public String Name
        {
            get { return "Scanner" + scannerId.ToString(); }
        }

        private void onScannerMsg(int handle, int msg, int loword, int hiword)
        {
            // since all the scanners hook into the Manager events we must see if this is ours
            if (this.handle != handle) return;

            switch (msg)
            {
                case SnapiDLL.WM_DECODE:
                    // if no receiver for decode messages then we're done
                    if (Decode == null) break;

                    // create managed array for decode data and pass to event handler
                    byte[] decData = new byte[hiword - 2];
                    Array.Copy(dllBuffer, 2, decData, 0, hiword - 2);
                    Decode(this, new DecodeData((DecodeData.CodeTypes)dllBuffer[0], decData));
                    break;

                case SnapiDLL.WM_IMAGE:
                    if (Image == null) break;
                    Image(this, ImageDataFactory.CreateImageData(dllBuffer, hiword));
                    break;

                case SnapiDLL.WM_VIDEO:
                    if (Video == null) break;
                    Video(this, ImageDataFactory.CreateImageData(dllBuffer, hiword));
                    break;

                case SnapiDLL.WM_SWVERSION:
                    if (Version == null) break;
                    byte[] versionData = new byte[hiword];
                    Array.Copy(versionBuffer, 0, versionData, 0, hiword);
                    Version(this, new VersionData(versionData, hiword));
                    break;

                case SnapiDLL.WM_CAPABILITIES:
                    if (Capabilities == null) break;
                    if (hiword == 0) break;
                    byte[] capData = new byte[hiword];
                    Array.Copy(capBuffer, 0, capData, 0, hiword);
                    Capabilities(this, new CapabilitiesData(capData, hiword)); ;
                    break;

                case SnapiDLL.WM_FU_PROGRESS:
                    if (FirmwareUpdate == null) break;
                    FirmwareUpdate(this, new FirmwareUpdateData(false, hiword));
                    break;

                case SnapiDLL.WM_FU_STARTED:
                    if (FirmwareUpdate == null) break;
                    FirmwareUpdate(this, new FirmwareUpdateData(true, hiword));
                    break;

                case SnapiDLL.WM_PARAMS:
                    if (Params == null) break;
                    byte[] paramData = new byte[hiword];
                    Array.Copy(paramBuffer, 0, paramData, 0, hiword);
                    Params(this, new ParamData(paramData, hiword)); ;
                    break;

                case SnapiDLL.WM_DEVICE_NOTIFICATION:
                    // we're only concerned with disconnect events
                    if (loword != SnapiDLL.DEVICE_DISCONNECT) break;

                    // remove us from the managers WndProc routing
                    Manager.ScannerMsg -= new Manager.ScannerMsgHandler(onScannerMsg);

                    // fire event stating that we've disconnected from system
                    if (Disconnect != null) Disconnect(this);

                    // release any resources we may have allocated
                    Release();

                    // show this scanner is no longer valid
                    isValid = false;
                    break;

                default:
                    break;
            }
        }

        public SnapiScanner(int handle, int id)
        {
            this.handle = handle;
            this.scannerId = id;
            this.paramPersist = true;

            // route SNAPI messages to this scanner from Manager
            Manager.ScannerMsg += new Manager.ScannerMsgHandler(onScannerMsg);

            // obtain and populate the s/n for this scanner
            System.Text.StringBuilder sb = new System.Text.StringBuilder(100);
            SnapiDLL.SNAPI_GetSerialNumber(handle, sb);

            // bit of a hack but we know what to expect from the s/n
            serialNo = sb.ToString(4, sb.Length - 4);

            int fmStart = serialNo.IndexOf(' ');
            firmware = serialNo.Substring(fmStart + 5);
            serialNo = serialNo.Remove(fmStart);

            // no one has claimed (opened) us but we are a valid scanner (for now)
            isAttached = false;
            isValid = true;
            Manager.Attachedmgr += new SnapiScanner.AttachHandler(OnAttachmentDevice);
            Manager.Detachedmgr += new SnapiScanner.DetachHandler(OnDetachmentDevice);
        }

        /// <summary>
        /// Attach to this scanner and allow operations on it
        /// </summary>
        /// <returns>Returns true if successfully attched to the scanner; false otherwise</returns>
        public bool Claim()
        {
            if (isAttached) return true;
            if (!isValid) return false;

            int status = SnapiDLL.SNAPI_Connect(handle);
            if (status == SnapiDLL.SNAPI_NOERROR)
            {
                isAttached = true;

                System.Threading.Monitor.Enter(this);

                // The SNAPI dll expects this memory to be fixed; so we need to pin it in .NET
                dllBuffer = new byte[SnapiDLL.MAX_VIDEO_LEN];
                pinnedBuffer = GCHandle.Alloc(dllBuffer, GCHandleType.Pinned);

                versionBuffer = new byte[SnapiDLL.MAX_VIDEO_LEN];
                pinnedVersionBuffer = GCHandle.Alloc(versionBuffer, GCHandleType.Pinned);

                paramBuffer = new byte[SnapiDLL.MAX_VIDEO_LEN];
                pinnedParamBuffer = GCHandle.Alloc(paramBuffer, GCHandleType.Pinned);

                capBuffer = new byte[SnapiDLL.MAX_VIDEO_LEN];
                pinnedCapBuffer = GCHandle.Alloc(capBuffer, GCHandleType.Pinned);

                unsafe
                {
                    SnapiDLL.SNAPI_SetDecodeBuffer(handle, pinnedBuffer.AddrOfPinnedObject(), SnapiDLL.MAX_VIDEO_LEN);
                    SnapiDLL.SNAPI_SetImageBuffer(handle, pinnedBuffer.AddrOfPinnedObject(), SnapiDLL.MAX_VIDEO_LEN);
                    SnapiDLL.SNAPI_SetVideoBuffer(handle, pinnedBuffer.AddrOfPinnedObject(), SnapiDLL.MAX_VIDEO_LEN);
                    SnapiDLL.SNAPI_SetVersionBuffer(handle, pinnedVersionBuffer.AddrOfPinnedObject(), SnapiDLL.MAX_VIDEO_LEN);
                    SnapiDLL.SNAPI_SetCapabilitiesBuffer(handle, pinnedCapBuffer.AddrOfPinnedObject(), SnapiDLL.MAX_VIDEO_LEN);
                    SnapiDLL.SNAPI_SetParameterBuffer(handle, pinnedParamBuffer.AddrOfPinnedObject(), SnapiDLL.MAX_VIDEO_LEN);
                    SnapiDLL.SNAPI_SetParamPersistance(handle, paramPersist ? 1 : 0);
                }
                System.Threading.Monitor.Exit(this);
            }
            return isAttached;
        }

        /// <summary>
        /// Disconnect from a previously claimed scanner
        /// </summary>
        public void Release()
        {
            if (!isAttached) return;

            System.Threading.Monitor.Enter(this);
            SnapiDLL.SNAPI_Disconnect(handle);
            isAttached = false;
            dllBuffer = null;
            pinnedBuffer.Free();

            versionBuffer = null;
            pinnedVersionBuffer.Free();

            paramBuffer = null;
            pinnedParamBuffer.Free();

            capBuffer = null;
            pinnedCapBuffer.Free();

            System.Threading.Monitor.Exit(this);
        }

        /// <summary>
        /// Assign a new value to a scanner parameter
        /// </summary>
        /// <param name="param">A single parameter value to change</param>
        /// <returns>Returns true if the parameter was changed; false otherwise</returns>
        /// <exception cref="ScannerInvalidException">Thrown if the scanner instance is either not attached or invalid</exception>
        /// <exception cref="SnapiSimpleParam">Thrown if the parameter is not of a simple type</exception>
        public bool SetParam(SnapiParam param)
        {
            if (!isAttached || !isValid)
                Console.WriteLine(@"This scanner instance is either no longer attached or has not been claimed");

            // For now we only deal with integers and booleans; no strings
            SnapiSimpleParam simpleParam = (SnapiSimpleParam)param;
            if (simpleParam == null) throw new InvalidParamClass();

            short[] p = { simpleParam.RawId, simpleParam.RawValue };
            return (SnapiDLL.SNAPI_SetParameters(p, 2, handle) == SnapiDLL.SNAPI_NOERROR);
        }

        /// <summary>
        /// On scanner device attached
        /// </summary>
        /// <param name="s"></param>
        private void OnAttachmentDevice(SnapiScanner s)
        {
            prevHandleDis = 0;
            if (s.handle != SnapiScanner.prevHandleCon)
            {
                prevHandleCon = s.handle;
                if (Attached != null) Attached(s);
            }
        }

        /// <summary>
        /// On scanner device detached
        /// </summary>
        /// <param name="s"></param>
        private void OnDetachmentDevice(SnapiScanner s)
        {
            SnapiScanner.prevHandleCon = 0;
            if (s.handle != SnapiScanner.prevHandleDis)
            {
                prevHandleDis = s.handle;
                if (SnapiScanner.Disconnect != null) SnapiScanner.Disconnect(s);
            }
        }

        public bool ScannerEnabled
        {
            get
            {
                return this.scannerEnable;
            }

            set
            {
                if (!isAttached || !isValid) throw new ScannerInvalidException();
                if (value)
                {
                    SnapiDLL.SNAPI_ScanEnable(handle);
                    this.scannerEnable = true;
                }
                else
                {
                    SnapiDLL.SNAPI_ScanDisable(handle);
                    this.scannerEnable = false;
                }
            }
        }

        /// <summary>
        /// Manages the single instance of the SNAPI DLL. Maintains a collection of scanners that
        /// are presently connected to the system. It is also responsible for routing the SNAPI
        /// library windows messages to the appropriate scanner object.
        /// </summary>

        // SnapiScanner state variables
        private String firmware;                // Installed firmware (release) string

        public String serialNo;                 // Unique S/N for scanner unit
        private bool paramPersist;              // Permanent or temporary params
        private bool scannerEnable;             // Scanner enable/disable
        private bool videoViewFinderEnable;     // Video view finder enable/disable
        private int scannerId;                  // Simple id for scanner within this Manager instance
        public int handle;                      // handle from SNAPI DLL for this scanner
        private bool isAttached;                // whether this scanner has been claimed (opened) or not
        private bool isValid;                   // if scanner disconnects, then the object is now invalid; cannot support comm
        private byte[] dllBuffer;               // Various destination buffers for SNAPI.DLL
        private GCHandle pinnedBuffer;          // pinned version of dllBuffer passed to SNAPI.DLL
        private byte[] versionBuffer;           // version destination buffers for SNAPI.DLL
        private GCHandle pinnedVersionBuffer;   // pinned version of versionBuffer passed to SNAPI.DLL
        private byte[] paramBuffer;             // Parameter destination buffers for SNAPI.DLL
        private GCHandle pinnedParamBuffer;     // pinned version of paramBuffer passed to SNAPI.DLL
        private byte[] capBuffer;               // Capabilties destination buffers for SNAPI.DLL
        private GCHandle pinnedCapBuffer;       // pinned version of capBuffer passed to SNAPI.DLL
    }
}