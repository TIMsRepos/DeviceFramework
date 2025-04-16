using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TIM.Common.CoreStandard;

namespace TIM.Devices.Framework.Loader
{
    internal class AssemblyItem
    {
        #region Members

        private Type[] MyTypes = null;
        private Assembly MyAssembly = null;
        private string strName;

        #endregion

        #region Getter & Setter

        /// <summary>
        /// Gets the assembly itself by using lazy loading
        /// </summary>
        public Assembly Assembly
        {
            get
            {
                if (MyAssembly == null)
                {
                    foreach (DirectoryInfo dirLibraryPath in ObjectManager.LibraryPaths)
                    {
                        string strFullFileName = string.Format(@"{0}{2}{1}.dll", dirLibraryPath.FullName, strName, Path.DirectorySeparatorChar);
                        try
                        {
                            MyAssembly = System.Reflection.Assembly.LoadFile(strFullFileName);
                            break;
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }

                    if (MyAssembly == null)
                        throw new FileNotFoundException();
                }

                return MyAssembly;
            }
        }

        #endregion

        #region Constructors

        static AssemblyItem()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                string[] strParts = args.Name.Split(',');
                string strName = strParts[0].Trim();
                if (strName.EndsWith("resources"))
                    return null;

                string strFullFileName = GetRootPath() + args.Name.Split(new char[] { ',' })[0] + ".dll";
                Assembly MyAssembly = Assembly.LoadFile(strFullFileName);

                return MyAssembly;
            };
        }

        /// <summary>
        /// Creates a new AssemblyItem object without loading the assembly
        /// </summary>
        /// <param name="strName">The name of the assembly</param>
        public AssemblyItem(string strName)
        {
            this.strName = strName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Searches inside the assembly for the given type's fullname
        /// </summary>
        /// <param name="strFullName">Wanted type's fullname</param>
        /// <returns>The found Type object or null of not found</returns>
        public Type GetType(string strFullName)
        {
            if (MyTypes == null)
                MyTypes = Assembly.GetExportedTypes();

            for (int i = 0; i < MyTypes.Length; ++i)
            {
                if (MyTypes[i].FullName == strFullName)
                    return MyTypes[i];
            }

            return null;
        }

        /// <summary>
        /// Gets the TIM application path from registry
        /// </summary>
        /// <returns>TIM's application path</returns>
        private static string GetRootPath()
        {
            switch (ObjectManager.DataSourceType)
            {
                case Enums.DataSourceType.DB:
                case Enums.DataSourceType.XML:
                    return Path.GetDirectoryName(Application.ExecutablePath) + @"\";

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion
    }
}