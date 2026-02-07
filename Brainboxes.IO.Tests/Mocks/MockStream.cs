using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Brainboxes.IO.Tests.Mocks
{
    /// <summary>
    /// Mock stream for testing without hardware. Inherits from BBStream to provide
    /// the streamReadLock and streamWriteLock that ASCIIProtocol.SendCommand() requires.
    /// </summary>
    public class MockStream : BBStream
    {
        private readonly Queue<byte[]> _responseQueue = new Queue<byte[]>();
        private readonly List<byte[]> _sentData = new List<byte[]>();
        private byte[] _currentResponse;
        private int _currentPosition;
        private readonly Encoding _encoding;
        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new MockStream with ISO-8859-1 encoding (ASCII protocol default).
        /// Sets a default read timeout of 1000ms for protocol compatibility.
        /// </summary>
        public MockStream() : base()
        {
            _encoding = Encoding.GetEncoding("ISO-8859-1");
            // Set a reasonable timeout for mock testing (protocol checks this)
            _readTimeout = 1000;
        }

        /// <summary>
        /// Queue a string response to be returned on the next read operation.
        /// Automatically appends carriage return if not present (ASCII protocol terminator).
        /// </summary>
        public void QueueResponse(string response)
        {
            if (!response.EndsWith("\r"))
            {
                response += "\r";
            }
            QueueResponse(_encoding.GetBytes(response));
        }

        /// <summary>
        /// Queue a raw byte array response to be returned on the next read operation.
        /// Thread-safe.
        /// </summary>
        public void QueueResponse(byte[] response)
        {
            lock (_lock)
            {
                _responseQueue.Enqueue(response);
            }
        }

        /// <summary>
        /// Queue multiple responses at once.
        /// </summary>
        public void QueueResponses(params string[] responses)
        {
            foreach (var response in responses)
            {
                QueueResponse(response);
            }
        }

        /// <summary>
        /// Clear any queued responses. Thread-safe.
        /// </summary>
        public void ClearResponses()
        {
            lock (_lock)
            {
                _responseQueue.Clear();
                _currentResponse = null;
                _currentPosition = 0;
            }
        }

        /// <summary>
        /// Gets all data that has been written to this stream.
        /// </summary>
        public IReadOnlyList<byte[]> SentData => _sentData;

        /// <summary>
        /// Gets the last command sent as a string (without the carriage return terminator).
        /// </summary>
        public string LastSentCommand
        {
            get
            {
                if (_sentData.Count == 0) return null;
                var lastData = _sentData[_sentData.Count - 1];
                var str = _encoding.GetString(lastData);
                return str.TrimEnd('\r');
            }
        }

        /// <summary>
        /// Gets all commands sent as strings.
        /// </summary>
        public IEnumerable<string> AllSentCommands
        {
            get
            {
                foreach (var data in _sentData)
                {
                    yield return _encoding.GetString(data).TrimEnd('\r');
                }
            }
        }

        /// <summary>
        /// Clears all recorded sent data.
        /// </summary>
        public void ClearSentData()
        {
            _sentData.Clear();
        }

        /// <summary>
        /// Number of bytes available to read. Only returns data when a response
        /// has been triggered by a Write operation (command sent). Thread-safe.
        /// </summary>
        public override int DataAvailable
        {
            get
            {
                lock (_lock)
                {
                    // Only return data from current response, not from the queue
                    // The queue represents future responses for future commands
                    if (_currentResponse != null && _currentPosition < _currentResponse.Length)
                    {
                        return _currentResponse.Length - _currentPosition;
                    }
                    return 0;
                }
            }
        }

        /// <summary>
        /// Read from the current response (which was triggered by Write). Thread-safe.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                // Only read from current response - don't auto-dequeue
                // Responses are triggered by Write operations
                if (_currentResponse == null || _currentPosition >= _currentResponse.Length)
                {
                    // No data available
                    return 0;
                }

                int bytesToRead = Math.Min(count, _currentResponse.Length - _currentPosition);
                Array.Copy(_currentResponse, _currentPosition, buffer, offset, bytesToRead);
                _currentPosition += bytesToRead;

                return bytesToRead;
            }
        }

        /// <summary>
        /// Capture written data for verification. Also triggers the next
        /// queued response to become available for reading. Thread-safe.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                byte[] copy = new byte[count];
                Array.Copy(buffer, offset, copy, 0, count);
                _sentData.Add(copy);

                // When a command is written, make the next response available
                if (_responseQueue.Count > 0)
                {
                    _currentResponse = _responseQueue.Dequeue();
                    _currentPosition = 0;
                }
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
