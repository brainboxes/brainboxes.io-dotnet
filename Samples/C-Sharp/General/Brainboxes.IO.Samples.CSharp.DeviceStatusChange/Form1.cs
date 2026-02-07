using Brainboxes.IO;
using System;
using System.Net;
using System.Windows.Forms;

namespace DeviceStatusChange
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //timeout for when text box input has finished
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += timer_Elapsed;
        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            updateConnection();
        }

        private ESDevice es;
        private System.Timers.Timer timer;

        private void AppendLine(string text)
        {
            lastActionLabel.AppendText( "\r\n" + DateTime.Now.ToLongTimeString() + " " + text );

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                AppendLine("Attempting to Connect");
                es.Connect();
            } catch (Exception ex)
            {
                 AppendLine("Error Connecting \r\n"+ex);
            }

        }

        private void OnDeviceStatusChangedEvent(IDevice<IConnection, ISerialProtocol> device, string property, bool newValue)
        {
            this.Invoke(new Action(() =>
            {
                isAvailableCheckBox.Checked = device.IsAvailable;
                isConnectedCheckBox.Checked = device.IsConnected;
                AppendLine("Device Status Change Event: "+property+ " changed to: "+newValue);
            }));
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            es.Disconnect();
            AppendLine("Dis-connecting");
        }

        private void updateConnection()
        {
            IPAddress ip;
            if(IPAddress.TryParse(ipTextBox.Text, out ip))
            {
                if (es != null && es.IPAddress == ip.ToString()) return; //no change

                AppendLine("Recognised IP address: " + ip.ToString());
                AppendLine("Attempting to determine device type");
                es = ESDevice.Create(ip.ToString());
                if(es.IsAvailable)
                {
                    AppendLine("Device recognised as " + es.ToString());
                }
                else
                {
                    AppendLine("Device not currently available, using generic ESDevice");
                }
                AppendLine("Updating Connection to: " + (es.Ports[0].Connection as TCPConnection).IP);
                AppendLine("IsAvailable now set to: " + es.Ports[0].IsAvailable);
                AppendLine("IsConnected now set to: " + es.Ports[0].IsConnected);

                isConnectedCheckBox.Checked = es.IsConnected;
                isAvailableCheckBox.Checked = es.IsAvailable;

                es.Ports[0].DeviceStatusChangedEvent += OnDeviceStatusChangedEvent;
            }
            else
            {
                AppendLine("IP Address not recognised");
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer.Stop();
            this.Invoke(new Action(() =>
            {
                updateConnection();
            }));
        }

        private void ipTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.timer.Enabled)
                this.timer.Start();
            else
            {
                this.timer.Stop();
                this.timer.Start();
            }
        }
    }
}
