using System.DirectoryServices;

namespace TIM.Devices.Framework.UserAccountControl.ActiveDirectory
{
    internal static class ADEntry
    {
        public static string GetDisplayName(string userName)
        {
            try
            {
                var entry = UACHelper.UACDirectoryEntry;
                var mySearcher = new DirectorySearcher(entry)
                {
                    Filter = "(sAMAccountName= " + userName + ")",
                    PropertyNamesOnly = false
                };

                foreach (SearchResult result in mySearcher.FindAll())
                {
                    return result.GetDirectoryEntry().Properties["displayName"].Value.ToString();
                }

                return userName;
            }
            catch
            {
                return userName;
            }
        }
    }
}