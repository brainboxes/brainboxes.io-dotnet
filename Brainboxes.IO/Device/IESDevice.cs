using System.Collections.Generic;

namespace Brainboxes.IO
{
    /// <summary>
    /// Interface to Brainboxes Device with an Ethernet to serial port(s)
    /// </summary>
    public interface IESDevice
    {
        /// <summary>
        /// A list of the serial ports which the Ethernet to Serial
        /// device has. E.g. an ES-257 has 2 RS232 ports, an ES-346 has 4 RS422/485 ports
        /// </summary>
        IList<BBSerialPort> Ports {get;}
    }
}
