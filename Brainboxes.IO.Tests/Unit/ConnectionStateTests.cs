using System;
using Brainboxes.IO.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brainboxes.IO.Tests.Unit
{
    /// <summary>
    /// Unit tests for connection state management using MockConnection.
    /// These tests verify connection lifecycle and event handling without hardware.
    /// </summary>
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    [TestCategory(TestCategories.Connection)]
    public class ConnectionStateTests
    {
        private MockConnection _connection;

        [TestInitialize]
        public void Setup()
        {
            _connection = new MockConnection();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _connection?.Dispose();
        }

        #region Connection Lifecycle Tests

        [TestMethod]
        public void MockConnection_InitialState_NotConnected()
        {
            // Assert
            Assert.IsFalse(_connection.IsConnected);
        }

        [TestMethod]
        public void MockConnection_InitialState_IsAvailable()
        {
            // Assert
            Assert.IsTrue(_connection.IsAvailable);
        }

        [TestMethod]
        public void Connect_Success_IsConnectedTrue()
        {
            // Act
            _connection.Connect();

            // Assert
            Assert.IsTrue(_connection.IsConnected);
        }

        [TestMethod]
        public void Disconnect_AfterConnect_IsConnectedFalse()
        {
            // Arrange
            _connection.Connect();

            // Act
            _connection.Disconnect();

            // Assert
            Assert.IsFalse(_connection.IsConnected);
        }

        [TestMethod]
        public void Connect_AlreadyConnected_NoError()
        {
            // Arrange
            _connection.Connect();

            // Act - Should not throw
            _connection.Connect();

            // Assert
            Assert.IsTrue(_connection.IsConnected);
        }

        [TestMethod]
        public void Disconnect_NotConnected_NoError()
        {
            // Act - Should not throw
            _connection.Disconnect();

            // Assert
            Assert.IsFalse(_connection.IsConnected);
        }

        #endregion

        #region Connection Failure Simulation Tests

        [TestMethod]
        public void Connect_SimulateFailure_ThrowsException()
        {
            // Arrange
            _connection.SimulateConnectionFailure = true;

            // Act & Assert
            Assert.ThrowsException<System.IO.IOException>(() => _connection.Connect());
        }

        [TestMethod]
        public void Connect_SimulateFailure_RemainsDisconnected()
        {
            // Arrange
            _connection.SimulateConnectionFailure = true;

            // Act
            try { _connection.Connect(); } catch { }

            // Assert
            Assert.IsFalse(_connection.IsConnected);
        }

        #endregion

        #region Event Notification Tests

        [TestMethod]
        public void Connect_RaisesConnectionStatusChangedEvent()
        {
            // Arrange
            string propertyChanged = null;
            bool newValue = false;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) =>
            {
                propertyChanged = prop;
                newValue = val;
            };

            // Act
            _connection.Connect();

            // Assert
            Assert.AreEqual("IsConnected", propertyChanged);
            Assert.IsTrue(newValue);
        }

        [TestMethod]
        public void Disconnect_RaisesConnectionStatusChangedEvent()
        {
            // Arrange
            _connection.Connect();
            string propertyChanged = null;
            bool newValue = true;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) =>
            {
                propertyChanged = prop;
                newValue = val;
            };

            // Act
            _connection.Disconnect();

            // Assert
            Assert.AreEqual("IsConnected", propertyChanged);
            Assert.IsFalse(newValue);
        }

        [TestMethod]
        public void SetIsAvailable_RaisesConnectionStatusChangedEvent()
        {
            // Arrange
            string propertyChanged = null;
            bool newValue = true;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) =>
            {
                propertyChanged = prop;
                newValue = val;
            };

            // Act
            _connection.SetIsAvailable(false);

            // Assert
            Assert.AreEqual("IsAvailable", propertyChanged);
            Assert.IsFalse(newValue);
        }

        [TestMethod]
        public void SetIsAvailable_SameValue_NoEventRaised()
        {
            // Arrange
            int eventCount = 0;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) => eventCount++;

            // Act
            _connection.SetIsAvailable(true); // Same as initial value

            // Assert
            Assert.AreEqual(0, eventCount);
        }

        [TestMethod]
        public void SimulateDisconnect_RaisesEvent()
        {
            // Arrange
            _connection.Connect();
            bool eventRaised = false;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) =>
            {
                if (prop == "IsConnected" && !val)
                    eventRaised = true;
            };

            // Act
            _connection.SimulateDisconnect();

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.IsFalse(_connection.IsConnected);
        }

        #endregion

        #region Timeout Tests

        [TestMethod]
        public void Timeout_DefaultValue()
        {
            // Assert
            Assert.AreEqual(1000, _connection.Timeout);
        }

        [TestMethod]
        public void Timeout_SetAndGet()
        {
            // Act
            _connection.Timeout = 5000;

            // Assert
            Assert.AreEqual(5000, _connection.Timeout);
        }

        #endregion

        #region Stream Tests

        [TestMethod]
        public void Stream_ReturnsMockStream()
        {
            // Act
            var stream = _connection.Stream;

            // Assert
            Assert.IsNotNull(stream);
            Assert.IsInstanceOfType(stream, typeof(MockStream));
        }

        [TestMethod]
        public void MockStream_CanQueueResponses()
        {
            // Act
            _connection.QueueResponse(">1234");

            // Assert
            Assert.IsTrue(_connection.MockStream.DataAvailable == 0); // No data until Write triggers
        }

        #endregion

        #region Dispose Tests

        [TestMethod]
        public void Dispose_DisconnectsIfConnected()
        {
            // Arrange
            _connection.Connect();

            // Act
            _connection.Dispose();

            // Assert
            Assert.IsFalse(_connection.IsConnected);
        }

        [TestMethod]
        public void Connect_AfterDispose_ThrowsObjectDisposedException()
        {
            // Arrange
            _connection.Dispose();

            // Act & Assert
            Assert.ThrowsException<ObjectDisposedException>(() => _connection.Connect());
        }

        #endregion

        #region Multiple Event Handler Tests

        [TestMethod]
        public void ConnectionStatusChangedEvent_MultipleHandlers_AllCalled()
        {
            // Arrange
            int handler1Count = 0;
            int handler2Count = 0;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) => handler1Count++;
            _connection.ConnectionStatusChangedEvent += (conn, prop, val) => handler2Count++;

            // Act
            _connection.Connect();

            // Assert
            Assert.AreEqual(1, handler1Count);
            Assert.AreEqual(1, handler2Count);
        }

        [TestMethod]
        public void ConnectionStatusChangedEvent_RemoveHandler_NotCalled()
        {
            // Arrange
            int handlerCount = 0;
            ConnectionStatusChangedEventHandler handler = (conn, prop, val) => handlerCount++;
            _connection.ConnectionStatusChangedEvent += handler;
            _connection.ConnectionStatusChangedEvent -= handler;

            // Act
            _connection.Connect();

            // Assert
            Assert.AreEqual(0, handlerCount);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsDescriptiveString()
        {
            // Act
            string result = _connection.ToString();

            // Assert
            Assert.IsTrue(result.Contains("MockConnection"));
            Assert.IsTrue(result.Contains("Connected=False"));
        }

        [TestMethod]
        public void ToString_AfterConnect_ReflectsState()
        {
            // Arrange
            _connection.Connect();

            // Act
            string result = _connection.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Connected=True"));
        }

        #endregion
    }
}
