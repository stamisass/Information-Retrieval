namespace WindowsFormsApplication1
{
    partial class DeleteFile
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
            this.DeleteFilelistBox = new System.Windows.Forms.ListBox();
            this.DeleteFileButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // DeleteFilelistBox
            // 
            this.DeleteFilelistBox.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.DeleteFilelistBox.FormattingEnabled = true;
            this.DeleteFilelistBox.Location = new System.Drawing.Point(12, 12);
            this.DeleteFilelistBox.Name = "DeleteFilelistBox";
            this.DeleteFilelistBox.Size = new System.Drawing.Size(375, 186);
            this.DeleteFilelistBox.TabIndex = 0;
            // 
            // DeleteFileButton
            // 
            this.DeleteFileButton.Location = new System.Drawing.Point(312, 219);
            this.DeleteFileButton.Name = "DeleteFileButton";
            this.DeleteFileButton.Size = new System.Drawing.Size(75, 42);
            this.DeleteFileButton.TabIndex = 1;
            this.DeleteFileButton.Text = "Delete";
            this.DeleteFileButton.UseVisualStyleBackColor = true;
            this.DeleteFileButton.Click += new System.EventHandler(this.DeleteFileButton_Click);
            // 
            // DeleteFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GrayText;
            this.ClientSize = new System.Drawing.Size(400, 273);
            this.Controls.Add(this.DeleteFileButton);
            this.Controls.Add(this.DeleteFilelistBox);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.GrayText;
            this.Name = "DeleteFile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DeleteFile";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox DeleteFilelistBox;
        private System.Windows.Forms.Button DeleteFileButton;
    }
}