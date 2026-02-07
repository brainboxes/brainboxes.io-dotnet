using System;
using System.Collections.Generic;

[assembly: CLSCompliant(true)]
namespace Brainboxes.IO
{
    /// <summary>
    /// The <c>Brainboxes.IO</c> namespace provides a .NET API for communicating with
    /// Brainboxes Ethernet-attached data acquisition and control devices.
    ///
    /// <para><b>Device Types</b></para>
    /// <list type="bullet">
    ///   <item><term>ED-series (<see cref="EDDevice"/>)</term>
    ///   <description>Ethernet Remote IO with digital and/or analog inputs and outputs.
    ///   Examples: <see cref="ED588"/> (8 DI + 8 DO), <see cref="ED516"/> (16 DI),
    ///   <see cref="ED527"/> (16 DO), <see cref="ED549"/> (8 AI), <see cref="ED560"/> (4 AO).</description></item>
    ///   <item><term>ES-series (<see cref="ESDevice"/>)</term>
    ///   <description>Ethernet to Serial adapters providing network-accessible serial ports.
    ///   Examples: <see cref="ES246"/> (1x RS232), <see cref="ES257"/> (2x RS232).</description></item>
    ///   <item><term><see cref="BB400"/></term>
    ///   <description>Industrial edge controller with 8 DI + 8 DO.</description></item>
    /// </list>
    ///
    /// <para><b>Architecture</b></para>
    /// <para>
    /// Each device combines a <see cref="IConnection"/> (how to reach the device) with an
    /// <see cref="IProtocol"/> (how to communicate). The two connection types are
    /// <see cref="TCPConnection"/> (Ethernet) and <see cref="SerialConnection"/> (COM port).
    /// The two IO protocols are <see cref="ASCIIProtocol"/> (human-readable, port 9500) and
    /// <see cref="ModbusTCPProtocol"/> (binary, port 502). For serial devices, use
    /// <see cref="DefaultSerialProtocol"/>.
    /// </para>
    ///
    /// <para><b>Quick Start</b></para>
    /// <para>
    /// For documentation and examples see
    /// <a href="https://docs.brainboxes.com/api/dotnet/Brainboxes.IO/">Brainboxes.IO Documentation</a>.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// // Auto-detect device type from IP address
    /// using (EDDevice ed = EDDevice.Create("192.168.0.80"))
    /// {
    ///     Console.WriteLine(ed.Inputs[0].Value);  // Read digital input 0
    ///     ed.Outputs[0].Value = 1;                // Set digital output 0 high
    /// }
    /// </code>
    /// </example>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    #region Remote IO

    /// <summary>
    /// Brainboxes BB-400 Neuron Edge Controller
    /// You can develop code to run remotely against the IO over the network or to run locally on the device itself
    /// The code remains the same, only (potentially) the IP address will change
    /// </summary>
    public class BB400: EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/bb-400">BB-400</see>: Brainboxes BB-400 Neuron Edge Controller
        /// </summary>
        public BB400() : base() { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/bb-400">BB-400</see>: Brainboxes BB-400 Neuron Edge Controller
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public BB400(IConnection connection) : base(connection) { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/bb-400">BB-400</see>: Brainboxes BB-400 Neuron Edge Controller
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public BB400(IConnection connection, IIOProtocol protocol) : base(connection, protocol)
        {
            if(protocol is ModbusTCPProtocol)
            {
                throw new InvalidOperationException("currently ModbusTCP is unsupported");
            }
        }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            // 8 digital input
            for (int i = 0; i < 8; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            for (int i = 8; i < 16; i++)
            {
                this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// ED-593: Ethernet to 8 Thermocouple Inputs + Serial Gateway
    /// </summary>
    public class ED593 : EDDevice
    {
        /// <summary>
        /// ED-593: Ethernet to 8 Thermocouple Inputs + Serial Gateway
        /// </summary>
        public ED593() : base() { }
        /// <summary>
        /// ED-593: Ethernet to 8 Thermocouple Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED593(IConnection connection) : base(connection) { }
        /// <summary>
        /// ED-593: Ethernet to 8 Thermocouple Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A Connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED593(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }
        internal override void _initLines()
        {
            {
                this.IOLines = new IOList<IOLine>(8);
                //8 Thermocouple inputs
                for(int i = 0; i < 8; i++)
                {
                    this.IOLines.Add(new IOLine(i, i, IODirection.AInput, IOType.Analog, this));
                }
            }
            base._initLines();
        }
    }
    /// <summary>
    /// ED-582: Ethernet to 4 RTD Inputs + Serial Gateway
    /// </summary>
    public class ED582 : EDDevice
    {
        /// <summary>
        /// ED-582: Ethernet to 4 RTD Inputs + Serial Gateway
        /// </summary>
        public ED582() : base() {}
        /// <summary>
        /// ED-582: Ethernet to 4 RTD Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED582(IConnection connection) : base(connection) { }
        /// <summary>
        /// ED-582: Ethernet to 4 RTD Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED582(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }
        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(4);
            //4 RTD Inputs
            for(int i = 0; i < 4; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.AInput, IOType.Analog, this));
            }
            base._initLines();
        }

    }
	/// <summary>
	/// ED-560: Ethernet to DIO 4 Analog Outputs + Serial Gateway
	/// </summary>
	public class ED560 : EDDevice
	{
		/// <summary>
		/// ED-560: Ethernet to DIO 4 Analog Outputs + Serial Gateway
		/// </summary>
		public ED560() : base() { }
		/// <summary>
		/// ED-560: Ethernet to DIO 4 Analog Outputs + Serial Gateway
		/// </summary>
		/// <param name="connection">A connection to the device</param>
		public ED560(IConnection connection) : base(connection) { }

		/// <summary>
		/// ED-560: Ethernet to DIO 4 Analog Outputs + Serial Gateway
		/// </summary>
		/// <param name="connection">A connection to the device</param>
		/// <param name="protocol">The protocol to use when communicating with the device</param>
		public ED560(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

		/// <summary>
		/// Initialize the IO Lines
		/// </summary>
		internal override void _initLines()
		{
			this.IOLines = new IOList<IOLine>(4);

			// 4 analog outputs
			for (int i = 0; i < 4; i++)
			{
				this.IOLines.Add(new IOLine(i, i, IODirection.AOutput, IOType.Analog, this));
			}
			base._initLines();
		}
	}



	/// <summary>
	/// ED-549: Ethernet to DIO 8 Analog Inputs + Serial Gateway
	/// </summary>
	public class ED549 : EDDevice
	{
		/// <summary>
		/// ED-549: Ethernet to DIO 8 Analog Inputs + Serial Gateway
		/// </summary>
		public ED549() : base() { }
		/// <summary>
		/// ED-549: Ethernet to DIO 8 Analog Inputs + Serial Gateway
		/// </summary>
		/// <param name="connection">A connection to the device</param>
		public ED549(IConnection connection) : base(connection) { }

		/// <summary>
		/// ED-549: Ethernet to DIO 8 Analog Inputs + Serial Gateway
		/// </summary>
		/// <param name="connection">A connection to the device</param>
		/// <param name="protocol">The protocol to use when communicating with the device</param>
		public ED549(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

		/// <summary>
		/// Initialize the IO Lines
		/// </summary>
		internal override void _initLines()
		{
			this.IOLines = new IOList<IOLine>(8);

			// 8 analog inputs
			for (int i = 0; i < 8; i++)
			{
				this.IOLines.Add(new IOLine(i, i, IODirection.AInput, IOType.Analog, this));
			}
			base._initLines();
		}
	}

	/// <summary>
	/// <see href="http://www.brainboxes.com/product/ed-588">ED-588</see>: Ethernet to DIO 8 Digital Inputs and 8 Digital Outputs + Serial Gateway
	/// </summary>
	public class ED588 : EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-588">ED-588</see>: Ethernet to DIO 8 Digital Inputs and 8 Digital Outputs + Serial Gateway
        /// </summary>
        public ED588() : base() { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-588">ED-588</see>: Ethernet to DIO 8 Digital Inputs and 8 Digital Outputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED588(IConnection connection) : base(connection) { }

        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-588">ED-588</see>: Ethernet to DIO 8 Digital Inputs and 8 Digital Outputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED588(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            // 8 digital inputs
            for (int i = 0; i < 8; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            // 8 digital outputs
            for (int i = 8; i < 16; i++)
            {
                this.IOLines.Add(new IOLine(i, i%8, IODirection.Output, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-538">ED-538</see>: Ethernet to DIO 4 Digital Relays and 8 Digital Inputs + Serial Gateway
    /// </summary>
    public class ED538 : EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-538">ED-538</see>: Ethernet to DIO 4 Digital Relays and 8 Digital Inputs + Serial Gateway
        /// </summary>
        public ED538() : base() { }

        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-538">ED-538</see>: Ethernet to DIO 4 Digital Relays and 8 Digital Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED538(IConnection connection) : base(connection) { }

        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-538">ED-538</see>: Ethernet to DIO 4 Digital Relays and 8 Digital Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED538(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            // 8 digital inputs
            for (int i = 0; i < 8; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            // 4 relays
            for (int i = 8; i < 12; i++)
            {
                this.IOLines.Add(new IOLine(i,i%8, IODirection.Output, IOType.Relay, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-516">ED-516</see>: Ethernet to DIO 16 Digital Inputs + Serial Gateway
    /// </summary>
    public class ED516 : EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-516">ED-516</see>: Ethernet to DIO 16 Digital Inputs + Serial Gateway
        /// </summary>
        public ED516() : base() { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-516">ED-516</see>: Ethernet to DIO 16 Digital Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED516(IConnection connection) : base(connection) { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-516">ED-516</see>: Ethernet to DIO 16 Digital Inputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED516(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            // 16 digital inputs
            for (int i = 0; i < 16; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-527">ED-527</see>: Ethernet to DIO 16 Digital Outputs + Serial Gateway
    /// </summary>
    public class ED527 : EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-527">ED-527</see>: Ethernet to DIO 16 Digital Outputs + Serial Gateway
        /// </summary>
        public ED527() : base() { }

        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-527">ED-527</see>: Ethernet to DIO 16 Digital Outputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED527(IConnection connection) : base(connection) { }

        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-527">ED-527</see>: Ethernet to DIO 16 Digital Outputs + Serial Gateway
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED527(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            // 16 digital outputs
            for (int i = 0; i < 16; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Output, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-004">ED-004</see>: Ethernet to 4 DIO + Ethernet to RS232
    /// </summary>
    public class ED004 : EDDevice
    {
        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/ed-004">ED-004</see>: Ethernet to 4 DIO + Ethernet to RS232
        /// </summary>
        public ED004() : base() { }

        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/ed-004">ED-004</see>: Ethernet to 4 DIO + Ethernet to RS232
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED004(IConnection connection) : base(connection) { }

        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/ed-004">ED-004</see>: Ethernet to 4 DIO + Ethernet to RS232
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED004(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(4);
            // 4 digital inputs
            for (int i = 0; i < 4; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            //4 digital outputs
            for (int i = 8; i < 12; i++)
            {
                this.IOLines.Add(new IOLine(i, i%8, IODirection.Output, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-204">ED-204</see>: Ethernet to 4 DIO + Ethernet to RS232
    /// </summary>
    public class ED204 : EDDevice
    {
        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/ed-204">ED-204</see>: Ethernet to 4 DIO + Ethernet to RS232
        /// </summary>
        public ED204() : base() { }

        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/ed-204">ED-204</see>: Ethernet to 4 DIO + Ethernet to RS232
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED204(IConnection connection) : base(connection) { }

        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/ed-204">ED-204</see>: Ethernet to 4 DIO + Ethernet to RS232
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED204(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(4);
            // 4 digital inputs
            for (int i = 0; i < 4; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            //4 digital outputs
            for (int i = 8; i < 12; i++)
            {
                this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-008">ED-008</see>: Ethernet to 8 Digital IO Ports
    /// </summary>
    public class ED008 : EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-008">ED-008</see>: Ethernet to 8 Digital IO Ports
        /// </summary>
        public ED008() : base() { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-008">ED-008</see>: Ethernet to 8 Digital IO Ports
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED008(IConnection connection) : base(connection) { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-008">ED-008</see>: Ethernet to 8 Digital IO Ports
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED008(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            // 8 digital input
            for (int i = 0; i < 8; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            for (int i = 8; i < 16; i++)
            {
                this.IOLines.Add(new IOLine(i, i % 8, IODirection.Output, IOType.Digital, this));
            }
            base._initLines();
        }
    }

    /// <summary>
    /// <see href="http://www.brainboxes.com/product/ed-038">ED-038</see>: Ethernet to 3 x Relay + 3 Digital Inputs
    /// </summary>
    public class ED038 : EDDevice
    {
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-038">ED-038</see>: Ethernet to 3 x Relay + 3 Digital Inputs
        /// </summary>
        public ED038() : base() { }
        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-038">ED-038</see>: Ethernet to 3 x Relay + 3 Digital Inputs
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        public ED038(IConnection connection) : base(connection) { }

        /// <summary>
        /// <see href="http://www.brainboxes.com/product/ed-038">ED-038</see>: Ethernet to 3 x Relay + 3 Digital Inputs
        /// </summary>
        /// <param name="connection">A connection to the device</param>
        /// <param name="protocol">The protocol to use when communicating with the device</param>
        public ED038(IConnection connection, IIOProtocol protocol) : base(connection, protocol) { }

        /// <summary>
        /// Initialize the IO Lines
        /// </summary>
        internal override void _initLines()
        {
            this.IOLines = new IOList<IOLine>(16);
            //3 Digital Inputs
            for (int i = 0; i < 3; i++)
            {
                this.IOLines.Add(new IOLine(i, i, IODirection.Input, IOType.Digital, this));
            }
            // 3 x Relay
            for (int i = 8; i < 11; i++)
            {
                this.IOLines.Add(new IOLine(i, i%8, IODirection.Output, IOType.Relay, this));
            }
            base._initLines();
        }
    }

    #endregion Remote IO

    #region Ethernet to Serial

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-511">ES-511</see> 1 Port RS232/422/485 Industrial Ethernet to Serial Adapter
    /// </summary>
    public class ES511 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-511">ES-511</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES511()
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-511">ES-511</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ES511(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-511">ES-511</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES511(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-511">ES-511</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES511(IConnection portConnection)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-511">ES-511</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES511(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }
    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-522">ES-522</see> 2 Port RS232/422/485 Industrial Ethernet to Serial Adapter
    /// </summary>
    public class ES522 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-522">ES-522</see> with 2 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES522()
            : base(2, BBSerialPortType.RS232_422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-522">ES-522</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ES522(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(2, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-522">ES-522</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES522(string ipAddress, ISerialProtocol protocol) : base(2, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-522">ES-522</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES522(IConnection portConnection)
            : base(2, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-522">ES-522</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES522(IConnection portConnection, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }
    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-551">ES-551</see> 1 Port RS232/422/485 Isolated Industrial Ethernet to Serial Adapter
    /// </summary>
    public class ES551 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-551">ES-551</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES551()
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-551">ES-551</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ES551(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-551">ES-551</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES551(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-551">ES-551</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES551(IConnection portConnection)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-551">ES-551</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES551(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }
    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-571">ES-571</see> 1 Port RS232/422/485 Industrial Ethernet to Serial Adapter + Switch
    /// </summary>
    public class ES571 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-571">ES-571</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES571()
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-571">ES-571</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ES571(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-571">ES-571</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES571(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-571">ES-571</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES571(IConnection portConnection)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-571">ES-571</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES571(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS232_422485)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }
    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-246">ES-246</see> 1 Port RS232 Ethernet to Serial Adapter
    /// </summary>
    public class ES246 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-246">ES-246</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES246()
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-246">ES-246</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ES246(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-246">ES-246</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES246(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-246">ES-246</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES246(IConnection portConnection)
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-246">ES-246</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES246(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-446">ES-446</see> 1 Port RS232 PoE Ethernet to Serial Adapter
    /// </summary>
    public class ES446 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-446">ES-446</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES446()
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-446">ES-446</see> with 1 Port PoE which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        /// 
        public ES446(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-446">ES-446</see> with 1 Port PoE which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES446(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-446">ES-446</see> with 1 Port PoE which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES446(IConnection portConnection)
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-446">ES-446</see> with 1 Port PoE which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES446(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS232)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-257">ES-257</see> 2 Port RS232 Ethernet to Serial Adapter
    /// </summary>
    public class ES257 : ESDevice
    {
        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/es-257">ES-257</see> 2 Port RS232 Ethernet to Serial Adapter
        /// </summary>
        public ES257()
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts();
        }



        /// <summary>
        /// Create an <see href="http://www.brainboxes.com/product/es-257">ES-257</see> with using given ip port number and timeout 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES257(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-257">ES-257</see> with 2 Ports which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES257(string ipAddress, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-257">ES-257</see> with 2 Ports which use the given connections 
        /// </summary>
        /// <param name="portConnections"></param>
        public ES257(List<IConnection> portConnections)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-257">ES-257</see> with 2 Ports which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES257(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-257">ES-257</see> with 2 Ports which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES257(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, protocols);
        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-457">ES-457</see> 2 Port RS232 PoE Ethernet to Serial Adapter
    /// </summary>
    public class ES457 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-457">ES-457</see> 2 Port RS232 PoE Ethernet to Serial Adapter
        /// </summary>
        public ES457()
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-457">ES-457</see> 2 Port RS232 PoE Ethernet to Serial Adapter using given ip address, port 1 port number and timeout
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES457(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-457">ES-457</see> with 2 Ports which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES457(string ipAddress, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-457">ES-457</see> with 2 Ports which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES457(List<IConnection> portConnections)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-457">ES-457</see> with 2 Ports which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES457(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-457">ES-457</see> with 2 Ports which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES457(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, protocols);
        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-320">ES-320</see> 1 Port RS422/485 Ethernet to Serial Adapter
    /// </summary>
    public class ES320 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-320">ES-320</see> with 1 Port RS422/485 which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES320()
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-320">ES-320</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES320(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-320">ES-320</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES320(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-320">ES-320</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES320(IConnection portConnection)
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-320">ES-320</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES320(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-420">ES-420</see> 1 Port RS422/485 PoE Ethernet to Serial Adapter
    /// </summary>
    public class ES420 : ESDevice
    {
        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-420">ES-420</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection unassigned
        /// </summary>
        public ES420()
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-420">ES-420</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the args
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES420(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-420">ES-420</see> with 1 Port which has uses the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES420(string ipAddress, ISerialProtocol protocol) : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-420">ES-420</see> with 1 Port which has uses the DefaultSerialProtocol and has its connection provided by the arg
        /// </summary>
        /// <param name="portConnection"></param>
        public ES420(IConnection portConnection)
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(new List<IConnection>() { portConnection });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-420">ES-420</see> with 1 Port which has uses the supplied serial protocol and has its connection
        /// </summary>
        /// <param name="portConnection"></param>
        /// <param name="protocol"></param>
        public ES420(IConnection portConnection, ISerialProtocol protocol)
            : base(1, BBSerialPortType.RS422485)
        {
            _initPorts(new List<IConnection>() { portConnection }, new List<ISerialProtocol>() { protocol });

        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter
    /// </summary>
    public class ES357 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        public ES357()
            : base(2, BBSerialPortType.RS232)
        {
            _initPorts();
        }

        /// <summary>
        /// Initialized the serial ports on the Ethernet to Serial device
        /// using the predefined properties NumberOfPorts and PortType
        /// </summary>
        protected new void _initPorts()
        {
            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts)
            {
                new BBSerialPort(1, BBSerialPortType.RS232),
                new BBSerialPort(2, BBSerialPortType.RS422485),
            };
            _ports = p.AsReadOnly();
        }

        protected new void _initPorts(string ipAddress, ISerialProtocol protocol)
        {
            IPAddress = ipAddress;

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts)
            {
                new BBSerialPort(1, BBSerialPortType.RS232, new TCPConnection(ipAddress, 9001)),
                new BBSerialPort(1, BBSerialPortType.RS422485, new TCPConnection(ipAddress, 9002))
            };
            _ports = p.AsReadOnly();

            Protocol = protocol;
        }

        protected new void _initPorts(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
        {
            IPAddress = ipAddress;

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts)
            {
                new BBSerialPort(1, BBSerialPortType.RS232, new TCPConnection(ipAddress, 9001, timeout)),
                new BBSerialPort(1, BBSerialPortType.RS422485, new TCPConnection(ipAddress, 9002, timeout))
            };

            _ports = p.AsReadOnly();
        }

        protected new void _initPorts(List<IConnection> connections)
        {
            if (connections.Count != NumberOfPorts)
            {
                throw new InvalidOperationException("There must be " + NumberOfPorts + " of connections");
            }

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts)
            {
                new BBSerialPort(1, BBSerialPortType.RS232, connections[0]),
                new BBSerialPort(1, BBSerialPortType.RS422485, connections[1])
            };

            _ports = p.AsReadOnly();
        }

        protected new void _initPorts(List<IConnection> connections, List<ISerialProtocol> protocols)
        {
            if (connections.Count != NumberOfPorts)
            {
                throw new InvalidOperationException("There must be " + NumberOfPorts + " of connections");
            }
            if (protocols.Count != 1 || protocols.Count != NumberOfPorts)
            {
                throw new InvalidOperationException("There must be at either one or " + NumberOfPorts + " protocols provided");
            }

            //there may only be one protocol which all the ports use
            ISerialProtocol sp = protocols.Count == 1 ? protocols[0] : null;

            List<BBSerialPort> p = new List<BBSerialPort>(NumberOfPorts)
            {
                new BBSerialPort(1, BBSerialPortType.RS232, connections[0], sp ?? protocols[0]),
                new BBSerialPort(1, BBSerialPortType.RS422485, connections[1], sp ?? protocols[1])
            };

            _ports = p.AsReadOnly();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES357(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES357(string ipAddress, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter which use the given port connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES357(List<IConnection> portConnections)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter which use the given port connections
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES357(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-357">ES-357</see> 1 Port RS232 and 1 Port RS422/485 Ethernet to Serial Adapter which use the given port connections
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES357(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, protocols);
        }

    }




    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter
    /// </summary>
    public class ES313 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        public ES313()
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES313(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES313(string ipAddress, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES313(List<IConnection> portConnections)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES313(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-313">ES-313</see> 2 Port RS422/485 Ethernet to Serial Adapter which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES313(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, protocols);
        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter
    /// </summary>
    public class ES413 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter
        /// </summary>
        public ES413()
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES413(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES413(string ipAddress, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES413(List<IConnection> portConnections)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES413(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-413">ES-413</see> 2 Port RS422/485 PoE Ethernet to Serial Adapter which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES413(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(2, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, protocols);
        }

    }


    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter
    /// </summary>
    public class ES701 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter
        /// </summary>
        public ES701()
            : base(4, BBSerialPortType.RS232)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES701(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(4, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES701(string ipAddress, ISerialProtocol protocol)
            : base(4, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES701(List<IConnection> portConnections)
            : base(4, BBSerialPortType.RS232)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES701(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(4, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-701">ES-701</see> 4 Port RS232 Ethernet to Serial Adapter which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES701(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(4, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, protocols);
        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter
    /// </summary>
    public class ES346 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        public ES346()
            : base(4, BBSerialPortType.RS422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port1PortNumber"></param>
        /// <param name="timeout"></param>
        public ES346(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(4, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter which use the given ip address and protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES346(string ipAddress, ISerialProtocol protocol)
            : base(4, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES346(List<IConnection> portConnections)
            : base(4, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES346(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(4, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Create Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-346</see> 4 Port RS422/485 Ethernet to Serial Adapter which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES346(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(4, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, protocols);
        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-279</see> 8 Port RS232 Ethernet to Serial Adapter
    /// </summary>
    public class ES279 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-279</see> 8 Port RS232 Ethernet to Serial Adapter
        /// </summary>
        public ES279()
            : base(8, BBSerialPortType.RS232)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-346">ES-279</see> 8 Port RS232 Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress">IP address of the device</param>
        /// <param name="port1PortNumber">the TCP port of port 1 of the device</param>
        /// <param name="timeout">the timeout when connecting to the device</param>
        public ES279(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(8, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-346">ES-279</see> with 8 Ports which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES279(string ipAddress, ISerialProtocol protocol)
            : base(8, BBSerialPortType.RS232)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-346">ES-279</see> with 8 Ports which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES279(List<IConnection> portConnections)
            : base(8, BBSerialPortType.RS232)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-346">ES-279</see> with 8 Ports which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES279(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(8, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Create <see href="http://www.brainboxes.com/product/es-346">ES-279</see> with 8 Ports which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES279(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(8, BBSerialPortType.RS232)
        {
            _initPorts(portConnections, protocols);
        }

    }

    /// <summary>
    /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter
    /// </summary>
    public class ES842 : ESDevice
    {
        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        public ES842()
            : base(8, BBSerialPortType.RS422485)
        {
            _initPorts();
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter
        /// </summary>
        /// <param name="ipAddress">IP address of the device</param>
        /// <param name="port1PortNumber">the TCP port of port 1 of the device</param>
        /// <param name="timeout">the timeout when connecting to the device</param>
        public ES842(string ipAddress, int port1PortNumber = 9001, int timeout = 2000)
            : base(8, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, port1PortNumber, timeout);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter which use the given protocol
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        public ES842(string ipAddress, ISerialProtocol protocol)
            : base(8, BBSerialPortType.RS422485)
        {
            _initPorts(ipAddress, protocol);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter which use the given connections
        /// </summary>
        /// <param name="portConnections"></param>
        public ES842(List<IConnection> portConnections)
            : base(8, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections);
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter which use the given connections and protocol
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocol"></param>
        public ES842(List<IConnection> portConnections, ISerialProtocol protocol)
            : base(8, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, new List<ISerialProtocol>() { protocol });
        }

        /// <summary>
        /// Brainboxes <see href="http://www.brainboxes.com/product/es-842">ES-842</see> 8 Port RS422/485 Ethernet to Serial Adapter which use the given connections and protocols
        /// </summary>
        /// <param name="portConnections"></param>
        /// <param name="protocols"></param>
        public ES842(List<IConnection> portConnections, List<ISerialProtocol> protocols)
            : base(8, BBSerialPortType.RS422485)
        {
            _initPorts(portConnections, protocols);
        }

    }

    #endregion Ethernet to Serial
}
