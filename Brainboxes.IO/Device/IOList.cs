using System;
using System.Collections.Generic;

namespace Brainboxes.IO
{
    /// <summary>
    /// List to hold any number of IOLines. Provides convenience methods for register events to all lines in the list and setting values of all outputs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IOList<T> : IList<T>
       where T : IOLine
    {
        //facade the underlying list
        private IList<T> _list;

        private bool _readonly = false;

        /// <summary>
        /// Initialize a new instance of an IOList
        /// </summary>
        public IOList()
        {
            _list = new List<T>();
        }

        /// <summary>
        /// Initialize a new instance of an IOList that is empty and has the specified capacity
        /// </summary>
        public IOList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        /// <summary>
        /// Initialize a new instance of an IOList that contains elements copied from the collection
        /// </summary>
        /// <param name="collection"></param>
        public IOList(IEnumerable<T> collection)
        {
            _list = new List<T>(collection);
        }

        /// <summary>
        /// User definable label for the IOList to help identify/ when debugging
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Convert to List&lt;T&gt;
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return new List<T>(_list);
        }

        /// <summary>
        /// set all output values in Collection, if there are not Outputs nothing will be set
        /// </summary>
        public int Values
        {
            set
            {
                foreach (IOLine line in _list)
                {
                    if (line.IODirection == IODirection.Output)
                    {
                        line.Value = value;
                    }
                }
            }

        }
        /// <summary>
        /// Set all analog output values in collection, if there are no outputs nothing will be set
        /// </summary>
        public double AValues
        {
            set
            {
                foreach(IOLine line in _list)
                {
                    if(line.IODirection == IODirection.AOutput)
                    {
                        line.AValue = value;
                    }
                }
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _list.Count + " " + (this.Label == null ? base.ToString() : this.Label);
        }

        /// <summary>
        /// Give a complete summary of the EDDevice
        /// </summary>
        /// <returns></returns>
        public string Describe()
        {
            string returnString = this.ToString();
            foreach (IOLine line in _list)
            {
                returnString += "\r\n\t- " + line.ToString();
            }
            return returnString;
        }

        /// <summary>
        /// Lock for synchronizing event handler dictionary access
        /// </summary>
        private readonly object _eventLock = new object();

        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredEvents = 0;

        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredChangeEvents = 0;

        /// <summary>
        /// use dictionary t bool as an alternative to hashset (only available in .net 3.5+)
        /// this means only unique events can be added and existing events removed
        /// </summary>
        protected Dictionary<IOLineChangedEventHandler, bool> _ioLineChangeHandlers = new Dictionary<IOLineChangedEventHandler, bool>();

        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredRisingEvents = 0;
        /// <summary>
        /// use dictionary t bool as an alternative to hashset (only available in .net 3.5+)
        /// this means only unique events can be added and existing events removed
        /// </summary>
        protected Dictionary<IOLineChangedEventHandler, bool> _ioLineRisingHandlers = new Dictionary<IOLineChangedEventHandler, bool>();

        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredFallingEvents = 0;
        /// <summary>
        /// use dictionary t bool as an alternative to hashset (only available in .net 3.5+)
        /// this means only unique events can be added and existing events removed
        /// </summary>
        protected Dictionary<IOLineChangedEventHandler, bool> _ioLineFallingHandlers = new Dictionary<IOLineChangedEventHandler, bool>();
        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredChangedDeltaEvents = 0;
        /// <summary>
        /// use dictionary t bool as an alternative to hashset (only available in .net 3.5+)
        /// this means only unique events can be added and existing events removed
        /// </summary>
        protected Dictionary<AIOLineChangedEventHandler, bool> _aioLineChangedDeltaHandlers = new Dictionary<AIOLineChangedEventHandler, bool>();
        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredChangedTargetEvents = 0;
        /// <summary>
        /// use dictionary t bool as an alternative to hashset (only available in .net 3.5+)
        /// this means only unique events can be added and existing events removed
        /// </summary>
        protected Dictionary<AIOLineChangedEventHandler, bool> _aioLineChangedTargetHandlers = new Dictionary<AIOLineChangedEventHandler, bool>();
        /// <summary>
        /// can only be positive
        /// </summary>
        /// <summary>
        /// can only be positive
        /// </summary>
        protected uint _numOfRegisteredChangedTargetRangeEvents = 0;
        /// <summary>
        /// use dictionary t bool as an alternative to hashset (only available in .net 3.5+)
        /// this means only unique events can be added and exisiting events removed
        /// </summary>
        protected Dictionary<AIOLineChangedEventHandler, bool> _aioLineChangedTargetRangeHandlers = new Dictionary<AIOLineChangedEventHandler, bool>();

        /// <summary>
        /// Add/remove IOLineChange event to all lines in the list,
        /// if IOLines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// If events are removed which were not previously added, they will still be attempted to be removed from the IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineChange
        {
            add
            {
                T[] lines;
                lock (_eventLock)
                {
                    //only allow unique events
                    if (_ioLineChangeHandlers.ContainsKey(value)) return;
                    _ioLineChangeHandlers.Add(value, true);
                    _numOfRegisteredChangeEvents++;
                    _numOfRegisteredEvents++;
                    lines = new List<T>(_list).ToArray();
                }
                foreach (IOLine l in lines)
                {
                    l.IOLineChanged += value;
                }
            }
            remove
            {
                T[] lines;
                lock (_eventLock)
                {
                    if (_ioLineChangeHandlers.ContainsKey(value))
                    {
                        _ioLineChangeHandlers.Remove(value);
                        _numOfRegisteredChangeEvents--;
                        _numOfRegisteredEvents--;
                    }
                    lines = new List<T>(_list).ToArray();
                }
                foreach (IOLine l in lines)
                {
                    l.IOLineChanged -= value;
                }
            }
        }
        /// <summary>
        /// Add/remove IOLineRisingEdge event to all lines in the list,
        /// if IOLines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// If events are removed which were not previously added, they will still be attempted to be removed from the IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineRisingEdge
        {
            add
            {
                T[] lines;
                lock (_eventLock)
                {
                    //only allow unique events
                    if (_ioLineRisingHandlers.ContainsKey(value)) return;
                    _ioLineRisingHandlers.Add(value, true);
                    _numOfRegisteredRisingEvents++;
                    _numOfRegisteredEvents++;
                    lines = new List<T>(_list).ToArray();
                }
                foreach (IOLine l in lines)
                {
                    l.IOLineRisingEdge += value;
                }
            }
            remove
            {
                T[] lines;
                lock (_eventLock)
                {
                    if (_ioLineRisingHandlers.ContainsKey(value))
                    {
                        _ioLineRisingHandlers.Remove(value);
                        _numOfRegisteredRisingEvents--;
                        _numOfRegisteredEvents--;
                    }
                    lines = new List<T>(_list).ToArray();
                }
                foreach (IOLine l in lines)
                {
                    l.IOLineRisingEdge -= value;
                }
            }
        }

        /// <summary>
        /// Add/remove IOLineFallingEdge event to all lines in the list,
        /// if IOLines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// If events are removed which were not previously added, they will still be attempted to be removed from the IOLine
        /// </summary>
        public event IOLineChangedEventHandler IOLineFallingEdge
        {
            add
            {
                T[] lines;
                lock (_eventLock)
                {
                    //only allow unique events
                    if (_ioLineFallingHandlers.ContainsKey(value)) return;
                    _ioLineFallingHandlers.Add(value, true);
                    _numOfRegisteredFallingEvents++;
                    _numOfRegisteredEvents++;
                    lines = new List<T>(_list).ToArray();
                }
                foreach (IOLine l in lines)
                {
                    l.IOLineFallingEdge += value;
                }
            }
            remove
            {
                T[] lines;
                lock (_eventLock)
                {
                    if (_ioLineFallingHandlers.ContainsKey(value))
                    {
                        _ioLineFallingHandlers.Remove(value);
                        _numOfRegisteredFallingEvents--;
                        _numOfRegisteredEvents--;
                    }
                    lines = new List<T>(_list).ToArray();
                }
                foreach (IOLine l in lines)
                {
                    l.IOLineFallingEdge -= value;
                }
            }
        }
        /// <summary>
        /// Register an event with this handler to be notified when the state of this AIOLines in the list
        /// changes over the delta value specified.
        /// If IOLines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// The event can be unregistered using the UnsubscribeToDeltaEvent
        /// </summary>
        public void SubscribeToDeltaEvent(ref AIOLineChangedEventHandler function, double delta)
        {

            foreach (IOLine l in _list)
            {
                l.SubscribeToDeltaEvent(ref function, delta);
            }

        }
        /// <summary>
        /// Register event with this handler to be notified when the state of this AIOLines in the list
        /// goes over the value specified and below the value specified.
        /// If IOLines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// The event can be unregistered using the UnsubscribeToTargetEvents
        /// </summary>
        [Obsolete("SubscribeToTargetEvents is deprecated. Replaced by SubscribeToTargetEvent")]
        public void SubscribeToTargetEvents(ref AIOLineChangedEventHandler function, double target)
        {
            this.SubscribeToTargetEvent(ref function, target);
        }
        /// <summary>
        /// Register event with this handler to be notified when the state of this AIOLines in the list
        /// goes over the value specified and below the value specified.
        /// If IOLines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// The event can be unregistered using the UnsubscribeToTargetEvents
        /// </summary>
        public void SubscribeToTargetEvent(ref AIOLineChangedEventHandler function, double target)
        {

            foreach (IOLine l in _list)
            {
                l.SubscribeToTargetEvent(ref function, target);
            }

        }
        /// <summary>     
        /// Register event with this handler to be notified when the state of this AIOLines in the list
        /// changes more than the specified delta value away from the specified target value.
        /// If IOlines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// The event can be unregistered using the UnsubscribeToTargetRangeEvents
        /// </summary>
        [Obsolete("SubscribeToTargetRangeEvents is deprecated. Replaced by SubscribeToTargetRangeEvent")]
        public void SubscribeToTargetRangeEvents(ref AIOLineChangedEventHandler function, double target, double delta)
        {

            this.SubscribeToTargetRangeEvent(ref function, target, delta);
            
        }

        /// <summary>     
        /// Register event with this handler to be notified when the state of this AIOLines in the list
        /// changes more than the specified delta value away from the specified target value.
        /// If IOlines are subsequently added or removed then the events will also be applied
        /// The same event cannot be added twice (function will return and do nothing)
        /// The event can be unregistered using the UnsubscribeToTargetRangeEvents
        /// </summary>
        public void SubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function, double target, double delta)
        {

            foreach (IOLine l in _list)
            {
                l.SubscribeToTargetRangeEvent(ref function, target, delta);
            }

        }

        /// <summary>
        /// Unsubscribe a registered delta event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>


        public void UnsubscribeToDeltaEvent(ref AIOLineChangedEventHandler function)
        {
            foreach(IOLine l in _list)
            {
                l.UnsubscribeToDeltaEvent(ref function);
            }
        }
        /// <summary>
        /// Unsubscribe a registered target event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>
        [Obsolete("UnsubscribeToTargetEvents is deprecated. Replaced by UnsubscribeToTargetEvent")]

        public void UnsubscribeToTargetEvents(ref AIOLineChangedEventHandler function)
        {
            this.UnsubscribeToTargetEvent(ref function);
        }


        /// <summary>
        /// Unsubscribe a registered target event with this handler to stop being notified
        /// when this event is triggered
        /// </summary>

        public void UnsubscribeToTargetEvent(ref AIOLineChangedEventHandler function)
        {
            foreach (IOLine l in _list)
            {
                l.UnsubscribeToTargetEvent(ref function);
            }
        }
        /// <summary>
        /// Unsubscribe a registered target range event with this handler to stop being notified 
        /// when this event is triggered
        /// </summary>
        [Obsolete("UnsubscribeToTargetRangeEvents is deprecated. Replaced by UnsubscribeToTargetRangeEvent")]
        public void UnsubscribeToTargetRangeEvents(ref AIOLineChangedEventHandler function)
        {
            this.UnsubscribeToTargetRangeEvent(ref function);
        }


        /// <summary>
        /// Unsubscribe a registered target range event with this handler to stop being notified 
        /// when this event is triggered
        /// </summary>
        public void UnsubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function)
        {
            foreach (IOLine l in _list)
            {
                l.UnsubscribeToTargetRangeEvent(ref function);
            }
        }

        /// <summary>
        /// Apply any events which are currently added
        /// </summary>
        /// <param name="line"></param>
        protected void _applyListEvents(T line)
        {
            IOLineChangedEventHandler[] changeHandlers;
            IOLineChangedEventHandler[] fallingHandlers;
            IOLineChangedEventHandler[] risingHandlers;

            lock (_eventLock)
            {
                if (_numOfRegisteredChangeEvents == 0) return;
                changeHandlers = new List<IOLineChangedEventHandler>(_ioLineChangeHandlers.Keys).ToArray();
                fallingHandlers = new List<IOLineChangedEventHandler>(_ioLineFallingHandlers.Keys).ToArray();
                risingHandlers = new List<IOLineChangedEventHandler>(_ioLineRisingHandlers.Keys).ToArray();
            }

            foreach (IOLineChangedEventHandler changeEvent in changeHandlers)
            {
                line.IOLineChanged += changeEvent;
            }
            foreach (IOLineChangedEventHandler changeEvent in fallingHandlers)
            {
                line.IOLineFallingEdge += changeEvent;
            }
            foreach (IOLineChangedEventHandler changeEvent in risingHandlers)
            {
                line.IOLineRisingEdge += changeEvent;
            }
        }

        /// <summary>
        /// Remove any events which are currently added
        /// </summary>
        /// <param name="line"></param>
        protected void _removeListEvents(T line)
        {
            IOLineChangedEventHandler[] changeHandlers;
            IOLineChangedEventHandler[] fallingHandlers;
            IOLineChangedEventHandler[] risingHandlers;

            lock (_eventLock)
            {
                if (_numOfRegisteredChangeEvents == 0) return;
                changeHandlers = new List<IOLineChangedEventHandler>(_ioLineChangeHandlers.Keys).ToArray();
                fallingHandlers = new List<IOLineChangedEventHandler>(_ioLineFallingHandlers.Keys).ToArray();
                risingHandlers = new List<IOLineChangedEventHandler>(_ioLineRisingHandlers.Keys).ToArray();
            }

            foreach (IOLineChangedEventHandler changeEvent in changeHandlers)
            {
                line.IOLineChanged -= changeEvent;
            }
            foreach (IOLineChangedEventHandler changeEvent in fallingHandlers)
            {
                line.IOLineFallingEdge -= changeEvent;
            }
            foreach (IOLineChangedEventHandler changeEvent in risingHandlers)
            {
                line.IOLineRisingEdge -= changeEvent;
            }
        }

        /// <summary>
        /// Determine the index of a specific IOLine
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int IndexOf(T line)
        {
            return this._list.IndexOf(line);
        }

        /// <summary>
        /// Insert an IOLine at a specific index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="line"></param>
        public void Insert(int index, T line)
        {
            if(_readonly) throw new NotSupportedException("This IOList is read-only");
            this._list.Insert(index, line);
            _applyListEvents(line);
        }

        /// <summary>
        /// Removes the IOLine at the specific index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            if (_readonly) throw new NotSupportedException("This IOList is read-only");
            T line = _list[index];
            this._list.RemoveAt(index);
            _removeListEvents(line);
        }

        /// <summary>
        /// Get the IOLine at a particular index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        /// <summary>
        /// Adds an IOLine to the List
        /// </summary>
        /// <param name="line"></param>
        public void Add(T line)
        {
            if (_readonly) throw new NotSupportedException("This IOList is read-only");
            _list.Add(line);
            _applyListEvents(line);
        }

        /// <summary>
        /// Removes all IOLines from the list
        /// </summary>
        public void Clear()
        {
            if (_readonly) throw new NotSupportedException("This IOList is read-only");
            if (_numOfRegisteredChangeEvents > 0)
            {
                foreach (T line in _list)
                {
                    _removeListEvents(line);
                }
            }
            _list.Clear();
        }

        /// <summary>
        /// Determines whether the list contains a specific IOLine
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool Contains(T line)
        {
            return _list.Contains(line);
        }

        /// <summary>
        /// Copies the IOLine(s) starting at a particular index to the supplied array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of IOLines in the List
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Gets the value indicating whether the IOList is read-only
        /// </summary>
        public bool IsReadOnly
        {
            get { return _list.IsReadOnly; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific IOLine from the list
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool Remove(T line)
        {
            if (_readonly) throw new NotSupportedException("This IOList is read-only");
            _removeListEvents(line);
            return _list.Remove(line);
        }

        /// <summary>
        /// Returns the IOList enumerator that iterates through the collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Converts the IOList to a Read-only List
        /// </summary>
        /// <returns></returns>
        public IOList<T> AsReadOnly()
        {
            _readonly = true;
            return this;
        }
    }

    /// <summary>
    /// Cast IEnumerable&lt;IOLine&gt; to IOList so that we can easy add and remove events from the list
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Convert IEnumerable&lt;IOLine&gt; to an IOList&lt;IOLine&gt;
        /// </summary>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public static IOList<IOLine> AsIOList(this IEnumerable<IOLine> lineList)
        {
            return new IOList<IOLine>(lineList);
        }
    }
}
