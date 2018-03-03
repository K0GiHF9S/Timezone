namespace Timezone
{
    partial class TimeZoneSetting
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.timeZoneComboBox = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timeZoneComboBox
            // 
            this.timeZoneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeZoneComboBox.FormattingEnabled = true;
            this.timeZoneComboBox.IntegralHeight = false;
            this.timeZoneComboBox.Location = new System.Drawing.Point(32, 12);
            this.timeZoneComboBox.MaxDropDownItems = 20;
            this.timeZoneComboBox.Name = "timeZoneComboBox";
            this.timeZoneComboBox.Size = new System.Drawing.Size(206, 20);
            this.timeZoneComboBox.TabIndex = 0;
            // 
            // ok
            // 
            this.okButton.Location = new System.Drawing.Point(191, 67);
            this.okButton.Name = "ok";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // TimeZoneSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 129);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.timeZoneComboBox);
            this.Name = "TimeZoneSetting";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.TimeZoneSetting_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox timeZoneComboBox;
        private System.Windows.Forms.Button okButton;
    }
}

