using System;

namespace TIM.Devices.Framework.Common.Settings
{
    public class ConfigStringDescription
    {
        /// <summary>
        /// The case-sensitive name of the value
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of the value
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Defines whether this value is required
        /// </summary>
        public bool Required { get; private set; }

        /// <summary>
        /// Defines whether this value is identifing
        /// </summary>
        public bool Identifier { get; private set; }

        public ConfigStringDescription(string strName, Type MyType, bool blnRequired, bool blnIdentifier)
        {
            Name = strName;
            Type = MyType;
            Required = blnRequired;
            Identifier = blnIdentifier;
        }
    }
}