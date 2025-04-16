using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SetHttpCfg.AccessControl;

namespace SetHttpCfg
{
    public class HttpConfig : IDisposable
    {
        #region P/Invokes
        [DllImport("Httpapi.dll")]
        private static extern int HttpQueryServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG ConfigId, IntPtr pInputConfigInfo, int InputConfigLength, IntPtr pOutputConfigInfo, int OutputConfigInfoLength, ref int pReturnLength, IntPtr pOverlapped);
        [DllImport("Httpapi.dll")]
        private static extern int HttpInitialize(HTTP_VERSION Version, HTTP_INITIALIZE Flags, IntPtr pReserved);
        [DllImport("Httpapi.dll")]
        private static extern int HttpTerminate(HTTP_INITIALIZE Flags, IntPtr pReserved);
        [DllImport("Httpapi.dll")]
        private static extern int HttpSetServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG ConfigId, IntPtr pConfigInformation, int ConfigInformationLength, IntPtr pOverlapped);
        [DllImport("Httpapi.dll")]
        private static extern int HttpDeleteServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG ConfigId, IntPtr pConfigInformation, int ConfigInformationLength, IntPtr pOverlapped);
        #endregion

        #region Constants
        private const int NO_ERROR = 0;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int ERROR_NO_MORE_ITEMS = 259;
        #endregion

        #region Constructor & Deconstructor
        public HttpConfig()
        {
            HTTP_VERSION version = new HTTP_VERSION(1, 0);

            HttpInitialize(version, HTTP_INITIALIZE.CONFIG, IntPtr.Zero);
        }
        ~HttpConfig()
        {
            Dispose();
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            HttpTerminate(HTTP_INITIALIZE.CONFIG, IntPtr.Zero);
        }
        public Dictionary<string, string> GetACLs()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            HTTP_SERVICE_CONFIG_URLACL_QUERY query = new HTTP_SERVICE_CONFIG_URLACL_QUERY();
            query.QueryDesc = HTTP_SERVICE_CONFIG_QUERY_TYPE.Next;

            IntPtr pQuery = Marshal.AllocHGlobal(Marshal.SizeOf(query));

            try
            {
                int retVal = NO_ERROR;
                query.Token = 0;
                while (true)
                {
                    Marshal.StructureToPtr(query, pQuery, false);

                    try
                    {
                        int size = 0;

                        retVal = HttpQueryServiceConfiguration(IntPtr.Zero,
                            HTTP_SERVICE_CONFIG.UrlAclInfo, pQuery,
                            Marshal.SizeOf(query), IntPtr.Zero, 0,
                            ref size, IntPtr.Zero);

                        if (retVal == ERROR_NO_MORE_ITEMS)
                            break;
                        if (retVal != ERROR_INSUFFICIENT_BUFFER)
                            throw new Exception("HttpQueryServiceConfiguration unexpected error code!");

                        IntPtr pConfig = Marshal.AllocHGlobal((IntPtr)size);

                        try
                        {
                            retVal = HttpQueryServiceConfiguration(IntPtr.Zero,
                                HTTP_SERVICE_CONFIG.UrlAclInfo, pQuery,
                                Marshal.SizeOf(query), pConfig, size,
                                ref size, IntPtr.Zero);

                            if (retVal == NO_ERROR)
                            {
                                HTTP_SERVICE_CONFIG_URLACL_SET config = (HTTP_SERVICE_CONFIG_URLACL_SET)Marshal.PtrToStructure(pConfig, typeof(HTTP_SERVICE_CONFIG_URLACL_SET));
                                dict.Add(config.KeyDesc.UrlPrefix, config.ParamDesc.StringSecurityDescriptor);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pConfig);
                        }
                    }
                    finally
                    {
                        Marshal.DestroyStructure(pQuery, typeof(HTTP_SERVICE_CONFIG_URLACL_QUERY));
                    }

                    ++query.Token;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pQuery);
            }

            return dict;
        }
        public void AddACL(string urlPrefix, HttpCfgAccessControlList acl)
        {
            HTTP_SERVICE_CONFIG_URLACL_SET config = new HTTP_SERVICE_CONFIG_URLACL_SET();
            config.KeyDesc.UrlPrefix = urlPrefix;
            config.ParamDesc.StringSecurityDescriptor = acl.ToSDDLString();

            IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config));

            Marshal.StructureToPtr(config, pConfig, false);

            try
            {
                int retVal = HttpSetServiceConfiguration(IntPtr.Zero,
                    HTTP_SERVICE_CONFIG.UrlAclInfo, pConfig,
                    Marshal.SizeOf(config), IntPtr.Zero);

                if (retVal != NO_ERROR)
                    throw new Exception(string.Format("Error setting config with code '{0}'", retVal));
            }
            finally
            {
                if (pConfig != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(pConfig, typeof(HTTP_SERVICE_CONFIG_URLACL_SET));
                    Marshal.FreeHGlobal(pConfig);
                }
            }
        }
        public void RemoveACL(string urlPrefix)
        {
            HTTP_SERVICE_CONFIG_URLACL_SET config = new HTTP_SERVICE_CONFIG_URLACL_SET();
            config.KeyDesc.UrlPrefix = urlPrefix;

            IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config));

            Marshal.StructureToPtr(config, pConfig, false);

            try
            {
                int retVal = HttpDeleteServiceConfiguration(IntPtr.Zero,
                    HTTP_SERVICE_CONFIG.UrlAclInfo, pConfig,
                    Marshal.SizeOf(config), IntPtr.Zero);

                if (retVal != NO_ERROR)
                    throw new Exception(string.Format("Error removing config, error code: '{0}'", retVal));
            }
            finally
            {
                if (pConfig != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(pConfig, typeof(HTTP_SERVICE_CONFIG_URLACL_SET));
                    Marshal.FreeHGlobal(pConfig);
                }
            }
        }
        public bool AddACLChecked(string urlPrefix, HttpCfgAccessControlList acl)
        {
            Dictionary<string, string> dict;

            // Check if existing => delete
            dict = GetACLs();
            if (dict.ContainsKey(urlPrefix))
            {
                if (!RemoveACLChecked(urlPrefix))
                    return false;
            }

            // Add
            AddACL(urlPrefix, acl);

            // Check if successful
            dict = GetACLs();
            return dict.ContainsKey(urlPrefix);
        }
        public bool RemoveACLChecked(string urlPrefix)
        {
            Dictionary<string, string> dict;

            // Check if existing
            dict = GetACLs();
            if (!dict.ContainsKey(urlPrefix))
                return true;

            // Remove
            RemoveACL(urlPrefix);

            // Check if successful
            dict = GetACLs();
            return !dict.ContainsKey(urlPrefix);
        }
        #endregion

        #region Enumerations
        [Flags]
        enum HTTP_INITIALIZE : uint
        {
            SERVER = 0x00000001,
            CONFIG = 0x00000002
        }
        enum HTTP_SERVICE_CONFIG_QUERY_TYPE
        {
            Exact = 0,
            Next,
            Max
        }
        enum HTTP_SERVICE_CONFIG
        {
            IPListenList = 0,
            SSLCertInfo,
            UrlAclInfo,
            Max
        }
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        struct HTTP_SERVICE_CONFIG_URLACL_QUERY
        {
            public HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
            public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
            public uint Token;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct HTTP_SERVICE_CONFIG_URLACL_KEY
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string UrlPrefix;

            public HTTP_SERVICE_CONFIG_URLACL_KEY(string urlPrefix)
            {
                UrlPrefix = urlPrefix;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HTTP_SERVICE_CONFIG_URLACL_SET
        {
            public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
            public HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct HTTP_SERVICE_CONFIG_URLACL_PARAM
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string StringSecurityDescriptor;

            public HTTP_SERVICE_CONFIG_URLACL_PARAM(string securityDescriptor)
            {
                StringSecurityDescriptor = securityDescriptor;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        struct HTTP_VERSION
        {
            public ushort MajorVersion;
            public ushort MinorVersion;

            public HTTP_VERSION(ushort majorVersion, ushort minorVersion)
            {
                MajorVersion = majorVersion;
                MinorVersion = minorVersion;
            }
        }
        #endregion
    }
}
