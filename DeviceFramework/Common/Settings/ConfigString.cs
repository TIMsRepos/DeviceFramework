using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TIM.Devices.Framework.Common.Settings
{
    public class ConfigString : IComparable<ConfigString>
    {
        private Dictionary<string, string> dicValues;
        private char chrNameValueDelimiter;
        private char chrPairDelimiter;
        private ConfigStringDescription[] MyConfigStringDescriptions;

        public string this[string strName]
        {
            get { return dicValues[strName.ToUpper()]; }
            set
            {
                if (dicValues.ContainsKey(strName.ToUpper()))
                    dicValues[strName.ToUpper()] = value;
                else
                    dicValues.Add(strName.ToUpper(), value);
            }
        }

        public bool IsEmpty
        {
            get { return dicValues.Count < 1; }
        }

        public ConfigString(char chrNameValueDelimiter, char chrPairDelimiter, ConfigStringDescription[] MyConfigStringDescriptions)
        {
            this.chrNameValueDelimiter = chrNameValueDelimiter;
            this.chrPairDelimiter = chrPairDelimiter;
            this.MyConfigStringDescriptions = MyConfigStringDescriptions;

            dicValues = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            StringBuilder sbBuilder = new StringBuilder();

            foreach (KeyValuePair<string, string> MyPair in dicValues)
                sbBuilder.AppendFormat("{0}{1}{2}{3}", MyPair.Key, chrNameValueDelimiter, MyPair.Value.ToString(), chrPairDelimiter);

            return sbBuilder.ToString();
        }

        public static ConfigString Parse(string strConfigString, char chrNameValueDelimiter, char chrPairDelimiter, ConfigStringDescription[] MyConfigStringDescriptions)
        {
            ConfigString MyConfigString = new ConfigString(chrNameValueDelimiter, chrPairDelimiter, MyConfigStringDescriptions);

            if (strConfigString.Trim().Length > 0)
            {
                string[] strPairs = strConfigString.Split(new char[] { chrPairDelimiter });
                foreach (string strPair in strPairs)
                {
                    string[] strNameValue = strPair.Split(new char[] { chrNameValueDelimiter });
                    MyConfigString[strNameValue[0].ToUpper()] = strNameValue[1];
                }
            }

            return MyConfigString;
        }

        public int CompareTo(ConfigString MyOtherConfigString)
        {
            bool blnAllEqual;

            // All equal => 0
            blnAllEqual = true;
            foreach (KeyValuePair<string, string> MyPair in this.dicValues)
            {
                try
                {
                    if (this[MyPair.Key].CompareTo(MyOtherConfigString[MyPair.Key]) != 0)
                    {
                        blnAllEqual = false;
                        break;
                    }
                }
                catch (KeyNotFoundException)
                {
                    blnAllEqual = false;
                    break;
                }
            }
            if (blnAllEqual && this.dicValues.Count == MyOtherConfigString.dicValues.Count)
                return 0;

            // Identifier equal => 1
            blnAllEqual = true;
            foreach (ConfigStringDescription MyDesc in MyConfigStringDescriptions)
            {
                if (MyDesc.Identifier)
                {
                    try
                    {
                        if (this[MyDesc.Name].CompareTo(MyOtherConfigString[MyDesc.Name]) != 0)
                        {
                            blnAllEqual = false;
                            break;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        blnAllEqual = false;
                        break;
                    }
                }
            }
            if (blnAllEqual)
                return 1;

            return -1;
        }

        public bool EntryExists(string name)
        {
            var entry = dicValues.FirstOrDefault(t => t.Key == name);

            return entry.Key != null;
        }
    }
}