using System;
using System.Collections.Generic;
using System.Text;

namespace SetHttpCfg.AccessControl
{
    public interface IAccessControlList
    {
        List<IAccessControlEntry> Entries { get; }

        string ToSDDLString();
    }
}
