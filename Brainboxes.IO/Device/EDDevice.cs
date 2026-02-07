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
using System.Threading;
using System.Xml;


namespace Brainboxes.IO
{
    /// <summary>
    /// Base class for all Brainboxes Ethernet Remote IO devices (part numbers starting "ED-XXX").
    /// Provides access to digital and analog IO lines, event-driven monitoring, and device management.
    /// </summary>
    /// <remarks>
    /// <para>
    /// IO lines are accessible via the <see cref="Inputs"/>, <see cref="Outputs"/>,
    /// <see cref="AInputs"/>, and <see cref="AOutputs"/> collections. Each line's state
    /// is read/written through the <see cref="IOLine.Value"/> property (digital) or
    /// <see cref="IOLine.AValue"/> property (analog).
    /// </para>
    /// <para>
    /// For event-driven monitoring, subscribe to <see cref="IOLineChanged"/>,
    /// <see cref="IOLineRisingEdge"/>, <see cref="IOLineFallingEdge"/>, or
    /// <see cref="IOLineCount"/>. A background polling timer checks for state changes
    /// at intervals controlled by <see cref="IOLineCacheTimeout"/>.
    /// </para>
    /// <para>
    /// Use the <see cref="Create(string, int, int)"/> factory method to auto-detect
    /// the device type from its IP address, or instantiate a specific subclass
    /// (e.g. <see cref="ED588"/>, <see cref="ED549"/>) directly.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// // Auto-detect device type
    /// using (EDDevice ed = EDDevice.Create("192.168.0.80"))
    /// {
    ///     // Read digital inputs
    ///     Console.WriteLine(ed.Inputs[0].Value);
    ///
    ///     // Set digital outputs
    ///     ed.Outputs[0].Value = 1;
    ///
    ///     // Monitor changes
    ///     ed.IOLineChanged += (line, device, changeType) =>
    ///         Console.WriteLine($"Line {line.IONumber}: {changeType}");
    /// }
    /// </code>
    /// </example>
    public class EDDevice : Device<IConnection, IIOProtocol>, IEDDevice, IIOLineDelegate
    {
        /// <summary>
        /// Create a new EDDevice, connection needs to be set, protocol defaults to ASCIIProtocol
        /// </summary>
        public EDDevice()
        {
            //default protocol
            this._protocol = new ASCIIProtocol();
            _initLines();
        }

        /// <summary>
        /// Create a new EDDevice, passing in the connection, protocol defaults to ASCIIProtocol
        /// </summary>
        public EDDevice(IConnection connection) : base(connection)
        {
            //default protocol
            if (connection is SerialConnection)
            {
                this.Protocol = new ASCIIProtocol();
            }
            else
            {
                TCPConnection con = connection as TCPConnection;
                if (con.Port == TCPConnection.DEFAULT_MODBUSTCP_PORT)
                {
                    this.Protocol = new ModbusTCPProtocol();
                }
                else
                {
                    this.Protocol = new ASCIIProtocol();
                }
            }
            _initLines();
        }

        /// <summary>
        /// Create an EDDevice passing in the protocol, connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="protocol"></param>
        public EDDevice(IConnection connection, IIOProtocol protocol) : base(connection, protocol)
        {
            _initLines();
        }

        /// <summary>
        /// Send a command to the Brainboxes device using the supplied protocol.
        /// </summary>
        /// <param name="command">command to send</param>
        /// <returns>Response or null if no response</returns>
        public string SendCommand(string command)
        {
            return this._protocol.SendCommand(command);
        }

        /// <summary>
        /// extending classes should override this method to set the IO lines correctly then call this base method at the end
        /// </summary>
        internal virtual void _initLines()
        {
            List<IOLine> lines = IOLines.ToList();

            //if there are no IOLines then make empty list an assume user might want to add/remove from it manually
            if (this.IOLines == null)
            {
                this.IOLines = new IOList<IOLine>();
                this.Outputs = new IOList<IOLine>();
                this.Inputs = new IOList<IOLine>();
                this.AInputs = new IOList<IOLine>();
                this.AOutputs = new IOList<IOLine>();
            }
            else //make all lists read-only
            {
                this.Outputs = IOLines.ToList()
                    .FindAll(delegate (IOLine l) { return (l.IODirection & IODirection.Output) == IODirection.Output; }).AsIOList();
                this.Inputs = IOLines.ToList()
                    .FindAll(delegate (IOLine l) { return (l.IODirection & IODirection.Input) == IODirection.Input; }).AsIOList();
                this.AOutputs = IOLines.ToList()
                    .FindAll(delegate (IOLine l) { return (l.IODirection & IODirection.AOutput) == IODirection.AOutput; }).AsIOList();
                this.AInputs = IOLines.ToList()
                    .FindAll(delegate (IOLine l) { return (l.IODirection & IODirection.AInput) == IODirection.AInput; }).AsIOList();
            }
            //make read-only and label
            this.IOLines = this.IOLines.AsReadOnly();
            this.Inputs = this.Inputs.AsReadOnly();
            this.Outputs = this.Outputs.AsReadOnly();
            this.AOutputs = this.AOutputs.AsReadOnly();
            this.AInputs = this.AInputs.AsReadOnly();
            this.Outputs.Label = "Outputs";
            this.Inputs.Label = "Inputs";
            this.AOutputs.Label = "AOutputs";
            this.AInputs.Label = "AInputs";
            this.IOLines.Label = "All Device Lines";
        }

        /// <summary>
        /// Factory resets the device,
        /// this can take some time as the current connection is broken and a new on has to be re-established
        /// the connection may not reconnect if the current IP address settings cannot be re-got be the device in default settings mode
        /// </summary>
        public void FactoryReset()
        {
            this._protocol.ResetToFactoryDefaultSettings(); //takes 5 seconds
            this._reconnectAfterResetOrRestart("FACTORY RESET");
        }

        /// <summary>
        /// Power Cycle the device
        /// this can take some time as the current connection is broken and a new on has to be re-established
        /// the function will block until the device is powered up and reconnected to
        /// </summary>
        public void Restart()
        {
            this._protocol.Restart();
            this._reconnectAfterResetOrRestart("RESTART");
        }

        /// <summary>
        /// Disconnects from the device the tries to reconnect each second
        /// over a period of 20 seconds, returns as soon as a connection is established
        /// </summary>
        /// <param name="restartOrReconnect"></param>
        protected void _reconnectAfterResetOrRestart(string restartOrReconnect)
        {
            DateTime startTime = DateTime.UtcNow;
            //through testing I have discovered serial connections take longer to recover than TCP
            //after factory reset. good magic numbers:
            //20 seconds for serial
            //10 seconds for TCP
            //therefore set the maximum to be cautious
            int maxResetWaitTimeSeconds = 20;

            //this.Disconnect();
            while (DateTime.UtcNow < startTime.AddSeconds(maxResetWaitTimeSeconds))
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    Debug.WriteLine("Checking if still connected...");
                    this.Protocol.GetDeviceName(); //see if we can still communicate with the ed
                }
                catch (Exception e)
                {
                    if (!(e is System.IO.InvalidDataException))
                    {
                        this.Disconnect();
                        Debug.WriteLine("Disconnected ... took " + (DateTime.UtcNow - startTime).TotalSeconds);
                        //we're now disconnected the device has powered down
                        break;
                    }
                }
            }
            //try auto reconnect

            DateTime connectStartTime = DateTime.UtcNow;

            int numOfAttempts = 0;
            System.Threading.Thread.Sleep(2500); // heres a magic number wait 2.5 seconds before attempting first reconnect

            do
            {
                try
                {
                    numOfAttempts++;
                    this.Connect();
                    //
                    this.Protocol.GetDeviceName();
                    this.Connection.Stream.Flush();
                    Debug.WriteLine("Re-established a connection to the Device after " + restartOrReconnect + ". Tried for " + (DateTime.UtcNow - startTime).TotalSeconds + " seconds and made " + numOfAttempts + " reconnection attempts.");
                    //if it gets here we're connected therefore return
                    return;
                }
                catch (Exception)
                {
                    Debug.WriteLine("Waiting 1 second seconds for device to " + restartOrReconnect);
                    System.Threading.Thread.Sleep(1000);
                    //still not reset loop until timeout
                }
            }
            while (DateTime.UtcNow < connectStartTime.AddSeconds(maxResetWaitTimeSeconds));

            throw new Exception("Failed to re-establish a connection to the Device after " + restartOrReconnect + ". Tried for " + (DateTime.UtcNow - connectStartTime).TotalSeconds + " seconds and made " + numOfAttempts + " reconnection attempts.");
        }

        /// <summary>
        /// Connect to a Brainboxes Remote IO Device
        /// </summary>
        public override void Connect()
        {
            base.Connect();
            _startPollingForChanges(); //will only start if there are events waiting
        }
      
        /// <summary>
        /// Disconnect from a Brainboxes Remote IO Device
        /// </summary>
        public override void Disconnect()
        {
            _stopPollingForChanges(); //will only stop if its really running run before base method
            base.Disconnect();
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (AInputs.Count > 0 || AOutputs.Count > 0)
            {
                return base.ToString() + " " + this.AInputs.Describe() + " " + this.AOutputs.Describe();
            }
            return base.ToString() + " " + this.Inputs.ToString() + " " + this.Outputs.ToString();
        }

        /// <summary>
        /// Give a complete summary of the EDDevice
        /// </summary>
        /// <returns></returns>
        public override string Describe()
        {
            if (AInputs.Count > 0 || AOutputs.Count > 0)
            {
                return this.ToString() + "\r\n" + this.AInputs.Describe() + "\r\n" + this.AOutputs.Describe();
            }
            return this.ToString() + "\r\n" + this.Inputs.Describe() + "\r\n" + this.Outputs.Describe();
        }

        /// <summary>
        /// IOLines Changed Event
        /// </summary>
        protected event IOLinesChangedEventHandler _ioLinesChanged;

        /// <summary>
        /// IOLine Changed Event
        /// </summary>
        protected event IOLineChangedEventHandler _ioLineChanged;

        /// <summary>
        /// IOLine Rising Edge Event
        /// </summary>
        protected event IOLineChangedEventHandler _ioLineRisingEdge;

        /// <summary>
        /// IOLine Falling Edge Event
        /// </summary>
        protected event IOLineChangedEventHandler _ioLineFallingEdge;

        /// <summary>
        /// IOLine Count Event
        /// </summary>
        protected event IOLineChangedEventHandler _ioLineCount;
        /// <summary>
        /// Analog IO Line Delta changed event
        /// </summary>
        protected event AIOLineChangedEventHandler _aioLineChangedDelta;
        /// <summary>
        /// Analog IO Line Target changed event
        /// </summary>
        protected event AIOLineChangedEventHandler _aioLineChangedTarget;
        /// <summary>
        /// Analog IO Line Target range changed event 
        /// </summary>
        protected event AIOLineChangedEventHandler _aioLineChangedTargetRange;
        /// <summary>
        /// The number of events registered with this EDDevice
        /// </summary>
        protected int _numberOfRegisteredEvents = 0;
        private Object _eventLock = new object();  
        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// one or more IOLines change within a polling interval
        /// This is useful when a state in the program is dependent on 2 or more ioLine states
        /// </summary>
        public event IOLinesChangedEventHandler IOLinesChanged
        {
            add
            {
                lock (_eventLock)
                {
                    _ioLinesChanged += value;
                    _eventAdded();
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _ioLinesChanged -= value;
                    _eventRemoved();
                }
            }
        }

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input or output line changes
        /// </summary>
        public event IOLineChangedEventHandler IOLineChanged
        {
            add
            {
                lock (_eventLock)
                {
                    _ioLineChanged += value;
                    _eventAdded();
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _ioLineChanged -= value;
                    _eventRemoved();
                }
            }


        }

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input or output line goes from 0 -> 1 / low to high / closed to open
        /// </summary>
        public event IOLineChangedEventHandler IOLineRisingEdge
        {
            add
            {
                lock (_eventLock)
                {
                    _ioLineRisingEdge += value;
                    _eventAdded();
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _ioLineRisingEdge -= value;
                    _eventRemoved();
                }
            }
        }

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input or output line goes from 1 -> 0 / high to low / open to closed
        /// </summary>
        public event IOLineChangedEventHandler IOLineFallingEdge
        {
            add
            {
                lock (_eventLock)
                {
                    _ioLineFallingEdge += value;
                    _eventAdded();
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _ioLineFallingEdge -= value;
                    _eventRemoved();
                }
            }
        }

        /// <summary>
        /// Register an event with this handler to be notified when the state of
        /// an input changes increment the count of the IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineCount
        {
            add
            {
                lock (_eventLock)
                {
                    _ioLineCount += value;
                    _eventAdded();
                    //force re-evaluation
                    IOChangeTypes t = this.IOCounterUpdateDirection;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _ioLineCount -= value;
                    _eventRemoved();
                }
            }
        }       
      
        /// <summary>
        /// Register an event with this handler to be notified when the state of an Analog input
        /// goes over the limit specified
        /// </summary>
        /// <param name="function"></param>
        /// <param name="target"></param>       
        public void SubscribeToTargetEvent(ref AIOLineChangedEventHandler function, double target)
        {
            lock (_eventLock)
            {                
                _eventAdded();
            }
        }
        /// <summary>
        /// Register an event with this handler to be notified when the state of an Analog line
        /// changes by larger than the limit specified
        /// </summary>
        /// <param name="function"></param>
        /// <param name="delta"></param>        
        public void SubscribeToDeltaEvent(ref AIOLineChangedEventHandler function, double delta)
        {
            lock (_eventLock)
            {
                _eventAdded();
            }
        }
        /// <summary>
        /// Register an event with this handler to be notified when the state of an Analog line
        /// changes to be within the specified delta range of a specified target value and also when
        /// it changes to be outside of the specified delta range
        /// </summary>
        /// <param name="function"></param>
        /// <param name="target"></param>
        /// <param name="delta"></param>        
        public void SubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function, double target, double delta)
        {
            lock (_eventLock)
            {
                _eventAdded();
            }
        }
        /// <summary>
        /// Unsubscribe a registered delta event with this handler to stop being notified 
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        public void UnsubscribeToDeltaEvent(ref AIOLineChangedEventHandler function)
        {
            lock (_eventLock)
            {
                _eventRemoved();
            }

        }
        /// <summary>
        /// Unsubscribe a registered target event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        public void UnsubscribeToTargetEvent(ref AIOLineChangedEventHandler function)
        {
            lock (_eventLock)
            {
                _eventRemoved();
            }

        }

        /// <summary>
        /// Unsubscribe a registered target range event with this handler to stop being notified 
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        public void UnsubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function)
        {
            lock (_eventLock)
            {
                _eventRemoved();
            }
        }

        /// <summary>
        /// Called whenever an event is added to any of the event collection
        /// </summary>
        protected void _eventAdded()
        {
            _numberOfRegisteredEvents++;
            if (_numberOfRegisteredEvents == 1)
            {
                _startPollingForChanges();
            }
        }

        /// <summary>
        /// Called whenever an event is removed from any of the event collections
        /// </summary>
        protected void _eventRemoved()
        {
            _numberOfRegisteredEvents--;
            if (_numberOfRegisteredEvents == 0)
            {
                _stopPollingForChanges();
            }
        }

        /// <summary>
        /// whether the Digital Input IO Line counter updates on a rising edge or a falling edge
        /// </summary>
        protected IOChangeTypes _IOCounterUpdateDirection = IOChangeTypes.Undefined;

        /// <summary>
        /// whether the Digital Input IO Line counter updates on a rising edge or a falling edge
        /// </summary>
        private IOChangeTypes IOCounterUpdateDirection
        {
            get
            {
                if (_IOCounterUpdateDirection == IOChangeTypes.Undefined)
                {
                    this._IOCounterUpdateDirection = this._protocol.IOCounterUpdateDirection;
                }
                return this._IOCounterUpdateDirection;
            }
        }

        ///// <summary>
        ///// The IOLines which this device has
        ///// </summary>
        // protected IOList<IOLine> IOLines;

        /// <summary>
        /// IOLines indexed by their logical line number as described on the Brainboxes product label
        /// </summary>
        public IOList<IOLine> IOLines { get; protected set; } = new IOList<IOLine>() { };

        ///// <summary>
        ///// Outputs
        ///// </summary>
        //protected IOList<IOLine> Outputs;

        /// <summary>
        /// The devices output Lines (if it has any) indexed by IOLineNumber e.g. DOUT0, DOUT1, DOUT2
        /// </summary>
        public IOList<IOLine> Outputs { get; protected set; }

        ///// <summary>
        ///// Inputs
        ///// </summary>
        //protected IOList<IOLine> Inputs;

        /// <summary>
        /// The devices input Lines (if it has any) indexed by IOLineNumber e.g. DOUT0, DOUT1, DOUT2
        /// </summary>
        public IOList<IOLine> Inputs { get; protected set; }
        /// <summary>
        /// The devices analog input lines (if it has any) index by IOLineNumber e.g AOUT0, AOUT1, AOUT2
        /// </summary>
        public IOList<IOLine> AInputs { get; protected set; }
        /// <summary>
        /// The devices analog output lines (if it has any) indexed by IOLineNumber e.g AOUT0, AOUT1, AOUT2
        /// </summary>
        public IOList<IOLine> AOutputs { get; protected set; }
        /// <summary>
        /// The time between checking the state of the IO lines for event handling, defaults to 250 ms
        /// Also the stale period, when the cached value of the IO line has to be re-queried on the device
        /// </summary>
        public int IOLineCacheTimeout { get { return _ioLineCacheTimeout; } set { _ioLineCacheTimeout = value; if (_pollingTimer != null) { _pollingTimer.Change(value, value); } } }

        #region IIOLineDelegate

        /// <summary>
        /// The polling interval of the Device also the time for which a cached value of an IO line is valid without re-checking the device with a network call
        /// </summary>
        [Obsolete("cacheTimeout is deprecated. Replaced by CacheTimeout")]
        int IIOLineDelegate.cacheTimeout { get { return _ioLineCacheTimeout; } }
        /// <summary>
        /// The polling interval of the Device also the time for which a cached value of an IO line is valid without re-checking the device with a network call
        /// </summary>
        int IIOLineDelegate.CacheTimeout { get { return _ioLineCacheTimeout; } }
        /// <summary>
        /// Locks write access to all ioLines _value , _valueCacheTime , _highLatchedStatus , _highLatchedCacheTime, _lowLatchedStatus, _lowLatchedCacheTime
        /// </summary>
        private object _ioLineWriteLock = new Object();
        [Obsolete("getAllLineStates is deprecated. Replaced by GetAllDigitalLineStates")]
        void IIOLineDelegate.getAllLineStates()
        {
            (this as IIOLineDelegate).GetAllDigitalLineStates();
        }
        void IIOLineDelegate.GetAllDigitalLineStates()
        {
            lock (_ioLineWriteLock)
            {
                int lineStates = _protocol.GetAllDigitalLineStates();
                DateTime cacheTime = DateTime.UtcNow;
                //Ascii protocol gives the lines in logical order
                if (_protocol is ASCIIProtocol)
                {
                    foreach (IOLine line in IOLines)
                    {
                        line._previousValue = line._value;
                        line._value = (lineStates >> line.LogicalNumber) & 1;
                        line._valueCacheTime = cacheTime;
                    }
                }
                //we fudge the modbusTCP protocol to give lines in 2groups,
                //bottom 16 bits are inputs
                //top 16 bits are outputs
                else if (_protocol is ModbusTCPProtocol)
                {
                    foreach (IOLine line in Inputs)
                    {
                        line._previousValue = line._value;
                        line._value = (lineStates >> line.IONumber) & 1;
                        line._valueCacheTime = cacheTime;
                    }
                    foreach (IOLine line in Outputs)
                    {
                        line._previousValue = line._value;
                        line._value = (lineStates >> (line.IONumber + 16)) & 1;
                        line._valueCacheTime = cacheTime;
                    }
                }
            }
        }
        void IIOLineDelegate.GetAllAnalogLineStates()
        {
            lock (_ioLineWriteLock)
            {
                DateTime cacheTime = DateTime.UtcNow;

                // Fetch all states once before the loop to avoid N network calls
                double[] inputStates = AInputs.Count > 0 ? _protocol.GetAllAnalogInputLineStates(IOLines.Count) : null;
                double[] outputStates = AOutputs.Count > 0 ? _protocol.GetAllAnalogOutputLineStates() : null;

                foreach (IOLine line in IOLines)
                {
                    if (line.IODirection == IODirection.AInput && inputStates != null)
                    {
                        line._previousAValue = line._aValue;
                        line._aValue = inputStates[line.IONumber];
                        line._valueCacheTime = cacheTime;
                    }
                    if (line.IODirection == IODirection.AOutput && outputStates != null)
                    {
                        line._previousAValue = line._aValue;
                        line._aValue = outputStates[line.IONumber];
                        line._valueCacheTime = cacheTime;
                    }
                }
            }
        }
        [Obsolete("setLineState is deprecated. Replaced by SetDigitalOutputLineState")]
        void IIOLineDelegate.setLineState(IOLine line, int value)
        {
            (this as IIOLineDelegate).SetDigitalOutputLineState(line, value);
        }
        void IIOLineDelegate.SetDigitalOutputLineState(IOLine line, int value)
        {
            lock (_ioLineWriteLock)
            {
                line._previousValue = line._value;
                _protocol.SetDigitalOutputLineState(line.IONumber, value);
                line._value = value;
                line._valueCacheTime = DateTime.UtcNow;

                IOChangeTypes currentChangeType = line.MostRecentChangeType;

                if (currentChangeType == IOChangeTypes.Undefined || currentChangeType == IOChangeTypes.NoChange) return;

                if (_ioLineChanged != null)
                {
                    _ioLineChanged(line, this, currentChangeType); //call the change handler
                }
                if (_ioLinesChanged != null)
                {
                    List<IOLine> lines = new List<IOLine>();
                    lines.Add(line);
                    _ioLinesChanged(lines, this); //call the change handler
                }
                if (currentChangeType == IOChangeTypes.FallingEdge && _ioLineFallingEdge != null)
                {
                    _ioLineFallingEdge(line, this, currentChangeType);
                }
                if (currentChangeType == IOChangeTypes.RisingEdge && _ioLineRisingEdge != null)
                {
                    _ioLineRisingEdge(line, this, currentChangeType);
                }
            }
        }
        void IIOLineDelegate.SetAnalogLineState(IOLine line, double value)
        {
            double difference;
            double previousAValue;
            double currentAValue;
            KeyValuePair<Delegate, RegisteredEvents>[] deltaEvents;
            KeyValuePair<Delegate, RegisteredEvents>[] targetEvents;
            KeyValuePair<Delegate, RegisteredEvents>[] targetRangeEvents;

            lock (_ioLineWriteLock)
            {
                difference = line._aValue - value;
                line._previousAValue = line._aValue;
                _protocol.SetAnalogOutputLineState(line.IONumber, value);
                line._aValue = value;
                line._valueCacheTime = DateTime.UtcNow;
                previousAValue = line._previousAValue;
                currentAValue = line._aValue;

                // Copy dictionaries under lock to prevent modification during iteration
                lock (line._analogEventLock)
                {
                    deltaEvents = new List<KeyValuePair<Delegate, RegisteredEvents>>(line._aioLineChangedDeltaEvents).ToArray();
                    targetEvents = new List<KeyValuePair<Delegate, RegisteredEvents>>(line._aioLineChangedTargetEvents).ToArray();
                    targetRangeEvents = new List<KeyValuePair<Delegate, RegisteredEvents>>(line._aioLineChangedTargetRangeEvents).ToArray();
                }
            }

            // Invoke handlers outside locks to prevent deadlocks
            foreach (KeyValuePair<Delegate, RegisteredEvents> deltaEvent in deltaEvents)
            {
                if (Math.Abs(deltaEvent.Value.Delta) < Math.Abs(difference))
                {
                    deltaEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Delta);
                }
            }
            foreach (KeyValuePair<Delegate, RegisteredEvents> targetEvent in targetEvents)
            {
                //If the previous value was below the target and is still below the target do nothing
                //If the previous value was below the target and the current value is above the target this is ABOVE state
                //If the previous value was above the target and is still above the target do nothing
                //If the previous value was above the target and the current value is below the target this is BELOW state
                if ((targetEvent.Value.Target < previousAValue && targetEvent.Value.Target > currentAValue))
                {
                    targetEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Below);
                }
                else if (targetEvent.Value.Target > previousAValue && targetEvent.Value.Target < currentAValue)
                {
                    targetEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Above);
                }
            }
            foreach (KeyValuePair<Delegate, RegisteredEvents> targetRangeEvent in targetRangeEvents)
            {
                //Offset values by target to simplify range comparison
                double previousValue = previousAValue - targetRangeEvent.Value.Target;
                double currentValue = currentAValue - targetRangeEvent.Value.Target;
                double absDelta = Math.Abs(targetRangeEvent.Value.Delta);
                //If the previous value was in range and is still in range do nothing
                //If the previous value was in range and the current value is not in range this is EXIT state
                //If the previous value wasn't in range and is still not in range do nothing
                //If the previous value wasn't in range and the current value is in range this is ENTER state
                if ((previousAValue < (targetRangeEvent.Value.Target - absDelta) && currentAValue > (targetRangeEvent.Value.Target + absDelta))
                || (previousAValue > (targetRangeEvent.Value.Target + absDelta) && currentAValue < (targetRangeEvent.Value.Target - absDelta)))
                {
                    targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Enter);
                    targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Exit);
                }
                else if (Math.Abs(previousValue) > absDelta && Math.Abs(currentValue) < absDelta)
                {
                    targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Enter);
                }
                else if (Math.Abs(previousValue) < absDelta && Math.Abs(currentValue) > absDelta)
                {
                    targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Exit);
                }
            }
        }
        [Obsolete("getAllInputLowLatchedStatus is deprecated. Replaced by GetAllDigitalInputLowLatchedStatus")]
        void IIOLineDelegate.getAllInputLowLatchedStatus()
        {
            (this as IIOLineDelegate).GetAllDigitalInputLowLatchedStatus();
        }
        void IIOLineDelegate.GetAllDigitalInputLowLatchedStatus()
        {
            lock (_ioLineWriteLock)
            {
                int lineStates = _protocol.GetAllLatchedLowDigitalInputStates();
                DateTime cacheTime = DateTime.UtcNow;
                foreach (IOLine line in IOLines)
                {
                    line._lowLatchedStatus = ((lineStates >> line.LogicalNumber) & 1) == 1;
                    line._valueCacheTime = cacheTime;
                }
                //TODO:async fire and forget
                this._protocol.ClearAllLatchedDigitalInputs();
            }
        }
        [Obsolete("getAllInputHighLatchedStatus is deprecated. Replaced by GetAllDigitalInputHighLatchedStatus")]
        void IIOLineDelegate.getAllInputHighLatchedStatus()
        {
            (this as IIOLineDelegate).GetAllDigitalInputHighLatchedStatus();
        }
        void IIOLineDelegate.GetAllDigitalInputHighLatchedStatus()
        {
            lock (_ioLineWriteLock)
            {
                int lineStates = _protocol.GetAllLatchedHighDigitalInputStates();
                DateTime cacheTime = DateTime.UtcNow;
                foreach (IOLine line in IOLines)
                {
                    line._highLatchedStatus = ((lineStates >> line.LogicalNumber) & 1) == 1;
                    line._valueCacheTime = cacheTime;
                }
                //TODO:async fire and forget
                this._protocol.ClearAllLatchedDigitalInputs();
            }
        }
        [Obsolete("getLineCount is deprecated. Replaced by GetDigitalInputCount")]
        void IIOLineDelegate.getLineCount(IOLine iOLine)
        {
            (this as IIOLineDelegate).GetDigitalInputLineCount(iOLine);
        }
        void IIOLineDelegate.GetDigitalInputLineCount(IOLine ioLine)
        {
            lock (_ioLineWriteLock)
            {
                ioLine._previousCount = ioLine._count;
                ioLine._count = _protocol.GetDigitalInputLineCount(ioLine.IONumber);
                ioLine._countCacheTime = DateTime.UtcNow;
            }
        }
        void IIOLineDelegate.ClearLineCount(IOLine iOLine)
        {
            lock (_ioLineWriteLock)
            {
                _protocol.ClearDigitalInputLineCount(iOLine.IONumber);
                iOLine._previousCount = iOLine._count;
                iOLine._count = 0;
                iOLine._countCacheTime = DateTime.UtcNow;
            }
        }

        #endregion IIOLineDelegate

        #region Device Polling

        /// <summary>
        /// The timer thread used for polling IO
        /// </summary>
        protected Timer _pollingTimer;

        /// <summary>
        /// flag to indicate whether the pollingThread should be running
        /// </summary>
        protected volatile bool _threadShouldBeRunning = false;

        /// <summary>
        /// Lock for synchronizing polling timer start/stop
        /// </summary>
        private readonly object _pollingLock = new object();

        /// <summary>
        /// cache timeout is also the polling interval
        /// </summary>
        protected int _ioLineCacheTimeout = 250;

        /// <summary>
        /// Spins up a background timing thread which polls the Brainboxes ED Device periodically at ioLineCacheTimeout
        /// </summary>
        protected void _startPollingForChanges()
        {
            // Check IsConnected before acquiring lock to avoid potential deadlock
            // (IsConnected may trigger I/O which could block)
            if (!IsConnected) return;

            lock (_pollingLock)
            {
                if (Inputs == null || Outputs == null || AInputs == null || AOutputs == null) return;
                if (_threadShouldBeRunning || !IsConnected) return;
                //before starting make sure that the IO lines do not have an unset state (-1) and latches are cleared

                //if (_threadShouldBeRunning || _numberOfRegisteredEvents == 0 || !IsConnected) return;
                if (Inputs.Count != 0 || Outputs.Count != 0)
                {
                    if (_numberOfRegisteredEvents == 0) return;
                    (this as IIOLineDelegate).GetAllDigitalLineStates();
                    if (Inputs.Count > 0 && _protocol is ASCIIProtocol)
                    {
                        this._protocol.ClearAllLatchedDigitalInputs();
                    }
                }

                if (AInputs.Count != 0 || AOutputs.Count != 0)
                {
                    if (_numberOfRegisteredEvents == 0) return;
                    (this as IIOLineDelegate).GetAllAnalogLineStates();
                }

                _threadShouldBeRunning = true;

                //use Timeout.Infinite for period to prevent overlapping callbacks
                //the timer is re-armed at the end of _pollingThreadFunc
                _pollingTimer = new Timer(new TimerCallback(_pollingThreadFunc), null, 0, System.Threading.Timeout.Infinite);
            }
        }

        /// <summary>
        /// Stop and dispose the background thread which is polling for changes on the Brainboxes ED Device
        /// </summary>
        protected void _stopPollingForChanges()
        {
            Timer timer;
            lock (_pollingLock)
            {
                _threadShouldBeRunning = false;
                timer = _pollingTimer;
                _pollingTimer = null;
            }

            if (timer != null)
            {
                // Use a WaitHandle to ensure the timer callback has completed before returning
                // This prevents race conditions where the socket is closed while the callback is still running
                using (var waitHandle = new System.Threading.ManualResetEvent(false))
                {
                    if (timer.Dispose(waitHandle))
                    {
                        // Wait for up to 5 seconds for any in-progress callback to complete
                        waitHandle.WaitOne(5000);
                    }
                }
            }
        }

        /// <summary>
        /// This is the function which is run on the background thread which Polls the ED Device for changes and then dispatches events
        /// </summary>
        /// <param name="state"></param>
        protected void _pollingThreadFunc(object state)
        {
            if (_threadShouldBeRunning && this.IsConnected)
            {
                try
                {
                    if (Outputs.Count > 0 || Inputs.Count > 0)
                    {
                        if (_protocol is ASCIIProtocol)
                        {
                            _pollIOASCII();
                        }
                        else if (_protocol is ModbusTCPProtocol)
                        {
                            _pollIOModbusTCP();
                        }
                    }
                    //if in future we have both digital and analog on one device make this an if, instead of an if else
                    if (AOutputs.Count > 0 || AInputs.Count > 0)
                    {
                        _pollAnalog();
                    }

                    // Dispatch events only after a successful poll to avoid sending stale data
                    if (Inputs.Count > 0 || Outputs.Count > 0)
                    {
                        _dispatchDigitalEvents();
                    }
                    else if (AOutputs.Count > 0 || AInputs.Count > 0)
                    {
                        _dispatchAnalogEvents();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("EDDevice: _pollingThreadFunc: " + e.Message);
                }
            }

            // Re-arm the timer for the next poll (one-shot to prevent overlap)
            lock (_pollingLock)
            {
                if (_threadShouldBeRunning && _pollingTimer != null)
                {
                    try
                    {
                        _pollingTimer.Change(_ioLineCacheTimeout, System.Threading.Timeout.Infinite);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Timer was disposed between check and Change call
                    }
                }
            }
        }
        /// <summary>
        /// Get the Status of the analog IO lines
        /// Update all the Input and the Output values
        /// </summary>
        protected void _pollAnalog()
        {
            //update all ioLines
            lock (_ioLineWriteLock)
            {
                DateTime newCacheTime = DateTime.UtcNow;
                if (AInputs.Count > 0)
                {
                    double[] newStates = this._protocol.GetAllAnalogInputLineStates(IOLines.Count);
                    foreach (IOLine line in IOLines)
                    {
                        line._previousAValue = line._aValue;
                        line._aValue = newStates[line.LogicalNumber];
                        line._valueCacheTime = newCacheTime;
                    }
                }
                if (AOutputs.Count > 0)
                {
                    double[] newStates = this._protocol.GetAllAnalogOutputLineStates();
                    foreach (IOLine line in IOLines)
                    {
                        line._previousAValue = line._aValue;
                        line._aValue = newStates[line.LogicalNumber];
                        line._valueCacheTime = newCacheTime;
                    }
                }
            }
        }

        /// <summary>
        /// Get the status of the Digital IO lines
        /// first check the current values
        /// if there are any inputs also check the latched values
        /// if the latches have been triggered reset them
        /// </summary>
        protected void _pollIOASCII()
        {
            // prevents other threads writing to the ioLines memory block while polling in progress
            lock (_ioLineWriteLock)
            {
                Debug.WriteLine("** Polling IO **");

                DateTime newCacheTime = DateTime.UtcNow;
                int newState = this._protocol.GetAllDigitalLineStates();
                int highLatchedStatus = 0;
                int lowLatchedStatus = 0;
                if (Inputs.Count > 0)
                {
                    //TODO: make async would speed up these 2/3 calls
                    // only applies to devices with digital inputs
                    highLatchedStatus = this._protocol.GetAllLatchedHighDigitalInputStates();
                    lowLatchedStatus = this._protocol.GetAllLatchedLowDigitalInputStates();
                    //AND the 2 responses together to see if we need to clear the latches see below for reasoning
                    if ((highLatchedStatus & lowLatchedStatus) > 0)
                    {
                        this._protocol.ClearAllLatchedDigitalInputs();
                    }
                }
                //update the values on each line before dispatching events so that when the user
                //receives the events all lines will be synchronized
                foreach (IOLine line in IOLines)
                {
                    line._previousValue = line._value;
                    // the logicalNumber represents the bit position in the GetAllLineStates response
                    line._value = (newState >> line.LogicalNumber) & 1;
                    line._valueCacheTime = newCacheTime;

                    if (line.IODirection == IODirection.Input)
                    {
                        line._highLatchedStatus = ((highLatchedStatus >> line.LogicalNumber) & 1) == 1;
                        line._lowLatchedStatus = ((lowLatchedStatus >> line.LogicalNumber) & 1) == 1;
                        line._highLatchedCacheTime = newCacheTime;
                        line._lowLatchedCacheTime = newCacheTime;
                    }
                }
            }
        }

        /// <summary>
        /// Get the status of the IO lines
        /// first check the current values
        /// if there are any inputs also check the counters
        /// </summary>
        protected void _pollIOModbusTCP()
        {
            // prevents other threads writing to the ioLines memory block while polling in progress
            lock (_ioLineWriteLock)
            {
                Debug.WriteLine("** Polling IO **");

                DateTime newCacheTime = DateTime.UtcNow;
                int lineStates = this._protocol.GetAllDigitalLineStates();

                if (Inputs.Count > 0)
                {
                    //TODO: it appears modbus allows us to get all counts at once
                    //this would be a way to detect changes between polling intervals in one go
                }

                //update the values on each line before dispatching events so that when the user
                //receives the events all lines will be synchronized
                foreach (IOLine line in Inputs)
                {
                    line._previousValue = line._value;
                    line._value = (lineStates >> line.IONumber) & 1;
                    line._valueCacheTime = newCacheTime;
                }
                foreach (IOLine line in Outputs)
                {
                    line._previousValue = line._value;
                    line._value = (lineStates >> (line.IONumber + 16)) & 1;
                    line._valueCacheTime = newCacheTime;
                }
            }
        }

        /// <summary>
        /// Only Dispatch events after the state of all the digital IOLines has been updated by the _pollIO function
        /// otherwise event handlers may experience inconsistencies in the digital IOLine data
        /// </summary>
        protected void _dispatchDigitalEvents()
        {
            List<IOLine> changedLines = new List<IOLine>();
            //dispatch events
            foreach (IOLine line in IOLines)
            {
                IOChangeTypes currentChange = line.MostRecentChangeType;

                //go to next IO line
                if (currentChange == IOChangeTypes.NoChange) continue;

                changedLines.Add(line);
                //any changes
                if (_ioLineChanged != null)
                {
                    _ioLineChanged(line, this, currentChange); //call the change handler
                }
                //falling edge low frequency
                if (_ioLineFallingEdge != null && currentChange == IOChangeTypes.FallingEdge)
                {
                    _ioLineFallingEdge(line, this, currentChange);
                }
                //rising edge low frequency
                if (_ioLineRisingEdge != null && currentChange == IOChangeTypes.RisingEdge)
                {
                    _ioLineRisingEdge(line, this, currentChange);
                }

                //high frequency
                if (currentChange == IOChangeTypes.Latched)
                {
                    //falling edge
                    if (line._value == 1) // starts with falling-edge , ends with rising
                    {
                        if (_ioLineFallingEdge != null)
                        {
                            _ioLineFallingEdge(line, this, IOChangeTypes.Latched);
                        }
                        if (_ioLineRisingEdge != null)
                        {
                            _ioLineRisingEdge(line, this, IOChangeTypes.Latched);
                        }
                    }
                    //rising edge
                    else// starts with rising-edge , ends with falling
                    {
                        if (_ioLineRisingEdge != null)
                        {
                            _ioLineRisingEdge(line, this, IOChangeTypes.Latched);
                        }
                        if (_ioLineFallingEdge != null)
                        {
                            _ioLineFallingEdge(line, this, IOChangeTypes.Latched);
                        }
                    }
                }

                //counter
                if (_ioLineCount != null && (currentChange == _IOCounterUpdateDirection || currentChange == IOChangeTypes.Latched))
                {
                    _ioLineCount(line, this, _IOCounterUpdateDirection);
                }
            }
            //if there were one or more changes of any type send the collection of lines which changed to the handler function
            if (_ioLinesChanged != null && changedLines.Count > 0)
            {
                _ioLinesChanged(changedLines, this);
            }
        }
        /// <summary>
        /// Only Dispatch events after the state of all the analog IO lines has been updated by the _pollAnalog function
        /// other event handlers may experience inconsistencies in the analog IO line data
        /// </summary>
        protected void _dispatchAnalogEvents()
        {
            foreach (IOLine line in IOLines)
            {
                double change = line._previousAValue - line._aValue;
                double previousAValue = line._previousAValue;
                double currentAValue = line._aValue;

                // Copy dictionaries under lock to prevent modification during iteration
                KeyValuePair<Delegate, RegisteredEvents>[] deltaEvents;
                KeyValuePair<Delegate, RegisteredEvents>[] targetEvents;
                KeyValuePair<Delegate, RegisteredEvents>[] targetRangeEvents;

                lock (line._analogEventLock)
                {
                    deltaEvents = new List<KeyValuePair<Delegate, RegisteredEvents>>(line._aioLineChangedDeltaEvents).ToArray();
                    targetEvents = new List<KeyValuePair<Delegate, RegisteredEvents>>(line._aioLineChangedTargetEvents).ToArray();
                    targetRangeEvents = new List<KeyValuePair<Delegate, RegisteredEvents>>(line._aioLineChangedTargetRangeEvents).ToArray();
                }

                foreach (KeyValuePair<Delegate, RegisteredEvents> deltaEvent in deltaEvents)
                {
                    if (Math.Abs(deltaEvent.Value.Delta) < Math.Abs(change))
                    {
                        deltaEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Delta);
                    }
                }

                foreach (KeyValuePair<Delegate, RegisteredEvents> targetEvent in targetEvents)
                {
                    //If the previous value was below the target and is still below the target do nothing
                    //If the previous value was below the target and the current value is above the target this is ABOVE state
                    //If the previous value was above the target and is still above the target do nothing
                    //If the previous value was above the target and the current value is below the target this is BELOW state
                    if ((targetEvent.Value.Target < previousAValue && targetEvent.Value.Target > currentAValue))
                    {
                        targetEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Below);
                    }
                    else if (targetEvent.Value.Target > previousAValue && targetEvent.Value.Target < currentAValue)
                    {
                        targetEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Above);
                    }
                }

                foreach (KeyValuePair<Delegate, RegisteredEvents> targetRangeEvent in targetRangeEvents)
                {
                    //Offset everything by target to simplify the code since I only need to worry about positive values
                    double previousValue = previousAValue - targetRangeEvent.Value.Target;
                    double currentValue = currentAValue - targetRangeEvent.Value.Target;
                    //If the previous value was in range and is still in range do nothing
                    //If the previous value was in range and the current value is not in range this is EXIT state
                    //If the previous value wasn't in range and is still not in range do nothing
                    //If the previous value wasn't in range and the current value is in range this is ENTER state
                    //Edge case: If the previous value was below the range and the current value is above the range, the event should be triggered for both enter and exit
                    if ((previousAValue < (targetRangeEvent.Value.Target - Math.Abs(targetRangeEvent.Value.Delta)) && currentAValue > (targetRangeEvent.Value.Target + Math.Abs(targetRangeEvent.Value.Delta)))
                        || (previousAValue > (targetRangeEvent.Value.Target + Math.Abs(targetRangeEvent.Value.Delta)) && currentAValue < (targetRangeEvent.Value.Target - Math.Abs(targetRangeEvent.Value.Delta))))
                    {
                        targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Enter);
                        targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Exit);
                    }
                    else if ((Math.Abs(previousValue) > Math.Abs(targetRangeEvent.Value.Delta) && Math.Abs(currentValue) < Math.Abs(targetRangeEvent.Value.Delta)))
                    {
                        targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Enter);
                    }
                    else if ((Math.Abs(previousValue) < Math.Abs(targetRangeEvent.Value.Delta) && Math.Abs(currentValue) > Math.Abs(targetRangeEvent.Value.Delta)))
                    {
                        targetRangeEvent.Value.Invoke(line, this, currentAValue, AIOChangeTypes.Exit);
                    }
                }
            }
        }

        #endregion Device Polling

        /// <summary>
        /// Dispose of this Brainboxes Remote IO Device
        /// </summary>
        /// <param name="itIsSafeToAlsoFreeManagedObjects"></param>
        protected override void Dispose(Boolean itIsSafeToAlsoFreeManagedObjects)
        {
            if (itIsSafeToAlsoFreeManagedObjects)
            {
                _stopPollingForChanges();
            }
            base.Dispose(itIsSafeToAlsoFreeManagedObjects);
        }

        /// <summary>
        /// Supply a connection IP address or com port and the correct ED device will be created and returned.
        /// </summary>
        /// <param name="ipAddressOrComPort">Either an IP address e.g. "192.168.0.1" or a COM port name e.g. "COM3".</param>
        /// <param name="portOrBaudRate">Optional. Either the IP port number (e.g. 9500 for ASCII, 502 for Modbus) or the COM port baud rate (e.g. 115200). If not supplied, the port is determined by querying the device.</param>
        /// <param name="timeout">The connection timeout in milliseconds. Defaults to 2000ms.</param>
        /// <returns>An <see cref="EDDevice"/> subclass matching the detected device type, or <c>null</c> if the device type cannot be determined.</returns>
        /// <example>
        /// <code>
        /// using (EDDevice ed = EDDevice.Create("192.168.0.5"))
        /// {
        ///     // Connection is opened automatically
        ///     Console.WriteLine(ed.Inputs[0].Value);  // Read digital input 0
        ///     ed.Outputs[0].Value = 1;                // Set digital output 0 high
        /// } // Connection is closed and object disposed after using block
        /// </code>
        /// </example>
        public static EDDevice Create(string ipAddressOrComPort, int portOrBaudRate = 0, int timeout = 2000)
        {
            if (ipAddressOrComPort.StartsWith("COM"))
            {
                return CreateComDevice(ipAddressOrComPort, portOrBaudRate, timeout);
            }
            string ip = ipAddressOrComPort;
            XmlDocument xmlDoc;
            Type edDeviceClass = DeviceTypeFromIP(ip, out xmlDoc);
            string protocol = "";
            int protocolPort = 0;
            if (edDeviceClass == null)
            {
                return null;
            }
            else if(edDeviceClass == typeof(BB400))
            {
                protocol = "ASCII";
                protocolPort = 9500;
            }
            else
            {
                //query XML doc for protocol and port number
                protocol = xmlDoc.GetElementsByTagName("currpro")[0].InnerText;
                protocolPort = portOrBaudRate > 0 ? portOrBaudRate : Convert.ToInt32(xmlDoc.GetElementsByTagName(protocol == "ASCII" ? "dtcpport" : "mtcpport")[0].InnerText);
                // string deviceName = xmlDoc.GetElementsByTagName("devname")[0].InnerText;
            }

            IProtocol p;
            if (protocol.StartsWith("Modbus")) //ternary operator wouldn't work here
            {
                p = new ModbusTCPProtocol();
            }
            else
            {
                p = new ASCIIProtocol();
            }
            IConnection c = new TCPConnection(ip, protocolPort, timeout);

            c.Connect();

            return System.Activator.CreateInstance(edDeviceClass, c, p) as EDDevice;
        }

        /// <summary>
        /// Create an EDDevice using COM port and therefore ASCII protocol
        /// </summary>
        /// <param name="comPort"></param>
        /// <param name="baudRate"></param>
        /// <param name="timeout"></param>
        /// <param name="asciiAddress"></param>
        /// <returns></returns>
        private static EDDevice CreateComDevice(string comPort, int baudRate = 0, int timeout = 2000, string asciiAddress = "01")
        {
            IConnection connection = Brainboxes.IO.Connection.Create(comPort, baudRate);

            //its a com port & therefore ASCII protocol, query device over ASCII to figure out its type
            IIOProtocol protocol = new ASCIIProtocol();
            connection.Connect();
            protocol.Stream = connection.Stream;
            string response = protocol.SendCommand("$" + asciiAddress + "M0").Remove(0, 3).Replace("-", "");

            //try to cast to an EDDevice
            Type edDeviceClass = Type.GetType("Brainboxes.IO." + response, false);

            if (edDeviceClass != null)
            {
                return System.Activator.CreateInstance(edDeviceClass, new object[] { connection, protocol }) as EDDevice;
            }

            //get the device characteristics >LLLL>IOIO
            response = protocol.SendCommand("$" + asciiAddress + "M2");

            EDDevice ed = new EDDevice(connection, protocol);

            if (response.StartsWith(">"))
            {
                string[] dataString = response.Split('>');
                int ioLines = Convert.ToInt32(dataString[1], 16);
                int isInputOrOutput = Convert.ToInt32(dataString[2], 16);

                int directionChangedOnLine = 0;
                int currentDirection = 1; //inputs come first then outputs
                int lineNumber = 0;
                while (ioLines > 0)
                {
                    if ((ioLines & 1) == 1) //if there is an IO line
                    {
                        IODirection direction = (isInputOrOutput & 1) == 1 ? IODirection.Input : IODirection.Output;
                        if ((isInputOrOutput & 1) != currentDirection)
                        {
                            currentDirection = (isInputOrOutput & 1);
                            directionChangedOnLine = lineNumber;
                        }
                        ed.IOLines.Add(new IOLine(lineNumber, lineNumber - directionChangedOnLine, direction, IOType.Digital, ed));
                    }
                    lineNumber++;
                    ioLines >>= 1;
                    isInputOrOutput >>= 1;
                }
            }

            return ed;
        }
    }
}