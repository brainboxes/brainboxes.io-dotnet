namespace Brainboxes.IO.Samples
{
    partial class EDConsole
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EDConsole));
            this.sendCommandButton = new System.Windows.Forms.Button();
            this.commandTextBox = new System.Windows.Forms.ComboBox();
            this.terminal = new System.Windows.Forms.RichTextBox();
            this.tcpSettingsPanel = new System.Windows.Forms.Panel();
            this.portNumberComboBox = new System.Windows.Forms.ComboBox();
            this.tcpConnectButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ipAddressTextBox4 = new System.Windows.Forms.TextBox();
            this.ipAddressTextBox3 = new System.Windows.Forms.TextBox();
            this.ipAddressTextBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ipAddressTextBox1 = new System.Windows.Forms.TextBox();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.serialSettingsPanel = new System.Windows.Forms.Panel();
            this.serialConnectButton = new System.Windows.Forms.Button();
            this.comPortComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.sendCommandPanel = new System.Windows.Forms.Panel();
            this.modbusLabel2 = new System.Windows.Forms.Label();
            this.modbusLabel = new System.Windows.Forms.Label();
            this.modbusMbapLabel = new System.Windows.Forms.Label();
            this.protocolLabel = new System.Windows.Forms.Label();
            this.tcpSettingsPanel.SuspendLayout();
            this.serialSettingsPanel.SuspendLayout();
            this.sendCommandPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // sendCommandButton
            // 
            this.sendCommandButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sendCommandButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendCommandButton.Location = new System.Drawing.Point(372, 21);
            this.sendCommandButton.Name = "sendCommandButton";
            this.sendCommandButton.Size = new System.Drawing.Size(166, 32);
            this.sendCommandButton.TabIndex = 9;
            this.sendCommandButton.Text = "Send Command";
            this.sendCommandButton.UseVisualStyleBackColor = true;
            this.sendCommandButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // commandTextBox
            // 
            this.commandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandTextBox.AutoCompleteCustomSource.AddRange(new string[] {
            "\"$01C\"",
            "\"@0101\""});
            this.commandTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.commandTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commandTextBox.Location = new System.Drawing.Point(180, 24);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(188, 32);
            this.commandTextBox.TabIndex = 8;
            this.commandTextBox.Text = "$01M";
            this.commandTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ASCIICommand_KeyPress);
            // 
            // terminal
            // 
            this.terminal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.terminal.BackColor = System.Drawing.Color.Black;
            this.terminal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.terminal.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.terminal.ForeColor = System.Drawing.Color.White;
            this.terminal.Location = new System.Drawing.Point(12, 164);
            this.terminal.Name = "terminal";
            this.terminal.ReadOnly = true;
            this.terminal.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.terminal.Size = new System.Drawing.Size(563, 258);
            this.terminal.TabIndex = 100;
            this.terminal.Text = "";
            // 
            // tcpSettingsPanel
            // 
            this.tcpSettingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcpSettingsPanel.Controls.Add(this.portNumberComboBox);
            this.tcpSettingsPanel.Controls.Add(this.tcpConnectButton);
            this.tcpSettingsPanel.Controls.Add(this.label9);
            this.tcpSettingsPanel.Controls.Add(this.label8);
            this.tcpSettingsPanel.Controls.Add(this.label7);
            this.tcpSettingsPanel.Controls.Add(this.ipAddressTextBox4);
            this.tcpSettingsPanel.Controls.Add(this.ipAddressTextBox3);
            this.tcpSettingsPanel.Controls.Add(this.ipAddressTextBox2);
            this.tcpSettingsPanel.Controls.Add(this.label3);
            this.tcpSettingsPanel.Controls.Add(this.label2);
            this.tcpSettingsPanel.Controls.Add(this.label1);
            this.tcpSettingsPanel.Controls.Add(this.ipAddressTextBox1);
            this.tcpSettingsPanel.Location = new System.Drawing.Point(25, 21);
            this.tcpSettingsPanel.Name = "tcpSettingsPanel";
            this.tcpSettingsPanel.Size = new System.Drawing.Size(540, 61);
            this.tcpSettingsPanel.TabIndex = 4;
            // 
            // portNumberComboBox
            // 
            this.portNumberComboBox.FormattingEnabled = true;
            this.portNumberComboBox.Items.AddRange(new object[] {
            "9500",
            "502"});
            this.portNumberComboBox.Location = new System.Drawing.Point(248, 28);
            this.portNumberComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.portNumberComboBox.Name = "portNumberComboBox";
            this.portNumberComboBox.Size = new System.Drawing.Size(55, 21);
            this.portNumberComboBox.TabIndex = 4;
            this.portNumberComboBox.Text = "9500";
            // 
            // tcpConnectButton
            // 
            this.tcpConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tcpConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcpConnectButton.Location = new System.Drawing.Point(306, 20);
            this.tcpConnectButton.Name = "tcpConnectButton";
            this.tcpConnectButton.Size = new System.Drawing.Size(231, 32);
            this.tcpConnectButton.TabIndex = 5;
            this.tcpConnectButton.Text = "Connect Over TCP";
            this.tcpConnectButton.UseVisualStyleBackColor = true;
            this.tcpConnectButton.Click += new System.EventHandler(this.tcpConnectButton_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(164, 32);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = ".";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(116, 32);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(10, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = ".";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(68, 32);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(10, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = ".";
            // 
            // ipAddressTextBox4
            // 
            this.ipAddressTextBox4.Location = new System.Drawing.Point(180, 28);
            this.ipAddressTextBox4.Name = "ipAddressTextBox4";
            this.ipAddressTextBox4.Size = new System.Drawing.Size(29, 20);
            this.ipAddressTextBox4.TabIndex = 3;
            this.ipAddressTextBox4.Text = "255";
            this.ipAddressTextBox4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tcpPanel_KeyPress);
            // 
            // ipAddressTextBox3
            // 
            this.ipAddressTextBox3.Location = new System.Drawing.Point(132, 28);
            this.ipAddressTextBox3.Name = "ipAddressTextBox3";
            this.ipAddressTextBox3.Size = new System.Drawing.Size(29, 20);
            this.ipAddressTextBox3.TabIndex = 2;
            this.ipAddressTextBox3.Text = "127";
            this.ipAddressTextBox3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tcpPanel_KeyPress);
            // 
            // ipAddressTextBox2
            // 
            this.ipAddressTextBox2.Location = new System.Drawing.Point(84, 28);
            this.ipAddressTextBox2.Name = "ipAddressTextBox2";
            this.ipAddressTextBox2.Size = new System.Drawing.Size(29, 20);
            this.ipAddressTextBox2.TabIndex = 1;
            this.ipAddressTextBox2.Text = "168";
            this.ipAddressTextBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tcpPanel_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "TCP Settings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP:";
            // 
            // ipAddressTextBox1
            // 
            this.ipAddressTextBox1.Location = new System.Drawing.Point(39, 28);
            this.ipAddressTextBox1.Name = "ipAddressTextBox1";
            this.ipAddressTextBox1.Size = new System.Drawing.Size(29, 20);
            this.ipAddressTextBox1.TabIndex = 0;
            this.ipAddressTextBox1.Text = "192";
            this.ipAddressTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tcpPanel_KeyPress);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.disconnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disconnectButton.Location = new System.Drawing.Point(24, 22);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(540, 127);
            this.disconnectButton.TabIndex = 10;
            this.disconnectButton.Text = "Disconnect Brainboxes ED Device";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Visible = false;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // serialSettingsPanel
            // 
            this.serialSettingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serialSettingsPanel.Controls.Add(this.serialConnectButton);
            this.serialSettingsPanel.Controls.Add(this.comPortComboBox);
            this.serialSettingsPanel.Controls.Add(this.label6);
            this.serialSettingsPanel.Controls.Add(this.label5);
            this.serialSettingsPanel.Location = new System.Drawing.Point(25, 88);
            this.serialSettingsPanel.Name = "serialSettingsPanel";
            this.serialSettingsPanel.Size = new System.Drawing.Size(540, 60);
            this.serialSettingsPanel.TabIndex = 8;
            // 
            // serialConnectButton
            // 
            this.serialConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serialConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serialConnectButton.Location = new System.Drawing.Point(306, 18);
            this.serialConnectButton.Name = "serialConnectButton";
            this.serialConnectButton.Size = new System.Drawing.Size(231, 32);
            this.serialConnectButton.TabIndex = 7;
            this.serialConnectButton.Text = "Connect Over Serial";
            this.serialConnectButton.UseVisualStyleBackColor = true;
            this.serialConnectButton.Click += new System.EventHandler(this.serialConnectButton_Click);
            // 
            // comPortComboBox
            // 
            this.comPortComboBox.FormattingEnabled = true;
            this.comPortComboBox.Items.AddRange(new object[] {
            "COM3"});
            this.comPortComboBox.Location = new System.Drawing.Point(63, 27);
            this.comPortComboBox.Name = "comPortComboBox";
            this.comPortComboBox.Size = new System.Drawing.Size(121, 21);
            this.comPortComboBox.TabIndex = 6;
            this.comPortComboBox.DropDown += new System.EventHandler(this.comPortComboBox_DropDown);
            this.comPortComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comPortComboBox_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Com Port:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(4, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 20);
            this.label5.TabIndex = 6;
            this.label5.Text = "Serial Settings";
            // 
            // sendCommandPanel
            // 
            this.sendCommandPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sendCommandPanel.Controls.Add(this.modbusLabel2);
            this.sendCommandPanel.Controls.Add(this.modbusLabel);
            this.sendCommandPanel.Controls.Add(this.modbusMbapLabel);
            this.sendCommandPanel.Controls.Add(this.protocolLabel);
            this.sendCommandPanel.Controls.Add(this.commandTextBox);
            this.sendCommandPanel.Controls.Add(this.sendCommandButton);
            this.sendCommandPanel.Enabled = false;
            this.sendCommandPanel.Location = new System.Drawing.Point(26, 428);
            this.sendCommandPanel.Name = "sendCommandPanel";
            this.sendCommandPanel.Size = new System.Drawing.Size(540, 57);
            this.sendCommandPanel.TabIndex = 10;
            // 
            // modbusLabel2
            // 
            this.modbusLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modbusLabel2.AutoSize = true;
            this.modbusLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modbusLabel2.Location = new System.Drawing.Point(177, 3);
            this.modbusLabel2.Name = "modbusLabel2";
            this.modbusLabel2.Size = new System.Drawing.Size(115, 18);
            this.modbusLabel2.TabIndex = 15;
            this.modbusLabel2.Text = "func addr data";
            this.modbusLabel2.Visible = false;
            // 
            // modbusLabel
            // 
            this.modbusLabel.AutoSize = true;
            this.modbusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modbusLabel.Location = new System.Drawing.Point(6, 3);
            this.modbusLabel.Name = "modbusLabel";
            this.modbusLabel.Size = new System.Drawing.Size(163, 18);
            this.modbusLabel.TabIndex = 14;
            this.modbusLabel.Text = "trans prot length unit";
            this.modbusLabel.Visible = false;
            // 
            // modbusMbapLabel
            // 
            this.modbusMbapLabel.AutoSize = true;
            this.modbusMbapLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modbusMbapLabel.Location = new System.Drawing.Point(5, 28);
            this.modbusMbapLabel.Name = "modbusMbapLabel";
            this.modbusMbapLabel.Size = new System.Drawing.Size(161, 20);
            this.modbusMbapLabel.TabIndex = 13;
            this.modbusMbapLabel.Text = "0000 0000 0000 ff ";
            this.modbusMbapLabel.Visible = false;
            // 
            // protocolLabel
            // 
            this.protocolLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.protocolLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.protocolLabel.Location = new System.Drawing.Point(317, 4);
            this.protocolLabel.Name = "protocolLabel";
            this.protocolLabel.Size = new System.Drawing.Size(220, 19);
            this.protocolLabel.TabIndex = 12;
            this.protocolLabel.Text = "Protocol";
            this.protocolLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // EDConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 497);
            this.Controls.Add(this.sendCommandPanel);
            this.Controls.Add(this.terminal);
            this.Controls.Add(this.serialSettingsPanel);
            this.Controls.Add(this.tcpSettingsPanel);
            this.Controls.Add(this.disconnectButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "EDConsole";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Brainboxes ASCII/ModbusTCP Remote IO Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EDConsole_FormClosing);
            this.tcpSettingsPanel.ResumeLayout(false);
            this.tcpSettingsPanel.PerformLayout();
            this.serialSettingsPanel.ResumeLayout(false);
            this.serialSettingsPanel.PerformLayout();
            this.sendCommandPanel.ResumeLayout(false);
            this.sendCommandPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button sendCommandButton;
        private System.Windows.Forms.ComboBox commandTextBox;
        private System.Windows.Forms.RichTextBox terminal;
        private System.Windows.Forms.Panel tcpSettingsPanel;
        private System.Windows.Forms.TextBox ipAddressTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel serialSettingsPanel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comPortComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel sendCommandPanel;
        private System.Windows.Forms.TextBox ipAddressTextBox4;
        private System.Windows.Forms.TextBox ipAddressTextBox3;
        private System.Windows.Forms.TextBox ipAddressTextBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button tcpConnectButton;
        private System.Windows.Forms.Button serialConnectButton;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.ComboBox portNumberComboBox;
        private System.Windows.Forms.Label protocolLabel;
        private System.Windows.Forms.Label modbusMbapLabel;
        private System.Windows.Forms.Label modbusLabel;
        private System.Windows.Forms.Label modbusLabel2;
    }
}

