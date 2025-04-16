using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;

namespace SetHttpCfg.AccessControl
{
    public interface IAccessControlEntry
    {
        Rights Rights { get; set; }
        SecurityIdentifier SecurityIdentifier { get; set; }

        string ToSDDLString();
    }
}
