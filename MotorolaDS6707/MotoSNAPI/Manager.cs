using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TIM.Devices.MotorolaDS6707.MotoSNAPI
{
    internal class Manager
    {
        private const uint MAX_AVAIL_SCANNERS = 10;       /* Maximum number of scanners to be connected*/

        public delegate void ScannerMsgHandler(int handle, int msg, int lo, int hi);

        public static event ScannerMsgHandler ScannerMsg;

        public static event SnapiScanner.AttachHandler Attachedmgr;

        public static event SnapiScanner.DetachHandler Detachedmgr;

        static public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Manager.Instance.numScanners; ++i)
            {
                yield return Manager.knownScanners[i];
            }
        }

        private Manager()
        {
            msg = new SnapiMsgHandler();
            msg.SnapiMsg += new SnapiMsgHandler.MsgHandler(onSnapiMsg);

            int[] devHandles = new int[MAX_AVAIL_SCANNERS];
            int nh;
            numScanners = 0;

            unsafe
            {
                /* Initilaze SANPI.dll to detect all the scanners attached */
                int status = SnapiDLL.SNAPI_Init(msg.Handle, devHandles, &nh);
            }

            knownScanners = new SnapiScanner[10];

            int i = 0;

            for (i = 0; i < nh; ++i) knownScanners[i] = new SnapiScanner(devHandles[numScanners++], Manager.scannerId++);
        }

        protected void onSnapiMsg(ref System.Windows.Forms.Message msg)
        {
            // quick out for non-essential Windows messages
            if (msg.Msg < SnapiDLL.WM_APP) return;

            switch (msg.Msg)
            {
                case SnapiDLL.WM_DECODE:
                case SnapiDLL.WM_IMAGE:
                case SnapiDLL.WM_PARAMS:
                case SnapiDLL.WM_VIDEO:
                case SnapiDLL.WM_CAPABILITIES:
                case SnapiDLL.WM_SWVERSION:
                    if (ScannerMsg != null) ScannerMsg((int)msg.LParam, (int)msg.Msg, Marshal.ReadInt32((IntPtr)msg.WParam, 4), Marshal.ReadInt32((IntPtr)msg.WParam, 0));
                    break;

                case SnapiDLL.WM_FU_PROGRESS:
                case SnapiDLL.WM_FU_STARTED:
                    if (ScannerMsg != null) ScannerMsg((int)msg.LParam, (int)msg.Msg, msg.WParam.ToInt32(), msg.WParam.ToInt32());
                    break;

                case SnapiDLL.WM_DEVICE_NOTIFICATION:
                    int ev = Marshal.ReadInt32((IntPtr)msg.WParam, 4);
                    if (ev == SnapiDLL.DEVICE_ATTACH)
                    {
                        // wait for a zero attachment handle, then fire the event for the prior handle
                        if (SnapiScanner.AttachmentBug)
                        {
                            if ((int)msg.LParam == 0)
                            {
                                if (priorHandle == 0) break;
                                msg.LParam = (IntPtr)priorHandle;
                                SnapiScanner s = new SnapiScanner((int)msg.LParam, Manager.scannerId++);
                                knownScanners[numScanners++] = s;

                                // send notification
                                if (Manager.Attachedmgr != null) Manager.Attachedmgr(s);
                            }
                            else
                            {
                                priorHandle = (int)msg.LParam;
                                break;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            int i = 0; int y;
                            SnapiScanner[] temp = new SnapiScanner[MAX_AVAIL_SCANNERS];

                            do
                            {
                                if (knownScanners[i] == null)   //end of list
                                    break;
                                if ((int)msg.LParam == knownScanners[i].handle)
                                {
                                    SnapiScanner s;
                                    Array.Copy(knownScanners, temp, MAX_AVAIL_SCANNERS);
                                    s = knownScanners[i];
                                    for (y = i; y < MAX_AVAIL_SCANNERS - 1; y++)
                                    {
                                        knownScanners[y] = temp[y + 1];
                                    }
                                    knownScanners[y] = null;
                                    numScanners--;
                                    // send notification
                                    if (Manager.Detachedmgr != null) Manager.Detachedmgr(s);
                                    break;
                                }
                                else
                                    i++;
                            } while (i != MAX_AVAIL_SCANNERS);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }

                        if (ScannerMsg != null) ScannerMsg((int)msg.LParam, (int)msg.Msg, ev, Marshal.ReadInt32((IntPtr)msg.WParam, 0));
                    }
                    break;

                default:
                    break;
            }
        }

        private static int priorHandle = 0;

        static public Manager Instance
        {
            get { return manager; }
        }

        private SnapiMsgHandler msg;

        // Scanner collection
        static public SnapiScanner[] knownScanners;

        public int numScanners;

        private static Manager manager = new Manager();   // singleton instance
        private static int scannerId = 1;
    }
}