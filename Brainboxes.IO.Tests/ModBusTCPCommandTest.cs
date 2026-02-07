using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class ModBusTCPCommandTest : UnitTestBase
    {

        [TestMethod]
        public void GetAllIOLines()
        {
            foreach (TCPConnection tcp in cp.EDConnections.RandomTestOrder().AsModbusTCP())
            {
                using (EDDevice ed = EDDevice.Create(tcp.IP))
                {
                    ModbusTCPProtocol p = ed.Protocol as ModbusTCPProtocol;

                    int digitalInputs = ed.Inputs.Count;
                    int digitalOutputs = ed.Outputs.Count;
                    int analogInputs = ed.AInputs.Count;
                    int analogOutputs = ed.AOutputs.Count;
                    int expected = 0;
                    // digital IO device
                    if(digitalOutputs > 0 && digitalInputs > 0)
                    {
                        if(digitalOutputs > 0)
                        {
                            p.SetAllDigitalOutputLineStates(0xaaaa, digitalOutputs);
                            for (int i = 0; i < digitalOutputs; i++)
                            {
                                if(i%2 > 0)
                                {
                                    expected |= 1 << i + 16; //set a bit
                                }
                            }
                        }

                        if (digitalInputs > 0)
                        {
                            for (int i = 0; i < digitalInputs; i++)
                            {
                                expected |= 1 << i; //set a bit
                            }
                        }
                        int response = p.GetAllDigitalLineStates();

                        Assert.AreEqual(expected.ToString("x"), response.ToString("x"), "All exisiting inputs should be high (none exisiting low). Odd outputs high, and even inputs low (none exisiting low)");
                    }

                    //TODO: include analog io modbus test

                }
            }
        }


        [TestMethod]
        public void GetAllInputLines()
        {
            foreach (TCPConnection tcp in cp.DevicesWithInputsConnections.RandomTestOrder().AsModbusTCP())
            {
                using (EDDevice ed = EDDevice.Create(tcp.IP))
                {
                    ModbusTCPProtocol p = ed.Protocol as ModbusTCPProtocol;
                    int state = 0xffff;
                    int bitmask = 0;
                    for (int i = 0; i < ed.Inputs.Count; i++)
                    {
                        bitmask |= 1 << i; //set a bit
                    }

                    int response = p.GetAllInputStates();

                    Assert.AreEqual((state & bitmask).ToString("x"), (response & bitmask).ToString("x"), "The inputs should all be 1 HIGH by default");
                }
            }
        }

        [TestMethod]
        public void GetOutputLinesForDevicesWithoutOutputs()
        {
            // Exclude analog-only devices (e.g., ED-549) as they don't have digital outputs
            // and their Modbus output registers may contain unrelated data
            foreach (TCPConnection tcp in cp.EDConnections
                .Except(cp.DevicesWithOutputsConnections)
                .Except(cp.DevicesWithAnalogueInputsConnections)
                .RandomTestOrder().AsModbusTCP())
            {
                using (EDDevice ed = EDDevice.Create(tcp.IP))
                {
                    ModbusTCPProtocol p = ed.Protocol as ModbusTCPProtocol;

                    int response = p.GetAllOutputStates();

                    Assert.AreEqual((0).ToString("x"), (response).ToString("x"), "Devices without outputs should return 0");
                }
            }
        }

        [TestMethod]
        public void SetAllOutputLines()
        {
            foreach(TCPConnection tcp in cp.DevicesWithOutputsConnections.RandomTestOrder().AsModbusTCP())
            {
                using (EDDevice ed = EDDevice.Create(tcp.IP))
                {
                    ModbusTCPProtocol p = ed.Protocol as ModbusTCPProtocol;
                    //takes about 1 minute to test 16 lines
                    int combinations = 1 << ed.Outputs.Count;
                    //should be able to set every possible combination of states
                    for (int i = 0; i < combinations; i++)
                    {
                        p.SetAllDigitalOutputLineStates(i, ed.Outputs.Count);
                    }
                   
                }
            }
        }

        [TestMethod]
        public void GetAllOutputLines()
        {
            //cant do this test on devices with inputs because you can read outputs but cant set them
            foreach (TCPConnection tcp in cp.DevicesWithOutputsConnections.RandomTestOrder().AsModbusTCP())
            {
                using (EDDevice ed = EDDevice.Create(tcp.IP))
                {
                    ModbusTCPProtocol p = ed.Protocol as ModbusTCPProtocol;
                    int[] setStates = { 0xf731, 0x00, 0xff, 0xf0, 0x0f };
                    int bitmask = 0;
                    for(int i = 0; i < ed.Outputs.Count; i++)
                    {
                        bitmask |= 1 << i; //set a bit
                    }
                    //takes about 3 minutes to test 16 lines
                    int combinations = 1 << ed.Outputs.Count;
                    //should be able to set every possible combination of states
                    for (int i = 0; i < combinations; i++)
                    { 
                        p.SetAllDigitalOutputLineStates(i, ed.Outputs.Count);
                        int response = p.GetAllOutputStates();

                        Assert.AreEqual( (i&bitmask).ToString("x"), (response&bitmask).ToString("x"), "After setting IO lines the reponse should match the set state");
                    }
                }
            }
        }

        [TestMethod]
        public void SetOutputLine()
        {
            foreach (TCPConnection tcp in cp.DevicesWithOutputsConnections.RandomTestOrder().AsModbusTCP())
            {
                using (EDDevice ed = EDDevice.Create(tcp.IP))
                {
                    ModbusTCPProtocol p = ed.Protocol as ModbusTCPProtocol;
                    
                    foreach (IOLine line in ed.Outputs)
                    {
                        p.SetDigitalOutputLineState(line.IONumber, 0);
                    }

                    foreach (IOLine line in ed.Outputs)
                    {
                        Assert.AreEqual(0, p.GetDigitalLineState(line.IONumber, false), "IOLine " + line.IONumber + " should be 0");
                    }

                    foreach (IOLine line in ed.Outputs)
                    {
                        p.SetDigitalOutputLineState(line.IONumber, 1);
                    }

                    foreach (IOLine line in ed.Outputs)
                    {
                        Assert.AreEqual(1, p.GetDigitalLineState(line.IONumber, false), "IOLine " + line.IONumber + " should be 1");
                    }

                    foreach (IOLine line in ed.Outputs.Where(line => line.IONumber % 2 == 0))
                    {
                        p.SetDigitalOutputLineState(line.IONumber, 0);
                    }

                    uint expected = ed.Outputs.Count == 8 ? 0xaa0000 : 0xaaaa0000;
                    uint bitmask = ed.Outputs.Count == 8 ? 0xff0000 : 0xffff0000;

                    Assert.AreEqual(expected.ToString("x"), (p.GetAllDigitalLineStates()&bitmask).ToString("x"), "even outputs should be set, odd should be clear");

                }
            }
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

        [TestMethod]
        [TestCategory("LongRunning")]
        [Timeout(300_000)] // 5 minutes - ~10,000 commands at 6ms each = ~60s, with buffer
        public void SendCommandRepeatedly()
        {
            int devicesTestedCount = 0;

            for (int t = 0; t < 10; t++)
            {
                // Thread-safe collection to capture output from parallel threads
                // Console.WriteLine in Parallel.ForEach doesn't get captured by MSTest in .NET 10+
                var outputMessages = new ConcurrentBag<string>();

                Parallel.ForEach(cp.DevicesWithInputsConnections.RandomTestOrder().AsModbusTCP(), c =>
                {
                    System.Threading.Interlocked.Increment(ref devicesTestedCount);
                    var localOutput = new StringBuilder();

                    //use to calculate some stats
                    Stopwatch sw = new Stopwatch();
                    List<double> times = new List<double>();

                    EDDevice ed = new EDDevice(c, new ModbusTCPProtocol());

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
                            //these commands are sent synchronously
                            // read digital input line 0
                            ed.SendCommand("04 0020 0001");
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
            Assert.IsTrue(devicesTestedCount > 0, "No ED connections were available to test");
        }

    }
}
