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

namespace Brainboxes.IO
{
    /// <summary>
    /// When the status of the connection changes this event is raised
    /// e.g. when <c>IsConnected</c> changes from false to true
    /// or when <c>IsAvailable</c> changes:
    /// * for example for a <c>TCPConnection</c> if the IP address is goes from online to offline
    /// * or for example for a <c>SerialConnection</c> when the COM name is no longer present or in use by another program
    /// </summary>
    /// <param name="connection">The connection whose property changed</param>
    /// <param name="property">The property which has changed either IsConnected or IsAvailable</param>
    /// <param name="newValue">The new value of the property </param>
    public delegate void ConnectionStatusChangedEventHandler(IConnection connection, string property, bool newValue);


    /// <summary>
    /// Interface describing a connection to a Brainboxes device.
    /// </summary>
    /// <remarks>
    /// Two implementations are available:
    /// <list type="bullet">
    ///   <item><see cref="TCPConnection"/> — Ethernet TCP/IP connection (most common).
    ///   Port 9500 selects <see cref="ASCIIProtocol"/>, port 502 selects <see cref="ModbusTCPProtocol"/>.</item>
    ///   <item><see cref="SerialConnection"/> — direct COM port connection via a virtual serial port
    ///   (requires Brainboxes Boost.IO Manager). Not for ES-series Ethernet-to-Serial devices.</item>
    /// </list>
    /// Use <see cref="Connection.Create(string, int, int)"/> to auto-detect the correct type.
    /// </remarks>
    public interface IConnection : IDisposable
    {
        /// <summary>
        /// Whether the Brainboxes Device has an active connection from this connection instance
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Whether this connection to a Brainboxes Device is available, e.g. online or offline​. 
        /// In case of network TCP connection: this is if the device is pingable on the network. 
        /// In case of serial connection: this is if the device COM port is listed on the system and not open by another process.
        /// </summary>
        bool IsAvailable
        {
            get;
        }

        /// <summary>
        /// When the status of the connection changes this event is raised
        /// e.g. when <c>IsConnected</c> changes from false to true
        /// or when <c>IsAvailable</c> changes:
        /// * for example for a <c>TCPConnection</c> if the IP address is goes from online to offline
        /// * or for example for a <c>SerialConnection</c> when the COM name is no longer present or in use by another program
        /// </summary>
        event ConnectionStatusChangedEventHandler ConnectionStatusChangedEvent;

        /// <summary>
        /// Timeout for Reads and Writes
        /// </summary>
        int Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// The Connections underlying stream which it exposes once a connection has been initiated
        /// </summary>
        Stream Stream
        {
            get;
        }

        /// <summary>
        /// Connect to a Brainboxes Device
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from a Brainboxes Device
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Begin async connect to a Brainboxes Device
        /// </summary>
        //void BeginConnect();
        /// <summary>
        /// End async connect to a Brainboxes Device
        /// </summary>
        //void EndConnect();

    }
}
