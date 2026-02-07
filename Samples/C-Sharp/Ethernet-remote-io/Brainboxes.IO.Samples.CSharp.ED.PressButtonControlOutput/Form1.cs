using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brainboxes.IO;
using System.Net;

namespace Brainboxes.IO.Samples.PressButtonControlOutput
{
    public partial class Form1 : Form
    {
        public EDDevice ed;

        /// <summary>
        /// change this to your devices' IP address
        /// </summary>
        public readonly string initialIPAddress = "YOUR_DEVICE_IP";

        private Timer delayedTextChangedTimer;

        public int currentOutput = 0;


        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is called the first time the form appears on the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            ipAddressTextBox.Text = initialIPAddress;

            updateConnection();
        }

        void delayedTextChangedTimer_Tick(object sender, EventArgs e)
        {
            //if the text box has a valid number in it update the currentOutput
            IPAddress newValue;
            if (IPAddress.TryParse(ipAddressTextBox.Text, out newValue))
            {
                if (newValue.ToString() != (ed.Connection as TCPConnection).IP)
                {
                    updateConnection();
                }
            }
        }


        private void updateConnection()
        {
            try
            {
                errorLabel.BackColor = Color.DarkGray;

                if (ed != null)
                {
                    ed.Disconnect();
                }
                currentOutputTextBox.Enabled = false;
                toggleButton.Enabled = false;
                errorLabel.Text = "Connecting to: \n" + ipAddressTextBox.Text+"\nConnection timeout: 20 seconds";
                ed = EDDevice.Create(ipAddressTextBox.Text);
                errorLabel.Text = "Connected! to: \n" + ed.ToString().Replace("Brainboxes.IO.", "");
                updateOutputLabel();
            }
            catch(Exception e)
            {
                errorLabel.Text = "Error: \n"+e.ToString();
            }
        }

        /// <summary>
        /// when the output line state changes display the new value
        /// </summary>
        private void updateOutputLabel()
        {
            try
            {
                if (ed.Outputs.Any())
                {
                    currentOutputTextBox.Enabled = true;
                    toggleButton.Enabled = true;
                    //get theoutput line state and update the text box
                    currentOutputStateLabel.Text = (ed.Outputs[currentOutput].Value == 1 ? "On" : "Off");
                }
                else
                {
                    currentOutputStateLabel.Text = "No Outputs";
                }
                errorLabel.BackColor = SystemColors.Control;

            }
            catch (Exception e)
            {
                errorLabel.Text = e.ToString();
            }
        }



        /// <summary>
        /// This method is called whenever the button on the interface is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //any output you want to change from 0 - 15
            //ed.Outputs[currentOutput].Value = 1; // to turn the output on 
            //ed.Outputs[currentOutput].Value = 0; // to turn the output off 
            ed.Outputs[currentOutput].Toggle(); //to flip the output, e.g. if it was off turn it on and vice versa

            updateOutputLabel();
        }

        /// <summary>
        /// when the text in the text box changes this method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentOutputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            //if the text box has a valid number in it update the currentOutput
            int newValue = 0;
            if(Int32.TryParse(currentOutputTextBox.Text, out newValue))
            {
                currentOutput = newValue;
                updateOutputLabel();

            }
        }

        private void ipAddressTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(delayedTextChangedTimer != null)
            {
                delayedTextChangedTimer.Stop();
            }
            delayedTextChangedTimer = new Timer();
            delayedTextChangedTimer.Interval = 3000;
            delayedTextChangedTimer.Tick += delayedTextChangedTimer_Tick;
            delayedTextChangedTimer.Start();
        }

    }
}
