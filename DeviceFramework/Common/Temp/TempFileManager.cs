using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TIM.Devices.Framework.Common.Temp
{
    public class TempFileManager
    {
        #region Statics

        private static TempFileManager MyTempFileManager = null;

        public static TempFileManager Unique
        {
            get
            {
                if (MyTempFileManager == null)
                    MyTempFileManager = new TempFileManager();
                return MyTempFileManager;
            }
        }

        #endregion

        #region Events

        public event EventHandler<ReleasingFailedEventArgs> ReleasingFailed;

        public event EventHandler<GenerateFileNameEventArgs> GenerateFileName;

        #endregion

        #region Fields

        private List<FileInfo> lstFileInfos;

        #endregion

        #region Properties

        public DirectoryInfo TempDirectory { get; private set; }

        #endregion

        #region Constructors

        public TempFileManager() :
            this(new DirectoryInfo(Path.GetTempPath()))
        {
        }

        public TempFileManager(DirectoryInfo dirTemp)
        {
            lstFileInfos = new List<FileInfo>();
            TempDirectory = dirTemp;
        }

        #endregion

        public FileInfo GetTempFile()
        {
            return GetTempFile(null, null);
        }

        public FileInfo GetTempFile(string strExtension)
        {
            return GetTempFile(strExtension, null);
        }
        public FileInfo GetTempFile(string strExtension, object objData)
        {
            if (!string.IsNullOrEmpty(strExtension))
                strExtension = strExtension.Trim().ToLower();

            char[] chInvalid = Path.GetInvalidFileNameChars();
            GenerateFileNameEventArgs evtArgs = new GenerateFileNameEventArgs(TempDirectory, strExtension, objData);
            string strFileName = string.Empty;

            while (true)
            {
                OnGenerateFileName(evtArgs);
                strFileName = evtArgs.FileName == null ? null : evtArgs.FileName.Trim();
                if (string.IsNullOrEmpty(strFileName))
                {
                    if (string.IsNullOrEmpty(strExtension))
                        strFileName = string.Format("{0}.tmp", Guid.NewGuid());
                    else
                        strFileName = string.Format("{0}.tmp.{1}", Guid.NewGuid(), strExtension);
                }

                if (strFileName.Any(c => chInvalid.Contains(c)))
                    continue;

                FileInfo MyFile = null;
                try
                {
                    MyFile = new FileInfo(Path.Combine(TempDirectory.FullName, strFileName));
                    if (MyFile.Exists || lstFileInfos.Any(fi => fi.Name == strFileName))
                        throw new Exception();
                }
                catch (Exception ex)
                {
                    continue;
                }
                lstFileInfos.Add(MyFile);
                return MyFile;
            }
        }

        public bool ReleaseAllFiles()
        {
            bool blnAll = true;
            for (int i = 0; i < lstFileInfos.Count; ++i)
            {
                try
                {
                    File.Delete(lstFileInfos[i].FullName);
                    lstFileInfos.RemoveAt(i);
                    --i;
                }
                catch (Exception ex)
                {
                    ReleasingFailedEventArgs evtArgs = new ReleasingFailedEventArgs(lstFileInfos[i], ex);
                    OnReleasingFailed(evtArgs);
                    if (evtArgs.Remove)
                    {
                        lstFileInfos.RemoveAt(i);
                        --i;
                    }
                    blnAll = false;
                }
            }
            return blnAll;
        }

        protected virtual void OnReleasingFailed(ReleasingFailedEventArgs evtArgs)
        {
            EventHandler<ReleasingFailedEventArgs> evtHandler = ReleasingFailed;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnGenerateFileName(GenerateFileNameEventArgs evtArgs)
        {
            EventHandler<GenerateFileNameEventArgs> evtHandler = GenerateFileName;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }
    }
}