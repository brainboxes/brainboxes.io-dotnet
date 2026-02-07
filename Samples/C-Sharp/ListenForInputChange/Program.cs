using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brainboxes.IO;
using System.Threading;

namespace ListenForInputChange
{
	class Program
	{
		static void Main(string[] args)
		{

			//ip of ed device
			string ipAddress = "YOUR_DEVICE_IP";

			Console.WriteLine("Connecting to ED device at "+ ipAddress);
			using (EDDevice ed = EDDevice.Create(ipAddress))
			{
				Console.WriteLine("Connected to ED " + ed.Describe());

				//monitor when the status of the device changes e.g. disconnects
				ed.DeviceStatusChangedEvent += EdDeviceStatusChangedEvent;

				//monitor for the input line 0 changes
				//assume alarms fire when input goes from open (0) to closed (1)
				ed.Inputs[0].IOLineRisingEdge += AlarmStartedEvent;
				ed.Inputs[0].IOLineFallingEdge += AlarmStoppedEvent;

				Console.WriteLine("Monitoring IO Lines and device status, waiting for changes");

				Console.WriteLine("Press Any key to exit");
				Console.ReadKey();
				Console.WriteLine("EXITING");
				Thread.Sleep(2000);

			}
		}

		private static void AlarmStartedEvent(IOLine line, EDDevice device, IOChangeTypes changeType)
		{
			Console.WriteLine("The alarm has started at: " + DateTime.Now);
		}

		private static void AlarmStoppedEvent(IOLine line, EDDevice device, IOChangeTypes changeType)
		{
			Console.WriteLine("The alarm has stopped at: " + DateTime.Now);
		}

		private static void EdDeviceStatusChangedEvent(IDevice<IConnection, IIOProtocol> device, string property, bool newValue)
		{
			if (!device.IsConnected)
			{
				if(device.IsAvailable)
				{
					Console.WriteLine("The device is not connected, but has come online ... attempting reconnect");
					device.Connect();
					return;
				}
				Console.WriteLine("The device is not connected,and is offline! oh dear!");
			}
			else
			{
				Console.WriteLine("Device Auto Reconnected!");
			}
		}

	}
}
