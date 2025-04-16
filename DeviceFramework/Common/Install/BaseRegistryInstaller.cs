using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;

namespace TIM.Devices.Framework.Common.Install
{
    public abstract class BaseRegistryInstaller : Installer
    {
        private static SecurityIdentifier MyEveryoneSecurityIdentifier = null;
        private static NTAccount MyEveryoneNTAccount = null;

        public static SecurityIdentifier EveryoneSecurityIdentifier
        {
            get
            {
                if (MyEveryoneSecurityIdentifier == null)
                    MyEveryoneSecurityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                return MyEveryoneSecurityIdentifier;
            }
        }
        public static NTAccount EveryoneNTAccount
        {
            get
            {
                if (MyEveryoneNTAccount == null)
                    MyEveryoneNTAccount = EveryoneSecurityIdentifier.Translate(typeof(NTAccount)) as NTAccount;
                return MyEveryoneNTAccount;
            }
        }

        public abstract string[] CreatePaths { get; }
        public abstract string[] AccessPaths { get; }
        public abstract RegistryView[] RegistryViews { get; }
        public abstract NTAccount UserNTAccount { get; }

        public BaseRegistryInstaller()
            : base()
        {
            this.AfterInstall += new InstallEventHandler(RegistryInstaller_AfterInstall);
        }

        private void RegistryInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            Install();
        }

        public void Install()
        {
            CreateCreatePaths();
            SetAccessControl();
        }

        public void CreateCreatePaths(string[] strCreatePaths = null)
        {
            if (strCreatePaths == null)
                strCreatePaths = CreatePaths;

            foreach (RegistryView MyRegistryView in RegistryViews)
            {
                RegistryKey MyRootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, MyRegistryView);
                foreach (string strPath in strCreatePaths)
                {
                    RegistryKey MyKey = MyRootKey.CreateSubKey(strPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
            }
        }

        public void SetAccessControl(string[] strAccessPaths = null)
        {
            if (strAccessPaths == null)
                strAccessPaths = AccessPaths;

            foreach (RegistryView MyRegistryView in RegistryViews)
            {
                RegistryKey MyRootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, MyRegistryView);
                foreach (string strPath in strAccessPaths)
                {
                    RegistryKey MyKey = MyRootKey.OpenSubKey(strPath, true);
                    SetAccessControl(MyKey);
                }
            }
        }

        private void SetAccessControl(RegistryKey MyKey)
        {
            // Get, modify and save security
            RegistrySecurity MySecurity = MyKey.GetAccessControl();
            foreach (var accessRule in MySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
            {
                // Create access rule
                RegistryAccessRule MyAccessRule = new RegistryAccessRule(
                    UserNTAccount.ToString(),
                    RegistryRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                MySecurity.AddAccessRule(MyAccessRule);
            }
            MyKey.SetAccessControl(MySecurity);

            // Iterate over subkeys
            foreach (string strSubKeyName in MyKey.GetSubKeyNames())
            {
                RegistryKey MySubKey = MyKey.OpenSubKey(strSubKeyName, true);
                SetAccessControl(MySubKey);
            }
        }
    }
}
