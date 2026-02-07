using System;
using System.Diagnostics;
using System.Threading;

namespace BBDemoApp
{
    /// <summary>
    /// need to run a one time config to enable the demo
    /// 
    /// 1. all use of port 8888 for the server:
    /// netsh http add urlacl url=http://+:8888/ user=**YOUR WINDOWS LOGIN USERNAME INCLUDING DOMAIN e.g. Network1\name**
    /// netsh http add urlacl url=http://+:8989/ user=**YOUR WINDOWS LOGIN USERNAME INCLUDING DOMAIN e.g. Network1\name**
    /// 
    /// 2. open a port in the firewall to allow inbound traffic
    /// netsh advfirewall firewall add rule name="BBDemo" protocol=TCP localport=8888 action=allow dir=IN
    /// netsh advfirewall firewall add rule name="BBDemoWebSocket" protocol=TCP localport=8989 action=allow dir=IN
    /// Both commands need to be run as Admin
    /// </summary>
    class Program
    {
        private static AutoResetEvent preventExit;
        static void Main(string[] args)
        {
            /*
            if(args != null)
            {
                Console.Write("args length is ");
                Console.WriteLine(args.Length); // Write array length
                for (int i = 0; i < args.Length; i++) // Loop through array
                {
                    string argument = args[i];
                    Console.Write("args index ");
                    Console.Write(i); // Write index
                    Console.Write(" is [");
                    Console.Write(argument); // Write string
                    Console.WriteLine("]");
                }
            }
             */
            string ED527IP = "YOUR_DEVICE_IP";
            string ED204IP = "YOUR_DEVICE_IP";
            BBDemoApp app = new BBDemoApp();
            Console.WriteLine("Starting Server");
            try 
            {
                app.StartServers();
                Console.WriteLine("Opening demo in browser at " + app.ServerAddress);

                //Process.Start("chrome", app.ServerAddress+ " http://"+ED527IP+" http://"+ED204IP);
                //Process.Start(app.ServerAddress);
            }
            catch(Exception e)
            {
                Console.WriteLine("An error occured when trying to start up demo");
                Console.WriteLine(e);
            }
            using (preventExit = new AutoResetEvent(false))
            {
                preventExit.WaitOne();
            }
            //Console.WriteLine("\nPress ENTER to EXIT...");
            //Console.Read();
            app.StopServers();

        }
    }
}
