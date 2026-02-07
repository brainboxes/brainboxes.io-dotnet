using System;
using System.Threading;

namespace DemoKit_Analog_Core
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
				Console.WriteLine("Demo in browser at " + app.ServerAddress);
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occured when trying to start up demo");
				Console.WriteLine(e);
			}

			using (preventExit = new AutoResetEvent(false))
			{
				//pause current thread keep app running
				preventExit.WaitOne();
			}
			/*
            Do not use the following pattern for mono on Linux:
            in the application was causing an exit in mono startup scripts 
             

            Console.WriteLine("\nPress ENTER to EXIT...");
            Console.Read();
			*/
			app.StopServers();
		}
    }
}
