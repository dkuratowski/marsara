using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Implementation of the simulation scheduler.
    /// </summary>
    public class Scheduler : IDisposable
    {
        /// <summary>
        /// Prototype of simulator methods to execute on signal.
        /// </summary>
        public delegate void SimulatorFunc();

        /// <summary>
        /// Creates a Scheduler object.
        /// </summary>
        /// <param name="minMsPerFrame">The minimum amount of time between simulation frames in milliseconds.</param>
        /// <param name="signaled">True if this is a signaled scheduler, false otherwise.</param>
        public Scheduler(int minMsPerFrame, bool signaled)
        {
            if (minMsPerFrame <= 0) { throw new ArgumentOutOfRangeException("minMsPerFrame"); }

            this.isDisposed = false;
            this.isSignaled = signaled;
            this.minMsPerFrame = minMsPerFrame;
            this.timeAccumulator = 0;
            this.simulationSignal = false;
            this.evtSimulationStarted = this.isSignaled ? new AutoResetEvent(false) : null;
            this.simulatorMethods = new HashSet<SimulatorFunc>();
        }

        /// <summary>
        /// Adds a simulator method to this scheduler.
        /// </summary>
        /// <param name="simulatorMethod">The simulator method to be added.</param>
        public void AddSimulatorMethod(SimulatorFunc simulatorMethod)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("Scheduler"); }
            if (simulatorMethod == null) { throw new ArgumentNullException("simulatorMethod"); }
            if (this.simulatorMethods.Contains(simulatorMethod)) { throw new InvalidOperationException("The simulator method already added to the scheduler!"); }
            this.simulatorMethods.Add(simulatorMethod);
        }

        /// <summary>
        /// Removes a simulator method from this scheduler.
        /// </summary>
        /// <param name="simulatorMethod">The simulator method to be removed.</param>
        public void RemoveSimulatorMethod(SimulatorFunc simulatorMethod)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("Scheduler"); }
            if (simulatorMethod == null) { throw new ArgumentNullException("target"); }
            if (!this.simulatorMethods.Contains(simulatorMethod)) { throw new InvalidOperationException("The simulator method is not added to the scheduler!"); }
            this.simulatorMethods.Remove(simulatorMethod);
        }

        /// <summary>
        /// Call this function on every system updates.
        /// </summary>
        /// <param name="timeSinceLastUpdate">The elapsed time since the last system update in milliseconds.</param>
        public void SystemUpdate(int timeSinceLastUpdate)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("Scheduler"); }

            this.timeAccumulator += timeSinceLastUpdate;
            if (this.timeAccumulator < this.minMsPerFrame) { return; }
            if (this.isSignaled && !this.simulationSignal) { return; }

            this.timeAccumulator = 0;

            if (this.isSignaled)
            {
                this.simulationSignal = false;
                this.evtSimulationStarted.Set();
            }

            List<SimulatorFunc> simulatorMethodsCopy = new List<SimulatorFunc>(this.simulatorMethods);
            foreach (SimulatorFunc simulatorMethod in simulatorMethodsCopy) { simulatorMethod(); }
        }

        /// <summary>
        /// Sends a signal to this scheduler.
        /// </summary>
        public void SendSignal()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("Scheduler"); }
            if (!this.isSignaled) { throw new InvalidOperationException("This is not a signaled scheduler!"); }
            if (this.simulationSignal) { throw new InvalidOperationException("The scheduler has already been signaled!"); }

            this.simulationSignal = true;
            this.evtSimulationStarted.WaitOne();
        }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                if (this.isSignaled)
                {
                    this.evtSimulationStarted.Set();
                    this.evtSimulationStarted.Close();
                }

                this.isDisposed = true;
            }
        }

        #endregion IDisposable methods

        /// <summary>
        /// This flag indicates if this scheduler has been disposed.
        /// </summary>
        private volatile bool isDisposed;

        /// <summary>
        /// Signal for simulating the next frame.
        /// </summary>
        private volatile bool simulationSignal;

        /// <summary>
        /// True if this is a signaled scheduler, false otherwise.
        /// </summary>
        private volatile bool isSignaled;

        /// <summary>
        /// Semaphore for indicating that a simulation frame has been started.
        /// </summary>
        private AutoResetEvent evtSimulationStarted;

        /// <summary>
        /// The minimum amount of time between simulation frames in milliseconds.
        /// </summary>
        private int minMsPerFrame;

        /// <summary>
        /// The time accumulator.
        /// </summary>
        private int timeAccumulator;

        /// <summary>
        /// List of the simulator methods to execute when signaled.
        /// </summary>
        private HashSet<SimulatorFunc> simulatorMethods;
    }
}
