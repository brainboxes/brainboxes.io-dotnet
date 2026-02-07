using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class ED588Tests : UnitTestBase
    {

        //[TestCategory("CAT-TEST"), TestMethod]
        //public void FailingTest()
        //{
        //    Console.WriteLine("This should fail");
        //    Assert.Fail("This should fail");
        //}

        [TestCategory("CAT-TEST"), TestMethod]
        public void InconclusiveTest()
        {
            Console.WriteLine("This should be inconclusive");
            Assert.Inconclusive("This should be inconclusive");
        }

        [TestCategory("CAT-TEST"), TestMethod]
        public void PassingTest()
        {
            Console.WriteLine("This should pass");

        }

        /// <summary>
        /// test that the ED588 has the correct IOLines,
        /// they should all be outputs numbered from 0 to 15
        /// </summary>
        [TestCategory("CAT-TEST"), TestMethod]
        public void EnvVarTests()
        {
            Console.WriteLine("Started Test");

            Console.WriteLine(ConfigurationManager.AppSettings["ED588IPAddress"]);

            Console.WriteLine(ConfigurationManager.AppSettings["ED588SYSVAR"]);

            Console.WriteLine("Reading env variables from the app.config xml");

            Console.WriteLine("MACHINE:");
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine))
            {
                Console.WriteLine("Key:" + env.Key + ", value: " + env.Value);
            }

            Console.WriteLine("USER:");
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                Console.WriteLine("Key:" + env.Key + ", value: " + env.Value);
            }

            Console.WriteLine("PROCESS:");
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
            {
                Console.WriteLine("Key:" + env.Key + ", value: " + env.Value);
            }


            var ed588SysVar = ConfigurationManager.AppSettings["ED588SYSVAR"];
            Console.WriteLine(ed588SysVar != null ? Environment.ExpandEnvironmentVariables(ed588SysVar) : "(ED588SYSVAR not configured)");

            var usernameKey = ConfigurationManager.AppSettings["USERNAMEKEY"];
            Console.WriteLine(usernameKey != null ? Environment.ExpandEnvironmentVariables(usernameKey) : "(USERNAMEKEY not configured)");

            Console.WriteLine("Writing directly into method");


            Console.WriteLine("EXPAND environment var %USERNAME%: " + Environment.ExpandEnvironmentVariables("%USERNAME%"));
            Console.WriteLine("EXPAND environment var %BB_DUTIP%: " + Environment.ExpandEnvironmentVariables("%BB_DUTIP%"));

            Console.WriteLine("GET environment var USERNAME: " + Environment.GetEnvironmentVariable("USERNAME"));
            Console.WriteLine("GET environment var BB_DUTIP: " + Environment.GetEnvironmentVariable("BB_DUTIP"));
        }

        //TODO: this needs to use the connection pool

        //[TestCategory("CAT-TEST"), TestMethod]
        //public void IsAvailableExample()
        //{
        //    ED008 ed = new ED008(new TCPConnection("192,168.0.1"));

        //    ed.DeviceStatusChangedEvent += Ed_DeviceStatusChangedEvent;

        //    ed.Connect();

        //    ed.Disconnect();

        //}

        private void Ed_DeviceStatusChangedEvent(IDevice<IConnection, IIOProtocol> device, string property, bool newValue)
        {
            Console.WriteLine("Status Changed '" + device.Connection.ToString() + "' Property  " + property + " new Value: "+newValue);
        }

        [TestCategory("CAT-TEST"), TestMethod]
        public void CorrectIOLinesED588Test()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED588 ed588 = new ED588())
                {
                    Assert.IsTrue(ed588.IOLines.Take(8).All(line => line.IOType == IOType.Digital && line.IODirection == IODirection.Input), "First 8 IO Lines should be digital inputs");
                    Assert.IsTrue(ed588.IOLines.Skip(8).All(line => line.IOType == IOType.Digital && line.IODirection == IODirection.Output), "Last 8 IO Lines should be digital outputs");

                    Assert.AreEqual(8, ed588.Inputs.Count(), "ED-588 has 8 inputs");
                    Assert.AreEqual(8, ed588.Outputs.Count(), "ED-588 has 8 outputs");

                    Assert.IsTrue(ed588.Inputs.All(line => line.IOType == IOType.Digital && line.IODirection == IODirection.Input), "The inputs are inputs");
                    Assert.IsTrue(ed588.Outputs.All(line => line.IOType == IOType.Digital && line.IODirection == IODirection.Output), "The inputs are inputs");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CannotAddToReadOnlyIOLineList()
        {
            using (ED588 ed588 = new ED588())
            {
                ed588.IOLines.Add(new IOLine(1, 1, IODirection.Input, IOType.Relay, ed588));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CannotRemoveFromReadOnlyIOLineList()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED588 ed588 = new ED588())
                {
                    ed588.IOLines.Remove(ed588.Inputs[0]);
                }
            }
        }


        [TestMethod]
        public void ConnectED588Test()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED588 ed588 = new ED588(c))
                {
                    ed588.Connect();
                    ed588.Disconnect();
                }
            }
        }

        [TestMethod]
        public void GetDefaultIOLineStatesED588Test()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED588 ed588 = new ED588(c))
                {
                    ed588.Connect();
                    ed588.Protocol.SetAllDigitalOutputLineStates(0, ed588.Outputs.Count());

                    foreach (IOLine line in ed588.Inputs)
                    {
                        Assert.AreEqual(1, line.Value, "ED-588 IO Line " + line.ToString() + " should default to HIGH (1)");
                    }

                    foreach (IOLine line in ed588.Outputs)
                    {
                        Assert.AreEqual(0, line.Value, "ED-588 IO Line " + line.ToString() + " should default to OPEN (0)");
                    }

                    ed588.Disconnect();
                }
            }
        }

        /// <summary>
        /// This test demonstrated the need for two locks around the send command function
        /// one so that commands could be correlated with responses, the other so that 
        /// the polling could happen all at once without being interrupted by the other threads
        /// this is demonstrated when using serial connection as command->response take longer 
        /// as the baud rate is generally slower 
        /// </summary>
        [TestMethod]
        public void PollingTest()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED588 ed588 = new ED588(c))
                {
                    ed588.Connect();

                    ed588.Protocol.SetAllDigitalOutputLineStates(0, ed588.Outputs.Count());

                    int numberOfTimesEventHandlerCalled = 0;

                    IOLineChangedEventHandler ioChanged = (line, device, type) =>
                        {
                            Assert.AreEqual(line.LogicalNumber, ed588.Outputs[0].LogicalNumber, "The IO Line change method should only be called for this IO line");
                            numberOfTimesEventHandlerCalled++;
                        };


                    ed588.Outputs[0].IOLineChanged += ioChanged;

                    ed588.Outputs[1].Value = 1;
                    Assert.AreEqual(1, ed588.Outputs[1].Value, "Output 1 value should be set closed");

                    ed588.Outputs[0].Value = 1;
                    Assert.AreEqual(1, ed588.Outputs[1].Value, "Output 0 value should be set closed");
                    Assert.AreEqual(1, numberOfTimesEventHandlerCalled, "The event handler should only have been called once");

                    //this will fix the test by allowing the polling thread to finish before the test thread continues
                    //System.Threading.Thread.Sleep(ed588.IOLineCacheTimeout);

                    ed588.Outputs[0].Value = 0;
                    Assert.AreEqual(0, ed588.Outputs[0].Value, "Output 0 value should be set open");
                    Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should only have been called twice");
                    Debug.WriteLine("Assert 2 passed!");

                    ed588.Outputs[0].Value = 0;
                    Assert.AreEqual(0, ed588.Outputs[0].Value, "Output 0 value should still be set open");
                    Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should still only have been called twice");
                    Debug.WriteLine("Assert 2 passed a second time!");

                    //remove the event handler
                    ed588.Outputs[0].IOLineChanged -= ioChanged;

                    ed588.Outputs[0].Value = 1;
                    Assert.AreEqual(1, ed588.Outputs[1].Value, "Output 0 value should be set closed");
                    Assert.AreEqual(2, numberOfTimesEventHandlerCalled, "The event handler should still only have been called twice");

                    ed588.Disconnect();
                }
            }
        }


        [TestMethod]
        public void GetSetIOLinesED588Test()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED588 ed588 = new ED588(c))
                {
                    ed588.Connect();

                    ed588.Protocol.SetAllDigitalOutputLineStates(0, ed588.Outputs.Count());

                    //pick some random lines
                    Random r = new Random();
                    List<IOLine> randomLines = ed588.Outputs.OrderBy(line => r.Next()).Take(4).ToList();

                    //set the lines closed
                    foreach (IOLine line in randomLines)
                    {
                        Console.WriteLine("Setting " + line.ToString() + " CLOSED (1)");
                        line.Value = 1;
                    }
                    //check they are closed
                    foreach (IOLine line in randomLines)
                    {
                        Console.WriteLine("Getting " + line.ToString());
                        Assert.AreEqual(1, line.Value, "ED-588 IO Line " + line.ToString() + " should have been set closed");
                    }
                    //check the remaining lines are still open
                    foreach (IOLine line in ed588.Outputs.Except(randomLines))
                    {
                        Assert.AreEqual(0, line.Value, "ED-588 IO Line " + line.ToString() + " should still be OPEN as per factory default settings");
                    }

                    ed588.Disconnect();
                }
            }
        }

        /// <summary>
        /// Soak test to send 10,000 commands 
        /// </summary>
        [TestMethod]
        public void ED588SoakTest()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsASCIIProtocol())
            {
                Random rnd = new Random();
                int randomNumber;
                int numberOfCommands = 10_000;

                using (ED588 ed588 = new ED588(c))
                {
                    ed588.Connect();
                    ASCIIProtocol ap = (ASCIIProtocol) ed588.Protocol;
                    
                    for (int i = 1; i <= numberOfCommands; i++)
                    {
                        randomNumber = rnd.Next(256); // Generates random number  between 0 -255 (Hex: 0-FF)
                        // ASCII command to set digital output of lower 8 channels http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%23AA00DD.pdf
                        var cmdToSend = $"#{ap.Address:00}00{randomNumber:X2}";
                        var result = ed588.SendCommand(cmdToSend);
                        Assert.IsTrue(result.Equals(">"), $"Error: command response {cmdToSend} is not > but {result} after {i} requests");
                    }

                    ed588.Disconnect();
                }
            }
        }
    }
}
