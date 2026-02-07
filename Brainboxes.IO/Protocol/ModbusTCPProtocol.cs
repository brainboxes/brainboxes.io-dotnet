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
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Brainboxes.IO
{
    /// <summary>
    /// Implementation of <see cref="IIOProtocol"/> using the Modbus TCP binary protocol.
    /// Used for ED-series devices on TCP port 502.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modbus TCP uses binary byte arrays with a 7-byte MBAP (Modbus Application Protocol)
    /// header for framing. Each request/response is identified by a transaction ID for
    /// matching in concurrent environments.
    /// </para>
    /// <para>
    /// The protocol is selected automatically when using <see cref="TCPConnection"/> on
    /// port 502 (<see cref="TCPConnection.DEFAULT_MODBUSTCP_PORT"/>).
    /// </para>
    /// <para>
    /// Some operations are not supported via Modbus and will throw
    /// <see cref="NotImplementedException"/>: <c>DeviceName</c>, <c>GetDeviceName</c>,
    /// <c>GetDeviceConfiguration</c>, <c>SetDeviceConfiguration</c>,
    /// <c>ResetToFactoryDefaultSettings</c>, and <c>Restart</c>.
    /// Use <see cref="ASCIIProtocol"/> (port 9500) for these operations.
    /// </para>
    /// <para>
    /// For the Modbus command reference see
    /// <a href="http://www.brainboxes.com/modbus">Brainboxes Modbus Documentation</a>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// var connection = new TCPConnection("192.168.0.100", TCPConnection.DEFAULT_MODBUSTCP_PORT);
    /// var device = new ED588(connection);
    /// device.Connect();
    ///
    /// // Read all digital line states via Modbus
    /// int states = device.Protocol.GetAllDigitalLineStates();
    ///
    /// device.Disconnect();
    /// </code>
    /// </example>
    public class ModbusTCPProtocol : Protocol, IIOProtocol
    {
        /// <summary>
        /// Create a new Modbus TCP Protocol instance.
        /// </summary>
        public ModbusTCPProtocol()
        {

        }

        /// <summary>
        /// The last Modbus TCP Request sent
        /// encoded as string
        /// </summary>
        public string LastRequest
        {
            get
            {
                return GetRequestForTransactionId(_transactionId);
            }
        }

        /// <summary>
        /// The  Modbus TCP Response for the last Request sent
        /// note this could be null as the request may not have completed
        /// at time of asking
        /// </summary>
        public string LastResponse
        {
            get
            {
                return GetResponseForTransactionId(_transactionId);
            }
        }

        /// <summary>
        /// Get Modbus TCP Request for a particular TransactionId
        /// </summary>
        /// <param name="transId"></param>
        /// <returns></returns>
        public string GetRequestForTransactionId(int transId )
        {
            return PrettyPrintModbusADU(_requestHistory[transId % 64]);
        }

        /// <summary>
        /// Get Modbus TCP response for particular TransactionId
        /// </summary>
        /// <param name="transId"></param>
        /// <returns></returns>
        public string GetResponseForTransactionId(int transId)
        {
            return PrettyPrintModbusADU(_responseHistory[transId % 64]);
        }

        /// <summary>
        /// The last 64 requests sent to the device
        /// indexed by transactionId % 64
        /// </summary>
        protected byte[][] _requestHistory = new byte[64][];
        /// <summary>
        /// The responses which match the last 64 requests sent to the device
        /// </summary>
        protected byte[][] _responseHistory = new byte[64][];

        /// <summary>
        /// The ModbusTCP transaction Id starts at one and should be incremented each time
        /// from Modbus.org: Transaction Identifier - 
        /// It is used for transaction pairing, the MODBUS server copies
        /// in the response the transaction identifier of the request.
        /// </summary>
        public int TransactionId { get { return _transactionId; } }

        /// <summary>
        /// The ModbusTCP protocol, always 0
        /// From Modbus.org: Protocol Identifier – It is used for intra-system multiplexing. 
        /// The MODBUS protocol is identified by the value 0. 
        /// </summary>
        public int ProtocolId { get { return _protocolId; } }

        /// <summary>
        /// The ModbusTCP Unit identifier, 
        /// From Modbus.org: Unit Identifier – 
        /// This field is used for intra-system routing purpose. It is typically
        /// used to communicate to a MODBUS+ or a MODBUS serial line slave through a
        /// gateway between an Ethernet TCP-IP network and a MODBUS serial line.This field is
        /// set by the MODBUS Client in the request and must be returned with the same value in
        /// the response by the server.
        /// For our purposes: always 0xff
        /// </summary>
        public int UnitId { get { return _unitId; } }

        /// <summary>
        /// The Modbus TCP Length of Request
        /// From Modbus.rog: Length - 
        /// The length field is a byte count of the following fields, including the Unit
        /// Identifier and data fields.
        /// </summary>
        public int RequestLength { get { return _reqLength; } }


        /// <summary>
        /// The Modbus TCP Length of Response
        /// From Modbus.rog: Length - 
        /// The length field is a byte count of the following fields, including the Unit
        /// Identifier and data fields.
        /// </summary>
        public int ResponseLength { get { return _resLength; } }

        /// <summary>
        /// The ModbusTCP transaction Id starts at one and should be incremented each time
        /// from Modbus.org: Transaction Identifier - 
        /// It is used for transaction pairing, the MODBUS server copies
        /// in the response the transaction identifier of the request.
        /// </summary>
        protected int _transactionId = 0;

        /// <summary>
        /// The ModbusTCP protocol, always 0
        /// From Modbus.org: Protocol Identifier – It is used for intra-system multiplexing. 
        /// The MODBUS protocol is identified by the value 0. 
        /// </summary>
        readonly protected int _protocolId = 0;

        /// <summary>
        /// The ModbusTCP Unit identifier, 
        /// From Modbus.org: Unit Identifier – This field is used for intra-system routing purpose. It is typically
        /// used to communicate to a MODBUS+ or a MODBUS serial line slave through a
        /// gateway between an Ethernet TCP-IP network and a MODBUS serial line.This field is
        /// set by the MODBUS Client in the request and must be returned with the same value in
        /// the response by the server.
        /// For our purposes: always 0xff
        /// </summary>
        readonly protected int _unitId = 0xff;

        /// <summary>
        /// The Modbus TCP Length of Request
        /// From Modbus.rog: Length - 
        /// The length field is a byte count of the following fields, including the Unit
        /// Identifier and data fields.
        /// </summary>
        protected int _reqLength = 0;

        /// <summary>
        /// The Modbus TCP Length of Response
        /// From Modbus.rog: Length - 
        /// The length field is a byte count of the following fields, including the Unit
        /// Identifier and data fields.
        /// </summary>
        protected int _resLength = 0;

        /// <summary>
        /// Take a Modbus Command written in ASCII as a hex representation and
        /// prettifies it. e.g. 
        /// 006600000006ff0100000010
        /// 0066 0000 0006 ff 01 0000 0010
        /// TranID|ProtID|Len|UnitID|Func|DATA
        /// there is a space every 4 characters, and also a space at the 14 char
        /// between the UnitID and the Function code
        /// </summary>
        /// <param name="adu">Application Data Unit as defined by ModbusTCP spec, encoded in ASCII as hex</param>
        /// <returns></returns>
        public string PrettyPrintModbusADU(string adu)
        {
            if (adu.Length == 0) return "";
            //-- pretty print command in console --//
            StringBuilder formattedCommandBuilder = new StringBuilder( Convert.ToInt32(1+(adu.Length * 1.25)) );

            formattedCommandBuilder.Append(adu[0]);

            for (int i = 1; i < adu.Length; i++)
            {
                if (i % 4 == 0 || i == 14)
                {
                    formattedCommandBuilder.Append(" ");
                }
                formattedCommandBuilder.Append(adu[i]);
            }

            return formattedCommandBuilder.ToString();
        }

        /// <summary>
        /// Take a Modbus Command written in a hex byte array representation and
        /// prettifies it. e.g. 
        /// 0x006600000006ff0100000010
        /// 0066 0000 0006 ff 01 0000 0010
        /// TranID|ProtID|Len|UnitID|Func|DATA
        /// there is a space every 4 characters, and also a space at the 15 char
        /// between the UnitID and the Function code
        /// </summary>
        /// <param name="adu">Application Data Unit as defined by ModbusTCP spec</param>
        /// <returns></returns>
        public string PrettyPrintModbusADU(byte[] adu)
        {
            //decode
            StringBuilder responseBuilder = new StringBuilder(adu.Length);

            for (int i = 0; i < adu.Length; i++)
            {
                responseBuilder.Append(adu[i].ToString("X2"));
            }
            return PrettyPrintModbusADU( responseBuilder.ToString() );
        }

        /// <summary>
        /// Send a ModbusTCP command (ADU) to a Brainboxes Device and return response
        /// <b>The MBAP header is handled internally by this class, only the function and data need to be sent as a command</b>
        /// Any spacing is ignore, commands are hex encoded ASCII (e.g. "01 0000 0001")
        /// Modbus TCP commands are defined as follows:
        /// ADU - Application Data Unit, split into:
        /// MBAP Header - the header contains 4 fields:
        ///  * transactionId (2 bytes) = id starting at 01 increments by 1 for each new ADU sent
        ///  * protocol Id (2 bytes) = always 0x00 = modbus for brainboxes devices
        ///  * Length (2 bytes) = the total length in bytes of rest of the ADU from that point
        ///  * Unit Id (1 byte) = set by the client in the request the response must contain the same data, always 0xff
        ///  * Function Code (2 bytes)- the type of function requested can be:
        ///     * 0x01 = Read Coils
        ///     * 0x02 = Read Discrete Inputs
        ///     * 0x03 = Read Holding Registers
        ///     * 0x04 = Read Input Registers
        ///     * 0x05 = Write Single Coil
        ///     * 0x06 = Write Single Register
        ///  * Data (variable length) -
        /// The end of the modbusTCP command, which contains data relevant to the function code
        /// The format of the command is hex encoded values in ASCII, 
        ///
        /// The response is of the format:
        ///  * transactionId (2 bytes) = is the same transaction ID the PC used in its request
        ///  * protocol Id (2 bytes) =  is the protocol which is always 00 for Modbus TCP
        ///  * Length (2 bytes): is the number of bytes in the rest of the transaction
        ///  * Unit Id (1 byte): is the Unit Identifier =0xFF
        ///  * Function Code (2 bytes): is the Modbus Function code 01= Read Multiple Coils
        ///  * Byte Code (2 bytes): Is the byte count of data to follow in this case 2 bytes
        ///  * Data: is the value of data that corresponds to the function code
        /// </summary>
        /// <param name="pdu">Protocol Data Unit: Function Code and Data of the Modbus TCP command, spaces and dashes are ignored</param>
        /// <returns>response from the ED device without modbus header MBAP</returns>
        public string SendCommand(string pdu)
		{
			// Capture stream reference locally to avoid race condition during disconnect
			Stream localStream = stream;
			if (localStream == null) throw new InvalidOperationException("Brainboxes Device not connected");

			BBStream s = localStream as BBStream;
			if (s == null) throw new InvalidOperationException("Brainboxes Device not connected");

            //remove unwanted chars
            pdu = Regex.Replace(pdu, @"[\s-]", "");

            //convert from hex string to byte array of hex values
            byte[] pduBytes = new byte[pdu.Length / 2];
            for (int i = 0; i < pduBytes.Length; i++)
            {
                pduBytes[i] = Convert.ToByte(pdu.Substring(i * 2, 2), 16);
            }

            byte[] responseBytes = this.SendCommand(pduBytes);

            //decode
            StringBuilder responseBuilder = new StringBuilder(responseBytes.Length);

            for (int i = 0; i < responseBytes.Length; i++)
            {
                responseBuilder.Append(responseBytes[i].ToString("X2"));
            }
            return responseBuilder.ToString();

        }

        /// <summary>
        /// Send a ModbusTCP command (ADU) to a Brainboxes Device and return response
        /// <b>The MBAP header is handled internally by this class, only the function and data need to be sent as a command</b>
        /// Any spacing is ignore, commands as encoded hex byte array e.g. 0x0100000001
        /// Modbus TCP commands are defined as follows:
        /// ADU: Application Data Unit, split into: MBAP and PDU
        /// MBAP Header - the header contains 4 fields:
        ///  * transactionId (2 bytes) = id starting at 01 increments by 1 for each new ADU sent
        ///  * protocol Id (2 bytes) = always 0x00 = modbus for brainboxes devices
        ///  * Length (2 bytes) = the total length in bytes of rest of the ADU from that point
        ///  * Unit Id (1 byte) = set by the client in the request the response must contain the same data, always 0xff
        /// PDU: Protocol Data Unit, consists of 2 fields:
        /// Function Code (2 bytes)- the type of function requested can be:
        ///  * 0x01 = Read Coils
        ///  * 0x02 = Read Discrete Inputs
        ///  * 0x03 = Read Holding Registers
        ///  * 0x04 = Read Input Registers
        ///  * 0x05 = Write Single Coil
        ///  * 0x06 = Write Single Register
        /// Data (variable length) -
        /// The end of the modbusTCP command, which contains data relevant to the function code
        /// The format of the command is hex encoded values in ASCII, 
        /// </summary>
        /// <param name="pdu">Protocol Data Unit: Function Code and Data of the Modbus TCP command</param>
        /// <returns>response from the ED device without modbus header MBAP</returns>
        public byte[] SendCommand(byte[] pdu)
        {
            // Capture stream reference locally to avoid race condition during disconnect
            Stream localStream = stream;
            if (localStream == null) throw new InvalidOperationException("Brainboxes Device not connected");

            BBStream s = localStream as BBStream;
            if (s == null) throw new InvalidOperationException("Brainboxes Device not connected");

            lock (s.streamWriteLock)
            {
                lock (s.streamReadLock)
                {
                    //--build MBAP Header (big-endian / network byte order) --//
                    int length = _reqLength = pdu.Length + 1;
                    _transactionId++;
                    // Construct MBAP header using explicit byte arithmetic for correct
                    // big-endian encoding regardless of platform endianness
                    byte[] mbap = new byte[7]
                    {
                        (byte)((_transactionId >> 8) & 0xFF),  // Transaction ID high byte
                        (byte)(_transactionId & 0xFF),         // Transaction ID low byte
                        (byte)((_protocolId >> 8) & 0xFF),     // Protocol ID high byte
                        (byte)(_protocolId & 0xFF),            // Protocol ID low byte
                        (byte)((length >> 8) & 0xFF),          // Length high byte
                        (byte)(length & 0xFF),                 // Length low byte
                        (byte)(_unitId & 0xFF),                // Unit ID
                    };
                    //combine MBAP with function and data
                    byte[] adu = new byte[mbap.Length + pdu.Length];
                    Buffer.BlockCopy(mbap, 0, adu, 0, mbap.Length);
                    Buffer.BlockCopy(pdu, 0, adu, mbap.Length, pdu.Length);

                    _responseHistory[_transactionId % 64] = null; //ensure the response is empty
                    _requestHistory[_transactionId % 64] = adu;

                    s.Flush();
                    Debug.WriteLine("TX <== " + PrettyPrintModbusADU(adu));

                    byte[] response = this._sendCommand(adu, s);

                    _responseHistory[_transactionId % 64] = response;
                    Debug.WriteLine("RX ==> " + PrettyPrintModbusADU(response));

                    //remove header from response
                    byte[] responseNoMbap = new byte[response.Length - mbap.Length];
                    Buffer.BlockCopy(response, mbap.Length, responseNoMbap, 0, responseNoMbap.Length);
                    if( ((int)responseNoMbap[0] & 0x80) == 0x80)
                    {
                        throw new InvalidOperationException("SENT: " + PrettyPrintModbusADU(adu) + " RECEIVED INVALID RESPONSE: " + PrettyPrintModbusADU(response));
                    }
                    return responseNoMbap;
                }
            }
        }

        /// <summary>
        /// The update direction of the digital input counter
        /// </summary>
		protected IOChangeTypes _ioCounterUpdateDirection = IOChangeTypes.Undefined;
		/// <summary>
		/// whether the Digital Input IO Line counter updates on a rising edge or a falling edge
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
        /// Set an individual output line state open (0) or closed (1)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        [Obsolete("SetOutputLineState is deprecated. Replaced by SetDigitalOutputLineState")]
        public void SetOutputLineState(int line, int state)
        {
            this.SetDigitalOutputLineState(line, state);
        }
		/// <summary>
		/// Set an individual output line state open (0) or closed (1)
		/// </summary>
		/// <param name="line"></param>
		/// <param name="state"></param>
		public void SetDigitalOutputLineState(int line, int state)
		{
			Debug.WriteLine("Setting output line " + line + " to state " + state);

            // To write a single bit either high or low we use the Write Single Coil Function, code: 0x05
            // The data value is 0x0000 to output the bit low and the data value is 0xFF00 to output the bit high.

            // 05 line 00 00 <-- SET LINE LOW
            // 05 line FF 00 <-- SET LINE HIGH

            byte[] lineBytes = BitConverter.GetBytes(line);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lineBytes);
            }
            byte stateByte = state == 0 ? (byte)0x00 : (byte)0xff;

            byte[] command = new byte[5] { 0x05, lineBytes[1], lineBytes[0], stateByte, 0x00 };

			SendCommand(command);
        }
        /// <summary>
        /// Set an individual analog output line value
        /// </summary>
        /// <param name="line"></param>
        /// <param name="value"></param>
        public void SetAnalogOutputLineState(int line, double value)
        {
            byte[] lineBytes = BitConverter.GetBytes(line);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lineBytes);
            }
            //Using the write single register function code: 0x06
            //Need to * the value by 1000 then convert to hex then byte
            //
            int integer = (int)(value * 1000);
            string hex = integer.ToString("X4");
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            byte[] command = new byte[5] { 0x06, lineBytes[1], lineBytes[0], bytes[0], bytes[1] };
            SendCommand(command);
        }
        /// <summary>
        /// Get an individual analog line value
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInput"></param>
        /// <returns></returns>
        public double GetAnalogLineState(int line, int numberOfLines = 8, bool isInput = true)
        {
            double[] value = isInput ? GetAllAnalogInputLineStates(numberOfLines) : GetAllAnalogOutputLineStates();
            return value[line];
        }
        /// <summary>
        /// Get all analog input values
        /// </summary>
        /// <returns></returns>
        public double[] GetAllAnalogInputLineStates(int numberOfInputs = 8)
        {
            //Read holding registers
            // NOTE: numberOfInputs parameter is currently not used to set the register count.
            // The register count is hardcoded to 8 which matches all current Brainboxes analog input devices.
            byte[] command = new byte[5] { 0x03, 0x00, 0x00, 0x00, 0x08 };
            byte[] response = this.SendCommand(command);
            //Do -1 due to the first byte pair not being relevant to the analog states
            string[] stringBytes = new string[(response.Length / 2) - 1];
            double[] valuesToReturn = new double[(response.Length / 2) - 1];
            //Format the array into byte pairs to get the values for each line
            //Discard the first byte pair
            for (int i = 1; i < (response.Length / 2); i++)
            {
                stringBytes[i - 1] = response[(i * 2)].ToString("X2") + response[(i * 2) + 1].ToString("X2");
            }
            //Convert them into int from hex then /1000 to get float value
            for (int k = 0; k < (valuesToReturn.Length); k++)
            {
                valuesToReturn[k] = (int.Parse(stringBytes[k], System.Globalization.NumberStyles.HexNumber)) / 1000d;
            }

            return valuesToReturn;
        }
        /// <summary>
        /// Get all analog output values
        /// </summary>
        /// <returns></returns>
        public double[] GetAllAnalogOutputLineStates(int numberOfOutputs = 4)
        {
            //Read holding registers
            // NOTE: numberOfOutputs parameter is currently not used to set the register count.
            // The register count is hardcoded to 4 which matches all current Brainboxes analog output devices.
            byte[] command = new byte[5] { 0x03, 0x00, 0x00, 0x00, 0x04 };
            byte[] response = this.SendCommand(command);
            //Do -1 as we not need the first byte pair
            string[] stringBytes = new string[(response.Length / 2) - 1];
            double[] valuesToReturn = new double[(response.Length / 2) - 1];
            //Discard the first byte pair
            for (int i = 1; i < (response.Length / 2); i++)
            {
                stringBytes[i -1] = response[(i * 2)].ToString("X2") + response[(i * 2) + 1].ToString("X2");
            }
            //Format the array into byte pairs to get the values for each line
            //Convert them into int from hex then /1000 to get float value 
            for(int k = 0; k < (valuesToReturn.Length); k++)
            {
                valuesToReturn[k] = (int.Parse(stringBytes[k], System.Globalization.NumberStyles.HexNumber)) / 1000d;
            }
            
            return valuesToReturn;
        }
        /// <summary>
        /// Set all output lines on the device open (0) or closed (1)
        /// The bit position represents the line number
        /// </summary>
        /// <param name="states">The bit position represents the line number</param>
        /// <param name="numberOfOutputs">The number of outputs on the device</param>
        [Obsolete("SetAllOutputLineStates is deprecated. Replaced by SetAllDigitalOutputLineStates")]
        public void SetAllOutputLineStates(int states, int numberOfOutputs = 8)
        {
            this.SetAllDigitalOutputLineStates(states, numberOfOutputs);
        }
        /// <summary>
        /// Set all output lines on the device open (0) or closed (1)
        /// The bit position represents the line number
        /// </summary>
        /// <param name="states">The bit position represents the line number</param>
        /// <param name="numberOfOutputs">The number of outputs on the device</param>
        public void SetAllDigitalOutputLineStates(int states, int numberOfOutputs = 8)
		{
            /*
            from manual:
            We can use Modbus commands to write multiple bits to the ED-527, either high or low using the
            Write Multiple Coil Function, code:
            0x0F.
            The ED - 527 has 16 single bit output registers which addresses start at 0x0000 through to 0x000F.
            We send 0xEE to the lower 8 bits of the ED-527 output and 0xAA to the upper bits of the ED - 527
            outputs.
            To set the 16 bits starting at register 0 the Modbus data packet looks like this:
            */
            //func addr data
            //0F   0000 0010 02 EEAA

            byte[] stateBytes = BitConverter.GetBytes(states);
            byte[] numOutputsBytes = BitConverter.GetBytes(numberOfOutputs);
            int byteCountAsInt = Convert.ToInt32( Math.Ceiling(numberOfOutputs / 8.0) );
            byte[] byteCount = BitConverter.GetBytes(byteCountAsInt);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(stateBytes);
                Array.Reverse(numOutputsBytes);
                Array.Reverse(byteCount);
            }

            byte[] command = new byte[6 + byteCountAsInt];//
            command[0] = 0x0F;
            command[1] = 0x00;
            command[2] = 0x00;
            command[3] = numOutputsBytes[1];
            command[4] = numOutputsBytes[0];
            command[5] = byteCount[0];
            for(int i = 0; i < byteCountAsInt; i++)
            {
                command[i + 6] = stateBytes[i];
            }
            this.SendCommand(command);
        }
        /// <summary>
        /// Get the state of a digital IO Line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInput"></param>
        /// <returns>1: Line closed/High/On, 0: Line Open/Low/Off</returns>
        [Obsolete("GetLineState is deprecated. Replaced by GetDigitalLineState")]
        public int GetLineState(int line, bool isInput = true)
        {
            return this.GetDigitalLineState(line, isInput);
        }
        /// <summary>
        /// Get the state of a digital IO Line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="isInput"></param>
        /// <returns>1: Line closed/High/On, 0: Line Open/Low/Off</returns>
        public int GetDigitalLineState(int line, bool isInput = true)
		{
            //outputs: modbus function 3 Holding registers
            //inputs: modbus function 4 Input registers
            int response = isInput ? GetAllInputStates() : GetAllOutputStates();

			//line number is bit position from the right LSB
			response = (response >> line) & 1 ;

			return response;
		}

        /// <summary>
        /// Get the state of the inputs on the device
        /// </summary>
        /// <returns></returns>
        public int GetAllInputStates()
        {
            // read Input registers, bytes reversed
            // 04 0020 0001
            // read the first 2bytes of the output holding register


            byte[] command = new byte[5] { 0x04, 0x00, 0x20, 0x00, 0x01 };
            byte[] response = this.SendCommand(command);

            if (response.Length < 2)
            {
                throw new InvalidDataException("Invalid response, expected at least 2 bytes but got " + response.Length);
            }
            if (response[0] != 0x04)
            {
                throw new InvalidDataException("Invalid response, expecting first character of ADU to be 0x04, actual: " + response[0]);
            }
            int byteCount = response[1];
            if (byteCount != 2)
            {
                throw new InvalidDataException("Invalid response, expecting byte count to be 2 and instead: " + response[1]);
            }
            if(response.Length == 2) //this device has NO INPUTS
            {
                return 0;
            }
            //the byte order of the response is reversed
            Array.Reverse(response, 2, 2);
            //mask off the top 2 bytes as the return type is int32, but we are only reading int16
            return BitConverter.ToInt16(response, 2) & 0xffff;

        }
        
        /// <summary>
        /// Get all output line states
        /// </summary>
        /// <returns></returns>
        public int GetAllOutputStates()
        {
            // read holding registers, bytes reversed
            // 03 0020 0001
            // read the first 2bytes of the output holding register

            //alternative: To read all 16 ED-527 output bits back use the Read Multiple Coils Function, code: x01, and set the
            //number of coils to be read to 16: 0x10.
            //The ED - 527’s 16 output register addresses start at 0x0001 through to 0x0010.
            //byte[] command = new byte[5] { 0x01, 0x00, 0x00, 0x00, 0x0F };
            // problem with this method, you need to know the number of registers to read in advance
            //or you get an error

            byte[] command = new byte[5] { 0x03, 0x00, 0x20, 0x00, 0x01 };
            byte[] response = this.SendCommand(command);

            if (response.Length < 2)
            {
                throw new InvalidDataException("Invalid response, expected at least 2 bytes but got " + response.Length);
            }
            if (response[0] != 0x03)
            {
                throw new InvalidDataException("Invalid response, expecting first character of ADU to be 0x03, actual: " + response[0]);
            }
            int byteCount = response[1];
            if (byteCount != 2)
            {
                throw new InvalidDataException("Invalid response, expecting byte count to be 2 and instead: " + response[1]);
            }
            if (response.Length == 2) //this device has NO OUTPUTS
            {
                return 0;
            }
            //the byte order of the response is reversed
            Array.Reverse(response, 2, 2);
            //mask off the top 2 bytes as the return type is int32
            return BitConverter.ToInt16(response, 2) & 0xffff;
        }
        /// <summary>
        /// Get the state of all the Digital IOLines of the device as an integer
        /// the top 16 bits are the outputs, the bottom 16 bits are the inputs
        /// </summary>
        /// <returns>Each bit in the integer represents an IOLine state</returns>
        [Obsolete("GetAllLineStates is deprecated. Replaced by GetAllDigitalLineStates")]
        public int GetAllLineStates()
        {
            return this.GetAllDigitalLineStates();
        }
        /// <summary>
        /// Get the state of all the Digital IOLines of the device as an integer
        /// the top 16 bits are the outputs, the bottom 16 bits are the inputs
        /// </summary>
        /// <returns>Each bit in the integer represents an IOLine state</returns>
        public int GetAllDigitalLineStates()
		{
            //outputs: modbus function 3 Holding registers
            //inputs: modbus function 4 Input registers
            //design decision: assume there will never be more than 16 inputs on a device - not sure if this is a good decision or not
            int ioLines = (GetAllOutputStates() << 16) | (GetAllInputStates() & 0xffff);
            return ioLines;
        }

        /// <summary>
        /// Any command sent to the ED Device is sent by this function
        /// </summary>
        /// <param name="command">correctly encoded modbusTCP command</param>
        /// <param name="s">The stream to send the command on (captured locally to avoid race conditions)</param>
        /// <returns>response from the ED device</returns>
        protected byte[] _sendCommand(byte[] command, BBStream s)
		{
			// if there is a write timeout let it be thrown up to the caller
			s.Write(command, 0, command.Length);

			Byte[] receiveBuffer = new byte[256]; //should be sufficient! ModbusTCP commands are usually much shorter than this

            int timeout = s.ReadTimeout;
			Stopwatch sw;
			bool success = false;
			int readCount = 0;
			
			sw = Stopwatch.StartNew();
            int responseLength = 0;
			while (sw.ElapsedMilliseconds < timeout)
			{
				while (s.DataAvailable > 0)
				{
					readCount += s.Read(receiveBuffer, readCount, s.DataAvailable);
                    //both if statements must be evaluated, as responseLength can be set in the first one
                    //NetworkStream.DataAvailable only returns true or false, but if the user had a serial stream 
                    //(might as well put that scenario in even though unlikely) 
                    //it could return multiple bytes to read
                    //and therefore the readCount could be the length of the PDU on first time of reading
					if (responseLength == 0 && readCount > 5) 
					{
                        //the length part of the MBAP header is byte 4 and 5
                        //the length part of the MBAP header is bytes 4 (high) and 5 (low), big-endian
                        responseLength = (receiveBuffer[4] << 8) | receiveBuffer[5];
                    }
                    if(responseLength > 0 && readCount == responseLength + 6) //we have read the full response
                    {
                        success = true;
                        break;
                    }
				}
				if (success) break;
			}
			sw.Stop();
			if (success)
			{
                _resLength = readCount;
                byte[] response = new byte[readCount];
                Array.Copy(receiveBuffer, response, readCount);
                return response;
            }
            throw new TimeoutException("The operation has timed out after " + sw.ElapsedMilliseconds + "ms, no response received from the Brainboxes Device, was the command valid? Is the device still connected?");

		}

        /// <summary>
        /// The name of the Brainboxes ED Device
        /// </summary>
		protected string _deviceName = null;

		/// <summary>
		/// The name of the ED Device
		/// </summary>
		public string DeviceName
		{
			get
			{
                throw new NotImplementedException();
			}
			set
			{
                throw new NotImplementedException();
            }
        }

		/// <summary>
		/// Get the name of the Device
		/// </summary>
		/// <returns></returns>
		public string GetDeviceName()
		{
            throw new NotImplementedException();
		}

		/// <summary>
		/// Reset the ED device to factory default settings
		/// </summary>
		public void ResetToFactoryDefaultSettings()
		{
            throw new NotImplementedException();
            /*
            Debug.WriteLine("Resetting Device, will also restart device Please disconnect and reconnect connection");
            //success now reset any cached state
			this._resetCache();
            */
		}

		/// <summary>
		/// Power Off and the On the ED Device
		/// </summary>
		public void Restart()
		{
            throw new NotImplementedException();
            /*
            Debug.WriteLine("Restarting Device Please disconnect and reconnect connection");
			this._resetCache();
            */
		}

		private void _resetCache()
		{
			this._deviceName = null;
			this._transactionId = 01;
			this._ioCounterUpdateDirection = IOChangeTypes.Undefined;
		}
        /// <summary>
        /// Get the HIGH LATCH states of all the Digital Inputs
        /// </summary>
        /// <returns></returns>
        [Obsolete("GetAllLatchedHighInputStates is deprecated. Replaced by GetAllLatchedHighDigitalInputStates")]
        public int GetAllLatchedHighInputStates()
        {
            return this.GetAllLatchedHighDigitalInputStates();
        }
		/// <summary>
		/// Get the HIGH LATCH states of all the Digital Inputs
		/// </summary>
		/// <returns></returns>
		public int GetAllLatchedHighDigitalInputStates()
		{
			return GetAllLatchedInputStates();
		}
        /// <summary>
        /// Get the LOW LATCH state of all the Digital INPUTS
        /// </summary>
        /// <returns></returns>
        [Obsolete("GetAllLatchedLowInputStates is deprecated. Replaced by GetAllLatchedLowDigitalInputStates")]
        public int GetAllLatchedLowInputStates()
        {
            return this.GetAllLatchedLowDigitalInputStates();
        }
		/// <summary>
		/// Get the LOW LATCH state of all the Digital INPUTS
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
            throw new NotImplementedException();
            /*
            //from manual @AALS : Reads the status of the latched digital input channels.
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}L{1:D}", this._address, isHigh.GetHashCode()));
			//Valid Command: !DDDD00[CS](CR
			//Invalid Command: ?AA[CS](CR)
			if (response.Length == 0 || response[0] != '!')
			{
				throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
			}
			response = response.Substring(1, 4); // remove leading ">"
			return int.Parse(response, System.Globalization.NumberStyles.HexNumber);
            */
        }
        /// <summary>
        /// Clear the Digital INPUT latches
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
            throw new NotImplementedException();
            /*
            //from manual $AAC : Clears the status of the latched digital input channels..
            string response = this.SendCommand(String.Format(CultureInfo.InvariantCulture, "${0:X2}C", this._address));
			//Valid Command:   !AA[CS]
			//Invalid Command: ?AA[CS]
			if (response.Length == 0 || response[0] != '!')
			{
				throw new InvalidDataException("Invalid response, expecting first character to be \"!\", actual: " + response);
			}
            */
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
            byte[] lineBytes = BitConverter.GetBytes(line);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lineBytes);
            }

            // 04 0000 0001
            // Read DI counter values Input register 4 0x0000 – 7
            byte[] command = new byte[5] { 0x04, 0x00, lineBytes[0], 0x00, 0x01 };
            byte[] response = this.SendCommand(command);

            if (response[0] != 0x04)
            {
                throw new InvalidDataException("Invalid response, expecting first character of ADU to be 0x04, actual: " + response[0]);
            }
            int byteCount = response[1];
            if (byteCount != 2)
            {
                throw new InvalidDataException("Invalid response, expecting byte count to be 2 and instead: " + response[0]);
            }

            //the byte order of the response is reversed
            Array.Reverse(response, 2, 2);
            //mask off the top 2 bytes as the return type is int32
            return BitConverter.ToInt16(response, 2) & 0xffff;
        }
        /// <summary>
        /// Clears the digital input counter of the specified line,
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
            byte[] lineBytes = BitConverter.GetBytes(line);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lineBytes);
            }

            // 05 0200 0001
            // Clear DI counter values coils 5 0x0200 – 7
            byte[] command = new byte[5] { 0x05, 0x02, lineBytes[0], 0x00, 0x00 };
            byte[] response = this.SendCommand(command);

            if (response[0] != 0x05)
            {
                throw new InvalidDataException("Invalid response, expecting first character of ADU to be 0x05, actual: " + response[0]);
            }
        }

		/// <summary>
		/// Read the device configuration
		/// </summary>
		public void GetDeviceConfiguration()
		{
            throw new NotImplementedException();
            //need to know counter update direction??
		}

        public void SetDeviceConfiguration(int newAddress, int newBaudRate, IOChangeTypes ioCounterUpdateDirection, CounterMode counterMode, bool checksum)
        {
            throw new NotImplementedException();
        }

        public void SetDeviceConfiguration(int newAddress, int newBaudRate, AnalogDataFormat newAnalogDataFormat, bool newFilterSettings, bool newModuleSettings, bool newChecksum, TemperatureUnit temperatureUnit)
        {
            throw new NotImplementedException();
        }

    }
}
