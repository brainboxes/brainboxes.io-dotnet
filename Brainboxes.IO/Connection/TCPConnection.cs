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
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Brainboxes.IO
{
    /// <summary>
    /// A TCP/IP connection to a Brainboxes device over Ethernet.
    /// This is the most common connection type for both ED-series (Remote IO) and ES-series (Ethernet to Serial) devices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TCP port number determines which protocol is used:
    /// </para>
    /// <list type="bullet">
    ///   <item>Port 9500 (<see cref="DEFAULT_ASCII_PORT"/>) — <see cref="ASCIIProtocol"/> (default, human-readable commands).</item>
    ///   <item>Port 502 (<see cref="DEFAULT_MODBUSTCP_PORT"/>) — <see cref="ModbusTCPProtocol"/> (binary Modbus TCP).</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // ASCII protocol (default)
    /// var conn = new TCPConnection("192.168.0.100");
    ///
    /// // Modbus TCP protocol
    /// var modbusConn = new TCPConnection("192.168.0.100", TCPConnection.DEFAULT_MODBUSTCP_PORT);
    /// </code>
    /// </example>
    public class TCPConnection : Connection
    {
        /// <summary>
        /// The default port for ASCII
        /// </summary>
        public static readonly int DEFAULT_ASCII_PORT = 9500;
        /// <summary>
        /// The default port for Modbus TCP
        /// </summary>
        public static readonly int DEFAULT_MODBUSTCP_PORT = 502;

        /// <summary>
        /// Create a TCP Connection to a Brainboxes EDDevice
        /// Use Brainboxes Boost.IO to find out the IP address of the Brainboxes EDDevice on your network
        /// </summary>
        /// <param name="ip">The IP address of the EDDevice to connect to, defaults to 192.168.127.255, which will be the case if the device is on a network without DHCP and the connecting computer is on the same subnet</param>
        /// <param name="port">The TCP IP Port number, defaults to 9500</param>
        /// <param name="timeout">The Read and Write Timeout for data, defaults to 2,000ms</param>
        /// <param name="connectionTimeout">The Connection Timeout for the TCP socket to open, defaults to 20,000ms</param>
        public TCPConnection(string ip = "192.168.127.255", int port = 9500, int timeout = 2000, int connectionTimeout = 20000)
        {
            this.IP = ip;
            this._port = port;
            this._timeout = timeout;
            this.connectionTimeout = connectionTimeout;
            this._isAvailableCacheTimeoutMs = timeout;
            this._isConnectedCacheTimeoutMs = 0; //this is a very quick and simple check, no need to cache
        }

        /// <summary>
        /// The IP address of the Connection to the Brainboxes Device e.g. "192.168.0.1"
        /// </summary>
        public readonly string IP;

        /// <summary>
        /// The TCP port of the connection to the Brainboxes Device
        /// </summary>
        protected int _port;


        /// <summary>
        /// Lock for synchronizing socket/stream field access
        /// </summary>
        private readonly object _socketLock = new object();

        /// <summary>
        /// The TCP port of the Connection to the Brainboxes Device e.g. "9500"
        /// </summary>
        public int Port
        {
            get { return this._port; }
            set
            {
                lock (_socketLock)
                {
                    if (_socket != null && _socket.Connected)
                    {
                        throw new InvalidOperationException("Cannot Change the port when the connection is open");
                    }
                    this._port = value;
                }
            }
        }


        private int connectionTimeout = 20000;

        /// <summary>
        /// Connection timeout for the socket in milliseconds, default 20 seconds
        /// This is approximately the same as the TCPClient connection timeout, however
        /// unlike the TCPClient, this connection timeout can be altered
        /// </summary>
        public int ConnectionTimeout
        {
            get
            {
                return connectionTimeout;
            }
            set
            {
                connectionTimeout = value;
            }
        }

        /// <summary>
        /// Computes whether the socket is connected by making a nonblocking, zero-byte Send call.
        /// This method has no side effects - it just returns the computed state.
        /// </summary>
        protected override bool _computeIsConnectedValue()
        {
            // Capture socket reference locally under lock to avoid race conditions during disconnect
            Socket socket;
            Stream stream;
            lock (_socketLock)
            {
                socket = this._socket;
                stream = this._stream;
            }

            if (socket == null || !socket.Connected)
            {
                return false;
            }

            try
            {
                BBStream bbStream = stream as BBStream;
                if (bbStream == null)
                {
                    return false;
                }

                lock (bbStream.streamWriteLock)
                {
                    // Re-check socket after acquiring lock in case disconnect happened while waiting
                    lock (_socketLock)
                    {
                        if (socket != this._socket || socket.Connected == false)
                        {
                            return false;
                        }
                    }

                    //see: https://msdn.microsoft.com/en-us/library/3btfke0h(v=vs.110).aspx
                    bool blockingState = socket.Blocking;
                    byte[] tmp = new byte[1];

                    socket.Blocking = false;
                    socket.Send(tmp, 0, 0);
                    socket.Blocking = blockingState;
                    return true;
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket was disposed by another thread (disconnect)
                return false;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK means socket is still connected but send would block
                if (e.NativeErrorCode.Equals(10035))
                {
                    Debug.WriteLine("Still Connected, but the Send would block");
                    return true;
                }
                else
                {
                    Debug.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    return false;
                }
            }
        }

        /// <summary>
        /// Updates IsConnected and handles socket cleanup on errors.
        /// Called by polling to refresh the cache.
        /// </summary>
        protected override void _newValueIsConnected()
        {
            // Capture socket reference locally under lock to avoid race conditions during disconnect
            Socket socket;
            Stream stream;
            lock (_socketLock)
            {
                socket = this._socket;
                stream = this._stream;
            }

            if (socket == null || !socket.Connected)
            {
                this.IsConnected = false;
                return;
            }

            try
            {
                BBStream bbStream = stream as BBStream;
                if (bbStream == null)
                {
                    this.IsConnected = false;
                    return;
                }

                lock (bbStream.streamWriteLock)
                {
                    // Re-check socket after acquiring lock in case disconnect happened while waiting
                    lock (_socketLock)
                    {
                        if (socket != this._socket || socket.Connected == false)
                        {
                            this.IsConnected = false;
                            return;
                        }
                    }

                    //see: https://msdn.microsoft.com/en-us/library/3btfke0h(v=vs.110).aspx
                    bool blockingState = socket.Blocking;
                    byte[] tmp = new byte[1];

                    socket.Blocking = false;
                    socket.Send(tmp, 0, 0);
                    this.IsConnected = true;
                    socket.Blocking = blockingState;
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket was disposed by another thread (disconnect)
                this.IsConnected = false;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    Debug.WriteLine("Still Connected, but the Send would block");
                }
                else
                {
                    Debug.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                }
                // disconnect, this will set the IsConnected flag to false
                this.Disconnect();
            }
        }

        //Added lock on isAvailable to stop possible DoS attack on device
        private Object _isAvailableLock = new Object();

        /// <summary>
        /// Computes whether the device is available by sending a ping.
        /// This method has no side effects - it just returns the computed state.
        /// </summary>
        protected override bool _computeIsAvailableValue()
        {
            //lock function as ping is not threadsafe
            //especially when waiting timeout, could inadvertantly DOS the device
            lock (_isAvailableLock)
            {
                try
                {
                    Ping p = new Ping();
                    PingReply pr = p.Send(this.IP, this._timeout);
                    return (pr.Status == IPStatus.Success);
                }
                catch (PingException pe)
                {
                    Debug.WriteLine("ping exception: " + pe);
                    return false;
                }
            }
        }

        /// <summary>
        /// sends a ping to the device and determines if it
        /// is still available on the network
        /// </summary>
        protected override void _newValueIsAvailable()
        {
            IsAvailable = _computeIsAvailableValue();
        }

        /// <summary>
        /// The connection and stream timeouts (for both stream read and stream write), measured in millseconds 
        /// </summary>
        public override int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                this._timeout = value;
                this._isAvailableCacheTimeoutMs = value;
                var localStream = this._stream;
                if (localStream != null && IsConnected)
                {
                    localStream.ReadTimeout = value;
                    localStream.WriteTimeout = value;
                }
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + " " + this.IP + ":" + this.Port;
        }

        //TODO: Don't think this needs to be internal
        //used by the BBStream class to create a networkStream from the socket
        internal Socket _socket;

        /// <summary>
        /// Connect to the Brainboxes Device on the specified IP address and port, within the ConnectionTimeout
        /// </summary>
        protected override void _connect()
        {
            Debug.WriteLine(this.IP + ":" + this.Port);
            Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = this._timeout,
                SendTimeout = this._timeout,
                // -- after testing this doesn't seem to improve anything ;o)
                //NoDelay = true //disable nagle algorithm - thanks to Nick Williamson of National Instruments
            };

            //using async methods allows us to implement a timeout
            IAsyncResult result = newSocket.BeginConnect(this.IP, this.Port, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(this.connectionTimeout, true);

            if (!success)
            {
                newSocket.Close();
                throw new SocketException(10060); // Connection timed out.
            }

            newSocket.EndConnect(result);

            lock (_socketLock)
            {
                this._socket = newSocket;
                this._stream = new BBNetworkStream(newSocket);
            }
        }
        /// <summary>
        /// close this connection to the Brainboxes Device
        /// </summary>
        protected override void _disconnect()
        {
            Socket socketToClose;
            Stream streamToDispose;
            lock (_socketLock)
            {
                socketToClose = this._socket;
                streamToDispose = this._stream;
                this._socket = null;
                this._stream = null;
            }
            // Dispose the stream first (wraps NetworkStream which wraps the socket),
            // then close the socket
            streamToDispose?.Dispose();
            socketToClose?.Close();
        }

    }
}
