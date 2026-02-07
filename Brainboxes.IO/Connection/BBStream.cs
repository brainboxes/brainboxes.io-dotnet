using System;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;

namespace Brainboxes.IO
{
    /// <summary>
    /// Stream wrapper which adds ability to flush and check for 
    /// available data on all streams which need to be accessed.
    /// Also adds ability to have a timeout on stream read and writes without the
    /// underlying connection closing on failure
    /// </summary>
    public abstract class BBStream : Stream
    {

        internal BBStream()
        {

        }

        /// <summary>
        /// Create a BBStream from a connection class
        /// </summary>
        /// <param name="baseStream"></param>
        internal BBStream(Stream baseStream)
        {
            this._baseStream = baseStream;
            this._readTimeout = baseStream.ReadTimeout;
            baseStream.ReadTimeout = -1; //infinity
        }

        protected Stream _baseStream;
        /// <summary>
        /// the underlying stream
        /// </summary>
        public Stream BaseStream { get { return _baseStream; } }

        /// <summary>
        /// ensure mutual exclusion for send and receive functions
        /// which is really locking read and writes to the stream
        /// lock must be in stream not protocol as protocols could be shared
        /// </summary>
        internal object streamReadLock = new Object();

        /// <summary>
        /// ensure mutual exclusion for send and receive functions
        /// which is really locking read and writes to the stream
        /// lock must be in stream not protocol as protocols could be shared
        /// </summary>
        internal object streamWriteLock = new Object();

        /// <summary>
        /// implement a read timeout
        /// which does not cause the connection to go into an unknown state. Specifically
        /// NetworkStream timeouts cause disconnection, we don't want the protocol/device objects to have to know
        /// about or handle this. Disconnecting and Reconnecting the connection on timeout can be extremely slow
        /// instead, implement our own timeout mechanism.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(Byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Indicates whether data is available, can be 0, for no data, 1 for 1 or more bytes, 
        /// or a number representing the precise number of bytes
        /// </summary>
        public abstract int DataAvailable { get; }

        /// <summary>
        /// Clear the read and write streams
        /// </summary>
        public override void Flush()
        {
            _baseStream.Flush();
        }


        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _baseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _baseStream.CanWrite; }
        }

        public override long Length
        {
            get { return _baseStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _baseStream.Position;
            }
            set
            {
                _baseStream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        protected int _readTimeout = 0;
        public override int ReadTimeout
        {
            get
            {
                //use the writeTimeout to measure the read timeout differently
                return this._readTimeout;
            }
            set
            {
                this._readTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return _baseStream.WriteTimeout;
            }
            set
            {
                _baseStream.WriteTimeout = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal class BBNetworkStream : BBStream
    {
        internal BBNetworkStream(Socket soc) : base(new NetworkStream(soc))
        {
        }

        public override int DataAvailable
        {
            get { return ((NetworkStream)_baseStream).DataAvailable ? 1 : 0; }
        }

    }

    internal class BBSerialStream : BBStream
    {
        internal BBSerialStream(SerialPort sp) : base(sp.BaseStream)
        {
            this._sp = sp;
        }

        protected SerialPort _sp;

        public override int DataAvailable
        {
            get { return this._sp.BytesToRead; }
        }

        public override void Flush()
        {
            //this is not available through the serial port baseStream!!
            this._sp.DiscardInBuffer();
            this._sp.DiscardOutBuffer();
        }
    }
}
