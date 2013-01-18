using System;
using System.Collections.Generic;

namespace RC.UI
{
    /// <summary>
    /// Interface of the UI event queue.
    /// </summary>
    public interface IUIEventQueue
    {
        /// <summary>
        /// Subscribes the given handler function to events of type T.
        /// </summary>
        /// <param name="handlerFunc">The handler function to subscribe.</param>
        void Subscribe<T>(UIEventHandler<T> handlerFunc) where T : UIEventArgs;

        /// <summary>
        /// Unsubscribes the given handler function from events of type T.
        /// </summary>
        /// <param name="handlerFunc">The handler function to unsubscribe.</param>
        void Unsubscribe<T>(UIEventHandler<T> handlerFunc) where T : UIEventArgs;

        /// <summary>
        /// Puts an event into the queue.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="evtArgs">The event arguments.</param>
        void EnqueueEvent<T>(T evtArgs) where T : UIEventArgs;

        /// <summary>
        /// Posts every enqueued event to the corresponding handler functions.
        /// </summary>
        void PostEvents();
    }

    /// <summary>
    /// Base class of classes containing event data.
    /// </summary>
    public class UIEventArgs
    {
        /// <summary>
        /// Represents an event with no event data.
        /// </summary>
        public static UIEventArgs Empty = new UIEventArgs();
    }

    /// <summary>
    /// Represents the method that will handle an event.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the event that the function must handle. Has to be derived from UIEventArgs.
    /// </typeparam>
    /// <param name="eventArgs">The arguments of the event.</param>
    public delegate void UIEventHandler<T>(T eventArgs) where T : UIEventArgs;

    /// <summary>
    /// Singleton event dispatcher class that is used to dispatch incoming events of type T to the registered event handler functions.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the events that the dispatcher is responsible for. Has to be derived from UIEventArgs.
    /// </typeparam>
    static class UIEventDispatcher<T> where T : UIEventArgs
    {
        /// <summary>
        /// Subscribes the given handler function to events of type T.
        /// </summary>
        /// <param name="queue">The subscribing event queue.</param>
        /// <param name="handlerFunc">The handler function to subscribe.</param>
        public static void Subscribe(IUIEventQueue queue, UIEventHandler<T> handlerFunc)
        {
            if (queue == null) { throw new ArgumentNullException("queue"); }
            if (handlerFunc == null) { throw new ArgumentNullException("handlerFunc"); }

            if (!handlers.ContainsKey(queue))
            {
                handlers.Add(queue, new HashSet<UIEventHandler<T>>());
            }

            handlers[queue].Add(handlerFunc);
        }

        /// <summary>
        /// Unsubscribes the given handler function from events of type T.
        /// </summary>
        /// <param name="queue">The unsubscribing event queue.</param>
        /// <param name="handlerFunc">The handler function to unsubscribe.</param>
        public static void Unsubscribe(IUIEventQueue queue, UIEventHandler<T> handlerFunc)
        {
            if (queue == null) { throw new ArgumentNullException("queue"); }
            if (handlerFunc == null) { throw new ArgumentNullException("handlerFunc"); }

            if (handlers.ContainsKey(queue))
            {
                handlers[queue].Remove(handlerFunc);
                if (handlers[queue].Count == 0)
                {
                    handlers.Remove(queue);
                }
            }
        }

        /// <summary>
        /// Calls the registered event handler functions with the given event arguments.
        /// </summary>
        /// <param name="queue">The event queue that want to dispatch.</param>
        /// <param name="evtArgs">The event arguments.</param>
        public static void Dispatch(IUIEventQueue queue, T evtArgs)
        {
            if (queue == null) { throw new ArgumentNullException("queue"); }
            if (evtArgs == null) { throw new ArgumentNullException("evtArgs"); }
            if (dispatching) { throw new UIException("Recursive call on UIEventDispatcher<T>.Dispatch"); }

            if (handlers.ContainsKey(queue))
            {
                dispatching = true;

                UIEventHandler<T>[] handlersCopy = new UIEventHandler<T>[handlers[queue].Count];
                int i = 0;
                foreach (UIEventHandler<T> handler in handlers[queue])
                {
                    handlersCopy[i] = handler;
                    i++;
                }

                foreach (UIEventHandler<T> handler in handlersCopy)
                {
                    handler(evtArgs);
                }

                dispatching = false;
            }
        }

        /// <summary>
        /// List of the subscribed event handler functions mapped by the event queues.
        /// </summary>
        private static Dictionary<IUIEventQueue, HashSet<UIEventHandler<T>>> handlers =
            new Dictionary<IUIEventQueue, HashSet<UIEventHandler<T>>>();

        /// <summary>
        /// Flag to avoid recursive call on UIEventDispatcher<T>.Dispatch.
        /// </summary>
        private static bool dispatching = false;
    }
}
