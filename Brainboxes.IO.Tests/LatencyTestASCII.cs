using HdrHistogram;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Brainboxes.IO.Tests
{
    /// <summary>
    /// Perform Latency tests on ASCII devices
    /// send and receive a particular ASCII command repeatedly to the device in 1 or more threads
    /// record result of each transaction to a HDR histogram
    /// WARNING: these tests can take a long time to run (up to 20 minutes each) 
    /// </summary>
    [TestClass]
    [Ignore("These tests are very long running and is not suitable for regular test runs, but can be used for manual performance testing when needed")]
    public class LatencyTestASCII : UnitTestBase
    {
        // shared random object for all tests
        readonly Random rand = new Random();

        // random is not thread safe so we need a lock
        readonly object randLock = new object();

        /// <summary>
        /// #AA1N0FData ED-560 soak test: set all 4 analog output channels to random voltages (0-10V)
        /// </summary>
        [TestMethod]
        [Timeout(1200_000)] // 20 minutes
        public void AA1N0FLatencyTest()
        {
            string name = "#AA1N0F-soak";

            // always set all 4 channels (0F = channels 0-3) with random voltages 0-10V
            string commandGenerator()
            {
                string voltageString = "";
                for (int channel = 0; channel < 4; channel++)
                {
                    double voltage;
                    lock (randLock)
                    {
                        voltage = rand.NextDouble() * 10.0;
                    }
                    voltageString += string.Format("{0:+00.000}", voltage);
                }
                return $"#011N0F{voltageString}";
            }

            var devicesToTest = cp.ED560Connections;

            fullHDRLatencyTest(commandGenerator, name, devicesToTest);
        }

        /// <summary>
        /// @AA Read the status of the digital I/O lines.
        /// http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%40AA.pdf
        /// </summary>
        [TestMethod]
        [Timeout(1200_000)] // 20 minutes
        public void AtAALatencyTest()
        {
            string name = "@AA";

            // always return constant
            string commandGenerator() => "@01";

            // test ed devices with digital inputs or outputs
            var devicesToTest = cp.DevicesWithDigitalIOConnections;

            fullHDRLatencyTest(commandGenerator, name, devicesToTest);
        }

        /// <summary>
        /// @AA(Data) Sets the digital output channel value at a specified address
        /// http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%40AA(Data).pdf
        /// </summary>
        [TestMethod]
        [Timeout(1200_000)] // 20 minutes
        public void AtAADDLatencyTest()
        {
            string name = "@AA(Data)";

            // channel (Data) is a random variable from 0 to 65535 to set up to 16 outputs
            string commandGenerator()
            {
                lock (randLock)
                {
                    return string.Format("@01{0:X4}", rand.Next(0, 0xffff));
                }
            }

            // test ed devices with digital outputs
            var devicesToTest = cp.DevicesWithOutputsConnections;

            fullHDRLatencyTest(commandGenerator, name, devicesToTest);
        }

        /// <summary>
        /// #AAN Read the digital input counter value of specified channel.
        /// http://www.brainboxes.com/files/catalog/product/ED/ED-588/documents/ED%20Range%20Manual.pdf#page=68
        /// </summary>
        [TestMethod]
        [Timeout(1200_000)] // 20 minutes
        public void HashAANLatencyTest()
        {
            string name = "#AAN";

            // channel N is a random variable from 0 to max input channel number
            string commandGenerator()
            {
                lock (randLock)
                {
                    return string.Format("#01{0:X1}", rand.Next(0, 0xf));
                }
            }

            // test ed devices with digital inputs
            var devicesToTest = cp.DevicesWithInputsConnections;

            fullHDRLatencyTest(commandGenerator, name, devicesToTest);
        }

        /// <summary>
        /// #AA1NVVData Set multiple analog output lines simultaneously
        /// Originally requested by Complete Genomics
        /// </summary>
        [TestMethod]
        [Timeout(1200_000)] // 20 minutes
        public void HashAA1NVVDataAnalogSetMultipleLines()
        {
            string name = "#AA1NVVData";

            // set between 1 and 4 outputs each to values between +000.00 and +099.99 % FSR
            string commandGenerator()
            {
                int channelsToSet = 0;
                lock (randLock)
                {
                    // the command supports up to 0xff but our current device ED-560 only has 4 outputs
                    // max value is exclusive
                    channelsToSet = rand.Next(1, 0x1f);
                }
                // there's only 4 channels so its easiest to check each bit individually
                int numberOfChannels = (channelsToSet & 1) + (channelsToSet >> 1 & 1) + (channelsToSet >> 2 & 1) + (channelsToSet >> 3 & 1);

                string values = "";
                for (int i = 0; i < numberOfChannels; i++)
                {
                    lock (randLock)
                    {
                        // % FSR format
                        values += string.Format("+{0:000.##}", rand.NextDouble() * 100);
                    }
                }
                return string.Format("#011N{0:X2}{1}", channelsToSet, values);
            };

            // test ed devices with analog outputs
            var devicesToTest = cp.DevicesWithAnalogueOutputsConnections;

            fullHDRLatencyTest(commandGenerator, name, devicesToTest);
        }

        /// <summary>
        /// Perform Full test suite of HDR Latency tests
        /// </summary>
        /// <param name="nextCommand">Next command generator</param>
        /// <param name="commandName">Command Name</param>
        /// <param name="devicesToTest">List of Devices to Test for the connection pool</param>
        /// <param name="totalRequests">Total number of request to perform and record latency</param>
        private void fullHDRLatencyTest(Func<string> nextCommand, string commandName, Dictionary<string, IConnection> devicesToTest, int totalRequests = 1_0_000)
        {
            if (!devicesToTest.Any())
            {
                Assert.Inconclusive("No devices to test against");
            }
            // 1, then 2, then 4, then 8 concurrent connections
            hdrLatencyTest(nextCommand, commandName, devicesToTest, 1, totalRequests);
            hdrLatencyTest(nextCommand, commandName, devicesToTest, 2, totalRequests);
            hdrLatencyTest(nextCommand, commandName, devicesToTest, 4, totalRequests);
            hdrLatencyTest(nextCommand, commandName, devicesToTest, 8, totalRequests);
        }
        /// <summary>
        /// PerformHDRHistogram response time data capture of specific ASCII command on a set of ASCII ED devices
        /// </summary>
        /// <param name="nextCommand">Function to generate next command</param>
        /// <param name="commandName">The name of the command for saving the histogram data to file</param>
        /// <param name="devicesToTest">a list of devices to test</param>
        /// <param name="parallelRequests">number of requests to perform in parallels</param>
        /// <param name="iterations">total number of requests to make across parallel connections</param>
        private void hdrLatencyTest(Func<string> nextCommand, string commandName, Dictionary<string, IConnection> devicesToTest, int parallelRequests = 1, int totalRequests = 1_0_000)
        {
            foreach (var c in devicesToTest.RandomTestOrder().AsASCIIProtocol())
            {
                // A Histogram covering the range from 1 tick (100ns) to 1 hour (3600,000,000,000 ns) with a resolution of 3 significant figures:
                // concurrent so it can be written to from different threads
                var histogram = new LongConcurrentHistogram(1, TimeStamp.Hours(1), 3);
                string ip;
                int port;
                c.CreateConnectionData(out ip, out port);

                string deviceType = "";
                // use create method so we can determine the correct device type for saving the results
                using (EDDevice ed = EDDevice.Create(ip))
                {
                    deviceType = ed.GetType().Name.ToUpper();
                }

                Parallel.For(0, parallelRequests, (p) =>
                {
                    // timeout 10 seconds
                    EDDevice ed = new EDDevice(new TCPConnection(ip, port, 10000), new ASCIIProtocol());
                    int i = 0;
                    long startTimestamp = Stopwatch.GetTimestamp();
                    int requestsPerLoop = totalRequests / parallelRequests;
                    try
                    {
                        ed.Connect();
                        for (; i < requestsPerLoop; i++)
                        {
                            //generate the next command
                            string newCommand = nextCommand();
                            startTimestamp = Stopwatch.GetTimestamp();
                            // these commands are sent synchronously
                            ed.SendCommand(newCommand);
                            long elapsed = Stopwatch.GetTimestamp() - startTimestamp;
                            histogram.RecordValue(elapsed);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(c);
                        Console.WriteLine("Failed on the " + i + " command.");
                        long elapsed = Stopwatch.GetTimestamp() - startTimestamp;
                        histogram.RecordValue(elapsed);
                        Debug.WriteLine("Failed Command Duration: {0:0.00} ms", (elapsed * 1000 / Stopwatch.Frequency));
                    }
                    finally
                    {
                        ed.Disconnect();
                    }
                });
                var writer = new StringWriter();
                // Output the percentile distribution of our results to the Console with values presented in Milliseconds
                histogram.OutputPercentileDistribution(
                    writer: writer,
                    outputValueUnitScalingRatio: OutputScalingFactor.TimeStampToMilliseconds);
                Console.WriteLine(writer.ToString());
                // write out to file
                // these can be plotted online here: https://hdrhistogram.github.io/HdrHistogram/plotFiles.html
                // write workign directory to output so it gets picked up by test explorer
                string fileName = $"{deviceType}-{commandName}-{parallelRequests}x.hgrm";
                Console.WriteLine("Working Directory: " + Environment.CurrentDirectory);
                Console.WriteLine("Writing results to: " + fileName);
                File.WriteAllText(fileName, writer.ToString());
            }
        }
    }
}