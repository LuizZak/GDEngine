using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDEngine3.Event
{
    /// <summary>
    /// Class that defines an interface for event receiver objects
    /// </summary>
    public interface IEventReceiver
    {
        /// <summary>
        /// Recenves an event fired by another object
        /// </summary>
        /// <param name="gameEvent">The event that was fired</param>
        void ReceiveEvent(GDEvent gameEvent);
    }
}