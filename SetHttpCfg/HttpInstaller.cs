using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using SetHttpCfg.AccessControl;
using System.Security.Principal;


namespace SetHttpCfg
{
    [RunInstaller(true)]
    public partial class HttpInstaller : Installer
    {
        private const string strUrlPrefix = @"http://+:54321/DeviceFramework/";

        private HttpConfig MyConfig = new HttpConfig();

        public HttpInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            HttpCfgAccessControlEntry MyEntry = new HttpCfgAccessControlEntry();
            MyEntry.Allow = true;
            MyEntry.Rights = Rights.GenericAll;
            MyEntry.SecurityIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            
            HttpCfgAccessControlList MyList = new HttpCfgAccessControlList();
            MyList.Entries.Add(MyEntry);

            MyConfig.AddACLChecked(strUrlPrefix, MyList);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);

            MyConfig.RemoveACLChecked(strUrlPrefix);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            MyConfig.RemoveACLChecked(strUrlPrefix);
        }
    }
}
