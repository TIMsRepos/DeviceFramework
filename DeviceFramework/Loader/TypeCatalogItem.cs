using System;

namespace TIM.Devices.Framework.Loader
{
    internal class TypeCatalogItem
    {
        #region Members

        private string strTypeFullName;

        #endregion

        #region Getter & Setter

        public AssemblyItem AssemblyItem { get; private set; }
        public bool IsSingleton { get; private set; }

        #endregion

        #region Constructors

        public TypeCatalogItem(AssemblyItem MyAssemblyItem, string strTypeFullName, bool blnSingleton)
        {
            this.strTypeFullName = strTypeFullName;
            AssemblyItem = MyAssemblyItem;
            IsSingleton = blnSingleton;
        }

        #endregion
    }
}