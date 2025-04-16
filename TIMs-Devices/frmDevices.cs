using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows.Forms;
using TIM.Common.Data.Factories;
using TIM.Devices.FeigIDRWA02;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.Communication;
using TIM.Devices.Framework.Database;
using TIM.Devices.GantnerAccess6200;
using TIM.Devices.GantnerFunWriter5200;
using TIM.Devices.KeyboardListener;
using TIM.Devices.MotorolaDS6707;

namespace TIMs_Devices
{
    public partial class FrmDevices : Form
    {
        #region Fields

        private ServiceHost _serviceHost;
        private FrmDeviceList _frmDeviceList;

        #endregion

        #region Properties

        public DeviceServer DeviceServer { get; private set; }

        private FrmDeviceList DeviceList => _frmDeviceList ?? (_frmDeviceList = new FrmDeviceList());

        private List<IDevice> Devices { get; } = new List<IDevice>();

        private static List<TIM.Common.Data.Entities.ConfigDeviceInHouse> _configDevicesInHouse;
        private static List<TIM.Common.Data.Entities.ConfigDeviceExternal> _configDevicesExternal;
        private static List<TIM.Common.Data.Entities.ConfigDeviceEmployees> _configDevicesEmployee;

        #endregion

        #region Constructor

        public FrmDevices()
        {
            InitializeComponent();

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;

            _configDevicesInHouse = DbControllers.ConfigDevicesInHouse.Get(t => t.SubsidiaryID == SettingsManager.ComputerSubsidiary.SubsidiaryID &&
                                                                                t.ActivatedFlag);
            _configDevicesExternal = DbControllers.ConfigDevicesExternal.Get(t => t.SubsidiaryID == SettingsManager.ComputerSubsidiary.SubsidiaryID &&
                                                                                  t.ActivatedFlag);
            _configDevicesEmployee = DbControllers.ConfigDevicesEmployees.Get(t => t.SubsidiaryID == SettingsManager.ComputerSubsidiary.SubsidiaryID &&
                                                                                   t.ActivatedFlag);

            LoadDevices();
        }

        #endregion

        #region Methods

        private void frmDevices_Load(object sender, EventArgs e)
        {
            Start();
            MyNotifyIcon.Visible = true;

            foreach (var device in Devices.Where(t => t.DeviceIsOpen))
            {
                DeviceServer.DeviceManager.AddNewDeviceEvents((int)device.DeviceID, device);
            }
        }

        private void frmDummy_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
            MyNotifyIcon.Visible = false;
        }

        private void frmDummy_Shown(object sender, EventArgs e)
        {
            Hide();
        }

        private void mnRestart_Click(object sender, EventArgs e)
        {
            Stop();
            DeviceServer.ClearExceptions();
            DBContext.ClearData();
            Start();
        }

        private void Start()
        {
            Stopwatch stop = Stopwatch.StartNew();

            bool blnDBOk = false;
            try
            {
                DBContext.CheckFailDBConnection();
                blnDBOk = true;
            }
            catch
            {
                MyNotifyIcon.ShowBalloonTip(5000, "TIM's-Devices",
                    "Die Datenbank-Authentifizierung ist fehlgeschlagen.\r\n" +
                    "Bitte benachrichtigen Sie den Administrator.", ToolTipIcon.Error);
            }

            if (blnDBOk)
            {
                // The following code shows how to create a self-hosted service using the NetTcpBinding class.
                try
                {
                    ServiceMetadataBehavior serviceMetadataBehavior = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true
                    };
                    ServiceThrottlingBehavior serviceThrottlingBehavior = new ServiceThrottlingBehavior();
                    serviceThrottlingBehavior.MaxConcurrentSessions =
                        serviceThrottlingBehavior.MaxConcurrentCalls =
                        serviceThrottlingBehavior.MaxConcurrentInstances =
                        100;

                    DeviceServer = new DeviceServer();

                    var netTcpBinding = new NetTcpBinding(SecurityMode.None, true)
                    {
                        ReliableSession = { InactivityTimeout = TimeSpan.MaxValue },
                        ReceiveTimeout = TimeSpan.MaxValue,
                        SendTimeout = TimeSpan.FromSeconds(1.0d)
                    };

                    _serviceHost = new ServiceHost(
                        DeviceServer,
                        new Uri("http://localhost:54321/DeviceFramework"));
                    _serviceHost.AddServiceEndpoint(typeof(IDeviceServer),
                        netTcpBinding, "net.tcp://localhost:54320/DeviceFramework");
                    _serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
                    _serviceHost.Description.Behaviors.Add(serviceThrottlingBehavior);
                    _serviceHost.Open();
                }
                catch (Exception ex)
                {
                    DeviceList.Errors += string.Format("{0}\r\n", ex);
                    MyNotifyIcon.ShowBalloonTip(5000, "TIM's-Devices",
                        "Das Starten des DeviceServers ist fehlgeschlagen.\r\n" +
                        "Kontrollieren Sie die globalen Einstellungen.\r\n" +
                        "Bitte benachrichtigen Sie den Administrator.", ToolTipIcon.Error);
                }
            }

            stop.Stop();
            Debug.WriteLine(stop.ElapsedMilliseconds);
        }

        private void Stop()
        {
            _serviceHost?.Close();
            DeviceServer?.Dispose();
        }

        private void mnClose_Click(object sender, EventArgs e)
        {
            CloseDevices();
            Close();
        }

        private void MyNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DeviceList.Show();
            DeviceList.Activate();
        }

        private void LoadDevices()
        {
            Devices.Clear();
            SetNotifyIconText("Geräte werden geladen");

            //GAT Writer
            FunWriter5200 funWriter5200 = null;
            FunWriter5250 funWriter5250 = null;
            FunWriter6000 funWriter6000 = null;
            IDRWA02 idrwa02 = null;
            Access6200 access6200 = null;
            KeyboardListener keyboardListener = null;
            DS6707 ds6707 = null;

            for (int i = 0; i < 7; i++)
            {
                //Make sure, TIM's Devices will not crash if constructor throws an error
                try
                {
                    if (funWriter5200 == null)
                        funWriter5200 = new FunWriter5200(0);

                    if (funWriter5250 == null)
                        funWriter5250 = new FunWriter5250(1);

                    if (funWriter6000 == null)
                        funWriter6000 = new FunWriter6000(2);

                    if (idrwa02 == null)
                        idrwa02 = new IDRWA02(3);

                    if (access6200 == null)
                        access6200 = new Access6200(4, true);

                    if (keyboardListener == null)
                        keyboardListener = new KeyboardListener(5);

                    if (ds6707 == null)
                        ds6707 = new DS6707(6);

                    break;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Devices.Add(funWriter5200);
            Devices.Add(funWriter5250);
            Devices.Add(funWriter6000);
            Devices.Add(idrwa02);
            Devices.Add(access6200);
            Devices.Add(keyboardListener);
            Devices.Add(ds6707);

            var deviceDetails = DeviceDetailController.GetDeviceDetails();
            var usedDeviceDetailIDs = DeviceDetailController.GetUsedDeviceDetailIDs();

            foreach (var device in Devices)
            {
                try
                {
                    if (device.AllowOpenDevice(deviceDetails, usedDeviceDetailIDs) && DeviceIsConfigured(device))
                        device.Open();
                }
                catch (DeviceException ex)
                {
                    Console.WriteLine(ex.Message);
                    DeviceList.Errors += $"{ex}{Environment.NewLine}";
                }

                device.MediaSearchEnabled = true;
            }

            SetNotifyIconText($"TIM's Geräte ({Devices.Count(t => t.DeviceIsOpen)} geladen)");

            Console.WriteLine(@"All devices loaded");
        }

        private void CloseDevices()
        {
            foreach (var device in Devices.Where(t => t.DeviceIsOpen))
            {
                device.Close();
            }
        }

        private void SetNotifyIconText(string text)
        {
            MyNotifyIcon.Text = text;
        }

        /// <summary>
        /// Returns true if the device is activated in InHouse-, External- or Employee-device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private bool DeviceIsConfigured(IDevice device)
        {
            if (_configDevicesInHouse.Any(t => t.DeviceID == (int)device.DeviceID) ||
                _configDevicesExternal.Any(t => t.DeviceID == (int)device.DeviceID) ||
                _configDevicesEmployee.Any(t => t.DeviceID == (int)device.DeviceID))
                return true;

            return false;
        }

        #endregion
    }
}