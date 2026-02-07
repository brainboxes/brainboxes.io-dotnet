using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;

namespace Brainboxes.IO
{
    /// <summary>
    /// Base class for Brainboxes Ethernet-to-Serial devices (part numbers starting "ES-XXX").
    /// Provides network-accessible serial ports via the <see cref="Ports"/> collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each physical serial port is represented as a <see cref="BBSerialPort"/> with
    /// <see cref="BBSerialPort.Send"/> and <see cref="BBSerialPort.Receive"/> methods.
    /// The serial protocol (<see cref="DefaultSerialProtocol"/>) handles encoding and
    /// terminating characters.
    /// </para>
    /// <para>
    /// ES-series devices connect via <see cref="TCPConnection"/> (Ethernet), not
    /// <see cref="SerialConnection"/>. Use the <see cref="Create(string, int, int)"/>
    /// factory method or instantiate a specific subclass (e.g. <see cref="ES246"/>,
    /// <see cref="ES257"/>) directly.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// var es = new ES246("192.168.0.100");
    /// es.Connect();
    /// es.Ports[0].Send("Hello");
    /// string response = es.Ports[0].Receive();
    /// es.Disconnect();
    /// </code>
    /// </example>
    public partial class ESDevice : Device<IConnection, ISerialProtocol>, IESDevice
    {

        /// <summary>
        /// List of serial ports on Ethernet to Serial Device indexed by port number
        /// </summary>
        protected IList<BBSerialPort> _ports;

        /// <summary>
        /// Number of Serial Ports on Brainboxes Ethernet to Serial Device
        /// </summary>
        public readonly int NumberOfPorts;

        /// <summary>
        /// The type of Serial ports on the Brainboxes Ethernet to Serial Port
        /// Either RS232 or RS422/485
        /// </summary>
        public readonly BBSerialPortType PortType;

        /// <summary>
        /// The IP address of the Brainboxes Ethernet to Serial Device
        /// </summary>
        public string IPAddress;

        /// <summary>
        /// The ping timeout used when determining if the device is available on the network
        /// </summary>
        protected int _pingTimeout = 10000;

        /// <summary>
        /// Create a Brainboxes Ethernet to Serial device with the specified number of
        /// ports and port type
        /// </summary>
        /// <param name="numberOfPorts"></param>
        /// <param name="portType"></param>
        protected ESDevice(int numberOfPorts, BBSerialPortType portType)
        {
            NumberOfPorts = numberOfPorts;
            PortType = portType;
        }

        /// <summary>
        /// Create a generic ES Device uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ESDevice(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
        {
            NumberOfPorts = 8;
            PortType = BBSerialPortType.RS232;
            _initPorts(ipAddress, port1PortNumber, timeout);
        }


        /// <summary>
        /// Create a generic ES Device uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// <param name="numberOfPorts"></param>
        /// <param name="portType"></param>
        public ESDevice(string ipAddress, int port1PortNumber = 9001, int timeout = 2000, int numberOfPorts = 8, BBSerialPortType portType = BBSerialPortType.RS232)
        {
            NumberOfPorts = numberOfPorts;
            PortType = portType;
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Initialized the serial ports on the Ethernet to Serial device
        /// using the predefined properties NumberOfPorts and PortType
        /// </summary>
        protected void _initPorts()
        {
            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts);

            for (int i = 1; i <= NumberOfPorts; i++)
            {
                BBSerialPort port = new BBSerialPort(i, PortType);

                p.Add(port);
            }

            _ports = p.AsReadOnly();
        }

        protected void _initPorts(string ipAddress, ISerialProtocol protocol)
        {
            IPAddress = ipAddress;

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts);

            for (int i = 1; i <= NumberOfPorts; i++)
            {
                p.Add(new BBSerialPort(i, PortType, new TCPConnection(ipAddress, 9000 + i)));
            }
            _ports = p.AsReadOnly();

            Protocol = protocol;
        }

        protected void _initPorts( string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
        {
            IPAddress = ipAddress;

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts);

            for (int i = 1; i <= NumberOfPorts; i++)
            {
                p.Add(new BBSerialPort(i, PortType, new TCPConnection(ipAddress, port1PortNumber + i - 1, timeout)));
            }

            _ports = p.AsReadOnly();
        }

        protected void _initPorts(List<IConnection> connections)
        {
            if (connections.Count != NumberOfPorts)
            {
                throw new InvalidOperationException("There must be " + NumberOfPorts + " of connections");
            }

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts);

            for (int i = 1; i <= NumberOfPorts; i++)
            {
                p.Add(new BBSerialPort(i, PortType, connections[i - 1]));
            }
            _ports = p.AsReadOnly();
        }

        protected void _initPorts(List<IConnection> connections, List<ISerialProtocol> protocols)
        {
            if (connections.Count != NumberOfPorts)
            {
                throw new InvalidOperationException("There must be " + NumberOfPorts + " of connections");
            }
            if (protocols.Count != 1 || protocols.Count != NumberOfPorts)
            {
                throw new InvalidOperationException("There must be at either one or " + NumberOfPorts + " protocols provided");
            }


            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts);

            //there may only be one protocol which all the ports use
            ISerialProtocol sp = protocols.Count == 1 ? protocols[0] : null;

            for (int i = 1; i <= NumberOfPorts; i++)
            {
                p.Add(new BBSerialPort(i, PortType, connections[i - 1], sp ?? protocols[i - 1]));
            }
            _ports = p.AsReadOnly();
        }

        /// <summary>
        /// List of Brainboxes Serial Ports on this Ethernet to Serial Device
        /// </summary>
        public IList<BBSerialPort> Ports
        {
            get { return _ports; }
        }

        /// <summary>
        /// Connect the To All the serial ports on the brainboxes device
        /// </summary>
        public override void Connect()
        {
            foreach(BBSerialPort port in _ports)
            {
                port.Connect();
            }
        }

        /// <summary>
        /// Disconnect from all the serial ports on the Brainboxes Device
        /// </summary>
        public override void Disconnect()
        {
            foreach (BBSerialPort port in _ports)
            {
                port.Disconnect();
            }
        }

        /// <summary>
        /// The event which describes when a device status change occurs
        /// </summary>
        protected new event DeviceStatusChangedEventHandler<IConnection, ISerialProtocol> _deviceStatusChangedEvent;

        /// <summary>
        /// Event When the status of the devices connection changes
        /// for example goes from Disconnected to Connected
        /// or from Available to Unavailable
        /// </summary>
        public new event DeviceStatusChangedEventHandler<IConnection, ISerialProtocol> DeviceStatusChangedEvent
        {
            add
            {
                _deviceStatusChangedEvent += value;
                if (_deviceStatusChangedEvent.GetInvocationList().Length == 1 && this._connection != null)
                {
                    //attach it to all the serial ports
                    foreach(BBSerialPort port in this.Ports)
                    {
                        port.Connection.ConnectionStatusChangedEvent += _connection_ConnectionStatusChangedEvent;
                    }
                }
            }
            remove
            {
                _deviceStatusChangedEvent -= value;
                if (_deviceStatusChangedEvent == null && this._connection != null)
                {
                    foreach (BBSerialPort port in this.Ports)
                    {
                        port.Connection.ConnectionStatusChangedEvent -= _connection_ConnectionStatusChangedEvent;
                    }
                }
            }
        }

        /// <summary>
        /// Get/Set the Serial Protocol Used when communicating
        /// with all Serial ports of device
        /// </summary>
        public override ISerialProtocol Protocol
        {
            get
            {
                return this._ports[0].Protocol;
            }
            set
            {
                foreach (BBSerialPort port in _ports)
                {
                    port.Protocol = (ISerialProtocol)value.Clone();
                }
            }
        }

        /// <summary>
        /// Returns a short description of the Brainboxes Ethernet to Serial Product
        /// </summary>
        /// <returns></returns>
        public override string ToString() 
        {
            return (this.Label == null ? "" : this.Label + " ") + this.GetType().Name.ToString() + " : " + _ports.Count + " Port " + PortType.ToString();
        }

        /// <summary>
        /// If there is an active connection from this instance to any of the serial ports attached to the ES device
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                foreach(BBSerialPort port in _ports)
                {
                    if(port.IsConnected)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// ping class used to determine whether end point is available when it is not already connected to
        /// </summary>
        protected Ping _ping = new Ping();

        /// <summary>
        /// Whether the connection to this Brainboxes Device is available, e.g. online or offline​. 
        /// In case of network TCP connection: this is if the device is pingable on the network. 
        /// In case of serial connection: this is if the device COM port is listed on the system and not open by another process.
        /// </summary>
        public override bool IsAvailable
        {
            get
            {
                // if (IsConnected) return true;
                //no socket open yet, attempt ping
                PingReply pr = _ping.Send(this.IPAddress, this._pingTimeout);
                return pr.Status == IPStatus.Success;
            }
        }

        /// <summary>
        /// Connects to the IP Address given and determines what kind of
        /// Brainboxes Ethernet to Serial Device is there, creates the object and returns it
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ESDevice Create(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
        {
            XmlDocument xmlDoc;
            Type esDeviceClass = DeviceTypeFromIP(ipAddress, out xmlDoc);
            if (esDeviceClass != null)
            {
                return System.Activator.CreateInstance(esDeviceClass, ipAddress, port1PortNumber, timeout) as ESDevice;
            }
            return null;
        }

        /// <summary>
        /// Connects to the IP Address provided and determines what kind of Brainboxes
        /// Ethernet to Serial Device is present, creates the object and sets all serial ports to
        /// use the provided protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static ESDevice Create(string ipAddress, ISerialProtocol protocol)
        {
            XmlDocument xmlDoc;
            Type esDeviceClass = DeviceTypeFromIP(ipAddress, out xmlDoc);
            if (esDeviceClass != null)
            {
                return System.Activator.CreateInstance(esDeviceClass, ipAddress, protocol) as ESDevice;
            }
            return null;
        }

    }
    
}
