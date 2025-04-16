using System;
using System.Collections.Generic;
using System.Text;

namespace SetHttpCfg.AccessControl
{
    public class HttpCfgAccessControlList : IAccessControlList
    {
        private List<IAccessControlEntry> entries;

        public List<IAccessControlEntry> Entries
        {
            get { return entries; }
        }

        public HttpCfgAccessControlList()
        {
            entries = new List<IAccessControlEntry>();
        }

        public string ToSDDLString()
        {
            StringBuilder sb = new StringBuilder();

            entries.Sort(new Comparison<IAccessControlEntry>(delegate(IAccessControlEntry ei1, IAccessControlEntry ei2)
            {
                HttpCfgAccessControlEntry e1 = (HttpCfgAccessControlEntry)ei1;
                HttpCfgAccessControlEntry e2 = (HttpCfgAccessControlEntry)ei2;

                if (e1.Allow != e2.Allow)
                {
                    if (e1.Allow)
                        return -1;
                    else
                        return 1;
                }
                else
                    return 0;
            }));

            sb.Append("D:");
            foreach (HttpCfgAccessControlEntry entry in entries)
                sb.Append(string.Format("({0})", entry.ToSDDLString()));

            return sb.ToString();
        }
    }
}
