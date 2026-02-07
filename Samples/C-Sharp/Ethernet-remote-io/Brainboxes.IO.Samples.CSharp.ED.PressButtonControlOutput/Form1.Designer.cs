namespace Brainboxes.IO.Samples.PressButtonControlOutput
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
            this.toggleButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.currentOutputStateLabel = new System.Windows.Forms.Label();
            this.currentOutputTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ipAddressTextBox = new System.Windows.Forms.TextBox();
            this.errorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // toggleButton
            // 
            this.toggleButton.Enabled = false;
            this.toggleButton.Location = new System.Drawing.Point(6, 99);
            this.toggleButton.Margin = new System.Windows.Forms.Padding(2);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new System.Drawing.Size(122, 35);
            this.toggleButton.TabIndex = 0;
            this.toggleButton.Text = "Click Me!";
            this.toggleButton.UseVisualStyleBackColor = true;
            this.toggleButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 51);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected Output:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 73);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current State:";
            // 
            // currentOutputStateLabel
            // 
            this.currentOutputStateLabel.AutoSize = true;
            this.currentOutputStateLabel.Location = new System.Drawing.Point(108, 73);
            this.currentOutputStateLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.currentOutputStateLabel.Name = "currentOutputStateLabel";
            this.currentOutputStateLabel.Size = new System.Drawing.Size(21, 13);
            this.currentOutputStateLabel.TabIndex = 3;
            this.currentOutputStateLabel.Text = "On";
            this.currentOutputStateLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // currentOutputTextBox
            // 
            this.currentOutputTextBox.Enabled = false;
            this.currentOutputTextBox.Location = new System.Drawing.Point(111, 50);
            this.currentOutputTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.currentOutputTextBox.Name = "currentOutputTextBox";
            this.currentOutputTextBox.Size = new System.Drawing.Size(19, 20);
            this.currentOutputTextBox.TabIndex = 4;
            this.currentOutputTextBox.Text = "0";
            this.currentOutputTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.currentOutputTextBox_KeyUp);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Device:";
            // 
            // ipAddressTextBox
            // 
            this.ipAddressTextBox.Location = new System.Drawing.Point(11, 24);
            this.ipAddressTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.ipAddressTextBox.Name = "ipAddressTextBox";
            this.ipAddressTextBox.Size = new System.Drawing.Size(119, 20);
            this.ipAddressTextBox.TabIndex = 6;
            this.ipAddressTextBox.Text = "YOUR_DEVICE_IP";
            this.ipAddressTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ipAddressTextBox_KeyUp);
            // 
            // errorLabel
            // 
            this.errorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorLabel.AutoSize = true;
            this.errorLabel.ForeColor = System.Drawing.Color.Red;
            this.errorLabel.Location = new System.Drawing.Point(8, 136);
            this.errorLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.errorLabel.MinimumSize = new System.Drawing.Size(120, 50);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(120, 50);
            this.errorLabel.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(135, 193);
            this.Controls.Add(this.errorLabel);
            this.Controls.Add(this.ipAddressTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.currentOutputTextBox);
            this.Controls.Add(this.currentOutputStateLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toggleButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Button Example";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button toggleButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label currentOutputStateLabel;
        private System.Windows.Forms.TextBox currentOutputTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ipAddressTextBox;
        private System.Windows.Forms.Label errorLabel;
    }
}

