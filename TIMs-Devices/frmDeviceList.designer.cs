namespace TIMs_Devices
{
    partial class FrmDeviceList
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lbxDevices = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.sbpClients = new System.Windows.Forms.StatusBarPanel();
            this.sbpVersion = new System.Windows.Forms.StatusBarPanel();
            this.btnMore = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sbpClients)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sbpVersion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 31);
            this.label1.TabIndex = 5;
            this.label1.Text = "TIM\'s Devices";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(12, 53);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(133, 16);
            this.lblVersion.TabIndex = 6;
            this.lblVersion.Text = "Version: 1.23.4567";
            // 
            // lbxDevices
            // 
            this.lbxDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxDevices.FormattingEnabled = true;
            this.lbxDevices.Location = new System.Drawing.Point(3, 16);
            this.lbxDevices.Name = "lbxDevices";
            this.lbxDevices.Size = new System.Drawing.Size(350, 100);
            this.lbxDevices.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lbxDevices);
            this.groupBox1.Location = new System.Drawing.Point(12, 101);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(356, 119);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Geladene Geräte";
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 255);
            this.statusBar.Name = "statusBar";
            this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.sbpClients,
            this.sbpVersion});
            this.statusBar.ShowPanels = true;
            this.statusBar.Size = new System.Drawing.Size(380, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 7;
            this.statusBar.Text = "statusBar1";
            // 
            // sbpClients
            // 
            this.sbpClients.Name = "sbpClients";
            this.sbpClients.Text = "Clients: 0";
            this.sbpClients.Width = 250;
            // 
            // sbpVersion
            // 
            this.sbpVersion.Name = "sbpVersion";
            this.sbpVersion.Text = "Version: 1.23.4567";
            this.sbpVersion.Width = 150;
            // 
            // btnMore
            // 
            this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMore.Location = new System.Drawing.Point(12, 226);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(133, 23);
            this.btnMore.TabIndex = 1;
            this.btnMore.Text = "Weitere Informationen";
            this.btnMore.UseVisualStyleBackColor = true;
            this.btnMore.Click += new System.EventHandler(this.btnMore_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClose.Location = new System.Drawing.Point(235, 226);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(133, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Schließen";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::TIMs_Devices.Properties.Resources.tim;
            this.pictureBox1.Location = new System.Drawing.Point(211, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(157, 78);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // frmDeviceList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 277);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDeviceList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TIM\'s Devices";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDeviceList_FormClosing);
            this.Shown += new System.EventHandler(this.frmDeviceList_Shown);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sbpClients)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sbpVersion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.ListBox lbxDevices;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.StatusBar statusBar;
        private System.Windows.Forms.StatusBarPanel sbpClients;
        private System.Windows.Forms.StatusBarPanel sbpVersion;
        private System.Windows.Forms.Button btnMore;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}