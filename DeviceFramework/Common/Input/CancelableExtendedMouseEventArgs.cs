using System.Windows.Forms;

namespace TIM.Devices.Framework.Common.Input
{
    public class CancelableExtendedMouseEventArgs : ExtendedMouseEventArgs
    {
        public bool Cancel { get; set; }

        public CancelableExtendedMouseEventArgs(MouseButtons MyMouseButtons, int intClicks, int intX, int intY, int intDelta, Control MyControl, Keys MyModifiers)
            : base(MyMouseButtons, intClicks, intX, intY, intDelta, MyControl, MyModifiers)
        {
            Cancel = false;
        }
    }
}