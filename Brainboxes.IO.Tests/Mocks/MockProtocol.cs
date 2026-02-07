using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Brainboxes.IO.Tests.Mocks
{
    /// <summary>
    /// Mock protocol for testing without hardware. Implements IIOProtocol
    /// with configurable simulated device state.
    /// </summary>
    public class MockProtocol : IIOProtocol
    {
        private Stream _stream;

        // Simulated device state
        private int _digitalLineStates;
        private double[] _analogInputStates;
        private double[] _analogOutputStates;
        private int[] _lineCounts;
        private int _latchedHighStates;
        private int _latchedLowStates;

        // Configuration
        private string _deviceName = "MockDevice";
        private IOChangeTypes _counterUpdateDirection = IOChangeTypes.RisingEdge;

        /// <summary>
        /// Creates a MockProtocol with default configuration.
        /// </summary>
        public MockProtocol()
        {
            NumberOfDigitalInputs = 8;
            NumberOfDigitalOutputs = 8;
            NumberOfAnalogInputs = 0;
            NumberOfAnalogOutputs = 0;

            _analogInputStates = new double[8];
            _analogOutputStates = new double[4];
            _lineCounts = new int[16];
        }

        #region Configuration Properties

        /// <summary>
        /// Number of digital inputs the simulated device has.
        /// </summary>
        public int NumberOfDigitalInputs { get; set; }

        /// <summary>
        /// Number of digital outputs the simulated device has.
        /// </summary>
        public int NumberOfDigitalOutputs { get; set; }

        /// <summary>
        /// Number of analog inputs the simulated device has.
        /// </summary>
        public int NumberOfAnalogInputs { get; set; }

        /// <summary>
        /// Number of analog outputs the simulated device has.
        /// </summary>
        public int NumberOfAnalogOutputs { get; set; }

        /// <summary>
        /// When true, SendCommand will throw a TimeoutException.
        /// </summary>
        public bool SimulateTimeout { get; set; }

        /// <summary>
        /// When true, SendCommand will return an invalid response.
        /// </summary>
        public bool SimulateInvalidResponse { get; set; }

        /// <summary>
        /// Track all commands sent through this protocol. Thread-safe.
        /// </summary>
        public ConcurrentBag<string> CommandHistory { get; } = new ConcurrentBag<string>();

        #endregion

        #region IProtocol Implementation

        /// <summary>
        /// Sets the stream used by this protocol.
        /// </summary>
        public Stream Stream
        {
            set => _stream = value;
        }

        #endregion

        #region IIOProtocol Implementation

        /// <summary>
        /// Simulates sending a command and returns a response.
        /// </summary>
        public string SendCommand(string command)
        {
            CommandHistory.Add(command);

            if (SimulateTimeout)
            {
                throw new TimeoutException("Simulated timeout");
            }

            if (SimulateInvalidResponse)
            {
                return "?01\r"; // Invalid command response
            }

            // Return appropriate response based on command
            // Most commands will be handled by the specific Get/Set methods
            return "!01\r"; // Generic success response
        }

        /// <summary>
        /// Gets all digital line states as a bitmask.
        /// </summary>
        public int GetAllDigitalLineStates()
        {
            CommandHistory.Add("GetAllDigitalLineStates");
            return _digitalLineStates;
        }

        /// <summary>
        /// Legacy method - delegates to GetAllDigitalLineStates.
        /// </summary>
        [Obsolete]
        public int GetAllLineStates()
        {
            return GetAllDigitalLineStates();
        }

        /// <summary>
        /// Sets all digital output line states.
        /// </summary>
        public void SetAllDigitalOutputLineStates(int states, int numberOfOutputs)
        {
            CommandHistory.Add($"SetAllDigitalOutputLineStates({states}, {numberOfOutputs})");

            // Mask to only affect output bits
            int outputMask = (1 << numberOfOutputs) - 1;
            int inputMask = ~outputMask;

            // Preserve input states, update output states
            _digitalLineStates = (_digitalLineStates & inputMask) | (states & outputMask);
        }

        /// <summary>
        /// Legacy method - delegates to SetAllDigitalOutputLineStates.
        /// </summary>
        [Obsolete]
        public void SetAllOutputLineStates(int states, int numberOfOutputs)
        {
            SetAllDigitalOutputLineStates(states, numberOfOutputs);
        }

        /// <summary>
        /// Gets the state of a single digital line.
        /// </summary>
        public int GetDigitalLineState(int line, bool isInput = true)
        {
            CommandHistory.Add($"GetDigitalLineState({line}, {isInput})");
            return (_digitalLineStates >> line) & 1;
        }

        /// <summary>
        /// Legacy method - delegates to GetDigitalLineState.
        /// </summary>
        [Obsolete]
        public int GetLineState(int line, bool isInput = true)
        {
            return GetDigitalLineState(line, isInput);
        }

        /// <summary>
        /// Sets a single digital output line.
        /// </summary>
        public void SetDigitalOutputLineState(int line, int value)
        {
            CommandHistory.Add($"SetDigitalOutputLineState({line}, {value})");

            if (value == 0)
            {
                _digitalLineStates &= ~(1 << line);
            }
            else
            {
                _digitalLineStates |= (1 << line);
            }
        }

        /// <summary>
        /// Legacy method - delegates to SetDigitalOutputLineState.
        /// </summary>
        [Obsolete]
        public void SetOutputLineState(int line, int value)
        {
            SetDigitalOutputLineState(line, value);
        }

        /// <summary>
        /// Gets all analog input line states.
        /// </summary>
        public double[] GetAllAnalogInputLineStates(int numberOfInputs = 8)
        {
            CommandHistory.Add($"GetAllAnalogInputLineStates({numberOfInputs})");

            var result = new double[numberOfInputs];
            for (int i = 0; i < numberOfInputs && i < _analogInputStates.Length; i++)
            {
                result[i] = _analogInputStates[i];
            }
            return result;
        }

        /// <summary>
        /// Gets all analog output line states.
        /// </summary>
        public double[] GetAllAnalogOutputLineStates(int numberOfOutputs = 4)
        {
            CommandHistory.Add($"GetAllAnalogOutputLineStates({numberOfOutputs})");

            var result = new double[numberOfOutputs];
            for (int i = 0; i < numberOfOutputs && i < _analogOutputStates.Length; i++)
            {
                result[i] = _analogOutputStates[i];
            }
            return result;
        }

        /// <summary>
        /// Gets a single analog line state.
        /// </summary>
        public double GetAnalogLineState(int line, int numberOfLines = 8, bool isInput = true)
        {
            CommandHistory.Add($"GetAnalogLineState({line}, {numberOfLines}, {isInput})");

            if (isInput)
            {
                return line < _analogInputStates.Length ? _analogInputStates[line] : double.NaN;
            }
            return line < _analogOutputStates.Length ? _analogOutputStates[line] : double.NaN;
        }

        /// <summary>
        /// Sets a single analog output line.
        /// </summary>
        public void SetAnalogOutputLineState(int line, double value)
        {
            CommandHistory.Add($"SetAnalogOutputLineState({line}, {value})");

            if (line < _analogOutputStates.Length)
            {
                _analogOutputStates[line] = value;
            }
        }

        /// <summary>
        /// Counter update direction (rising/falling edge).
        /// </summary>
        public IOChangeTypes IOCounterUpdateDirection => _counterUpdateDirection;

        /// <summary>
        /// Gets the count for a digital input line.
        /// </summary>
        public int GetDigitalInputLineCount(int line)
        {
            CommandHistory.Add($"GetDigitalInputLineCount({line})");
            return line < _lineCounts.Length ? _lineCounts[line] : 0;
        }

        /// <summary>
        /// Legacy method - delegates to GetDigitalInputLineCount.
        /// </summary>
        [Obsolete]
        public int GetLineCount(int line)
        {
            return GetDigitalInputLineCount(line);
        }

        /// <summary>
        /// Clears the count for a digital input line.
        /// </summary>
        public void ClearDigitalInputLineCount(int line)
        {
            CommandHistory.Add($"ClearDigitalInputLineCount({line})");
            if (line < _lineCounts.Length)
            {
                _lineCounts[line] = 0;
            }
        }

        /// <summary>
        /// Legacy method - delegates to ClearDigitalInputLineCount.
        /// </summary>
        [Obsolete]
        public void ClearLineCount(int line)
        {
            ClearDigitalInputLineCount(line);
        }

        /// <summary>
        /// Gets all latched high digital input states.
        /// </summary>
        public int GetAllLatchedHighDigitalInputStates()
        {
            CommandHistory.Add("GetAllLatchedHighDigitalInputStates");
            return _latchedHighStates;
        }

        /// <summary>
        /// Legacy method.
        /// </summary>
        [Obsolete]
        public int GetAllLatchedHighInputStates()
        {
            return GetAllLatchedHighDigitalInputStates();
        }

        /// <summary>
        /// Gets all latched low digital input states.
        /// </summary>
        public int GetAllLatchedLowDigitalInputStates()
        {
            CommandHistory.Add("GetAllLatchedLowDigitalInputStates");
            return _latchedLowStates;
        }

        /// <summary>
        /// Legacy method.
        /// </summary>
        [Obsolete]
        public int GetAllLatchedLowInputStates()
        {
            return GetAllLatchedLowDigitalInputStates();
        }

        /// <summary>
        /// Clears all latched digital inputs.
        /// </summary>
        public void ClearAllLatchedDigitalInputs()
        {
            CommandHistory.Add("ClearAllLatchedDigitalInputs");
            _latchedHighStates = 0;
            _latchedLowStates = 0;
        }

        /// <summary>
        /// Legacy method.
        /// </summary>
        [Obsolete]
        public void ClearAllLatchedInputs()
        {
            ClearAllLatchedDigitalInputs();
        }

        /// <summary>
        /// Device name.
        /// </summary>
        public string DeviceName
        {
            get => _deviceName;
            set => _deviceName = value;
        }

        /// <summary>
        /// Gets the device name (uncached).
        /// </summary>
        public string GetDeviceName()
        {
            CommandHistory.Add("GetDeviceName");
            return _deviceName;
        }

        /// <summary>
        /// Gets device configuration (caches internally).
        /// </summary>
        public void GetDeviceConfiguration()
        {
            CommandHistory.Add("GetDeviceConfiguration");
            // No-op for mock - configuration is already set via properties
        }

        /// <summary>
        /// Sets device configuration for a digital device.
        /// </summary>
        public void SetDeviceConfiguration(int newAddress, int newBaudRate, IOChangeTypes ioCounterUpdateDirection, CounterMode counterMode, bool checksum)
        {
            CommandHistory.Add($"SetDeviceConfiguration({newAddress}, {newBaudRate}, {ioCounterUpdateDirection}, {counterMode}, {checksum})");
            _counterUpdateDirection = ioCounterUpdateDirection;
        }

        /// <summary>
        /// Sets device configuration for an analog device.
        /// </summary>
        public void SetDeviceConfiguration(int newAddress, int newBaudRate, AnalogDataFormat newAnalogDataFormat, bool newFilterSettings, bool newModuleSettings, bool newChecksum, TemperatureUnit temperatureUnit)
        {
            CommandHistory.Add($"SetDeviceConfiguration(analog: {newAddress}, {newBaudRate}, {newAnalogDataFormat})");
        }

        /// <summary>
        /// Resets to factory default settings.
        /// </summary>
        public void ResetToFactoryDefaultSettings()
        {
            CommandHistory.Add("ResetToFactoryDefaultSettings");
            _digitalLineStates = 0;
            _latchedHighStates = 0;
            _latchedLowStates = 0;
            Array.Clear(_lineCounts, 0, _lineCounts.Length);
            Array.Clear(_analogInputStates, 0, _analogInputStates.Length);
            Array.Clear(_analogOutputStates, 0, _analogOutputStates.Length);
        }

        /// <summary>
        /// Restarts the device.
        /// </summary>
        public void Restart()
        {
            CommandHistory.Add("Restart");
        }

        #endregion

        #region Test Helper Methods

        /// <summary>
        /// Sets a specific digital input state (for simulating external input changes).
        /// </summary>
        public void SetDigitalInputState(int line, int value)
        {
            if (value == 0)
            {
                _digitalLineStates &= ~(1 << line);
            }
            else
            {
                _digitalLineStates |= (1 << line);
            }
        }

        /// <summary>
        /// Sets a specific analog input state (for simulating sensor readings).
        /// </summary>
        public void SetAnalogInputState(int line, double value)
        {
            if (line < _analogInputStates.Length)
            {
                _analogInputStates[line] = value;
            }
        }

        /// <summary>
        /// Triggers a latch event on a line.
        /// </summary>
        public void TriggerLatch(int line, bool high)
        {
            if (high)
            {
                _latchedHighStates |= (1 << line);
            }
            else
            {
                _latchedLowStates |= (1 << line);
            }
        }

        /// <summary>
        /// Increments the counter for a line.
        /// </summary>
        public void IncrementCounter(int line)
        {
            if (line < _lineCounts.Length)
            {
                _lineCounts[line]++;
            }
        }

        /// <summary>
        /// Sets the counter value for a line.
        /// </summary>
        public void SetCounterValue(int line, int value)
        {
            if (line < _lineCounts.Length)
            {
                _lineCounts[line] = value;
            }
        }

        /// <summary>
        /// Sets the counter update direction.
        /// </summary>
        public void SetCounterUpdateDirection(IOChangeTypes direction)
        {
            _counterUpdateDirection = direction;
        }

        /// <summary>
        /// Clears the command history. Thread-safe.
        /// </summary>
        public void ClearCommandHistory()
        {
            // ConcurrentBag doesn't have Clear(), so drain all items
            while (CommandHistory.TryTake(out _)) { }
        }

        #endregion
    }
}
