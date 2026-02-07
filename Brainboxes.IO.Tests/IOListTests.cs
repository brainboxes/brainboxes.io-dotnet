using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    [TestCategory(TestCategories.IOLine)]
    public class IOListTests
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
        public void SetAllValuesTest()
        {
            foreach (IConnection c in cp.DevicesWithInputsConnections.RandomTestOrder().AsEachProtocol())
            {
                string connectionString = "";
                int connectionData;

                c.CreateConnectionData(out connectionString, out connectionData);

                using (EDDevice ED = EDDevice.Create(connectionString, connectionData))
                {
                    ED.Connect();
                    //inputs float high
                    Assert.IsTrue(ED.Inputs.All(l => l.Value == 1), "All inputs float high");
                    //has no effect
                    ED.Inputs.Values = 0;
                    Assert.IsTrue(ED.Inputs.All(l => l.Value == 1), "All inputs are still high");

                    //inputs float high
                    ED.Outputs.Values = 0;
                    Assert.IsTrue(ED.Outputs.All(l => l.Value == 0), "All inputs default low");
                    //set all high
                    ED.Outputs.Values = 1;
                    Assert.IsTrue(ED.Outputs.All(l => l.Value == 1), "All inputs are should be high");

                    //set all high
                    ED.Outputs.Values = 0;
                    Assert.IsTrue(ED.Outputs.All(l => l.Value == 0), "All inputs are should be low");

                }
            }
        }

        [TestMethod]
        public void EventsOnOutputsTest()
        {
            int numberOfTimesEventHandlerCalled = 0;
            IOLineChangedEventHandler ioChanged = (line, device, type) =>
            {
                Debug.WriteLine("");
                Debug.WriteLine("==== ioChanged event handler called for line: " + line + " ====");
                Debug.WriteLine("");
                numberOfTimesEventHandlerCalled++;
            };

            IOLineChangedEventHandler ioRisingEdge = (line, device, type) =>
            {
                Debug.WriteLine("");
                Debug.WriteLine("==== ioRisingEdge event handler called for line: " + line + " ====");
                Debug.WriteLine("");
                numberOfTimesEventHandlerCalled++;
            };

            IOLineChangedEventHandler ioFallingEdge = (line, device, type) =>
            {
                Debug.WriteLine("");
                Debug.WriteLine("==== ioFallingEdge event handler called for line: " + line + " ====");
                Debug.WriteLine("");
                numberOfTimesEventHandlerCalled++;
            };

            foreach (IConnection c in cp.DevicesWithOutputsConnections.RandomTestOrder().AsEachProtocol())
            {
                numberOfTimesEventHandlerCalled = 0;
                string connectionString = "";
                int connectionData;

                c.CreateConnectionData(out connectionString, out connectionData);

                using (EDDevice ED = EDDevice.Create(connectionString, connectionData))
                {
                    ED.Connect();

                    //set them all to zero
                    ED.Outputs.Values = 0;

                    //register for all outputs
                    ED.Outputs.IOLineChange += ioChanged;

                    //close, open by default
                    ED.Outputs[0].Value = 1;

                    Assert.AreEqual(1, numberOfTimesEventHandlerCalled, "The event handler should have been called once");

                    ED.Outputs.Take(3).ToList().ForEach(line => line.Value = 1);

                    Assert.AreEqual(3, numberOfTimesEventHandlerCalled, "The event handler should have been called 3 times");

                    ED.Outputs[0].Value = 1;
                    Assert.AreEqual(3, numberOfTimesEventHandlerCalled, "The event handler should still have been called 3 times");

                    ED.Outputs.IOLineChange -= ioChanged;

                    ED.Outputs.Values = 0;
                    Assert.AreEqual(3, numberOfTimesEventHandlerCalled, "The event handler should still have been called 3 times");

                }

            }

        }
    }
}
