using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIM.Devices.KeyboardListener
{
    public class RingBuffer
    {
        #region Fields

        private LinkedList<char> _lstBuffer;

        #endregion Fields
        
        #region Constructors

        public RingBuffer()
        {
            _lstBuffer = new LinkedList<char>();
        }

        #endregion Constructors

        #region Methods

        public void Append(char c)
        {
            _lstBuffer.AddLast(c);
            if (_lstBuffer.Count > 37)
                _lstBuffer.RemoveFirst();
        }

        public void Clear()
        {
            _lstBuffer.Clear();
        }

        /// <summary>
        /// Returns true if the stored content matches the given regex.
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public bool IsMatch(Regex regex)
        {
            return regex != null && regex.Match(ToString()).Success;
        }

        public string GetIDWhichFitsRegex(Regex regex)
        {
            if (regex == null)
                return "";
            return !IsMatch(regex) ? null : ToString();
        }

        public override string ToString()
        {
            return new string(_lstBuffer.ToArray());
        }

        #endregion Methods
    }
}