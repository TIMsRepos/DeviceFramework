using System;

namespace TIM.Devices.Framework
{
    [global::System.Serializable]
    public class AssemblyMissingException : FrameworkException
    {
        public AssemblyMissingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}