using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using TIM.Common.CoreStandard;
using TIM.Common.CoreStandard.Helper;
using TIM.Common.Data.Helper;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.Framework.Database
{
    public class DBContext
    {
        #region Members

        private static TIMDataClassesDataContext ctxTIMDataClasses;
        private static Nullable<int> MyCurrentComputerSubsidiaryID = null;

        #endregion

        #region Getter & Setter

        //public static TIMDataClassesDataContext DataContext
        //{
        //    get
        //    {
        //        if (ctxTIMDataClasses == null)
        //            ctxTIMDataClasses =
        //                new TIMDataClassesDataContext(
        //                    GetConnectionString(GetDefaultConfiguration()));

        //        return ctxTIMDataClasses;
        //    }
        //}
        public static TIMDataClassesDataContext DataContextRead
        {
            get
            {
                TIMDataClassesDataContext ctxFrontEndDataClasses =
                    new TIMDataClassesDataContext(GetConnectionString(GetDefaultConfiguration()));

                ctxFrontEndDataClasses.ObjectTrackingEnabled = false;
                ctxFrontEndDataClasses.DeferredLoadingEnabled = false;
                //ctxFrontEndDataClasses.Log = Console.Out; // Disable !!

                return ctxFrontEndDataClasses;
            }
        }

        public static TIMDataClassesDataContext DataContextEdit
        {
            get
            {
                TIMDataClassesDataContext ctxFrontEndDataClasses =
                    new TIMDataClassesDataContext(GetConnectionString(GetDefaultConfiguration()));

                ctxFrontEndDataClasses.ObjectTrackingEnabled = true;
                ctxFrontEndDataClasses.DeferredLoadingEnabled = false;
                //ctxFrontEndDataClasses.Log = Console.Out; // Disable !!

                return ctxFrontEndDataClasses;
            }
        }

        public static int CurrentComputerSubsidiaryID
        {
            get
            {
                if (MyCurrentComputerSubsidiaryID.HasValue == false)
                {
                    int intSubsidiaryID;

                    // load ComputerSetting
                    ComputerSetting MySetting =
                        DataContextRead.ComputerSettings.SingleOrDefault<ComputerSetting>(
                        s => s.ComputerNameID == System.Windows.Forms.SystemInformation.ComputerName
                            && s.ComputerSettingID == "ComputerLocation"
                            && s.ComputerDetailSettingID == "SubsidiaryID");
                    if (MySetting == null)
                        throw new ComputerSubsidiaryException("Es wurde keine Anlage für diesen Computer hinterlegt.");
                    if (Int32.TryParse(MySetting.Value.ToString(), out intSubsidiaryID) == false
                        || intSubsidiaryID < 0)
                        throw new ComputerSubsidiaryException("Die ID der Anlage dieses Computers ist ungültig.");

                    // load Subsidiary
                    Subsidiary MySubsidiary =
                        DataContextRead.Subsidiaries.SingleOrDefault<Subsidiary>(
                        s => s.SubsidiaryID == intSubsidiaryID);
                    if (MySubsidiary == null)
                        throw new ComputerSubsidiaryException("Die Anlage dieses Computers ist ungültig.");

                    // set SubsidiaryID in DBContext
                    MyCurrentComputerSubsidiaryID = MySubsidiary.SubsidiaryID;
                }

                return MyCurrentComputerSubsidiaryID.Value;
            }
        }

        #endregion

        #region Methods

        public static void ClearData()
        {
            MyCurrentComputerSubsidiaryID = new int?();
        }

        public static string GetConnectionString(Configuration MyConfig = null)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = AppSettingHelper.DataSource,
                InitialCatalog = AppSettingHelper.InitialCatalog,
                IntegratedSecurity = AppSettingHelper.IntegratedSecurity
            };

            if (!AppSettingHelper.IntegratedSecurity)
            {
                builder.UserID = AppSettingHelper.UserID;
                builder.Password = AppSettingHelper.Password;
            }
            return builder.ToString();
            //return MyConfig.ConnectionStrings.ConnectionStrings["TIM_LINQ"].ConnectionString;
        }

        public static Configuration GetDefaultConfiguration()
        {
            Enums.Applications MyApplication = AppHelper.DetectApplication();

            if (MyApplication == Enums.Applications.TIMsGateway)
            {
                Configuration MyCfg = WebConfigurationManager.OpenWebConfiguration("~");
                return MyCfg;
            }
            else
            {
                string strConfigFileName = @"\TIM-Administrator.exe.config";

                ExeConfigurationFileMap MyFileMap = new ExeConfigurationFileMap();
                string strFullConfigFileName = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + strConfigFileName;
                if (!File.Exists(strFullConfigFileName))
                {
                    foreach (Enums.Applications MyApp in Enum.GetValues(typeof(Enums.Applications)))
                    {
                        DirectoryInfo dirApp = RegistryHelper.GetInstallPath(MyApp);
                        if (dirApp != null)
                        {
                            strFullConfigFileName = dirApp.FullName + strConfigFileName;
                            if (File.Exists(strFullConfigFileName))
                                break;
                        }
                    }
                }
                MyFileMap.ExeConfigFilename = strFullConfigFileName;
                return ConfigurationManager.OpenMappedExeConfiguration(MyFileMap, ConfigurationUserLevel.None);
            }
        }

        public static void CheckFailDBConnection()
        {
            DBContext.DataContextRead.Subsidiaries.Count();
        }

        public static bool SubmitChanges(TIMDataClassesDataContext MyContext)
        {
            for (int intRetry = 0; intRetry < 4; intRetry++)
            {
                try
                {
                    MyContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                    return true;
                }
                catch (ChangeConflictException)
                {
                    MyContext.ChangeConflicts.ResolveAll(RefreshMode.KeepChanges);
                }
            }

            System.Windows.Forms.Application.Exit();
            throw new Exception("Die Änderungen konnten nicht gespeichert werden!");
        }

        #endregion
    }
}