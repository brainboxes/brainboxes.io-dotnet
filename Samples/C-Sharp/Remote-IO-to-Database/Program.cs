using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brainboxes.IO;

namespace Remote_IO_to_Database
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> deviceIps = new List<string>() { "YOUR_DEVICE_IP", "YOUR_DEVICE_IP", "YOUR_DEVICE_IP" };

            List<EDDevice> devices = new List<EDDevice>();

            foreach(string ip in deviceIps)
            {
                EDDevice ed = new ED008(new TCPConnection(ip));
                ed.IOLineChanged += Ed_IOLineChanged;
                ed.Connect();

                devices.Add(ed);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }


        /// This function is called from the thread pool
        private static void Ed_IOLineChanged(IOLine line, EDDevice device, IOChangeTypes changeType)
        {
            Console.WriteLine("A change has occurred, to " +((TCPConnection)device.Connection).IP+ " on line " + line);
        }
    }
}
