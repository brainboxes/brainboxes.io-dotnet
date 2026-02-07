
namespace Brainboxes.IO
{
    /// <summary>
    /// The type of Serial Port
    /// </summary>
    public enum BBSerialPortType
    {
        /// <summary>
        /// RS-232 Serial Port
        /// </summary>
        RS232,
        /// <summary>
        /// RS-422/485 Serial Port
        /// </summary>
        RS422485,
        /// <summary>
        /// Software selectable serial port
        /// </summary>
        RS232_422485,
    }

    /// <summary>
    /// Encapsulates a Brainboxes Serial Port, communicates over TCP rather than using the Com port API, this means no drivers are required to be installed on the computer
    /// </summary>
    public class BBSerialPort : Device<IConnection, ISerialProtocol>
    {
        private BBSerialPortType _bbSerialPortType;
        /// <summary>
        /// The type of Serial port either RS232 or RS422/485
        /// </summary>
        public readonly BBSerialPortType BBSerialPortType;

        /// <summary>
        /// The port number of the device
        /// </summary>
        public readonly int PortNumber;

        /// <summary>
        /// Indicates whether data is available, can be 0, for no data, 1 for 1 or more bytes, 
        /// or a number representing the precise number of bytes
        /// </summary>
        public int DataAvailable 
        { 
            get 
            {
                return this._protocol.DataAvailable;
            } 
        }

        /// <summary>
        /// Transmit data to the serial port
        /// </summary>
        /// <param name="message"></param>
        public void Send(string data)
        {
            this._protocol.Send(data);
        }

        /// <summary>
        /// Receive data from the serial port
        /// </summary>
        /// <returns></returns>
        public string Receive()
        {
            return this._protocol.Receive();
        }

        internal BBSerialPort(int PortNumber, BBSerialPortType Type, IConnection connection, ISerialProtocol protocol)
            : base(connection, protocol)
        {
            this.PortNumber = PortNumber;
            this.BBSerialPortType = Type;
        }

        internal BBSerialPort(int PortNumber, BBSerialPortType Type, IConnection connection)
            : base(connection, new DefaultSerialProtocol())
        {
            this.PortNumber = PortNumber;
            this.BBSerialPortType = Type;
        }

        internal BBSerialPort(int PortNumber, BBSerialPortType Type)
            : base( new DefaultSerialProtocol())
        {
            this.PortNumber = PortNumber;
            this.BBSerialPortType = Type;
        }

    }
}
