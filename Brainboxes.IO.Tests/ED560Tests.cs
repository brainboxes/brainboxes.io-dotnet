using Microsoft.VisualStudio.TestTools.UnitTesting;
using HdrHistogram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class ED560Tests : UnitTestBase
    {

        [TestMethod]
        public void CorrectIOLinesED560()
        {
            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsEachProtocol())
            {
                string connectionString = "";
                int data = 0;
                c.CreateConnectionData(out connectionString, out data);
                using (EDDevice ed560 = EDDevice.Create(connectionString, data))
                {
                    ed560.Connect();
                    Assert.IsTrue(ed560.IOLines.All(line => line.IOType == IOType.Analog && line.IODirection == IODirection.AOutput), "All IO Lines should be analogue & Outputs");
                    Assert.AreEqual(4, ed560.AOutputs.Count(), "ED-560 has 4 analog outputs");
                    Assert.AreEqual(0, ed560.AInputs.Count(), "ED-560 has no analog inputs");
                    Assert.AreEqual(typeof(ED560), ed560.GetType());
                }
            }
        }
        [TestMethod]
        public void ConnectED560Test()
        {
            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED560 ed560 = new ED560(c))
                {
                    ed560.Connect();
                    ed560.Disconnect();
                }
            }
        }
        [TestMethod]
        public void GetIOLinesED560()
        {
            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (EDDevice ed560 = new ED560(c))
                {
                    ed560.Connect();
                    ed560.Restart();
                    foreach (IOLine line in ed560.IOLines)
                    {
                        Assert.AreEqual(0, line.AValue, "ED-560 IO Line " + line.ToString() + " should be at 0");
                    }
                }
            }
        }

        [TestMethod]
        public void GetSetIOLinesED560Test()
        {
            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsEachProtocol())
            {
                using (EDDevice ed560 = new ED560(c))
                {
                    ed560.Connect();
                    try
                    {
                        ed560.FactoryReset();
                    }
                    catch (NotImplementedException)
                    {

                    }
                    //Pick some random lines
                    Random r = new Random();
                    for (int loop = 0; loop < 10; loop++)
                    {
                        List<IOLine> randomLines = ed560.IOLines.OrderBy(line => r.Next()).Take(3).ToList();
                        List<double> randomNewValues = new List<double>(randomLines.Count);

                        foreach (IOLine line in ed560.IOLines.Except(randomLines))
                        {
                            line.AValue = 0;
                        }
                        //set the lines to random number between 0 and 100
                        foreach (IOLine line in randomLines)
                        {
                            double newVal = r.NextDouble() * 10;
                            Console.WriteLine("Setting " + line.ToString() + "To " + newVal);
                            line.AValue = newVal;
                            randomNewValues.Add(newVal);
                        }
                        //Check that it is 5
                        for (int i = 0; i < randomLines.Count; i++)
                        {
                            IOLine line = randomLines[i];
                            double expectedVal = randomNewValues[i];
                            Console.WriteLine("Getting " + line.ToString());
                            Assert.AreEqual(expectedVal, line.AValue, 0.01, "ED-560 IO Line " + line.ToString() + " should have been set to " + expectedVal);
                        }
                        //Check that the remaining lines are set to 0
                        foreach (IOLine line in ed560.IOLines.Except(randomLines))
                        {
                            Assert.AreEqual(0, line.AValue, "ED-560 IO Line " + line.ToString() + " should still be at 0 per default");
                        }
                    }
                    // reset values
                    foreach (IOLine line in ed560.IOLines)
                    {
                        line.AValue = 0;
                    }
                    ed560.Disconnect();
                }
            }
        }
        [TestMethod]
        public void GetDefaultIOLineStatesED560Test()
        {
            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED560 ed560 = new ED560(c))
                {
                    ed560.Connect();
                    try
                    {
                        ed560.FactoryReset();
                    }
                    catch (NotImplementedException)
                    {

                    }
                    foreach (IOLine line in ed560.AOutputs)
                    {
                        Assert.AreEqual(0, ed560.Protocol.GetAnalogLineState(line.IONumber, isInput: false), "ED-560 Output should default to 0");
                    }
                    ed560.Disconnect();
                }
            }
        }
        [TestMethod]
        public void EventHandlingTestED560()
        {
            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED560 ed560 = new ED560(c))
                {
                    ed560.Connect();
                }
            }
        }
        [TestMethod]
        public void ProtocolED560Test()
        {
            double[] testValues = new double[] { 4.983d, 1.999d, 6.924d, 8.923d };

            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsEachProtocol())
            {
                using (EDDevice edOut = new ED560(c))
                {
                    edOut.Connect();
                    

                    for (int i = 0; i < testValues.Count(); i++)
                    {
                        edOut.Protocol.SetAnalogOutputLineState(i, testValues[i]);
                        double lineState = edOut.Protocol.GetAnalogLineState(i, isInput: false);
                        Assert.AreEqual(testValues[i], lineState);
                    }
                    double[] lineStates = edOut.Protocol.GetAllAnalogOutputLineStates();
                    for (int i = 0; i < testValues.Count(); i++)
                    {
                        Assert.AreEqual(testValues[i], lineStates[i]);
                    }

                }
            }
        }

        [TestMethod]
        public void SetGetEngineeringDataFormatTest()
        {             
            Regex regexExpectedFormat = new Regex("[0-9]{2}.[0-9]{3}"); // engineering data format
            Random r = new Random();

            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED560 ed560 = new ED560(c))
                {
                    ed560.Connect();
                    string response = "",
                            command = "",
                            expectedVoltage = $"{r.NextDouble() * 10.0:00.000}";

                    ASCIIProtocol ap = (ASCIIProtocol)ed560.Protocol;
                    // setting data format
                    ap.SetAnalogDataFormat(AnalogDataFormat.Engineering);

                    // reading the data format from the device
                    AnalogDataFormat actualDataFormat = ap.DataFormat;

                    Assert.AreEqual(AnalogDataFormat.Engineering, actualDataFormat, "The set data format is " + AnalogDataFormat.Engineering + " but getting back " + actualDataFormat);
                    // setting the analog lines
                    for (int lineNumber = 0; lineNumber < ed560.AOutputs.Count; lineNumber++)
                    {
                        ap.SetAnalogOutputLineState(lineNumber, expectedVoltage);
                    }

                    // reading the line outputs to check if in expected format
                    foreach (IOLine line in ed560.AOutputs)
                    {
                        command = string.Format("${0:X2}6{1}", ap.Address, line.IONumber);
                        response = ap.SendCommand(command);
                        // removing !aa+ from the response
                        response = response.Remove(0, 4);
                        Console.WriteLine(response);
                        Assert.IsTrue(regexExpectedFormat.IsMatch(response), "The analogue input read doesn't match engineering format pattern.");
                        Assert.AreEqual(expectedVoltage, response, "The output voltage is not as expected");
                    }
                }
            }
        }

        [TestMethod]
        public void SetGetFSRDataFormatTest()
        {
            Regex regexExpectedFormat = new Regex("[0-9]{3}.[0-9]{2}"); // FSR data format
            Random r = new Random();

            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED560 ed560 = new ED560(c))
                {
                    ed560.Connect();
                    string response = "",
                            command = "",
                            expectedVoltage = $"{r.NextDouble() * 100.0:000.00}";

                    ASCIIProtocol ap = (ASCIIProtocol)ed560.Protocol;
                    // setting data format
                    ap.SetAnalogDataFormat(AnalogDataFormat.FullScaleRange);

                    // reading the data format from the device
                    AnalogDataFormat actualDataFormat = ap.DataFormat;

                    Assert.AreEqual(AnalogDataFormat.FullScaleRange, actualDataFormat, "The set data format is " + AnalogDataFormat.FullScaleRange + " but getting back " + actualDataFormat);

                    // setting the analog lines
                    for (int lineNumber = 0; lineNumber < ed560.AOutputs.Count; lineNumber++)
                    {
                        ap.SetAnalogOutputLineState(lineNumber, expectedVoltage);
                    }

                    // reading the line outputs to check if in expected format
                    foreach (IOLine line in ed560.AOutputs)
                    {
                        command = string.Format("${0:X2}6{1}", ap.Address, line.IONumber);
                        response = ap.SendCommand(command);
                        // removing !aa+ from the response
                        response = response.Remove(0, 4);
                        Console.WriteLine(response);
                        Assert.IsTrue(regexExpectedFormat.IsMatch(response), "The analogue input read doesn't match FSR format pattern.");
                        Assert.AreEqual(expectedVoltage, response, "The output voltage is not as expected");

                    }
                }
            }

        }

        [TestMethod]
        public void SetGetHexadecimalDataFormatTest()
        {
            Regex regexExpectedFormat = new Regex("[0-9A-F]{4}"); // hexadecimal data format
            Random r = new Random();

            foreach (IConnection c in cp.ED560Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED560 ed560 = new ED560(c))
                {
                    ed560.Connect();
                    string response = "",
                            command = "",
                            expectedVoltage = r.Next(1, 99).ToString("X4");

                    ASCIIProtocol ap = (ASCIIProtocol)ed560.Protocol;
                    // setting data format
                    ap.SetAnalogDataFormat(AnalogDataFormat.Hexadecimal);
                    
                    // reading the data format from the device
                    AnalogDataFormat actualDataFormat = ap.DataFormat;

                    Assert.AreEqual(AnalogDataFormat.Hexadecimal, actualDataFormat, "The set data format is " + AnalogDataFormat.Hexadecimal + " but getting back " + actualDataFormat);

                    // setting the analog lines
                    for (int lineNumber = 0; lineNumber < ed560.AOutputs.Count; lineNumber++)
                    {
                        ap.SetAnalogOutputLineState(lineNumber, expectedVoltage); 
                    }

                    // reading the line outputs to check if in expected format
                    foreach (IOLine line in ed560.AOutputs)
                    {
                        command = string.Format("${0:X2}6{1}", ap.Address, line.IONumber);
                        response = ap.SendCommand(command);
                        // removing !aa from the response
                        response = response.Remove(0, 3);
                        Console.WriteLine(response);
                        Assert.IsTrue(regexExpectedFormat.IsMatch(response), "The analogue input read doesn't match hexadecimal format pattern.");
                        Assert.AreEqual(expectedVoltage, response, "The output voltage is not as expected");

                    }


                }
            }
        }


    }
}
