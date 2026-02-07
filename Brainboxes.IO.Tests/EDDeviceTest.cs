using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class EDDeviceTest : UnitTestBase
    {

        /// <summary>
        /// if a device's connection is replaced it should be set 
        /// into the same state as the one that was there before
        /// </summary>
        [TestMethod]
        public void ChangeConnectionWithASCIIProtocolTest()
        {
            if (cp.EDConnections.Count() < 2)
            {
                Assert.Inconclusive("Not enough Connections in app.config");
            }
            using (EDDevice ED = new EDDevice())
            {
                var conns = cp.EDConnections.RandomTestOrder().AsASCIIProtocol();
                IConnection connection1 = conns.First();
                IConnection connection2 = conns.Skip(1).First();

                Assert.IsFalse(ED.IsConnected, "Connection is closed when there is no connection object present");
                ED.Connection = connection1;
                Assert.IsFalse(ED.IsConnected, "Connection is still closed");

                ED.Connect();

                Assert.IsTrue(ED.IsConnected, "Connection is open");
                Assert.AreEqual("!01", ED.SendCommand("~011"), "Should be able to send command and receive response");
                ED.Connection = connection2;
                Assert.IsTrue(ED.IsConnected, "Connection is still open");
                Assert.AreEqual("!01", ED.SendCommand("~011"), "Should still be able to send command and receive response");

                ED.Connection = connection1;
                Assert.IsTrue(ED.IsConnected, "Connection is open");
                Assert.AreEqual("!01", ED.SendCommand("~011"), "Should be able to send command and receive response");

                ED.Connection = connection2;
                Assert.IsTrue(ED.IsConnected, "Connection is still open");
                Assert.AreEqual("!01", ED.SendCommand("~011"), "Should still be able to send command and receive response");

                ED.Disconnect();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void ConnectionInvalidTCP()
        {
            TCPConnection tcpConn = (TCPConnection) cp.InvalidTCPConnections.RandomTestOrder().FirstOrDefault();
            //speed up the test by loving the length of the timeout
            tcpConn.ConnectionTimeout = 100; //100MS
            using (EDDevice ED = new EDDevice(tcpConn))
            {
                ED.Connect();
            }
        }


        [TestMethod]
        [ExpectedException(typeof(SystemException), AllowDerivedTypes = true)]
        public void ConnectionInvalidSerial()
        {
            using (EDDevice ED = new EDDevice(cp.InvalidSerialConnections.RandomTestOrder().FirstOrDefault()))
            {
                ED.Connect();
            }
        }

        /// <summary>
        /// Trying sending command when no connection throws invalid-operation
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConnectionInvalidCommand()
        {
            using (EDDevice ED = new EDDevice())
            {
                ED.SendCommand("!01");
            }
        }

        /// <summary>
        /// Test the IOLine get methods
        /// WARNING THIS WILL RESTORE FACTORY DEFAULTS
        /// </summary>
        [TestMethod]
        public void GetIOLinesTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsEachProtocol())
            {
                string connectionString = "";
                int data = 0;
                c.CreateConnectionData(out connectionString, out data);
                using (EDDevice ED = EDDevice.Create(connectionString, data))
                {
                    try
                    {
                        ED.Restart();
                    }
                    catch (NotImplementedException) { /* not implemented yet in modbus */ }
                    Console.WriteLine("Device type: " + ED.GetType().Name);
                    if(ED.Inputs.Any())
                    {
                        Assert.IsTrue(ED.Inputs.All(line => line.Value == 1), "ALL inputs should have a value of 1 inputs float HIGH. Ensure the inputs are disconnected from any circuitry");
                    }
                    if(ED.Outputs.Any())
                    {
                        Assert.IsTrue(ED.Outputs.All(line => line.Value == 0), "ALL outputs should have a value of 0 outputs are open");
                    }
                }
            }
        }

        /// <summary>
        /// Test the IOLine set methods
        /// WARNING THIS WILL RESTORE FACTORY DEFAULTS
        /// </summary>
        [TestMethod]
        public void SetOutputLinesTest()
        {
            foreach (IConnection c in cp.DevicesWithOutputsConnections.RandomTestOrder().AsEachProtocol())
            {
                string connectionString = "";
                int data = 0;
                c.CreateConnectionData(out connectionString, out data);
                using (EDDevice ED = EDDevice.Create(connectionString, data))
                {
                    try
                    {
                        ED.Restart();
                    }
                    catch (NotImplementedException) {
                         //TODO: not implemented yet in modbus
                    }
                    Console.WriteLine("Device type: " + ED.GetType().Name);
                    Assert.IsTrue(ED.Outputs.All(line => line.Value == 0), "ALL outputs should have a value of 0 outputs are open");
                
                    Random r = new Random();
                    //the first line plus a random selection of the other lines
                    IOList<IOLine> randomSelection = ED.Outputs.Where(line => r.Next(10) > 4).AsIOList();
                    if(!randomSelection.Any())
                    {
                        randomSelection.Add(ED.Outputs.First()); //make sure there is at least one output added the collection
                    }
                    //close all random lines selected
                    randomSelection.Values = 1;
                    Assert.IsTrue(randomSelection.All(line => line.Value == 1), "ALL selected outputs should have a value of 1 outputs are closed");
                    Assert.IsTrue(ED.Outputs.Except(randomSelection).All(line => line.Value == 0), "ALL other outputs should still have a value of 0. outputs are open");

                    randomSelection.First().Value = 0;

                    Assert.IsTrue(randomSelection.Except(randomSelection.Take(1)).All(line => line.Value == 1), "ALL but the first random selected outputs should still have a value of 1 outputs are closed");
                    Assert.IsTrue(ED.Outputs.Except(randomSelection).Union(randomSelection.Take(1)).All(line => line.Value == 0), "ALL other outputs and the first random selected output should still have a value of 0. outputs are open");

                    randomSelection.Values = 0;
                    Assert.IsTrue(ED.Outputs.All(line => line.Value == 0), "ALL outputs should have a value of 0 outputs are open");

                }
            }
        }

        /// <summary>
        /// Should not be able to set a digital output
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SystemException), AllowDerivedTypes = true)]
        public void CannotSetAnInputLineExceptionTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder())
            {
                using (EDDevice ED = new EDDevice(c))
                {
                    ED.Connect();

                    IOLine inputLine = new IOLine(0, 0, IODirection.Input, IOType.Digital, ED);
                    inputLine.Value = 1;
                    ED.Disconnect();
                }
            }
        }

        [TestMethod]
        [Timeout(600000)] //10 minute timeout as factory reset can take a long time
        public void FactoryResetTest()
        {
            foreach(IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                Console.WriteLine("Connecting using " + c.GetType());

                EDDevice ED = new EDDevice(c, new ASCIIProtocol());
                ED.Connect();

                string[] names = { "ABC", "123", "ABC123", "123ABC" };

                DateTime startTime;

                foreach(string name in names)
                {
                    Console.WriteLine("setting name to " + name);
                    ED.Protocol.DeviceName = name;

                    Console.WriteLine("factory resetting");
                    startTime = DateTime.UtcNow;
                    ED.FactoryReset();
                    Console.WriteLine("Factory Reset took " + (DateTime.UtcNow - startTime).TotalSeconds.ToString("F1") + " seconds");

                    Assert.AreNotEqual(ED.Protocol.DeviceName, name, "The name of the device should reset to factory default");
                }

                ED.Disconnect();
            }
        }

        [TestMethod]
        public void RestartTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                Console.WriteLine("Connecting using " + c.GetType());

                EDDevice ED = new EDDevice(c, new ASCIIProtocol());
                ED.Connect();

                string[] names = { "ABC", "123", "ABC123", "123ABC" };

                foreach (string name in names)
                {
                    Console.WriteLine("setting name to " + name);
                    ED.Protocol.DeviceName = name;

                    Console.WriteLine("Restarting");
                    ED.Restart();

                    Assert.AreEqual(ED.Protocol.DeviceName, name, "The name of the device should be the same after restart");
                }

                ED.Disconnect();
            }
        }
        static int loopCount;
        static DateTime lastTime;
        //[TestMethod]
        public void NuneatonIssue()
        {
            
            using (EDDevice edIn = EDDevice.Create("YOUR_DEVICE_IP"))
            {
                using (EDDevice edOut = EDDevice.Create("YOUR_DEVICE_IP"))
                {
                    edIn.Inputs[0].IOLineFallingEdge += lineFallingEdge;
                    edIn.IOLineCacheTimeout = 100;
                    loopCount = 0;
                    lastTime = DateTime.UtcNow;
                    while (true)
                    {
                        loopCount++;
                        edOut.Outputs[0].Toggle();
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        private static void lineFallingEdge(IOLine line, EDDevice device, IOChangeTypes changeType)
        {

            Console.WriteLine("Falling edge event hit. Change type: " + changeType.ToString() + " Value is: " + line.Value);
            Console.WriteLine("Loop Count: " + loopCount + " Time since last: " + (DateTime.UtcNow - lastTime));
            if ((DateTime.UtcNow - lastTime) < new TimeSpan(0,0,0,0,100))
            {
                throw new Exception("Last time called was less than the IOLineCacheTimeout was: " + (DateTime.UtcNow - lastTime));
            }
            lastTime = DateTime.UtcNow;
        }

        /*
        [TestMethod]
        public void ConnectionFunctionalAfterReset()
        {
            foreach (IConnection c in cp.ValidConnections.RandomTestOrder())
            {
                EDDevice ED = new EDDevice(c, new ASCIIProtocol());
                ED.Connect();

                string response = ED.SendCommand("$01S1");
                if (response[0] == '!')
                {
                    Console.WriteLine("Device reported that reset was successful");
                }
                // but im still connected and can still send commands
                while (true)
                {
                    string lineStates = ED.SendCommand("@01");
                    Console.WriteLine("current line states {0:H4}", lineStates);
                    System.Threading.Thread.Sleep(100); 
                }
                //after an indeterminate amount of time the connection closes, now the reset has really happened
            }
        }
         */
    }
}
