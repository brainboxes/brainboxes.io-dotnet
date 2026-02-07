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

using System.IO;

namespace Brainboxes.IO
{
    /// <summary>
    /// Abstract base class for all Brainboxes communication protocols.
    /// Manages the underlying <see cref="Stream"/> used for command/response communication.
    /// </summary>
    /// <remarks>
    /// Concrete implementations:
    /// <list type="bullet">
    ///   <item><see cref="ASCIIProtocol"/> — human-readable ASCII/DCON protocol on TCP port 9500.</item>
    ///   <item><see cref="ModbusTCPProtocol"/> — binary Modbus TCP protocol on port 502.</item>
    ///   <item><see cref="DefaultSerialProtocol"/> — text-based serial protocol for ES-series devices.</item>
    /// </list>
    /// </remarks>
    public abstract class Protocol : IProtocol
    {
        /// <summary>
        /// the stream which the protocol data is written and read from
        /// </summary>
        protected Stream stream;

        /// <summary>
        /// Set the stream which the protocol data is written and read from
        /// </summary>
        public Stream Stream
        {
            set { stream = value; }
        }

    }


}
