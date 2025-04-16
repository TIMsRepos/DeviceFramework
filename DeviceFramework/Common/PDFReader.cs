using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using TIM.Common.CoreStandard;

namespace TIM.Devices.Framework.Common
{
    public class PDFReader
    {
        public string Version { get; private set; }
        public string Name { get; private set; }
        public FileInfo Executeable { get; private set; }
        public Enums.PDFReaderTypes PDFReaderType { get; private set; }

        private PDFReader(string strName, string strVersion, FileInfo MyExecuteable, Enums.PDFReaderTypes MyPDFReaderType)
        {
            Name = strName;
            Version = strVersion;
            Executeable = MyExecuteable;
            PDFReaderType = MyPDFReaderType;
        }

        public void Open(string strFullFileName)
        {
            switch (PDFReaderType)
            {
                case Enums.PDFReaderTypes.AdobeAcrobatReader:
                case Enums.PDFReaderTypes.AdobeAcrobat:
                case Enums.PDFReaderTypes.FoxitReader:
                    Process.Start(Executeable.FullName, string.Format("\"{0}\"", strFullFileName));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static IEnumerable<PDFReader> FindAllReaders()
        {
            return FindReaders(Enums.PDFReaderTypes.AdobeAcrobatReader, Enums.PDFReaderTypes.AdobeAcrobat, Enums.PDFReaderTypes.FoxitReader);
        }

        public static IEnumerable<PDFReader> FindReaders(params Enums.PDFReaderTypes[] MyPDFReaderTypes)
        {
            foreach (Enums.PDFReaderTypes MyPDFReaderType in MyPDFReaderTypes)
            {
                switch (MyPDFReaderType)
                {
                    case Enums.PDFReaderTypes.AdobeAcrobatReader:
                        {
                            RegistryKey regReaderRoot = null;
                            regReaderRoot = Registry.LocalMachine.OpenSubKey(@"Software\Adobe\Acrobat Reader");
                            if (regReaderRoot == null)
                                regReaderRoot = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Adobe\Acrobat Reader");
                            if (regReaderRoot != null)
                            {
                                string[] strVersions = regReaderRoot.GetSubKeyNames();
                                SortedList<string, string> dicVersions = new SortedList<string, string>();
                                foreach (string strVersion in strVersions)
                                    dicVersions.Add(strVersion, strVersion);
                                foreach (KeyValuePair<string, string> MyVersionPair in dicVersions)
                                {
                                    RegistryKey regReaderVersion = regReaderRoot.OpenSubKey(MyVersionPair.Value);
                                    if (regReaderVersion != null)
                                    {
                                        RegistryKey regReaderInstallPath = regReaderVersion.OpenSubKey("InstallPath");
                                        if (regReaderInstallPath != null)
                                        {
                                            object objInstallPath = regReaderInstallPath.GetValue(null);
                                            if (Directory.Exists(objInstallPath.ToString()))
                                            {
                                                FileInfo MyExecuteable = new FileInfo(Path.Combine(objInstallPath.ToString(), "AcroRd32.exe"));
                                                if (MyExecuteable.Exists)
                                                {
                                                    yield return new PDFReader("Adobe Acrobat Reader", MyVersionPair.Value,
                                                        MyExecuteable, Enums.PDFReaderTypes.AdobeAcrobatReader);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case Enums.PDFReaderTypes.AdobeAcrobat:
                        {
                            RegistryKey regReaderRoot = null;
                            regReaderRoot = Registry.LocalMachine.OpenSubKey(@"Software\Adobe\Adobe Acrobat");
                            if (regReaderRoot == null)
                                regReaderRoot = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Adobe\Adobe Acrobat");
                            if (regReaderRoot != null)
                            {
                                string[] strVersions = regReaderRoot.GetSubKeyNames();
                                SortedList<string, string> dicVersions = new SortedList<string, string>();
                                foreach (string strVersion in strVersions)
                                    dicVersions.Add(strVersion, strVersion);
                                foreach (KeyValuePair<string, string> MyVersionPair in dicVersions)
                                {
                                    RegistryKey regReaderVersion = regReaderRoot.OpenSubKey(MyVersionPair.Value);
                                    if (regReaderVersion != null)
                                    {
                                        RegistryKey regReaderInstallPath = regReaderVersion.OpenSubKey("InstallPath");
                                        if (regReaderInstallPath != null)
                                        {
                                            object objInstallPath = regReaderInstallPath.GetValue(null);
                                            if (Directory.Exists(objInstallPath.ToString()))
                                            {
                                                FileInfo MyExecuteable = new FileInfo(Path.Combine(objInstallPath.ToString(), "Acrobat.exe"));
                                                if (MyExecuteable.Exists)
                                                {
                                                    yield return new PDFReader("Adobe Acrobat", MyVersionPair.Value,
                                                        MyExecuteable, Enums.PDFReaderTypes.AdobeAcrobat);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case Enums.PDFReaderTypes.FoxitReader:
                        {
                            RegistryKey regFoxitReader = null;
                            regFoxitReader = Registry.LocalMachine.OpenSubKey(@"Software\Foxit Software\Foxit Reader");
                            if (regFoxitReader == null)
                                regFoxitReader = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Foxit Software\Foxit Reader");
                            if (regFoxitReader != null)
                            {
                                object objInstallPath = regFoxitReader.GetValue("InstallPath");
                                object objVersion = regFoxitReader.GetValue("Version");
                                if (Directory.Exists(objInstallPath.ToString()))
                                {
                                    FileInfo MyExecuteable = new FileInfo(Path.Combine(objInstallPath.ToString(), "Foxit Reader.exe"));
                                    if (MyExecuteable.Exists)
                                    {
                                        yield return new PDFReader("Foxit Reader", objVersion == null ? string.Empty : objVersion.ToString(),
                                            MyExecuteable, Enums.PDFReaderTypes.FoxitReader);
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}