namespace TIM.Devices.GigatekPCR300M
{
    partial class frmOCXHelper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOCXHelper));
            this.axPCR300x1 = new AxPCR300XLib.AxPCR300x();
            ((System.ComponentModel.ISupportInitialize)(this.axPCR300x1)).BeginInit();
            this.SuspendLayout();
            // 
            // axPCR300x1
            // 
            this.axPCR300x1.Enabled = true;
            this.axPCR300x1.Location = new System.Drawing.Point(397, 315);
            this.axPCR300x1.Name = "axPCR300x1";
            this.axPCR300x1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axPCR300x1.OcxState")));
            this.axPCR300x1.Size = new System.Drawing.Size(112, 50);
            this.axPCR300x1.TabIndex = 0;
            // 
            // frmHelper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.axPCR300x1);
            this.Name = "frmHelper";
            this.Text = "frmHelper";
            ((System.ComponentModel.ISupportInitialize)(this.axPCR300x1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxPCR300XLib.AxPCR300x axPCR300x1;

    }
}