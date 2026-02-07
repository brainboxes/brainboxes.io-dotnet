using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Brainboxes.IO.Tests
{
    [TestClass]
    public class ConnectionTest : UnitTestBase
    {
        [TestMethod]
        public void ThreadSafeConnectionTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                Exception threadException = null;

                for(int i = 0; i < 1000; i++)
                {
                    Thread thread1 = new Thread(() => { try { c.Connect(); } catch (Exception ex) { threadException = ex; } });
                    Thread thread2 = new Thread(() => { try { c.Connect(); } catch (Exception ex) { threadException = ex; } });

                    thread1.Start();
                    thread2.Start();

                    thread1.Join();
                    thread2.Join();

                    if (threadException != null) throw threadException;

                    Assert.IsTrue(c.IsConnected, "Connection should be connected");

                    Thread thread3 = new Thread(() => { try { c.Disconnect(); } catch (Exception ex) { threadException = ex; } });
                    Thread thread4 = new Thread(() => { try { c.Disconnect(); } catch (Exception ex) { threadException = ex; } });

                    thread3.Start();
                    thread4.Start();

                    thread3.Join();
                    thread4.Join();

                    if (threadException != null) throw threadException;

                    Assert.IsFalse(c.IsConnected, "Connection should be disconnected");
                }
            }
        }


        [TestMethod]
        public void TimeoutTest()
        {
            EDDevice ed = new EDDevice();
            Stopwatch sw = new Stopwatch();
            //takes approx 5 seconds per connection
            //only applies to ASCII as its straight forward to tell a bad command with modbus
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsASCIIProtocol())
            {
                ed.Connection = c;

                Assert.AreEqual(2000, c.Timeout, "The connection timeout should default to 500 ms");
                ed.Connect();
                Assert.AreEqual(2000, c.Timeout, "The connection timeout should default to 500 ms");
                Assert.AreEqual(2000, c.Stream.ReadTimeout, "The stream read timeout should default to 500 ms");
                Assert.AreEqual(2000, c.Stream.WriteTimeout, "The stream write timeout should default to 500 ms");

                sw.Reset();
                sw.Start();
                try
                {
                    //bad command
                    ed.SendCommand("@junkcommand");
                } 
                catch(TimeoutException )
                {
                    sw.Stop();
                    Assert.AreEqual(2, Convert.ToInt32( sw.ElapsedMilliseconds / 1000), "command should timeout in 2 seconds");
                }


                ed.Disconnect();

                c.Timeout = 1000;
                Assert.AreEqual(1000, c.Timeout, "The connection timeout should now be 1000 ms");
                ed.Connect();

                Stream stream = c.Stream;

                Assert.AreEqual(1000, c.Timeout, "The connection timeout should now be 1000 ms");
                Assert.AreEqual(1000, stream.ReadTimeout, "The stream read timeout should now be 1000 ms");
                Assert.AreEqual(1000, stream.WriteTimeout, "The stream write timeout should now be 1000 ms");

                sw.Reset();
                sw.Start();
                try
                {
                    ed.SendCommand("@junkCommand");
                }
                catch (TimeoutException)
                {
                    sw.Stop();
                    Assert.AreEqual(1, Convert.ToInt32(sw.ElapsedMilliseconds / 1000), "command should timeout in 1 second");
                }


                //change timeout while connected
                c.Timeout = 250;
                Assert.AreEqual(250, c.Timeout, "The connection timeout should now be back at default of 500 ms");
                Assert.AreEqual(250, stream.ReadTimeout, "The stream read timeout should now be back at default of 500 ms");
                Assert.AreEqual(250, stream.WriteTimeout, "The stream write timeout should now be back at default of 500 ms");

                sw.Reset();
                sw.Start();
                try
                {
                    ed.SendCommand("@junkCommand");
                }
                catch (TimeoutException)
                {
                    sw.Stop();
                    Assert.AreEqual(25, Convert.ToInt32(sw.ElapsedMilliseconds / 10), "command should timeout in approx 250 milliseconds");
                }

                //change timeout while connected
                c.Timeout = 2000;
                Assert.AreEqual(2000, c.Timeout, "The connection timeout should now be back at default of 500 ms");
                Assert.AreEqual(2000, stream.ReadTimeout, "The stream read timeout should now be back at default of 500 ms");
                Assert.AreEqual(2000, stream.WriteTimeout, "The stream write timeout should now be back at default of 500 ms");

                sw.Reset();
                sw.Start();
                try
                {
                    ed.SendCommand("@junkCommand");
                }
                catch (TimeoutException)
                {
                    sw.Stop();
                    Assert.AreEqual(2, Convert.ToInt32(sw.ElapsedMilliseconds / 1000), "command should timeout in 2 seconds");
                }

                ed.Disconnect();

            }
        }

        [TestMethod]
        [ExpectedException(typeof(SystemException), AllowDerivedTypes = true)]
        /// <summary>
        /// Attempt to connect to an invalid serial connection: in windows
        /// .net10 throws FileNotFoundException
        /// .net48 throws IOException
        /// In linux/MacOS throws UnauthorizedAccessException
        /// /// </summary>
        public void InvalidConnectionSerialTest()
        {
            IConnection c = cp.InvalidSerialConnections.RandomTestOrder().FirstOrDefault();
            c.Connect();
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void InvalidConnectionTCPTest()
        {
            TCPConnection tcpConn = cp.InvalidTCPConnections.RandomTestOrder().FirstOrDefault() as TCPConnection;
            //speed up the test by shortening the length of the timeout
            tcpConn.ConnectionTimeout = 100; //100MS
            tcpConn.Connect();
        }

        /// <summary>
        /// note MacOS network stack cannot control timeouts above ~4 seconds
        /// therefore the test is limited to timeouts below this level
        /// </summary>
        [TestMethod]
        public void InvalidConnectionTCPTimeoutTest()
        {
            TCPConnection tcpConn = cp.InvalidTCPConnections.RandomTestOrder().FirstOrDefault() as TCPConnection;
            int[] timeouts = { 100, 200, 500, 1000, 1500, 2000};
            Stopwatch sw = new Stopwatch();

            foreach(int timeout in timeouts)
            {
                tcpConn.ConnectionTimeout = timeout;
                try
                {
                    Console.WriteLine("SETTING TIMEOUT TO " + timeout + "ms");
                    sw.Reset();
                    sw.Start();
                    tcpConn.Connect();
                }
                catch (SocketException)
                {
                    sw.Stop();
                    Console.WriteLine("TIMED OUT IN " + sw.ElapsedMilliseconds+ " ms");

                    Assert.AreEqual( timeout, sw.ElapsedMilliseconds, 200, "timeout should approximately equal "+timeout+ "ms, within 200ms");
                }
                finally
                {
                    if(sw.IsRunning)
                    {
                        Assert.Fail("tcpConnection should be invalid "+tcpConn.IP);
                    }
                }
            }
        }

        [TestMethod]
        public void TestConnectionStream()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsEachProtocol())
            {
                Assert.IsNull(c.Stream, "Connection stream should be null when not connected");

                c.Connect();
                Assert.IsTrue(c.Stream.CanRead, "Should be able to read from a connection stream once connected");
                Assert.IsTrue(c.Stream.CanWrite, "Should be able to write to a connection stream once connected");

                c.Disconnect();

                Assert.IsNull(c.Stream, "Connection stream should be null when not connected");
                Assert.IsFalse(c.IsConnected, "Connection should be closed at the end of the test");
            }
        }

        [TestMethod]
        public void IsConnectedTest()
        {
            foreach (IConnection c in cp.EDConnections.RandomTestOrder().AsEachProtocol())
            {
                c.Connect();
                Assert.IsTrue(c.IsConnected, "When the connection is connected IsConnected should return true");
                c.Disconnect();
                Assert.IsFalse(c.IsConnected, "When the connection is disconnected IsConnected should return true");
                c.Connect();
                Assert.IsTrue(c.IsConnected, "Should be able to reconnect to a previously disconnected connection");
                c.Disconnect();
                Assert.IsFalse(c.IsConnected, "Connection should be closed at the end of the test");
            }
        }
    }
}
