using System;
using System.IO;
using Brainboxes.IO.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brainboxes.IO.Tests.Unit
{
    /// <summary>
    /// Unit tests for ASCII protocol command parsing and response handling.
    /// These tests use MockStream to simulate device responses without hardware.
    /// </summary>
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    [TestCategory(TestCategories.Protocol)]
    public class ASCIIProtocolParsingTests
    {
        private MockStream _mockStream;
        private ASCIIProtocol _protocol;

        [TestInitialize]
        public void Setup()
        {
            _mockStream = new MockStream();
            _protocol = new ASCIIProtocol();
            _protocol.Stream = _mockStream;
        }

        #region GetAllDigitalLineStates Tests

        [TestMethod]
        public void GetAllDigitalLineStates_ValidResponse_ParsesCorrectly()
        {
            // Arrange - Queue response for @01 command: >DDDD
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0x1234));

            // Act
            int result = _protocol.GetAllDigitalLineStates();

            // Assert
            Assert.AreEqual(0x1234, result);
        }

        [TestMethod]
        public void GetAllDigitalLineStates_AllHigh_Returns0xFFFF()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0xFFFF));

            // Act
            int result = _protocol.GetAllDigitalLineStates();

            // Assert
            Assert.AreEqual(0xFFFF, result);
        }

        [TestMethod]
        public void GetAllDigitalLineStates_AllLow_Returns0x0000()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0x0000));

            // Act
            int result = _protocol.GetAllDigitalLineStates();

            // Assert
            Assert.AreEqual(0x0000, result);
        }

        [TestMethod]
        public void GetAllDigitalLineStates_InvalidPrefix_ThrowsException()
        {
            // Arrange - Response with wrong prefix (should be >)
            _mockStream.QueueResponse("?01");

            // Act & Assert
            Assert.ThrowsException<InvalidDataException>(() => _protocol.GetAllDigitalLineStates());
        }

        [TestMethod]
        public void GetAllDigitalLineStates_VerifiesCommandSent()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0x0000));

            // Act
            _protocol.GetAllDigitalLineStates();

            // Assert - Verify the correct command was sent (@01 for address 01)
            Assert.AreEqual("@01", _mockStream.LastSentCommand);
        }

        #endregion

        #region SetDigitalOutputLineState Tests

        [TestMethod]
        public void SetDigitalOutputLineState_LowerChannel_UsesACommand()
        {
            // Arrange - Line 0-7 uses 'A' command
            _mockStream.QueueResponse(ASCIIResponseGenerator.SetDigitalOutputLineSuccessResponse());

            // Act
            _protocol.SetDigitalOutputLineState(3, 1);

            // Assert - Command format: #AAAnDD where A=command for lower channels
            string lastCommand = _mockStream.LastSentCommand;
            Assert.IsTrue(lastCommand.Contains("A3"), $"Expected command to contain 'A3' for line 3, got: {lastCommand}");
        }

        [TestMethod]
        public void SetDigitalOutputLineState_UpperChannel_UsesBCommand()
        {
            // Arrange - Line 8-15 uses 'B' command
            _mockStream.QueueResponse(ASCIIResponseGenerator.SetDigitalOutputLineSuccessResponse());

            // Act
            _protocol.SetDigitalOutputLineState(10, 1);

            // Assert - Command format: #AABnDD where B=command for upper channels
            string lastCommand = _mockStream.LastSentCommand;
            Assert.IsTrue(lastCommand.Contains("B2"), $"Expected command to contain 'B2' for line 10 (10-8=2), got: {lastCommand}");
        }

        [TestMethod]
        public void SetDigitalOutputLineState_InvalidResponse_ThrowsException()
        {
            // Arrange - Invalid response (protocol checks for exactly "?")
            _mockStream.QueueResponse("?");

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _protocol.SetDigitalOutputLineState(0, 1));
        }

        [TestMethod]
        public void SetDigitalOutputLineState_IgnoredResponse_ThrowsException()
        {
            // Arrange - Ignored command response
            _mockStream.QueueResponse("!");

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _protocol.SetDigitalOutputLineState(0, 1));
        }

        #endregion

        #region GetDeviceName Tests

        [TestMethod]
        public void GetDeviceName_ValidResponse_ParsesCorrectly()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetDeviceNameResponse("ED-588"));

            // Act
            string result = _protocol.GetDeviceName();

            // Assert
            Assert.AreEqual("ED-588", result);
        }

        [TestMethod]
        public void GetDeviceName_EmptyName_ReturnsEmptyString()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetDeviceNameResponse(""));

            // Act
            string result = _protocol.GetDeviceName();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void GetDeviceName_InvalidResponse_ThrowsException()
        {
            // Arrange - Response without ! prefix
            _mockStream.QueueResponse(">01");

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _protocol.GetDeviceName());
        }

        #endregion

        #region Latch State Tests

        [TestMethod]
        public void GetAllLatchedHighDigitalInputStates_ValidResponse_ParsesCorrectly()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetLatchedHighStatesResponse(0x00F0));

            // Act
            int result = _protocol.GetAllLatchedHighDigitalInputStates();

            // Assert
            Assert.AreEqual(0x00F0, result);
        }

        [TestMethod]
        public void GetAllLatchedLowDigitalInputStates_ValidResponse_ParsesCorrectly()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetLatchedLowStatesResponse(0x0F00));

            // Act
            int result = _protocol.GetAllLatchedLowDigitalInputStates();

            // Assert
            Assert.AreEqual(0x0F00, result);
        }

        [TestMethod]
        public void ClearAllLatchedDigitalInputs_ValidResponse_Succeeds()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.ClearLatchesSuccessResponse());

            // Act - Should not throw
            _protocol.ClearAllLatchedDigitalInputs();

            // Assert - Verify command sent
            Assert.IsTrue(_mockStream.LastSentCommand.StartsWith("$01C"));
        }

        #endregion

        #region Counter Tests

        [TestMethod]
        public void GetDigitalInputLineCount_ValidResponse_ParsesCorrectly()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetLineCountResponse(12345));

            // Act
            int result = _protocol.GetDigitalInputLineCount(0);

            // Assert
            Assert.AreEqual(12345, result);
        }

        [TestMethod]
        public void GetDigitalInputLineCount_ZeroCount_ReturnsZero()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetLineCountResponse(0));

            // Act
            int result = _protocol.GetDigitalInputLineCount(0);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ClearDigitalInputLineCount_ValidResponse_Succeeds()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.ClearLineCountSuccessResponse());

            // Act - Should not throw
            _protocol.ClearDigitalInputLineCount(0);

            // Assert - Verify command sent ($01C0 for clearing line 0)
            Assert.IsTrue(_mockStream.LastSentCommand.StartsWith("$01C"));
        }

        #endregion

        #region Device Configuration Tests

        [TestMethod]
        public void GetDeviceConfiguration_ValidResponse_ParsesCorrectly()
        {
            // Arrange - Response for a digital device
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetDeviceConfigurationResponse(
                deviceType: "40",
                baudRateCode: "06",
                checksum: false,
                counterDirection: IOChangeTypes.RisingEdge));

            // Act - Should not throw
            _protocol.GetDeviceConfiguration();

            // Assert - Verify command sent
            Assert.AreEqual("$012", _mockStream.LastSentCommand);
        }

        [TestMethod]
        public void ResetToFactoryDefaultSettings_ValidResponse_Succeeds()
        {
            // Arrange
            _mockStream.QueueResponse(ASCIIResponseGenerator.ResetSuccessResponse());

            // Act - Should not throw
            _protocol.ResetToFactoryDefaultSettings();

            // Assert
            Assert.AreEqual("$01S1", _mockStream.LastSentCommand);
        }

        #endregion

        #region SendCommand Tests

        [TestMethod]
        public void SendCommand_NoStreamSet_ThrowsInvalidOperationException()
        {
            // Arrange - Create protocol without setting stream
            var protocol = new ASCIIProtocol();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => protocol.SendCommand("@01"));
        }

        [TestMethod]
        public void SendCommand_EmptyResponse_HandlesGracefully()
        {
            // Arrange - Queue empty response
            _mockStream.QueueResponse("");

            // Act & Assert - Behavior depends on command type
            // For most commands, empty response will cause parsing to fail
            try
            {
                _protocol.GetAllDigitalLineStates();
                Assert.Fail("Expected exception for empty response");
            }
            catch (Exception ex)
            {
                // Expected - empty response should cause an error
                Assert.IsTrue(ex is InvalidDataException || ex is IndexOutOfRangeException || ex is FormatException,
                    $"Unexpected exception type: {ex.GetType().Name}");
            }
        }

        #endregion

        #region Analog Tests

        [TestMethod]
        public void GetAllAnalogInputLineStates_ValidResponse_ParsesCorrectly()
        {
            // Arrange - Response for analog inputs with lines enabled response
            // The protocol also calls GetDeviceConfiguration internally, so we need 3 responses
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAllAnalogInputLineStatesResponse(1.234, 2.345, 3.456, 4.567));
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetLinesEnabledResponse(enabledMask: 0x000F)); // 4 lines enabled
            // Use analog device configuration (device type "08" = ED-549) with Engineering format
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAnalogDeviceConfigurationResponse(
                dataFormat: AnalogDataFormat.Engineering));

            // Act
            double[] result = _protocol.GetAllAnalogInputLineStates(4);

            // Assert - Check first value parsed correctly
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(1.234, result[0], 0.001);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Protocol_MultipleCommands_MaintainsState()
        {
            // Arrange
            _mockStream.QueueResponses(
                ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0x1234),
                ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0x5678)
            );

            // Act
            int result1 = _protocol.GetAllDigitalLineStates();
            int result2 = _protocol.GetAllDigitalLineStates();

            // Assert
            Assert.AreEqual(0x1234, result1);
            Assert.AreEqual(0x5678, result2);
        }

        [TestMethod]
        public void Protocol_DifferentAddresses_SendsCorrectCommand()
        {
            // Arrange - Create protocol with address 0x05
            var protocol = new ASCIIProtocol(address: 0x05);
            protocol.Stream = _mockStream;
            _mockStream.QueueResponse(ASCIIResponseGenerator.GetAllDigitalLineStatesResponse(0x0000));

            // Act
            protocol.GetAllDigitalLineStates();

            // Assert - Command should use address 05
            Assert.AreEqual("@05", _mockStream.LastSentCommand);
        }

        #endregion
    }
}
