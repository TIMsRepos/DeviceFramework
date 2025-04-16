using System;
using System.Windows.Forms;

namespace TIMs_Devices
{
    public partial class frmErrors : Form
    {
        public string Errors
        {
            get { return txtErrors.Text; }
            set { txtErrors.Text = value; }
        }

        public frmErrors()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}