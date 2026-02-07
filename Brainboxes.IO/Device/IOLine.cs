using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleToAttribute("Brainboxes.IO.Tests")]
namespace Brainboxes.IO
{
    /// <summary>
    /// IODirection either input or output, for lines which can be both input and output the flags can be combined e.g. IODirection.Input | IODirection.Output
    /// </summary>
    [Flags]
    public enum IODirection
    {
        /// <summary>
        /// Input
        /// </summary>
        Input = 1,
        /// <summary>
        /// Output
        /// </summary>
        Output = 2,
        /// <summary>
        /// Analog input
        /// </summary>
        AInput = 4,
        /// <summary>
        /// Analog output
        /// </summary>
        AOutput = 8
    }

    /// <summary>
    /// A type of change which can occur to a Digital IOLine
    /// </summary>
    [Flags]
    public enum IOChangeTypes
    {
        /// <summary>
        /// No Change occurred
        /// </summary>
        NoChange = 0,
        /// <summary>
        /// Rising Edge
        /// Digital Input went from low to high or 0 to 1
        /// Digital Output/Relay went from open to closed or 0 to 1
        /// </summary>
        RisingEdge = 1,
        /// <summary>
        /// Falling Edge
        /// Digital Input went from high to low or 1 to 0
        /// Digital Output/Relay went from closed to open or 1 to 0
        /// </summary>
        FallingEdge = 2,
        /// <summary>
        /// Within the sample interval the IOLine has had 1 or more falling edges and 1 or more rising edges
        /// </summary>
        Latched = 7, //means it has a falling and rising edge
        /// <summary>
        /// undefined
        /// </summary>
        Undefined = 99,
    }
    /// <summary>
    /// A type of change which can occur to a Analog IOLine
    /// </summary>
    public enum AIOChangeTypes
    {
        /// <summary>
        /// Delta event only:
        /// When the differnce between the previous sampled value and the current sampled value is more than the delta value specified
        /// </summary>
        Delta = 0,
        /// <summary>
        /// Target event only:
        /// When the previous sampled value was above the target value and the current sampled value is below the target value
        /// </summary>
        Below = 1,
        /// <summary>
        /// Target event only:
        /// When the previous sampled value was below the target value and the current sampled value is above the target value
        /// </summary>
        Above = 2,
        /// <summary>
        /// Target range event only:
        /// When the previous sampled value was outside of the delta range of the target value and the current sampled value
        /// goes inside the delta range of the target value
        /// </summary>
        Enter = 3,
        /// <summary>
        /// Target range event only:
        /// When the previous sampled value was inside of the delta range of the target value and the current sampled value
        /// goes outside the delta range of the target value
        /// </summary>
        Exit = 4,
    }
    /// <summary>
    /// The type of IOLine
    /// </summary>
    public enum IOType
    {
        /// <summary>
        /// Digital IOLine
        /// </summary>
        Digital,
        /// <summary>
        /// Analog IOLine
        /// </summary>
        Analog,
        /// <summary>
        /// Relay, note from a programming perspective this is the same as a digital input line
        /// </summary>
        Relay = Digital, //same from a programming point of view: on off switch
    }

    /// <summary>
    /// Represents a single IO line (digital or analog, input or output) on a Brainboxes
    /// <see cref="EDDevice"/>. Access IO lines via the <see cref="EDDevice.Inputs"/>,
    /// <see cref="EDDevice.Outputs"/>, or <see cref="EDDevice.IOLines"/> collections.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Value caching:</strong> IO reads are batched — reading any single line's
    /// <see cref="Value"/> or <see cref="AValue"/> refreshes <em>all</em> lines of the same
    /// type in one network call. Values are cached for <see cref="EDDevice.IOLineCacheTimeout"/>
    /// milliseconds (default 10 ms). Within that window, repeated reads return the cached value
    /// without additional network traffic.
    /// </para>
    /// <para>
    /// <strong>Write suppression:</strong> Setting an output line to its current cached value
    /// within the cache window is a no-op — no command is sent to the device.
    /// </para>
    /// <para>
    /// <strong>Events:</strong> Digital lines support <see cref="IOLineChanged"/>,
    /// <see cref="IOLineRisingEdge"/>, <see cref="IOLineFallingEdge"/>, and
    /// <see cref="IOLineCount"/> events. Analog lines support delta, target, and
    /// target-range events via <see cref="SubscribeToDeltaEvent"/>,
    /// <see cref="SubscribeToTargetEvent"/>, and <see cref="SubscribeToTargetRangeEvent"/>.
    /// Events are driven by the <see cref="EDDevice"/> polling timer.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using Brainboxes.IO;
    ///
    /// using (EDDevice ed = EDDevice.Create("192.168.0.5"))
    /// {
    ///     // Read a digital input
    ///     int inputState = ed.Inputs[0].Value;   // 1 = HIGH, 0 = LOW
    ///
    ///     // Set a digital output
    ///     ed.Outputs[0].Value = 1;               // 1 = CLOSED, 0 = OPEN
    ///
    ///     // Toggle an output
    ///     int newState = ed.Outputs[0].Toggle();
    ///
    ///     // Read an analog input (ED-549/ED-560)
    ///     double voltage = ed.Inputs[0].AValue;
    ///
    ///     // Monitor a specific line for changes
    ///     ed.Inputs[0].IOLineChanged += (line, device, changeType) =>
    ///     {
    ///         Console.WriteLine($"Line {line.IONumber} changed: {changeType}");
    ///     };
    /// }
    /// </code>
    /// </example>
    public class IOLine
    {
        //GetLineValueDelegate getDelegate, SetLineValueDelegate setDelegate = null
        internal IOLine(int logicalNumber, int ioNumber, IODirection ioDirection, IOType ioType, IIOLineDelegate ioLineDelegate)
        {

            this.LogicalNumber = logicalNumber;
            this.IONumber = ioNumber;
            this.IODirection = ioDirection;
            this.IOType = ioType;
            this._ioLineDelegate = ioLineDelegate;
        }

        /// <summary>
        /// User definable label for the io line to help when debugging
        /// </summary>
        public string Label { get; set; }


        /// <summary>
        /// The IO Line Number from 0 to 15 as defined on the case
        /// </summary>
        public readonly int LogicalNumber;

        /// <summary>
        /// The IO Number from 0 to 15 e.g DOUT2, DIN1, Relay3
        /// </summary>
        public readonly int IONumber;

        /// <summary>
        /// Whether the IO line is an input or an output
        /// </summary>
        public readonly IODirection IODirection;

        /// <summary>
        /// Whether the line is digital or analog
        /// </summary>
        public readonly IOType IOType;

        /// <summary>
        /// The most recent type of change to occur to the IOLine examples:
        /// a. if the previous value seen was 1 and the current value is 0 then the change type will be IOChangeType.FallingEdge.
        /// b. if the device has just been turned on then the change type will be IOChangeType.Undefined
        /// c. if the line has gone from 1 to 0 and back to 1 within the <see cref="EDDevice.IOLineCacheTimeout"/> the change type will be IOChangeType.Latched
        /// d. if the line has not changed within the last <see cref="EDDevice.IOLineCacheTimeout"/> the change type will be IOChangeType.NoChange
        /// 
        /// The method will only return meaningful results if there are event handlers attached to the EDDevice
        /// </summary>
        public IOChangeTypes MostRecentChangeType
        {
            get
            {
                if (this._previousValue == -1)
                {
                    return IOChangeTypes.Undefined;
                }
                //simple method: low frequency changes > pollingInterval (_ioLineCacheTimeout)
                //only way to detect change on Outputs (not initiated by this program) is to compare the current to previous value, also can detect input change
                if (this._value != this._previousValue)
                {
                    return (this._value == 1 ? IOChangeTypes.RisingEdge : IOChangeTypes.FallingEdge);
                }
                // more complicated method: high frequency changes < polling interval ( _ioLineCacheTimeout)
                // additional way to detect changes for inputs e.g. if in the polling interval the line has gone: high, low and back to high the first method would not pick it up
                // in this scenario we can check the latched values: see Karnaugh map
                // ------------------------
                // value | hl | ll | change | comment
                // ------------------------
                //    0  | 0  |  0 |  0  | not possible
                //    0  | 0  |  1 |  0  | no change 
                //    0  | 1  |  0 |  1  | not possible
                //    0  | 1  |  1 |  1
                //    1  | 0  |  0 |  0  | not possible
                //    1  | 0  |  1 |  1  | not possible
                //    1  | 1  |  0 |  0  | no change
                //    1  | 1  |  1 |  1
                //which reduces to
                else if (this.IODirection == IODirection.Input && this._highLatchedStatus && this._lowLatchedStatus)
                {
                    return IOChangeTypes.Latched;
                }
                return IOChangeTypes.NoChange;
            }
        }

        /// <summary>
        /// Any getting or setting of IOLine states is handled by the delegate
        /// </summary>
        internal IIOLineDelegate _ioLineDelegate;

        /// <summary>
        /// The last known previous value of the IO line
        /// default -1 means unknown
        /// </summary>
        internal int _previousValue = -1;
        /// <summary>
        /// The last known previous value of the analog IO line
        /// default -1 means unknown
        /// </summary>
        internal double _previousAValue = -1d;

        /// <summary>
        /// The last known value of the IO line
        /// default -1 means unknown
        /// </summary>
        internal int _value = -1;
        /// <summary>
        /// The last known value of the analog IO line
        /// default -1 means unknown
        /// </summary>
        internal double _aValue = -1d;
        /// <summary>
        /// The date-time the last known value was checked on the device
        /// </summary>
        internal DateTime _valueCacheTime = DateTime.MinValue;

        //TODO: NEEDS TO BE UINT32 to handle 32 bit counts as well as 16 bit
        /// <summary>
        /// The previous count value, -2 if unknown (so its not the same as _count)
        /// </summary>
        internal int _previousCount = -2;

        /// <summary>
        /// The last known count of the IO line
        /// default -1 means unknown
        /// </summary>
        internal int _count = -1;

        /// <summary>
        /// The date-time the last known count was checked on the device
        /// </summary>
        internal DateTime _countCacheTime = DateTime.MinValue;

        internal bool _highLatchedStatus = false;

        /// <summary>
        /// The date-time the last known high-latch was checked on the device
        /// </summary>
        internal DateTime _highLatchedCacheTime = DateTime.MinValue;

        internal bool _lowLatchedStatus = false;

        /// <summary>
        /// The date-time the last known lowLatch was checked on the device
        /// </summary>
        internal DateTime _lowLatchedCacheTime = DateTime.MinValue;

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (this.Label == null ? "" : this.Label + " ")
                + (this.IOType == IOType.Digital ? "D" : "A")
                + (this.IODirection == IODirection.Input ? "In" : "Out")
                + this.IONumber.ToString("D2")
                + " (line " + this.LogicalNumber.ToString("D2") + ")";
        }

        /// <summary>
        /// Give a complete summary of the IOLine
        /// </summary>
        /// <returns></returns>
        public string Describe()
        {
            string description =
             this.ToString() +
                String.Format(CultureInfo.InvariantCulture, "\n\tCurrent Value: \t\t{0:D},\tSampled at: {1}", this._value, this._valueCacheTime.ToString("yyyy-MM-dd hh:mm:ss.fff")) +
                String.Format(CultureInfo.InvariantCulture, "\n\tPrevious Value: \t{0:D}", this._previousValue) +
                String.Format(CultureInfo.InvariantCulture, "\n\tMost Recent Change: \t{0}", this.MostRecentChangeType.ToString("G"));


            if (this.IODirection == IODirection.Input)
            {
                description +=
                String.Format(CultureInfo.InvariantCulture, "\n\tCount: \t{0:D},\tSampled at: {1}", this.Count, this._countCacheTime.ToString("yyyy-MM-dd hh:mm:ss.fff")) +
                String.Format(CultureInfo.InvariantCulture, "\n\tHigh Latch Status: \t{0:D},\tSampled at: {1}", this._highLatchedStatus, this._highLatchedCacheTime.ToString("yyyy-MM-dd hh:mm:ss.fff")) +
                String.Format(CultureInfo.InvariantCulture, "\n\tLow Latch Status: \t{0:D},\tSampled at: {1}", this._lowLatchedStatus, this._lowLatchedCacheTime.ToString("yyyy-MM-dd hh:mm:ss.fff"));
            }

            return description;
        }

        /// <summary>
        /// The current digital state of this IO line.
        /// For inputs: HIGH (1) or LOW (0).
        /// For outputs/relays: CLOSED (1) or OPEN (0).
        /// Only output lines can be set; setting an input throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Get:</strong> If the cached value is older than <see cref="EDDevice.IOLineCacheTimeout"/>,
        /// a network call refreshes all digital line states in one batch. Otherwise the cached value is returned.
        /// </para>
        /// <para>
        /// <strong>Set:</strong> If the new value equals the cached value and the cache has not expired,
        /// no command is sent to the device.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when attempting to set the value of an input line.</exception>
        public int Value
        {
            get
            {
                if (this._valueCacheTime < DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    //IO is much slower than CPU
                    //therefore its just as cheap to get all lines values in one network call, therefore refresh ALL lines
                    _ioLineDelegate.GetAllDigitalLineStates();
                }
                return this._value;
            }
            set
            {
                if (this.IODirection == IODirection.Input)
                {
                    throw new InvalidOperationException("Cannot set the value of an Input");
                }
                //if we're within the cache timeout and the new value is the same as the existing value do nothing
                if (this._value == value && this._valueCacheTime > DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    return;
                }
                _ioLineDelegate.SetDigitalOutputLineState(this, value);
            }
        }
        /// <summary>
        /// The current analog value of this IO line, in the units determined by the device's
        /// <see cref="AnalogDataFormat"/> configuration (e.g. volts, milliamps, or temperature).
        /// Only analog output lines can be set; setting an analog input throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <remarks>
        /// Caching behaviour is the same as <see cref="Value"/>: a get refreshes all analog
        /// line states in one batch if the cache has expired.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when attempting to set the value of an analog input line.</exception>
        public double AValue
        {
            get
            {
                if (this._valueCacheTime < DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    //This call only works for analog inputs
                    //IO is much slower than CPU
                    //there its just as cheap to get all lines values in one network call, therefor refresh all lines
                    _ioLineDelegate.GetAllAnalogLineStates();
                }
                return this._aValue;
            }
            set
            {
                if (this.IODirection == IODirection.AInput)
                {
                    throw new InvalidOperationException("Cannot set the value of an Input");
                }
                //if we're within the cache timeout and the new value is the same as the existing value do nothing
                if (this._aValue == value && this._valueCacheTime > DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    return;
                }
                _ioLineDelegate.SetAnalogLineState(this, value);
            }
        }
        /// <summary>
        /// If the line is an output this function will invert the state of the line
        /// e.g. if the line was closed (1) it will become open (0)
        /// or if the line was open (0) it will become closed (1)
        /// only valid for Digital outputs
        /// </summary>
        /// <returns>The new line Value, 1 or 0</returns>
        /// <exception cref="InvalidOperationException">Thrown when the line is not a digital output.</exception>
        public int Toggle()
        {
            if (this.IODirection != IODirection.Output || this.IOType != IO.IOType.Digital)
            {
                throw new InvalidOperationException("Can only toggle a digital output line");
            }
            int newValue = this.Value == 1 ? 0 : 1;
            this.Value = newValue;
            return newValue;
        }

        /// <summary>
        /// Get the count of the input, that is the number of times the input has latched since
        /// the counter has been reset. Use <see cref="ClearCount"/> to reset.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the line is an output.</exception>
        public int Count
        {
            get
            {
                if (this.IODirection == IODirection.Output)
                {
                    throw new InvalidOperationException("Cannot read the count of an output");
                }

                if (this._countCacheTime < DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    _ioLineDelegate.GetDigitalInputLineCount(this);
                }
                return this._count;
            }
        }

        /// <summary>
        /// Clear the count currently recorded on the line
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the line is an output.</exception>
        public void ClearCount()
        {
            if (this.IODirection == IODirection.Output)
            {
                throw new InvalidOperationException("Cannot clear the count of an output");
            }
            _ioLineDelegate.ClearLineCount(this);
        }

        /// <summary>
        /// Whether this IOLine has had its Low Latch set since the last latch reset
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the line is an output.</exception>
        public bool LowLatchedStatus
        {
            get
            {
                if (this.IODirection == IODirection.Output)
                {
                    throw new InvalidOperationException("Cannot read the latch of an output");
                }

                //get command for the device if the cache timeout has expired
                if (this._lowLatchedCacheTime < DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    //IO is much slower than CPU
                    //therefore its just as cheap to get all lines values in one network call, therefore refresh ALL lines
                    _ioLineDelegate.GetAllDigitalInputLowLatchedStatus();
                }
                return this._lowLatchedStatus;
            }
        }

        /// <summary>
        /// Whether this IOLine has had its High Latch set since the last latch reset
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the line is an output.</exception>
        public bool HighLatchedStatus
        {
            get
            {
                if (this.IODirection == IODirection.Output)
                {
                    throw new InvalidOperationException("Cannot read the latch of an output");
                }

                //get command for the device if the cache timeout has expired
                if (this._highLatchedCacheTime < DateTime.UtcNow.AddMilliseconds(-_ioLineDelegate.CacheTimeout))
                {
                    _ioLineDelegate.GetAllDigitalInputHighLatchedStatus();
                }
                return this._highLatchedStatus;
            }
        }

        #region event handling

        // register for events which only fire for this IOLine

        /// <summary>
        /// Lock for synchronizing access to digital event handler lists
        /// </summary>
        private readonly object _digitalEventLock = new object();

        /// <summary>
        /// List of registered IOLine Change Event handlers
        /// </summary>
        protected List<IOLineChangedEventHandler> _ioLineChangedEvents = new List<IOLineChangedEventHandler>();
        /// <summary>
        /// List of registered IOLine Rising Edge Event handlers
        /// </summary>
        protected List<IOLineChangedEventHandler> _ioLineRisingEdgeEvents = new List<IOLineChangedEventHandler>();
        /// <summary>
        /// List of registered IOLine Falling Edge Event handlers
        /// </summary>
        protected List<IOLineChangedEventHandler> _ioLineFallingEdgeEvents = new List<IOLineChangedEventHandler>();
        /// <summary>
        /// List of registered IOLine Count Event handlers
        /// </summary>
        protected List<IOLineChangedEventHandler> _ioLineCountEvents = new List<IOLineChangedEventHandler>();
        /// <summary>
        /// List of registered Analog IOLine Delta Change Event handlers
        /// </summary>
        internal Dictionary<Delegate, RegisteredEvents> _aioLineChangedDeltaEvents = new Dictionary<Delegate, RegisteredEvents>();
        /// <summary>
        /// List of registered Analog IOLine Target Change Event handlers
        /// </summary>
        internal Dictionary<Delegate, RegisteredEvents> _aioLineChangedTargetEvents = new Dictionary<Delegate, RegisteredEvents>();
        /// <summary>
        /// List of registered Analog IOLine Target Change Event handlers
        /// </summary>
        internal Dictionary<Delegate, RegisteredEvents> _aioLineChangedTargetRangeEvents = new Dictionary<Delegate, RegisteredEvents>();
        /// <summary>
        /// wrapper function calls all events handlers in the _ioLineChangedEvents list
        /// </summary>
        /// <param name="line"></param>
        /// <param name="device"></param>
        /// <param name="changeType"></param>
        protected void thisIOLineChanged(IOLine line, EDDevice device, IOChangeTypes changeType)
        {
            if (line == this)
            {
                IOLineChangedEventHandler[] handlers;
                lock (_digitalEventLock)
                {
                    handlers = _ioLineChangedEvents.ToArray();
                }
                foreach (IOLineChangedEventHandler ioLineChangedEvent in handlers)
                {
                    ioLineChangedEvent(line, device, changeType);
                }
            }
        }

        /// <summary>
        /// wrapper function calls all events handlers in the _ioLineRisingEdgeEvents list
        /// </summary>
        /// <param name="line"></param>
        /// <param name="device"></param>
        /// <param name="changeType"></param>
        protected void thisIOLineRisingEdge(IOLine line, EDDevice device, IOChangeTypes changeType)
        {
            if (line == this)
            {
                IOLineChangedEventHandler[] handlers;
                lock (_digitalEventLock)
                {
                    handlers = _ioLineRisingEdgeEvents.ToArray();
                }
                foreach (IOLineChangedEventHandler ioLineRisingEdgeEvent in handlers)
                {
                    ioLineRisingEdgeEvent(line, device, changeType);
                }
            }
        }

        /// <summary>
        /// wrapper function calls all events handlers in the _ioLineFallingEdgeEvents list
        /// </summary>
        /// <param name="line"></param>
        /// <param name="device"></param>
        /// <param name="changeType"></param>
        protected void thisIOLineFallingEdge(IOLine line, EDDevice device, IOChangeTypes changeType)
        {
            if (line == this)
            {
                IOLineChangedEventHandler[] handlers;
                lock (_digitalEventLock)
                {
                    handlers = _ioLineFallingEdgeEvents.ToArray();
                }
                foreach (IOLineChangedEventHandler ioLineFallingEdgeEvent in handlers)
                {
                    ioLineFallingEdgeEvent(line, device, changeType);
                }
            }
        }

        /// <summary>
        /// wrapper function calls all events handlers in the _ioLineCountEvents list
        /// </summary>
        /// <param name="line"></param>
        /// <param name="device"></param>
        /// <param name="changeType"></param>
        protected void thisIOLineCount(IOLine line, EDDevice device, IOChangeTypes changeType)
        {
            if (line == this)
            {
                IOLineChangedEventHandler[] handlers;
                lock (_digitalEventLock)
                {
                    handlers = _ioLineCountEvents.ToArray();
                }
                foreach (IOLineChangedEventHandler ioLineCountEvent in handlers)
                {
                    ioLineCountEvent(line, device, changeType);
                }
            }
        }
        /// <summary>
        /// register an event for this IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineChanged
        {
            add
            {
                lock (_digitalEventLock)
                {
                    _ioLineChangedEvents.Add(value);
                    if (_ioLineChangedEvents.Count == 1)
                    {
                        _ioLineDelegate.IOLineChanged += thisIOLineChanged;
                    }
                }
            }
            remove
            {
                lock (_digitalEventLock)
                {
                    _ioLineChangedEvents.Remove(value);
                    if (_ioLineChangedEvents.Count == 0)
                    {
                        _ioLineDelegate.IOLineChanged -= thisIOLineChanged;
                    }
                }
            }
        }

        /// <summary>
        /// register a rising edge event for this IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineRisingEdge
        {
            add
            {
                lock (_digitalEventLock)
                {
                    _ioLineRisingEdgeEvents.Add(value);
                    if (_ioLineRisingEdgeEvents.Count == 1)
                    {
                        _ioLineDelegate.IOLineRisingEdge += thisIOLineRisingEdge;
                    }
                }
            }
            remove
            {
                lock (_digitalEventLock)
                {
                    _ioLineRisingEdgeEvents.Remove(value);
                    if (_ioLineRisingEdgeEvents.Count == 0)
                    {
                        _ioLineDelegate.IOLineRisingEdge -= thisIOLineRisingEdge;
                    }
                }
            }
        }

        /// <summary>
        /// register a rising edge event for this IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineFallingEdge
        {
            add
            {
                lock (_digitalEventLock)
                {
                    _ioLineFallingEdgeEvents.Add(value);
                    if (_ioLineFallingEdgeEvents.Count == 1)
                    {
                        _ioLineDelegate.IOLineFallingEdge += thisIOLineFallingEdge;
                    }
                }
            }
            remove
            {
                lock (_digitalEventLock)
                {
                    _ioLineFallingEdgeEvents.Remove(value);
                    if (_ioLineFallingEdgeEvents.Count == 0)
                    {
                        _ioLineDelegate.IOLineFallingEdge -= thisIOLineFallingEdge;
                    }
                }
            }
        }

        /// <summary>
        /// register a count event for this IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineCount
        {
            add
            {
                lock (_digitalEventLock)
                {
                    _ioLineCountEvents.Add(value);
                    if (_ioLineCountEvents.Count == 1)
                    {
                        _ioLineDelegate.IOLineCount += thisIOLineCount;
                    }
                }
            }
            remove
            {
                lock (_digitalEventLock)
                {
                    _ioLineCountEvents.Remove(value);
                    if (_ioLineCountEvents.Count == 0)
                    {
                        _ioLineDelegate.IOLineCount -= thisIOLineCount;
                    }
                }
            }
        }
        internal readonly object _analogEventLock = new object();
        /// <summary>
        /// Register an event with this handler to be notified when the state of this AIOLine
        /// changes over the delta value specified
        /// </summary>
        /// <param name="function"></param>
        /// <param name="delta"></param>
        public void SubscribeToDeltaEvent(ref AIOLineChangedEventHandler function, double delta)
        {
            lock (_analogEventLock)
            {
                _aioLineChangedDeltaEvents.Add(function, new RegisteredEvents { Delta = delta, Invoke = function, EventName = "DELTA" });
                if(_aioLineChangedDeltaEvents.Count == 1)
                {
                    _ioLineDelegate.SubscribeToDeltaEvent(ref function, delta);
                }
            }
        }
        /// <summary>
        /// Register an event with this handler to be notified when the state of this AIOLine 
        /// goes over the value specified and below the value specified
        /// </summary>
        /// <param name="function"></param>
        /// <param name="target"></param>
        [Obsolete("SubscribeToTargetEvents is deprecated. Replaced by SubscribeToTargetEvent")]
        public void SubscribeToTargetEvents(ref AIOLineChangedEventHandler function, double target)
        {
            this.SubscribeToTargetEvent(ref function, target);
        }
        /// <summary>
        /// Register an event with this handler to be notified when the state of this AIOLine 
        /// goes over the value specified and below the value specified
        /// </summary>
        /// <param name="function"></param>
        /// <param name="target"></param>
        public void SubscribeToTargetEvent(ref AIOLineChangedEventHandler function, double target)
        {
            lock (_analogEventLock)
            {
                _aioLineChangedTargetEvents.Add(function, new RegisteredEvents { Target = target, Invoke = function, EventName = "TARGET" });
                if (_aioLineChangedTargetEvents.Count == 1)
                {
                    _ioLineDelegate.SubscribeToTargetEvent(ref function, target);
                }
            }
        }

        /// <summary>
        /// Register an event with this handler to be notified when the state of this AIOLine
        /// changes more than the specified delta value away from the specified target value
        /// </summary>
        /// <param name="function"></param>
        /// <param name="target"></param>
        /// <param name="delta"></param>
        [Obsolete("SubscribeToTargetRangeEvents is deprecated. Replaced by SubscribeToTargetRangeEvent")]
        public void SubscribeToTargetRangeEvents(ref AIOLineChangedEventHandler function, double target, double delta)
        {
            this.SubscribeToTargetRangeEvent(ref function, target, delta);
        }

        /// <summary>
        /// Register an event with this handler to be notified when the state of this AIOLine
        /// changes more than the specified delta value away from the specified target value
        /// </summary>
        /// <param name="function"></param>
        /// <param name="target"></param>
        /// <param name="delta"></param>
        public void SubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function, double target, double delta)
        {
            lock (_analogEventLock)
            {
                _aioLineChangedTargetRangeEvents.Add(function, new RegisteredEvents { Target = target, Delta = delta, Invoke = function, EventName = "TARGETRANGE" });
                if(_aioLineChangedTargetRangeEvents.Count == 1)
                {
                    _ioLineDelegate.SubscribeToTargetRangeEvent(ref function, target, delta);
                }

            }
        }
        /// <summary>
        /// Unsubscribe a registered delta event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        public void UnsubscribeToDeltaEvent(ref AIOLineChangedEventHandler function)
        {
            lock (_analogEventLock)
            {
                _aioLineChangedDeltaEvents.Remove(function);
                if (_aioLineChangedDeltaEvents.Count == 0)
                {
                    _ioLineDelegate.UnsubscribeToDeltaEvent(ref function);
                }
            }
        }
        /// <summary>
        /// Unsubscribe a registered target event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        [Obsolete("UnsubscribeToTargetEvents is deprecated. Replaced with UnsubscribeToTargetEvent.")]
        public void UnsubscribeToTargetEvents(ref AIOLineChangedEventHandler function)
        {
            this.UnsubscribeToTargetEvent(ref function);
        }

        /// <summary>
        /// Unsubscribe a registered target event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        public void UnsubscribeToTargetEvent(ref AIOLineChangedEventHandler function)
        {
            lock (_analogEventLock)
            {
                _aioLineChangedTargetEvents.Remove(function);
                if (_aioLineChangedTargetEvents.Count == 0)
                {
                    _ioLineDelegate.UnsubscribeToTargetEvent(ref function);
                }
            }
        }

        /// <summary>
        /// Unsubscribe a registered target range event with this handler to stop being notified 
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        [Obsolete("UnsubscribeToTargetRangeEvents is deprecated. Replaced with UnsubscribeToTargetRangeEvent.")]
        public void UnsubscribeToTargetRangeEvents(ref AIOLineChangedEventHandler function)
        {
            this.UnsubscribeToTargetRangeEvent(ref function);
        }

        /// <summary>
        /// Unsubscribe a registered target range event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>
        /// <param name="function"></param>
        public void UnsubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function)
        {
            lock (_analogEventLock)
            {
                _aioLineChangedTargetRangeEvents.Remove(function);
                if (_aioLineChangedTargetRangeEvents.Count == 0)
                {
                    _ioLineDelegate.UnsubscribeToTargetRangeEvent(ref function);
                }
            }
        }
        #endregion event handling

    }

}
