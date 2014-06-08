using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDEngine3.Event
{
    /// <summary>
    /// Class that defines an interface for objects that handle event dispatching
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// Clears all the events on this IEventHandler
        /// </summary>
        void Clear();

        /// <summary>
        /// Registers the given IEventReceiver object to receive events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to register</param>
        /// <param name="eventType">The event type to notify the receiver of</param>
        void RegisterEventReceiver(IEventReceiver receiver, string eventType);

        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to unregister</param>
        /// <param name="eventType">The event type to unregister</param>
        void UnregisterEventReceiver(IEventReceiver receiver, string eventType);

        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving all events it is currently registered on
        /// </summary>
        /// <param name="receiver">The event receiver</param>
        void UnregisterEventReceiverFromAllEvents(IEventReceiver receiver);

        /// <summary>
        /// Broadcasts the given GDEvent to all currently registered receivers
        /// </summary>
        /// <param name="gameEvent">The event to broadcast</param>
        void BroadcastEvent(GDEvent gameEvent);
    }
}