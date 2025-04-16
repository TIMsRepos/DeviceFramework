using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using TIM.Frontend.Common.Data.Entities;
using TIM.Frontend.Common.Data.Factories;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Crypto;
using TIM.Devices.Framework.Common.Extensions;
using TIM.Devices.Framework.Common.Log;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.Communication;
using TIM.Devices.Framework.ElectronicCash.ZVT;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Events;
using TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers;
using TIM.Devices.Framework.UserAccountControl;

namespace TestProject
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppHelper.ForceApplication = Applications.Frontend;

            DbControllersOrganizationsCache.Subsidiaries.GetAll();
            DbControllersOrganizationsCache.Subsidiaries.GetAll();
            //DbControllersCache.ClearCache();
            DbControllersOrganizationsCache.Subsidiaries.GetAllClearCache();

            //var result = ControllerFactory.Functions.fnCalculateFirstProRate(DateTime.Today, 100);
            //MessageBox.Show(result.ToString("c"));

            return;

            //var vend = ControllerFactory.VendingDevices.Get(d => !d.ActivatedFlag).OrderBy(d => d.VendingDeviceID).First();
            //vend.Description = "Test " + DateTime.Now.ToShortTimeString() + " 123456789 123456789 123456789 123456789 123456789 123456789 123456789";
            //vend.CreateDate = DateTime.Today.AddDays(1);
            //vend.CreatePersonID = " FEHLER ";
            //ControllerFactory.VendingDevices.Update(vend, " kb ");

            var log = new VendingLog();
            log.VendingLogID = Guid.NewGuid();
            log.VendingLogType = 1;
            //log.Remark = " 123 ";
            //log.Details = " 456 ";
            log.ComputerName = " test ";
            DbControllers.VendingLogs.Insert(log, "kb1 123456789 123456789 123456789 123456789 123456789 123456789 ");



            //int intDeviceID = ConfigManager.GetAppSetting<int>("DeviceID");

            //var ready = IsReady;

            //ComTransportLayer MyComTransportLayer = new ComTransportLayer(1, 9600);
            ////IPTransportLayer MyIPTransportLayer = new IPTransportLayer(new IPEndPoint(IPAddress.Parse("10.1.50.10"), 22000));
            //SimpleTerminal<PaymentTerminal, ComTransportLayer> MyTerminal = new SimpleTerminal<PaymentTerminal, ComTransportLayer>(MyComTransportLayer);
            //MyTerminal.ExceptionThrown += MyTerminal_ExceptionThrown;
            //MyTerminal.PaymentCompleted += MyTerminal_PaymentCompleted;
            //MyTerminal.Terminal.StateInfo += Terminal_StateInfo;
            //MyTerminal.Terminal.InterimStateInfo += Terminal_InterimStateInfo;

            //MyTerminal.Terminal.Register(
            //            ConfigByte.POSControlsPayment | ConfigByte.POSPrintsPayment,
            //            Currencies.Eur,
            //            SettingsManager.Get<string>("EC_Terminal", "Password", true));
            //MyTerminal.Terminal.LogOff();

            //MyTerminal.Payment(Currencies.Eur, 001, true, PaymentTypes.PT_Decission);


            //Console.WriteLine(TraceHelper.Dump());

            //IDRWA02 MyAccess = new IDRWA02();
            //Access6200 MyAccess = new Access6200();
            //MyAccess.MediaDetected += MyAccess_MediaDetected;
            //MyAccess.Passed += MyAccess_Passed;
            //MyAccess.ConfigString = ConfigString.Parse("Host=10.7.20.12|Port=8000", '=', '|', MyAccess.ConfigStringDescriptions);
            //MyAccess.ConfigString = ConfigString.Parse("COM=1|Baud=9600|TriggerActive=200", '=', '|', MyAccess.ConfigStringDescriptions);
            //MyAccess.Open();
            //Application.Run(new Form1());


            return;

            // --------------------------------- //
            //LogManager.Log(LogLevel.Info, new Exception("DFTestLogger Test"));
            return;

            // --------------------------------- //

            string strTmp = Path.GetTempFileName() + ".png";
            using (Bitmap bmp = new Bitmap(640, 480))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Yellow);
                    RectangleF bounds = new RectangleF(0, 0, 200, 100);
                    g.DrawStrings(new string[] { "Hello", "Hello", "Hello", "Hello" },
                        new Font(FontFamily.GenericSansSerif, 10f), Brushes.Black, bounds, StringAlignment.Center, 5);
                    g.DrawRectangle(Pens.Red, Rectangle.Round(bounds));
                }
                bmp.Save(strTmp, ImageFormat.Png);
            }
            Process.Start(strTmp);
            return;

            // --------------------------------- //

            UACEntry entry = UACEntry.GetGroup("TIM-Administrator", "test");
            entry = UACEntry.GetGroup("Test-Administrator", "MERIDIANSPA_NET");
            entry = UACEntry.GetGroup("TIM-Administrator", "MERIDIANSPA_NET");
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("IsMinWin6: {0}", WindowsTools.IsMinWin6);
            Console.WriteLine("IsElevated: {0}", WindowsTools.IsElevated);
            Console.WriteLine("ElevationType: {0}", WindowsTools.GetElevationType);
            Console.WriteLine("CurrentUserIsAdmin: {0}", WindowsTools.IsCurrentUserWinAdministrator);
            Console.WriteLine();

            Console.WriteLine("ADName: " + UACHelper.ActiveDirectoryName);
            Console.WriteLine("Location: " + UACHelper.CurrentEntryLocation);
            Console.WriteLine();

            Console.WriteLine("MatchGroupOfServiceEmployeeWithUAC: " + UACHelper.MatchGroupOfServiceEmployeeWithUAC(446, "TEST"));
            Console.WriteLine("MatchGroupOfServiceEmployeeWithUAC: " + UACHelper.MatchGroupOfServiceEmployeeWithUAC("karsten", "TEST"));
            Console.WriteLine("Groups of karsten:");
            var groups = UACHelper.MatchingServiceEmployeeGroupIDsOfEmployee("karsten");
            foreach (var i in groups)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            var user = UACEntry.GetUser("karsten");
            Console.WriteLine("Get User: " + user.Name + " / " + user.DisplayName);
            var group1 = UACEntry.GetGroup("ServiceEmployeeGroupID_11");
            Console.WriteLine("Get Group: " + group1.Name + " / " + group1.DisplayName);
            var group2 = UACEntry.GetGroup("Beauty");
            Console.WriteLine("Get Group: " + group2.Name + " / " + group2.DisplayName);
            Console.WriteLine();

            var newGroup = UACEntry.AddGroup("Massage");
            Console.WriteLine("Add group: " + newGroup.Name);
            newGroup = UACEntry.AddGroup("test");
            Console.WriteLine("Gruppe test existiert nicht: " + (newGroup == null).ToString());
            Console.WriteLine();

            Console.WriteLine("Get all groups:");
            var groups2 = UACEntry.GetAllGroups();
            foreach (UACEntry uacEntry in groups2)
            {
                //if (uacEntry.Name.StartsWith("TIM-"))
                Console.WriteLine(uacEntry.Name + " / " + uacEntry.DisplayName + " / " + uacEntry.CN);
            }
            Console.WriteLine();

            Console.WriteLine("Get all users:");
            var users2 = UACEntry.GetAllUsers();
            foreach (UACEntry user2 in users2)
            {
                Console.WriteLine(user2.Name + " / " + user2.DisplayName + " / " + user2.CN);
            }
            Console.WriteLine();

            Console.WriteLine("GetEntryAndSubGroups 'Beauty': " + UACEntry.GetEntryAndSubGroups("Beauty").Count);
            Console.WriteLine("GetEntryAndSubGroups 'karsten': " + UACEntry.GetEntryAndSubGroups("karsten").Count);
            Console.WriteLine("GetDirectoryEntry: " + UACEntry.GetUsers("Böhler").Count);
            var testuser = UACEntry.GetUser("TimTest");
            Console.WriteLine("GetEntryName: " + UACEntry.GetEntryName("Massage"));
            Console.WriteLine("GetEntryName: " + UACEntry.GetEntryName("ServiceEmployeeGroupID_11"));
            Console.WriteLine("GetEntryName: " + UACEntry.GetEntryName(testuser.Id));
            Console.WriteLine("GetFullName: " + UACEntry.GetFullName(testuser.Name));
            Console.WriteLine("CheckUserNameAndPassword test: " + UACEntry.CheckUserNameAndPassword(testuser.Name, "test"));
            Console.WriteLine("CheckUserNameAndPassword Meridianspa!: " + UACEntry.CheckUserNameAndPassword(testuser.Name, "Meridianspa!"));
            Console.WriteLine();

            newGroup = UACEntry.GetGroup("Beauty");
            Console.WriteLine("RenameGroup " + newGroup.Name);
            newGroup.RenameGroup("TIM-" + DateTime.Now.ToShortTimeString());
            newGroup = UACEntry.GetGroup(newGroup.Name);
            Console.WriteLine("RenameGroup " + newGroup.Name);
            Console.WriteLine();

            Console.WriteLine("User {0} is in group {1}: {2}", testuser.Name, group1.Name, UACEntry.GetEntryAndSubGroups(testuser.CN).Count);
            Console.WriteLine("Add user");
            testuser.AddUserToGroup(group1);
            Console.WriteLine("User {0} is in group {1}: {2}", testuser.Name, group1.Name, UACEntry.GetEntryAndSubGroups(testuser.CN).Count);
            Console.WriteLine("Remove user");
            testuser.RemoveUserFromGroup(group1);
            Console.WriteLine("User {0} is in group {1}: {2}", testuser.Name, group1.Name, UACEntry.GetEntryAndSubGroups(testuser.CN).Count);
            Console.ReadKey();
        }

        public static bool IsReady
        {
            get
            {
                const string strSetting = "EC_Terminal";
                string[] strSettingDetailsIP = { "Host", "Port", "Password" };
                string[] strSettingDetailsCOM = { "COM", "Baud", "Password" };

                if (SettingsManager.ExistsAll(strSetting, strSettingDetailsIP, true) &&
                        !SettingsManager.EmptyAny(strSetting, strSettingDetailsIP, true))
                {
                    return SimpleTerminal<PaymentTerminal, IPTransportLayer>.IsReady();
                }

                if (SettingsManager.ExistsAll(strSetting, strSettingDetailsCOM, true) &&
                        !SettingsManager.EmptyAny(strSetting, strSettingDetailsCOM, true))
                {
                    return SimpleTerminal<PaymentTerminal, ComTransportLayer>.IsReady();
                }

                return false;
            }
        }

        static void MyAccess_Passed(object sender, TIM.Devices.Framework.Turnstiles.TurnstileEventArgs e)
        {
            Console.WriteLine("{1} Durch Drehkreuz gegangen #: {0}", e.Payload[Payloads.MediaID], DateTime.Now);
        }

        static void Terminal_InterimStateInfo(object sender, InterimStateInfoEventArgs e)
        {
            Console.WriteLine("[{0}] InterimStateInfo => {1}", DateTime.Now, e.Result);
        }

        static void Terminal_StateInfo(object sender, StateInfoEventArgs e)
        {
            Console.WriteLine("[{0}] StateInfo => {1}", DateTime.Now, e.Result);
        }

        static void MyTerminal_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            ((SimpleTerminal<PaymentTerminal, ComTransportLayer>)sender).Dispose();

            string tmp = "";
        }

        static void MyTerminal_PaymentCompleted(object sender, PostStateInfoEventArgs e)
        {
            string tmp = "";
        }

        static void MyAccess_MediaDetected(object sender, DeviceMediaDetectedEventArgs2 e)
        {
            Console.WriteLine("{1} TestProject: #: {0}", e.Payload[Payloads.MediaID], DateTime.Now);

            //if (e.Payload[Payloads.MediaID].Equals("144057792"))
            e.Device.Trigger(TimeSpan.FromMilliseconds(1));
            //Console.WriteLine("[{0}] Triggered", DateTime.Now);
            //else
            //    e.Device.Trigger(TimeSpan.FromMilliseconds(0));
        }

        static void tp_Print(object sender, GenericCancelableEventArgs<object> e)
        {
            object data = e.Data;

            if (false)
                e.Cancel = true;

            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            object objFileName = @"C:\Users\chunke.MERIDIANSPA_NET\Documents\Test.docx";
            object objTrue = true;
            object objFalse = false;
            object objMissing = Type.Missing;
            Microsoft.Office.Interop.Word.Document doc = app.Documents.Open(ref objFileName, ref objMissing, ref objMissing,
                ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing,
                ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing);
            SetPrinter(ref app);
            doc.PrintOut(ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing,
                ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing,
                ref objMissing, ref objMissing, ref objMissing, ref objMissing, ref objMissing);
            doc.Close(ref objFalse, ref objMissing, ref objMissing);
        }

        static void tp_Print(object sender, GenericEventArgs<Container<PrintPageEventArgs, object>> e)
        {
            Graphics g = e.Data.Value1.Graphics;
            Rectangle rect = new Rectangle(10, 10, 250, 150);
            g.FillEllipse(Brushes.LightGray, rect);
            Pen pen = new Pen(Brushes.Black, 3);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
            g.DrawEllipse(pen, rect);
            g.DrawString("Hello World!", new Font(FontFamily.GenericSansSerif, 24, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline), Brushes.Gray, 40, 60);
            e.Data.Value1.HasMorePages = false;
        }

        static void Unique_ReleasingFailed(object sender, TIM.Devices.Framework.Common.Temp.ReleasingFailedEventArgs e)
        {
            string s = "";
        }

        static void Unique_GenerateFileName(object sender, TIM.Devices.Framework.Common.Temp.GenerateFileNameEventArgs e)
        {
            string s = "";
        }

        [STAThread]
        static void Main2(string[] args)
        {
            AppHelper.ForceApplication = Applications.Administrator;

            return;

            /*for (int i = 0; i < 50; ++i)
            {
                ThermalPrinter MyThermalPrinter = new ThermalPrinter();
                MyThermalPrinter.Execute(true, null, PrinterType.ReceiptPrinter);
                Thread.Sleep(500);
            }*/

            FileDialogFilter fdf = new FileDialogFilter();
            fdf.Add("PNG Bilddateien", "png");
            fdf.Add("JPEG Bilddateien", "jpg", "jpeg");
            string filter = fdf.ToString();
            return;

            using (MemoryStream memStream = new MemoryStream())
            {
                DictionaryEx<string, string> dic = new DictionaryEx<string, string>();
                for (int i = 0; i < 5; ++i)
                    dic.Add("test" + i.ToString(), "tested" + i.ToString());
                new DataContractJsonSerializer(typeof(DictionaryEx<string, string>)).WriteObject(memStream, dic);
                string str = Encoding.Default.GetString(memStream.GetBuffer());
            }
            return;

            #region WebSafe AES
            string strPW = "t9$O,^Y@iZ=OxI7;K9D";
            string strID = "190000355";

            byte[] bytID = Encoding.UTF8.GetBytes(strID);
            // --- Data border
            byte[] bytEncrypted = CryptoHelper.Encrypt<AesManaged>(bytID, strPW);
            string strUrlEncrypted = HttpUtility.UrlEncode(Convert.ToBase64String(bytEncrypted));
            // --- URL Transport border
            byte[] bytUrlEncrypted = Convert.FromBase64String(HttpUtility.UrlDecode(strUrlEncrypted));
            byte[] bytDecrypted = CryptoHelper.Decrypt<AesManaged>(bytUrlEncrypted, strPW);
            // --- Data border
            string strID2 = Encoding.UTF8.GetString(bytDecrypted);
            #endregion
            return;

            #region QueryString slit
            string strQueryStringInput = "?id=0+ci1xzA5NJZpQAhjj6ygw==";
            NameValueCollection MyColl = new NameValueCollection();

            string strQueryStringRaw = strQueryStringInput;
            string strQueryString = strQueryStringRaw.Replace("?", "");
            string[] strPairs = strQueryString.Split(new char[] { '&' });
            foreach (string strPair in strPairs)
            {
                string[] strParts = strPair.Split(new char[] { '=' });
                if (strParts.Length == 2)
                {
                    MyColl.Add(strParts[0], strParts[1]);
                }
            }
            #endregion
            string strQueryStringID = MyColl["id"];
            return;

            /*IDRWA02 MyFeig = new IDRWA02();
            MyFeig.SelfConfig();
            MyFeig.Open();
            //for (int i = 0; i < 5; ++i)
            //    MyFeig.TriggerOrToggle();
            Thread.Sleep(30000);
            MyFeig.Close();*/

            /*Rectangle rectA = new Rectangle(10, 10, 600, 400);
            Rectangle rectB = new Rectangle(10, 10, 500, 200);*/
            /*Rectangle rectA = new Rectangle(10, 10, 400, 600);
            Rectangle rectB = new Rectangle(10, 10, 200, 500);*/
            /*Rectangle rectA = new Rectangle(10, 10, 600, 400);
            Rectangle rectB = new Rectangle(10, 10, 200, 500);*/
            Rectangle rectA = new Rectangle(10, 10, 400, 600);
            Rectangle rectB = new Rectangle(10, 10, 500, 200);
            Rectangle rectC = rectA.CalcProportionalScaledRect(rectB);
            Rectangle rectD = rectA.CenterRectangle(rectC);

            /*MethodBase method = MethodInfo.GetCurrentMethod();
            Console.WriteLine(method.GetFullName());*/
            Console.WriteLine(SimpleTerminal<PaymentTerminal, ComTransportLayer>.IsReady());
            Console.Read();
            //Test040FTripleDLE();

            return;

            /*ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_TCPIPPrinterPort");
            foreach (ManagementObject obj in searcher.Get())
                Console.WriteLine("Found!");

            Console.ReadLine();*/

            /*while (true)
            {
                string strPrinterName = PrinterHelper.GetPrinterName(PrinterType.CounterReceiptPrinter);
                Console.WriteLine("{0}: {1}", strPrinterName, PrinterHelper.GetStatus(strPrinterName));
                Thread.Sleep(500);
            }*/

            //string strPad = "1234".PadLeft(15, '0');

            #region Test1
            /*LogManager.Logger.Log(LogLevel.Warning, new Exception("Test"));



            KeyValuePair<int, ConfigString>[] cfgs = TurnstileHelper.GetConfigStrings().ToArray();
            //DirectoryInfo dirApp = new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath));

            Application.Run(new Form2());*/

            //ConfigString MyConfigString = new ConfigString('=', '|', ConfigStringDescriptions);
            //MyConfigString["COM"] = (11).ToString();
            //MyConfigString["BAUD"] = (9600).ToString();
            //MyConfigString["TRIGGERACTIVE"] = (250).ToString();

            //for (int i = 0; i < 100; ++i)
            //{
            //    IDevice MyDev = DeviceManager.GetDeviceByDeviceDetailID(7, MyConfigString);
            //    MyDev.TriggerOrToggle();
            //    Thread.Sleep(5000);
            //    MyDev.Close();
            //}

            /*IDevice dev = new IDRWA02();
            dev.Open();
            Console.WriteLine(dev.Name);
            //dev.MediaSearchEnabled = true;
            //Thread.Sleep(60000);
            /*for (int i = 0; i < 10; ++i)
            {
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    Console.WriteLine(dev.GetMediaID());
                }
                catch (MissingMediaException)
                {
                }
                Console.WriteLine(sw.ElapsedMilliseconds);
                Thread.Sleep(500);
            }*/
            /*((IDRWA02)dev).Trigger(TimeSpan.FromMilliseconds(500));
            Thread.Sleep(3000);
            dev.Close();*/

            /*SimpleComTerminal terminal = new SimpleComTerminal();
            terminal.Register();*/

            //ITerminal terminal = null;
            //try
            //{
            //    using (ITransportLayer transportLayer = new ComTransportLayer())
            //    {
            //        terminal = new ComTerminal(transportLayer);
            //        terminal.StateInfo += delegate(object sender, StateInfoEventArgs evtArgs) { Console.WriteLine("{0} => {1}", evtArgs.GetType().Name, evtArgs.Result); };
            //        terminal.Register(
            //            ConfigByte.POSControlsPayment,
            //            Currencies.Eur, SettingsManager.Get<string>("EC_Terminal", "Password", true));
            //        //terminal.Register(ConfigByte.None, Currencies.Eur, "123456");
            //        //terminal.Register(ConfigByte.POSControlsPayment, Currencies.Eur, "123456");                    
            //        //terminal.DisplayText(new string[] { "TIM's", "EC-Terminal" });
            //        terminal.Authenticate(1, Currencies.Eur, TIM.Devices.Framework.ElectronicCash.ZVT.PaymentTypes.PT_Decission);
            //        /*for (int i = 0; i < 100; ++i)
            //            Console.WriteLine("0x{0:X2}", ((ComTransportLayer)transportLayer).ReadByte());*/
            //        /*terminal.Register(
            //            ConfigByte.POSControlsPayment | ConfigByte.POSPrintsPayment | ConfigByte.ECRRequiresIntermediateStatusInfo,
            //            Currencies.Eur, SettingsManager.Get<string>("EC_Terminal", "Password", true));*/
            //        //terminal.LogOff();
            //        Console.WriteLine(TraceHelper.Dump());
            //    }
            //}
            //catch (TerminalException ex)
            //{
            //    //Console.WriteLine(TraceHelper.Dump());
            //    /*Console.WriteLine();
            //    Console.WriteLine(ex.Message);*/
            //}
            //finally
            //{
            //    /*if (terminal != null)
            //        terminal.LogOff();*/
            //}

            //for (int i = 0; i < 3; ++i)
            //    Console.WriteLine("-----------------------------------------");

            //Console.WriteLine(TraceHelper.Dump());
            #endregion

            #region BCDCodeHelper Test - 0x40 0x43
            //short receiptNumber = BCDCodeHelper.Bcd2Dec<short>(new byte[] { 0x40, 0x43 }, 0, 2);
            #endregion

            #region BCDCodeHelper Test - Amount Error - PostStateInfoEventArgs
            PostStateInfoEventArgs psiEvtArgs = new PostStateInfoEventArgs(new byte[] {
                0x27, 0x00, 
                0x04, 0x00, 0x00, 0x00, 0x00, 0x49, 0x00, 
                0x0B, 0x00, 0x82, 0x61, 
                0x0C, 0x21, 0x42, 0x44, 
                0x0D, 0x05, 0x11, 
                0x0E, 0x14, 0x12, 
                0x17, 0x00, 0x04, 
                0x19, 0x70, 
                0x22, 0xF1, 0xF0, 0x67, 0x25, 0x40, 0x01, 0x01, 0x32, 0x04, 0x49, 0x26, 0x5F, 
                0x29, 0x60, 0x60, 0x30, 0x79, 
                0x49, 0x09, 0x78, 
                0x87, 0x40, 0x87, 
                0x8A, 0x05,
                0x8C, 0x00, 
                0x2A, 0x34, 0x35, 0x35, 0x36, 0x30, 0x30, 0x35, 0x32, 0x39, 0x34, 0x31, 0x34, 0x20, 0x20, 0x20, 
                0x8B, 0xF0, 0xF9, 0x67, 0x69, 0x72, 0x6F, 0x63, 0x61, 0x72, 0x64, 0x00
            });
            #endregion

            TraceHelper.DirectDumpEnabled = true;

            /*SimpleTerminal<ComTerminal, ComTransportLayer> MyTerminal = null;
            try
            {
                MyTerminal = new SimpleTerminal<ComTerminal, ComTransportLayer>();

                MyTerminal.PaymentCompleted += new EventHandler<PostStateInfoEventArgs>(delegate(object sender, PostStateInfoEventArgs e)
                    {
                        ObjDump(e);
                    });
                MyTerminal.Payment(Currencies.Eur, 1, true, TIM.Devices.Framework.ElectronicCash.ZVT.PaymentTypes.PT_Decission);
            }
            catch (TerminalNotAvailableException ex)
            {
                MessageBox.Show("Terminal ist nicht verfügbar/angeschlossen");
            }
            catch (TerminalCanceledException ex)
            {
                MessageBox.Show("Die Transaktion wurde abgebrochen");
            }
            // LOG
            catch (TerminalException ex)
            {
                MessageBox.Show("Es ist ein Fehler beim Terminal aufgetreten");
            }
            // LOG
            catch (TransportLayerException ex)
            {
                MessageBox.Show("Es ist ein Fehler bei der Kommunikation mit dem Terminal aufgetreten");
            }
            // LOG
            catch (ZVTException ex)
            {
                MessageBox.Show("Es ist ein Fehler aufgetreten");
            }
            finally
            {
                if (MyTerminal != null)
                    MyTerminal.Dispose();
            }*/












            // ResetEvent for waiting for payment
            ManualResetEvent mre = new ManualResetEvent(false);

            // Init the terminal
            SimpleTerminal<PaymentTerminal, ComTransportLayer> MyTerminal = null;
            MyTerminal = new SimpleTerminal<PaymentTerminal, ComTransportLayer>();

            // Hook up to events
            MyTerminal.ExceptionThrown += new EventHandler<ExceptionEventArgs>(delegate(object sender, ExceptionEventArgs e)
                {
                    Type MyExType = e.Exception.GetType();

                    if (MyExType == typeof(TerminalNotAvailableException))
                    {
                        // Terminal is not available, wrong config, turned off etc.
                    }
                    else if (MyExType == typeof(TerminalCanceledException))
                    {
                        // Action was canceled before transaction was started
                    }
                    else if (MyExType == typeof(TerminalException) ||
                        MyExType == typeof(TransportLayerException) ||
                        MyExType == typeof(ZVTException))
                    {
                        // Errors to log!
                    }

                    mre.Set();
                });
            MyTerminal.PaymentCompleted += new EventHandler<PostStateInfoEventArgs>(delegate(object sender, PostStateInfoEventArgs e)
                {
                    Console.WriteLine(e.Result);
                    mre.Set();
                });

            // Start payment
            MyTerminal.Payment(Currencies.Eur, 1, true, TIM.Devices.Framework.ElectronicCash.ZVT.PaymentTypes.PT_Decission);

            // Wait for payment
            while (!mre.WaitOne(250))
                Application.DoEvents();

            // Clean up
            MyTerminal.Dispose();

            Console.WriteLine("Finish");















            Console.ReadLine();

            return;

            #region Test2
            //Console.WriteLine(DBContext.GetConfiguration(Applications.Frontend).FilePath);

            //Console.WriteLine(string.Join(", ", ));
            //string[] strConn = DBContext.GetConnectionStrings(DBContext.GetDefaultConfiguration());

            // bool blnWordDisplayAlways = TIM.Devices.Framework.Common.Settings.SettingsManager.GetOrPredefined<bool>(enmComputerSetting.Word, enmComputerDetailSetting.Word_DisplayAlways, false);

            //string strPrinterName = PrinterHelper.GetPrinterNameWithFallback(PrinterType.Receipt, PrinterType.SystemDefault);

            //DateTimeRange dtRange = new DateTimeRange(DateTime.Now.Date, DateTime.Now.Date.AddWeeks(1).Subtract(TimeSpan.FromTicks(1)));
            //DateTimeRange dtRange = new DateTimeRange(new DateTime(2009, 06, 06), new DateTime(2009, 07, 13));
            //Console.WriteLine(dtRange.ToString());
            //Console.WriteLine(DateTime.Today.NextDayOfWeek(DayOfWeek.Friday));
            //Console.WriteLine(DateTime.Today.PreviousDayOfWeek(DayOfWeek.Monday));
            /*foreach (DateTime dtDate in dtRange.Days)
                Console.WriteLine(dtDate);
            DateTimeRange dtRange = new DateTimeRange(new DateTime(2009, 02, 01), new DateTime(2009, 08, 05));
            foreach (DateTime dtDate in dtRange.Months)
                Console.WriteLine(dtDate);*/

            //string strExe = Application.ExecutablePath;
            //string strExeFileName = Path.GetFileName(strExe);
            //byte[] vals = (from v in new byte[] { }
            //               select v).ToArray();

            //Stopwatch sw = Stopwatch.StartNew();
            //string strSummary = LogManager.GetFormattedSummary(new Exception("Test1"), new Exception("Test2"), LogLevel.Error);
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            //PrinterHelper.PrintText(
            //    strSummary, 
            //    new Font(FontFamily.GenericMonospace, 10f), 
            //    PrinterHelper.GetPrinterName(PrinterType.SystemDefault));

            /*foreach (Screen MyScreen in Screen.AllScreens)
            {
                Bitmap bmp = new Bitmap(MyScreen.Bounds.Width, MyScreen.Bounds.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(MyScreen.Bounds.Location, Point.Empty, bmp.Size, CopyPixelOperation.SourceCopy);
                }
                string tmpBmp = Path.GetTempFileName() + ".png";
                bmp.Save(tmpBmp);
                Process.Start(tmpBmp);
            }*/

            /*DateTimeRange dtRange = DateTimeRange.GetWeek();
            DayOfWeek[] MyDaysOfWeek = new DayOfWeek[] {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            DateTime[] dtDays = dtRange.Days.ToArray();
            DateTime[] dtDaysOrdered = new DateTime[dtDays.Length];
            for (int i = 0; i < dtDays.Length; ++i)
                dtDaysOrdered[i] = dtDays.Where(d => d.DayOfWeek == MyDaysOfWeek[i]).Single();*/


            //string strUsername = TIM.Devices.Framework.Common.Settings.SettingsManager.Get<string>("CourseSchedules", "Username");
            //string strPassword = TIM.Devices.Framework.Common.Settings.SettingsManager.Get<string>("CourseSchedules", "Password");
            //string strHost = TIM.Devices.Framework.Common.Settings.SettingsManager.Get<string>("CourseSchedules", "Host");
            //byte[] data = FTPHelper.Download(new Uri("ftp://" + strHost + "/de/hamburg_wandsbek/kursplantest/CourseSchedules_Wandsbek.png"),
            //    new NetworkCredential(strUsername, strPassword));

            Application.Run(new Form1());

            return;
            int tmp = 0;
            /*Console.WriteLine(PrinterHelper.GetSystemDefaultPrinterName());
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                Console.WriteLine(PrinterHelper.IsBonPrinter(printer));
            }
            Console.Read();*/

            //Application.Run(new Form1());

            /*while (true)
            {
                Console.WriteLine(PrinterHelper.GetStatus(RegistryManager.GetBonPrinterName()));
                Thread.Sleep(250);
            }*/

            //ThermalPrinter tp = new ThermalPrinter();
            //tp.PrintPage += new EventHandler<GenericEventArgs<Container<PrintPageEventArgs, object>>>(tp_Print); // <==
            //tp.CoverClosed += new EventHandler(tp_CoverClosed);
            //tp.CoverOpened += new EventHandler(tp_CoverOpened);
            //tp.CoverStateChanged += new EventHandler<GenericEventArgs<bool>>(tp_CoverStateChanged);
            //tp.DrawerClosed += new EventHandler(tp_DrawerClosed);
            //tp.DrawerOpened += new EventHandler(tp_DrawerOpened);
            //tp.DrawerOpenedTooLong += new EventHandler(tp_DrawerOpenedTooLong); // <==
            //tp.DrawerStateChanged += new EventHandler<GenericEventArgs<bool>>(tp_DrawerStateChanged);
            //tp.PaperOut += new EventHandler(tp_PaperOut); // <==
            //tp.ExecuteFinished += new EventHandler(tp_ExecuteFinished);
            ////tp.ExecuteStarted += new EventHandler(tp_ExecuteStarted);
            //tp.PrintFinshed += new EventHandler(tp_PrintFinshed);
            //tp.PrintStarted += new EventHandler(tp_PrintStarted);
            //try
            //{
            //    tp.Execute(true, null, false);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ERROR");
            //}

            //Console.Read();






            /*AutoResetEvent
            ManualResetEvent*/

            //PCR300M dev = new PCR300M();
            //dev.MediaFound += delegate(object sender, DeviceEventArgs e)
            //{
            //    Console.WriteLine(e.Device.GetMediaID());
            //};
            //dev.Open();
            //dev.MediaSearchEnabled = true;

            //Form frm = new Form();
            //frm.Shown += delegate(object sender, EventArgs e)
            //{
            //    //dev.Request();
            //};
            //Application.Run(frm);
            /*IDevice dev = new GriauleFingerprintReader.FingerprintReader();
            dev.MediaFound += delegate(object sender, DeviceEventArgs e)
            {
                Console.WriteLine(e.Device.GetMediaID());
            };
            dev.Open();*/

            //int tmp = 0;
            /*IDevice dev = new FunWriter5200();
            dev.Open();
            dev.MediaSearchEnabled = true;
            dev.MediaFound += new EventHandler<DeviceEventArgs>(dev_MediaFound);
            dev.MediaRemoved += new EventHandler<DeviceEventArgs>(dev_MediaRemoved);
            Thread.Sleep(30000);
            dev.Close();*/

            //DeviceClient devClient = new DeviceClient(null, TIM.Devices.Framework.ConfigDevices.Employees, true);
            //devClient.MediaDetected += new EventHandler<TIM.Devices.Framework.TIMsDevices.MediaDetectedEventArgs>(devClient_MediaDetected);
            //devClient.MediaRemoved += new EventHandler<TIM.Devices.Framework.TIMsDevices.MediaRemovedEventArgs>(devClient_MediaRemoved);

            /*DeviceClient client1 = new DeviceClient(null, TIM.Devices.Framework.ConfigDevices.Employees, true);
            client1.MediaDetected += new EventHandler<TIM.Devices.Framework.TIMsDevices.MediaDetectedEventArgs>(client1_MediaDetected);
            client1.MediaRemoved += new EventHandler<TIM.Devices.Framework.TIMsDevices.MediaRemovedEventArgs>(client1_MediaRemoved);
            client1.MissingMedia += new EventHandler<MissingMediaEventArgs>(client1_MissingMedia);
            client1.ScopeCheckDelegate = new Func<bool>(delegate() { return true; });*/

            /*MD5 md5 = MD5.Create();
            md5.Initialize();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes("123;123456789;ABCDEFG;ABCDEFG;ABCDEFG"));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            string strHash = sBuilder.ToString();

            IDevice devDS6707 = new DS6707();
            devDS6707.MediaFound += new EventHandler<DeviceEventArgs>(devDS6707_MediaFound);
            devDS6707.Open();
            devDS6707.MediaSearchEnabled = true;*/

            //IDevice dev = new PCR300M();

            //IDevice dev = new FunWriter5250();
            //dev.Open();

            //Thread.Sleep(600000);
            #endregion
        }

        private static void Test040FTripleDLE()
        {
            MemoryStream memStream040F = new MemoryStream(new byte[] {
                0x27, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x62, 0x00, 0x0B, 
                0x00, 0x94, 0x36, 0x0C, 0x15, 0x35, 0x59, 0x0D, 0x07, 0x19, 
                0x0E, 0x13, 0x12, 0x17, 0x00, 0x01, 0x19, 0x70, 0x22, 0xF1, 
                0xF0, 0x67, 0x29, 0x20, 0x47, 0x00, 0x02, 0x60, 0x10, 0x10, 
                0x10, 0x10, 0x5F, 0x29, 0x60, 0x60, 0x28, 0x57, 0x3B, 0x33, 
                0x39, 0x35, 0x36, 0x39, 0x38, 0x00, 0x00, 0x49, 0x09, 0x78, 
                0x87, 0x44, 0x64, 0x8A, 0x05, 0x8C, 0x00, 0xBA, 0x01, 0x00, 
                0x00, 0x00, 0x02, 0x2A, 0x34, 0x35, 0x35, 0x36, 0x30, 0x30, 
                0x35, 0x32, 0x39, 0x34, 0x31, 0x34, 0x20, 0x20, 0x20, 0x8B, 
                0xF0, 0xF9, 0x67, 0x69, 0x72, 0x6F, 0x63, 0x61, 0x72, 0x64, 
                0x00
            });

            /*byte[] bytData = new byte[0x63];
            byte bytLastData = 0;
            for (int i = 0; i < bytData.Length; ++i)
            {
                bytData[i] = (byte)memStream040F.ReadByte(); // DATA

                // skip double DLEs
                if (bytLastData == (byte)SpecialChars.DataLineEscape &&
                    bytData[i] == (byte)SpecialChars.DataLineEscape)
                    --i;
                bytLastData = bytData[i];
            }*/

            byte[] bytData = new byte[0x63];
            byte bytLastData = 0;
            for (int i = 0; i < bytData.Length; ++i)
            {
                bytData[i] = (byte)memStream040F.ReadByte(); // DATA

                // skip double DLEs
                if (bytLastData == (byte)SpecialChars.DataLineEscape &&
                    bytData[i] == (byte)SpecialChars.DataLineEscape)
                {
                    --i;
                    // Force unknown last data
                    // Without: [ 0x10, 0x10 ]             => [ 0x10 ]
                    // Without: [ 0x10, 0x10, 0x10 ]       => [ 0x10 ]
                    // Without: [ 0x10, 0x10, 0x10, 0x10 ] => [ 0x10 ]
                    // With:    [ 0x10, 0x10, 0x10, 0x10 ] => [ 0x10, 0x10 ]
                    bytLastData = 0;
                }
                else
                    bytLastData = bytData[i];
            }
        }

        private static void ObjDump(PostStateInfoEventArgs e)
        {
            Type type = typeof(PostStateInfoEventArgs);
            PropertyInfo[] props = type.GetProperties();

            foreach (PropertyInfo prop in props)
                Console.WriteLine("{0} => '{1}'", prop.Name, prop.GetValue(e, null));
        }

        static void tp_PrintStarted(object sender, EventArgs e)
        {
            Console.WriteLine("PRINT STARTED");
        }

        static void tp_PrintFinshed(object sender, EventArgs e)
        {
            Console.WriteLine("PRINT FINISHED");
        }

        static void tp_ExecuteStarted(object sender, EventArgs e)
        {
            Console.WriteLine("EXECUTE STARTED");
        }

        static void tp_ExecuteFinished(object sender, EventArgs e)
        {
            Console.WriteLine("EXECUTE FINISHED");
        }

        static void tp_PaperOut(object sender, EventArgs e)
        {
            Console.WriteLine("PAPER OUT");
            //MessageBox.Show("PAPER OUT");
        }

        static void tp_DrawerStateChanged(object sender, GenericEventArgs<bool> e)
        {
            Console.WriteLine("DRAWER CHANGE = {0}", e.Data);
        }

        static void tp_DrawerOpenedTooLong(object sender, EventArgs e)
        {
            Console.WriteLine("DRAWER OPENED TOO LONG");
            //MessageBox.Show("DRAWER OPENED TOO LONG");
        }

        static void tp_DrawerOpened(object sender, EventArgs e)
        {
            Console.WriteLine("DRAWER OPENED");
        }

        static void tp_DrawerClosed(object sender, EventArgs e)
        {
            Console.WriteLine("DRAWER CLOSED");
        }

        static void tp_CoverStateChanged(object sender, GenericEventArgs<bool> e)
        {
            Console.WriteLine("COVER CHANGE = {0}", e.Data);
        }

        static void tp_CoverOpened(object sender, EventArgs e)
        {
            Console.WriteLine("COVER OPENED");
        }

        static void tp_CoverClosed(object sender, EventArgs e)
        {
            Console.WriteLine("COVER CLOSED");
        }

        static void client1_MediaRemoved(object sender, TIM.Devices.Framework.TIMsDevices.MediaRemovedEventArgs e)
        {
            Console.WriteLine("REMOVED:" + e.CheckInID.Value.ToString());
        }

        static void client1_MissingMedia(object sender, MissingMediaEventArgs e)
        {
            new Form().ShowDialog();
        }

        static void client1_MediaDetected(object sender, TIM.Devices.Framework.TIMsDevices.MediaDetectedEventArgs e)
        {
            Console.WriteLine(string.Format("{0} - {1}", e.Date, e.ECheckInID));
        }

        static void devDS6707_MediaFound(object sender, DeviceEventArgs e)
        {
            Console.WriteLine(e.Device.GetMediaID());
            Console.WriteLine(e.Device.GetMediaContent().ToString());
        }

        static void devClient_MediaRemoved(object sender, TIM.Devices.Framework.TIMsDevices.MediaRemovedEventArgs e)
        {
            Console.WriteLine("R");
        }

        static void devClient_MediaDetected(object sender, TIM.Devices.Framework.TIMsDevices.MediaDetectedEventArgs e)
        {
            Console.WriteLine("F");
        }

        /*static void dev_MediaRemoved(object sender, DeviceEventArgs e)
        {
            Console.WriteLine("REMOVED");
        }

        static void dev_MediaFound(object sender, DeviceEventArgs e)
        {
            Console.WriteLine("FOUND");
        }*/

        private static void SetPrinter(ref Microsoft.Office.Interop.Word.Application word)
        {
            object objWordBasic = word.WordBasic;
            String[] strArgNames = new String[] { "Printer", "DoNotSetAsSysDefault" };
            //object[] objValues = objValues = new object[] { RegistryManager.GetBonPrinterName(), 1 };
            //objWordBasic.GetType().InvokeMember("FilePrintSetup", BindingFlags.InvokeMethod, null, objWordBasic, objValues, null, null, strArgNames);
        }
    }
}
