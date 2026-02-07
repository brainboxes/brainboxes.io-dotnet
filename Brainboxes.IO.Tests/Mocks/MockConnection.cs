using System;
using System.IO;

namespace Brainboxes.IO.Tests.Mocks
{
    /// <summary>
    /// Mock connection for testing without hardware. Implements IConnection
    /// and provides control over connection state and behavior.
    /// </summary>
    public class MockConnection : IConnection
    {
        private MockStream _stream;
        private bool _isConnected;
        private bool _isAvailable = true;
        private int _timeout = 1000;
        private bool _disposed;

        /// <summary>
        /// Creates a new MockConnection with its own MockStream.
        /// </summary>
        public MockConnection()
        {
            _stream = new MockStream();
        }

        /// <summary>
        /// Creates a MockConnection using an existing MockStream.
        /// </summary>
        public MockConnection(MockStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        #region IConnection Implementation

        /// <summary>
        /// Whether the connection is currently connected.
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Whether the connection is available (device online/reachable).
        /// </summary>
        public bool IsAvailable => _isAvailable;

        /// <summary>
        /// Timeout for read/write operations in milliseconds.
        /// </summary>
        public int Timeout
        {
            get => _timeout;
            set => _timeout = value;
        }

        /// <summary>
        /// The underlying stream (returns MockStream).
        /// </summary>
        public Stream Stream => _stream;

        /// <summary>
        /// Event raised when connection status changes.
        /// </summary>
        public event ConnectionStatusChangedEventHandler ConnectionStatusChangedEvent;

        /// <summary>
        /// Simulates connecting to a device.
        /// </summary>
        public void Connect()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MockConnection));

            if (SimulateConnectionFailure)
            {
                throw new IOException("Simulated connection failure");
            }

            if (!_isConnected)
            {
                _isConnected = true;
                OnConnectionStatusChanged("IsConnected", true);
            }
        }

        /// <summary>
        /// Simulates disconnecting from a device.
        /// </summary>
        public void Disconnect()
        {
            if (_isConnected)
            {
                _isConnected = false;
                OnConnectionStatusChanged("IsConnected", false);
            }
        }

        /// <summary>
        /// Disposes the connection.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
        }

        #endregion

        #region Test Control Properties

        /// <summary>
        /// Gets the underlying MockStream for queueing responses and verifying sent data.
        /// </summary>
        public MockStream MockStream => _stream;

        /// <summary>
        /// When true, Connect() will throw an IOException.
        /// </summary>
        public bool SimulateConnectionFailure { get; set; }

        /// <summary>
        /// Simulated latency to add to operations (not yet implemented).
        /// </summary>
        public TimeSpan SimulatedLatency { get; set; }

        /// <summary>
        /// Sets the IsAvailable state and raises the ConnectionStatusChangedEvent if changed.
        /// </summary>
        public void SetIsAvailable(bool available)
        {
            if (_isAvailable != available)
            {
                _isAvailable = available;
                OnConnectionStatusChanged("IsAvailable", available);
            }
        }

        /// <summary>
        /// Simulates a disconnection (for testing reconnection scenarios).
        /// </summary>
        public void SimulateDisconnect()
        {
            if (_isConnected)
            {
                _isConnected = false;
                OnConnectionStatusChanged("IsConnected", false);
            }
        }

        /// <summary>
        /// Queues a response string to be returned on the next protocol read.
        /// Convenience method that delegates to MockStream.
        /// </summary>
        public void QueueResponse(string response)
        {
            _stream.QueueResponse(response);
        }

        /// <summary>
        /// Queues multiple responses at once.
        /// </summary>
        public void QueueResponses(params string[] responses)
        {
            _stream.QueueResponses(responses);
        }

        /// <summary>
        /// Gets the last command sent through this connection.
        /// </summary>
        public string LastSentCommand => _stream.LastSentCommand;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Raises the ConnectionStatusChangedEvent.
        /// </summary>
        protected virtual void OnConnectionStatusChanged(string property, bool newValue)
        {
            ConnectionStatusChangedEvent?.Invoke(this, property, newValue);
        }

        /// <summary>
        /// Returns a string representation of this connection.
        /// </summary>
        public override string ToString()
        {
            return $"MockConnection [Connected={_isConnected}, Available={_isAvailable}]";
        }

        #endregion
    }
}
