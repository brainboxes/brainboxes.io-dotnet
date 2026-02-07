using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace BbDemoRsTruck
{
    class Program
    {
        private static AutoResetEvent preventExit;
        static void Main(string[] args)
        {
            BBDemoApp app = new BBDemoApp();
            Console.WriteLine("Starting Server");
            try
            {
                app.StartServers();
                Console.WriteLine("Demo in broswer at " + app.ServerAddress);
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
                app.StopServers();
        }
    }
}
