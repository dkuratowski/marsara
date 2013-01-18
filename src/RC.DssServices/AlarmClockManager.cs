using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// Interface to indicate when the timeout thread of an AlarmClock has been finished.
    /// </summary>
    interface IAlarmClockFinished
    {
        /// <summary>
        /// Indicates that the timeout thread of the given AlarmClock has been timed out or stopped.
        /// </summary>
        void AlarmClockFinished(AlarmClock whichClock);
    }

    /// <summary>
    /// Interface to call the function corresponding to the given AlarmClock.
    /// </summary>
    interface IAlarmClockInvoke
    {
        /// <summary>
        /// Calls the alarm function corresponding to the given AlarmClock if necessary.
        /// </summary>
        /// <param name="whichClock">The clock whose alarm function you want to call.</param>
        /// <param name="invoke">True if the alarm function should be called, false if not.</param>
        void InvokeIfNecessary(AlarmClock whichClock, bool invoke);

        /// <summary>
        /// Cancels the given AlarmClock.
        /// </summary>
        /// <param name="whichClock">The AlarmClock you want to cancel.</param>
        void CancelAlarmClock(AlarmClock whichClock);
    }

    /// <summary>
    /// When you call AlarmClockManager.SetAlarmClock from the DSS-thread, the AlarmClockManager class allocates a free clock
    /// and returns a wrapper over that clock. This class represents such a wrapper.
    /// </summary>
    class AlarmClock
    {
        /// <summary>
        /// Constructs an alarm clock object.
        /// </summary>
        public AlarmClock(int targetTime, IAlarmClockInvoke invokeIface)
        {
            if (targetTime < 0) { throw new ArgumentOutOfRangeException("targetTime"); }
            if (invokeIface == null) { throw new ArgumentNullException("invokeIface"); }

            this.dssThread = RCThread.CurrentThread;
            this.targetTime = targetTime;
            this.isActive = true;
            this.invokeIface = invokeIface;

            this.testID = nextID; /// TODO: delete this.testID
            nextID++;
        }

        /// <summary>
        /// You can cancel the underlying alarm clock by calling this function from the DSS-thread.
        /// </summary>
        public void Cancel()
        {
            if (this.dssThread != RCThread.CurrentThread) { throw new DssException("AlarmClock is only accessable from the DSS-thread!"); }

            if (this.isActive)
            {
                this.isActive = false;
                this.invokeIface.CancelAlarmClock(this);
            }
            TraceManager.WriteAllTrace(string.Format("AlarmClock-{0} cancelled", this.testID), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
        }

        /// <summary>
        /// Invokes the function corresponding to this alarm clock if necessary. If the function has already been
        /// invoked then this function has no effect.
        /// </summary>
        public void InvokeIfNecessary()
        {
            if (this.dssThread != RCThread.CurrentThread) { throw new DssException("AlarmClock is only accessable from the DSS-thread!"); }

            TraceManager.WriteAllTrace(string.Format("AlarmClock-{0}.Invoke", this.testID), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
            this.invokeIface.InvokeIfNecessary(this, this.isActive);
            this.isActive = false;
        }

        /// <summary>
        /// Represents the DSS-thread.
        /// </summary>
        private RCThread dssThread;

        /// <summary>
        /// The underlying alarm clock has been set to this time.
        /// </summary>
        /// <remarks>
        /// There can be small differences between this target time and the real time of the invocation because
        /// of thread synchronization.
        /// </remarks>
        private int targetTime;

        /// <summary>
        /// This flag indicates whether the alarm clock is active or not.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Interface to invoke the function corresponding to this alarm clock.
        /// </summary>
        private IAlarmClockInvoke invokeIface;

        /// <summary>
        /// This random number is used to identify the AlarmClock objects.
        /// </summary>
        public readonly int testID;
        private static int nextID = 0;
    }

    /// <summary>
    /// This class is responsible to let the DSS-thread to call given functions in given times.
    /// </summary>
    class AlarmClockManager : IAlarmClockInvoke, IDisposable
    {
        /// <summary>
        /// Prototype of functions that can be called by this alarm clock.
        /// </summary>
        /// <param name="whichClock">The alarm clock that caused the invocation.</param>
        /// <param name="param">The parameter of the invocation.</param>
        public delegate void AlarmFunction(AlarmClock whichClock, object param);

        /// <summary>
        /// Constructs an alarm clock object.
        /// </summary>
        public AlarmClockManager(IAlarmClockFinished alarmClkFinishedIface)
        {
            if (null == alarmClkFinishedIface) { throw new ArgumentNullException("alarmClkFinishedIface"); }

            this.dssThread = RCThread.CurrentThread;
            this.alarmClkFinishedIface = alarmClkFinishedIface;
            this.timers = new Fifo<AlarmClockThread>(DssConstants.INITIAL_ALARM_CLOCK_CAPACITY);
            this.currentCapacity = DssConstants.INITIAL_ALARM_CLOCK_CAPACITY;
            this.targetFunctions = new Dictionary<AlarmClockThread, AlarmFunction>();
            this.parameters = new Dictionary<AlarmClockThread, object>();
            this.alarmClocks = new Dictionary<AlarmClockThread, AlarmClock>();
            this.clockTimerMap = new Dictionary<AlarmClock, AlarmClockThread>();
            this.lockObj = new object();

            for (int i = 0; i < this.currentCapacity; i++)
            {
                this.timers.Push(new AlarmClockThread());
                TraceManager.WriteAllTrace(string.Format("Push timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
            }
        }

        /// <summary>
        /// Asks the AlarmClockManager to call the given function at the given time.
        /// </summary>
        /// <param name="when">The time when the call must be performed.</param>
        /// <param name="funcToCall">The function that must be called.</param>
        /// <returns>A reference to the allocated alarm clock.</returns>
        public AlarmClock SetAlarmClock(int when, AlarmFunction funcToCall)
        {
            return SetAlarmClock(when, funcToCall, null);
        }

        /// <summary>
        /// Asks the AlarmClockManager to call the given function at the given time with the given parameter.
        /// </summary>
        /// <param name="when">The time when the call must be performed.</param>
        /// <param name="funcToCall">The function that must be called.</param>
        /// <param name="param">The parameter of the function to call with.</param>
        /// <returns>A reference to the allocated alarm clock.</returns>
        public AlarmClock SetAlarmClock(int when, AlarmFunction funcToCall, object param)
        {
            if (this.dssThread != RCThread.CurrentThread) { throw new DssException("AlarmClockManager.SetAlarmClock must be called from the DSS-thread!"); }
            if (funcToCall == null) { throw new ArgumentNullException("funcToCall"); }
            if (when < 0) { throw new ArgumentOutOfRangeException("when"); }

            int currentTime = DssRoot.Time;
            if (when > currentTime)
            {
                /// Delayed invoke
                lock (this.lockObj)
                {
                    if (this.timers.Length == 0)
                    {
                        /// No free timer found --> create a new one
                        AlarmClockThread newTimer = new AlarmClockThread();
                        this.currentCapacity++;
                        this.timers = new Fifo<AlarmClockThread>(currentCapacity);
                        this.timers.Push(newTimer);
                        TraceManager.WriteAllTrace(string.Format("Delayed: New timer: {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                        TraceManager.WriteAllTrace(string.Format("Push timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    }
                    else
                    {
                        TraceManager.WriteAllTrace("Delayed: clock reuse", DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    }

                    AlarmClockThread timerToUse = this.timers.Get();
                    TraceManager.WriteAllTrace(string.Format("Get timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    AlarmClock retClock = new AlarmClock(when, this);
                    this.targetFunctions.Add(timerToUse, funcToCall);
                    this.parameters.Add(timerToUse, param);
                    this.alarmClocks.Add(timerToUse, retClock);
                    this.clockTimerMap.Add(retClock, timerToUse);

                    timerToUse.Start(when - currentTime, this.AlarmClockThreadFinishedHdl, this.AlarmClockThreadFinishedHdl);
                    TraceManager.WriteAllTrace(string.Format("AlarmClock-{0} started", retClock.testID), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    return retClock;
                }
            }
            else
            {
                /// Immediate invoke
                lock (this.lockObj)
                {
                    if (this.timers.Length == 0)
                    {
                        /// No free timer found --> create a new one
                        AlarmClockThread newTimer = new AlarmClockThread();
                        this.currentCapacity++;
                        this.timers = new Fifo<AlarmClockThread>(currentCapacity);
                        this.timers.Push(newTimer);
                        TraceManager.WriteAllTrace(string.Format("Immediate: New timer: {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                        TraceManager.WriteAllTrace(string.Format("Push timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    }
                    else
                    {
                        TraceManager.WriteAllTrace("Immediate: clock reuse", DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    }

                    AlarmClockThread timerToUse = this.timers.Get();
                    TraceManager.WriteAllTrace(string.Format("Get timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    AlarmClock retClock = new AlarmClock(when, this);
                    this.targetFunctions.Add(timerToUse, funcToCall);
                    this.parameters.Add(timerToUse, param);
                    this.alarmClocks.Add(timerToUse, retClock);
                    this.clockTimerMap.Add(retClock, timerToUse);

                    TraceManager.WriteAllTrace(string.Format("AlarmClock-{0} started immediately", retClock.testID), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    this.alarmClkFinishedIface.AlarmClockFinished(retClock);
                    return retClock;
                }
            }
        }

        #region IAlarmClockInvoke interface members

        /// <see cref="IAlarmClockInvoke.InvokeIfNecessary"/>
        public void InvokeIfNecessary(AlarmClock whichClock, bool invoke)
        {
            if (this.dssThread != RCThread.CurrentThread) { throw new DssException("AlarmClockManager.InvokeIfNecessary must be called from the DSS-thread!"); }

            lock (this.lockObj)
            {
                if (this.clockTimerMap.ContainsKey(whichClock))
                {
                    AlarmClockThread correspondingTimer = this.clockTimerMap[whichClock];
                    if (this.alarmClocks.ContainsKey(correspondingTimer) &&
                        this.alarmClocks[correspondingTimer] == whichClock &&
                        this.targetFunctions.ContainsKey(correspondingTimer) &&
                        this.parameters.ContainsKey(correspondingTimer))
                    {
                        TraceManager.WriteAllTrace(string.Format("Invoking alarm clock @ {0}", DssRoot.Time), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                        if (invoke)
                        {
                            this.targetFunctions[correspondingTimer](whichClock, this.parameters[correspondingTimer]);
                        }
                        this.clockTimerMap.Remove(whichClock);
                        this.alarmClocks.Remove(correspondingTimer);
                        this.targetFunctions.Remove(correspondingTimer);
                        this.parameters.Remove(correspondingTimer);

                        this.timers.Push(correspondingTimer); /// Put the timer back to the FIFO
                        TraceManager.WriteAllTrace(string.Format("Push timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    }
                    else
                    {
                        TraceManager.WriteAllTrace("Warning! Invalid alarm clock invocation!", DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                    }
                }
                else
                {
                    TraceManager.WriteAllTrace("Warning! Invalid alarm clock invocation!", DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                }
            }
        }

        /// <see cref="IAlarmClockInvoke.CancelAlarmClock"/>
        public void CancelAlarmClock(AlarmClock whichClock)
        {
            if (this.dssThread != RCThread.CurrentThread) { throw new DssException("AlarmClockManager.CancelAlarmClock must be called from the DSS-thread!"); }

            lock (this.lockObj)
            {
                if (this.clockTimerMap.ContainsKey(whichClock))
                {
                    AlarmClockThread correspondingTimer = this.clockTimerMap[whichClock];
                    correspondingTimer.Stop();
                }
                else
                {
                    TraceManager.WriteAllTrace("Warning! Invalid alarm clock cancel!", DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                }
            }
        }

        #endregion

        #region IDisposable interface members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.dssThread != RCThread.CurrentThread) { throw new DssException("AlarmClockManager.Dispose must be called from the DSS-thread!"); }

            /// Stop and dispose every unused timer.
            int numOfTimers = this.timers.Length;
            for (int i = 0; i < numOfTimers; i++)
            {
                AlarmClockThread timer = this.timers.Get();
                TraceManager.WriteAllTrace(string.Format("Get timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                timer.Stop();
                timer.Dispose();
                this.timers.Push(timer);
                TraceManager.WriteAllTrace(string.Format("Push timer - {0}", this.timers.Length), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
            }

            /// Stop and dispose every timer that are currently in use.
            foreach (KeyValuePair<AlarmClockThread, AlarmClock> item in this.alarmClocks)
            {
                AlarmClockThread timer = item.Key;
                timer.Stop();
                timer.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// This function is called by the underlying timers.
        /// </summary>
        /// <param name="whichTimer">Reference to the timer that caused this function call.</param>
        private void AlarmClockThreadFinishedHdl(AlarmClockThread whichTimer)
        {
            AlarmClock whichClock = null;

            lock (this.lockObj)
            {
                if (this.targetFunctions.ContainsKey(whichTimer) &&
                    this.parameters.ContainsKey(whichTimer) &&
                    this.alarmClocks.ContainsKey(whichTimer) &&
                    this.clockTimerMap.ContainsKey(this.alarmClocks[whichTimer]) &&
                    this.clockTimerMap[this.alarmClocks[whichTimer]] == whichTimer)
                {
                    whichClock = this.alarmClocks[whichTimer];
                }
            }

            if (whichClock != null)
            {
                TraceManager.WriteAllTrace(string.Format("AlarmClock-{0} timeout", whichClock.testID), DssTraceFilters.ALARM_CLOCK_MANAGER_INFO);
                this.alarmClkFinishedIface.AlarmClockFinished(whichClock);
            }
            else
            {
                throw new DssException("Invalid timer call.");
            }
        }

        /// <summary>
        /// List of the timers.
        /// </summary>
        private Fifo<AlarmClockThread> timers;

        /// <summary>
        /// The current capacity of the timer FIFO.
        /// </summary>
        private int currentCapacity;

        /// <summary>
        /// Maps each timer to it's corresponding alarm function.
        /// </summary>
        private Dictionary<AlarmClockThread, AlarmFunction> targetFunctions;

        /// <summary>
        /// Maps each timer to the parameter of it's corresponding alarm function.
        /// </summary>
        private Dictionary<AlarmClockThread, object> parameters;

        /// <summary>
        /// Maps each timer to the corresponding alarm clock object.
        /// </summary>
        private Dictionary<AlarmClockThread, AlarmClock> alarmClocks;

        /// <summary>
        /// Maps each alarm clock to the corresponding timer object.
        /// </summary>
        private Dictionary<AlarmClock, AlarmClockThread> clockTimerMap;

        /// <summary>
        /// This object is used to synchronize the DSS-thread and the timer threads.
        /// </summary>
        private object lockObj;

        /// <summary>
        /// The interface of the DSS-thread to indicate when an alarm clock thread has been finished.
        /// </summary>
        private IAlarmClockFinished alarmClkFinishedIface;

        /// <summary>
        /// The thread that created this manager object.
        /// </summary>
        private RCThread dssThread;
    }
}
