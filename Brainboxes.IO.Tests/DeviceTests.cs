using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Brainboxes.IO.Tests
{
	[TestClass]
	[TestCategory(TestCategories.Integration)]
	public class DeviceTests : UnitTestBase
	{

		[TestMethod]
		public void AddRemoveDeviceStatusChangeHandler()
		{
			using (EDDevice ed = new EDDevice())
			{
				ed.DeviceStatusChangedEvent += Ed_DeviceStatusChangedEvent;
				Thread.Sleep(1);//wait for device status change background thread to run
				ed.DeviceStatusChangedEvent -= Ed_DeviceStatusChangedEvent;
			}
		}

		private void Ed_DeviceStatusChangedEvent(IDevice<IConnection, IIOProtocol> device, string property, bool newValue)
		{
			Console.WriteLine("Device Status Change event Called");
		}
	}
}
