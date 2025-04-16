namespace TIMs_Devices
{
    partial class FrmDevices
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDevices));
            this.MyNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.MyContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnRestart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnClose = new System.Windows.Forms.ToolStripMenuItem();
            this.MyContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MyNotifyIcon
            // 
            this.MyNotifyIcon.ContextMenuStrip = this.MyContextMenuStrip;
            this.MyNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("MyNotifyIcon.Icon")));
            this.MyNotifyIcon.Text = "TIM\'s Geräte";
            this.MyNotifyIcon.Visible = true;
            this.MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MyNotifyIcon_MouseDoubleClick);
            // 
            // MyContextMenuStrip
            // 
            this.MyContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnRestart,
            this.mnClose});
            this.MyContextMenuStrip.Name = "MyContextMenuStrip";
            this.MyContextMenuStrip.Size = new System.Drawing.Size(121, 48);
            // 
            // mnRestart
            // 
            this.mnRestart.Name = "mnRestart";
            this.mnRestart.Size = new System.Drawing.Size(120, 22);
            this.mnRestart.Text = "Neustart";
            this.mnRestart.Visible = false;
            this.mnRestart.Click += new System.EventHandler(this.mnRestart_Click);
            // 
            // mnClose
            // 
            this.mnClose.Name = "mnClose";
            this.mnClose.Size = new System.Drawing.Size(120, 22);
            this.mnClose.Text = "Beenden";
            this.mnClose.Click += new System.EventHandler(this.mnClose_Click);
            // 
            // frmDevices
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(135, 98);
            this.Name = "FrmDevices";
            this.ShowInTaskbar = false;
            this.Text = "frmDummy";
            this.Load += new System.EventHandler(this.frmDevices_Load);
            this.Shown += new System.EventHandler(this.frmDummy_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDummy_FormClosing);
            this.MyContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon MyNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip MyContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnClose;
        private System.Windows.Forms.ToolStripMenuItem mnRestart;
    }
}