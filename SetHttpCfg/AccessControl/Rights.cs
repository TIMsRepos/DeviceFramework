using System;
using System.Collections.Generic;
using System.Text;

namespace SetHttpCfg.AccessControl
{
    [Flags]
    public enum Rights
    {
        // Generic access rights
        GenericAll = 0,
        GenericRead,
        GenericWrite,
        GenericExecute,

        // Standard access rights
        ReadControl,
        Delete,
        WriteDAC,
        WriteOwner,

        // Directory service object access rights
        ReadProperty,
        WriteProperty,
        CreateChild,
        DeleteChild,
        ListChildren,
        SelfWrite,
        ListObject,
        DeleteTree,
        ControlAccess,

        // File access rights
        FileAll,
        FileRead,
        FileWrite,
        FileExecute,

        // Registry key access rights
        KeyAll,
        KeyRead,
        KeyWrite,
        KeyExecute
    }

    public static class RightsExtensions
    {
        public static string ToSDDLString(this Rights flags)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Rights flag in Enum.GetValues(typeof(Rights)))
            {
                if ((flags & flag) != flag)
                    continue;

                switch (flags)
                {
                    case Rights.GenericAll:
                        sb.Append("GA");
                        break;
                    case Rights.GenericRead:
                        sb.Append("GR");
                        break;
                    case Rights.GenericWrite:
                        sb.Append("GW");
                        break;
                    case Rights.GenericExecute:
                        sb.Append("GX");
                        break;
                    case Rights.ReadControl:
                        sb.Append("RC");
                        break;
                    case Rights.Delete:
                        sb.Append("SD");
                        break;
                    case Rights.WriteDAC:
                        sb.Append("WD");
                        break;
                    case Rights.WriteOwner:
                        sb.Append("WO");
                        break;
                    case Rights.ReadProperty:
                        sb.Append("RP");
                        break;
                    case Rights.WriteProperty:
                        sb.Append("WP");
                        break;
                    case Rights.CreateChild:
                        sb.Append("CC");
                        break;
                    case Rights.DeleteChild:
                        sb.Append("DC");
                        break;
                    case Rights.ListChildren:
                        sb.Append("LC");
                        break;
                    case Rights.SelfWrite:
                        sb.Append("SW");
                        break;
                    case Rights.ListObject:
                        sb.Append("LO");
                        break;
                    case Rights.DeleteTree:
                        sb.Append("DT");
                        break;
                    case Rights.ControlAccess:
                        sb.Append("CR");
                        break;
                    case Rights.FileAll:
                        sb.Append("FA");
                        break;
                    case Rights.FileRead:
                        sb.Append("FR");
                        break;
                    case Rights.FileWrite:
                        sb.Append("FW");
                        break;
                    case Rights.FileExecute:
                        sb.Append("FX");
                        break;
                    case Rights.KeyAll:
                        sb.Append("KA");
                        break;
                    case Rights.KeyRead:
                        sb.Append("KR");
                        break;
                    case Rights.KeyWrite:
                        sb.Append("KW");
                        break;
                    case Rights.KeyExecute:
                        sb.Append("KX");
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            return sb.ToString();
        }
    }
}
