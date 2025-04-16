using System;
using System.Collections;
using System.ComponentModel;

namespace AccessControlInstaller
{
    [RunInstaller(true)]
    public partial class AccessControlInstaller : System.Configuration.Install.Installer
    {
        public AccessControlInstaller()
        {
            InitializeComponent();
        }
        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            if (AccessControlHelper.CheckForExistingAccessControl())
            {
                return;
            }

            if (!AccessControlHelper.AddDeviceFrameworkAccessControlEntry(aclEntryAlreadyChecked: true))
            {
                throw new Exception("The Access Control List Entry for the Device Framework could not be created.");
            }
        }

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            base.OnAfterUninstall(savedState);

            if (AccessControlHelper.CheckForExistingAccessControl())
            {
                AccessControlHelper.RemoveAccessControlEntry();
            }
        }

        protected override void OnAfterRollback(IDictionary savedState)
        {
            base.OnAfterRollback(savedState);

            if (AccessControlHelper.CheckForExistingAccessControl())
            {
                AccessControlHelper.RemoveAccessControlEntry();
            }
        }
    }
}