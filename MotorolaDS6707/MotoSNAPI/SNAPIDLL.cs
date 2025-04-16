using System;
using System.Runtime.InteropServices;

namespace TIM.Devices.MotorolaDS6707.MotoSNAPI
{
    internal class SnapiDLL
    {
        /*****************************************************************************************************
         * Constants
         *****************************************************************************************************/
        public const int SNAPI_NOERROR = 0;                 /* Function return value - no error          */
        public const int MAX_VIDEO_LEN = 1312000;           /* Maximum frame buffer size                 */
        /* (Frame Width*Frame Hight)+Frame Header len*/
        /* 1312000 = (1280*1024) + 0x500             */

        public const int MAX_PARAM_LEN = 2;                 /* Maximum number of bytes per parameter     */
        public const int DEVICE_ATTACH = 0;
        public const int DEVICE_DISCONNECT = 1;

        public const short BMP_FILE_SELECTION = 0x0003;     /* These values may change with the scanner  */
        public const short TIFF_FILE_SELECTION = 0x0004;    /* models. Please refer scanner PRGs for     */
        public const short JPEG_FILE_SELECTION = 0x0001;    /* more information on scanner parameters.   */
        public const short VIDEOVIEWFINDER_PARAMNUM = 0x0144;
        public const short VIDEOVIEWFINDER_ON = 0x0001;     /* Video view finder on                      */
        public const short VIDEOVIEWFINDER_OFF = 0x0000;    /* Video view finder off                     */

        public const int LED_1 = 0x0001;   /* Select LED 1 */
        public const int LED_2 = 0x0002;   /* Select LED 2 */
        public const int LED_3 = 0x0004;   /* Select LED 3 */
        public const int LED_4 = 0x0008;   /* Select LED 4 */
        public const int LED_5 = 0x0010;   /* Select LED 5 */

        /*****************************************************************************************************
         * DLL Imports
         * ===========
         * Please make sure to add the SNAPI.DLL path inside DllImport method if you are
         * using relative paths as illustrated below examples.
         *
         * Example 1
         *      [DllImport("SNAPI.dll")]
         *      If SNAPI.DLL is in your installation path “..\Motorola SNAPI SDK\SDK\SNAPI.DLL” and the
         *      sample application is in “..\Motorola SNAPI SDK\Samples\src\SNAPI_APP\bin\Release\” folder.
         *
         * Example 2
         *      [DllImport("SNAPI.dll")]
         *      If,
         *         1. SNAPI.DLL resides with your application in the same folder.
         *         2. SNAPI.DLL resides in folder where the folder is in your PATH environment variable.
         *         3. SNAPI.DLL resides in “..\WINDOWS\system32\”.
         *
         *****************************************************************************************************/
        
        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_Init(IntPtr hwnd, int[] DeviceHandles, int* NumDevices);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_Connect(int DeviceHandles);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_Disconnect(int DeviceHandles);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_PullTrigger(int DeviceHandle);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_ReleaseTrigger(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_GetSerialNumber(int DeviceHandle, System.Text.StringBuilder SerialNo);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_SnapShot(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_TransmitVideo(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_TransmitVersion(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_AimOn(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_AimOff(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_LedOn(int DeviceHandle, byte nLEDselection);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_LedOff(int DeviceHandle, byte nLEDselection);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_ScanEnable(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_ScanDisable(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_SoundBeeper(int DeviceHandle, byte nBeepCode);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_UpdateFirmware(byte[] frmFilePath, int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_AbortFirmwareUpdate(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_RequestScannerCapabilities(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_AbortMacroPdf(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_FlushMacroPdf(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_SetParameterDefaults(int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_SetParamPersistance(int DeviceHandle, int bPersist);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_SetParameters(short[] Params, int ParamWords, int DeviceHandle);

        [DllImport("SNAPI.dll", CharSet = CharSet.Ansi)]
        public static extern unsafe int SNAPI_RequestParameters(short[] Params, int ParamWords, int DeviceHandle);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_SetVideoBuffer(int DeviceHandle, IntPtr buf, int max_length);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_SetImageBuffer(int DeviceHandle, IntPtr buf, int max_length);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_SetDecodeBuffer(int DeviceHandle, IntPtr buf, int max_length);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_SetParameterBuffer(int DeviceHandle, IntPtr buf, int max_length);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_SetVersionBuffer(int DeviceHandle, IntPtr pData, int max_length);

        [DllImport("SNAPI.dll")]
        public static extern unsafe int SNAPI_SetCapabilitiesBuffer(int DeviceHandle, IntPtr pData, int max_length);

        public const int WM_APP = 0x8000;   /* Starting value for windows messages       */
        /*****************************************************************************************************
        *  Windows Messages sent to calling process
        *****************************************************************************************************/
        public const int WM_DECODE = WM_APP + 1;		//Sent if there is decode data available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is  the buffer status of the data stored
        // HIDWORD(wparam)  is the length of the data in bytes
        //lparam  is the handle to the device for which the message was posted

        public const int WM_IMAGE = WM_APP + 2;		//Sent if there is image data available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        //LODWORD (wparam ) is  the buffer status of the data stored
        //HIDWORD(wparam)  is the length of the data in bytes
        //lparam  is the handle to the device for which the message was posted

        public const int WM_VIDEO = WM_APP + 3;	    //Sent if there is a video frame available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is  the buffer status of the data stored
        // HIDWORD(wparam)  is the length of the data in bytes
        //lparam  is the handle to the device for which the message was posted

        public const int WM_ERROR = WM_APP + 4;		//Sent if an error occurred.
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the error code (cast to signed short) (see WM_ERROR codes list )
        //lparam  is the handle to the device for which the message was posted

        public const int WM_TIMEOUT = WM_APP + 5;		//Sent if the scanner does not respond to a request from the library within
        // the timeout for the request.
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam )  is set to zero (reserved for future use)
        // HIDWORD (wparam )  is the request code (int) that did not receive the response
        // (see WM_TIMEOUT codes list)
        //lparam  is the handle to the device for which the message was posted

        private const int WM_CMDCOMPLETEMSG = WM_APP + 6;       //Sent when an ACK is received from the scanner in response to a handled
        // user command.
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the command status.
        // HIDWORD (wparam ) is extended command status.
        //lparam  is the handle to the device for which the message was posted

        private const int WM_XFERSTATUS = WM_APP + 7;  	    //Sent during the transfer of image data from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *).
        // LODWORD (wparam ) is the total number of bytes received so far (cast to uint)
        // HIDWORD (wparam ) is the total number of bytes expected (cast to uint)
        //lparam  is the handle to the device for which the message was posted

        public const int WM_SWVERSION = WM_APP + 8;	    //Sent when the software version information is available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the buffer status code
        // HIDWORD (wparam ) is the length of the data in bytes (cast to int).
        // version data is device dependent
        //lparam  is the handle to the device for which the message was posted

        public const int WM_PARAMS = WM_APP + 9;       //Sent when parameter information is available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the buffer status,
        // HIDWORD (wparam ) is the length of the parameter data as number of words(cast to int).
        //lparam  is the handle to the device for which the message was posted

        public const int WM_CAPABILITIES = WM_APP + 10;	    //Sent when capabilities data is available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam )  is the buffer status
        // HIDWORD (wparam ) is the length of the data in bytes (cast to int).
        //lparam  is the handle to the device for which the message was posted

        private const int WM_EVENT = WM_APP + 11;      //Sent when event data is available from the scanner
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the event data
        // HIDWORD (wparam ) is the length of the data in bytes (always 1 byte).
        //lparam  is the handle to the device for which the message was posted

        public const int WM_DEVICE_NOTIFICATION = WM_APP + 12;      //Sent when a SNAPI compatible device is detected or
        // removed from the system.
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is either of
        // - DEVICE_ARRIVE or DEVICE_REMOVE
        //lparam  is the handle to the device for which the message was posted

        private const int WM_MGMT_CMD_RESP = WM_APP + 13;      //Sent when a response is available from the scanner to a management command
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam )  is the buffer status
        // HIDWORD (wparam ) is the length of the data in bytes (cast to int).
        //lparam  is the handle to the device for which the message was posted

        public const int WM_FU_STARTED = WM_APP + 14;      //Sent when firmware update process starts
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the number steps in firmware update process
        //lparam  is the handle to the device for which the message was posted

        public const int WM_FU_PROGRESS = WM_APP + 15;      //Sent when each firmware update step finished
        //wparam is a pointer to DWPARAM structure (cast to DWPARAM *)
        // LODWORD (wparam ) is the current step number
        //lparam  is the handle to the device for which the message was posted
    }
}