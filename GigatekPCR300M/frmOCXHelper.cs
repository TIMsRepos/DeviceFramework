using System.Windows.Forms;

namespace TIM.Devices.GigatekPCR300M
{
    public partial class frmOCXHelper : Form
    {
        public AxPCR300XLib.AxPCR300x AxPCR300x
        {
            get { return axPCR300x1; }
        }

        public frmOCXHelper()
        {
            InitializeComponent();
        }
    }
}