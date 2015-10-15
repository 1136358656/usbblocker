namespace USBBlocker
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.CheckDeviceStatus_Lable = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CheckDeviceStatus_Lable
            // 
            this.CheckDeviceStatus_Lable.AutoSize = true;
            this.CheckDeviceStatus_Lable.Location = new System.Drawing.Point(27, 28);
            this.CheckDeviceStatus_Lable.Name = "CheckDeviceStatus_Lable";
            this.CheckDeviceStatus_Lable.Size = new System.Drawing.Size(55, 15);
            this.CheckDeviceStatus_Lable.TabIndex = 0;
            this.CheckDeviceStatus_Lable.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Controls.Add(this.CheckDeviceStatus_Lable);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CheckDeviceStatus_Lable;
    }
}

