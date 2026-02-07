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
using System.IO;
using System.Net;
using System.Xml;

namespace Brainboxes.IO
{
    /// <summary>
    /// A generic Brainboxes Device
    /// </summary>
    /// <typeparam name="Conn">A class which implements IConnection</typeparam>
    /// <typeparam name="Proto">A class which implements IProtocol</typeparam>
    public abstract class Device<Conn, Proto> : IDevice<Conn, Proto>
        where Conn : IConnection
        where Proto : IProtocol
    {
        /// <summary>
        /// Connects to the IP Address given and determines what kind of
        /// Brainboxes Device is there, creates the object and returns it
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static Device<Conn,Proto> Create(string ipAddress)
        {
            XmlDocument xmlDoc;
            Type esDeviceClass = DeviceTypeFromIP(ipAddress, out xmlDoc);
            if (esDeviceClass != null)
            {
                return System.Activator.CreateInstance(esDeviceClass, ipAddress) as Device<Conn, Proto>;
            }
            return null;
        }

        /// <summary>
        /// Connect to an IP address and download the devinfo.xml document
        /// look through the document and find the device's model number
        /// Convert the model number into a type
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="xmlDoc">if the XML file is required for further processing, it will be passed out</param>
        /// <returns></returns>
        internal static Type DeviceTypeFromIP(string ipAddress, out XmlDocument xmlDoc) 
        {
            try
            {
                string model = "";

                WebRequest request = WebRequest.Create("http://" + ipAddress + "/devinfo.xml");
                request.Timeout = 10000;
                using (WebResponse response = request.GetResponse())
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(response.GetResponseStream());
                    model = xmlDoc.GetElementsByTagName("model")[0].InnerText;
                }

                return Type.GetType("Brainboxes.IO." + model.Replace("-", "").ToUpper());
            }
            catch(Exception e)
            {
                // assume BB-400
                xmlDoc = null;
                return typeof(BB400);
            }
        }


        /// <summary>
        /// The connection
        /// </summary>
        protected Conn _connection;
        /// <summary>
        /// The protocol
        /// </summary>
        protected Proto _protocol;

        /// <summary>
        /// User definable label for the Device to help identify when many devices / when debugging
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Default generic constructor
        /// </summary>
        public Device()
        {

        }
        
        /// <summary>
        /// Construct with connection
        /// </summary>
        /// <param name="connection"></param>
        public Device(Conn connection)
        {
            this._connection = connection;
        }

        /// <summary>
        /// Construct with protocol
        /// </summary>
        /// <param name="protocol"></param>
        public Device(Proto protocol)
        {
            this._protocol = protocol;
        }

        /// <summary>
        /// Construct with connection and protocol
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="protocol"></param>
        public Device(Conn connection, Proto protocol)
        {
            //set the protocol first, so that if the connection is already connected we can hook up the streams
            this.Protocol = protocol;
            this.Connection = connection;
        }

        /// <summary>
        /// Whether this Brainboxes Device instance has an active connection
        /// </summary>
        public virtual bool IsConnected
        {
            get
            {
                var conn = this._connection;
                return conn != null && conn.IsConnected;
            }
        }

        /// <summary>
        /// Whether the connection to this Brainboxes Device is available, e.g. online or offline​.
        /// In case of network TCP connection: this is if the device is pingable on the network.
        /// In case of serial connection: this is if the device COM port is listed on the system and not open by another process.
        /// </summary>
        public virtual bool IsAvailable
        {
            get
            {
                var conn = this._connection;
                return conn != null && conn.IsAvailable;
            }
        }

        /// <summary>
        /// The event which describes when a device status change occurs
        /// </summary>
        protected event DeviceStatusChangedEventHandler<Conn,Proto> _deviceStatusChangedEvent;

        /// <summary>
        /// Lock for synchronizing event registration and connection changes
        /// </summary>
        private readonly object _deviceEventLock = new object();

        /// <summary>
        /// when the status of a connection changes this event is called which then
        /// fires a device status change event
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="property"></param>
        /// <param name="newValue"></param>
        protected void _connection_ConnectionStatusChangedEvent(IConnection connection, string property, bool newValue)
        {
            DeviceStatusChangedEventHandler<Conn, Proto> handler;
            lock (_deviceEventLock)
            {
                handler = _deviceStatusChangedEvent;
            }
            handler?.Invoke(this, property, newValue);
        }

        /// <summary>
        /// Event When the status of the devices connection changes
        /// for example goes from Disconnected to Connected
        /// or from Available to Unavailable
        /// </summary>
        public event DeviceStatusChangedEventHandler<Conn,Proto> DeviceStatusChangedEvent
        {
            add
            {
                lock (_deviceEventLock)
                {
                    bool wasEmpty = _deviceStatusChangedEvent == null;
                    _deviceStatusChangedEvent += value;
                    if (wasEmpty && this._connection != null)
                    {
                        this._connection.ConnectionStatusChangedEvent += _connection_ConnectionStatusChangedEvent;
                    }
                }
            }
            remove
            {
                lock (_deviceEventLock)
                {
                    _deviceStatusChangedEvent -= value;
                    if (_deviceStatusChangedEvent == null && this._connection != null)
                    {
                        this._connection.ConnectionStatusChangedEvent -= _connection_ConnectionStatusChangedEvent;
                    }
                }
            }
        }


        /// <summary>
        /// Connect the Brainboxes Device
        /// </summary>
        public virtual void Connect()
        {
            this._connection.Connect();
            //pass the connections stream to the protocol so it can send commands
            this._protocol.Stream = this.Connection.Stream;
        }
        ///// <summary>
        ///// Begin async connect to a Brainboxes device (TCPConnection only)
        ///// </summary>
        //public virtual void BeginConnect()
        //{
        //    this._connection.BeginConnect();
        //}
        ///// <summary>
        ///// End async connect to a Brainboxes device (TCPConnection only)
        ///// </summary>
        //public virtual void EndConnect()
        //{
        //    this._connection.EndConnect();
        //    //pass the connections stream to the protocol so it can send commands
        //    this._protocol.Stream = this.Connection.Stream;
        //}
        /// <summary>
        /// Disconnect the Brainboxes Device
        /// </summary>
        public virtual void Disconnect()
        {
            if (this._connection == null) return;
            this._connection.Disconnect();
            this._protocol.Stream = null;
        }

        /// <summary>
        /// The connection. If the connection is replaced and the previous connection was connected, the new connection will be connected too
        /// </summary>
        public virtual Conn Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                bool hadEvents;
                Conn oldConnection;
                bool wasConnected;

                lock (_deviceEventLock)
                {
                    hadEvents = _deviceStatusChangedEvent != null;
                    oldConnection = this._connection;
                    wasConnected = oldConnection != null && oldConnection.IsConnected;

                    //remove any added events from the old connection
                    if (hadEvents && oldConnection != null)
                    {
                        oldConnection.ConnectionStatusChangedEvent -= _connection_ConnectionStatusChangedEvent;
                    }

                    this._connection = value;

                    //add any existing events to the new connection
                    if (hadEvents && this._connection != null)
                    {
                        this._connection.ConnectionStatusChangedEvent += _connection_ConnectionStatusChangedEvent;
                    }
                }

                // Connection state changes done outside lock to avoid deadlocks
                if (wasConnected)
                {
                    oldConnection?.Disconnect();
                    if (value != null)
                    {
                        Connect();
                    }
                }
                else if (value != null && value.IsConnected)
                {
                    // if the passed in connection is already connected
                    Connect();
                }
            }
        }

        /// <summary>
        /// Marking as virtual allows it to be overridden in the child class
        /// </summary>
        public virtual Proto Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                this._protocol = value;
                //if the connection is already connected then set up the necessary to enable it to stay connected using this protocol
                if (this._connection != null && this._connection.IsConnected)
                {
                    Connect();
                }
            }
        }

        /// <summary>
        /// Describe this Brainboxes Device
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (this.Label == null ? "" : this.Label +" ") + base.ToString();
        }

        /// <summary>
        /// Give a complete summary of the Device
        /// </summary>
        /// <returns></returns>
        public virtual string Describe()
        {
            return this.ToString();
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
        protected virtual void Dispose(Boolean itIsSafeToAlsoFreeManagedObjects)
        {
            //Free unmanaged resources

            //Free managed resources too, but only if I'm being called from Dispose
            //(If I'm being called from Finalize then the objects might not exist
            //anymore
            if (itIsSafeToAlsoFreeManagedObjects)
            {
                Disconnect();
                _connection = default(Conn);
                _protocol = default(Proto);
            }
        }
    }

}
