using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class EDFactoryTests
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
        public void ED588FactoryTest()
        {
            foreach (IConnection c in cp.ED588Connections.RandomTestOrder().AsASCIIProtocol())
            {

                EDDevice ed = new ED588(c);

                Type type1 = ed.GetType();
                Console.WriteLine("The type device is: " + type1);
                Console.WriteLine("There are " + ed.Outputs.Count + " Ouput");


                string connString = "";
                int data = 0;
                c.CreateConnectionData(out connString, out data);
                ed = EDDevice.Create(connString, data);

                Console.WriteLine("The type device is: " + ed.GetType());
                Console.WriteLine("There are " + ed.Outputs.Count + " Ouput");

                Assert.AreEqual(type1, ed.GetType());
            }
        }
    }
}
