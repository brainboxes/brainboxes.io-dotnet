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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Brainboxes.IO
{
    /// <summary>
    /// Specifies the data format for analog line readings from Brainboxes ED-series analog devices
    /// (e.g. <see cref="ED549"/>, <see cref="ED560"/>).
    /// </summary>
    public enum AnalogDataFormat
    {
        /// <summary>
        /// Undefined if the analog data format is not retrieved from the device
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// The actual value read from the analogue input line
        /// </summary>
        Engineering = 0,

        /// <summary>
        /// The actual value read from the analogue input line as a percentage
        /// </summary>
        FullScaleRange = 1,

        /// <summary>
        /// The actual value read from the analogue input line converted into 2's complement
        /// </summary>
        Hexadecimal = 2
    };

    /// <summary>
    /// Specifies the unit of measurement for temperature readings from Brainboxes
    /// thermocouple and RTD devices (<see cref="ED582"/>, <see cref="ED593"/>).
    /// </summary>
    public enum TemperatureUnit
    {
        /// <summary>
        /// Undefined if temperature unit is not retrieved from the device
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Temperature unit in Celsius
        /// </summary>
        Celsius = 0,

        /// <summary>
        /// Temperature unit in Fahrenheit
        /// </summary>
        Fahrenheit = 1,

        /// <summary>
        /// Temperature unit in Kelvin
        /// </summary>
        Kelvin = 2
    };

    /// <summary>
    /// Specifies the bit width of the digital input counter on Brainboxes ED-series devices.
    /// </summary>
    public enum CounterMode
    {
        /// <summary>
        /// Undefined if the counter mode is not retrieved from the device
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// 16 bits to represent the digital IO line counter
        /// </summary>
        CounterMode16Bits = 0,

        /// <summary>
        /// 32 bits to represent the digital IO line counter
        /// </summary>
        CounterMode32Bits = 1
    };

    /// <summary>
    /// Implementation of <see cref="IIOProtocol"/> using the Brainboxes ASCII/DCON command protocol.
    /// This is the default protocol for ED-series devices on TCP port 9500.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Commands are human-readable ASCII strings terminated with a carriage return (CR).
    /// Response prefixes indicate status: <c>&gt;</c> = valid response, <c>?</c> = invalid command,
    /// <c>!</c> = successful write.
    /// </para>
    /// <para>
    /// For the full command reference see
    /// <a href="http://www.brainboxes.com/faq/items/ascii-protocol-and-commands">ASCII Protocol Commands</a>.
    /// </para>
    /// <para>
    /// The protocol is selected automatically when using <see cref="TCPConnection"/> on
    /// port 9500 (the default). To use Modbus TCP instead, connect on port 502.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// var connection = new TCPConnection("192.168.0.100", 9500);
    /// var device = new ED588(connection);
    /// device.Connect();
    ///
    /// // Read all digital line states
    /// int states = device.Protocol.GetAllDigitalLineStates();
    ///
    /// // Send a raw ASCII command
    /// string response = device.SendCommand("$01M");
    ///
    /// device.Disconnect();
    /// </code>
    /// </example>
    public class ASCIIProtocol : Protocol, IIOProtocol
    {
        /// <summary>
        /// The device type string cached from a device name query.
        /// </summary>
        protected string deviceType;
        /// <summary>
        /// Create a new ASCII Protocol instance.
        /// </summary>
        /// <param name="address">The ASCII/DCON address of the ED device (0-255). Defaults to 01.</param>
        public ASCIIProtocol(int address = 01)
        {
            this._address = address;           
        }

        /// <summary>
        /// The ASCII address of the device
        /// </summary>
        protected int _address = 01;
        /// <summary>
        /// default ASCII DCON device address can be set in constructor to specific value
        /// </summary>
        public int Address { get { return _address; } }

        /// <summary>
        /// The baud rate of the device
        /// </summary>
        protected int _baudRate = -1;

        /// <summary>
        /// The baud rate of the device
        /// </summary>
        public int BaudRate
        { get
            {
                if (_baudRate == -1)
                {
                    this.GetDeviceConfiguration();
                }
                return _baudRate;
            }

            internal set { _baudRate = value; }
        }

        /// <summary>
        /// The keys are the available baud rates.
        /// The values are the respective Hex code used to set the baud rate using ASCII commands.
        /// </summary>
        internal readonly static Dictionary<int, string> BAUD_RATE_DICT = new Dictionary<int, string>()
        {
            { 1200, "03" },
            { 2400, "04" },
            { 4800, "05" },
            { 9600, "06" },
            { 19200, "07" },
            { 38400, "08" },
            { 57600, "09" },
            { 115200, "0A" }
        };

        /// <summary>
        /// The checksum setting of the device
        /// </summary>
        protected bool _checksum = false;

        /// <summary>
        /// The filter setting of the analog device
        /// </summary>
        protected bool _analogFilterSetting = false;

        /// <summary>
        /// The module setting of the analog device
        /// </summary>
        protected bool _analogModuleSetting = false;

        /// <summary>
		/// The ASCII data format of the analog device.        
		/// </summary>    
		protected AnalogDataFormat _analogDataFormat = AnalogDataFormat.Undefined;

        /// <summary>
        /// The temperature unit of analog temperature measurement device (ED-582, ED-593)      
        /// Defaults to Celsius format
        /// </summary>    
        protected TemperatureUnit _temperatureUnit = TemperatureUnit.Undefined;

        /// <summary>
        /// The data format of the analog device
        /// Defaults to Engineering format
        /// </summary>
        public AnalogDataFormat DataFormat
        {
            get
            {
                if (_analogDataFormat == AnalogDataFormat.Undefined)
                {
                    this.GetDeviceConfiguration();
                }
                return _analogDataFormat;
            }
            internal set { _analogDataFormat = value; }
        }

        /// <summary>
        /// The unit of temperature of the analog device        
        /// </summary>
        public TemperatureUnit TemperatureUnit
        {
            get
            {
                if (_temperatureUnit == TemperatureUnit.Undefined)
                {
                    this.GetDeviceConfiguration();
                }
                return _temperatureUnit;
            }
            internal set { _temperatureUnit = value; }
        }

        /// <summary>
        /// The counter mode setting of the digital device
        /// </summary>
        protected CounterMode _ioCounterMode = CounterMode.Undefined;
        /// <summary>
        /// The counter mode of the digital device, whether it is a 16-bit or 32-bit counter
        /// </summary>
        public CounterMode IOCounterMode
        {
            get
            {
                if (_ioCounterMode == CounterMode.Undefined)
                {
                    this.GetDeviceConfiguration();
                }
                return _ioCounterMode;
            }
            internal set { _ioCounterMode = value; }
        }

        /// <summary>
        /// The counter update direction of the digital device
        /// </summary>
        protected IOChangeTypes _ioCounterUpdateDirection = IOChangeTypes.Undefined;
        /// <summary>
        /// The counter update direction of the digital device, whether the Digital Input IO Line counter updates on a rising edge or a falling edge
        /// </summary>
        IOChangeTypes IIOProtocol.IOCounterUpdateDirection
        {
            get
            {
                if (_ioCounterUpdateDirection == IOChangeTypes.Undefined)
                {
                    this.GetDeviceConfiguration();
                }
                return _ioCounterUpdateDirection;
            }
        }

        /// <summary>
        /// Sets the data format setting of the device. 
        /// For more info see http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%25aannttccffanalog.pdf
        /// </summary>
        /// <param name="dataFormat"></param>
        public void SetAnalogDataFormat(AnalogDataFormat dataFormat)
        {                           
            SetDeviceConfiguration(Address, BaudRate, newAnalogDataFormat:dataFormat);           
        }


        /// <summary>
        /// Sets the temperature unit setting of the device. 
        /// For more info see http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%25aannttccffanalog.pdf
        /// </summary>
        /// <param name="temperatureUnit"></param>
        public void SetTemperatureUnit(TemperatureUnit temperatureUnit)
        {
            // data format should be engineering for all temperature units
            SetDeviceConfiguration(Address, BaudRate, AnalogDataFormat.Engineering, newTemperatureUnit: temperatureUnit);
        }


        /// <summary>
        /// Send a command to a Brainboxes Device and optionally receive a response back from the device
        /// </summary>
        /// <param name="command">command to send to device</param>
        /// <returns>response from device, null if there is no response</returns>
        public string SendCommand(string command)
        {
            // Capture stream reference locally to avoid race condition during disconnect
            Stream localStream = stream;
            if (localStream == null) throw new InvalidOperationException("Brainboxes Device not connected");
            // the following code block must be locked to ensure the response on the stream correlates to the command sent
            // and is not a response to another thread calling sendCommand
            BBStream s = localStream as BBStream;
            if (s == null) throw new InvalidOperationException("Brainboxes Device not connected");
            lock (s.streamWriteLock)
            {
                lock (s.streamReadLock)
                {
                    Debug.WriteLine("TX <== " + command);
                    localStream.Flush();
                    string response = this._sendCommand(command, s);
                    Debug.WriteLine("RX ==> " + response);
                    return response;
                }
            }
        }

        /// <summary>
        /// Set an individual digital output line state open (0) or closed (1)s
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        [Obsolete("SetOutputLineState is deprecated. Replaced by SetDigitalOutputLineState")]
        public void SetOutputLineState(int line, int state)
        {
            this.SetDigitalOutputLineState(line, state);
        }
        /// <summary>
        /// Set an individual digital output line state open (0) or closed (1)s
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        public void SetDigitalOutputLineState(int line, int state)
        {
            Debug.WriteLine("Setting output line " + line + " to state " + state);
            // 2 different functions available depending on if the line is on the top or bottom 8
            // #AAAcDD lower 8 channels
            // #AABcDD upper 8 channels

            // NOTE: The online manual documents this as #AA1cDD, but the device firmware
            // accepts #AAAcDD (lower 8) and #AABcDD (upper 8) for setting individual lines.
            // Both formats have been verified to work with Brainboxes hardware.
            string commandType = (line < 8) ? "A" : "B";
            string command = string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1}{2}{3:D2}", this._address, commandType, line % 8, state);
            string response = this.SendCommand(command);
            // Valid Command:     >[CS](CR)
            // Invalid Command:  ?[CS](CR)
            // Ignored Command: ![CS](CR)
            switch (response)
            {
                case ">":
                    return;
                case "?":
                    throw new InvalidOperationException("INVALID command. The ED Device reported that the setOutputLine Command " + command + " was INVALID");
                case "!":
                    throw new InvalidOperationException("IGNORED command. The ED Device reported that the setOutputLine Command " + command + " was IGNORED");
            }

        }
        /// <summary>
        /// Set an individual analogue output line value
        /// </summary>
        /// <param name="line"></param>
        /// <param name="value"></param>
        public void SetAnalogOutputLineState(int line, double value)
        {
            Debug.WriteLine("Setting output line " + line + " to state " + value);
            // Command #AAn(Data) 
            // specify invariant culture so that on all OS' the double to string uses . to separate units from decimal place
            // bug found by French customer: Gregory.Daminet@materianova.be, who was running this command on an OS set with French-Belgium culture
            string command = string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1}{2}", this._address, line, value);
            string response = this.SendCommand(command);
            switch (response)
            {
                case ">":
                    return;
                case "?":
                    throw new InvalidOperationException("INVALID command. The ED Device reported the setAnalogOutputLine Command " + command + " was INVALID");
            }
        }

        /// <summary>
        /// Set an individual analogue output line to a Hexadecimal value
        /// </summary>
        /// <param name="line"></param>
        /// <param name="hexValue"></param>
        public void SetAnalogOutputLineState(int line, string hexValue)
        {
            Debug.WriteLine("Setting output line " + line + " to state " + hexValue);
            // Command #AAn(Data) 
            // specify invariant culture so that on all OS' the double to string uses . to separate units from decimal place
            // bug found by French customer: Gregory.Daminet@materianova.be, who was running this command on an OS set with French-Belgium culture
            string command = string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1}{2}", this._address, line, hexValue);
            string response = this.SendCommand(command);
            switch (response)
            {
                case ">":
                    return;
                case "?":
                    throw new InvalidOperationException("INVALID command. The ED Device reported the setAnalogOutputLine Command " + command + " was INVALID");
            }
        }

        /// <summary>
        /// set all output lines on the device open (0) or closed (1)
        /// The bit position represents the line number
        /// </summary>
        /// <param name="states"></param>
        /// <param name="numberOfOutputs"></param>
        [Obsolete("SetAllOutputLineStates is deprecated. Replaced by SetAllDigitalOutputLineStates")]
        public void SetAllOutputLineStates(int states, int numberOfOutputs = 8)
        {
            this.SetAllDigitalOutputLineStates(states, numberOfOutputs);
        }
        /// <summary>
        /// set all output lines on the device open (0) or closed (1)
        /// The bit position represents the line number
        /// </summary>
        /// <param name="states"></param>
        /// <param name="numberOfOutputs">The number of outputs on the device</param>
        public void SetAllDigitalOutputLineStates(int states, int numberOfOutputs = 8)
        {
            //less than 8 outputs uses 2 bytes, less than 16 4 bytes of hex etc.
            int byteAlign = numberOfOutputs <= 8 ? 2 : numberOfOutputs <= 16 ? 4 : 6;


            //8.24	@AA(Data)
            Debug.WriteLine("Setting all output lines to  " + string.Format(CultureInfo.InvariantCulture, "{0:X" + byteAlign + "}", states));

            string command = string.Format(CultureInfo.InvariantCulture, "@{0:X2}{1:X" + byteAlign + "}", this._address, states);
            string response = this.SendCommand(command);

            switch (response)
            {
                case ">":
                    return;
                case "?":
                    throw new InvalidOperationException("INVALID command. The ED Device reported that the SetAllOutputLineStates Command " + command + " was INVALID");
                case "!":
                    throw new InvalidOperationException("IGNORED command. The ED Device reported that the SetAllOutputLineStates Command " + command + " was IGNORED");
            }
        }
        /// <summary>
        /// Get the state of a digital IO line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInput"></param>
        /// <returns></returns>
        [Obsolete("GetLineState is deprecated. Replaced by GetDigitalLineState")]
        public int GetLineState(int line, bool isInput = true)
        {
            return this.GetDigitalLineState(line, isInput);
        }
        /// <summary>
        /// Get the state of a digital IO Line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInput">ignored by ASCII protocol</param>
        /// <returns>1: Line closed/High/On, 0: Line Open/Low/Off</returns>
        public int GetDigitalLineState(int line, bool isInput = true)
        {
            //read all lines
            int response = GetAllDigitalLineStates();

            // ---------------------------------------------------------------
            // | Device	| First 2 hexadecimal digits	| Second 2 hexadecimal digits
            // ---------------------------------------------------------------
            // | ED-588	| DOut 7 - DOut 0	00-FF	    | DIn 7 – DIn 0	00-FF
            // | ED-516	| DIn 15 – DIn 8    00-FF	    | DIn 7 – DIn 0	00-FF
            // | ED-527	| DOut 15 – DOut 8	00-FF	    | DOut 7 – DIn 0	00-FF
            // ---------------------------------------------------------------

            //the response is of the form DDDD
            //line number is bit position from the right LSB
            response = (response >> line) & 1;

            return response;
        }
        /// <summary>
        /// Get the state of an Analog IO Line. If the analog line is disabled it will return +00.000
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInput">Whether the analog IO Line is an input</param>
        /// <returns></returns>
        public double GetAnalogLineState(int line, int numberOfLines = 8, bool isInput = true)
        {
            double returnValue;
            if (isInput)
            {
                double[] response = GetAllAnalogInputLineStates(numberOfLines);
                returnValue = response[line];
                //The response is in the form of a double from #AA command so this will not work for outputs
            }
            else
            {
                //using command $AA6n for analog output devices
                string response = this.SendCommand(string.Format(CultureInfo.InvariantCulture, "${0:X2}6" + line, this._address));
                if (string.IsNullOrEmpty(response) || response[0] != '!')
                {
                    throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
                }
                response = response.Remove(0, 3); //remove leading !AA
                if (this._analogDataFormat == AnalogDataFormat.Undefined)
                {
                    this.GetDeviceConfiguration();
                }
                switch (this._analogDataFormat)
                {
                    case (AnalogDataFormat.Engineering):
                    case (AnalogDataFormat.FullScaleRange):
                        returnValue = double.Parse(response, CultureInfo.InvariantCulture);
                        break;
                    case (AnalogDataFormat.Hexadecimal):
                        // converting hex to double to match the return type of this method.
                        // Might need to create a new method (similar to GetAllAnalogInputLineStates) for Hexadecimal format
                        returnValue = (double)Int32.Parse(response, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new InvalidDataException("Invalid data format " + this._analogDataFormat);
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Get the state of all the digital IOLines of the device as an integer
        /// </summary>
        /// <returns>Each bit in the integer represents a digital IOLine state</returns>
        [Obsolete("GetAllLineStates is deprecated. Replaced by GetAllDigitalLineStates")]
        public int GetAllLineStates()
        {
            return this.GetAllDigitalLineStates();
        }
        /// <summary>
        /// Get the state of all the digital IOLines of the device as an integer
        /// </summary>
        /// <returns>Each bit in the integer represents a digital IOLine state</returns>
        public int GetAllDigitalLineStates()
        {
            Debug.WriteLine("Getting state of all lines");

            // from manual @AA : Read the status of the digital I/O lines.
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "@{0:X2}", this._address));
            // Valid Command: >DDDD[CS](CR)
            // Invalid Command: ?AA[CS](CR)
            if (string.IsNullOrEmpty(response) || response[0] != '>')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \">\", actual: " + response);
            }
            response = response.Substring(1); // remove leading ">"
            return int.Parse(response, System.Globalization.NumberStyles.HexNumber);
        }
        /// <summary>
        /// Gets the state of all the Analog Input lines of the device as double[lineNumber] 
        /// </summary>
        /// <returns></returns>
        public double[] GetAllAnalogInputLineStates(int numberOfInputs = 8)
        {
            Debug.WriteLine("Getting state of all analog lines");
            //from manual #aa (this is only for ED-549, need to limit it to Input devices only
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "#{0:X2}", this._address));
            string linesEnabledResponse = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}6", this._address));

            //Valid command: >+DD:DDD-
            if (string.IsNullOrEmpty(response) || response[0] != '>')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \">\" for command #AA, actual: " + response);
            }

            if (string.IsNullOrEmpty(linesEnabledResponse) || linesEnabledResponse[0] != '!')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \"!\" for command $AA6, actual: " + linesEnabledResponse);
            }

            response = response.Remove(0, 1); //remove leading ">"
            linesEnabledResponse = linesEnabledResponse.Remove(0, 3); //remove leading !AA
            Convert.ToInt32(linesEnabledResponse, 16);

            string[] stringArray = new string[numberOfInputs];
            double[] values = new double[numberOfInputs];

            if (this._analogDataFormat == AnalogDataFormat.Undefined)
            {
                this.GetDeviceConfiguration();
            }
            switch (this._analogDataFormat)
            {
                case AnalogDataFormat.Engineering:
                case AnalogDataFormat.FullScaleRange:
                    // Getting the enable state of the lines
                    int intValue = Convert.ToInt32(linesEnabledResponse, 16);
                    string binaryString = Convert.ToString(intValue, 2).PadLeft(linesEnabledResponse.Length * 4, '0');
                    char[] enabled = binaryString.ToCharArray();
                    Array.Reverse(enabled);

                    int c = 0;
                    foreach (string str in Regex.Split(response, @"(?=[+-])"))
                    {
                        if(str.Equals(""))
                        {
                            continue;
                        }
                        stringArray[c] = str;
                        c++;
                    }

                    c = 0;
                    for (int i = 0; i < numberOfInputs; i++)
                    {
                        if (enabled[i] == '1')
                        {
                            values[i] = double.Parse(stringArray[c], CultureInfo.InvariantCulture);
                            c++;
                        }
                        else
                        {
                            values[i] = double.NaN;
                        }
                    }

                    return values;

                case AnalogDataFormat.Hexadecimal:
                    int chunkSize = 4, // size of an hexadecimal line value
                        count = 0; // counter to add elements to stringArray

                    for (int i = 0; i < response.Length; i += chunkSize)
                    {
                        stringArray[count] = response.Substring(i, chunkSize);
                        count++;
                    }
                    for (int i = 0; i < numberOfInputs; i++)
                    {
                        // converting hex to double to match the return type of this method.
                        // Might need to create a new method (similar to GetAllAnalogInputLineStates) for Hexadecimal format
                        values[i] = (double)Int32.Parse(stringArray[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    return values;
                default:
                    throw new InvalidDataException("Invalid data format " + this._analogDataFormat);
            }
        }

        /// <summary>
        /// Gets the state of all the Analog Output lines of the device as double[lineNumber]
        /// </summary>
        /// <returns></returns>
        public double[] GetAllAnalogOutputLineStates(int numberOfOutputs = 4)
        {
            //Not sure how to get number of lines here?
            Debug.WriteLine("Getting state of all analog lines");
            double[] values = new double[numberOfOutputs];
            for (int i = 0; i < numberOfOutputs; i++)
            {
                values[i] = this.GetAnalogLineState(i, numberOfOutputs, false);
            }
            return values;
        }
        /// <summary>
        /// Test whether the command does not have an ASCII response from the ED Device
        /// </summary>
        /// <param name="command"></param>
        /// <returns>true if not response</returns>
        public bool CommandDoesNotHaveResponse(string command)
        {
            foreach (Regex test in this._commandsWithoutResponse)
            {
                if (test.Match(command).Success) return true;
            }
            return false;
            //return Array.IndexOf(this._commandsWithoutResponse, command) > -1;
        }

        /// <summary>
        /// ASCII character encoding is used to send and receive data to/from the EDDevice
        /// Windows-28591 = ISO-8859-1 = ASCII
        /// </summary>
        protected readonly Encoding _commsEncoding = System.Text.Encoding.GetEncoding(28591);

        /// <summary>
        /// New line character used by the ASCII protocol to signal end of message
        /// </summary>
        protected readonly string _newLine = "\r";
        /// <summary>
        /// New line character code 13
        /// </summary>
        protected readonly int _newLineCode = 13;

        /// <summary>
        /// commands which have no response
        /// #** Synchronized Sampling Command
        /// ~** Host is OK Command
        /// $AARS Restart the device to power on setting
        /// SendCommand returns null for these commands
        /// </summary>
        protected readonly Regex[] _commandsWithoutResponse = { new Regex(@"#\*\*"), new Regex(@"~\*\*"), new Regex(@"\$[0-9A-F][0-9A-F]RS") };

        /// <summary>
        /// Any command sent to the ED Device is sent by this function
        /// </summary>
        /// <param name="command">The ASCII command</param>
        /// <param name="s">The stream to send the command on (captured locally to avoid race conditions)</param>
        /// <returns>response from the ED device</returns>
        protected string _sendCommand(string command, BBStream s)
        {
            Byte[] sendBuffer = this._commsEncoding.GetBytes(command + this._newLine);
            // if there is a write timeout let it be thrown up to the caller
            s.Write(sendBuffer, 0, sendBuffer.Length);
            if (this.CommandDoesNotHaveResponse(command))
            {
                return null;
            }

            byte[] receiveBuffer = new byte[256]; //all ASCII DCON commands are much shorter than this: NB discovered analog commands can be longer than 64 bytes

            int timeout = s.ReadTimeout;
            Stopwatch sw;
            bool success = false;
            int readCount = 0;

            sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeout)
            {
                while (s.DataAvailable > 0)
                {
                    int bytesToRead = Math.Min(s.DataAvailable, receiveBuffer.Length - readCount);
                    if (bytesToRead <= 0) break;
                    readCount += s.Read(receiveBuffer, readCount, bytesToRead);
                    if (receiveBuffer[readCount - 1] == _newLineCode)
                    {
                        success = true;
                    }
                }
                if (success) break;
            }
            sw.Stop();
            if (success)
            {
                //decode and remove the newline character
                string response = this._commsEncoding.GetString(receiveBuffer, 0, readCount).Replace(this._newLine, "");
                //TODO: validate and remove checksum if present
                return response;
            }
            throw new TimeoutException("The operation has timed out after " + sw.ElapsedMilliseconds + "ms, no response received from the Brainboxes Device, was the command valid? Is the device still connected?");

        }

        string _deviceName = null;

        /// <summary>
        /// The name of the ED Device
        /// </summary>
        public string DeviceName
        {
            get
            {
                if (_deviceName == null)
                {
                    _deviceName = GetDeviceName();
                }
                return _deviceName;
            }
            set
            {
                string command = String.Format(CultureInfo.InvariantCulture, "~{0:X2}O{1}", this._address, value);
                string response = this.SendCommand(command);
                if (!string.IsNullOrEmpty(response) && response[0] == '!') //success
                {
                    _deviceName = value;
                }
                else
                {
                    throw new InvalidOperationException("Invalid command: \"" + command + "\" response: \"" + response + "\"");
                }
            }
        }

        /// <summary>
        /// Get the name of the Device
        /// </summary>
        /// <returns></returns>
        public string GetDeviceName()
        {
            string command = String.Format(CultureInfo.InvariantCulture, "${0:X2}M", this._address);
            string response = this.SendCommand(command);
            if (string.IsNullOrEmpty(response) || response.Length < 3 || response[0] != '!')
            {
                throw new InvalidOperationException("Attempted to get device name with command: \"" + command + "\" invalid response: \"" + response + "\"");
            }
            return response.Substring(3);
        }

        /// <summary>
        /// Reset the ED device to factory default settings
        /// </summary>
        public void ResetToFactoryDefaultSettings()
        {
            Debug.WriteLine("Resetting Device, will also restart device Please disconnect and reconnect connection");

            string command = String.Format(CultureInfo.InvariantCulture, "${0:X2}S1", this._address);
            string response = this.SendCommand(command);
            if (string.IsNullOrEmpty(response) || response[0] != '!')
            {
                throw new InvalidOperationException("Reset Failed command: \"" + command + "\" response: \"" + response + "\"");
            }
            //success now reset any cached state
            this._resetCache();
        }

        /// <summary>
        /// Power Off and the On the ED Device
        /// </summary>
        public void Restart()
        {
            Debug.WriteLine("Restarting Device Please disconnect and reconnect connection");
            string command = String.Format(CultureInfo.InvariantCulture, "${0:X2}RS", this._address);
            //no response
            this.SendCommand(command);
            this._resetCache();
        }

        private void _resetCache()
        {
            this._deviceName = null;
            this._address = 01;
            this._ioCounterUpdateDirection = IOChangeTypes.Undefined;
        }
        /// <summary>
        /// Get the HIGH LATCH states of all the digital inputs
        /// </summary>
        /// <returns></returns>
        [Obsolete("GetAllLatchedHighInputStates is deprecated. Replaced by GetAllLatchedHighDigitalInputStates")]
        public int GetAllLatchedHighInputStates()
        {
            return this.GetAllLatchedHighDigitalInputStates();
        }
        /// <summary>
        /// Get the HIGH LATCH states of all the digital Inputs
        /// </summary>
        /// <returns></returns>
        public int GetAllLatchedHighDigitalInputStates()
        {
            return GetAllLatchedInputStates();
        }
        /// <summary>
        /// Get the LOW LATCH state of all the digital INPUTS
        /// </summary>
        /// <returns></returns>
        [Obsolete("GetAllLatchedLowInputStates is deprecated. Replaced by GetAllLatchedLowDigitalInputStates")]
        public int GetAllLatchedLowInputStates()
        {
            return this.GetAllLatchedLowDigitalInputStates();
        }
        /// <summary>
        /// Get the LOW LATCH state of all the digital INPUTS
        /// </summary>
        /// <returns></returns>
        public int GetAllLatchedLowDigitalInputStates()
        {
            return GetAllLatchedInputStates(false);
        }

        /// <summary>
        /// Get the LATCH state of the INPUTS
        /// </summary>
        /// <param name="isHigh">Whether to get the HIGH or LOW LATCH</param>
        /// <returns></returns>
        protected int GetAllLatchedInputStates(bool isHigh = true)
        {
            //from manual @AALS : Reads the status of the latched digital input channels.
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}L{1:D}", this._address, (isHigh ? 1 : 0)));
            //Valid Command: !DDDD00[CS](CR
            //Invalid Command: ?AA[CS](CR)
            if (string.IsNullOrEmpty(response) || response[0] != '!')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
            }
            response = response.Substring(1, 4); // remove leading ">"
            return int.Parse(response, System.Globalization.NumberStyles.HexNumber);
        }
        /// <summary>
        /// Clear the digital INPUT latches
        /// </summary>
        [Obsolete("ClearAllLatchedInputs is deprecated. Replaced by ClearAllLatchedDigitalInputs")]
        public void ClearAllLatchedInputs()
        {
            this.ClearAllLatchedDigitalInputs();
        }
        /// <summary>
        /// Clear the INPUT latches
        /// </summary>
        public void ClearAllLatchedDigitalInputs()
        {
            //from manual $AAC : Clears the status of the latched digital input channels..
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}C", this._address));
            //Valid Command:   !AA[CS]
            //Invalid Command: ?AA[CS]
            if (string.IsNullOrEmpty(response) || response[0] != '!')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
            }
        }
        /// <summary>
        /// Reads the digital input counter of the specified channel
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        [Obsolete("GetLineCount is deprecated. Replaced by GetDigitalInputLineCount")]
        public int GetLineCount(int line)
        {
            return this.GetDigitalInputLineCount(line);
        }
        /// <summary>
        /// Reads the digital input counter of the specified channel
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetDigitalInputLineCount(int line)
        {
            //from manual #AAN
            //#	Delimiter character
            //AA	Address of the device to be configured in hexadecimal format (00 to FF)to FF)
            //N	Digital input channel to be read (0 to F)
            string request = String.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X1}", this._address, line);
            string response = this.SendCommand(request);
            //Valid Response:   !AAData[CS]
            //Invalid Response: ?AA[CS]
            if (string.IsNullOrEmpty(response) || response[0] != '!')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
            }
            response = response.Substring(3); // remove leading "!AA"

            int countValue;

            if (Int32.TryParse(response, out countValue))
            {
                return countValue; //note the number is already in base 10, not hex
            }

            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid response to the count command for Input line {0}. Response did not contain a valid integer. \nRequest: {1}\nResponse: {2} ", line, request, response));

        }
        /// <summary>
        /// Clears the digital input counter of the specified line.
        /// </summary>
        /// <param name="line"></param>
        [Obsolete("ClearLineCount is deprecated. Replaced by ClearDigitalInputLineCount")]
        public void ClearLineCount(int line)
        {
            this.ClearDigitalInputLineCount(line);
        }

        /// <summary>
        /// Clears the digital input counter of the specified line.
        /// </summary>
        /// <param name="line"></param>
        public void ClearDigitalInputLineCount(int line)
        {
            //from manual $AACN : 
            //$	Delimiter character
            //AA	Address of the device to be configured in hexadecimal format (00 to FF)
            //C	Command to clear digital input counter
            //N	Channel to be cleared (0 to F)
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}C{1:X1}", this._address, line));
            //Valid Command:   !AA[CS]
            //Invalid Command: ?AA[CS]
            if (string.IsNullOrEmpty(response) || response[0] != '!')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
            }
        }

        /// <summary>
        /// Read the device configuration
        /// </summary>
        public void GetDeviceConfiguration()
        {
            // from the manual: $AA2[CS](CR)
            // Command $aa2 - http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%24AA2.pdf
            // $	Delimiter character
            // AA	Address of the device to be configured in hexadecimal format(00 to FF)
            // 2	Command to read the device configuration
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}2", this._address));
            //Valid Command:   !AA[CS]
            //Invalid Command: ?AA[CS]
            if (string.IsNullOrEmpty(response) || response[0] != '!')
            {
                throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
            }
            // Response: Valid Command: !AATTCCFF[CS](CR)
            // 
            // !	Delimiter for a valid command
            // AA	Address of the device (00 to FF)
            // TT	Device Type (40 for DIO Devices)
            // CC	Baud Rate of the device
            // Code:	03	04	05	06	07	08	09	0A
            // Baud Rate:	1200	2400	4800	9600	19200	38400	57600	1115200
            // FF	Data format setting
            // 7	6	5	4	3	2	1	0
            // CU	CS	CM	Reserved
            // CS: Checksum Setting
            // 0: Disabled
            // 1: Enabled

            // CU: Counter Update
            // 0: Falling edge in the input signal
            // 1: Rising edge in the input signal.

            // CM: Input Counter Mode
            // 0: 16bit counter
            // 1: 32bit counter

            this._address = Convert.ToInt32(response.Substring(1, 2), 16);

            //  TT - Device type
            //  DIO devices (TT) - 40
            //            Analog devices (TT):
            //  ========================================
            //  || ED-549  ||   ED-560   ||   ED-582  ||
            //  ========================================
            //  ||   08    ||     30     ||     80    ||
            //  ========================================

            this.deviceType = response.Substring(3,2);
            string tmpBaudRate = response.Substring(5, 2);

            foreach (KeyValuePair<int, string> kv in BAUD_RATE_DICT)
            {
                if (kv.Value == tmpBaudRate)
                {
                    _baudRate = kv.Key;
                    break;
                }
            }         

            string dataFormat = response.Substring(7, 2);
            int dataFormatSetting = Convert.ToInt32(dataFormat, 16);
            this._checksum = ((dataFormatSetting >> 6 )& 0x1) == 1;

            if (deviceType == "40") 
            {
                // DIO device
                this._ioCounterUpdateDirection = (dataFormatSetting >> 7 == 1) ? IOChangeTypes.RisingEdge : IOChangeTypes.FallingEdge;
                this._ioCounterMode = ((dataFormatSetting >> 5) & 0x1) == 1 ? CounterMode.CounterMode32Bits: CounterMode.CounterMode16Bits;                
            }
            else if (deviceType == "08" || deviceType == "30" || deviceType == "80")
            {
                // Analog device
                this._analogFilterSetting = ((dataFormatSetting >> 7) & 0x1) == 1;
                this._analogModuleSetting = ((dataFormatSetting >> 5) & 0x1) == 1;
                this._analogDataFormat = (AnalogDataFormat)(dataFormatSetting & 0x03);
                this._temperatureUnit =  deviceType == "80" ? (TemperatureUnit)((dataFormatSetting >> 2) & 0x03) : 0x00;
            }
            else
            {
                throw new InvalidDataException("Device type " + deviceType + " is unsupported");
            }
        }

        public void SetDeviceConfiguration(int newAddress, 
                                            int newBaudRate,                                             
                                            IOChangeTypes newIOCounterUpdateDirection,
                                            CounterMode newCounterMode, 
                                            bool newChecksum = false)
        {
            GetDeviceConfiguration(); // to get the deviceType and to make sure the device settings are up to date

            int ff = 0x00;          

            ff |= (newIOCounterUpdateDirection == IOChangeTypes.RisingEdge) ? 0x80 : 0x00;
            ff |= newChecksum  ? 0x40 : 0x00;
            ff |= (newCounterMode > 0) ? 0x20 : 0x00;

            // Command %aannttccff - http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%25AANNTTCCFF.pdf
            string command = string.Format("%{0:X2}{1:X2}{2}{3}{4:X2}", this._address, newAddress, deviceType, BAUD_RATE_DICT[newBaudRate], ff);
            string response = this.SendCommand(command);

            // Valid Command:   !01[CS](CR)
            // Invalid Command: ?01[CS](CR)
            if (response.StartsWith("!"))
            {
                this._address = newAddress;
                this._baudRate = newBaudRate;
                this._ioCounterUpdateDirection = newIOCounterUpdateDirection;
                this._ioCounterMode = newCounterMode;               
                this._checksum = newChecksum;
                return;
            }
            else
            {
                throw new InvalidOperationException("INVALID command. The ED Device reported that the SetDataFormat Command " + command + " was INVALID");
            }
        }

        public void SetDeviceConfiguration(int newAddress,
                                           int newBaudRate,
                                           AnalogDataFormat newAnalogDataFormat,                                           
                                           bool newFilterSettings = false,
                                           bool newModuleSettings = false,
                                           bool newChecksum = false,
                                           TemperatureUnit newTemperatureUnit = TemperatureUnit.Celsius)
        {
            GetDeviceConfiguration(); // to get the deviceType and to make sure the device settings are up to date
            int ff = 0x00; // Engineering, Celsius

            if (newAnalogDataFormat == AnalogDataFormat.FullScaleRange)
            {
                ff |= 0x01;
            }
            else if(newAnalogDataFormat == AnalogDataFormat.Hexadecimal)
            {
                ff |= 0x02;
            }

            if (newTemperatureUnit == TemperatureUnit.Fahrenheit)
            {
                ff |= 0x04;
            }
            else if (newTemperatureUnit == TemperatureUnit.Kelvin)
            {
                ff |= 0x08;
            }

            ff |= (newFilterSettings) ? 0x80 : 0x00;            
            ff |= (newChecksum)       ? 0x40 : 0x00;
            ff |= (newModuleSettings) ? 0x20 : 0x00;

            // Command %aannttccff -  http://www.brainboxes.com/files/pages/support/faqs/docs/AsciiCommands/%25aannttccffanalog.pdf
            string command = string.Format("%{0:X2}{1:X2}{2}{3}{4:X2}", this._address, newAddress, deviceType, BAUD_RATE_DICT[newBaudRate], ff);
            string response = this.SendCommand(command);

            // Valid Command:   !01[CS](CR)
            // Invalid Command: ?01[CS](CR)
            if (response.StartsWith("!"))
            {
                this._address = newAddress;
                this._baudRate = newBaudRate;
                this._analogDataFormat = newAnalogDataFormat;
                this._analogFilterSetting = newFilterSettings;
                this._analogModuleSetting = newModuleSettings;
                this._checksum = newChecksum;
                this._temperatureUnit = newTemperatureUnit;
                return;
            }
            else
            {
                throw new InvalidOperationException("INVALID command. The ED Device reported that the SetDataFormat Command " + command + " was INVALID");
            }
          
        }
    }
}
