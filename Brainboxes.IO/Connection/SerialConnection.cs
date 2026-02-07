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
using System.IO.Ports;

namespace Brainboxes.IO
{
    /// <summary>
    /// A connection to a Brainboxes device via a direct COM/serial port.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is for connecting to an ED-series device via a virtual COM port
    /// (installed using Brainboxes Boost.IO Manager). It is <b>not</b> for
    /// Ethernet-to-Serial (ES-series) devices — those use <see cref="TCPConnection"/>.
    /// </para>
    /// </remarks>
    public class SerialConnection : Connection
    {
        /// <summary>
        /// Lock for synchronizing serial port access
        /// </summary>
        private readonly object _serialPortLock = new object();

        /// <summary>
        /// The underlying serial port object
        /// </summary>
        protected SerialPort _serialPort;

        /// <summary>
        /// The name of the serial port
        /// </summary>
        public string PortName
        {
            get
            {
                return this._serialPort.PortName;
            }
        }

        /// <summary>
        /// The serial port baud rate
        /// </summary>
        public int BaudRate
        {
            get
            {
                return this._serialPort.BaudRate;
            }
        }

        /// <summary>
        /// Create a Serial connection to a Brainboxes EDDevice
        /// First install the virtual com port for the device using Brainboxes Boost.IO Manager
        /// </summary>
        /// <param name="portName">By default when installing a virtual COM port using Brainboxes Boost.IO Manager the lowest available COM port is chosen (greater than 2)</param>
        /// <param name="baudRate">Baud rate is irrelevant to brainboxes EDDevice as the serial port is virtual, but some software may be expecting to set/read it</param>
        /// <param name="Timeout">The Read and Write Timeout for data, defaults to 2000ms</param>
        public SerialConnection(string portName = "COM3", int baudRate = 115200, int Timeout = 2000)
        {
            this._timeout = Timeout;
            //the baud rate is irrelevant for brainboxes ED Device as the serial port is virtual
            this._serialPort = new SerialPort(portName, baudRate)
            {
                //important when sending incorrect commands to prevent blocking
                ReadTimeout = this._timeout,
                WriteTimeout = this._timeout,
            };
            //both are relatively quick checks and do not need caching
            this._isAvailableCacheTimeoutMs = 0;
            this._isConnectedCacheTimeoutMs = 0;
        }

        /// <summary>
        /// Computes whether the serial port is open.
        /// </summary>
        protected override bool _computeIsConnectedValue()
        {
            lock (_serialPortLock)
            {
                return this._serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Computes whether the serial port exists and is available to connect to.
        /// </summary>
        protected override bool _computeIsAvailableValue()
        {
            string portName;
            lock (_serialPortLock)
            {
                if (this._serialPort.IsOpen)
                {
                    return true; //already connected
                }
                portName = this._serialPort.PortName;
            }

            //check the port name exists
            bool portExists = false;
            foreach (string port in SerialPort.GetPortNames())
            {
                if (portName == port)
                {
                    portExists = true;
                    break;
                }
            }
            if (!portExists)
            {
                return false; //the port doesn't exist
            }

            //check the port is available to connect to
            lock (_serialPortLock)
            {
                // Re-check if already open after acquiring lock
                if (this._serialPort.IsOpen)
                {
                    return true;
                }

                try
                {
                    this._serialPort.Open();
                }
                catch (Exception)
                {
                    return false; //the port is open somewhere else
                }
                this._serialPort.Close();
            }
            return true;
        }


        public override int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                this._timeout = this._serialPort.ReadTimeout = this._serialPort.WriteTimeout = value;
                if (IsConnected)
                {
                    this._stream.ReadTimeout = value;
                    this._stream.WriteTimeout = value;
                }
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + " " + this.PortName + " @ " + this.BaudRate;
        }

        /// <summary>
        /// Opens a new connection to a serial port
        /// </summary>
        protected override void _connect()
        {
            lock (_serialPortLock)
            {
                Debug.WriteLine(this.PortName + " @ " + this.BaudRate + " baud");
                this._serialPort.Open();
                this._stream = new BBSerialStream(this._serialPort);
            }
        }


        /// <summary>
        /// Closed the port connection
        /// </summary>
        protected override void _disconnect()
        {
            lock (_serialPortLock)
            {
                Stream streamToDispose = this._stream;
                this._stream = null;
                // Dispose the stream before closing the serial port
                streamToDispose?.Dispose();
                this._serialPort.Close();
            }
        }
    }
}
