
using System;

namespace Brainboxes.IO
{
    internal delegate void OnChangeDelegate(IOLine line);

    /// <summary>
    /// Defines the contract with IOLine which the a delegate must implement
    /// </summary>
    internal interface IIOLineDelegate
    {
        /// <summary>
        /// a collection of all the lines for which the delegate will act
        /// </summary>
        IOList<IOLine> IOLines { get; }
        event IOLineChangedEventHandler IOLineChanged;

        event IOLineChangedEventHandler IOLineRisingEdge;
        event IOLineChangedEventHandler IOLineFallingEdge;
        event IOLineChangedEventHandler IOLineCount;
        void SubscribeToDeltaEvent(ref AIOLineChangedEventHandler function, double delta);
        void SubscribeToTargetEvent(ref AIOLineChangedEventHandler function, double target);
        void SubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function, double target, double delta);
        void UnsubscribeToDeltaEvent(ref AIOLineChangedEventHandler function);
        void UnsubscribeToTargetEvent(ref AIOLineChangedEventHandler function);
        void UnsubscribeToTargetRangeEvent(ref AIOLineChangedEventHandler function);
        /// <summary>
        /// whether the Digital Input IO Line counter updates on a rising edge or a falling edge
        /// </summary>
        /// IOChangeTypes IOCounterUpdateDirection { get; }

        /// <summary>
        /// The time for which a cache value of an IO line is still valid
        /// </summary>
        [Obsolete("cacheTimeout is deprecated. Replaced by CacheTimeout")]
        int cacheTimeout{ get; }
        /// <summary>
        /// The time for which a cache value of an IO line is still valid
        /// </summary>
        int CacheTimeout { get; }
        /// <summary>
        /// Update the IOLine cache of all the Digital IO line states
        /// </summary>
        [Obsolete("getAllLineStates is deprecated. Replaced by GetAllDigitalLineStates")]
        void getAllLineStates();
        /// <summary>
        /// Update the IOLine cache of all the Digital IO line states
        /// </summary>
        void GetAllDigitalLineStates();
        /// <summary>
        /// Update the Analog IOLine cache of all the line states
        /// </summary>
        void GetAllAnalogLineStates();
        /// <summary>
        /// Set the state of a Digital IOLine
        /// </summary>
        /// <param name="line"></param>
        /// <param name="value"></param>
        [Obsolete("setLineState is deprecated. Replaced by SetDigitalOutputLineState")]
        void setLineState(IOLine line, int value);
        /// <summary>
        /// Set the state of a Digital IOLine
        /// </summary>
        /// <param name="line"></param>
        /// <param name="value"></param>
        void SetDigitalOutputLineState(IOLine line, int value);
        /// <summary>
        /// Set the state of an analog IOLine
        /// </summary>
        /// <param name="line"></param>
        /// <param name="value"></param>
        void SetAnalogLineState(IOLine line, double value);
        /// <summary>
        /// Read the status of the low latched digital input lines on the device and
        /// update them all in the IOLines collection
        /// </summary>
        /// <returns></returns>
        [Obsolete("getAllInputLowLatchedStatus is deprecated. Replaced by GetAllDigitalInputLowLatchedStatus")]
        void getAllInputLowLatchedStatus();
        /// <summary>
        /// Read the status of the low latched digital input lines on the device and
        /// update the all in the IOLine collection
        /// </summary>
        void GetAllDigitalInputLowLatchedStatus();
        /// <summary>
        /// Read the status of the high latched digital input line
        /// update them all in the IOLines collection
        /// </summary>
        /// <returns></returns>
        [Obsolete("getAllInputHighLatchedStatus is deprecated. Replaced by GetAllDigitalInputHighLatchedStatus")]
        void getAllInputHighLatchedStatus();
        /// <summary>
        /// Read the status of the high latched digital input line
        /// update them all in the IOLine collection
        /// </summary>
        void GetAllDigitalInputHighLatchedStatus();

        /// <summary>
        /// Get the Count of the digital input
        /// </summary>
        /// <param name="iOLine"></param>
        [Obsolete("getLineCount is deprecated. Replaced by GetDigitalInputLineCount")]
        void getLineCount(IOLine iOLine);
        /// <summary>
        /// Get the Count of the digital input
        /// </summary>
        /// <param name="ioLine"></param>
        void GetDigitalInputLineCount(IOLine ioLine);
        /// <summary>
        /// Clear the count of the digital input
        /// </summary>
        /// <param name="iOLine"></param>
        void ClearLineCount(IOLine iOLine);

    }
}
