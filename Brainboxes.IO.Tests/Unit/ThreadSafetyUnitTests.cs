using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brainboxes.IO.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brainboxes.IO.Tests.Unit
{
    /// <summary>
    /// Mock-based thread safety tests that don't require hardware.
    /// These tests verify concurrent access patterns using MockConnection and MockProtocol.
    /// </summary>
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    [TestCategory(TestCategories.ThreadSafety)]
    public class ThreadSafetyUnitTests
    {
        private const int ConcurrentThreads = 20;
        private const int IterationsPerThread = 1000;

        #region MockConnection Concurrency Tests

        [TestMethod]
        public void MockConnection_ConcurrentConnectDisconnect_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();

            using (var connection = new MockConnection())
            {
                var tasks = new List<Task>();

                // Multiple threads connecting/disconnecting
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < IterationsPerThread; i++)
                        {
                            try
                            {
                                if (threadId % 2 == 0)
                                {
                                    connection.Connect();
                                }
                                else
                                {
                                    connection.Disconnect();
                                }
                            }
                            catch (ObjectDisposedException)
                            {
                                // Expected if connection disposed during test
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        [TestMethod]
        public void MockConnection_ConcurrentEventRegistration_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var handlers = new List<ConnectionStatusChangedEventHandler>();

            for (int i = 0; i < 50; i++)
            {
                handlers.Add((conn, prop, val) => { });
            }

            using (var connection = new MockConnection())
            {
                var tasks = new List<Task>();

                // Multiple threads adding/removing event handlers
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < IterationsPerThread; i++)
                        {
                            try
                            {
                                var handler = handlers[i % handlers.Count];
                                if (threadId % 2 == 0)
                                {
                                    connection.ConnectionStatusChangedEvent += handler;
                                }
                                else
                                {
                                    connection.ConnectionStatusChangedEvent -= handler;
                                }
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        [TestMethod]
        public void MockConnection_ConcurrentIsConnectedAccess_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var readCount = 0;

            using (var connection = new MockConnection())
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var tasks = new List<Task>();

                // Reader threads
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                bool _ = connection.IsConnected;
                                Interlocked.Increment(ref readCount);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                // Writer thread
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            connection.Connect();
                            Thread.Sleep(1);
                            connection.Disconnect();
                            Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));

                Task.WaitAll(tasks.ToArray());
            }

            // Assert
            Console.WriteLine($"Total reads: {readCount}");
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
            Assert.IsTrue(readCount > 0, "Should have performed reads");
        }

        #endregion

        #region MockProtocol Concurrency Tests

        [TestMethod]
        public void MockProtocol_ConcurrentStateAccess_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var protocol = new MockProtocol();

            var tasks = new List<Task>();

            // Multiple threads reading/writing digital state
            for (int t = 0; t < ConcurrentThreads; t++)
            {
                int threadId = t;
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < IterationsPerThread; i++)
                    {
                        try
                        {
                            if (threadId % 2 == 0)
                            {
                                // Read state
                                int _ = protocol.GetAllDigitalLineStates();
                            }
                            else
                            {
                                // Write state
                                protocol.SetDigitalOutputLineState(threadId % 8, i % 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        [TestMethod]
        public void MockProtocol_ConcurrentCommandHistory_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var protocol = new MockProtocol();

            var tasks = new List<Task>();

            // Multiple threads adding to command history and reading it
            for (int t = 0; t < ConcurrentThreads; t++)
            {
                int threadId = t;
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < IterationsPerThread; i++)
                    {
                        try
                        {
                            if (threadId % 3 == 0)
                            {
                                protocol.GetAllDigitalLineStates();
                            }
                            else if (threadId % 3 == 1)
                            {
                                protocol.SetDigitalOutputLineState(0, 1);
                            }
                            else
                            {
                                // Read command history
                                int _ = protocol.CommandHistory.Count;
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        [TestMethod]
        public void MockProtocol_ConcurrentAnalogAccess_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var protocol = new MockProtocol { NumberOfAnalogInputs = 8 };

            var tasks = new List<Task>();

            // Multiple threads reading/writing analog values
            for (int t = 0; t < ConcurrentThreads; t++)
            {
                int threadId = t;
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < IterationsPerThread; i++)
                    {
                        try
                        {
                            if (threadId % 2 == 0)
                            {
                                // Simulate input changes
                                protocol.SetAnalogInputState(threadId % 8, i * 0.1);
                            }
                            else
                            {
                                // Read analog values
                                double[] _ = protocol.GetAllAnalogInputLineStates(8);
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        #endregion

        #region MockStream Concurrency Tests

        [TestMethod]
        public void MockStream_ConcurrentReadWrite_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var stream = new MockStream();

            var tasks = new List<Task>();

            // Queue many responses
            for (int i = 0; i < 1000; i++)
            {
                stream.QueueResponse($">TEST{i}");
            }

            // Multiple threads reading
            for (int t = 0; t < ConcurrentThreads / 2; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < IterationsPerThread / 10; i++)
                    {
                        try
                        {
                            var buffer = new byte[128];
                            // Write triggers response
                            stream.Write(new byte[] { 0x40, 0x30, 0x31, 0x0D }, 0, 4);

                            if (stream.DataAvailable > 0)
                            {
                                stream.Read(buffer, 0, stream.DataAvailable);
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            // Thread checking DataAvailable
            for (int t = 0; t < ConcurrentThreads / 2; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < IterationsPerThread; i++)
                    {
                        try
                        {
                            int _ = stream.DataAvailable;
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        [TestMethod]
        public void MockStream_ConcurrentQueueAndRead_NoExceptions()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var stream = new MockStream();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var tasks = new List<Task>();

            // Producer - queues responses
            tasks.Add(Task.Run(() =>
            {
                int count = 0;
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        stream.QueueResponse($">RESP{count++}");
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                    Thread.SpinWait(100);
                }
            }));

            // Consumers - trigger and read responses
            for (int t = 0; t < ConcurrentThreads; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            // Trigger response with a write
                            stream.Write(new byte[] { 0x40, 0x30, 0x31, 0x0D }, 0, 4);

                            var buffer = new byte[128];
                            if (stream.DataAvailable > 0)
                            {
                                stream.Read(buffer, 0, Math.Min(buffer.Length, stream.DataAvailable));
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.AreEqual(0, exceptions.Count,
                $"Unexpected exceptions: {string.Join(", ", exceptions)}");
        }

        #endregion

        #region Combined Stress Tests

        [TestMethod]
        [Timeout(30000)]
        public void CombinedMockStressTest_AllOperations()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();
            var connection = new MockConnection();
            var protocol = new MockProtocol();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var tasks = new List<Task>();

            // Task 1: Toggle connection
            tasks.Add(Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        connection.Connect();
                        Thread.Sleep(10);
                        connection.Disconnect();
                        Thread.Sleep(10);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));

            // Task 2: Check connection state
            tasks.Add(Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        bool _ = connection.IsConnected;
                        bool __ = connection.IsAvailable;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));

            // Task 3: Protocol digital operations
            tasks.Add(Task.Run(() =>
            {
                int i = 0;
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        protocol.SetDigitalOutputLineState(i % 8, i % 2);
                        int _ = protocol.GetAllDigitalLineStates();
                        i++;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));

            // Task 4: Protocol analog operations
            tasks.Add(Task.Run(() =>
            {
                int i = 0;
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        protocol.SetAnalogInputState(i % 8, i * 0.1);
                        double[] _ = protocol.GetAllAnalogInputLineStates();
                        i++;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));

            // Task 5: Event registration
            tasks.Add(Task.Run(() =>
            {
                ConnectionStatusChangedEventHandler handler = (c, p, v) => { };
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        connection.ConnectionStatusChangedEvent += handler;
                        Thread.SpinWait(100);
                        connection.ConnectionStatusChangedEvent -= handler;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));

            Task.WaitAll(tasks.ToArray());
            connection.Dispose();

            // Assert
            Console.WriteLine($"Exceptions count: {exceptions.Count}");
            if (exceptions.Count > 0)
            {
                foreach (var ex in exceptions)
                {
                    Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
                }
            }
            Assert.AreEqual(0, exceptions.Count, "Should have no unexpected exceptions");
        }

        #endregion
    }
}
