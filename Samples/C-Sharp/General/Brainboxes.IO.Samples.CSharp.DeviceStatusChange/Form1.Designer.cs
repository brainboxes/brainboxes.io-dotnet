namespace DeviceStatusChange
{
    partial class Form1
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
            this.connectButton = new System.Windows.Forms.Button();
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.isAvailableCheckBox = new System.Windows.Forms.CheckBox();
            this.isConnectedCheckBox = new System.Windows.Forms.CheckBox();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lastActionLabel = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F);
            this.connectButton.Location = new System.Drawing.Point(24, 504);
            this.connectButton.Margin = new System.Windows.Forms.Padding(6);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(518, 90);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // ipTextBox
            // 
            this.ipTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ipTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F);
            this.ipTextBox.Location = new System.Drawing.Point(316, 402);
            this.ipTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(694, 87);
            this.ipTextBox.TabIndex = 3;
            this.ipTextBox.Text = "192.168.127.254";
            this.ipTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ipTextBox_KeyUp);
            // 
            // isAvailableCheckBox
            // 
            this.isAvailableCheckBox.AutoSize = true;
            this.isAvailableCheckBox.Checked = true;
            this.isAvailableCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isAvailableCheckBox.Enabled = false;
            this.isAvailableCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.isAvailableCheckBox.Location = new System.Drawing.Point(24, 23);
            this.isAvailableCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.isAvailableCheckBox.Name = "isAvailableCheckBox";
            this.isAvailableCheckBox.Size = new System.Drawing.Size(406, 83);
            this.isAvailableCheckBox.TabIndex = 4;
            this.isAvailableCheckBox.Text = "IsAvailable";
            this.isAvailableCheckBox.UseVisualStyleBackColor = true;
            // 
            // isConnectedCheckBox
            // 
            this.isConnectedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.isConnectedCheckBox.AutoSize = true;
            this.isConnectedCheckBox.Enabled = false;
            this.isConnectedCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.isConnectedCheckBox.Location = new System.Drawing.Point(555, 23);
            this.isConnectedCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.isConnectedCheckBox.Name = "isConnectedCheckBox";
            this.isConnectedCheckBox.Size = new System.Drawing.Size(459, 83);
            this.isConnectedCheckBox.TabIndex = 5;
            this.isConnectedCheckBox.Text = "IsConnected";
            this.isConnectedCheckBox.UseVisualStyleBackColor = true;
            // 
            // disconnectButton
            // 
            this.disconnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.disconnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F);
            this.disconnectButton.Location = new System.Drawing.Point(554, 504);
            this.disconnectButton.Margin = new System.Windows.Forms.Padding(6);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(460, 90);
            this.disconnectButton.TabIndex = 6;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 142);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 79);
            this.label1.TabIndex = 7;
            this.label1.Text = "Log:";
            // 
            // lastActionLabel
            // 
            this.lastActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lastActionLabel.Location = new System.Drawing.Point(24, 223);
            this.lastActionLabel.Margin = new System.Windows.Forms.Padding(6);
            this.lastActionLabel.Multiline = true;
            this.lastActionLabel.Name = "lastActionLabel";
            this.lastActionLabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.lastActionLabel.Size = new System.Drawing.Size(986, 164);
            this.lastActionLabel.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(24, 408);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(282, 79);
            this.label2.TabIndex = 10;
            this.label2.Text = "IP/COM";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 606);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lastActionLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.isConnectedCheckBox);
            this.Controls.Add(this.isAvailableCheckBox);
            this.Controls.Add(this.ipTextBox);
            this.Controls.Add(this.connectButton);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "Form1";
            this.Text = "Connection Status Test";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.CheckBox isAvailableCheckBox;
        private System.Windows.Forms.CheckBox isConnectedCheckBox;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox lastActionLabel;
        private System.Windows.Forms.Label label2;
    }
}

