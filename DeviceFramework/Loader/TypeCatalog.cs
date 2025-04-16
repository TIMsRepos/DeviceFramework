using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using TIM.Devices.Framework.Database;

namespace TIM.Devices.Framework.Loader
{
    public class TypeCatalog
    {
        #region Fields

        private readonly Dictionary<string, TypeCatalogItem> _types;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new TypeCatalog and reads the DB
        /// </summary>
        public TypeCatalog()
        {
            _types = new Dictionary<string, TypeCatalogItem>();

            AssemblyType[] MyAssemblyTypes =
                (from MyAssemblyType in DBContext.DataContextRead.AssemblyTypes
                 orderby MyAssemblyType.Assembly
                 select MyAssemblyType).ToArray();
            string[] MyAssemblies =
                (from MyAssembly in MyAssemblyTypes
                 group MyAssembly by MyAssembly.Assembly into MyAssemblyGroup
                 select MyAssemblyGroup.Key).ToArray();

            AssemblyType[] MyFilteredAssemblyTypes;
            AssemblyItem MyAssemblyItem;
            string strTypeFullName;
            TypeCatalogItem MyTypeCatalogItem;
            foreach (string MyAssembly in MyAssemblies)
            {
                MyFilteredAssemblyTypes =
                    (from MyAssemblyType in MyAssemblyTypes
                     where MyAssemblyType.Assembly == MyAssembly
                     select MyAssemblyType).ToArray();

                MyAssemblyItem = new AssemblyItem(MyAssembly);
                foreach (AssemblyType MyAssemblyType in MyFilteredAssemblyTypes)
                {
                    strTypeFullName = MyAssemblyType.Namespace + "." + MyAssemblyType.Name;
                    MyTypeCatalogItem = new TypeCatalogItem(MyAssemblyItem, strTypeFullName, MyAssemblyType.Singleton);
                    _types.Add(strTypeFullName, MyTypeCatalogItem);
                }
            }
        }

        /// <summary>
        /// Creates a new TypeCatalog and reads the file and parses the xml code
        /// </summary>
        /// <param name="fullFileName">The full file name of the xml catalog file</param>
        public TypeCatalog(string fullFileName)
        {
            if (!File.Exists(fullFileName))
                throw new FrameworkException("Catalog file is missing");

            _types = new Dictionary<string, TypeCatalogItem>();

            XmlDocument docXml = new XmlDocument();
            docXml.Load(fullFileName);

            XmlElement elmCatalog = docXml.DocumentElement;
            XmlElement elmAssemblies = elmCatalog["Assemblies"];

            string strTypeFullName;
            TypeCatalogItem MyTypeCatalogItem;
            foreach (XmlElement elmAssembly in elmAssemblies.ChildNodes)
            {
                if (elmAssembly.Name != "Assembly")
                    throw new FrameworkException("Invalid element in assembly collection");

                AssemblyItem MyAssemblyItem = new AssemblyItem(elmAssembly.Attributes["Name"].Value);
                XmlElement elmAssemblyTypes = elmAssembly["Assembly.Types"];
                foreach (XmlElement elmType in elmAssemblyTypes.ChildNodes)
                {
                    if (elmType.Name != "Type")
                        throw new FrameworkException("Invalid element in type collection");

                    strTypeFullName = elmType.Attributes["Namespace"].Value + "." + elmType.Attributes["Name"].Value;
                    MyTypeCatalogItem = new TypeCatalogItem(MyAssemblyItem, strTypeFullName, bool.Parse(elmType.Attributes["Singleton"].Value));
                    _types.Add(strTypeFullName, MyTypeCatalogItem);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the assembly that defines the type with the given fullname
        /// </summary>
        /// <param name="fullName">The type's fullname to lookup the assembly</param>
        /// <returns>The assembly defining the requested type</returns>
        public Assembly GetAssembly(string fullName)
        {
            if (!_types.ContainsKey(fullName))
                throw new FrameworkException(string.Format("Requested type '{0}' isn't available", fullName));
            return _types[fullName].AssemblyItem.Assembly;
        }

        public Type GetType(string fullName)
        {
            return _types[fullName].AssemblyItem.GetType(fullName);
        }

        public bool IsSingleton(string fullName)
        {
            return _types[fullName].IsSingleton;
        }

        #endregion
    }
}