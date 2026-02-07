using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class ED582Tests : UnitTestBase
    {

        int eventCount = 0;

        [TestMethod]
        public void ConnectTest()
        {
            //ED582 does not have modbus yet
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();            
                    ed582.Disconnect();
                }
            }
        }
        
        [TestMethod]
        public void CheckAnalogIOLinesTest()
        {
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    Assert.IsTrue(ed582.IOLines.All(line => line.IOType == IOType.Analog && line.IODirection == IODirection.AInput), "All IO lines should be analogue inputs");
                    Assert.AreEqual(4, ed582.AInputs.Count(), "ED-582 has 4 inputs");
                }
            }
        }

        [TestMethod]
        public void GetIOLinesTest()
        {
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();
                    ed582.FactoryReset();
                    double[] aValues = ed582.Protocol.GetAllAnalogInputLineStates(ed582.IOLines.Count);
                    foreach (IOLine line in ed582.IOLines)
                    {
                        Assert.IsTrue(aValues[line.IONumber] >= -9999.9d && aValues[line.IONumber] <= +9999.9d, "ED-582 IO Line " + line.ToString() + " should be between -9999.9 and +9999.9");
                    }
                }
            }
        } 
        
        [TestMethod]
        public void SetDataFormatFahrenheit()
        {
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();
                    ASCIIProtocol ap = (ASCIIProtocol)ed582.Protocol;
                    TemperatureUnit expectedTemperatureUnit = TemperatureUnit.Fahrenheit;
                    ap.SetTemperatureUnit(expectedTemperatureUnit);
                    TemperatureUnit actualTemperatureUnit = ap.TemperatureUnit;
                    Assert.AreEqual(expectedTemperatureUnit, actualTemperatureUnit, "The set data format is " + expectedTemperatureUnit + " but read back " + actualTemperatureUnit);
                    ed582.Restart();
                    double[] aValues = ed582.Protocol.GetAllAnalogInputLineStates(ed582.IOLines.Count);
                 
                    Assert.IsTrue(aValues[3] >= 70, "ED-582 IO Line 3 should be above 70");                   
                }
            }
        }

        [TestMethod]
        public void SetDataFormatCelsius()
        {
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();
                    ASCIIProtocol ap = (ASCIIProtocol)ed582.Protocol;
                    TemperatureUnit expectedTemperatureUnit = TemperatureUnit.Celsius;
                    ap.SetTemperatureUnit(expectedTemperatureUnit);
                    TemperatureUnit actualTemperatureUnit = ap.TemperatureUnit;
                    Assert.AreEqual(expectedTemperatureUnit, actualTemperatureUnit, "The set data format is " + expectedTemperatureUnit + " but read back " + actualTemperatureUnit);
                    ed582.Restart();
                    double[] aValues = ed582.Protocol.GetAllAnalogInputLineStates(ed582.IOLines.Count);

                    Assert.IsTrue(aValues[3] >= 18, "ED-582 IO Line 3 should be above 18");
                }
            }
        }

        [TestMethod]
        public void SetDataFormatKelvin()
        {
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();
                    ASCIIProtocol ap = (ASCIIProtocol)ed582.Protocol;
                    TemperatureUnit expectedTemperatureUnit = TemperatureUnit.Kelvin;
                    ap.SetTemperatureUnit(expectedTemperatureUnit);
                    TemperatureUnit actualTemperatureUnit = ap.TemperatureUnit;
                    Assert.AreEqual(expectedTemperatureUnit, actualTemperatureUnit, "The set data format is " + expectedTemperatureUnit + " but read back " + actualTemperatureUnit);
                    ed582.Restart();
                    double[] aValues = ed582.Protocol.GetAllAnalogInputLineStates(ed582.IOLines.Count);

                    Assert.IsTrue(aValues[3] >= 273, "ED-582 IO Line 3 should be above 273");
                }
            }
        }


        [TestMethod]
        public void TargetEventRaisedTest()
        {
            // This function doesn't have pass/fail criteria. This is used only to test if the event is triggered in real time.
            int count = 0;
            foreach (IConnection c in cp.ED582Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();
                    double target = 25.00;
                    AIOLineChangedEventHandler temperatureEventHandler = TemperatureEventHandlerFunction;
                    ed582.IOLines[0].SubscribeToTargetEvent(ref temperatureEventHandler, target);
                    Console.WriteLine(ed582.IOLines[0].AValue);
                    while (count < 80)
                    {
                        Console.WriteLine(this.eventCount);
                        Thread.Sleep(250);
                        count++;
                    }
                }
            }
        }

        [TestMethod]
        public void TargetrangeEventTest()
        {
            // This function doesn't have pass/fail criteria. This is used only to test if the event is triggered in real time.

            int count = 0;

            foreach (IConnection c in cp.ED582Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED582 ed582 = new ED582(c))
                {
                    ed582.Connect();
                    double target = 20.00,
                        range = 2.0; // 18.0 and 22.0
                    AIOLineChangedEventHandler temperatureEventHandler = TemperatureEventHandlerFunction;
                    ed582.IOLines[0].SubscribeToTargetRangeEvent(ref temperatureEventHandler, target, range);
                    Console.WriteLine(ed582.IOLines[0].AValue);
                    while (count < 100)
                    {
                        Console.WriteLine(this.eventCount);
                        Thread.Sleep(250);
                        count++;
                    }
                }
            }
        }
        private void TemperatureEventHandlerFunction(IOLine line, EDDevice device, double value, AIOChangeTypes changeType)
        {
            this.eventCount++;
            Console.WriteLine("Event triggered!");
        }


    }
}
