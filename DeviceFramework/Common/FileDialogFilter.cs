using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TIM.Devices.Framework.Common
{
    public class FileDialogFilter
    {
        private Dictionary<string, string[]> dicFilters;

        public FileDialogFilter()
        {
            dicFilters = new Dictionary<string, string[]>();
        }

        public void Add(string strDescription, params string[] strExtensions)
        {
            dicFilters.Add(strDescription, strExtensions);
        }

        public override string ToString()
        {
            // Style: Text files (*.txt)|*.txt
            StringBuilder sbFilter = new StringBuilder();
            foreach (KeyValuePair<string, string[]> MyFilterPair in dicFilters)
            {
                string strExtensions = string.Join(";", MyFilterPair.Value.Select(e => string.Format("*.{0}", e.ToLower())).ToArray());
                sbFilter.AppendFormat("{0} ({1})|{1}|", MyFilterPair.Key, strExtensions);
            }
            return sbFilter.ToString().Trim('|');
        }
    }
}