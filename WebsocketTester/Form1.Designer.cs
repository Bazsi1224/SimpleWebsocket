
namespace WebsocketTester
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ClientLog = new System.Windows.Forms.TextBox();
            this.ServerLog = new System.Windows.Forms.TextBox();
            this.ServerField = new System.Windows.Forms.TextBox();
            this.ClientField = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.ClientAddress = new System.Windows.Forms.TextBox();
            this.ServerAddress = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ClientLog
            // 
            this.ClientLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ClientLog.Location = new System.Drawing.Point(12, 41);
            this.ClientLog.Multiline = true;
            this.ClientLog.Name = "ClientLog";
            this.ClientLog.Size = new System.Drawing.Size(620, 615);
            this.ClientLog.TabIndex = 0;
            // 
            // ServerLog
            // 
            this.ServerLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerLog.Location = new System.Drawing.Point(638, 41);
            this.ServerLog.Multiline = true;
            this.ServerLog.Name = "ServerLog";
            this.ServerLog.Size = new System.Drawing.Size(620, 615);
            this.ServerLog.TabIndex = 1;
            // 
            // ServerField
            // 
            this.ServerField.Enabled = false;
            this.ServerField.Location = new System.Drawing.Point(638, 662);
            this.ServerField.Name = "ServerField";
            this.ServerField.Size = new System.Drawing.Size(620, 23);
            this.ServerField.TabIndex = 2;
            this.ServerField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ServerField_KeyDown);
            // 
            // ClientField
            // 
            this.ClientField.Enabled = false;
            this.ClientField.Location = new System.Drawing.Point(12, 662);
            this.ClientField.Name = "ClientField";
            this.ClientField.Size = new System.Drawing.Size(620, 23);
            this.ClientField.TabIndex = 3;
            this.ClientField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ClientField_KeyDown);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(557, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ClientAddress
            // 
            this.ClientAddress.Location = new System.Drawing.Point(12, 12);
            this.ClientAddress.Name = "ClientAddress";
            this.ClientAddress.Size = new System.Drawing.Size(539, 23);
            this.ClientAddress.TabIndex = 5;
            // 
            // ServerAddress
            // 
            this.ServerAddress.Enabled = false;
            this.ServerAddress.Location = new System.Drawing.Point(638, 12);
            this.ServerAddress.Name = "ServerAddress";
            this.ServerAddress.Size = new System.Drawing.Size(620, 23);
            this.ServerAddress.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1266, 688);
            this.Controls.Add(this.ServerAddress);
            this.Controls.Add(this.ClientAddress);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ClientField);
            this.Controls.Add(this.ServerField);
            this.Controls.Add(this.ServerLog);
            this.Controls.Add(this.ClientLog);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ClientLog;
        private System.Windows.Forms.TextBox ServerLog;
        private System.Windows.Forms.TextBox ServerField;
        private System.Windows.Forms.TextBox ClientField;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox ClientAddress;
        private System.Windows.Forms.TextBox ServerAddress;
    }
}

