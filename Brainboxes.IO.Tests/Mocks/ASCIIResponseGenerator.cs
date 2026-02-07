using System;
using System.Globalization;
using System.Text;

namespace Brainboxes.IO.Tests.Mocks
{
    /// <summary>
    /// Static helper to generate valid ASCII protocol responses for testing.
    /// Based on the Brainboxes ASCII Protocol specification.
    /// </summary>
    public static class ASCIIResponseGenerator
    {
        /// <summary>
        /// Default device address (01 in hex).
        /// </summary>
        public const int DefaultAddress = 0x01;

        #region Digital IO Responses

        /// <summary>
        /// Generates a response for GetAllDigitalLineStates (@AA command).
        /// Format: >DDDD where DDDD is the hex state (4 hex digits for 16 lines).
        /// </summary>
        /// <param name="states">The digital line states as a bitmask.</param>
        /// <param name="hexDigits">Number of hex digits (4 for 16 lines, 2 for 8 lines).</param>
        public static string GetAllDigitalLineStatesResponse(int states, int hexDigits = 4)
        {
            return string.Format(CultureInfo.InvariantCulture, ">{0:X" + hexDigits + "}", states);
        }

        /// <summary>
        /// Generates a success response for SetDigitalOutputLineState.
        /// Format: >
        /// </summary>
        public static string SetDigitalOutputLineSuccessResponse()
        {
            return ">";
        }

        /// <summary>
        /// Generates a success response for SetAllDigitalOutputLineStates (@AA command).
        /// Format: >
        /// </summary>
        public static string SetAllDigitalOutputLinesSuccessResponse()
        {
            return ">";
        }

        #endregion

        #region Analog IO Responses

        /// <summary>
        /// Generates a response for GetAllAnalogInputLineStates (#AA command).
        /// Format: >+DD.DDD+DD.DDD... (engineering format with +/- prefix per value)
        /// </summary>
        /// <param name="values">The analog values.</param>
        public static string GetAllAnalogInputLineStatesResponse(params double[] values)
        {
            var sb = new StringBuilder(">");
            foreach (var value in values)
            {
                string sign = value >= 0 ? "+" : "";
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1:F3}", sign, value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a response for the lines enabled query ($AA6 command).
        /// Format: !AAFFFF where FFFF is the enabled lines mask.
        /// </summary>
        /// <param name="address">Device address.</param>
        /// <param name="enabledMask">Bitmask of enabled lines.</param>
        public static string GetLinesEnabledResponse(int address = DefaultAddress, int enabledMask = 0xFF)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}{1:X4}", address, enabledMask);
        }

        /// <summary>
        /// Generates a response for GetAnalogOutputLineState ($AA6n command).
        /// Format: !AA+DD.DDD
        /// </summary>
        public static string GetAnalogOutputLineStateResponse(double value, int address = DefaultAddress)
        {
            string sign = value >= 0 ? "+" : "";
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}{1}{2:F3}", address, sign, value);
        }

        /// <summary>
        /// Generates a success response for SetAnalogOutputLineState.
        /// Format: >
        /// </summary>
        public static string SetAnalogOutputLineSuccessResponse()
        {
            return ">";
        }

        #endregion

        #region Latch Responses

        /// <summary>
        /// Generates a response for GetAllLatchedHighDigitalInputStates ($AAL1 command).
        /// Format: !DDDD00 where DDDD is the latched states.
        /// </summary>
        public static string GetLatchedHighStatesResponse(int states, int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X4}00", states);
        }

        /// <summary>
        /// Generates a response for GetAllLatchedLowDigitalInputStates ($AAL0 command).
        /// Format: !DDDD00 where DDDD is the latched states.
        /// </summary>
        public static string GetLatchedLowStatesResponse(int states, int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X4}00", states);
        }

        /// <summary>
        /// Generates a success response for ClearAllLatchedDigitalInputs ($AAC command).
        /// Format: !AA
        /// </summary>
        public static string ClearLatchesSuccessResponse(int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}", address);
        }

        #endregion

        #region Counter Responses

        /// <summary>
        /// Generates a response for GetDigitalInputLineCount (#AAN command).
        /// Format: !AAData where Data is the count value.
        /// </summary>
        public static string GetLineCountResponse(int count, int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}{1}", address, count);
        }

        /// <summary>
        /// Generates a success response for ClearDigitalInputLineCount ($AACN command).
        /// Format: !AA
        /// </summary>
        public static string ClearLineCountSuccessResponse(int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}", address);
        }

        #endregion

        #region Device Configuration Responses

        /// <summary>
        /// Generates a response for GetDeviceName ($AAM command).
        /// Format: !AAname
        /// </summary>
        public static string GetDeviceNameResponse(string name, int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}{1}", address, name);
        }

        /// <summary>
        /// Generates a response for GetDeviceConfiguration ($AA2 command) for a digital device.
        /// Format: !AAttccff where:
        /// - tt = device type
        /// - cc = baud rate code
        /// - ff = configuration flags
        /// </summary>
        public static string GetDeviceConfigurationResponse(
            int address = DefaultAddress,
            string deviceType = "40", // ED-588 default
            string baudRateCode = "06", // 9600
            bool checksum = false,
            IOChangeTypes counterDirection = IOChangeTypes.RisingEdge,
            CounterMode counterMode = CounterMode.CounterMode16Bits)
        {
            int ff = 0;
            ff |= counterDirection == IOChangeTypes.FallingEdge ? 0x80 : 0x00;
            ff |= checksum ? 0x40 : 0x00;
            ff |= counterMode == CounterMode.CounterMode32Bits ? 0x20 : 0x00;

            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}{1}{2}{3:X2}",
                address, deviceType, baudRateCode, ff);
        }

        /// <summary>
        /// Generates a response for GetDeviceConfiguration ($AA2 command) for an analog device.
        /// Device types: "08" (ED-549), "30" (ED-560), "80" (ED-582/ED-593)
        /// Format: !AAttccff where ff contains analog data format in bits 0-1
        /// </summary>
        public static string GetAnalogDeviceConfigurationResponse(
            int address = DefaultAddress,
            string deviceType = "08", // ED-549 default
            string baudRateCode = "06", // 9600
            AnalogDataFormat dataFormat = AnalogDataFormat.Engineering,
            bool filterSetting = false,
            bool moduleSetting = false,
            bool checksum = false,
            TemperatureUnit temperatureUnit = TemperatureUnit.Celsius)
        {
            int ff = (int)dataFormat & 0x03;
            ff |= ((int)temperatureUnit & 0x03) << 2;
            ff |= moduleSetting ? 0x20 : 0x00;
            ff |= checksum ? 0x40 : 0x00;
            ff |= filterSetting ? 0x80 : 0x00;

            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}{1}{2}{3:X2}",
                address, deviceType, baudRateCode, ff);
        }

        /// <summary>
        /// Generates a success response for SetDeviceConfiguration (%AANNTTCCFF command).
        /// Format: !AA
        /// </summary>
        public static string SetDeviceConfigurationSuccessResponse(int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}", address);
        }

        /// <summary>
        /// Generates a success response for ResetToFactoryDefaultSettings ($AAS1 command).
        /// Format: !AA
        /// </summary>
        public static string ResetSuccessResponse(int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}", address);
        }

        #endregion

        #region Error Responses

        /// <summary>
        /// Generates an invalid command response.
        /// Format: ?AA
        /// </summary>
        public static string InvalidCommandResponse(int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "?{0:X2}", address);
        }

        /// <summary>
        /// Generates an ignored command response.
        /// Format: !
        /// </summary>
        public static string IgnoredCommandResponse()
        {
            return "!";
        }

        /// <summary>
        /// Generates a generic success response.
        /// Format: !AA
        /// </summary>
        public static string GenericSuccessResponse(int address = DefaultAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, "!{0:X2}", address);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Formats an address as a 2-digit hex string.
        /// </summary>
        public static string FormatHexAddress(int address)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X2}", address);
        }

        /// <summary>
        /// Formats digital states as a hex string with the specified width.
        /// </summary>
        public static string FormatDigitalStates(int states, int hexDigits = 4)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X" + hexDigits + "}", states);
        }

        #endregion
    }
}
