using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class ESDeviceTest
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
        public void ESOpenCloseTest()
        {
            foreach(TCPConnection c in cp.ESConnections.RandomTestOrder())
            {
                ESDevice es = ESDevice.Create(c.IP);
                Assert.AreEqual(es.GetType(), typeof(ES701), "The device created should be an ES-701");
            }
        }

        [TestMethod]
        public void ESNonStandardProtocolTest()
        {
            foreach (TCPConnection c in cp.ESConnections.RandomTestOrder())
            {
                DefaultSerialProtocol protocol = new DefaultSerialProtocol(){
                    TerminatingCharacters = "\r\r\n\r\r\n"
                };
                ESDevice es = new ES701(c.IP, protocol);
                Assert.AreEqual(es.GetType(), typeof(ES701), "The device created should be an ES-701");
                foreach(BBSerialPort p in es.Ports)
                {
                    Assert.AreEqual(protocol.TerminatingCharacters, ((DefaultSerialProtocol)p.Protocol).TerminatingCharacters, "The terminating characters must be the same as that specified");
                }
            }
        }

        [TestMethod]
        public void Es257TxRxTest()
        {
            foreach (TCPConnection c in cp.ES257Connections.Where(kvp => kvp.Value is TCPConnection).RandomTestOrder())
            {
                ES257 es = new ES257(c.IP);

                Assert.AreEqual(9001, (es.Ports[0].Connection as TCPConnection).Port, "Port 1 should default to connecting to tcp port 9001");
                Assert.AreEqual(9002, (es.Ports[1].Connection as TCPConnection).Port, "Port 2 should default to connecting to tcp port 9002");

                string helloWorld = "Hello World!!!";
                Random r = new Random();

                for (int i = 0; i < 10; i++ )
                {
                    es.Ports[0].Connect();
                    es.Ports[1].Connect();

                    string testMessage = new StringBuilder().Insert(0, helloWorld, r.Next(1, 500)).ToString();

                    es.Ports[0].Send(testMessage);
                    Assert.AreEqual(testMessage, es.Ports[1].Receive(), "Port 2 should recieve the message sent down port 1");
                }

                es.Ports[0].Disconnect();
                es.Ports[1].Disconnect();

            }
        }

        [TestMethod]
        public void Es257TxRxTimeoutTest()
        {
            foreach (TCPConnection c in cp.ES257Connections.Where(kvp => kvp.Value is TCPConnection).RandomTestOrder())
            {
                ES257 es = new ES257(c.IP);

                es.Ports[0].Connect();
                es.Ports[1].Connect();

                try
                {
                    es.Ports[0].Receive();
                    Assert.Fail("Should timeout after recieve function as no data is present");
                }
                catch(TimeoutException)
                {
                    Console.WriteLine("Pass");
                }

                es.Ports[0].Protocol = new DefaultSerialProtocol()
                {
                    TerminatingCharacters = "not the same as the other protocol"
                };

                try
                {
                    es.Ports[0].Send("hello");
                    es.Ports[1].Receive();
                    Assert.Fail("Should timeout as the terminating character is not transmitted");
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Pass");
                }

                es.Ports[0].Disconnect();
                es.Ports[1].Disconnect();

            }
        }

    }
}
