using System;
using System.Collections.Generic;

namespace RC.UI
{
    /// <summary>
    /// Interface of event argument carrier classes.
    /// </summary>
    interface IUIEventCarrier
    {
        /// <summary>
        /// Posts the carried event arguments to the corresponding handler functions.
        /// </summary>
        void Post();
    }

    /// <summary>
    /// Event argument carrier class of event type T.
    /// </summary>
    class UIEventCarrier<T> : IUIEventCarrier where T : UIEventArgs
    {
        /// <summary>
        /// Constructs a UIEventCarrier object.
        /// </summary>
        public UIEventCarrier(T carriedEvtArgs, IUIEventQueue queue)
        {
            if (carriedEvtArgs == null) { throw new ArgumentNullException("carriedEvtArgs"); }
            if (queue == null) { throw new ArgumentNullException("queue"); }
            this.carriedEvtArgs = carriedEvtArgs;
            this.correspondingQueue = queue;
        }

        /// <see cref="UIEventCarrier.Post"/>
        public void Post()
        {
            UIEventDispatcher<T>.Dispatch(this.correspondingQueue, this.carriedEvtArgs);
        }

        /// <summary>
        /// The arguments of the carried event.
        /// </summary>
        private T carriedEvtArgs;

        /// <summary>
        /// The corresponding event queue.
        /// </summary>
        private IUIEventQueue correspondingQueue;
    }

    /// <summary>
    /// The default implementation of the IUIEventQueue interface.
    /// </summary>
    public class UIEventQueue : IUIEventQueue
    {
        /// <summary>
        /// Constructs a UIEventQueue object.
        /// </summary>
        public UIEventQueue()
        {
            this.eventQueue = new List<IUIEventCarrier>();
            this.posting = false;
        }

        /// <see cref="IUIEventQueue.Subscribe<T>"/>
        public void Subscribe<T>(UIEventHandler<T> handlerFunc) where T : UIEventArgs
        {
            UIEventDispatcher<T>.Subscribe(this, handlerFunc);
        }

        /// <see cref="IUIEventQueue.Unsubscribe<T>"/>
        public void Unsubscribe<T>(UIEventHandler<T> handlerFunc) where T : UIEventArgs
        {
            UIEventDispatcher<T>.Unsubscribe(this, handlerFunc);
        }

        /// <see cref="IUIEventQueue.EnqueueEvent<T>"/>
        public void EnqueueEvent<T>(T evtArgs) where T : UIEventArgs
        {
            if (this.posting) { throw new UIException("Illegal call on IUIEventQueue.EnqueueEvent<T> during posting."); }
            if (evtArgs == null) { throw new ArgumentNullException("evtArgs"); }
            this.eventQueue.Add(new UIEventCarrier<T>(evtArgs, this));
        }

        /// <see cref="IUIEventQueue.PostEvents"/>
        public void PostEvents()
        {
            if (this.posting) { throw new UIException("Recursive call on IUIEventQueue.PostEvents."); }
            this.posting = true;
            foreach (IUIEventCarrier evtCarrier in this.eventQueue)
            {
                evtCarrier.Post();
            }
            this.eventQueue.Clear();
            this.posting = false;
        }

        /// <summary>
        /// The list of the enqueued events.
        /// </summary>
        private List<IUIEventCarrier> eventQueue;

        /// <summary>
        /// Flag to avoid illegal calls to UIEventQueue.EnqueueEvent<T> when events are being posted to the handlers.
        /// </summary>
        private bool posting;
    }
}
