using System.Windows.Forms;

namespace TIM.Devices.Framework.Common.Input
{
    public class ExtendedMouseEventArgs : MouseEventArgs
    {
        public Control Control { get; private set; }
        public Keys Modifiers { get; private set; }

        public ExtendedMouseEventArgs(MouseButtons MyMouseButtons, int intClicks, int intX, int intY, int intDelta, Control MyControl, Keys MyModifiers)
            : base(MyMouseButtons, intClicks, intX, intY, intDelta)
        {
            Control = MyControl;
            Modifiers = MyModifiers;
        }
    }
}