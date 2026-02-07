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
    /// Base interface for all communication protocols used by Brainboxes devices.
    /// A protocol defines how commands are formatted, sent over the underlying
    /// <see cref="Stream"/>, and how responses are parsed.
    /// </summary>
    /// <remarks>
    /// The Brainboxes.IO library provides two protocol hierarchies:
    /// <list type="bullet">
    ///   <item><see cref="IIOProtocol"/> — for Remote IO devices (ED-series).
    ///   Implementations: <see cref="ASCIIProtocol"/> (port 9500) and
    ///   <see cref="ModbusTCPProtocol"/> (port 502).</item>
    ///   <item><see cref="ISerialProtocol"/> — for Ethernet-to-Serial devices (ES-series).
    ///   Implementation: <see cref="DefaultSerialProtocol"/>.</item>
    /// </list>
    /// You do not normally instantiate protocols directly. When creating a device
    /// via <see cref="EDDevice.Create(string, int, int)"/> or a device constructor,
    /// the appropriate protocol is selected automatically based on the connection port.
    /// </remarks>
    public interface IProtocol
    {
        /// <summary>
        /// The stream on which the protocol sends commands and reads responses.
        /// Set automatically when a device connection is opened.
        /// </summary>
        Stream Stream
        {
            set;
        }

    }
}
