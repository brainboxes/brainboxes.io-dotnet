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

namespace Brainboxes.IO
{
    /// <summary>
    /// When the status of the devices' connection changes
    /// </summary>
    /// <param name="device">The device whose connection status has changed</param>
    /// <param name="property">The property which has changed either IsConnected or IsAvailable</param>
    /// <param name="newValue">The new value of the property </param>
    public delegate void DeviceStatusChangedEventHandler<C, P>(IDevice<C, P> device, string property, bool newValue)
            where C : IConnection
        where P : IProtocol;


    /// <summary>
    /// Interface to a Brainboxes Device
    /// </summary>
    public interface IDevice<C,P> : IDisposable
        where C : IConnection
        where P : IProtocol
    {
        /// <summary>
        /// The connection to this device
        /// </summary>
        C Connection { get; set; }

        /// <summary>
        /// The protocol used to communicate which this device
        /// </summary>
        P Protocol { get; set; }

        /// <summary>
        /// Whether the Brainboxes Device has an active connection from this connection instance
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Whether the connection to this Brainboxes Device is available, e.g. online or offline​. 
        /// In case of network TCP connection: this is if the device is pingable on the network. 
        /// In case of serial connection: this is if the device COM port is listed on the system and not open by another process.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Event called when the status of the device changes, either the IsConnected Property
        /// or the IsAvailable property
        /// </summary>
        event DeviceStatusChangedEventHandler<C,P> DeviceStatusChangedEvent;

        /// <summary>
        /// Open Connection to a Brainboxes Device, must be called before SendCommand
        /// Throws exception on connection failure
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from Brainboxes Device, will be automatically called when class is disposed
        /// </summary>
        void Disconnect();

    }
}
