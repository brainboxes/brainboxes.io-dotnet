/*
This file is part of the C# example/library code for communication with 
Brainboxes Ethernet-attached data acquisition and control products, and is 
provided by Brainboxes Limited.  Examples in other programming languages are 
also available.
Visit http://www.brainboxes.com to see our range of Brainboxes Ethernet-
attached data acquisition and control products, and to check for updates to
this code package.

This is free and unencumbered software released into the public domain.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Brainboxes.IO.Samples
{
    /// <summary>
    /// Create a simple Graphical User Interface which allows a user to 
    /// send and receive ASCII commands to/from a Brainboxes Ethernet to DIO Device
    /// using either a TCP connection or a virtual com port.
    /// Note the to use a virtual com port first the port must be installed using
    /// Brainboxes Boost.IO Manager
    /// </summary>
    public partial class EDConsole : Form
    {
        private EDDevice EDDevice;

        /// <summary>
        /// history of commands sent to devices
        /// </summary>
        private List<string> asciiCommandHistory;
        private List<string> modbusCommandHistory;

        private List<string> currentCommandHistory;

        public EDConsole()
        {
            InitializeComponent();
            //set last used values
            string[] IPAddress = Properties.Settings.Default.IPAddress.Split('.');
            ipAddressTextBox1.Text = IPAddress[0];
            ipAddressTextBox2.Text = IPAddress[1];
            ipAddressTextBox3.Text = IPAddress[2];
            ipAddressTextBox4.Text = IPAddress[3];
            portNumberComboBox.Text = Properties.Settings.Default.Port;
            asciiCommandHistory = new List<string>(Properties.Settings.Default.asciiCommandHistory.Split(','));
            modbusCommandHistory = new List<string>(Properties.Settings.Default.modbusCommandHistory.Split(','));

            Write(Properties.Settings.Default.ConsoleHistory);

            EDDevice = new EDDevice();

            WriteLine("");
            WriteLine("=== START SESSION: "+DateTime.Now.ToString(@"yyyy\/MM\/dd HH:mm")+" ===");

            WriteLine("Choose a method to connect to the Brainboxes ED Device");
        }

        /// <summary>
        /// Add a message to the console (which is a Windows.Form.TextBox)
        /// Scroll the text box down to the newly added message
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message, bool isError = false)
        {
            if(isError)
            {
                this.terminal.SelectionStart = this.terminal.Text.Length;
                this.terminal.SelectionLength = 0;
                this.terminal.SelectionColor = System.Drawing.Color.Red;
            }
            this.terminal.AppendText(message);
            if(isError)
            {
                this.terminal.SelectionColor = this.terminal.ForeColor;
            }
            //scroll to bottom
            this.terminal.SelectionStart = this.terminal.Text.Length;
            this.terminal.ScrollToCaret();
            this.Refresh();
        }

        /// <summary>
        /// Same as Write but adds a newline to the end of the string
        /// </summary>
        /// <param name="line"></param>
        public void WriteLine(string line, bool isError = false)
        {
            this.Write(line + "\r\n", isError);
        }

        /// <summary>
        /// Attempt to connect to the ED device using the settings
        /// specified by the user
        /// </summary>
        protected void connect()
        {
            this.tcpSettingsPanel.Visible = false;
            this.serialSettingsPanel.Visible = false;
            try
            {
                WriteLine("Connecting...");
                this.EDDevice.Connect();
                WriteLine("Connected !");
                this.sendCommandPanel.Enabled = true;
                this.disconnectButton.Visible = true;
            }
            catch (Exception e)
            {
                WriteLine("Connection Error: " + e.Message, true);
                this.tcpSettingsPanel.Visible = true;
                this.serialSettingsPanel.Visible = true;
            }
        }

        /// <summary>
        /// Disconnect from the ED device
        /// </summary>
        protected void disconnect()
        {
            this.sendCommandPanel.Enabled = false;

            try
            {
                if(this.EDDevice.IsConnected)
                {
                    WriteLine("Disconnecting...");
                    this.EDDevice.Disconnect();
                    WriteLine("Disconnected !");
                }
            }
            catch (Exception e)
            {
                WriteLine("Disconnect Error: " + e.Message, true);
            }
            finally
            {
                this.tcpSettingsPanel.Visible = true;
                this.serialSettingsPanel.Visible = true;
                this.disconnectButton.Visible = false;
            }
 
        }

        /// <summary>
        /// Try to send the command in the Command Text box to the
        /// ED Device and display the response
        /// </summary>
        private void sendCommand()
        {
            try
            {
                this.sendCommandButton.Enabled = false;

                //messy
                if (currentCommandHistory[0] != commandTextBox.Text)
                {
                    currentCommandHistory.Insert(0, commandTextBox.Text);
                    commandTextBox.DataSource = null;
                    commandTextBox.DataSource = currentCommandHistory;
                }

                ModbusTCPProtocol m = EDDevice.Protocol as ModbusTCPProtocol;
                if (m != null) //modbus protocol
                {
                    //rather than figure out the Modbus Header from the current ADU, simply send the command
                    //and ask the ModbusTCPProtocol what it sent
                    this.EDDevice.SendCommand(this.commandTextBox.Text);
                    modbusMbapLabel.Text = m.LastRequest.Substring(0,17); //the modbus TCP header is the first 17 characters
                    WriteLine("TX <== " + m.LastRequest);
                    WriteLine("RX ==> " + m.LastResponse);
                }
                else //ASCII protocol
                {
                    WriteLine("TX <== " + this.commandTextBox.Text);
                    string receiveCommand = this.EDDevice.SendCommand(this.commandTextBox.Text);
                    if (receiveCommand == null)
                    {
                        WriteLine("This command does not have a response.");
                    }
                    else
                    {
                        WriteLine("RX ==> " + receiveCommand);
                    }
                }

            }
            catch (Exception e)
            {
                WriteLine("Send Command Error: " + e.Message, true);
            }
            finally
            {
                this.sendCommandButton.Enabled = true;
            }
        }

        /// <summary>
        /// If the user clicks the "send Command" button then attempt
        /// to send command to the ED device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void button1_Click(object sender, EventArgs e)
        {
            this.sendCommand();
        }

        /// <summary>
        /// If the user presses the enter key when in the ASCII command text box
        /// then attempt to send the command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ASCIICommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                this.sendCommand();
            }
        }

        /// <summary>
        /// Attempt to connect to the ED device over TCP using the settings
        /// supplied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcpConnectButton_Click(object sender, EventArgs e)
        {
            WriteLine("");
            WriteLine("Connecting over TCP IP");
            string IPAddress = ipAddressTextBox1.Text + "." + ipAddressTextBox2.Text + "." + ipAddressTextBox3.Text + "." + ipAddressTextBox4.Text;
            int port = Convert.ToInt32(portNumberComboBox.Text);
            WriteLine("Address: " + IPAddress + ":" + port.ToString());
            WriteLine("Default connection timeout if address not found: 20 seconds");
            this.EDDevice.Connection = new TCPConnection(IPAddress, port);
            //success
            Properties.Settings.Default.IPAddress = IPAddress;
            Properties.Settings.Default.Port = port.ToString();
            if (port == 502)
            {
                WriteLine("ModbusTCP Protocol");
                protocolLabel.Text = "Modbus TCP Protocol";
                modbusMbapLabel.Visible = true;
                modbusLabel.Visible = true;
                modbusLabel2.Visible = true;
                commandTextBox.Text = "03 0000 0001";
                currentCommandHistory = modbusCommandHistory;
                this.EDDevice.Protocol = new ModbusTCPProtocol();
            }
            else
            {
                WriteLine("ASCII Protocol");
                protocolLabel.Text = "ASCII Protocol";
                modbusMbapLabel.Visible = false;
                modbusLabel.Visible = false;
                modbusLabel2.Visible = false;
                commandTextBox.Text = "$01M";
                currentCommandHistory = asciiCommandHistory;
                this.EDDevice.Protocol = new ASCIIProtocol();
            }
            //update combo-box
            commandTextBox.DataSource = null;
            commandTextBox.DataSource = currentCommandHistory;

            this.connect();
        }

        /// <summary>
        /// Attempt to connect over serial com port
        /// note the ED device virtual com must be installed
        /// using Brainboxes Boost.IO Manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialConnectButton_Click(object sender, EventArgs e)
        {
            WriteLine("");
            WriteLine("Connecting over Serial");
            string comText = comPortComboBox.SelectedItem != null ? comPortComboBox.SelectedItem.ToString() : comPortComboBox.Text;
            WriteLine(comText);
            this.EDDevice.Connection = new SerialConnection(comText, 115200);

            WriteLine("ASCII Protocol");
            protocolLabel.Text = "ASCII Protocol";
            modbusMbapLabel.Visible = false;
            modbusLabel.Visible = false;
            modbusLabel2.Visible = false;
            commandTextBox.Text = "$01M";
            this.EDDevice.Protocol = new ASCIIProtocol();

            this.connect();
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            this.disconnect();
        }

        /// <summary>
        /// When the user presses the enter key when in the TCP settings
        /// panel then attempt to connect to the ED device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcpPanel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                this.tcpConnectButton_Click(sender,e);
            }
        }

        private void comPortComboBox_DropDown(object sender, EventArgs e)
        {
            //list all valid system com ports
            this.comPortComboBox.DataSource = System.IO.Ports.SerialPort.GetPortNames();
        }

        private void comPortComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                this.serialConnectButton_Click(sender, e);
            }
        }

        private void EDConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.disconnect();

            WriteLine("=== END SESSION: " + DateTime.Now.ToString(@"yyyy\/MM\/dd HH:mm") + " ===");


            //last 50 lines in history

            string history = Properties.Settings.Default.ConsoleHistory + terminal.Text;

            string[] lines = history.Split('\n');

            if(lines.Length > 50)
            {
                history = "";
                for(int i = lines.Length - 50; i < lines.Length; i++)
                {
                    history += (lines[i] +"\n");
                }
            }

            Properties.Settings.Default.ConsoleHistory = history;

            //last 10 commands in history

            string[] ascii = asciiCommandHistory.Count > 10 ? asciiCommandHistory.GetRange(0, 10).ToArray() : asciiCommandHistory.ToArray();
            string[] modbus = modbusCommandHistory.Count > 10 ? modbusCommandHistory.GetRange(0, 10).ToArray() : modbusCommandHistory.ToArray();

            Properties.Settings.Default.asciiCommandHistory = string.Join(",", ascii);
            Properties.Settings.Default.modbusCommandHistory = string.Join(",", modbus);

            Properties.Settings.Default.Save();
        }
    }

}
