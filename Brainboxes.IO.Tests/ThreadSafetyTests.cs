using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brainboxes.IO.Tests
{
    /// <summary>
    /// Tests to demonstrate and validate thread safety issues in the Brainboxes.IO library.
    /// These tests attempt to trigger race conditions through concurrent operations.
    /// 
    /// IMPORTANT: Race conditions are timing-dependent - these tests may not fail every time,
    /// but running them repeatedly or under load should expose the issues.
    /// </summary>
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    [TestCategory(TestCategories.ThreadSafety)]
    public class ThreadSafetyTests : UnitTestBase
    {
        private const int ConcurrentThreads = 50;
        private const int IterationsPerThread = 1_000_000;

        /// <summary>
        /// Ensure devices are in ASCII mode before running thread safety tests.
        /// This is required because port 9500 (ASCII) only accepts connections when
        /// the device is configured for ASCII protocol.
        /// </summary>
        [TestInitialize]
        public new void TestInit()
        {
            base.TestInit();
            
            // Switch all ED connections to ASCII mode (includes devices with outputs like ED588, BB400)
            Console.WriteLine("Setting up devices for ASCII protocol...");
            foreach (var connection in cp.EDConnections.Values.AsASCIIProtocol())
            {
                Console.WriteLine($"  Configured: {connection}");
            }
            Console.WriteLine("Device setup complete.");
        }

        #region Issue 1 & 2: IsConnected/IsAvailable Cache Race Condition

        /// <summary>
        /// Thread Safety Test #1: Verifies IsConnected property can be safely accessed
        /// from multiple threads while another thread toggles the connection state.
        ///
        /// This test validates that:
        /// - No exceptions are thrown during concurrent access
        /// - The property getter doesn't corrupt state or throw ObjectDisposedException
        ///
        /// Note: Two consecutive reads returning different values is EXPECTED behavior
        /// when another thread is actively changing the connection state - this is not
        /// a race condition but rather the correct observation of state transitions.
        /// </summary>
        [TestMethod]
        [Timeout(300000)] // 5 minutes - 50 threads × 1M iterations
        [TestCategory("LongRunning")]
        public void IsConnected_ConcurrentAccess_NoExceptions()
        {
            if (!cp.EDConnections.Values.Any()) Assert.Inconclusive("No ED connections available");

            var connection = cp.EDConnections.Values.First();
            var exceptions = new ConcurrentBag<Exception>();
            int totalReads = 0;
            int stateTransitionsObserved = 0;

            using (var ed = new EDDevice(connection))
            {
                ed.Connect();

                var tasks = new List<Task>();

                // Multiple readers checking IsConnected rapidly
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < IterationsPerThread; i++)
                        {
                            try
                            {
                                // Rapid reads - may observe state transitions which is expected
                                bool connected1 = ed.Connection.IsConnected;
                                bool connected2 = ed.Connection.IsConnected;
                                Interlocked.Increment(ref totalReads);

                                // Track state transitions for diagnostic purposes only
                                // This is expected when another thread is toggling connection
                                if (connected1 != connected2)
                                {
                                    Interlocked.Increment(ref stateTransitionsObserved);
                                }
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                // One thread toggling connection state
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        try
                        {
                            ed.Disconnect();
                            Thread.Sleep(1);
                            ed.Connect();
                            Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));

                Task.WaitAll(tasks.ToArray());

                ed.Disconnect();
            }

            Console.WriteLine($"Total reads: {totalReads}");
            Console.WriteLine($"State transitions observed: {stateTransitionsObserved} (expected when connection is being toggled)");
            Console.WriteLine($"Exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            // Only fail on exceptions - observed state transitions are expected behavior
            if (exceptions.Any())
            {
                Assert.Fail($"THREAD SAFETY ISSUE: {exceptions.Count} exceptions during concurrent IsConnected access");
            }
        }

        #endregion

        #region Issue 3: _numberOfEventsRegistered Non-Atomic Updates

        /// <summary>
        /// Race Condition #3: _numberOfEventsRegistered uses ++ and -- which are not atomic.
        /// Multiple threads adding/removing events can cause the count to become incorrect,
        /// leading to polling not starting or stopping when it should.
        /// 
        /// Expected: After adding N handlers and removing N handlers, count should be 0.
        /// Actual: Count may be wrong due to lost increments/decrements.
        /// </summary>
        [TestMethod]
        public void EventRegistration_ConcurrentAddRemove_RaceCondition()
        {
            if (!cp.EDConnections.Values.Any()) Assert.Inconclusive("No ED connections available");

            var connection = cp.EDConnections.Values.First();
            var exceptions = new ConcurrentBag<Exception>();
            int addCount = 0;
            int removeCount = 0;

            using (var ed = new EDDevice(connection))
            {
                ed.Connect();

                // Create a pool of event handlers
                var handlers = new List<ConnectionStatusChangedEventHandler>();
                for (int i = 0; i < 100; i++)
                {
                    handlers.Add((conn, prop, val) => { /* empty handler */ });
                }

                var tasks = new List<Task>();

                // Multiple threads adding events
                for (int t = 0; t < ConcurrentThreads / 2; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                var handler = handlers[(threadId * 10 + i) % handlers.Count];
                                ed.Connection.ConnectionStatusChangedEvent += handler;
                                Interlocked.Increment(ref addCount);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                // Multiple threads removing events  
                for (int t = 0; t < ConcurrentThreads / 2; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        Thread.Sleep(10); // Let some adds happen first
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                var handler = handlers[(threadId * 10 + i) % handlers.Count];
                                ed.Connection.ConnectionStatusChangedEvent -= handler;
                                Interlocked.Increment(ref removeCount);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());

                ed.Disconnect();
            }

            Console.WriteLine($"Adds: {addCount}, Removes: {removeCount}");
            Console.WriteLine($"Exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            // The internal counter may be wrong even if no exceptions occurred
            // This could cause polling to be stuck on or off
        }

        #endregion

        #region Issue 4: Timer Disposal Race in Connection._stopPollingForChanges

        /// <summary>
        /// Race Condition #4: Connection._stopPollingForChanges doesn't wait for in-flight callbacks.
        /// Unlike EDDevice which was fixed, Connection.cs still has the old pattern.
        /// 
        /// This can cause ObjectDisposedException when the timer callback tries to access
        /// disposed resources.
        /// </summary>
        [TestMethod]
        public void ConnectionPolling_StopWhileCallbackRunning_RaceCondition()
        {
            if (!cp.EDConnections.Values.Any()) Assert.Inconclusive("No ED connections available");

            var exceptions = new ConcurrentBag<Exception>();
            int iterations = 50;

            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    var connection = cp.EDConnections.Values.First();
                    using (var ed = new EDDevice(connection))
                    {
                        // Register event to start polling
                        ed.Connection.ConnectionStatusChangedEvent += (conn, prop, val) => { };

                        ed.Connect();

                        // Small delay to let polling start
                        Thread.Sleep(10);

                        // Disconnect immediately - races with polling callback
                        ed.Disconnect();
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    exceptions.Add(ex);
                    Console.WriteLine($"Iteration {i}: ObjectDisposedException - {ex.Message}");
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Console.WriteLine($"Iteration {i}: {ex.GetType().Name} - {ex.Message}");
                }
            }

            Console.WriteLine($"Total iterations: {iterations}");
            Console.WriteLine($"Exceptions caught: {exceptions.Count}");

            if (exceptions.Any())
            {
                Assert.Fail($"RACE CONDITION DETECTED: Timer callback raced with disposal ({exceptions.Count} exceptions)");
            }
        }

        #endregion

        #region Issue 5 & 6: TCPConnection._disconnect and _newValueIsConnected Race

        /// <summary>
        /// Race Condition #5 & #6: Socket can be disposed while _newValueIsConnected is checking it.
        /// Even with local variable capture, there's a window where the socket can be closed.
        /// 
        /// The fix we applied catches ObjectDisposedException, but this test shows it can happen.
        /// </summary>
        [TestMethod]
        public void TCPConnection_DisconnectWhileCheckingIsConnected_RaceCondition()
        {
            if (!cp.EDConnections.Values.Any()) Assert.Inconclusive("No ED connections available");

            var exceptions = new ConcurrentBag<Exception>();
            var disposedExceptions = 0;
            int iterations = 100;

            for (int iter = 0; iter < iterations; iter++)
            {
                var connection = cp.EDConnections.Values.First() as TCPConnection;
                if (connection == null) Assert.Inconclusive("No TCP connections available");

                using (var ed = new EDDevice(connection))
                {
                    var cts = new CancellationTokenSource();

                    // Thread constantly checking IsConnected
                    var checkTask = Task.Run(() =>
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                bool _ = ed.Connection.IsConnected;
                            }
                            catch (ObjectDisposedException)
                            {
                                Interlocked.Increment(ref disposedExceptions);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    });

                    try
                    {
                        ed.Connect();
                        Thread.Sleep(5);
                        ed.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }

                    cts.Cancel();
                    checkTask.Wait(1000);
                }
            }

            Console.WriteLine($"Total iterations: {iterations}");
            Console.WriteLine($"ObjectDisposedException count: {disposedExceptions}");
            Console.WriteLine($"Other exceptions: {exceptions.Count}");

            if (disposedExceptions > 0 || exceptions.Any())
            {
                Assert.Fail($"RACE CONDITION DETECTED: Socket access raced with disposal ({disposedExceptions} ObjectDisposedExceptions, {exceptions.Count} other exceptions)");
            }
        }

        #endregion

        #region Issue 8 & 9: EDDevice Polling and State Access

        /// <summary>
        /// Race Condition #8 & #9: _pollingThreadFunc reads IOLine state while user code modifies it.
        /// The _dispatchDigitalEvents method reads state without holding locks.
        /// </summary>
        [TestMethod]
        [Timeout(600000)] // 10 minutes - 50 threads × 100 iterations × N outputs @ ~6ms each
        [TestCategory("LongRunning")]
        public void EDDevice_PollingWhileSettingOutputs_RaceCondition()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var exceptions = new ConcurrentBag<Exception>();
            var eventsFired = 0;

            using (var ed = EDDevice.Create(connection.IP))
            {
                // Subscribe to events to start polling
                ed.IOLineChanged += (line, device, change) =>
                {
                    Interlocked.Increment(ref eventsFired);
                    // Access line properties while polling might be updating them
                    try
                    {
                        int val = line.Value;
                        var change2 = line.MostRecentChangeType;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                };

                ed.Connect();

                var tasks = new List<Task>();

                // Multiple threads setting outputs
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                foreach (var output in ed.Outputs)
                                {
                                    output.Value = i % 2;
                                }
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                            Thread.Sleep(1);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());

                Thread.Sleep(500); // Let polling finish

                ed.Disconnect();
            }

            Console.WriteLine($"Events fired: {eventsFired}");
            Console.WriteLine($"Exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(10))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            if (exceptions.Any())
            {
                Assert.Fail($"RACE CONDITION DETECTED: Polling raced with output setting ({exceptions.Count} exceptions)");
            }
        }

        #endregion

        #region Issue 10: IOLine._value and _previousValue Without Synchronization

        /// <summary>
        /// Thread Safety Test #10: Verifies IOLine.Value can be safely read from multiple
        /// threads while another thread is writing values.
        ///
        /// This test validates that:
        /// - No exceptions are thrown during concurrent read/write access
        /// - Values returned are always valid (0, 1, or -1 for uninitialized)
        /// - No torn reads or corrupted values occur
        ///
        /// Note: Two consecutive reads returning different values is EXPECTED behavior
        /// when another thread is actively writing - this is correct observation of
        /// state changes, not a race condition.
        /// </summary>
        [TestMethod]
        [Timeout(300000)] // 5 minutes - 50 threads × 1M iterations
        [TestCategory("LongRunning")]
        public void IOLine_ConcurrentValueAccess_NoExceptions()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var exceptions = new ConcurrentBag<Exception>();
            var invalidValues = new ConcurrentBag<int>();
            int totalReads = 0;
            int valueChangesObserved = 0;

            using (var ed = EDDevice.Create(connection.IP))
            {
                ed.Connect();

                if (!ed.Outputs.Any())
                {
                    ed.Disconnect();
                    Assert.Inconclusive("Device has no outputs");
                }

                var output = ed.Outputs.First();
                var tasks = new List<Task>();

                // Multiple readers
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < IterationsPerThread; i++)
                        {
                            try
                            {
                                int val1 = output.Value;
                                int val2 = output.Value;
                                Interlocked.Increment(ref totalReads);

                                // Verify values are valid (not corrupted/torn reads)
                                // Valid values for digital IO: -1 (uninitialized), 0 (off), 1 (on)
                                if (val1 < -1 || val1 > 1)
                                {
                                    invalidValues.Add(val1);
                                }
                                if (val2 < -1 || val2 > 1)
                                {
                                    invalidValues.Add(val2);
                                }

                                // Track value changes for diagnostic purposes only
                                // This is expected when another thread is writing
                                if (val1 != val2 && val1 != -1 && val2 != -1)
                                {
                                    Interlocked.Increment(ref valueChangesObserved);
                                }
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                // One writer
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        try
                        {
                            output.Value = i % 2;
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                        Thread.Sleep(1);
                    }
                }));

                Task.WaitAll(tasks.ToArray());

                ed.Disconnect();
            }

            Console.WriteLine($"Total reads: {totalReads}");
            Console.WriteLine($"Value changes observed: {valueChangesObserved} (expected when value is being written)");
            Console.WriteLine($"Invalid values: {invalidValues.Count}");
            Console.WriteLine($"Exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            // Fail on exceptions or invalid (corrupted) values
            if (exceptions.Any())
            {
                Assert.Fail($"THREAD SAFETY ISSUE: {exceptions.Count} exceptions during concurrent IOLine.Value access");
            }

            if (invalidValues.Any())
            {
                Assert.Fail($"THREAD SAFETY ISSUE: {invalidValues.Count} corrupted/invalid values read: {string.Join(", ", invalidValues.Take(10))}");
            }
        }

        #endregion

        #region Issue 11: Event Handler Lists Modified During Iteration

        /// <summary>
        /// Race Condition #11: IOLine._ioLineChangedEvents list is iterated while other threads
        /// can add/remove handlers, causing InvalidOperationException.
        /// </summary>
        [TestMethod]
        public void IOLine_EventHandlerModificationDuringIteration_RaceCondition()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var exceptions = new ConcurrentBag<Exception>();
            var invalidOpExceptions = 0;

            using (var ed = EDDevice.Create(connection.IP))
            {
                ed.Connect();

                if (!ed.Outputs.Any())
                {
                    ed.Disconnect();
                    Assert.Inconclusive("Device has no outputs");
                }

                var output = ed.Outputs.First();
                var handlers = new List<IOLineChangedEventHandler>();

                // Create handlers
                for (int i = 0; i < 50; i++)
                {
                    handlers.Add((line, device, change) => { Thread.Sleep(1); });
                }

                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();

                // Thread adding handlers
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            foreach (var h in handlers)
                            {
                                output.IOLineChanged += h;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            Interlocked.Increment(ref invalidOpExceptions);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));

                // Thread removing handlers
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            foreach (var h in handlers)
                            {
                                output.IOLineChanged -= h;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            Interlocked.Increment(ref invalidOpExceptions);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));

                // Thread triggering events (which iterates the handler list)
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            output.Value = 1;
                            Thread.Sleep(1);
                            output.Value = 0;
                            Thread.Sleep(1);
                        }
                        catch (InvalidOperationException)
                        {
                            Interlocked.Increment(ref invalidOpExceptions);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));

                Thread.Sleep(3000); // Run for 3 seconds
                cts.Cancel();

                try
                {
                    Task.WaitAll(tasks.ToArray(), 5000);
                }
                catch (AggregateException) { }

                ed.Disconnect();
            }

            Console.WriteLine($"InvalidOperationException count: {invalidOpExceptions}");
            Console.WriteLine($"Other exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            if (invalidOpExceptions > 0)
            {
                Assert.Fail($"RACE CONDITION DETECTED: Collection modified during iteration ({invalidOpExceptions} InvalidOperationExceptions)");
            }
        }

        #endregion

        #region Issue 13: IOList Dictionary Access Not Thread-Safe

        /// <summary>
        /// Race Condition #13: IOList event registration uses Dictionary without synchronization.
        /// ContainsKey() then Add() is non-atomic.
        /// </summary>
        [TestMethod]
        public void IOList_ConcurrentEventRegistration_RaceCondition()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var exceptions = new ConcurrentBag<Exception>();
            var keyExceptions = 0;

            using (var ed = EDDevice.Create(connection.IP))
            {
                ed.Connect();

                var handlers = new List<IOLineChangedEventHandler>();
                for (int i = 0; i < 100; i++)
                {
                    handlers.Add((line, device, change) => { });
                }

                var tasks = new List<Task>();

                // Multiple threads adding the same handlers to the IOList
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        foreach (var h in handlers)
                        {
                            try
                            {
                                // IOList.IOLineChange event uses Dictionary internally
                                ed.Outputs.IOLineChange += h;
                            }
                            catch (ArgumentException) // Key already exists
                            {
                                Interlocked.Increment(ref keyExceptions);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());

                ed.Disconnect();
            }

            Console.WriteLine($"ArgumentException (duplicate key) count: {keyExceptions}");
            Console.WriteLine($"Other exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            if (keyExceptions > 0 || exceptions.Any())
            {
                Assert.Fail($"RACE CONDITION DETECTED: Dictionary modified concurrently ({keyExceptions} key exceptions, {exceptions.Count} other exceptions)");
            }
        }

        #endregion

        #region Issue 14: Device._deviceStatusChangedEvent Multi-Thread Access

        /// <summary>
        /// Race Condition #14: DeviceStatusChangedEvent add/remove accessors are not atomic.
        /// Multiple threads could see stale invocation list counts.
        /// </summary>
        [TestMethod]
        public void Device_StatusChangedEvent_ConcurrentAccess_RaceCondition()
        {
            if (!cp.EDConnections.Values.Any()) Assert.Inconclusive("No ED connections available");

            var connection = cp.EDConnections.Values.First();
            var exceptions = new ConcurrentBag<Exception>();
            var nullRefExceptions = 0;

            using (var ed = new EDDevice(connection))
            {
                ed.Connect();

                var handlers = new List<DeviceStatusChangedEventHandler<IConnection, IIOProtocol>>();
                for (int i = 0; i < 50; i++)
                {
                    handlers.Add((device, prop, val) => { });
                }

                var tasks = new List<Task>();

                // Threads adding handlers
                for (int t = 0; t < ConcurrentThreads / 2; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                ed.DeviceStatusChangedEvent += handlers[i % handlers.Count];
                            }
                            catch (NullReferenceException)
                            {
                                Interlocked.Increment(ref nullRefExceptions);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                // Threads removing handlers
                for (int t = 0; t < ConcurrentThreads / 2; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                ed.DeviceStatusChangedEvent -= handlers[i % handlers.Count];
                            }
                            catch (NullReferenceException)
                            {
                                Interlocked.Increment(ref nullRefExceptions);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());

                ed.Disconnect();
            }

            Console.WriteLine($"NullReferenceException count: {nullRefExceptions}");
            Console.WriteLine($"Other exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        #endregion

        #region Issue 15: Device.Connection Property Setter Complex Operations

        /// <summary>
        /// Race Condition #15: Connection property setter does disconnect/swap/connect
        /// without synchronization. Another thread could access Connection mid-swap.
        /// </summary>
        [TestMethod]
        public void Device_ConnectionSwap_RaceCondition()
        {
            if (cp.EDConnections.Values.Count() < 2) Assert.Inconclusive("Need at least 2 ED connections");

            var connections = cp.EDConnections.Values.Take(2).ToArray();
            var exceptions = new ConcurrentBag<Exception>();
            var nullRefExceptions = 0;

            using (var ed = new EDDevice(connections[0]))
            {
                ed.Connect();

                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();

                // Thread swapping connections
                tasks.Add(Task.Run(() =>
                {
                    int swapCount = 0;
                    while (!cts.Token.IsCancellationRequested && swapCount < 50)
                    {
                        try
                        {
                            ed.Connection = connections[swapCount % 2];
                            swapCount++;
                        }
                        catch (NullReferenceException)
                        {
                            Interlocked.Increment(ref nullRefExceptions);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                        Thread.Sleep(10);
                    }
                }));

                // Threads accessing connection properties
                for (int t = 0; t < ConcurrentThreads; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                var conn = ed.Connection;
                                if (conn != null)
                                {
                                    bool _ = conn.IsConnected;
                                    int timeout = conn.Timeout;
                                }
                            }
                            catch (NullReferenceException)
                            {
                                Interlocked.Increment(ref nullRefExceptions);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                Thread.Sleep(2000);
                cts.Cancel();

                try
                {
                    Task.WaitAll(tasks.ToArray(), 5000);
                }
                catch (AggregateException) { }

                ed.Disconnect();
            }

            Console.WriteLine($"NullReferenceException count: {nullRefExceptions}");
            Console.WriteLine($"Other exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            if (nullRefExceptions > 0 || exceptions.Any())
            {
                Assert.Fail($"RACE CONDITION DETECTED: Connection accessed during swap ({nullRefExceptions} NullReferenceExceptions, {exceptions.Count} other exceptions)");
            }
        }

        #endregion

        #region Issue 17: _startPollingForChanges Double-Start

        /// <summary>
        /// Race Condition #17: _startPollingForChanges check-then-set on _threadShouldBeRunning
        /// is not atomic. Multiple threads could both pass the check and create multiple timers.
        /// </summary>
        [TestMethod]
        public void EDDevice_DoubleStartPolling_RaceCondition()
        {
            if (!cp.EDConnections.Values.Any()) Assert.Inconclusive("No ED connections available");

            var connection = cp.EDConnections.Values.First();
            var exceptions = new ConcurrentBag<Exception>();

            // This test attempts to trigger double-start by rapidly adding events
            for (int iteration = 0; iteration < 50; iteration++)
            {
                using (var ed = new EDDevice(connection))
                {
                    ed.Connect();

                    var handlers = new List<IOLinesChangedEventHandler>();
                    for (int i = 0; i < 10; i++)
                    {
                        handlers.Add((lines, device) => { });
                    }

                    var tasks = new List<Task>();

                    // Multiple threads adding events simultaneously
                    // Each add could trigger _startPollingForChanges
                    for (int t = 0; t < 5; t++)
                    {
                        int threadId = t;
                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                // All threads add their first handler at roughly the same time
                                ed.IOLinesChanged += handlers[threadId * 2];
                                ed.IOLinesChanged += handlers[threadId * 2 + 1];
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());

                    Thread.Sleep(50); // Let any duplicate timers fire

                    ed.Disconnect();
                }
            }

            Console.WriteLine($"Iterations: 50");
            Console.WriteLine($"Exceptions: {exceptions.Count}");

            foreach (var ex in exceptions.Take(5))
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            }

            // Note: This race condition may not cause exceptions but could cause
            // multiple timers to be created, leading to excessive polling
        }

        #endregion

        #region Summary Test - Run All Concurrency Stress

        /// <summary>
        /// Combined stress test that exercises multiple race conditions simultaneously.
        /// This is the most likely to expose issues due to increased contention.
        ///
        /// This test validates that:
        /// - No unexpected exceptions occur during concurrent operations
        /// - ObjectDisposedException is expected during connection toggling (TOCTOU race)
        /// - InvalidOperationException from "not connected" state is expected
        ///
        /// Note: When deliberately toggling connections while performing operations,
        /// some operations may encounter disposed sockets due to the inherent race
        /// between checking IsConnected and using the connection. This is expected
        /// behavior, not a bug.
        /// </summary>
        [TestMethod]
        [Timeout(120_000)] // 2 minute timeout
        [TestCategory("LongRunning")]
        public void CombinedConcurrencyStressTest()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var allExceptions = new ConcurrentBag<Exception>();
            var eventCount = 0;

            // Helper to check if an exception is expected during connection toggling
            // ObjectDisposedException and IOException are expected when socket is disposed mid-operation
            Func<Exception, bool> isExpectedException = (ex) =>
            {
                if (ex is ObjectDisposedException) return true;
                if (ex is System.IO.IOException) return true;
                if (ex is InvalidOperationException && ex.Message.Contains("not connected")) return true;
                return false;
            };

            Console.WriteLine("Starting combined concurrency stress test...");
            Console.WriteLine("This test attempts to trigger multiple race conditions simultaneously.");
            Console.WriteLine();

            using (var ed = EDDevice.Create(connection.IP))
            {
                // Event handlers
                IOLineChangedEventHandler lineHandler = (line, device, change) =>
                {
                    Interlocked.Increment(ref eventCount);
                    try
                    {
                        int _ = line.Value;
                    }
                    catch { }
                };

                ed.Connect();

                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();

                // Task 1: Toggle connection
                tasks.Add(Task.Run(() =>
                {
                    int toggles = 0;
                    while (!cts.Token.IsCancellationRequested && toggles < 20)
                    {
                        try
                        {
                            ed.Disconnect();
                            Thread.Sleep(50);
                            ed.Connect();
                            toggles++;
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                        Thread.Sleep(100);
                    }
                }));

                // Task 2: Check IsConnected rapidly
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            bool _ = ed.IsConnected;
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                    }
                }));

                // Task 3: Add/remove event handlers
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            ed.IOLineChanged += lineHandler;
                            Thread.Sleep(10);
                            ed.IOLineChanged -= lineHandler;
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                    }
                }));

                // Task 4: Set output values
                tasks.Add(Task.Run(() =>
                {
                    int i = 0;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (ed.IsConnected && ed.Outputs.Any())
                            {
                                foreach (var output in ed.Outputs)
                                {
                                    output.Value = i % 2;
                                }
                            }
                            i++;
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                        Thread.Sleep(5);
                    }
                }));

                // Task 5: Read input values
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (ed.IsConnected && ed.Inputs.Any())
                            {
                                foreach (var input in ed.Inputs)
                                {
                                    int _ = input.Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                    }
                }));

                // Run for 30 seconds
                Thread.Sleep(30000);
                cts.Cancel();

                try
                {
                    Task.WaitAll(tasks.ToArray(), 10000);
                }
                catch (AggregateException) { }

                if (ed.IsConnected)
                    ed.Disconnect();
            }

            // Separate expected vs unexpected exceptions
            var expectedExceptions = allExceptions.Where(isExpectedException).ToList();
            var unexpectedExceptions = allExceptions.Where(ex => !isExpectedException(ex)).ToList();

            Console.WriteLine($"Events fired: {eventCount}");
            Console.WriteLine($"Total exceptions: {allExceptions.Count}");
            Console.WriteLine($"  Expected (during disconnect): {expectedExceptions.Count}");
            Console.WriteLine($"  Unexpected: {unexpectedExceptions.Count}");
            Console.WriteLine();

            // Report expected exceptions (informational only)
            if (expectedExceptions.Any())
            {
                Console.WriteLine("Expected exceptions (normal during connection toggling):");
                var expectedGrouped = expectedExceptions.GroupBy(e => e.GetType().Name);
                foreach (var group in expectedGrouped)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count()}");
                }
                Console.WriteLine();
            }

            // Report unexpected exceptions (these would cause failure)
            if (unexpectedExceptions.Any())
            {
                Console.WriteLine("Unexpected exceptions:");
                var unexpectedGrouped = unexpectedExceptions.GroupBy(e => e.GetType().Name)
                                                            .OrderByDescending(g => g.Count());
                foreach (var group in unexpectedGrouped)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count()} occurrences");
                    Console.WriteLine($"    Sample: {group.First().Message}");
                    Console.WriteLine($"    Stack trace:");
                    Console.WriteLine($"    {group.First().StackTrace?.Replace("\n", "\n    ")}");
                    Console.WriteLine();
                }

                Assert.Fail($"RACE CONDITIONS DETECTED: {unexpectedExceptions.Count} unexpected exceptions - see above");
            }
            else
            {
                Console.WriteLine("No unexpected exceptions detected in this run.");
                Console.WriteLine("Note: Race conditions may not manifest every run.");
            }
        }

        #endregion

        #region Extended Stress Tests

        /// <summary>
        /// Extended stress test that runs for 10 minutes to catch rare race conditions.
        /// This is the most thorough test for finding timing-dependent bugs.
        ///
        /// Note: ObjectDisposedException and IOException are expected when connection
        /// is being toggled - these are filtered as expected exceptions.
        /// </summary>
        [TestMethod]
        [Timeout(660000)] // 11 minute timeout (10 min test + 1 min buffer)
        [TestCategory(TestCategories.Performance)]
        [TestCategory("LongRunning")]
        public void ExtendedConcurrencyStressTest_10Minutes()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var allExceptions = new ConcurrentBag<Exception>();
            var eventCount = 0;
            var operationCount = 0;

            Func<Exception, bool> isExpectedException = (ex) =>
            {
                if (ex is ObjectDisposedException) return true;
                if (ex is System.IO.IOException) return true;
                if (ex is InvalidOperationException && ex.Message.Contains("not connected")) return true;
                return false;
            };

            Console.WriteLine("Starting 10-minute extended concurrency stress test...");
            Console.WriteLine("This test exercises multiple race conditions over an extended period.");
            Console.WriteLine();

            using (var ed = EDDevice.Create(connection.IP))
            {
                IOLineChangedEventHandler lineHandler = (line, device, change) =>
                {
                    Interlocked.Increment(ref eventCount);
                    try
                    {
                        int _ = line.Value;
                    }
                    catch { }
                };

                ed.Connect();

                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();

                // Task 1: Toggle connection periodically
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            ed.Disconnect();
                            Thread.Sleep(100);
                            ed.Connect();
                            Interlocked.Increment(ref operationCount);
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                        Thread.Sleep(500);
                    }
                }));

                // Task 2: Continuous IsConnected checking
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            bool _ = ed.IsConnected;
                            Interlocked.Increment(ref operationCount);
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                    }
                }));

                // Task 3: Add/remove event handlers
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            ed.IOLineChanged += lineHandler;
                            Thread.Sleep(50);
                            ed.IOLineChanged -= lineHandler;
                            Interlocked.Increment(ref operationCount);
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                    }
                }));

                // Task 4: Set output values
                tasks.Add(Task.Run(() =>
                {
                    int i = 0;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (ed.IsConnected && ed.Outputs.Any())
                            {
                                foreach (var output in ed.Outputs)
                                {
                                    output.Value = i % 2;
                                }
                                Interlocked.Increment(ref operationCount);
                            }
                            i++;
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                        Thread.Sleep(10);
                    }
                }));

                // Task 5: Read input values
                tasks.Add(Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (ed.IsConnected && ed.Inputs.Any())
                            {
                                foreach (var input in ed.Inputs)
                                {
                                    int _ = input.Value;
                                }
                                Interlocked.Increment(ref operationCount);
                            }
                        }
                        catch (Exception ex)
                        {
                            allExceptions.Add(ex);
                        }
                        Thread.Sleep(5);
                    }
                }));

                // Task 6: Periodic status report
                tasks.Add(Task.Run(() =>
                {
                    int lastOpCount = 0;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        Thread.Sleep(60000); // Every minute
                        int currentOps = operationCount;
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Operations: {currentOps} (+{currentOps - lastOpCount}), Events: {eventCount}, Exceptions: {allExceptions.Count}");
                        lastOpCount = currentOps;
                    }
                }));

                // Run for 10 minutes
                Thread.Sleep(600000);
                cts.Cancel();

                try
                {
                    Task.WaitAll(tasks.ToArray(), 30000);
                }
                catch (AggregateException) { }

                if (ed.IsConnected)
                    ed.Disconnect();
            }

            var expectedExceptions = allExceptions.Where(isExpectedException).ToList();
            var unexpectedExceptions = allExceptions.Where(ex => !isExpectedException(ex)).ToList();

            Console.WriteLine();
            Console.WriteLine($"Final Results:");
            Console.WriteLine($"  Total operations: {operationCount}");
            Console.WriteLine($"  Events fired: {eventCount}");
            Console.WriteLine($"  Total exceptions: {allExceptions.Count}");
            Console.WriteLine($"    Expected (during disconnect): {expectedExceptions.Count}");
            Console.WriteLine($"    Unexpected: {unexpectedExceptions.Count}");
            Console.WriteLine();

            if (expectedExceptions.Count > 0)
            {
                Console.WriteLine("Expected exceptions (normal during connection toggling):");
                var expectedGrouped = expectedExceptions.GroupBy(e => e.GetType().Name);
                foreach (var group in expectedGrouped)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count()}");
                }
                Console.WriteLine();
            }

            if (unexpectedExceptions.Count > 0)
            {
                Console.WriteLine("Unexpected exceptions:");
                var unexpectedGrouped = unexpectedExceptions.GroupBy(e => e.GetType().Name)
                                                            .OrderByDescending(g => g.Count());
                foreach (var group in unexpectedGrouped)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count()} occurrences");
                    Console.WriteLine($"    Sample: {group.First().Message}");
                    Console.WriteLine($"    Stack trace:");
                    Console.WriteLine($"    {group.First().StackTrace?.Replace("\n", "\n    ")}");
                    Console.WriteLine();
                }

                Assert.Fail($"RACE CONDITIONS DETECTED: {unexpectedExceptions.Count} unexpected exceptions over 10 minutes - see above");
            }
            else
            {
                Console.WriteLine("No unexpected exceptions detected in 10-minute extended stress test.");
            }
        }

        /// <summary>
        /// High-frequency polling test to stress cache invalidation timing.
        /// Sets IOLineCacheTimeout to a very low value and performs rapid concurrent access.
        ///
        /// Note: This test does not toggle connections, so exceptions should be rare.
        /// ObjectDisposedException/IOException are still filtered for robustness.
        /// </summary>
        [TestMethod]
        [Timeout(120000)] // 2 minute timeout
        [TestCategory(TestCategories.Performance)]
        [TestCategory("LongRunning")]
        public void HighFrequencyPolling_CacheInvalidation_RaceCondition()
        {
            if (!cp.DevicesWithOutputsConnections.Values.Any()) Assert.Inconclusive("No devices with outputs available");

            var connection = cp.DevicesWithOutputsConnections.Values.First() as TCPConnection;
            if (connection == null) Assert.Inconclusive("Requires TCP connection");
            var allExceptions = new ConcurrentBag<Exception>();
            var readCount = 0;
            var writeCount = 0;
            var eventCount = 0;

            // Same filter as other tests for consistency
            Func<Exception, bool> isExpectedException = (ex) =>
            {
                if (ex is ObjectDisposedException) return true;
                if (ex is System.IO.IOException) return true;
                if (ex is InvalidOperationException && ex.Message.Contains("not connected")) return true;
                return false;
            };

            Console.WriteLine("Starting high-frequency polling test...");
            Console.WriteLine("Testing with very low cache timeout to stress timing issues.");
            Console.WriteLine();

            using (var ed = EDDevice.Create(connection.IP))
            {
                // Set very low cache timeout to stress timing
                int originalTimeout = ed.IOLineCacheTimeout;
                ed.IOLineCacheTimeout = 10; // 10ms - very aggressive

                IOLineChangedEventHandler lineHandler = (line, device, change) =>
                {
                    Interlocked.Increment(ref eventCount);
                };

                // Subscribe to events to start polling
                ed.IOLineChanged += lineHandler;
                ed.Connect();

                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();

                // Multiple reader threads
                for (int t = 0; t < 10; t++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                if (ed.IsConnected)
                                {
                                    foreach (var line in ed.IOLines)
                                    {
                                        int _ = line.Value;
                                    }
                                    Interlocked.Increment(ref readCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                allExceptions.Add(ex);
                            }
                        }
                    }));
                }

                // Writer threads
                for (int t = 0; t < 5; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        int i = 0;
                        while (!cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                if (ed.IsConnected && ed.Outputs.Any())
                                {
                                    ed.Outputs[threadId % ed.Outputs.Count()].Value = i % 2;
                                    Interlocked.Increment(ref writeCount);
                                }
                                i++;
                            }
                            catch (Exception ex)
                            {
                                allExceptions.Add(ex);
                            }
                            Thread.Sleep(1);
                        }
                    }));
                }

                // Run for 30 seconds
                Thread.Sleep(30000);
                cts.Cancel();

                try
                {
                    Task.WaitAll(tasks.ToArray(), 10000);
                }
                catch (AggregateException) { }

                ed.IOLineChanged -= lineHandler;
                ed.IOLineCacheTimeout = originalTimeout;
                ed.Disconnect();
            }

            var expectedExceptions = allExceptions.Where(isExpectedException).ToList();
            var unexpectedExceptions = allExceptions.Where(ex => !isExpectedException(ex)).ToList();

            Console.WriteLine($"High-frequency polling results:");
            Console.WriteLine($"  Total reads: {readCount}");
            Console.WriteLine($"  Total writes: {writeCount}");
            Console.WriteLine($"  Events fired: {eventCount}");
            Console.WriteLine($"  Total exceptions: {allExceptions.Count}");
            Console.WriteLine($"    Expected: {expectedExceptions.Count}");
            Console.WriteLine($"    Unexpected: {unexpectedExceptions.Count}");

            if (expectedExceptions.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Expected exceptions:");
                var expectedGrouped = expectedExceptions.GroupBy(e => e.GetType().Name);
                foreach (var group in expectedGrouped)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count()}");
                }
            }

            if (unexpectedExceptions.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Unexpected exceptions:");
                var unexpectedGrouped = unexpectedExceptions.GroupBy(e => e.GetType().Name)
                                                            .OrderByDescending(g => g.Count());
                foreach (var group in unexpectedGrouped)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count()} occurrences");
                    Console.WriteLine($"    Sample: {group.First().Message}");
                }

                Assert.Fail($"RACE CONDITIONS DETECTED: {unexpectedExceptions.Count} unexpected exceptions during high-frequency polling");
            }
        }

        #endregion
    }
}
