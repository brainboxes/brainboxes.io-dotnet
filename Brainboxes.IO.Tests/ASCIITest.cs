using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class ASCIITest : UnitTestBase
    {

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

        [TestMethod]
        [TestCategory("LongRunning")]
        [Timeout(300000)] // 5 minutes - ~10,000 commands at 6ms each = ~60s, with buffer
        public void SendCommandRepeatedly()
        {
            int devicesTestedCount = 0;
            
            for (int t = 0; t < 10; t++)
            {
                // Thread-safe collection to capture output from parallel threads
                // Console.WriteLine in Parallel.ForEach doesn't get captured by MSTest in .NET 10+
                var outputMessages = new ConcurrentBag<string>();
                
                // sending @01 only value for digital io devices
                Parallel.ForEach(cp.DevicesWithDigitalIOConnections.RandomTestOrder().AsASCIIProtocol(), c =>
                //foreach (IConnection c in cp.ValidConnections.RandomTestOrder())
                {
                    System.Threading.Interlocked.Increment(ref devicesTestedCount);
                    var localOutput = new StringBuilder();
                    
                    //use to calculate some stats
                    Stopwatch sw = new Stopwatch();
                    List<double> times = new List<double>();

                    EDDevice ed = new EDDevice(c);

                    ed.Connection.Timeout = 10_000; //10 seconds
                    int i = 0;
                    try
                    {
                        ed.Connect();
                        localOutput.AppendLine("Connection Timeout set to :" + ed.Connection.Timeout + " ms");

                        for (; i < 1_000; i++)
                        {
                            sw.Reset();
                            sw.Start();
                            // these commands are sent synchronously
                            ed.SendCommand("@01");
                            sw.Stop();
                            double time = sw.Elapsed.TotalMilliseconds;
                            times.Add(time);
                            Debug.WriteLine("Command Duration: " + time + "ms");
                        }
                    }
                    catch (Exception e)
                    {
                        sw.Stop();
                        localOutput.AppendLine(c.ToString());
                        localOutput.AppendLine("Failed on the " + i + " command.");

                        double time = sw.Elapsed.TotalMilliseconds; //1 tick = 10,000 milliseconds
                        times.Add(time);
                        localOutput.AppendLine("Failed Command Duration: " + time + "ms");

                        throw e;
                    }
                    finally
                    {
                        ed.Disconnect();
                        if (times.Count > 0)
                        {
                            localOutput.AppendLine("Average command duration: " + String.Format("{0:0.00}", times.Average()) + " ms");
                            localOutput.AppendLine("Maximum command duration: " + String.Format("{0:0.00}", times.Max()) + " ms");
                            localOutput.AppendLine("Minimum command duration: " + String.Format("{0:0.00}", times.Min()) + " ms");
                            localOutput.AppendLine("Standard deviation: " + String.Format("{0:0.00}", CalculateStdDev(times)) + " ms");
                        }
                        localOutput.AppendLine();
                        times.Clear(); //need to reset for other runs
                        
                        outputMessages.Add(localOutput.ToString());
                    }

                });
                
                // Print output after each test run on the main test thread where MSTest captures it
                Console.WriteLine($"=== Test Run {t + 1} of 10 ===");
                foreach (var message in outputMessages)
                {
                    Console.WriteLine(message);
                }
            }
            
            // Ensure test doesn't silently pass when no devices are available
            Assert.IsTrue(devicesTestedCount > 0, "No devices with digital IO connections were available to test");
        }

        /// <summary>
        /// utility method for standard dev
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private double CalculateStdDev(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        /// <summary>
        /// specify invariant culture so that on all OS' the double to string uses . to separate units from decimal place
        /// bug found by french customer: Gregory.Daminet@materianova.be, who was running this command on an OS set with French-Belgium culture
        /// related bug reported by customer: glenn.livens@continental-corporation.com, using ED-549, where the Parse function also needs to be CultureInvariant
        /// </summary>
        [TestMethod]
        public void CultureInfoInvariantTest()
        {
            Random r = new Random();
            // SetAnalogOutputLineState

            int address = 1;
            int line = 4;
            double value = r.NextDouble() * 10;

            string command = string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1}{2}", address, line, value);

            Assert.IsTrue(command.Contains("."), "The set analog output value should contain a dot");
            Assert.IsFalse(command.Contains(","), "The set analog output value should not contain a comma to separate");

            string frenchOScommand = string.Format(CultureInfo.GetCultureInfo("FR-FR"), "#{0:X2}{1}{2}", address, line, value);

            Assert.IsTrue(!frenchOScommand.Contains("."), "The set analog output value should contain a dot but the french localised command contains a ,");
            Assert.IsFalse(!frenchOScommand.Contains(","), "The set analog output value should not contain a comma to separate  but the french localised command does");

            string number = "+1.32";
            //pass
            Console.WriteLine("Parsing InvariantCulture pass: " + double.Parse(number, CultureInfo.InvariantCulture) );
            //english Norway - behavior changed in .NET 10 (ICU data updates), may no longer accept '+' prefix
            try
            {
                Console.WriteLine("Parsing English Language foreign region pass: " + double.Parse(number, NumberStyles.Float, CultureInfo.GetCultureInfo("en-NO")));
            }
            catch (FormatException)
            {
                Console.WriteLine("en-NO culture does not accept '+' prefix in .NET 10+ (expected on newer runtimes)");
            }
            // all 3 non-English Norwegian cultures fail test
            try
            {
                Console.WriteLine(double.Parse(number, CultureInfo.GetCultureInfo("nb-NO")));
                Assert.Fail("Shouldnt be able to parse double into nb-NO culture");
            }
            catch (FormatException) { }
            try
            {
                Console.WriteLine(double.Parse(number, CultureInfo.GetCultureInfo("nn-NO")));
                Assert.Fail("Shouldnt be able to parse double into nn-NO culture");
            }
            catch (FormatException) { }
            try
            {
                Console.WriteLine(double.Parse(number, CultureInfo.GetCultureInfo("no")));
                Assert.Fail("Shouldnt be able to parse double into no culture");
            }
            catch (FormatException) { }

        }

        [TestMethod]
        public void CommandsWithoutResponseTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                CommandResponseList commandsWithoutResponse = new CommandResponseList()
                { 
                    //command, expectedResponse, Comment
                    {"#**", null, "not in manual"},
                    {"~**", null, "Informs all modules that host is OK."},
                };
                using (EDDevice ED = new EDDevice(c, new ASCIIProtocol()))
                {
                    ED.Connect();
                    commandsWithoutResponse.RunTest(ED);
                    ED.Disconnect();
                }
            }
        }

        /// <summary>
        /// WARNING THIS TEST WILL RESTORE FACTORY DEFAULTS
        /// </summary>
        [TestMethod]
        public void GetAndSetIOLines()
        {
            foreach (IConnection c in cp.DevicesWhichCanBeFactoryReset.RandomTestOrder().AsASCIIProtocol())
            {
                using (EDDevice ED = new EDDevice(c, new ASCIIProtocol()))
                {
                    ED.Connect();
                    ED.FactoryReset();
                    CommandResponseList commandsList = new CommandResponseList();
                    switch (ED.Protocol.DeviceName)
                    {
                        case "ED-588":
                            ED.Protocol.SetAllDigitalOutputLineStates(0, 8);

                            commandsList = new CommandResponseList()
                        { 
                            //command,  response,   Comment
                            {"@01",     ">00FF",    "Outputs Open, Inputs float HIGH (when Jumper is in factory default NPN position)"},
                            {"$016",    "!00FF00",  "Outputs Open, Inputs float HIGH (when Jumper is in factory default NPN position)"},
                            {"$01M",    "!01ED-588",  "Device name should be default ED-588"},

                            {"#0100FF", ">",        "Set all 8 output channels of ED-588 high"},
                            {"@01",     ">FFFF",    "All lower 8 output channels should be high, as well as the input channels"},
                            {"#010000", ">",        "Set all 8 output channels of ED-588 low"},
                            {"@01",     ">00FF",    "Outputs Open, Inputs float HIGH"},

                            {"#0100FF", ">",        "Set all 8 output channels of ED-588 high"},
                            {"@01",     ">FFFF",    "All lower 8 output channels should be high, as well as the input channels"},
                            {"#010000", ">",        "Set all 8 output channels of ED-588 low"},
                            {"@01",     ">00FF",    "Outputs Open, Inputs float HIGH"},


                            {"#01A001", ">",        "Set DOUT0 to 1"},
                            {"@01",     ">01FF",    "DOUT0 to 1, rest 0"},
                            {"#01A101", ">",        "Set DOUT1 to 1"},
                            {"@01",     ">03FF",    "DOUT0 and DOUT1 to 1, rest 0"},
                            {"#011000", ">",        "Set DOUT0 to 0"},
                            {"@01",     ">02FF",    "DOUT1 to 1, rest 0"},
                            {"#01A100", ">",        "Set DOUT1 to 0"},
                            {"@01",     ">00FF",    "All channels should be low"},
                        };
                            break;
                        case "ED-527":
                            ED.Protocol.SetAllDigitalOutputLineStates(0, 16);

                            commandsList = new CommandResponseList()
                        { 
                            //command,  response,   Comment
                            {"@01",     ">0000",    "All IO lines should be 0"},
                            {"$016",    "!000000",  "All IO lines should be 0"},
                            {"$01M",    "!01ED-527",  "Device name should be default ED-527"},

                            {"#0100FF", ">",        "Set all lower 8 output channels of ED-527 high"},
                            {"@01",     ">00FF",    "All lower 8 output channels should be high"},
                            {"#010000", ">",        "Set all lower 8 output channels of ED-527 low"},
                            {"@01",     ">0000",    "All channels should be low"},

                            {"#010AFF", ">",        "Set all lower 8 output channels of ED-527 high"},
                            {"@01",     ">00FF",    "All lower 8 output channels should be high"},
                            {"#010A00", ">",        "Set all lower 8 output channels of ED-527 low"},
                            {"@01",     ">0000",    "All channels should be low"},

                            {"#010BFF", ">",        "Set all upper 8 output channels of ED-527 high"},
                            {"@01",     ">FF00",    "All upper 8 output channels should be high"},
                            {"#010B00", ">",        "Set all upper 8 output channels of ED-527 low"},
                            {"@01",     ">0000",    "All channels should be low"},

                            {"#01A001", ">",        "Set DOUT0 to 1"},
                            {"@01",     ">0001",    "DOUT0 to 1, rest 0"},
                            {"#01A101", ">",        "Set DOUT1 to 1"},
                            {"@01",     ">0003",    "DOUT0 and DOUT1 to 1, rest 0"},
                            {"#01A100", ">",        "Set DOUT1 to 0"},
                            {"@01",     ">0001",    "DOUT0 to 1, rest 0"},
                            {"#011000", ">",        "Set DOUT0 to 0"},
                            {"@01",     ">0000",    "All channels should be low"},
                        };
                            break;
                        case "ED-516":

                            commandsList = new CommandResponseList()
                        { 
                            //command,  response,   Comment
                            {"@01",     ">FFFF",    "Outputs Open, Inputs float HIGH (when Jumper is in factory default NPN position)"},
                            {"$016",    "!FFFF00",  "Outputs Open, Inputs float HIGH (when Jumper is in factory default NPN position)"},
                            {"$01M",    "!01ED-516",  "Device name should be default ED-516"},

                            {"@01",     ">FFFF",    "All channels should be low"},
                        };
                            break;

                    }

                    commandsList.RunTest(ED);
                    ED.Disconnect();
                }
            }
        }

        [TestMethod]
        public void FactoryResetTest()
        {
            string newDeviceName = "bob";
            foreach (IConnection c in cp.DevicesWhichCanBeFactoryReset.RandomTestOrder().AsASCIIProtocol())
            {
                using (EDDevice ED = new EDDevice(c, new ASCIIProtocol()))
                {
                    ED.Connect();
                    ED.Protocol.DeviceName = newDeviceName;
                    ED.Disconnect();
                }

                using (EDDevice ED = new EDDevice(c, new ASCIIProtocol()))
                {
                    ED.Connect();
                    Assert.AreEqual(newDeviceName, ED.Protocol.DeviceName, "The ED device should have kept its news name between connections and instances");

                    ED.FactoryReset();

                    Assert.AreNotEqual(newDeviceName, ED.Protocol.DeviceName, "The ED device should have reverted to its default device name");
                    ED.Disconnect();
                }
            }
        }

        /// <summary>
        /// An invalid command should throw a TimeoutException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void InvalidCommandTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                using (EDDevice ED = new EDDevice(c, new ASCIIProtocol()))
                {
                    ED.Connect();
                    ED.SendCommand("INVALIDCOMMAND");
                }
            }
        }

        /// <summary>
        /// An invalid command should throw a TimeoutException (as the device does not respond)
        /// but the connection should remain and further commands should respond as detailed in the
        /// ASCII spec
        /// </summary>
        [TestMethod]
        public void RecoverFromInvalidCommandTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                using (EDDevice ED = new EDDevice(c, new ASCIIProtocol()))
                {
                    ED.Connect();
                    //read device name response of the form !AA(deviceName)
                    string deviceName = ED.Protocol.DeviceName;
                    try
                    {
                        ED.SendCommand("INVALIDCOMMAND");
                        Assert.Fail("A TimeoutException should have been thrown by the previous command");
                    }
                    catch (TimeoutException)
                    {

                    }
                    Assert.IsTrue(ED.Connection.IsConnected, "device should still be connected");
                    Assert.AreEqual(deviceName, ED.SendCommand("$01M").Substring(3), "device should still have the same name");
                    ED.Disconnect();
                }

            }


        }

    }
}
