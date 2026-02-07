/*
This file is part of the C# example/library code for communication with 
Brainboxes Ethernet-attached data acquisition and control products, and is 
provided by Brainboxes Limited.  Examples in other programming languages are 
also available.
Visit http://www.brainboxes.com to see our range of Brainboxes Ethernet-
attached data acquisition and control products, and to check for updates to
this code package.

This is free and unencumbered software released into the public domain.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Brainboxes.IO
{
    /// <summary>
    /// Abstract base class for connections to Brainboxes devices.
    /// Provides caching for <see cref="IsConnected"/> and <see cref="IsAvailable"/>
    /// properties, and a background polling timer that raises
    /// <see cref="ConnectionStatusChangedEvent"/> when connection status changes.
    /// </summary>
    /// <remarks>
    /// Subclasses must implement <c>_computeIsConnectedValue()</c> and
    /// <c>_computeIsAvailableValue()</c> to provide the actual connectivity checks.
    /// The polling timer uses one-shot mode to prevent overlap: it fires once,
    /// the callback re-arms it after completing the check.
    /// </remarks>
    public abstract class Connection : IConnection
    {
        /// <summary>
        /// 2000 is the default timeout for the stream in milliseconds
        /// it is applied to both the read and write timeout
        /// </summary>
        protected int _timeout = 2000;

        /// <summary>
        /// The stream which can be accessed once a connection is made
        /// </summary>
        protected Stream _stream;

        /// <summary>
        /// Lock for synchronizing access to IsConnected cache fields
        /// </summary>
        private readonly object _isConnectedCacheLock = new object();

        /// <summary>
        /// last cached value of the isConnected flag
        /// </summary>
        protected bool _lastCacheIsConnected = false;


        /// <summary>
        /// The time (UTC/GMT) which the IsConnected property was cached
        /// </summary>
        private DateTime _lastCacheTimeIsConnected = DateTime.MinValue;

        /// <summary>
        /// The timeout for the isConnected Cache, this should be set
        /// to something useful in the child class
        /// </summary>
        protected int _isConnectedCacheTimeoutMs = 0;


        /// <summary>
        /// Whether this connection instance to a Brainboxes Device is active
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (_isConnectedCacheLock)
                {
                    if (_lastCacheTimeIsConnected > DateTime.UtcNow.AddMilliseconds(-_isConnectedCacheTimeoutMs))
                    {
                        return _lastCacheIsConnected;
                    }
                }

                // Compute new value outside lock (may do I/O)
                bool computedValue = _computeIsConnectedValue();

                // Update cache and fire event if changed
                ConnectionStatusChangedEventHandler handler = null;
                lock (_isConnectedCacheLock)
                {
                    _lastCacheTimeIsConnected = DateTime.UtcNow;
                    if (_lastCacheIsConnected != computedValue)
                    {
                        _lastCacheIsConnected = computedValue;
                        handler = _connectionStatusChangedEvent;
                    }
                }

                handler?.Invoke(this, "IsConnected", computedValue);

                // Return the computed value, not the cache (which could have been changed by another thread)
                return computedValue;
            }
            protected set
            {
                ConnectionStatusChangedEventHandler handler;
                lock (_isConnectedCacheLock)
                {
                    _lastCacheTimeIsConnected = DateTime.UtcNow;
                    if (_lastCacheIsConnected == value)
                    {
                        return;
                    }
                    _lastCacheIsConnected = value;
                    handler = _connectionStatusChangedEvent;
                }
                handler?.Invoke(this, "IsConnected", value);
            }
        }

        /// <summary>
        /// Computes the current IsConnected state. Override in derived classes.
        /// This method should not have side effects - just compute and return the value.
        /// </summary>
        protected virtual bool _computeIsConnectedValue()
        {
            return _lastCacheIsConnected;
        }

        /// <summary>
        /// Implementation specific method to update the status whether the connection is connected.
        /// Called by polling to refresh the cache. May have side effects (e.g., disconnect on error).
        /// </summary>
        protected virtual void _newValueIsConnected()
        {
            IsConnected = _computeIsConnectedValue();
        }

        /// <summary>
        /// Lock for synchronizing access to IsAvailable cache fields
        /// </summary>
        private readonly object _isAvailableCacheLock = new object();

        /// <summary>
        /// the last cached value of the isAvailable flag
        /// </summary>
        private bool _lastCacheIsAvailable = true;

        /// <summary>
        /// The time (UTC) which the IsAvailable property was cached
        /// </summary>
        private DateTime _lastCacheTimeIsAvailable = DateTime.MinValue;

        /// <summary>
        /// The timeout for the isAvailable Cache, this should be set
        /// to something useful in the child class
        /// </summary>
        protected int _isAvailableCacheTimeoutMs = 0;

        /// <summary>
        /// Whether this connection to a Brainboxes Device is available, e.g. online or offline​.
        /// In case of network TCP connection: this is if the device is pingable on the network.
        /// In case of serial connection: this is if the device COM port is listed on the system and not open by another process.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                lock (_isAvailableCacheLock)
                {
                    if (_lastCacheTimeIsAvailable > DateTime.UtcNow.AddMilliseconds(-_isAvailableCacheTimeoutMs))
                    {
                        return _lastCacheIsAvailable;
                    }
                }

                // Compute new value outside lock (may do I/O like ping)
                bool computedValue = _computeIsAvailableValue();

                // Update cache and fire event if changed
                ConnectionStatusChangedEventHandler handler = null;
                lock (_isAvailableCacheLock)
                {
                    _lastCacheTimeIsAvailable = DateTime.UtcNow;
                    if (_lastCacheIsAvailable != computedValue)
                    {
                        _lastCacheIsAvailable = computedValue;
                        handler = _connectionStatusChangedEvent;
                    }
                }

                handler?.Invoke(this, "IsAvailable", computedValue);

                // Return the computed value, not the cache (which could have been changed by another thread)
                return computedValue;
            }
            protected set
            {
                ConnectionStatusChangedEventHandler handler;
                lock (_isAvailableCacheLock)
                {
                    _lastCacheTimeIsAvailable = DateTime.UtcNow;
                    if (_lastCacheIsAvailable == value)
                    {
                        return;
                    }
                    _lastCacheIsAvailable = value;
                    handler = _connectionStatusChangedEvent;
                }
                handler?.Invoke(this, "IsAvailable", value);
            }
        }

        /// <summary>
        /// Computes the current IsAvailable state. Override in derived classes.
        /// This method should not have side effects - just compute and return the value.
        /// </summary>
        protected abstract bool _computeIsAvailableValue();

        /// <summary>
        /// Connection specific implementation to determine if the connection is available.
        /// Called by polling to refresh the cache. Default implementation uses _computeIsAvailableValue().
        /// </summary>
        protected virtual void _newValueIsAvailable()
        {
            IsAvailable = _computeIsAvailableValue();
        }


        /// <summary>
        /// Connection Status Changed Event Handler
        /// </summary>
        protected event ConnectionStatusChangedEventHandler _connectionStatusChangedEvent;

        /// <summary>
        /// Lock for synchronizing event registration and polling control
        /// </summary>
        private readonly object _eventRegistrationLock = new object();

        /// <summary>
        /// The number of registered connection status changed events
        /// </summary>
        protected int _numberOfEventsRegistered = 0;

        /// <summary>
        /// When the status of the connection changes this event is raised
        /// e.g. when <c>IsConnected</c> changes from false to true
        /// or when <c>IsAvailable</c> changes:
        /// * for example for a <c>TCPConnection</c> if the IP address is goes from on-line to off-line
        /// * or for example for a <c>SerialConnection</c> when the COM name is no longer present or in use by another program
        /// </summary>
        public event ConnectionStatusChangedEventHandler ConnectionStatusChangedEvent
        {
            add
            {
                lock (_eventRegistrationLock)
                {
                    _connectionStatusChangedEvent += value;
                    _numberOfEventsRegistered++;
                    if (_numberOfEventsRegistered == 1)
                    {
                        _startPollingForChanges();
                    }
                }
            }
            remove
            {
                lock (_eventRegistrationLock)
                {
                    _connectionStatusChangedEvent -= value;
                    _numberOfEventsRegistered--;
                    if (_numberOfEventsRegistered == 0)
                    {
                        _stopPollingForChanges();
                    }
                }
            }
        }

        #region Connection Status Polling
        /// <summary>
        /// The timer thread used for polling IO
        /// </summary>
        protected Timer _pollingTimer;
        /// <summary>
        /// flag to indicate whether the pollingThread should be running
        /// </summary>
        protected volatile bool _threadShouldBeRunning = false;

        /// <summary>
        /// Lock for synchronizing polling timer operations
        /// </summary>
        private readonly object _pollingLock = new object();

        /// <summary>
        /// start polling the connection for changes to the status of IsAvailable
        /// or IsConnected. Must be called while holding _eventRegistrationLock.
        /// </summary>
        protected void _startPollingForChanges()
        {
            lock (_pollingLock)
            {
                if (_threadShouldBeRunning || _numberOfEventsRegistered == 0) return;
                _threadShouldBeRunning = true;
                //make the first callback fire instantly, then reset the timer each time to prevent overlap
                _pollingTimer = new Timer(new TimerCallback(_pollingThreadFunc), null, 0, System.Threading.Timeout.Infinite);
            }
        }

        private void _pollingThreadFunc(object state)
        {
            lock (_pollingLock)
            {
                if (!_threadShouldBeRunning || _pollingTimer == null)
                {
                    return;
                }
            }

            // Call _newValueIsConnected outside the polling lock to avoid holding
            // multiple locks. _newValueIsConnected may have side effects (e.g., Disconnect).
            _newValueIsConnected();

            bool isConnected;
            lock (_isConnectedCacheLock)
            {
                isConnected = _lastCacheIsConnected;
            }

            // if its not connected check if its available
            if (!isConnected)
            {
                _newValueIsAvailable();
            }

            // Re-check after potentially long operations
            lock (_pollingLock)
            {
                if (_threadShouldBeRunning && _pollingTimer != null)
                {
                    //reset the timer to just run once
                    //this means that our polling function cannot overlap
                    try
                    {
                        _pollingTimer.Change(this.Timeout, System.Threading.Timeout.Infinite);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Timer was disposed between check and Change call
                    }
                }
            }
        }

        /// <summary>
        /// Stop the thread which is monitoring the connection.
        /// Must be called while holding _eventRegistrationLock.
        /// </summary>
        protected void _stopPollingForChanges()
        {
            lock (_pollingLock)
            {
                _threadShouldBeRunning = false;
                Timer timer = _pollingTimer;
                _pollingTimer = null;
                timer?.Dispose();
            }
        }

        #endregion Connection Status Polling


        /// <summary>
        /// The Connections underlying stream class
        /// </summary>
        public Stream Stream
        {
            get
            {
                return this._stream;
            }
        }

        /// <summary>
        /// Used to make the Connect and Disconnect command Threadsafe
        /// </summary>
        private object _connectDisconnectLock = new object();

        /// <summary>
        /// Connect to the ED Device
        /// </summary>
        public void Connect()
        {
            lock (_connectDisconnectLock)
            {
                if (this.IsConnected) return;
                Debug.WriteLine("Connecting over " + this.GetType().Name);
                this._connect();
                this._stream.Flush();
                Debug.WriteLine("Connected");
                this.IsConnected = true;
            }
        }

        /// <summary>
        /// Disconnect from the ED Device
        /// </summary>
        public void Disconnect()
        {
            lock (_connectDisconnectLock)
            {
                if (!this.IsConnected) return;
                Debug.WriteLine("Disconnecting...");
                this._disconnect();
                Debug.WriteLine("Disconnected");
                this.IsConnected = false;
            }
        }

        /// <summary>
        /// Override in the derived connection class
        /// </summary>
        protected abstract void _connect();

        /// <summary>
        /// Override in the derived connection class
        /// </summary>
        protected abstract void _disconnect();


        /// <summary>
        /// The timeout for Stream reads and writes, and connection availability test
        /// </summary>
        public abstract int Timeout
        {
            get;
            set;
        }


        /// <summary>
        /// Dispose of this Brainboxes device
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of this Brainboxes device
        /// </summary>
        /// <param name="itIsSafeToAlsoFreeManagedObjects"></param>
        protected void Dispose(Boolean itIsSafeToAlsoFreeManagedObjects)
        {
            //Free unmanaged resources

            //Free managed resources too, but only if I'm being called from Dispose
            //(If I'm being called from Finalize then the objects might not exist
            //anymore
            if (itIsSafeToAlsoFreeManagedObjects)
            {
                _stopPollingForChanges();
                this.Disconnect();
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GetType().Name.ToString();
        }

        /// <summary>
        /// Supply a connection IP address or COM port and the correct <see cref="IConnection"/> concrete class will be created and returned.
        /// </summary>
        /// <param name="ipAddressOrComPort">Either an IP address e.g. "192.168.0.1" or a COM port name e.g. "COM3".</param>
        /// <param name="portOrBaudRate">Optional. Either the IP port number (e.g. 9500 for ASCII, 502 for Modbus) or the COM port baud rate (e.g. 115200).</param>
        /// <param name="timeout">Optional. The read/write timeout for the connection in milliseconds. Defaults to 2000ms.</param>
        /// <returns>A <see cref="TCPConnection"/> if an IP address is provided, or a <see cref="SerialConnection"/> if a COM port name is provided.</returns>
        /// <example>
        /// <code>
        /// IConnection c1 = Connection.Create("192.168.0.5");       // Returns a TCPConnection
        /// IConnection c2 = Connection.Create("COM6", 9600);        // Returns a SerialConnection with baud rate 9600
        /// IConnection c3 = Connection.Create("192.168.0.5", 502);  // Returns a TCPConnection on Modbus port
        /// </code>
        /// </example>
        public static IConnection Create(string ipAddressOrComPort, int portOrBaudRate = 0, int timeout = 2000)
        {
            IPAddress ip;
            IConnection connection;
            if (IPAddress.TryParse(ipAddressOrComPort, out ip))
            {
                connection = new TCPConnection(ip.ToString(), portOrBaudRate == 0 ? 9500 : portOrBaudRate, timeout);
            }
            else
            {
                connection = new SerialConnection(ipAddressOrComPort, portOrBaudRate == 0 ? 115200 : portOrBaudRate, timeout);
            }
            return connection;
        }

    }

}
