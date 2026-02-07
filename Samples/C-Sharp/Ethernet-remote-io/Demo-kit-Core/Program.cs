using System;

namespace Demo_kit_Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

			string ED527IP = "YOUR_DEVICE_IP";
			string ED204IP = "YOUR_DEVICE_IP";
			BBDemoApp app = new BBDemoApp();
			Console.WriteLine("Starting Server");
			try
			{
				app.StartServers();
				Console.WriteLine("Opening demo in browser at " + app.ServerAddress);

			//	Process.Start("chrome", app.ServerAddress + " http://" + ED527IP + " http://" + ED204IP);
				//Process.Start(app.ServerAddress);
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occured when trying to start up demo");
				Console.WriteLine(e);
			}

			Console.WriteLine("\nPress ENTER to EXIT...");
			Console.Read();
			app.StopServers();
		}
    }
}
