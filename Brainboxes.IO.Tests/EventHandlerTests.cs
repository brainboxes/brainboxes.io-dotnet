using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Brainboxes.IO.Tests
{
    /// <summary>
    /// best way to test event hanlding is to extend the EDDevuice base class that way we can check the internals 
    /// of the class are behaning as expected
    /// </summary>
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    [TestCategory(TestCategories.Events)]
    public class EventHandlerTests : EDDevice
    {
        ConnectionPool cp;

        Dictionary<string, IConnection> connections;

        [TestInitialize]
        public void TestInit()
        {
            //this is a lot of boiler plate, im not sure how to change it
            // needs to init for all available devices rather than picking one
            cp = new ConnectionPool();
            this.IOLines = new IOList<IOLine>(16);
            if (cp.ED588Connections.Any())
            {
                for (int i = 0; i < 8; i++)
                {
                    this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
                }
                // 8 digital outputs
                for (int i = 8; i < 16; i++)
                {
                    this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
                }
                connections = cp.ED588Connections;
            }
            else if (cp.ED527Connections.Any())
            {
                for (int i = 0; i < 16; i++)
                {
                    this.IOLines.Add(new IOLine(i, i, IODirection.Output, IOType.Digital, this));
                }
                connections = cp.ED527Connections;
            }
            else if (cp.ED008Connections.Any())
            {
                this.IOLines = new IOList<IOLine>(16);
                // 8 digital input
                for (int i = 0; i < 8; i++)
                {
                    this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
                }
                for (int i = 8; i < 16; i++)
                {
                    this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
                }
                connections = cp.ED008Connections;
            }
            else if (cp.BB400Connections.Any())
            {
                this.IOLines = new IOList<IOLine>(16);
                // 8 digital input
                for (int i = 0; i < 8; i++)
                {
                    this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
                }
                for (int i = 8; i < 16; i++)
                {
                    this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
                }
                connections = cp.BB400Connections;
            }
            else if (cp.ED204Connections.Any())
            {
                this.IOLines = new IOList<IOLine>(16);
                // 8 digital input
                for (int i = 0; i < 4; i++)
                {
                    this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
                }
                for (int i = 8; i < 12; i++)
                {
                    this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
                }
                connections = cp.BB400Connections;
            }
            _initLines();

        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.Dispose();
            this.Disconnect();
            cp.CloseAll();
        }

        /// <summary>
        /// Will only work with ED-588 and requires external input to test
        /// </summary>
        //[TestMethod]
        public void InputChangeEventHandlerTest()
        {
            foreach (IConnection c in connections.RandomTestOrder().AsEachProtocol())
            {
                TCPConnection tcpConn = c as TCPConnection;
                if (tcpConn == null || tcpConn.Port == TCPConnection.DEFAULT_ASCII_PORT)
                {
                    this.Protocol = new ASCIIProtocol();
                }
                else
                {
                    this.Protocol = new ModbusTCPProtocol();
                }
                this.Connection = c;
                this.Connect();

                int numberOfTimesEventHandlerCalled = 0;

                IOLineChangedEventHandler ioChanged = (line, device, type) =>
                {
                    Debug.WriteLine("");
                    Debug.WriteLine("==== ioChanged event handler called for line: " + line + " ====");
                    Debug.WriteLine("");
                    numberOfTimesEventHandlerCalled++;
                };

                this.IOLineChanged += ioChanged;
                this.Inputs[0].IOLineChanged += ioChanged;

                Console.WriteLine("you have 3 seconds to Press input 0 button once");
                System.Threading.Thread.Sleep(3000);

                Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should not have been called");


                this.IOLineChanged -= ioChanged;
                this.Inputs[0].IOLineChanged -= ioChanged;
                this.Disconnect();

            }
        }


        [TestMethod]
        public void EventHandlerCalledOnSetTest()
        {
            foreach (IConnection c in connections.RandomTestOrder().AsEachProtocol())
            {
                TCPConnection tcpConn = c as TCPConnection;
                if (tcpConn == null || tcpConn.Port == TCPConnection.DEFAULT_ASCII_PORT)
                {
                    this.Protocol = new ASCIIProtocol();
                }
                else
                {
                    this.Protocol = new ModbusTCPProtocol();
                }
                this.Connection = c;
                this.Connect();

                this.Protocol.SetAllDigitalOutputLineStates(0, this.Outputs.Count());

                int numberOfTimesEventHandlerCalled = 0;

                IOLineChangedEventHandler ioChanged = (line, device, type) =>
                {
                    Debug.WriteLine("");
                    Debug.WriteLine("==== ioChanged event handler called for line: " + line + " ====");
                    Debug.WriteLine("");
                    Assert.AreEqual(line.LogicalNumber, this.Outputs[0].LogicalNumber, "The IO Line change method should only be called for this IO line");
                    numberOfTimesEventHandlerCalled++;
                };


                this.Outputs[0].IOLineChanged += ioChanged;

                this.Outputs[1].Value = 1;
                Assert.AreEqual(1, this.Outputs[1].Value, "Output 1 value should be set closed");
                Assert.AreEqual(0, numberOfTimesEventHandlerCalled, "The event handler should not have been called");

                this.Outputs[0].Value = 1;
                Assert.AreEqual(1, this.Outputs[0].Value, "Output 0 value should be set closed");
                Assert.AreEqual(1, numberOfTimesEventHandlerCalled, "The event handler should only have been called once");

                this.Outputs[0].Value = 0;
                Assert.AreEqual(0, this.Outputs[0].Value, "Output 0 value should be set open");
                Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should only have been called twice");

                this.Outputs[0].Value = 0;
                Assert.AreEqual(0, this.Outputs[0].Value, "Output 0 value should still be set open");
                Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should still only have been called twice");

                //remove the event handler
                this.Outputs[0].IOLineChanged -= ioChanged;

                this.Outputs[0].Value = 1;
                Assert.AreEqual(1, this.Outputs[1].Value, "Output 0 value should be set closed");
                Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should still only have been called twice");

                this.Disconnect();
            }
        }

        /// <summary>
        /// Test whether the event handler works when the state of the IO lines changes without calling a Set method
        /// cannot test the input lines as this would require an external stimulus
        /// instead we can change the value of the outputs and see if the changes are detected
        /// </summary>
        [TestMethod]
        public void EventHandlerCalledOnChangeTest()
        {
            int numberOfTimesEventHandlerCalled = 0;

            IOLineChangedEventHandler ioChanged = (line, device, type) =>
            {
                numberOfTimesEventHandlerCalled++;
                Console.WriteLine("");
                Console.WriteLine("==== ioChanged event handler called for line: " + line + " change type:"+type+" ====" );
                Console.WriteLine("==== numTimesCount: "+numberOfTimesEventHandlerCalled);
                Console.WriteLine("");
            };

            foreach (IConnection c in connections.RandomTestOrder().AsEachProtocol())
            {
                TCPConnection tcpConn = c as TCPConnection;
                if(tcpConn == null || tcpConn.Port == TCPConnection.DEFAULT_ASCII_PORT)
                {
                    this.Protocol = new ASCIIProtocol();
                }
                else
                {
                    this.Protocol = new ModbusTCPProtocol();
                }
                this.Connection = c;
                this.Connect();

                //clear all lines before starting test
                this.Outputs.Values = 0;

                numberOfTimesEventHandlerCalled = 0;
                //register for all IO lines
                this.IOLineChanged += ioChanged;

                Console.WriteLine("Test sleep start");
                //wait 2 times cache timeout to give the polling a chance to detect the change
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                //this method call by-passes automatic calling of events and value caching in the IOLines, 
                //instead it will rely on polling to detect the change
                this.Protocol.SetDigitalOutputLineState(0, 1);

                Console.WriteLine("Test sleep start");
                //wait 2 times cache timeout to give the polling a chance to detect the change
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                Assert.AreEqual(1, numberOfTimesEventHandlerCalled, "The event handler should have been called a time after polling");
                Assert.AreEqual(1, this.Outputs[0].Value, "Output 0 value should now be set closed");

                this.Protocol.SetDigitalOutputLineState(0, 1); //no change
                this.Protocol.SetDigitalOutputLineState(1, 1); //change

                Console.WriteLine("Test sleep start");
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should have been called a second time after polling");
                Assert.AreEqual(1, this.Outputs[0].Value, "Output 0 value should still be set closed");
                Assert.AreEqual(1, this.Outputs[1].Value, "Output 1 value should now be set closed");

                this.Protocol.SetDigitalOutputLineState(0, 0); //change
                this.Protocol.SetDigitalOutputLineState(1, 0); //change
                this.Protocol.SetDigitalOutputLineState(2, 1); //change

                Console.WriteLine("Test sleep start");
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                Assert.AreEqual(5, numberOfTimesEventHandlerCalled, "The event handler should have been called a total of 5 times after polling");
                Assert.AreEqual(0, this.Outputs[0].Value, "Output 0 value should now be set open");
                Assert.AreEqual(0, this.Outputs[1].Value, "Output 1 value should now be set open");
                Assert.AreEqual(1, this.Outputs[2].Value, "Output 1 value should now be set closed");

                //also add the handler specifically to line DOUT03
                this.Outputs[3].IOLineChanged += ioChanged;

                this.Protocol.SetDigitalOutputLineState(3, 1); //change

                Console.WriteLine("Test sleep start");
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                Assert.AreEqual(7, numberOfTimesEventHandlerCalled, "The event handler should have been called a total of 7 times after polling. twice for the change on DOUT03");

                Assert.AreEqual(0, this.Outputs[0].Value, "Output 0 value should still be set open");
                Assert.AreEqual(0, this.Outputs[1].Value, "Output 1 value should still be set open");
                Assert.AreEqual(1, this.Outputs[2].Value, "Output 1 value should still be set closed");

                //unregister the overall IOLineChangedHandler
                this.IOLineChanged -= ioChanged;

                this.Protocol.SetDigitalOutputLineState(3, 0); //change
                this.Protocol.SetDigitalOutputLineState(2, 0); //change does not trigger event

                Console.WriteLine("Test sleep start");
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                Assert.AreEqual(8, numberOfTimesEventHandlerCalled, "The event handler should have been called a total of 8 times after polling. once for the change on DOUT03");

                this.Outputs[3].IOLineChanged -= ioChanged;

                this.Protocol.SetDigitalOutputLineState(3, 1); //change does not trigger event
                this.Protocol.SetDigitalOutputLineState(2, 1); //change does not trigger event

                Console.WriteLine("Test sleep start");
                System.Threading.Thread.Sleep(this.IOLineCacheTimeout * 2);
                Console.WriteLine("Test sleep end");

                Assert.AreEqual(8, numberOfTimesEventHandlerCalled, "The event handler should still have been called a total of 8 times after polling");
                
                this.Protocol.SetAllDigitalOutputLineStates(0, this.Outputs.Count());

                this.Disconnect();


            }

        }


        public bool threadRunning
        {
            get
            {
                return this._pollingTimer != null && this._threadShouldBeRunning;
                //return this._pollingThread != null && this._pollingThread.IsAlive;
            }
        }

        [TestMethod]
        public void PollingConnectDisconnectTest()
        {
            IOLineChangedEventHandler ioChanged = (line, device, type) =>
            {
                Assert.Fail("The ioChanged Handler should never be invoked");
            };

            foreach (IConnection c in connections.RandomTestOrder().AsEachProtocol())
            {
                TCPConnection tcpConn = c as TCPConnection;
                if (tcpConn == null || tcpConn.Port == TCPConnection.DEFAULT_ASCII_PORT)
                {
                    this.Protocol = new ASCIIProtocol();
                }
                else
                {
                    this.Protocol = new ModbusTCPProtocol();
                }
                this.Connection = c;

                Assert.IsFalse(threadRunning, "The background thread should not be running");

                this.Connect();

                Assert.IsFalse(threadRunning, "The background thread should still not be running");

                this.Disconnect();

                Assert.IsFalse(threadRunning, "The background thread should STILL not be running");

                this.Connect();

                this.IOLineChanged += ioChanged;

                Assert.IsTrue(threadRunning, "The background thread should be running");

                this.IOLineChanged -= ioChanged;

                Assert.IsFalse(threadRunning, "The background thread should not be running");

                this.IOLineChanged += ioChanged;

                Assert.IsTrue(threadRunning, "The background thread should be running");

                this.Disconnect();

                Assert.IsFalse(threadRunning, "The background thread should not be running while disconnected");

                this.Connect();

                Assert.IsTrue(threadRunning, "The background thread should be run when reconnected");

                this.Disconnect();

                Assert.IsFalse(threadRunning, "The background thread should not be running while disconnected");

                this.Connect();

                Assert.IsTrue(threadRunning, "The background thread should be run when reconnected");

                this.IOLineChanged -= ioChanged;

                Assert.IsFalse(threadRunning, "The background thread should not be running when no events attached");

                this.Disconnect();

            }
        }
        

        /*
        [TestMethod]
        public void PollingTooFastTest()
        {
            int numberOfTimesEventHandlerCalled = 0;

            IOLineChangedEventHandler ioChanged = (line, device, type) =>
            {
                Debug.WriteLine("");
                Debug.WriteLine("==== ioChanged event handler called for line: " + line + " ====");
                Debug.WriteLine("");
                numberOfTimesEventHandlerCalled++;
            };

            this.IOLineChanged += ioChanged;

            foreach (IConnection c in connections.RandomTestOrder())
            {
                this.Connection = c;
                this.Connect();



                int originalIOCacheTimeout = this._ioLineCacheTimeout;
                System.Threading.Thread.Sleep(originalIOCacheTimeout * 2);

                int pollTimeMS = Convert.ToInt32(this.sw.ElapsedMilliseconds * 0.5);

                //make the IO cache timeout too fast!
                this.IOLineCacheTimeout = pollTimeMS;

                System.Threading.Thread.Sleep(originalIOCacheTimeout * 2);
                this.Disconnect();

                //set it back for the next type of connection
                this.IOLineCacheTimeout = originalIOCacheTimeout;
            }
        }
         * */

    }
}
