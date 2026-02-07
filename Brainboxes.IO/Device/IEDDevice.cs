using System.Collections.Generic;

namespace Brainboxes.IO
{
    /// <summary>
    /// When the status of an IO line changes, this can happen in 1 of 6 ways:
    /// ALL TYPES OF LINE:
    /// (1) the previous sampled value was 0 and the current sampled value is 1, RISING EDGE
    /// (2) the previous sampled value was 1 and the current sampled value is 0, FALLING EDGE
    /// ONLY INPUT LINES:
    /// (3) the previous sampled value was 1 and the current sampled value is 1, 
    /// but between the 2 samplings the low latch has triggered which means the line has gone from 1 to 0 and back to 1 again, LATCHED (includes falling and rising edge)
    /// (4) the previous sampled value was 0 and the current sampled value is 0, 
    /// but between the 2 samplings the high latch has triggered which means the line has gone from 0 to 1 and back to 0 again, LATCHED (includes rising and falling edge)
    /// ONLY OUTPUT LINES:
    /// (5) When the line is set changing it from a 0 to a 1, RISING EDGE
    /// (6) When the line is set changing it from a 1 to a 0, FALLING EDGE
    /// </summary>
    /// <param name="line"></param>
    /// <param name="device"></param>
    /// <param name="changeType">The type of change that occurred to cause this event to be fired</param>
    public delegate void IOLineChangedEventHandler(IOLine line, EDDevice device, IOChangeTypes changeType);
    /// <summary>
    /// When the status of 1 or more IO lines change on a device within a polling interval be notified of all the lines for which there is a change:
    /// ALL TYPES OF LINE:
    /// (1) the previous sampled value was 0 and the current sampled value is 1, RISING EDGE
    /// (2) the previous sampled value was 1 and the current sampled value is 0, FALLING EDGE
    /// ONLY INPUT LINES:
    /// (3) the previous sampled value was 1 and the current sampled value is 1, 
    /// but between the 2 samplings the low latch has triggered which means the line has gone from 1 to 0 and back to 1 again, LATCHED (includes falling and rising edge)
    /// (4) the previous sampled value was 0 and the current sampled value is 0, 
    /// but between the 2 samplings the high latch has triggered which means the line has gone from 0 to 1 and back to 0 again, LATCHED (includes rising and falling edge)
    /// ONLY OUTPUT LINES:
    /// (5) When the line is set changing it from a 0 to a 1, RISING EDGE
    /// (6) When the line is set changing it from a 1 to a 0, FALLING EDGE
    /// </summary>
    /// <param name="lines">The list of IO Lines which have changed within a polling interval</param>
    /// <param name="device"></param>
    public delegate void IOLinesChangedEventHandler(List<IOLine> lines, EDDevice device);
    /// <summary>
    /// When the status of 1 or more Analog IO lines change on a device within a polling internal be notified of the lines for which there is a change:
    /// TYPES OF EVENT:
    /// DELTA: When the line has changed more than the specified delta since the previous sampled value
    /// TARGET: When the line has changed to go above or below the specified target since the previous sampled value
    /// TARGET RANGE: When the line has changed to either enter the delta range of the target value or exit the delta
    /// range of the target value since the previous sampled value
    /// </summary>
    /// <param name="line"></param>
    /// <param name="device"></param>
    /// <param name="value"></param>
    /// <param name="changeType">The type of change that occurred to cause this event to be fired
    /// Delta event: Only Delta = 0 returned
    /// Target event: Above = 2 if the previous sampled value was below and the current sampled value goes above the specified target value
    ///               Below = 1 if the previous sampled value was above and the current sampled value goes below the specified target value
    /// Target Range event: Enter = 3 if the previous sampled value was outside of the delta range of the target value and the current sampled
    ///                     value goes inside the delta range of the target value
    ///                     Exit = 4 if the previous sampled value was inside of the delta range of the target value and the current sampled
    ///                     value goes outside the delta range of the target value</param>
    public delegate void AIOLineChangedEventHandler(IOLine line, EDDevice device, double value, AIOChangeTypes changeType);
    /// <summary>
    /// Remote Ethernet IO interface
    /// These devices use a command response protocol, almost every command sent to the device receives a response
    /// </summary>
    public interface IEDDevice : IDevice<IConnection, IIOProtocol>
    {
        /// <summary>
        /// Send a command to the Brainboxes Remote IO device using the supplied protocol.
        /// </summary>
        /// <param name="command">command to send</param>
        /// <returns>Response or null if no response</returns>
        string SendCommand(string command);

        /// <summary>
        /// IOLines indexed by their logical line number as described on the brainboxes product label
        /// </summary>
        IOList<IOLine> IOLines { get; }

        /// <summary>
        /// The devices output Lines (if it has any) indexed by IOLineNumber e.g. DOUT0, DOUT1, DOUT2
        /// </summary>
        IOList<IOLine> Outputs { get; }

        /// <summary>
        /// The devices input Lines (if it has any) indexed by IOLineNumber e.g. DIN0, DIN1, DIN2
        /// </summary>
        IOList<IOLine> Inputs { get; }
        /// <summary>
        /// The devices analog input lines (if it has any) indexed by IOLineNumber e.g. AIN0, AIN1, AIN2
        /// </summary>
        IOList<IOLine> AInputs { get; }
        /// <summary>
        /// The device analog output lines (if it has any) indexed by IOLineNumber e.g AOUT0, AOUT1, AOUT2
        /// </summary>
        IOList<IOLine> AOutputs { get; }
        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// one or more IOLines change within a polling interval
        /// This is useful when a state in the program is dependent on 2 or more ioLine states 
        /// </summary>
        event IOLinesChangedEventHandler IOLinesChanged;

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input or output line changes
        /// </summary>
        event IOLineChangedEventHandler IOLineChanged;

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input or output line goes from 0 -> 1 / low to high / closed to open
        /// </summary>
        event IOLineChangedEventHandler IOLineRisingEdge;

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input or output line goes from 1 -> 0 / high to low / open to closed
        /// </summary>
        event IOLineChangedEventHandler IOLineFallingEdge;

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input changes increment the count of the IOLine
        /// </summary>
        event IOLineChangedEventHandler IOLineCount;
    }
}
