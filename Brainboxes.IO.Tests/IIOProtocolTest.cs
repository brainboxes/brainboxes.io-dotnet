using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;


namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class IIOProtocolTest
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
        public void GetAllLineStates()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void SetAllOutputLineStates()
        {
            foreach (IConnection c in cp.DevicesWithOutputsConnections.RandomTestOrder().AsASCIIProtocol())
            {
                string connectionString = "";
                int portBaudRate = 0;
                c.CreateConnectionData(out connectionString, out portBaudRate);

                using (EDDevice ed = EDDevice.Create(connectionString, portBaudRate))
                {
                    int numberOfOutputs = ed.Outputs.Count;
                    int startIndex = numberOfOutputs == 8 ? 8 : 0;
                    int endIndex = 16;

                    ed.Protocol.SetAllDigitalOutputLineStates(0, numberOfOutputs);
                    Assert.AreEqual(0, ed.Protocol.GetAllDigitalLineStates() >> 8, "all outputs should be set open");

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        Assert.AreEqual(0, ed.Protocol.GetDigitalLineState(i), "DOUT" + i + " should be set OPEN 0");
                    }

                    ed.Protocol.SetAllDigitalOutputLineStates(1, numberOfOutputs);
                    Assert.AreEqual(1, ed.Protocol.GetAllDigitalLineStates() >> startIndex, "DOUT0 should be closed all else open");
                    Assert.AreEqual(1, ed.Protocol.GetDigitalLineState(0 + startIndex), "DOUT0 should be set CLOSED 1");
                    Assert.AreEqual(0, ed.Protocol.GetDigitalLineState(1 + startIndex), "DOUT1 should be set OPEN 0");

                    ed.Protocol.SetAllDigitalOutputLineStates(3, numberOfOutputs);
                    Assert.AreEqual(3, ed.Protocol.GetAllDigitalLineStates() >> startIndex, "DOUT0 and DOUT1 should be closed all else open");
                    Assert.AreEqual(1, ed.Protocol.GetDigitalLineState(0 + startIndex), "DOUT0 should be set CLOSED 1");
                    Assert.AreEqual(1, ed.Protocol.GetDigitalLineState(1 + startIndex), "DOUT1 should be set CLOSED 1");
                    Assert.AreEqual(0, ed.Protocol.GetDigitalLineState(2 + startIndex), "DOUT2 should be set OPEN 0");

                    ed.Protocol.SetAllDigitalOutputLineStates(2, numberOfOutputs);
                    Assert.AreEqual(2, ed.Protocol.GetAllDigitalLineStates() >> startIndex, "DOUT1 should be closed all else open");
                    Assert.AreEqual(0, ed.Protocol.GetDigitalLineState(0 + startIndex), "DOUT0 should be set OPEN 0");
                    Assert.AreEqual(1, ed.Protocol.GetDigitalLineState(1 + startIndex), "DOUT1 should be set CLOSED 1");
                    Assert.AreEqual(0, ed.Protocol.GetDigitalLineState(2 + startIndex), "DOUT2 should be set OPEN 0");

                }
            }
        }

        [TestMethod]
        public void SetValueOfUnconnectedDevice()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsEachProtocol())
            {
                try
                {
                    ED588 ed588 = new ED588(c);
                    ed588.Outputs[0].Value = 1;
                    Assert.Fail("Should not be able to set an output if the device is not connected");
                }
                catch (InvalidOperationException)
                {
                    
                }
            }
        }

        [TestMethod]
        public void SetOutputLineState()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void GetLineState()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void LatchedInputs()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void ClearCount()
        {
            foreach (IConnection c in cp.DevicesWithInputsConnections.RandomTestOrder().AsEachProtocol())
            {
                string connectionString = "";
                int portBaudRate = 0;
                c.CreateConnectionData(out connectionString, out portBaudRate);

               using(EDDevice ed = EDDevice.Create(connectionString, portBaudRate))
               {
                   foreach(IOLine line in ed.Inputs)
                   {
                       Console.WriteLine(line + " Count: " + line.Count);
                       line.ClearCount();
                       Assert.AreEqual(0, line.Count, "After reset the count of an input should be 0");
                   }
                   System.Threading.Thread.Sleep(ed.IOLineCacheTimeout * 2);

                   foreach (IOLine line in ed.Inputs)
                   {
                       Assert.AreEqual(0, line.Count, "After reset and cache timeout count of an input should be 0");
                   }
               }
            }
        }
    }
}
