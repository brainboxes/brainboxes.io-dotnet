using System;
using System.Configuration;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Brainboxes.IO.Samples.InputCounter
{
    public partial class Form1 : Form
    {
        private EDDevice ed;
        private IOLine weeklyCountSensor;
        private IOLine dailyCountSensor;

        private readonly string ipAddress;

        public Form1()
        {
            ipAddress = ConfigurationManager.AppSettings["IPAddress"];
            InitializeComponent();
            //GoFullscreen(true);
        }

        /// <summary>
        /// This method is called the first time the form loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            errorLabel.Text = "Connecting to Brainboxes ED Device at: " + ipAddress;
            try
            {
                ed = EDDevice.Create(ipAddress);
                errorLabel.Text = "";
                weeklyCountSensor = ed.Inputs[0];
                dailyCountSensor = ed.Inputs[1];

                setCounts();

                countTimer.Start();
            }
            catch(SocketException se)
            {
                errorLabel.Text = "There was an error connecting to Brainboxes ED device at IP address " + ipAddress + "\r\n"
                   + "Is the brainboxes device powered on and connected to the network?\r\n"
                   + "To change the IP address go to the file located here:" + System.Environment.CurrentDirectory + "\\App.config\r\n"
                   + se.ToString(); ;
            }
            catch(Exception ex)
            {
                errorLabel.Text = ex.ToString();
            }
        }

        private void setCounts()
        {
            try
            {
                dailyCountValueLabel.Text = string.Format("{0:n0}", dailyCountSensor.Count);
                weeklyCountValueLabel.Text = string.Format("{0:n0}", weeklyCountSensor.Count);
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.ToString();
            }
        }

        private void GoFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
        }

        private void resetDailyButton_Click(object sender, EventArgs e)
        {
            try
            {
                dailyCountSensor.ClearCount();
            }
            catch(Exception ex)
            {
                errorLabel.Text = ex.ToString();
            }
            setCounts();
        }

        private void resetWeeklyButton_Click(object sender, EventArgs e)
        {
            try
            {
                weeklyCountSensor.ClearCount();
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.ToString();
            }
            setCounts();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            countTimer.Stop();
            if(ed != null)
            {
                ed.Disconnect();
            }
        }

        private void countTimer_Tick(object sender, EventArgs e)
        {
            setCounts();
        }


    }
}