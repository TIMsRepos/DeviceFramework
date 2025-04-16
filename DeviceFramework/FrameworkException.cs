using System;

namespace TIM.Devices.Framework
{
    public class FrameworkException : ApplicationException
    {
        /// <summary>
        /// Creates a new empty FrameworkException
        /// </summary>
        public FrameworkException()
            : base()
        {
        }

        /// <summary>
        /// Creates a new FrameworkException with an error message
        /// </summary>
        /// <param name="strMessage">The error message</param>
        public FrameworkException(string strMessage)
            : base(strMessage)
        {
        }

        /// <summary>
        /// Creates a new FrameworkException with an error message and an inner exception
        /// </summary>
        /// <param name="strMessage">The error message</param>
        /// <param name="exInner">The inner exception</param>
        public FrameworkException(string strMessage, Exception exInner)
            : base(strMessage, exInner)
        {
        }
    }
}