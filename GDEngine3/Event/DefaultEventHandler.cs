using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDEngine3.Event
{
    /// <summary>
    /// Default IEventHandler implementation.
    /// Handles receivers using a list of weak references to receivers inside a hashtable with a key of an event type.
    /// </summary>
    public class GDDefaultEventHandler : IEventHandler
    {
        /// <summary>
        /// The internal list of event receivers
        /// </summary>
        private Dictionary<string, List<WeakReference>> eventTypes;

        /// <summary>
        /// Initializes a new instance of the GDDefaultEventHandler class
        /// </summary>
        public GDDefaultEventHandler()
        {
            eventTypes = new Dictionary<string, List<WeakReference>>();
        }

        /// <summary>
        /// Clears all the events on this IEventHandler
        /// </summary>
        public void Clear()
        {
            eventTypes.Clear();
        }

        /// <summary>
        /// Registers the given IEventReceiver object to receive events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to register</param>
        /// <param name="eventType">The event type to notify the receiver of</param>
        public void RegisterEventReceiver(IEventReceiver receiver, string eventType)
        {
            if (IsReceiverRegistered(receiver, eventType))
                return;

            List<WeakReference> receivers = GetListForEventType(eventType, true);

            receivers.Add(new WeakReference(receiver));
        }

        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to unregister</param>
        /// <param name="eventType">The event type to unregister</param>
        public void UnregisterEventReceiver(IEventReceiver receiver, string eventType)
        {
            InternalUnregisterEventReceiver(receiver, eventType, true);
        }
        
        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving all events it is currently registered on
        /// </summary>
        /// <param name="receiver">The event receiver</param>
        public void UnregisterEventReceiverFromAllEvents(IEventReceiver receiver)
        {
            foreach (string key in eventTypes.Keys)
            {
                InternalUnregisterEventReceiver(receiver, key, false);
            }

            ClearEmptyEventTypes();
        }

        /// <summary>
        /// Broadcasts the given GDEvent to all currently registered receivers
        /// </summary>
        /// <param name="gameEvent">The event to broadcast</param>
        public void BroadcastEvent(GDEvent gameEvent)
        {
            string eventType = gameEvent.EventType;

            if (!HasReceiversForEvent(eventType))
                return;

            // Flag used to signal whether to clear the list of event receivers after the event is broadcasted
            bool cleanAfter = false;

            for (int i = 0; i < eventTypes[eventType].Count; i++)
            {
                if (eventTypes[eventType][i].IsAlive)
                {
                    IEventReceiver receiver = eventTypes[eventType][i].Target as IEventReceiver;

                    if (receiver != null)
                    {
                        receiver.ReceiveEvent(gameEvent);
                    }
                }
                else
                {
                    cleanAfter = true;
                }
            }

            if (cleanAfter)
            {
                ClearEmptyEventTypes(eventType);
            }
        }

        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to unregister</param>
        /// <param name="eventType">The event type to unregister</param>
        /// <param name="forceClearEmpty">Whether to clear the event receiver list if it's empty after the unregistering is done</param>
        public void InternalUnregisterEventReceiver(IEventReceiver receiver, string eventType, bool forceClearEmpty)
        {
            if (!IsReceiverRegistered(receiver, eventType) && HasReceiversForEvent(eventType))
                return;

            // Flag used to indicate whether to check for clean event receivers after the removing loop
            bool cleanEmptyAter = false;
            bool cleanWeakAfter = false;

            foreach (WeakReference reference in GetListForEventType(eventType, false))
            {
                if (reference.IsAlive)
                {
                    if (reference.Target == receiver)
                    {
                        cleanEmptyAter = true;

                        GetListForEventType(eventType, false).Remove(reference);

                        break;
                    }
                }
                else
                {
                    cleanWeakAfter = true;
                }
            }

            if (cleanWeakAfter)
                CheckReferencesForEventType(eventType, forceClearEmpty);

            if (cleanEmptyAter)
                ClearEmptyEventTypes();
        }

        /// <summary>
        /// Gets the list of event handlers for the given type
        /// </summary>
        /// <param name="eventType">The event type to get or create the event receivers list</param>
        /// <param name="createNew">Whether to create a new event receiver lis, if not available</param>
        /// <returns>An event list, or null, if none is available and the createNew flag is set to false</returns>
        private List<WeakReference> GetListForEventType(string eventType, bool createNew)
        {
            if (HasReceiversForEvent(eventType))
            {
                return eventTypes[eventType];
            }
            else if (createNew)
            {
                return (eventTypes[eventType] = new List<WeakReference>());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns whether the given IEventReceiver is currently registered on the given event type
        /// </summary>
        /// <param name="receiver">The event receiver to search for</param>
        /// <param name="eventType">The type of the event to search in</param>
        /// <returns>Whether the given IEventReceiver is currently registered on the given event type</returns>
        private bool IsReceiverRegistered(IEventReceiver receiver, string eventType)
        {
            if (!HasReceiversForEvent(eventType))
                return false;

            foreach(WeakReference reference in eventTypes[eventType])
            {
                if (reference.Target == receiver)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether there are any event receivers currently registered for the given event type
        /// </summary>
        /// <param name="eventType">The event type to check</param>
        /// <returns>Whether there are any event receivers currently registered for the given event type</returns>
        private bool HasReceiversForEvent(string eventType)
        {
            return eventTypes.ContainsKey(eventType);
        }

        /// <summary>
        /// Performs a check for any dead reference for all the event types
        /// </summary>
        private void CheckAllReferences()
        {
            foreach(string key in eventTypes.Keys)
            {
                CheckReferencesForEventType(key);
            }

            ClearEmptyEventTypes();
        }

        /// <summary>
        /// Checks and removes any dead reference for event receivers of a specific event type
        /// </summary>
        /// <param name="eventType">The event type to check the references for</param>
        /// <param name="removeIfEmpty">Whether to remove the event type automatically if it has been found to be empty</param>
        private void CheckReferencesForEventType(string eventType, bool removeIfEmpty = false)
        {
            for (int i = 0; i < eventTypes[eventType].Count; i++)
            {
                if (!eventTypes[eventType][i].IsAlive)
                {
                    eventTypes[eventType].RemoveAt(i);
                    i--;
                }
            }

            if (removeIfEmpty)
            {
                ClearEmptyEventTypes(eventType);
            }
        }

        /// <summary>
        /// Removes references for empty event types
        /// </summary>
        /// <param name="eventType">An event type to force check on</param>
        private void ClearEmptyEventTypes(string eventType = "")
        {
            if (eventType != "")
            {
                if (eventTypes[eventType].Count == 0)
                {
                    eventTypes.Remove(eventType);
                }
            }
            else
            {
                string[] keys = eventTypes.Keys.ToArray<string>();

                foreach (string key in keys)
                {
                    if (eventTypes[key].Count == 0)
                    {
                        eventTypes.Remove(key);
                    }
                }
            }
        }
    }
}