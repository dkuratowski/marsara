using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Instances of this class send clock signal to its target.
    /// </summary>
    public class ClockSignal : IDisposable
    {
        /// <summary>
        /// Prototype of tick handler methods.
        /// </summary>
        public delegate void TickHandler();

        /// <summary>
        /// Constructs a ClockSignal instance.
        /// </summary>
        /// <param name="msPerSignal">The elapsed milliseconds between signals.</param>
        public ClockSignal(int msPerSignal)
        {
            if (msPerSignal <= 0) { throw new ArgumentOutOfRangeException("msPerSignal"); }
            this.msPerSignal = msPerSignal;
            this.timeAccumulator = 0;
            this.targets = new HashSet<TickHandler>();
        }

        /// <summary>
        /// Adds a target to this signal.
        /// </summary>
        /// <param name="target">The target to be added.</param>
        public void AddTarget(TickHandler target)
        {
            if (target == null) { throw new ArgumentNullException("target"); }
            if (this.targets.Contains(target)) { throw new InvalidOperationException("The target already added to the clock signal!"); }

            if (this.targets.Count == 0)
            {
                this.timeAccumulator = 0;
                UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnSystemUpdate);
            }
            this.targets.Add(target);
        }

        /// <summary>
        /// Removes a target from this signal.
        /// </summary>
        /// <param name="target">The target to be removed.</param>
        public void RemoveTarget(TickHandler target)
        {
            if (target == null) { throw new ArgumentNullException("target"); }
            if (!this.targets.Contains(target)) { throw new InvalidOperationException("The target is not added to the clock signal!"); }

            this.targets.Remove(target);
            if (this.targets.Count == 0)
            {
                UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnSystemUpdate);
            }
        }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnSystemUpdate);
        }

        #endregion IDisposable methods

        /// <summary>
        /// System update handler function.
        /// </summary>
        /// <param name="evtArgs">System time informations.</param>
        private void OnSystemUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            this.timeAccumulator += evtArgs.TimeSinceLastUpdate;
            while (this.timeAccumulator >= this.msPerSignal)
            {
                this.timeAccumulator -= this.msPerSignal;
                List<TickHandler> targetsCopy = new List<TickHandler>(this.targets);
                foreach (TickHandler target in targetsCopy) { target(); }
            }
        }

        /// <summary>
        /// The elapsed milliseconds between signals.
        /// </summary>
        private int msPerSignal;

        /// <summary>
        /// The time accumulator.
        /// </summary>
        private int timeAccumulator;

        /// <summary>
        /// List of the targets of this clock signal.
        /// </summary>
        private HashSet<TickHandler> targets;
    }
}
