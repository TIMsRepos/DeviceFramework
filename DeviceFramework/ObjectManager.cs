using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TIM.Devices.Framework.Loader;
using Enums = TIM.Common.CoreStandard.Enums;
using RegistryHelper = TIM.Common.CoreStandard.Helper.RegistryHelper;

namespace TIM.Devices.Framework
{
    /// <summary>
    /// A class to request and handle dynamically loaded objects
    /// </summary>
    public class ObjectManager
    {
        #region Members

        private static TypeCatalog MyTypeCatalog;
        private static Enums.DataSourceType MyDataSourceType = Enums.DataSourceType.DB;
        private static Dictionary<string, object> dicSingletons = new Dictionary<string, object>();
        private static List<DirectoryInfo> dicLibraryPaths = new List<DirectoryInfo>();

        #endregion

        #region Getter & Setter

        /// <summary>
        /// Defines the source of the typecatalog information and the loading behavior of the lazy assembly loading
        /// </summary>
        public static Enums.DataSourceType DataSourceType
        {
            get { return MyDataSourceType; }
            set { MyDataSourceType = value; }
        }

        /// <summary>
        /// Gets the library paths in priority order to lookup for assemblies
        /// </summary>
        public static DirectoryInfo[] LibraryPaths
        {
            get { return dicLibraryPaths.ToArray(); }
        }
        
        #endregion

        #region Constructors

        static ObjectManager()
        {
            // Add current app root path
            dicLibraryPaths.Add(new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath)));

            // Fetch and add TIM's Devices path
            DirectoryInfo dirDevices = RegistryHelper.GetInstallPath(Enums.Applications.Devices);
            if (dirDevices != null)
                dicLibraryPaths.Add(dirDevices);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an object by the given type's fullname
        /// </summary>
        /// <typeparam name="T">The expected return format, may be a base class or an interface</typeparam>
        /// <param name="strFullName">The type's fullname</param>
        /// <returns>The created or loaded object</returns>
        public static T Get<T>(string strFullName)
        {
            return Get<T>(strFullName, null);
        }

        /// <summary>
        /// Gets an object by the given type's fullname and passes the parameter to its constructor
        /// </summary>
        /// <typeparam name="T">The expected return format, may be a base class or an interface</typeparam>
        /// <param name="strFullName">The type's fullname</param>
        /// <param name="objParams">The paramters to pass to the constructor</param>
        /// <returns>The created or loaded object</returns>
        public static T Get<T>(string strFullName, params object[] objParams)
        {
            CheckTypeCatalog();

            if (MyTypeCatalog.IsSingleton(strFullName))
            {
                if (!dicSingletons.ContainsKey(strFullName))
                    dicSingletons.Add(strFullName, Create<T>(strFullName, objParams));

                return (T)dicSingletons[strFullName];
            }
            else
                return Create<T>(strFullName, objParams);
        }

        /// <summary>
        /// Creates an instance of a type using the given constructor parameters
        /// </summary>
        /// <typeparam name="T">The expected return format, may be a base class or an interface</typeparam>
        /// <param name="strFullName">The type's fullname</param>
        /// <param name="objParams">The paramters to pass to the constructor</param>
        /// <returns>The created instance</returns>
        private static T Create<T>(string strFullName, params object[] objParams)
        {
            Assembly MyAssembly;
            try
            {
                MyAssembly = MyTypeCatalog.GetAssembly(strFullName);
            }
            catch (FileNotFoundException ex)
            {
                throw new AssemblyMissingException(
                    string.Format("Assembly '{0}' is missing!", strFullName), ex);
            }
            Type MyType = MyTypeCatalog.GetType(strFullName);

            if (objParams == null)
                return InstanceBuilder.New<T>(MyAssembly, MyType);
            else
                return InstanceBuilder.New<T>(MyAssembly, MyType, objParams);
        }

        /// <summary>
        /// Checks if the typecatalog was initialized before the first usage and creates it if necessary
        /// </summary>
        private static void CheckTypeCatalog()
        {
            if (MyTypeCatalog == null)
            {
                switch (MyDataSourceType)
                {
                    case Enums.DataSourceType.XML:
                        MyTypeCatalog = new TypeCatalog(Path.GetDirectoryName(Application.ExecutablePath) + @"\catalog.xml");
                        break;

                    case Enums.DataSourceType.DB:
                        MyTypeCatalog = new TypeCatalog();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        #endregion
    }
}