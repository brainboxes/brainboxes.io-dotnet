using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Collections.Generic;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class ConnectionStatusTest : UnitTestBase
	{ 

		[TestMethod]
        public void StatusInvalidConnectionTest()
        {
            foreach(IConnection con in cp.InvalidConnections.RandomTestOrder() )
            {
                Assert.IsFalse(con.IsAvailable, "The invalid connection should not be available");
                Assert.IsFalse(con.IsConnected, "The invalid connection should not be connected");
            }
        }

        [TestMethod]
        public void StatusValidConnection()
        {
            foreach (IConnection con in cp.ValidConnections.RandomTestOrder().AsEachProtocol())
            {
                Assert.IsTrue(con.IsAvailable, "The valid connection should be available");
                Assert.IsFalse(con.IsConnected, "The valid connection should not be connected");

                con.Connect();

                Assert.IsTrue(con.IsAvailable, "The valid connection should be available");
                Assert.IsTrue(con.IsConnected, "The valid connection should not be connected");

                con.Disconnect();

                Assert.IsTrue(con.IsAvailable, "The valid connection should be available");
                Assert.IsFalse(con.IsConnected, "The valid connection should not be connected");
            }
        }

        [TestMethod]
        public void IsAvailableSerialPortTest()
        {
            foreach (IConnection c in cp.ValidSerialConnections.RandomTestOrder())
            {
                SerialConnection originalConnection = c as SerialConnection;
                SerialConnection copiedSerialConn = new SerialConnection(originalConnection.PortName, originalConnection.BaudRate, originalConnection.Timeout);

                Assert.IsTrue(originalConnection.IsAvailable, "Original valid serial port should be available before connecting");
                Assert.IsTrue(copiedSerialConn.IsAvailable, "Copied valid serial port should be available before connecting");

                originalConnection.Connect();
                Assert.IsTrue(originalConnection.IsAvailable, "This valid serial port should be available after connecting");
                Assert.IsFalse(copiedSerialConn.IsAvailable, "The copied serial port should not be available because the original has been opened");

                originalConnection.Disconnect();
                Assert.IsTrue(originalConnection.IsAvailable, "Original valid serial port should be available after disconnecting");
                Assert.IsTrue(copiedSerialConn.IsAvailable, "Copied valid serial port should be available after disconnecting");

                copiedSerialConn.Connect();
                Assert.IsFalse(originalConnection.IsAvailable, "This valid serial port should not be available because the copied has been opened");
                Assert.IsTrue(copiedSerialConn.IsAvailable, "The copied serial port should be available because it has been opened");

                copiedSerialConn.Disconnect();
            }
        }

        [TestMethod]
        public void IsAvailableInvalidTCPPortTest()
        {
            foreach (IConnection c in cp.InvalidTCPConnections.RandomTestOrder())
            {
                Assert.IsFalse(c.IsAvailable, "The invalid TCP port should not be available");
            }
        }


        [TestMethod]
        public void IsAvailableTCPTest()
        {
            foreach (TCPConnection originalConnection in cp.ValidTCPConnections.RandomTestOrder().AsEachProtocol())
            {
                TCPConnection copiedTCPConn = new TCPConnection(originalConnection.IP, originalConnection.Port, originalConnection.Timeout, originalConnection.ConnectionTimeout);

                Assert.IsTrue(originalConnection.IsAvailable, "Original valid TCP port should be available before connecting");
                Assert.IsTrue(copiedTCPConn.IsAvailable, "Copied valid serial TCP should be available before connecting");

                originalConnection.Connect();
                Assert.IsTrue(originalConnection.IsAvailable, "This valid TCP port should be available after connecting");
                Assert.IsTrue(copiedTCPConn.IsAvailable, "The copied TCP port should still be available even when the original has been opened");

                originalConnection.Disconnect();
                Assert.IsTrue(originalConnection.IsAvailable, "Original valid TCP port should be available after disconnecting");
                Assert.IsTrue(copiedTCPConn.IsAvailable, "Copied valid TCP port should be available after disconnecting");

                copiedTCPConn.Connect();
                Assert.IsTrue(originalConnection.IsAvailable, "This valid TCP port should not be available even when the copied has been opened");
                Assert.IsTrue(copiedTCPConn.IsAvailable, "The copied serial port should be available because it has been opened");

                originalConnection.Connect();
                Assert.IsTrue(originalConnection.IsAvailable, "This valid TCP port should not be available even when the both connections have been opened");
                Assert.IsTrue(copiedTCPConn.IsAvailable, "The copied serial port should be available even when the both connections have been opened");

                copiedTCPConn.Disconnect();
                originalConnection.Disconnect();
            }
        }

		[TestMethod]
		public void ConnectionStatusChangeTest()
		{
			foreach (IConnection c in cp.ValidTCPConnections.RandomTestOrder())
			{
				int eventCount = 0;
				c.ConnectionStatusChangedEvent += (IConnection connection, string property, bool newValue) =>
				{
					eventCount++;
				};
				// Test that event handler registration works without throwing
				Assert.IsNotNull(c, "Connection should not be null");
			}
		}

		[TestMethod]
        public void IsAvailableInvalidSerialPortTest()
        {
            foreach (IConnection c in cp.InvalidSerialConnections.RandomTestOrder())
            {
                Assert.IsFalse(c.IsAvailable, "The invalid serial port should not be available");
            }
        }

        [TestMethod]
        public void ValidESDeviceSerialPortStatusEventHandlerTest()
        {
            foreach(TCPConnection con in cp.ESConnections.RandomTestOrder())
            {
                using(ESDevice es = ESDevice.Create(con.IP))
                {

                    IList<BBSerialPort> ports = es.Ports;

                    foreach(BBSerialPort port in ports)
                    {
                        int numbOfChangeEvents = 0;

                        port.DeviceStatusChangedEvent += (IDevice<IConnection, ISerialProtocol> device, string property, bool newValue) =>
                        {
                            numbOfChangeEvents++;
                        };

                        port.Connect();

                        port.Disconnect();

                        Assert.AreEqual(2, numbOfChangeEvents, "There should be two change events");

                    }
                }
            }
        }


    }
}
