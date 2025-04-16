using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using TIM.Devices.Framework.Communication;
using TIMs_Devices.Properties;

namespace TIMs_Devices
{
    public partial class FrmDeviceList : Form
    {
        #region Properties

        public string Errors { get; set; }

        #endregion

        #region Constructor

        public FrmDeviceList()
        {
            InitializeComponent();

            sbpVersion.Text = lblVersion.Text = @"Version: " + GetAssemblyVersion(Assembly.GetExecutingAssembly());
        }

        #endregion

        #region Methods

        private void frmDeviceList_Shown(object sender, EventArgs e)
        {
            ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            foreach (DictionaryEntry dicEntry in resourceSet.OfType<DictionaryEntry>())
                Console.WriteLine(dicEntry.Key.ToString());

            if (Program.DevicesForm.DeviceServer != null)
            {
                foreach (var device in Program.DevicesForm.DeviceServer.DeviceManager.Devices)
                    lbxDevices.Items.Add(device.Name);
                foreach (Exception ex in DeviceServer.MyExceptions)
                {
                    Errors += $"{ex}\r\n";
                }

                Program.DevicesForm.DeviceServer.ClientsChanged += DeviceServer_ClientAdded;
            }
        }

        private void DeviceServer_ClientAdded(object sender, EventArgs e)
        {
            statusBar.Invoke(new MethodInvoker(() => sbpClients.Text = @"Clients: " + Program.DevicesForm.DeviceServer.ClientCount));
        }

        private void frmDeviceList_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            using (frmErrors frmErrors = new frmErrors())
            {
                frmErrors.Errors = Errors;
                frmErrors.ShowDialog();
            }
        }

        private string GetAssemblyVersion(Assembly assembly)
        {
            var fileVersionAttribute = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));

            if (fileVersionAttribute != null)
            {
                return fileVersionAttribute.Version.ToString();
            }
            else
            {
                return assembly.GetName().Version.ToString();
            }
        }

        #endregion
    }
}