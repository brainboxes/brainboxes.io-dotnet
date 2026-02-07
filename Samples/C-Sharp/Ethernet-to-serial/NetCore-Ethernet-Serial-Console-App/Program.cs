using System;
using System.Threading;
using Brainboxes.IO;

namespace NetCore_Ethernet_Serial_Console_App
{
    class Program
    {
        private static ESDevice es;
        private const string IP = "YOUR_DEVICE_IP";

        private static AutoResetEvent preventExit;
        static void Main(string[] args)
        {
            Console.WriteLine("Ethernet to Serial TX/RX Test program");

            es = new ES511(IP);//, timeout: 2 * 60 * 1000);


            es.Ports[0].Protocol = new DefaultSerialProtocol
            {
                // set the terminating character to \r\n, this will be auto appended and removed from any sent/recieved messages
                TerminatingCharacters = "\r\n",
                // Encoding = defaults to UTF-8 which covers ASCII and more
            };

            // with ED the there is one connection to the device, however with ES, there is one connection per ethernet port
            // each serial port maintains its own connection so attach events to the port rather than the ESDevice
            // note the device will only disconnect/ be considered offline after the timeout expires without being able to communicate
            // connection status events are propogated up, so only attach to the port and not the connection , its the same information
            es.Ports[0].DeviceStatusChangedEvent += DeviceStatusChangedEvent;

            if (es.Ports[0].IsAvailable)
            {
                // put the function in a new thread
                new Thread(() => { ConnectAndTxRx(); }).Start();
            }

            // otherwise wait for the device to be online, then start comms
            // the device status change event will handle this

            // pause the main thread
            using (preventExit = new AutoResetEvent(false))
            {
                //pause current thread keep app running
                preventExit.WaitOne();
            }
            Console.WriteLine("Exiting");
            Console.WriteLine("Exiting");
        }

        private static void DeviceStatusChangedEvent(IDevice<IConnection, ISerialProtocol> device, string property, bool newValue)
        {
            Console.WriteLine($"ES Device status change-- {property}:{newValue}");
            // if the device has just come back online start messaging
            if (property == "IsAvailable" && newValue)
            {
                Console.WriteLine("Reconnecting to the device");
                // put the function in a new thread
                new Thread(() => { ConnectAndTxRx(); }).Start();

            }
        }

        public static void ConnectAndTxRx()
        { 
            try
            {
                // just open the port we are interested in
                es.Ports[0].Connect();
                // alternatively this function connects all ports on the device (if multi port)
                // es.Connect();
                // only gets here if the connection is successful
                string buffer = string.Empty;
                string recievedData;
                while (buffer != "quit")
                {
                    Console.WriteLine("Enter Value to send to other device");
                    buffer = Console.ReadLine();
                    Console.WriteLine($"TX => {buffer}");
                    // the terminating character(s) is automatically appended
                    es.Ports[0].Send(buffer);
                    Console.WriteLine("Waiting for response from other device");
                    // note if the other device is being controlled by PUTTY, 
                    // Putty will only send \r when the <Enter> key is pressed, to add a \n type <ctrl>+J into the putty window
                    recievedData = es.Ports[0].Receive();
                    Console.WriteLine($"RX <= {recievedData}");
                }
                // only get here if the buffer == quit, signal the main thread to quit as well
                preventExit.Set();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error " + e);
            }
            finally
            {
                Console.WriteLine("disconnecting");
                // if the device has already disconnected, this is not a harmful call
                es.Disconnect();
            }
        
        }

    }

}
