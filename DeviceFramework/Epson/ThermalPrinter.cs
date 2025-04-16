using com.epson.pos.driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Extensions;
using TIM.Devices.Framework.Common.Printer;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.Common.Threading;

namespace TIM.Devices.Framework.Epson
{
    /// <summary>
    /// Epson ThermalPrinter class for secure printing and controlling cash drawer
    /// </summary>
    public class ThermalPrinter
    {
        private const bool LogToFileEnabled = false;

        private static object objExecuteLock = new object();
        private static object objStatusAPILock = new object();

        private static Enums.PrinterTypeFrontend[] MyThermalPrinterTypes =
            new Enums.PrinterTypeFrontend[] { Enums.PrinterTypeFrontend.ReceiptPrinter, Enums.PrinterTypeFrontend.KitchenReceiptPrinter, Enums.PrinterTypeFrontend.CounterReceiptPrinter };

        private static Enums.PrinterStatus[] MyInvalidPrinterStati =
            new Enums.PrinterStatus[] { Enums.PrinterStatus.Unknown, Enums.PrinterStatus.Offline, Enums.PrinterStatus.Stopped, Enums.PrinterStatus.Other };

        #region Events

        /// <summary>
        /// Fires if the drawer opens
        /// </summary>
        public event EventHandler DrawerOpened;

        /// <summary>
        /// Fires if the drawer gets closed
        /// </summary>
        public event EventHandler DrawerClosed;

        /// <summary>
        /// Fires if the drawer either opens or gets closed
        /// </summary>
        public event EventHandler<GenericEventArgs<bool>> DrawerStateChanged;

        /// <summary>
        /// Fires if the alarm starts because the drawer was opened too long
        /// </summary>
        public event EventHandler DrawerOpenedTooLong;

        /// <summary>
        /// Fires if the cover gets opened
        /// </summary>
        public event EventHandler CoverOpened;

        /// <summary>
        /// Fires if the cover gets closed
        /// </summary>
        public event EventHandler CoverClosed;

        /// <summary>
        /// Fires if the cover either gets opened or closed
        /// </summary>
        public event EventHandler<GenericEventArgs<bool>> CoverStateChanged;

        /// <summary>
        /// Fires if the paper is out
        /// </summary>
        public event EventHandler PaperOut;

        /// <summary>
        /// Fires if the actual print process will be started in the observed environment
        /// </summary>
        public event EventHandler<GenericEventArgs<Container<PrintPageEventArgs, object>>> PrintPage;

        /// <summary>
        /// Fires if the printer is valid, accessable and the print was started
        /// </summary>
        public event EventHandler PrintStarted;

        /// <summary>
        /// Fires if the print finished
        /// </summary>
        public event EventHandler PrintFinshed;

        /// <summary>
        /// Fires if an error occured because the drawer couldn't be opened
        /// </summary>
        public event EventHandler DrawerError;

        /// <summary>
        /// Fires if the execute method starts
        /// </summary>
        //public event EventHandler ExecuteStarted;
        /// <summary>
        /// Fires if the execute method finished, even if an error occured!
        /// </summary>
        public event EventHandler ExecuteFinished;

        #endregion

        #region Members

        private StatusAPI MyAPI;
        private bool blnPrintFinish;
        private bool blnCancelError;
        private bool blnDrawerOpen;
        private bool blnCoverOpen;
        private bool blnDrawerAlarmEnabled;
        private TimeSpan tsDrawerAlarm;
        private ManualResetEvent mreDrawerOpened;
        private ManualResetEvent mreDrawerClosed;
        private ManualResetEvent mreCoverOpened;
        private ManualResetEvent mreCoverClosed;
        private ManualResetEvent mrePrintTimeout;
        private object objData;
        private AsyncOperation MyAsyncOperation;
        private bool blnOpenDrawer;
        private bool blnPaperOutExchange;
        private bool blnIsBonPrinter;
        private bool blnForceSpooler;
        private string strPrinterName;
        private bool blnPrint;
        private PageSettings MyPageSettings;

        #endregion

        #region Getter & Setter

        /// <summary>
        /// Gets whether the drawer is open
        /// </summary>
        public bool DrawerOpen
        {
            get { return blnDrawerOpen; }
        }

        /// <summary>
        /// Gets whether the cover is open
        /// </summary>
        public bool CoverOpen
        {
            get { return blnCoverOpen; }
        }

        /// <summary>
        /// Controls the alarm when the <paramref name="DrawerAlarmTime"/> is elapsed after <paramref name="DrawerOpened"/> was fired
        /// </summary>
        public bool DrawerAlarmEnabled
        {
            get { return blnDrawerAlarmEnabled; }
            set { blnDrawerAlarmEnabled = value; }
        }

        /// <summary>
        /// Defines the time that may elapse while the drawer is open before the alarm gets fired
        /// </summary>
        public TimeSpan DrawerAlarmTime
        {
            get { return tsDrawerAlarm; }
            set { tsDrawerAlarm = value; }
        }

        /// <summary>
        /// Gets whether the printer is online
        /// </summary>
        public Boolean PrinterOffline
        {
            get
            {
                string strPrinterName = SettingsHelper.GetPrinterName(Enums.PrinterTypeFrontend.ReceiptPrinter);

                if (!PrinterHelper.IsValidPrinter(strPrinterName))
                    return true;
                Enums.PrinterStatus MyPrinterStatus = PrinterHelper.GetStatus(strPrinterName);
                if (MyPrinterStatus == Enums.PrinterStatus.Unknown ||
                    MyPrinterStatus == Enums.PrinterStatus.Offline ||
                    MyPrinterStatus == Enums.PrinterStatus.Stopped ||
                    MyPrinterStatus == Enums.PrinterStatus.Other)
                    return true;

                return false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new ThermalPrinter instance with default values (<paramref name="DrawerAlarmEnabled"/> = true; <paramref name="DrawerAlarmTime"/> = 0:0:10)
        /// </summary>
        public ThermalPrinter()
        {
            this.MyAPI = new StatusAPI();
            this.MyAPI.StatusCallback += new StatusAPI.StatusCallbackHandler(MyAPI_StatusCallback);
            this.blnDrawerAlarmEnabled = true;
            this.tsDrawerAlarm = new TimeSpan(0, 0, 10);

            mreCoverOpened = new ManualResetEvent(false);
            mreCoverClosed = new ManualResetEvent(false);

            MyAsyncOperation = AsyncOperationManager.CreateOperation(null);

            LogToFile("Created");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the printing and drawer controlling process
        /// </summary>
        /// <param name="blnOpenDrawer">Defines whether the drawer should be opened after printing</param>
        /// <param name="objData">Some custom data that gets forwarded to the <paramref name="Print"/> event args</param>
        /// <param name="MyPrinterType">The primary PrinterType</param>
        /// <param name="MyFallbackPrinterTypes">The alternative fallback PrinterTypes</param>
        public void Execute(bool blnOpenDrawer, object objData, Enums.PrinterTypeFrontend MyPrinterType, params Enums.PrinterTypeFrontend[] MyFallbackPrinterTypes)
        {
            Execute(blnOpenDrawer, objData, true, MyPrinterType, MyFallbackPrinterTypes);
        }

        /// <summary>
        /// Starts the printing and drawer controlling process
        /// </summary>
        /// <param name="blnOpenDrawer">Defines whether the drawer should be opened after printing</param>
        /// <param name="objData">Some custom data that gets forwarded to the <paramref name="Print"/> event args</param>
        /// <param name="blnPrint">Defines whether to print or just simulate a print (true = print; false = simulate)</param>
        /// <param name="MyPrinterType">The primary PrinterType</param>
        /// <param name="MyFallbackPrinterTypes">The alternative fallback PrinterTypes</param>
        public void Execute(bool blnOpenDrawer, object objData, bool blnPrint, Enums.PrinterTypeFrontend MyPrinterType, params Enums.PrinterTypeFrontend[] MyFallbackPrinterTypes)
        {
            this.blnOpenDrawer = blnOpenDrawer;

            Exception MyCachedException = null;

            Thread MyThread = new Thread(new ThreadStart(delegate
            {
                lock (objExecuteLock)
                {
                    try
                    {
                        LogToFile("Executing");

                        this.objData = objData;
                        this.blnPrint = blnPrint;

                        DetectPrinter(MyPrinterType, MyFallbackPrinterTypes, ref this.strPrinterName,
                                      ref this.blnIsBonPrinter, ref this.blnForceSpooler);

                        // Config page settings for open drawer only, pseudo print!
                        PrinterSettings MyPrinterSettings = new PrinterSettings();
                        MyPrinterSettings.PrinterName = this.strPrinterName;
                        MyPageSettings = null;
                        if (!blnPrint && blnIsBonPrinter)
                        {
                            LogToFile("Setting empty print for cashdrawer only");

                            MyPageSettings = new PageSettings(MyPrinterSettings);
                            MyPageSettings.PaperSource =
                                (from MyPaperSource in MyPrinterSettings.PaperSources.Cast<PaperSource>()
                                 where (MyPaperSource.RawKind == (int)PaperSourceKind.FormSource) ||
                                        (MyPaperSource.RawKind == (int)CustomPaperSources.DocumentNoFeedNoCut)
                                 select MyPaperSource).Single();
                        }

                        if (blnIsBonPrinter)
                        {
                            if (blnOpenDrawer && !blnPrint)
                            {
                                ProcessOpenDrawer(MyPrinterSettings);
                            }
                            else
                            {
                                ProcessBonPrinter(MyPrinterSettings);
                            }
                        }
                        else
                            ProcessPrinter(MyPrinterSettings);
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception ex)
                    {
                        MyCachedException = ex;
                    }
                    finally
                    {
                        EventHelper.Fire(this, ExecuteFinished, EventArgs.Empty, MyAsyncOperation);
                    }
                }
            }));
            MyThread.Start();

            while (MyThread.ThreadState == ThreadState.Running)
            {
                Application.DoEvents();
                Thread.Sleep(250);

                if (ThreadManager.GlobalAbort.IsSet)
                    MyThread.Abort();
            }

            LogToFile("Executed");

            if (MyCachedException != null)
                throw MyCachedException;
        }

        public void SimulateClosingDrawer()
        {
            HandleDrawerClosed();
        }

        /// <summary>
        /// Checks a list of defined PrinterTypes for availability and status. Additionally read extra printer configs from DB.
        /// </summary>
        /// <param name="MyPrinterType">The primary PrinterType</param>
        /// <param name="MyFallbackPrinterTypes">The alternativ fallback PrinterTypes</param>
        /// <param name="strPrinterName">The printer name</param>
        /// <param name="blnIsBonPrinter">Whether the detected printer is a bon printer</param>
        /// <param name="blnForceSpooler">Whether some setting forces using the spooler</param>
        private void DetectPrinter(Enums.PrinterTypeFrontend MyPrinterType, Enums.PrinterTypeFrontend[] MyFallbackPrinterTypes, ref string strPrinterName, ref bool blnIsBonPrinter, ref bool blnForceSpooler)
        {
            List<Enums.PrinterTypeFrontend> lstPrinterTypes = new List<Enums.PrinterTypeFrontend>();
            lstPrinterTypes.Add(MyPrinterType);
            lstPrinterTypes.AddRange(MyFallbackPrinterTypes);

            foreach (Enums.PrinterTypeFrontend MyPrinterTypeLocal in lstPrinterTypes)
            {
                strPrinterName = SettingsHelper.GetPrinterName(MyPrinterTypeLocal);
                if (!PrinterHelper.IsValidPrinter(strPrinterName))
                {
                    strPrinterName = "";
                }
                else
                {
                    Enums.PrinterStatus MyPrinterStatus = PrinterHelper.GetStatus(strPrinterName);
                    if (!MyInvalidPrinterStati.Contains(MyPrinterStatus))
                    {
                        blnIsBonPrinter = PrinterHelper.IsBonPrinter(strPrinterName);

                        // If thermalprinter check for ethernet to force spooler
                        if (MyThermalPrinterTypes.Contains(MyPrinterTypeLocal))
                        {
                            var bOk = Enum.TryParse(MyPrinterTypeLocal.ToString() + "_viaEthernet", out Enums.ComputerDetailSetting printerTypeLocal);
                            blnForceSpooler = bOk && SettingsManager.GetBoolOrPredefined(Enums.ComputerSetting.Printer, printerTypeLocal, false);
                        }

                        break;
                    }
                }
            }

            LogToFile(string.Format("PrinterName => '{0}'", strPrinterName));
            LogToFile(string.Format("IsBonPrinter => {0}", blnIsBonPrinter));
            LogToFile(string.Format("ForceSpooler => {0}", blnForceSpooler));

            if (string.IsNullOrEmpty(strPrinterName))
                throw new ThermalPrinterNotReadyException("Printer is invalid or not ready/connected");
        }

        private void Print(PrinterSettings MyPrinterSettings)
        {
            if (blnOpenDrawer)
                EventHelper.Fire(this, PrintStarted, EventArgs.Empty, MyAsyncOperation);

            LogToFile("Printing");

            mrePrintTimeout = new ManualResetEvent(false);

            if (PrintPage != null && (blnIsBonPrinter || blnPrint))
            {
                PrintDocument MyPrintDoc = new PrintDocument();
                MyPrintDoc.PrinterSettings = MyPrinterSettings;
                if (MyPageSettings != null)
                    MyPrintDoc.DefaultPageSettings.PaperSource = MyPageSettings.PaperSource;
                MyPrintDoc.DocumentName = "TIM-Bon";
                MyPrintDoc.PrintPage += new PrintPageEventHandler(MyPrintDoc_PrintPage);
                MyPrintDoc.Print();
            }

            if (blnIsBonPrinter && !blnForceSpooler && !(!blnPrint && blnIsBonPrinter))
            {
                // Wait for end of print
                if (!mrePrintTimeout.WaitOne(5000))
                    throw new ThermalPrinterPrintFailedException();
                mrePrintTimeout.WaitOne();

                if (blnOpenDrawer)
                    EventHelper.Fire(this, PrintFinshed, EventArgs.Empty, MyAsyncOperation);
            }

            LogToFile("Printed");
        }

        /// <summary>
        /// Open cashdrawer with retries, security alert and wait for close. Uses OpenDrawer.
        /// </summary>
        private void OpenCashDrawer()
        {
            // Open cash drawer
            if (blnOpenDrawer)
            {
                LogToFile("Opening Drawer");

                int intRetries = 0;
                do
                {
                    OpenDrawer();

                    // Wait for drawer to be really opened!
                    if (!mreDrawerOpened.WaitOne(2000))
                        ++intRetries;
                    else
                        intRetries = int.MaxValue;
                }
                while (intRetries < 3);

                if (intRetries > 2 && intRetries < int.MaxValue)
                    EventHelper.Fire(this, DrawerError, EventArgs.Empty, MyAsyncOperation);
                else
                {
                    // Security alert
                    if (blnDrawerAlarmEnabled)
                    {
                        // Wait for alert
                        if (!mreDrawerClosed.WaitOne(tsDrawerAlarm))
                        {
                            EventHelper.Fire(this, DrawerOpenedTooLong, EventArgs.Empty, MyAsyncOperation);
                            do
                            {
                                Notifier.PlayAudio(Enums.AudioSamples.Okay);
                            }
                            while (!mreDrawerClosed.WaitOne(5000));
                        }
                    }
                    else
                    {
                        // Wait until drawer is closed
                        mreDrawerClosed.WaitOne();
                    }
                }
            }
        }

        /// <summary>
        /// Simply open cashdrawer with retries. Used by OpenCashDrawer.
        /// </summary>
        private void OpenDrawer()
        {
            List<string> strErrors = new List<string>();
            ErrorCode MyErrorCode;

            for (int i = 0; i < 5; ++i)
            {
                LogToFile("Open Drawer");

                // Sometimes printer is locked
                try
                {
                    if (Environment.OSVersion.Version.Major < 6 ||
                        Environment.OSVersion.Version.Minor < 1)
                    {
                        // E.g. Windows XP
                        if (i > 0)
                        {
                            MyErrorCode = MyAPI.UnlockPrinter();
                            strErrors.Add(string.Format("UnlockPrinter() => {0}", MyErrorCode.ToString()));
                            Thread.Sleep(333);
                        }
                        MyErrorCode = MyAPI.OpenDrawer(Drawer.EPS_BI_DRAWER_1, Pulse.EPS_BI_PULSE_800);
                        strErrors.Add(string.Format("OpenDrawer(Drawer.EPS_BI_DRAWER_1, Pulse.EPS_BI_PULSE_800) => {0}", MyErrorCode.ToString()));
                        if (MyErrorCode == ErrorCode.SUCCESS)
                            return;
                    }
                    else
                    {
                        // E.g. Windows 7
                        int intRetries = 0;
                        MyErrorCode = MyAPI.UnlockPrinter();
                        LogToFile(MyErrorCode.ToString());
                        while (MyErrorCode != ErrorCode.SUCCESS &&
                            intRetries++ < 10)
                        {
                            Thread.Sleep(100);
                            //MyErrorCode = MyAPI.UnlockPrinter();
                            ASB MyASB;
                            MyErrorCode = MyAPI.GetRealStatus(out MyASB);
                            LogToFile(MyErrorCode.ToString());
                            LogToFile(MyASB.ToString());
                        }
                        if (MyErrorCode == ErrorCode.SUCCESS)
                        {
                            MyErrorCode = MyAPI.OpenDrawer(Drawer.EPS_BI_DRAWER_1, Pulse.EPS_BI_PULSE_800);
                            strErrors.Add(string.Format("OpenDrawer(Drawer.EPS_BI_DRAWER_1, Pulse.EPS_BI_PULSE_800) => {0}", MyErrorCode.ToString()));
                            if (MyErrorCode == ErrorCode.SUCCESS)
                                return;
                        }
                    }
                }
                catch
                {
                }
            }

            throw new ThermalPrinterException("Couldn't open drawer => " + string.Join(" | ", strErrors.ToArray()));
        }

        private void ProcessPrinter(PrinterSettings MyPrinterSettings)
        {
            LogToFile("Processing Printer");

            Print(MyPrinterSettings);
        }

        private void ProcessBonPrinter(PrinterSettings MyPrinterSettings)
        {
            lock (objStatusAPILock)
            {
                LogToFile("Processing BonPrinter");

                if (blnForceSpooler)
                {
                    LogToFile("Normal Spooler");

                    Print(MyPrinterSettings);
                }
                else
                {
                    LogToFile("Observed Spooler");

                    // Start printer monitoring
                    ErrorCode MyErrorCode;
                    MyErrorCode = MyAPI.OpenMonPrinter(OpenType.TYPE_PRINTER, MyPrinterSettings.PrinterName);
                    if (MyErrorCode == ErrorCode.SUCCESS)
                    {
                        // Init default pre-print values
                        blnCancelError = false;

                        mreDrawerClosed = new ManualResetEvent(false);
                        mreDrawerOpened = new ManualResetEvent(false);

                        MyErrorCode = MyAPI.SetStatusBack();
                        if (MyErrorCode == ErrorCode.SUCCESS)
                        {
                            Print(MyPrinterSettings);

                            // Recover printer if a recoverable error appears
                            if (blnCancelError)
                                MyAPI.CancelError();

                            OpenCashDrawer();

                            // Cleanup
                            MyAPI.CancelStatusBack();
                            MyErrorCode = MyAPI.CloseMonPrinter();
                            if (MyErrorCode != ErrorCode.SUCCESS)
                                throw new ThermalPrinterException(string.Format("Couldn't close the printer (ErrorCode: {0})", MyErrorCode));
                        }
                        else
                            throw new ThermalPrinterException(string.Format("Couldn't set status-callback (ErrorCode: {0})", MyErrorCode));
                    }
                    else
                        throw new ThermalPrinterException(string.Format("Couldn't open the printer (ErrorCode: {0})", MyErrorCode));
                }
            }
        }

        private void ProcessOpenDrawer(PrinterSettings MyPrinterSettings)
        {
            lock (objStatusAPILock)
            {
                LogToFile("Processing BonPrinter");

                if (blnForceSpooler)
                {
                    LogToFile("Normal Spooler");
                }
                else
                {
                    LogToFile("Observed Spooler");

                    // Start printer monitoring
                    ErrorCode MyErrorCode;
                    MyErrorCode = MyAPI.OpenMonPrinter(OpenType.TYPE_PRINTER, MyPrinterSettings.PrinterName);
                    if (MyErrorCode == ErrorCode.SUCCESS)
                    {
                        // Init default pre-print values
                        blnCancelError = false;

                        mreDrawerClosed = new ManualResetEvent(false);
                        mreDrawerOpened = new ManualResetEvent(false);

                        MyErrorCode = MyAPI.SetStatusBack();
                        if (MyErrorCode == ErrorCode.SUCCESS)
                        {
                            // Recover printer if a recoverable error appears
                            if (blnCancelError)
                                MyAPI.CancelError();

                            OpenCashDrawer();

                            // Cleanup
                            MyAPI.CancelStatusBack();
                            MyErrorCode = MyAPI.CloseMonPrinter();
                            if (MyErrorCode != ErrorCode.SUCCESS)
                                throw new ThermalPrinterException(string.Format("Couldn't close the printer (ErrorCode: {0})", MyErrorCode));
                        }
                        else
                            throw new ThermalPrinterException(string.Format("Couldn't set status-callback (ErrorCode: {0})", MyErrorCode));
                    }
                    else
                        throw new ThermalPrinterException(string.Format("Couldn't open the printer (ErrorCode: {0})", MyErrorCode));
                }
            }
        }

        private void MyPrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            GenericEventArgs<Container<PrintPageEventArgs, object>> MyArgs =
                new GenericEventArgs<Container<PrintPageEventArgs, object>>(new Container<PrintPageEventArgs, object>());

            EventHandler<GenericEventArgs<Container<PrintPageEventArgs, object>>> evtHandler = PrintPage;
            if (!blnPrint)
            {
                evtHandler = delegate (object sender2, GenericEventArgs<Container<PrintPageEventArgs, object>> e2)
                {
                };
            }
            MyArgs.Data.Value1 = e;
            MyArgs.Data.Value2 = objData;
            EventHelper.Fire<GenericEventArgs<Container<PrintPageEventArgs, object>>>(this, evtHandler, MyArgs);
        }

        private void MyAPI_StatusCallback(ASB status)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now.ToFullString(), status.ToString());

            if (GetStatus(status, ASB.ASB_PRINT_SUCCESS))
                mrePrintTimeout.Set();
            if (GetStatusOr(status, ASB.ASB_NO_RESPONSE, ASB.ASB_UNRECOVER_ERR))
            {
                //blnPrintFinish = true;
                blnCancelError = true;
            }
            else
            {
                if (!GetStatus(status, ASB.ASB_PRESENTER_COVER) && !blnDrawerOpen && blnOpenDrawer)
                {
                    HandleDrawerOpened();
                }
                else if (GetStatus(status, ASB.ASB_PRESENTER_COVER) && blnDrawerOpen && blnOpenDrawer)
                {
                    HandleDrawerClosed();
                }

                if (GetStatus(status, ASB.ASB_COVER_OPEN) && !blnCoverOpen)
                {
                    EventHelper.Fire(this, CoverOpened, EventArgs.Empty, MyAsyncOperation);
                    blnCoverOpen = true;
                    EventHelper.Fire<GenericEventArgs<bool>>(this, CoverStateChanged, new GenericEventArgs<bool>(blnCoverOpen), MyAsyncOperation);
                    mreCoverOpened.Set();
                }
                else if (!GetStatus(status, ASB.ASB_COVER_OPEN) && blnCoverOpen)
                {
                    EventHelper.Fire(this, CoverClosed, EventArgs.Empty, MyAsyncOperation);
                    blnCoverOpen = false;
                    EventHelper.Fire<GenericEventArgs<bool>>(this, CoverStateChanged, new GenericEventArgs<bool>(blnCoverOpen), MyAsyncOperation);
                    mreCoverClosed.Set();
                }

                if (GetStatus(status, ASB.ASB_PAPER_END) ||
                    GetStatus(status, ASB.ASB_RECEIPT_END))
                {
                    if (!CoverOpen/* && !blnPaperOutExchange*/)
                    {
                        new Thread(new ThreadStart(delegate ()
                        {
                            blnPaperOutExchange = true;
                            ASB MyASB;
                            do
                            {
                                mreCoverOpened.Reset();
                                mreCoverClosed.Reset();
                                EventHelper.Fire(this, PaperOut, EventArgs.Empty, MyAsyncOperation);
                                mreCoverOpened.WaitOne();
                                mreCoverClosed.WaitOne();
                                MyAPI.GetRealStatus(out MyASB);
                            }
                            while (GetStatus(MyASB, ASB.ASB_PAPER_END) ||
                                GetStatus(MyASB, ASB.ASB_RECEIPT_END));
                            blnPaperOutExchange = false;
                        })).Start();
                    }
                }
            }
        }

        private void HandleDrawerClosed()
        {
            if (blnOpenDrawer)
            {
                LogToFile("Closed Drawer");
                EventHelper.Fire(this, DrawerClosed, EventArgs.Empty, MyAsyncOperation);
            }
            blnDrawerOpen = false;
            if (blnOpenDrawer)
                EventHelper.Fire<GenericEventArgs<bool>>(this, DrawerStateChanged, new GenericEventArgs<bool>(blnDrawerOpen), MyAsyncOperation);
            mreDrawerClosed.Set();
        }

        private void HandleDrawerOpened()
        {
            if (blnOpenDrawer)
            {
                LogToFile("Opened Drawer");
                EventHelper.Fire(this, DrawerOpened, EventArgs.Empty, MyAsyncOperation);
            }
            blnDrawerOpen = true;
            if (blnOpenDrawer)
                EventHelper.Fire<GenericEventArgs<bool>>(this, DrawerStateChanged, new GenericEventArgs<bool>(blnDrawerOpen), MyAsyncOperation);
            mreDrawerOpened.Set();
        }

        protected static bool GetStatus(ASB MyASBFlags, ASB MyASBFlag)
        {
            return (MyASBFlags & MyASBFlag) == MyASBFlag;
        }

        protected static bool GetStatusOr(ASB MyASBFlags, params ASB[] MyASBFlagss)
        {
            foreach (ASB MyASBFlag in MyASBFlagss)
            {
                if (GetStatus(MyASBFlags, MyASBFlag))
                    return true;
            }
            return false;
        }

        private static void LogToFile(string message)
        {
            if (LogToFileEnabled)
            {
                Console.WriteLine("[{0:hh:mm:ss.fff}] (ThermalPrinter): {1}", DateTime.Now, message);
                //SimpleFileLogger.Unique.Log(LogLevel.Info, "ThermalPrinter", "{0}", message);
            }
        }

        private enum CustomPaperSources : int
        {
            DocumentFeedCut = 257,
            DocumentFeedNoCut = 258,
            DocumentNoFeedCut = 259,
            DocumentNoFeedNoCut = 260,
            PageFeedCut = 513,
            PageFeedNoCut = 514,
            PageNoFeedCut = 515
        }

        #endregion
    }
}