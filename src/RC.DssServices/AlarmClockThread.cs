using System;
using System.Threading;
using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// This class is responsible for call a given function after a given amount of time.
    /// </summary>
    public class AlarmClockThread : IDisposable
    {
        /// <summary>
        /// Prototype of the callback functions that will be called by the AlarmClockThread in case of timeout.
        /// </summary>
        /// <param name="whichThread">Reference to the AlarmClockThread object that called the callback function.</param>
        public delegate void TimeoutHandler(AlarmClockThread whichThread);

        /// <summary>
        /// Prototype of the callback functions that will be called by the AlarmClockThread in case of stop.
        /// </summary>
        /// <param name="whichThread">Reference to the AlarmClockThread object that called the callback function.</param>
        public delegate void StopHandler(AlarmClockThread whichThread);

        /// <summary>
        /// Constructs an AlarmClockThread object.
        /// </summary>
        public AlarmClockThread()
        {
            this.disposed = false;
            this.currentTimeoutValue = -1;
            this.currentTimeoutHandler = null;
            this.timerRunning = false;

            this.lockObject = new object();
            this.disposeEvent = new ManualResetEvent(false);
            this.startEvent = new ManualResetEvent(false);
            this.stopEvent = new ManualResetEvent(false);

            this.timeoutThread = new RCThread(this.TimeoutThreadProc, "AlarmClock");
            this.timeoutThread.Start();
        }

        /// <summary>
        /// Starts the timer with the given timeout value and timeout handler function.
        /// </summary>
        /// <param name="timeoutValue">
        /// The timeout handler function will be called by the AlarmClockThread if this amount of time elapses without stopping
        /// the timer object.
        /// </param>
        /// <param name="timeoutHandler">The handler function that will be called in case of timeout.</param>
        /// <param name="stopHandler">The handler function that will be called in case of stop.</param>
        /// <remarks>
        /// This function can only be called from another thread.
        /// </remarks>
        public void Start(int timeoutValue, TimeoutHandler timeoutHandler, StopHandler stopHandler)
        {
            if (timeoutValue < 0) { throw new ArgumentOutOfRangeException("timeoutValue"); }
            if (timeoutHandler == null) { throw new ArgumentNullException("timeoutHandler"); }
            if (stopHandler == null) { throw new ArgumentNullException("stopHandler"); }

            lock (this.lockObject)
            {
                if (this.disposed) { throw new ObjectDisposedException("AlarmClockThread"); }
                if (RCThread.CurrentThread == this.timeoutThread) { throw new DssException("Unable to access AlarmClockThread.Start from the current thread!"); }
                if (this.timerRunning) { throw new DssException("The timer is currently running!"); }

                this.currentTimeoutValue = timeoutValue;
                this.currentTimeoutHandler = timeoutHandler;
                this.currentStopHandler = stopHandler;
                this.timerRunning = true;
                this.startEvent.Set();
            }
        }

        /// <summary>
        /// Stops the AlarmClockThread.
        /// </summary>
        /// <remarks>
        /// This function can only be called from another thread. If the AlarmClockThread is not running then this function
        /// has no effect.
        /// </remarks>
        public void Stop()
        {
            lock (this.lockObject)
            {
                if (this.disposed) { throw new ObjectDisposedException("AlarmClockThread"); }
                if (RCThread.CurrentThread == this.timeoutThread) { throw new DssException("Unable to access AlarmClockThread.Stop from the current thread!"); }

                if (this.timerRunning)
                {
                    this.stopEvent.Set();
                    this.timerRunning = false;
                }
            }
        }

        #region IDisposable Members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            lock (this.lockObject)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.disposeEvent.Set();
                    this.stopEvent.Set();
                }
            }

            this.timeoutThread.Join();
            this.disposeEvent.Close();
            this.startEvent.Close();
            this.stopEvent.Close();
        }

        #endregion

        /// <summary>
        /// The starting function of the timeout thread.
        /// </summary>
        private void TimeoutThreadProc()
        {
            //Console.WriteLine("TimeoutThreadProc: begin");
            while (true)
            {
                int startedOrDisposed = EventWaitHandle.WaitAny(new WaitHandle[2] { this.startEvent, this.disposeEvent });
                if (0 == startedOrDisposed)
                {
                    /// The timer has been started.
                    lock (this.lockObject)
                    {
                        //Console.WriteLine("TimeoutThreadProc: start");
                        this.startEvent.Reset();
                        this.disposeEvent.Reset();
                        this.stopEvent.Reset();
                    }

                    /// Wait for stop event or timeout.
                    if (!this.stopEvent.WaitOne(this.currentTimeoutValue))
                    {
                        /// Timeout
                        lock (this.lockObject)
                        {
                            //Console.WriteLine("TimerThreadProc: timeout");
                            this.currentTimeoutHandler(this);
                            this.timerRunning = false;
                        }
                    }
                    else
                    {
                        /// Stop
                        lock (this.lockObject)
                        {
                            //Console.WriteLine("TimerThreadProc: stop");
                            this.currentStopHandler(this);
                            this.timerRunning = false;
                        }
                    }
                }
                else
                {
                    /// The timer has been disposed.
                    break;
                }
            }
            //Console.WriteLine("TimerThreadProc: end");
        }

        /// <summary>
        /// The underlying thread that will call the timeout handler.
        /// </summary>
        private RCThread timeoutThread;

        /// <summary>
        /// This event is fired when the alarm clock is being started.
        /// </summary>
        private ManualResetEvent startEvent;

        /// <summary>
        /// This event is fired when the alarm clock is being stopped.
        /// </summary>
        private ManualResetEvent stopEvent;

        /// <summary>
        /// This event is fired when the alarm clock is being disposed.
        /// </summary>
        private ManualResetEvent disposeEvent;

        /// <summary>
        /// This flag becomes true if the alarm clock has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Use this object as a mutex.
        /// </summary>
        private object lockObject;

        /// <summary>
        /// The currently set timeout in milliseconds.
        /// </summary>
        private int currentTimeoutValue;

        /// <summary>
        /// The currently set timeout handler function.
        /// </summary>
        private TimeoutHandler currentTimeoutHandler;

        /// <summary>
        /// The currently set stop handler function.
        /// </summary>
        private StopHandler currentStopHandler;

        /// <summary>
        /// This flag is true if the timer is currently running.
        /// </summary>
        private bool timerRunning;
    }
}