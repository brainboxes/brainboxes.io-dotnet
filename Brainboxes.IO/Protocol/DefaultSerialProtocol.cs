using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Brainboxes.IO
{
    /// <summary>
    /// Default implementation of <see cref="ISerialProtocol"/> for text-based serial communication
    /// over Brainboxes Ethernet-to-Serial devices (ES-series).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides encoding-aware <see cref="Send"/> and <see cref="Receive"/> methods with
    /// configurable <see cref="TerminatingCharacters"/> (default: <c>\n</c>) and
    /// <see cref="Encoding"/> (default: UTF-8).
    /// </para>
    /// <para>
    /// On multi-port devices (e.g. <see cref="ES257"/>), each port gets its own cloned
    /// protocol instance via <see cref="Clone"/> so that ports maintain independent state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// var es = new ES246(new TCPConnection("192.168.0.100"));
    /// es.Connect();
    /// es.Ports[0].Send("Hello");
    /// string response = es.Ports[0].Receive();
    /// es.Disconnect();
    /// </code>
    /// </example>
    public class DefaultSerialProtocol : Protocol, ISerialProtocol
    {
        /// <summary>
        /// default encoding UTF-8, should be a highly compatible option for text based communication protocols
        /// including full compatibility with ASCII
        /// </summary>
        protected Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// The character encoding used to send and receive data, default UTF-8, 
        /// should be a highly compatible option for text based communication protocols
        /// including full compatibility with ASCII
        /// </summary>
        public Encoding Encoding { get { return _encoding; } set { _encoding = value; } }

        /// <summary>
        /// The receive buffer stores the inbound stream data before it is converted to the char encoding
        /// the underlying stream has a default receive buffer size of 8192 see:
        /// https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.receivebuffersize%28v=vs.110%29.aspx
        /// </summary>
        protected Byte[] receiveBuffer = new byte[8192]; //magic number assumption

        /// <summary>
        /// default terminating character \n New Line (Linux Standard Line Ending)
        /// </summary>
        protected string _terminatingChars = "\n";

        /// <summary>
        /// The character or sequence of characters that define the end of a data transmission, 
        /// this will be automatically added to the end of sent data and stripped fromcheer
        /// the end of a received data
        /// The default terminating-character is \n New Line (Linux Standard Line Ending)
        /// Note Default Windows line ending is \r\n Carriage Return New Line, which will still work with this default, 
        /// For better results with Windows line endings change this value to '\r\n'
        /// </summary>
        public string TerminatingCharacters { get { return _terminatingChars; } set { _terminatingChars = value; } }

        /// <summary>
        /// Whether there is data available, returns 0 for no data and 1 more 1 or more characters
        /// </summary>
        public int DataAvailable
        {
            get
            {
                // Capture stream reference locally to avoid race condition during disconnect
                BBStream s = stream as BBStream;
                if (s == null) throw new InvalidOperationException("Brainboxes Device not connected");
                return s.DataAvailable;
            }
        }

        /// <summary>
        /// Send data down the serial port encoded in the set encoding
        /// </summary>
        /// <param name="message"></param>
        public virtual void Send(string message)
        {
            // Capture stream reference locally to avoid race condition during disconnect
            Stream localStream = stream;
            if (localStream == null) throw new InvalidOperationException("Brainboxes Device not connected");
            BBStream s = localStream as BBStream;
            if (s == null) throw new InvalidOperationException("Brainboxes Device not connected");
            lock(s.streamWriteLock)
            {
                Byte[] sendBuffer = _encoding.GetBytes(message+_terminatingChars);
                localStream.Write(sendBuffer, 0, sendBuffer.Length);
                localStream.Flush();
            }
        }

        /// <summary>
        /// Receive data from the serial port decoded in the set encoding
        /// Will block until: the timeout is reached in which case an exception is thrown
        /// or until the terminating character is found
        /// </summary>
        /// <returns></returns>
        public virtual string Receive()
        {
            // Capture stream reference locally to avoid race condition during disconnect
            Stream localStream = stream;
            if (localStream == null) throw new InvalidOperationException("Brainboxes Device not connected");
            BBStream s = localStream as BBStream;
            if (s == null) throw new InvalidOperationException("Brainboxes Device not connected");

            lock (s.streamReadLock)
            {
                Byte[] terminatingCharCode = _encoding.GetBytes(_terminatingChars);
                int terminatingCharsCodeLength = terminatingCharCode.Length;

                int timeout = s.ReadTimeout;
                bool success = false;
                int readCount = 0;
                bool dataWasAvailable = false;
                long loopCount = 0;

                Stopwatch sw = Stopwatch.StartNew();

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                while (sw.ElapsedMilliseconds < timeout)
                {
                    while (s.DataAvailable > 0)
                    {
                        dataWasAvailable = true;
                        readCount += s.Read(receiveBuffer, readCount, s.DataAvailable);
                        int i = 0;
                        while (++i <= terminatingCharsCodeLength) // check the terminating characters
                        {
                            if (receiveBuffer[readCount - i] == terminatingCharCode[terminatingCharsCodeLength - i])
                            {
                                success = true;
                            }
                            else
                            {
                                success = false;
                                break;
                            }
                        }

                    }
                    if (success) break;
                    if (dataWasAvailable)
                    {
                        dataWasAvailable = false;
                        //restart the stop watch after receiving data, otherwise for slow streams 
                        //of data the time will run out before all the data has finished arriving
                        sw.Reset();
                        sw.Start();
                    }
                    loopCount++;
                    //allow other tasks to run plus this loop can be too tight and CPU intensive
                    //without it (especially with multiple simultaneous requests to multiple devices
                    resetEvent.WaitOne(1);
                }
                sw.Stop();
                if (success)
                {

                    string response = _encoding.GetString(receiveBuffer, 0, readCount - terminatingCharsCodeLength);
                    Array.Clear(receiveBuffer, 0, readCount);
                    return response;
                }
                Array.Clear(receiveBuffer, 0, readCount);
                throw new TimeoutException("The operation has timed out after " + sw.ElapsedMilliseconds + "ms, no response received from the Brainboxes Device, was the command valid? Is the device still connected? "+loopCount);
            }
        }

        /// <summary>
        /// Make a shallow copy of the DefaultSerialProtocol
        /// This means when it is assigned to each port the instance is not shared
        /// so each port maintains its own state
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new DefaultSerialProtocol()
            {
                TerminatingCharacters = this.TerminatingCharacters,
                Stream = stream,
                Encoding = this.Encoding,
            };
        }
    }
}
