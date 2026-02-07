namespace Brainboxes.IO.Samples.InputCounter
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dailyCountLabel = new System.Windows.Forms.Label();
            this.weeklyCountLabel = new System.Windows.Forms.Label();
            this.dailyCountValueLabel = new System.Windows.Forms.Label();
            this.weeklyCountValueLabel = new System.Windows.Forms.Label();
            this.resetDailyButton = new System.Windows.Forms.Button();
            this.resetWeeklyButton = new System.Windows.Forms.Button();
            this.errorLabel = new System.Windows.Forms.Label();
            this.separatorLineLabel = new System.Windows.Forms.Label();
            this.countTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(52, 42);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(570, 109);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // dailyCountLabel
            // 
            this.dailyCountLabel.AutoSize = true;
            this.dailyCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 54F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dailyCountLabel.Location = new System.Drawing.Point(39, 242);
            this.dailyCountLabel.Name = "dailyCountLabel";
            this.dailyCountLabel.Size = new System.Drawing.Size(909, 82);
            this.dailyCountLabel.TabIndex = 1;
            this.dailyCountLabel.Text = "CARTON COUNT (DAILY)";
            // 
            // weeklyCountLabel
            // 
            this.weeklyCountLabel.AutoSize = true;
            this.weeklyCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 54F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weeklyCountLabel.Location = new System.Drawing.Point(39, 687);
            this.weeklyCountLabel.Name = "weeklyCountLabel";
            this.weeklyCountLabel.Size = new System.Drawing.Size(1002, 82);
            this.weeklyCountLabel.TabIndex = 2;
            this.weeklyCountLabel.Text = "CARTON COUNT (WEEKLY)";
            // 
            // dailyCountValueLabel
            // 
            this.dailyCountValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dailyCountValueLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.dailyCountValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 54F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dailyCountValueLabel.Location = new System.Drawing.Point(831, 242);
            this.dailyCountValueLabel.Name = "dailyCountValueLabel";
            this.dailyCountValueLabel.Size = new System.Drawing.Size(476, 108);
            this.dailyCountValueLabel.TabIndex = 3;
            this.dailyCountValueLabel.Text = "0";
            this.dailyCountValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // weeklyCountValueLabel
            // 
            this.weeklyCountValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.weeklyCountValueLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.weeklyCountValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 54F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weeklyCountValueLabel.Location = new System.Drawing.Point(830, 687);
            this.weeklyCountValueLabel.Name = "weeklyCountValueLabel";
            this.weeklyCountValueLabel.Size = new System.Drawing.Size(477, 108);
            this.weeklyCountValueLabel.TabIndex = 4;
            this.weeklyCountValueLabel.Text = "0";
            this.weeklyCountValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // resetDailyButton
            // 
            this.resetDailyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetDailyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetDailyButton.Location = new System.Drawing.Point(1221, 358);
            this.resetDailyButton.Name = "resetDailyButton";
            this.resetDailyButton.Size = new System.Drawing.Size(86, 36);
            this.resetDailyButton.TabIndex = 5;
            this.resetDailyButton.Text = "RESET";
            this.resetDailyButton.UseVisualStyleBackColor = true;
            this.resetDailyButton.Click += new System.EventHandler(this.resetDailyButton_Click);
            // 
            // resetWeeklyButton
            // 
            this.resetWeeklyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetWeeklyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetWeeklyButton.Location = new System.Drawing.Point(1221, 798);
            this.resetWeeklyButton.Name = "resetWeeklyButton";
            this.resetWeeklyButton.Size = new System.Drawing.Size(86, 36);
            this.resetWeeklyButton.TabIndex = 6;
            this.resetWeeklyButton.Text = "RESET";
            this.resetWeeklyButton.UseVisualStyleBackColor = true;
            this.resetWeeklyButton.Click += new System.EventHandler(this.resetWeeklyButton_Click);
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.ForeColor = System.Drawing.Color.Red;
            this.errorLabel.Location = new System.Drawing.Point(343, 42);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(0, 13);
            this.errorLabel.TabIndex = 7;
            // 
            // separatorLineLabel
            // 
            this.separatorLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.separatorLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separatorLineLabel.Location = new System.Drawing.Point(53, 532);
            this.separatorLineLabel.Name = "separatorLineLabel";
            this.separatorLineLabel.Size = new System.Drawing.Size(1254, 2);
            this.separatorLineLabel.TabIndex = 8;
            // 
            // countTimer
            // 
            this.countTimer.Interval = 500;
            this.countTimer.Tick += new System.EventHandler(this.countTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(1361, 741);
            this.Controls.Add(this.separatorLineLabel);
            this.Controls.Add(this.errorLabel);
            this.Controls.Add(this.resetWeeklyButton);
            this.Controls.Add(this.resetDailyButton);
            this.Controls.Add(this.weeklyCountValueLabel);
            this.Controls.Add(this.dailyCountValueLabel);
            this.Controls.Add(this.weeklyCountLabel);
            this.Controls.Add(this.dailyCountLabel);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Brainboxes Counting Application";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label dailyCountLabel;
        private System.Windows.Forms.Label weeklyCountLabel;
        private System.Windows.Forms.Label dailyCountValueLabel;
        private System.Windows.Forms.Label weeklyCountValueLabel;
        private System.Windows.Forms.Button resetDailyButton;
        private System.Windows.Forms.Button resetWeeklyButton;
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.Label separatorLineLabel;
        private System.Windows.Forms.Timer countTimer;

    }
}

