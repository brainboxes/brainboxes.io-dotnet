using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class ED527Tests
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

        /// <summary>
        /// test that the ED527 has the correct IOLines,
        /// they should all be outputs numbered from 0 to 15
        /// </summary>
        [TestMethod]
        public void CorrectIOLinesED527Test()
        {
            using (ED527 ed527 = new ED527())
            {

                Assert.IsTrue(ed527.IOLines.All(line => line.IOType == IOType.Digital && line.IODirection == IODirection.Output), "All IO Lines should be digital outputs");

                Assert.AreEqual(0, ed527.Inputs.Count(), "ED-560 has 0 inputs");
                Assert.AreEqual(16, ed527.Outputs.Count(), "ED-527 has 16 outputs");

                Assert.IsTrue(ed527.Outputs.All(line => line.IOType == IOType.Digital && line.IODirection == IODirection.Output), "The outputs are outputs");
            }
        }

        [TestMethod]
        public void ConnectED527Test()
        {
            foreach (IConnection c in cp.ED527Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED527 ed527 = new ED527(c))
                {
                    ed527.Connect();
                    ed527.Disconnect();
                }
            }
        }

        //currently only available with ascii
        [TestMethod]
        public void GetIOLinesED527Test()
        {
            foreach (IConnection c in cp.ED527Connections.RandomTestOrder().AsASCIIProtocol())
            {
                using (ED527 ed527 = new ED527(c))
                {
                    ed527.Connect();
                    ed527.FactoryReset();

                    foreach (IOLine line in ed527.IOLines)
                    {
                        Assert.AreEqual(0, line.Value, "ED-527 IO Line " + line.ToString() + " should default to OPEN");
                    }
                    ed527.Disconnect();
                }
            }
        }

        [TestMethod]
        public void GetSetIOLinesED527Test()
        {
            foreach (IConnection c in cp.ED527Connections.RandomTestOrder().AsEachProtocol())
            {
                using (ED527 ed527 = new ED527(c))
                {

                    ed527.Connect();
                    try
                    {
                        ed527.FactoryReset();
                    }
                    catch (NotImplementedException)
                    {
                        //not implemented in modbus
                    }

                    //pick some random lines
                    Random r = new Random();
                    List<IOLine> randomLines = ed527.IOLines.OrderBy(line => r.Next()).Take(8).ToList();

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
                        Assert.AreEqual(1, line.Value, "ED-527 IO Line " + line.ToString() + " should have been set closed");
                    }
                    //check the remaining lines are still open
                    foreach (IOLine line in ed527.IOLines.Except(randomLines))
                    {
                        Assert.AreEqual(0, line.Value, "ED-527 IO Line " + line.ToString() + " should still be OPEN as per default");
                    }
                    ed527.Disconnect();
                }
            }
        }
    }
}
