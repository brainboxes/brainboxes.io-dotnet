using System;
using System.Collections;
using System.Collections.Generic;

namespace Brainboxes.IO
{
    /// <summary>
    /// Defines the command interface for Brainboxes Remote IO devices (ED-series).
    /// Provides methods for reading and writing digital and analog IO lines,
    /// managing input counters and latches, and configuring device settings.
    /// </summary>
    /// <remarks>
    /// Two implementations are available:
    /// <list type="bullet">
    ///   <item><see cref="ASCIIProtocol"/> — human-readable commands on TCP port 9500 (default).</item>
    ///   <item><see cref="ModbusTCPProtocol"/> — binary Modbus TCP on port 502.</item>
    /// </list>
    /// The protocol is selected automatically based on the <see cref="TCPConnection"/> port number.
    /// You typically access protocol methods through an <see cref="EDDevice"/> rather than calling
    /// them directly.
    /// </remarks>
    public interface IIOProtocol : IProtocol
    {
        /// <summary>
        /// Send a raw command string using the underlying protocol and return the response.
        /// </summary>
        /// <param name="command">The protocol-specific command string to send to the device.</param>
        /// <returns>The response string from the device, or <c>null</c> if the command expects no response.</returns>
        /// <exception cref="InvalidOperationException">The device is not connected.</exception>
        /// <exception cref="TimeoutException">No response was received within the timeout period.</exception>
        string SendCommand(string command);

        /// <summary>
        /// Get all digital line states as a bitmask integer.
        /// </summary>
        /// <returns>An integer where each bit represents the state of the corresponding digital IO line
        /// (bit 0 = line 0, bit 1 = line 1, etc.). A set bit (1) indicates high/closed, a clear bit (0) indicates low/open.</returns>
        [Obsolete("GetAllLineStates is deprecated. Replaced by GetAllDigitalLineStates")]
        int GetAllLineStates();

        /// <summary>
        /// Get all digital line states as a bitmask integer.
        /// </summary>
        /// <returns>An integer where each bit represents the state of the corresponding digital IO line
        /// (bit 0 = line 0, bit 1 = line 1, etc.). A set bit (1) indicates high/closed, a clear bit (0) indicates low/open.</returns>
        int GetAllDigitalLineStates();

        /// <summary>
        /// Set all digital output line states simultaneously using a bitmask integer.
        /// </summary>
        /// <param name="states">Bitmask where each bit represents the desired state of the corresponding output line
        /// (1 = high/closed, 0 = low/open).</param>
        /// <param name="numberOfOutputs">The number of digital output lines on the device (e.g. 8 for ED-588).</param>
        [Obsolete("SetAllOutputLineStates is deprecated. Replaced by SetAllDigitalOutputLineStates")]
        void SetAllOutputLineStates(int states, int numberOfOutputs);

        /// <summary>
        /// Set all digital output line states simultaneously using a bitmask integer.
        /// </summary>
        /// <param name="states">Bitmask where each bit represents the desired state of the corresponding output line
        /// (1 = high/closed, 0 = low/open).</param>
        /// <param name="numberOfOutputs">The number of digital output lines on the device (e.g. 8 for ED-588).</param>
        void SetAllDigitalOutputLineStates(int states, int numberOfOutputs);

        /// <summary>
        /// Gets the state of all analog input lines on the device.
        /// </summary>
        /// <param name="numberOfInputs">The number of analog input lines to read. Defaults to 8.</param>
        /// <returns>An array of analog values indexed by line number. Disabled or unavailable lines return <c>double.NaN</c>.</returns>
        double[] GetAllAnalogInputLineStates(int numberOfInputs = 8);

        /// <summary>
        /// Gets the state of all analog output lines on the device.
        /// </summary>
        /// <param name="numberOfOutputs">The number of analog output lines to read. Defaults to 4.</param>
        /// <returns>An array of analog values indexed by line number.</returns>
        double[] GetAllAnalogOutputLineStates(int numberOfOutputs = 4);

        /// <summary>
        /// Indicates whether the input counter increments on a rising edge or falling edge transition.
        /// </summary>
        IOChangeTypes IOCounterUpdateDirection { get; }

        /// <summary>
        /// Reads the input counter value of the specified digital input line.
        /// </summary>
        /// <param name="line">Zero-based digital input line number.</param>
        /// <returns>The current counter value (16-bit or 32-bit depending on <see cref="CounterMode"/>).</returns>
        [Obsolete("GetLineCount is deprecated. Replaced by GetDigitalInputLineCount")]
        Int32 GetLineCount(int line);

        /// <summary>
        /// Reads the input counter value of the specified digital input line.
        /// </summary>
        /// <param name="line">Zero-based digital input line number.</param>
        /// <returns>The current counter value (16-bit or 32-bit depending on <see cref="CounterMode"/>).</returns>
        Int32 GetDigitalInputLineCount(int line);

        /// <summary>
        /// Clears (resets to zero) the input counter of the specified digital input line.
        /// </summary>
        /// <param name="line">Zero-based digital input line number.</param>
        [Obsolete("ClearLineCount is deprecated. Replaced by ClearDigitalInputLineCount")]
        void ClearLineCount(int line);

        /// <summary>
        /// Clears (resets to zero) the input counter of the specified digital input line.
        /// </summary>
        /// <param name="line">Zero-based digital input line number.</param>
        void ClearDigitalInputLineCount(int line);

        /// <summary>
        /// Set the state of an individual digital output line.
        /// </summary>
        /// <param name="line">Zero-based digital output line number.</param>
        /// <param name="value">The desired state: 0 for low/open/off, 1 for high/closed/on.</param>
        [Obsolete("SetOutputLineState is deprecated. Replaced by SetDigitalOutputLineState")]
        void SetOutputLineState(int line, int value);

        /// <summary>
        /// Set the state of an individual digital output line.
        /// </summary>
        /// <param name="line">Zero-based digital output line number.</param>
        /// <param name="value">The desired state: 0 for low/open/off, 1 for high/closed/on.</param>
        void SetDigitalOutputLineState(int line, int value);

        /// <summary>
        /// Set the value of an individual analog output line.
        /// </summary>
        /// <param name="line">Zero-based analog output line number.</param>
        /// <param name="value">The analog value to set (range depends on device and <see cref="AnalogDataFormat"/>).</param>
        void SetAnalogOutputLineState(int line, double value);

        /// <summary>
        /// Get the state of an individual digital line.
        /// </summary>
        /// <param name="line">Zero-based digital line number.</param>
        /// <param name="isInput">If <c>true</c>, reads an input line; if <c>false</c>, reads an output line.</param>
        /// <returns>The digital line state: 0 for low/open, 1 for high/closed.</returns>
        [Obsolete("GetLineState is deprecated. Replaced by GetDigitalLineState")]
        int GetLineState(int line, bool isInput = true);

        /// <summary>
        /// Get the state of an individual digital line.
        /// </summary>
        /// <param name="line">Zero-based digital line number.</param>
        /// <param name="isInput">If <c>true</c> (default), reads an input line; if <c>false</c>, reads an output line.</param>
        /// <returns>The digital line state: 0 for low/open, 1 for high/closed.</returns>
        int GetDigitalLineState(int line, bool isInput = true);

        /// <summary>
        /// Get the value of an individual analog line.
        /// </summary>
        /// <param name="line">Zero-based analog line number.</param>
        /// <param name="numberOfLines">Total number of analog lines on the device. Defaults to 8.</param>
        /// <param name="isInput">If <c>true</c> (default), reads an input line; if <c>false</c>, reads an output line.</param>
        /// <returns>The analog value, or <c>double.NaN</c> if the line is disabled.</returns>
        double GetAnalogLineState(int line, int numberOfLines = 8, bool isInput = true);

        /// <summary>
        /// Get all latched-high digital input states as a bitmask. A latched-high state indicates
        /// the input transitioned from low to high since the last read or clear.
        /// </summary>
        /// <returns>Bitmask where each set bit indicates the corresponding input line was latched high.</returns>
        [Obsolete("GetAllLatchedHighInputStates is deprecated. Replaced by GetAllLatchedHighDigitalInputStates")]
        int GetAllLatchedHighInputStates();

        /// <summary>
        /// Get all latched-high digital input states as a bitmask. A latched-high state indicates
        /// the input transitioned from low to high since the last read or clear.
        /// </summary>
        /// <returns>Bitmask where each set bit indicates the corresponding input line was latched high.</returns>
        int GetAllLatchedHighDigitalInputStates();

        /// <summary>
        /// Get all latched-low digital input states as a bitmask. A latched-low state indicates
        /// the input transitioned from high to low since the last read or clear.
        /// </summary>
        /// <returns>Bitmask where each set bit indicates the corresponding input line was latched low.</returns>
        [Obsolete("GetAllLatchedLowInputStates is deprecated. Replaced by GetAllLatchedLowDigitalInputStates")]
        int GetAllLatchedLowInputStates();

        /// <summary>
        /// Get all latched-low digital input states as a bitmask. A latched-low state indicates
        /// the input transitioned from high to low since the last read or clear.
        /// </summary>
        /// <returns>Bitmask where each set bit indicates the corresponding input line was latched low.</returns>
        int GetAllLatchedLowDigitalInputStates();

        /// <summary>
        /// Clear all latched digital input states, resetting them so new transitions can be detected.
        /// </summary>
        [Obsolete("ClearAllLatchedInputs is deprecated. Replaced by ClearAllLatchedDigitalInputs")]
        void ClearAllLatchedInputs();

        /// <summary>
        /// Clear all latched digital input states, resetting them so new transitions can be detected.
        /// </summary>
        void ClearAllLatchedDigitalInputs();

        /// <summary>
        /// The cached name of the device as reported by the device firmware.
        /// </summary>
        string DeviceName { get; set; }

        /// <summary>
        /// Queries the device for its name, bypassing any cached value.
        /// </summary>
        /// <returns>The device name string as reported by the firmware (e.g. "ED-588").</returns>
        string GetDeviceName();

        /// <summary>
        /// Queries the device for its configuration settings and caches them locally
        /// (address, baud rate, counter mode, analog data format, etc.).
        /// </summary>
        void GetDeviceConfiguration();

        /// <summary>
        /// Sets configuration parameters for a digital IO device.
        /// </summary>
        /// <param name="newAddress">The new ASCII/Modbus address for the device (0-255).</param>
        /// <param name="newBaudRate">The new baud rate (e.g. 9600, 115200).</param>
        /// <param name="ioCounterUpdateDirection">Whether the input counter increments on <see cref="IOChangeTypes.RisingEdge"/> or <see cref="IOChangeTypes.FallingEdge"/>.</param>
        /// <param name="counterMode">The counter bit width: 16-bit or 32-bit.</param>
        /// <param name="checksum">Whether to enable checksum validation on commands.</param>
        /// <exception cref="NotImplementedException">Thrown by <see cref="ModbusTCPProtocol"/> — device configuration is only supported via the ASCII protocol.</exception>
        void SetDeviceConfiguration(int newAddress, int newBaudRate, IOChangeTypes ioCounterUpdateDirection, CounterMode counterMode, bool checksum);

        /// <summary>
        /// Sets configuration parameters for an analog IO device.
        /// </summary>
        /// <param name="newAddress">The new ASCII/Modbus address for the device (0-255).</param>
        /// <param name="newBaudRate">The new baud rate (e.g. 9600, 115200).</param>
        /// <param name="newAnalogDataFormat">The analog data format: <see cref="AnalogDataFormat.Engineering"/>, <see cref="AnalogDataFormat.FullScaleRange"/>, or <see cref="AnalogDataFormat.Hexadecimal"/>.</param>
        /// <param name="newFilterSettings">Whether to enable input filtering.</param>
        /// <param name="newModuleSettings">Whether to enable module-specific settings.</param>
        /// <param name="newChecksum">Whether to enable checksum validation on commands.</param>
        /// <param name="temperatureUnit">The temperature unit for thermocouple/RTD devices: <see cref="TemperatureUnit.Celsius"/>, <see cref="TemperatureUnit.Fahrenheit"/>, or <see cref="TemperatureUnit.Kelvin"/>.</param>
        /// <exception cref="NotImplementedException">Thrown by <see cref="ModbusTCPProtocol"/> — device configuration is only supported via the ASCII protocol.</exception>
        void SetDeviceConfiguration(int newAddress, int newBaudRate, AnalogDataFormat newAnalogDataFormat, bool newFilterSettings, bool newModuleSettings, bool newChecksum, TemperatureUnit temperatureUnit);

        /// <summary>
        /// Resets the device to its factory default settings. The device will restart.
        /// Any cached state should be cleared after calling this method.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by <see cref="ModbusTCPProtocol"/> — factory reset is only supported via the ASCII protocol.</exception>
        void ResetToFactoryDefaultSettings();

        /// <summary>
        /// Restarts the device. The connection will be temporarily lost and re-established.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by <see cref="ModbusTCPProtocol"/> — restart is only supported via the ASCII protocol.</exception>
        void Restart();
    }
}
