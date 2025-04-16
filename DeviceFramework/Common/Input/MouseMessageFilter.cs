using System;
using System.Windows.Forms;

namespace TIM.Devices.Framework.Common.Input
{
    public class MouseMessageFilter : IMessageFilter
    {
        #region Events

        public event EventHandler<CancelableExtendedMouseEventArgs> BeforeFilter;

        public event EventHandler<CancelableExtendedMouseEventArgs> AfterFilter;

        public event EventHandler<ExtendedMouseEventArgs> MouseClick;

        public event EventHandler<ExtendedMouseEventArgs> MouseDown;

        public event EventHandler<ExtendedMouseEventArgs> MouseUp;

        public event EventHandler<ExtendedMouseEventArgs> MouseMove;

        public event EventHandler MouseEnter;

        public event EventHandler MouseLeave;

        #endregion

        #region Constants

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_MOUSELEAVE = 0x02A3;

        private const int MK_CONTROL = 0x0008; // The CTRL key is down.
        private const int MK_LBUTTON = 0x0001; // The left mouse button is down.
        private const int MK_MBUTTON = 0x0010; // The middle mouse button is down.
        private const int MK_RBUTTON = 0x0002; // The right mouse button is down.
        private const int MK_SHIFT = 0x0004; // The SHIFT key is down.
        private const int MK_XBUTTON1 = 0x0020; // The first X button is down.
        private const int MK_XBUTTON2 = 0x0040; // The second X button is down.

        #endregion

        #region Fields

        private bool blnInside;
        private int intLastX;
        private int intLastY;

        #endregion

        #region Constructors

        public MouseMessageFilter()
        {
            blnInside = false;
        }

        #endregion

        #region Methods

        public bool PreFilterMessage(ref Message m)
        {
            Control MyControl = Control.FromChildHandle(m.HWnd);

            MouseButtons MyMouseButtons = GetMouseButtons(m);
            int intX = 0, intY = 0;
            GetCoords(m, ref intX, ref intY);
            Keys MyModifiers = GetModifiers(m);
            CancelableExtendedMouseEventArgs evtCancelArgs = new CancelableExtendedMouseEventArgs(
                MyMouseButtons, 0, intX, intY, 0, MyControl, MyModifiers);

            OnBeforeFilter(evtCancelArgs);
            if (evtCancelArgs.Cancel)
                return true;

            switch (m.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                    {
                        OnMouseDown(new ExtendedMouseEventArgs(MyMouseButtons, 0, intX, intY, 0, MyControl, MyModifiers));
                        break;
                    }
                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                    {
                        OnMouseClick(new ExtendedMouseEventArgs(MyMouseButtons, 1, intX, intY, 0, MyControl, MyModifiers));
                        OnMouseUp(new ExtendedMouseEventArgs(MyMouseButtons, 0, intX, intY, 0, MyControl, MyModifiers));
                        break;
                    }
                case WM_MOUSEMOVE:
                    {
                        if (!blnInside)
                        {
                            OnMouseEnter(EventArgs.Empty);
                            blnInside = true;
                        }

                        if (intX != intLastX || intY != intLastY)
                            OnMouseMove(new ExtendedMouseEventArgs(MouseButtons.None, 0, intX, intY, 0, MyControl, MyModifiers));
                        intLastX = intX;
                        intLastY = intY;
                        break;
                    }
                case WM_MOUSELEAVE:
                    {
                        blnInside = false;

                        OnMouseLeave(EventArgs.Empty);
                        break;
                    }
                default:
                    break;
            }

            evtCancelArgs.Cancel = false;
            OnAfterFilter(evtCancelArgs);
            if (evtCancelArgs.Cancel)
                return true;

            return false;
        }

        private MouseButtons GetMouseButtons(Message MyMsg)
        {
            switch (MyMsg.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                    return MouseButtons.Left;

                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                    return MouseButtons.Right;

                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                    return MouseButtons.Middle;

                default:
                    return MouseButtons.None;
            }
        }

        private Keys GetModifiers(Message MyMsg)
        {
            Keys MyModifiers = Keys.None;

            switch (MyMsg.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                case WM_MOUSEMOVE:
                    {
                        UInt32 intWParam = (UInt32)MyMsg.WParam.ToInt32();
                        if ((intWParam & MK_CONTROL) == MK_CONTROL)
                            MyModifiers |= Keys.Control;
                        if ((intWParam & MK_SHIFT) == MK_SHIFT)
                            MyModifiers |= Keys.Shift;
                        break;
                    }
                default:
                    break;
            }

            return MyModifiers;
        }

        private void GetCoords(Message MyMsg, ref int intX, ref int intY)
        {
            switch (MyMsg.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                case WM_MOUSEMOVE:
                    {
                        UInt32 intLParam = (UInt32)MyMsg.LParam.ToInt32();
                        intX = (UInt16)(intLParam & 0xff);
                        intY = (UInt16)(intLParam >> 16);
                        break;
                    }
                default:
                    break;
            }
        }

        #endregion

        #region Event-Fire

        protected virtual void OnBeforeFilter(CancelableExtendedMouseEventArgs evtArgs)
        {
            EventHandler<CancelableExtendedMouseEventArgs> evtHandler = BeforeFilter;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnAfterFilter(CancelableExtendedMouseEventArgs evtArgs)
        {
            EventHandler<CancelableExtendedMouseEventArgs> evtHandler = AfterFilter;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnMouseLeave(EventArgs evtArgs)
        {
            EventHandler evtHandler = MouseLeave;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnMouseEnter(EventArgs evtArgs)
        {
            EventHandler evtHandler = MouseEnter;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnMouseMove(ExtendedMouseEventArgs evtArgs)
        {
            EventHandler<ExtendedMouseEventArgs> evtHandler = MouseMove;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnMouseDown(ExtendedMouseEventArgs evtArgs)
        {
            EventHandler<ExtendedMouseEventArgs> evtHandler = MouseDown;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnMouseUp(ExtendedMouseEventArgs evtArgs)
        {
            EventHandler<ExtendedMouseEventArgs> evtHandler = MouseUp;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        protected virtual void OnMouseClick(ExtendedMouseEventArgs evtArgs)
        {
            EventHandler<ExtendedMouseEventArgs> evtHandler = MouseClick;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }

        #endregion
    }
}