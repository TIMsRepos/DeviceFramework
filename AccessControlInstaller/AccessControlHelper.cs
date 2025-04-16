using System;
using System.Diagnostics;

namespace AccessControlInstaller
{
    /// <summary>
    /// This class adds, deletes or checks the existence of
    /// Access Control List (ACL) Entries using the netsh.exe
    ///
    /// For more info see Netsh Commands for Hypertext Transfer Protocol (HTTP):
    /// https://technet.microsoft.com/en-us/library/cc725882(WS.10).aspx#BKMK_9
    /// </summary>
    public static class AccessControlHelper
    {
        #region Fields

        // see SDDL (Security Descriptor Definition Language)
        private const string SecurityDescriptor = "D:(A;;GA;;;S-1-5-32-545)"; //D:(A;;GA;;;BU)
        private const string DeviceFrameworkUrlPrefix = @"http://+:54321/DeviceFramework/";
        private const string Domain = "BUILTIN";
        private const string User = "Users";

        #endregion

        #region Methods

        public static bool AddDeviceFrameworkAccessControlEntry(bool aclEntryAlreadyChecked = false)
        {
            return AddAccessControlEntry(DeviceFrameworkUrlPrefix, Domain, User, aclEntryAlreadyChecked);
        }

        public static bool AddAccessControlEntry(string address, string domain, string user,
            bool aclEntryAlreadyChecked = false)
        {
            if (!aclEntryAlreadyChecked &&
                CheckForExistingAccessControl())
            {
                if (!RemoveAccessControlEntry(address))
                {
                    // The ACL entry could not be created.
                    throw new Exception("The Access Control List Entry for the Device Framework could not be removed.");
                }
            }

            // create command with netsh
            string args =
                $@"http add urlacl url={address} user={domain}\{user} listen=yes delegate=yes sddl={SecurityDescriptor}";
            var psi = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            // execute netsh.exe with the given arguments
            var process = Process.Start(psi);
            process?.WaitForExit();

            return CheckForExistingAccessControl(address);
        }

        public static bool RemoveAccessControlEntry(string address = DeviceFrameworkUrlPrefix)
        {
            string args = $@"http delete urlacl url={address}";

            // create command with netsh
            var psi = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
            };

            // execute netsh.exe with the given arguments
            var process = Process.Start(psi);
            process?.WaitForExit();

            // check if the access control entry was successfully removed
            return !CheckForExistingAccessControl(address);
        }

        public static bool CheckForExistingAccessControl(string address = DeviceFrameworkUrlPrefix)
        {
            var installedAcs = GetAcl();
            return installedAcs.Contains(address);
        }

        private static string GetAcl()
        {
            var output = string.Empty;

            // create command with netsh
            var psi = new ProcessStartInfo("netsh", "http show urlacl")
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            // execute netsh.exe with the given arguments
            var process = Process.Start(psi);
            if (process != null)
            {
                // read list of installed ACL entries
                output = process.StandardOutput.ReadToEnd();
            }

            process?.WaitForExit();
            return output;
        }

        #endregion

    }
}