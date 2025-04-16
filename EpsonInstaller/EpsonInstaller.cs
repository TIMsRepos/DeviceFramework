using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.GAC;
using System.IO;
using TIM.Devices.Framework;

namespace EpsonInstaller
{
    [RunInstaller(true)]
    public partial class EpsonInstaller : Installer
    {
        public static string AssemblyName
        {
            get { return "EpsonStatusAPI"; }
        }
        public static FileInfo TargetDirAssembly
        {
            get { return new FileInfo(RegistryManager.GetInstallPath(TIM.Devices.Framework.Common.Applications.Devices).FullName + string.Format("\\{0}.dll", AssemblyName)); }
        }

        public EpsonInstaller()
        {
            InitializeComponent();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            Install();
        }

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            base.OnAfterUninstall(savedState);

            Uninstall();
        }

        protected override void OnAfterRollback(IDictionary savedState)
        {
            base.OnAfterRollback(savedState);

            Uninstall();
        }

        private void Install()
        {
            IAssemblyCache MyCache = AssemblyCache.CreateAssemblyCache();
            FUSION_INSTALL_REFERENCE[] MyInstallRefs = GetInstallReferences();

            MyCache.InstallAssembly(
                (uint)IASSEMBLYCACHE_INSTALL_FLAG.IASSEMBLYCACHE_INSTALL_FLAG_FORCE_REFRESH,
                TargetDirAssembly.FullName, MyInstallRefs);
        }

        private void Uninstall()
        {
            ASSEMBLY_INFO MyAssemblyInfo = new ASSEMBLY_INFO();
            IAssemblyCache MyGAC = AssemblyCache.CreateAssemblyCache();
            uint intResult;
            FUSION_INSTALL_REFERENCE[] MyInstallRefs = GetInstallReferences();

            MyGAC.QueryAssemblyInfo(
                (uint)QUERYASMINFO_FLAG.QUERYASMINFO_FLAG_GETSIZE,
                AssemblyName, ref MyAssemblyInfo);
            if (MyAssemblyInfo.uliAssemblySizeInKB > 0L)
                MyGAC.UninstallAssembly(0, AssemblyName, MyInstallRefs, out intResult);
        }

        private static FUSION_INSTALL_REFERENCE[] GetInstallReferences()
        {
            FUSION_INSTALL_REFERENCE[] MyInstallRef = new FUSION_INSTALL_REFERENCE[1];
            MyInstallRef[0].dwFlags = (uint)System.GAC.IASSEMBLYCACHE_INSTALL_FLAG.IASSEMBLYCACHE_INSTALL_FLAG_FORCE_REFRESH;
            MyInstallRef[0].guidScheme = System.GAC.AssemblyCache.FUSION_REFCOUNT_OPAQUE_STRING_GUID;
            MyInstallRef[0].szIdentifier = "TIM's Devices";
            MyInstallRef[0].szNonCannonicalData = "TIM's Devices";

            return MyInstallRef;
        }
    }
}
