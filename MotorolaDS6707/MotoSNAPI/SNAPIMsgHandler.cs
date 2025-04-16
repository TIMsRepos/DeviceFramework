using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TIM.Devices.MotorolaDS6707.MotoSNAPI
{
    internal class SnapiMsgHandler : System.Windows.Forms.Control
    {
        public delegate void MsgHandler(ref System.Windows.Forms.Message msg);
        public event MsgHandler SnapiMsg;

        unsafe protected override void WndProc(ref System.Windows.Forms.Message msg)
        {
            if (SnapiMsg != null) SnapiMsg(ref msg);
            base.WndProc(ref msg);
        }
        public SnapiMsgHandler()
        {
        }
    }
}
