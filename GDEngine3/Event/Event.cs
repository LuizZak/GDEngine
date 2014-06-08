using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDEngine3.Event
{
    /// <summary>
    /// Defines a game event object
    /// </summary>
    public class GDEvent
    {
        /// <summary>
        /// The type of this event
        /// </summary>
        private string eventType;

        /// <summary>
        /// The object that sent this event
        /// </summary>
        private object sender;

        /// <summary>
        /// Gets the type of this event
        /// </summary>
        public string EventType
        {
            get { return eventType; }
        }

        /// <summary>
        /// Gets the object that sent this event
        /// </summary>
        public object Sender
        {
            get { return sender; }
        }

        /// <summary>
        /// Initializes a new instance of the GDEvent class
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventType">The type for this event</param>
        public GDEvent(object sender, string eventType)
        {
            this.sender = sender;
            this.eventType = eventType;
        }
    }
}