using log4net;
using System;
using System.Diagnostics;

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
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            BBDemoApp app = new BBDemoApp();
            Console.WriteLine("Starting Server");
            try 
            {
                app.StartServers();
                Console.WriteLine("Demo in browser at " + app.ServerAddress);

               // Process.Start("chrome", app.ServerAddress+ " http://YOUR_DEVICE_IP http://YOUR_DEVICE_IP"); // " http://YOUR_DEVICE_IP http://YOUR_DEVICE_IP");
                //Process.Start(app.ServerAddress);
            }
            catch(Exception e)
            {
                Console.WriteLine("An error occured when trying to start up demo");
                Console.WriteLine(e);
            }

            Console.WriteLine("\nPress ENTER to EXIT...");
            Console.Read();
            app.StopServers();

        }
        /// <summary>
        /// Catch and email uncaught exceptions
        /// may not be possible due to network conditions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            LogManager.GetLogger("EmailLogger").Error("Application level exception uncaught", e.ExceptionObject as Exception);
        }
    }
}
