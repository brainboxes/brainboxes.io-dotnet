using System;
using System.Text;

namespace Brainboxes.IO
{
    /// <summary>
    /// Protocol interface for Brainboxes Ethernet-to-Serial devices (ES-series).
    /// Provides encoding-aware send/receive methods for text-based serial communication.
    /// Implements <see cref="ICloneable"/> so that each <see cref="BBSerialPort"/> on a
    /// multi-port device gets its own protocol instance with independent state.
    /// </summary>
    /// <remarks>
    /// The default implementation is <see cref="DefaultSerialProtocol"/>.
    /// Custom serial protocols can be created by implementing this interface
    /// and assigning them to <c>ESDevice.Ports[n].Protocol</c>.
    /// </remarks>
    public interface ISerialProtocol : IProtocol, ICloneable
    {
        /// <summary>
        /// The character encoding used to send and receive data. Default is UTF-8.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Indicates whether data is available to read.
        /// Returns 0 if no data, or a positive value indicating bytes are available.
        /// </summary>
        int DataAvailable { get; }

        /// <summary>
        /// Sends a message string through the serial port, encoded with <see cref="Encoding"/>.
        /// The terminating characters are appended automatically.
        /// </summary>
        /// <param name="message">The text message to send.</param>
        /// <exception cref="InvalidOperationException">The device is not connected.</exception>
        void Send(string message);

        /// <summary>
        /// Receives a message from the serial port, decoded with <see cref="Encoding"/>.
        /// Blocks until the terminating characters are received or the timeout expires.
        /// </summary>
        /// <returns>The received message string with terminating characters stripped.</returns>
        /// <exception cref="InvalidOperationException">The device is not connected.</exception>
        /// <exception cref="TimeoutException">No complete response received within the timeout period.</exception>
        string Receive();
    }
}
