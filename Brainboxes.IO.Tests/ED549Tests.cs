 using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class ED549Tests
    {
        ConnectionPool cp;
        [TestInitialize]
        public void TestInit()
        {
            cp = new ConnectionPool();
        }
        [TestCleanup]
        public void TestCleanup()
        {          
            cp.CloseAll();
        }
        [TestMethod]
        public void CorrectIOLinesED549Test()
        {
            foreach (IConnection c in cp.ED549Connections.RandomTestOrder())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    Assert.IsTrue(ed549.IOLines.All(line => line.IOType == IOType.Analog && line.IODirection == IODirection.AInput), "All IO lines should be analogue inputs");
                    Assert.AreEqual(8, ed549.AInputs.Count(), "ED-549 has 8 inputs");
                }
            }
        }
        [TestMethod]
        public void ConnectED549Test()
        {
            //ED549 does not have modbus yet
            foreach (IConnection c in cp.ED549Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    ed549.Connect();
                    ed549.Disconnect();
                }
            }
        }
        [TestMethod]
        public void GetIOLinesED549Test()
        {
            foreach (IConnection c in cp.ED549Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    ed549.Connect();
                    ed549.FactoryReset();
                    double[] aValues = ed549.Protocol.GetAllAnalogInputLineStates(8);
                    foreach (IOLine line in ed549.IOLines)
                    {
                        // Tolerance increased to 1.5 to account for analog drift with no signal connected
                        Assert.AreEqual(0d, aValues[line.IONumber], 1.5d, "ED-549 IO Line " + line.ToString() + " should be 0");
                    }
                }
            }
        }

        [TestMethod]
        public void GetIOLinesEnumerableED549Test()
        {
            foreach (IConnection c in cp.ED549Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    int c1 = 0, loopCount = 0;
                    ed549.Connect();

                    while (loopCount < 10)
                    {
                        string randomHex = GenerateRandomHexString(new Random(), 2);
                        ed549.SendCommand("$015" + randomHex);
                        Thread.Sleep(1000);
                        int intValue = Convert.ToInt32(randomHex, 16);
                        string binaryString = Convert.ToString(intValue, 2).PadLeft(randomHex.Length * 4, '0');
                        char[] a = binaryString.ToCharArray();
                        Array.Reverse(a);

                        double[] aValues = ed549.Protocol.GetAllAnalogInputLineStates(8);

                        c1 = 0;
                        foreach (IOLine line in ed549.IOLines)
                        {
                            Console.WriteLine(aValues[line.IONumber]);
                            if (a[c1] == '1')
                            {
                                Assert.IsTrue(aValues[line.IONumber] > -10.0, "ED-549 IO Line " + line.ToString() + " should be valid double. Value: " + aValues[line.IONumber] + " Line number: " + line.IONumber);
                            }
                            else
                            {
                                Assert.IsTrue(double.IsNaN(aValues[line.IONumber]), "ED-549 IO Line " + line.ToString() + " should be NaN. Value: " + aValues[line.IONumber] + " Line number: " + line.IONumber);
                            }
                            c1++;
                        }
                        loopCount++;
                    }
                }
            }
        }



        [TestMethod]
        public void SetGetEngineeringDataFormatTest()
        {

            Regex regexEngineeringFormat = new Regex("[0-9]{2}.[0-9]{3}");

            foreach (IConnection c in cp.ED549Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    ed549.Connect();
                    ASCIIProtocol ap = (ASCIIProtocol)ed549.Protocol;
                    ap.SetAnalogDataFormat(AnalogDataFormat.Engineering);
                    AnalogDataFormat actualDataFormat = ap.DataFormat;
                    Assert.AreEqual(AnalogDataFormat.Engineering, actualDataFormat, "The set data format is " + AnalogDataFormat.Engineering + " but getting back " + actualDataFormat);
                    string response = ap.SendCommand("#01");
                    string[] aValues = response.Split('+');
                    aValues = aValues.Where(a => a != ">").ToArray();
                    foreach (string aValue in aValues)
                    {
                        Console.WriteLine(aValue);

                        Assert.IsTrue(regexEngineeringFormat.IsMatch(aValue), "The analogue input read doesn't match engineering format pattern.");
                    }

                }
            }
        }

        [TestMethod]
        public void SetGetFSRDataFormatTest()
        {

            Regex regexFSRFormat = new Regex("[0-9]{3}.[0-9]{2}");

            foreach (IConnection c in cp.ED549Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    ed549.Connect();
                    ASCIIProtocol ap = (ASCIIProtocol)ed549.Protocol;
                    AnalogDataFormat expectedDataFormat = AnalogDataFormat.FullScaleRange;
                    ap.SetAnalogDataFormat(expectedDataFormat);
                    AnalogDataFormat actualDataFormat = ap.DataFormat;
                    Assert.AreEqual(AnalogDataFormat.FullScaleRange, actualDataFormat, "The set data format is " + expectedDataFormat + " but read back " + actualDataFormat);
                    string response = ap.SendCommand("#01");
                    string[] aValues = response.Split('+');
                    aValues = aValues.Where(a => a != ">").ToArray();

                    foreach (string aValue in aValues)
                    {
                        Console.WriteLine(aValue);
                        Assert.IsTrue(regexFSRFormat.IsMatch(aValue), "The analogue input read doesn't match FSR format pattern.");
                    }

                }
            }

        }

        [TestMethod]
        public void SetGetHexadecimalDataFormatTest()
        {

            Regex regexFSRFormat = new Regex("[0-9A-F]{4}");
            foreach (IConnection c in cp.ED549Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED549 ed549 = new ED549(c))
                {
                    ed549.Connect();

                    ASCIIProtocol ap = (ASCIIProtocol)ed549.Protocol;
                    AnalogDataFormat expectedDataFormat = AnalogDataFormat.Hexadecimal;
                    ap.SetAnalogDataFormat(expectedDataFormat);
                    AnalogDataFormat actualDataFormat = ap.DataFormat;
                    Assert.AreEqual(AnalogDataFormat.Hexadecimal, actualDataFormat, "The set data format is " + expectedDataFormat + " but read back " + actualDataFormat);
                    string response = ap.SendCommand("#01");
                    response = response.Remove(0, 1); // Remove '>' prefix
                    int chunkSize = 4;

                    // Parse actual number of values from response length
                    int valueCount = response.Length / chunkSize;
                    Assert.IsTrue(valueCount > 0, "Response should contain at least one hexadecimal value");

                    for (int i = 0; i < valueCount; i++)
                    {
                        string aValue = response.Substring(i * chunkSize, chunkSize);
                        Console.WriteLine(aValue);
                        Assert.IsTrue(regexFSRFormat.IsMatch(aValue), "The analogue input read doesn't match Hexadecimal format pattern.");
                    }

                }
            }
        }

        string GenerateRandomHexString(Random random, int length)
        {
            // Create a char array to store hexadecimal characters
            char[] hexChars = "0123456789ABCDEF".ToCharArray();

            // Create a char array to store random hexadecimal characters
            char[] hexArray = new char[length];

            // Generate random hexadecimal characters
            for (int i = 0; i < length; i++)
            {
                hexArray[i] = hexChars[random.Next(hexChars.Length)];
            }

            // Convert the char array to a string and return
            return new string(hexArray);
        }
    }
}
