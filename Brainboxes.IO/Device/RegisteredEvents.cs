using System;
using System.Collections.Generic;
using System.Text;

namespace Brainboxes.IO
{
    /// <summary>
    /// The class used to hold information about a registered event
    /// </summary>
    internal class RegisteredEvents
    {
        /// <summary>
        /// The Target value for the event - Either Target Event or Target Range Event
        /// </summary>
        public double Target { get; set; }
        /// <summary>
        /// The Delta value for the event - Either Delta Event or Target Range Event
        /// </summary>
        public double Delta { get; set; }
        /// <summary>
        /// The Event Name:
        /// DELTA
        /// TARGET
        /// TARGETRANGE
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// The function which is bound to the event 
        /// </summary>
        public AIOLineChangedEventHandler Invoke { get; set; }
            
    }
}
