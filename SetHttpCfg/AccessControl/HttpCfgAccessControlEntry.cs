using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;

namespace SetHttpCfg.AccessControl
{
    public class HttpCfgAccessControlEntry : IAccessControlEntry
    {
        #region Members
        private bool allow;
        private Rights rights;
        private SecurityIdentifier suid;
        #endregion

        #region Getter & Setter
        public bool Allow
        {
            get { return allow; }
            set { allow = value; }
        }
        public Rights Rights
        {
            get { return rights; }
            set { rights = value; }
        }
        public SecurityIdentifier SecurityIdentifier
        {
            get { return suid; }
            set { suid = value; }
        }
        #endregion

        public string ToSDDLString()
        {
            return string.Format("{0};{1};{2};{3};{4};{5}",
                allow ? "A" : "D",
                "",
                rights.ToSDDLString(),
                "",
                "",
                suid.ToString());
        }
    }
}
