using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using RC.Common.PNService;

namespace RC.DssServices
{
    /// <summary>
    /// This timer class is used to get a notification if a given amount of time elapses without stopping
    /// the timer.
    /// </summary>
    public class AdvancedTimer
    {
        /// <summary>
        /// Delegate that defines the type of the function that will be called in case of timeout.
        /// </summary>
        /// <param name="whichObject">The timer object caused the timeout event.</param>
        public delegate void TimeoutHandler(AdvancedTimer whichObject);

        /// <summary>
        /// Constructs a new timer object.
        /// </summary>
        public AdvancedTimer()
        {
            /// Construct the Petri-network that will control the timer.
            this.ConstructPetriNetwork();

            this.timerThreadCallbacks = new Dictionary<int, PetriNet.PNCallback>();
            timerThreadCallbacks.Add(TR1_STOP_HANDLED, this.TimerThreadCallback);
            timerThreadCallbacks.Add(TR1_TIMEOUT_HANDLED, this.TimerThreadCallback);
            timerThreadCallbacks.Add(TR1_TIMEOUT_HANDLED2, this.TimerThreadCallback);
            timerThreadCallbacks.Add(TR1_START_END, this.TimerThreadCallback);

            this.callerThreadCallbacks = new Dictionary<int, PetriNet.PNCallback>();
            this.callerThreadCallbacks.Add(TR0_STP_RE_FAIL, this.CallerThreadCallback);
            this.callerThreadCallbacks.Add(TR0_STP_RE_SUCC, this.CallerThreadCallback);
            this.callerThreadCallbacks.Add(TR0_STRT_RE_ERR, this.CallerThreadCallback);
            this.callerThreadCallbacks.Add(TR0_STRT_RE_OK, this.CallerThreadCallback);

            this.extTransitionsTimerThreadStart = new int[1] { TR1_STRT_BEGIN };
            this.extTransitionsTimerThreadStop = new int[1] { TR1_STOP_EVT };
            this.extTransitionsTimerThreadTimeout = new int[2] { TR1_TIMEOUT1, TR1_TIMEOUT2 };
            this.extTransitionsCallerThreadStart = new int[1] { TR0_START };
            this.extTransitionsCallerThreadStop = new int[1] { TR0_STOP };

            this.disposeTimer = new ManualResetEvent(false);
            this.stopTimer = new ManualResetEvent(false);
            this.startTimer = new ManualResetEvent(false);
            this.stopSuccessFlag = false;

            this.timerDisposed = false;
            this.currentTimeoutValue = -1;
            this.currentTimeoutHandler = null;
            this.timerThread = new RCThread(this.TimerThreadProc, "AdvancedTimer");
            this.timerThread.Start();
        }

        /// <summary>
        /// Starts the timer with the given timeout value and timeout handler function.
        /// </summary>
        /// <param name="timeoutValue">
        /// The timeout handler function will be called by the timer thread if this amount of time elapses without stopping
        /// the timer object.
        /// </param>
        /// <param name="timeoutHandler">The handler function that will be called in case of timeout.</param>
        public void Start(int timeoutValue, TimeoutHandler timeoutHandler)
        {
            if (this.timerDisposed) { throw new ObjectDisposedException("AdvancedTimer"); }
            if (timeoutValue > 0)
            {
                if (timeoutHandler != null)
                {
                    this.currentTimeoutValue = timeoutValue;
                    this.currentTimeoutHandler = timeoutHandler;
                    this.startTimer.Set();
                    this.controllerPN.AttachThread(this.extTransitionsCallerThreadStart, this.callerThreadCallbacks);
                }
                else { throw new ArgumentNullException("timeoutHandler"); }
            }
            else { throw new ArgumentOutOfRangeException("timeoutValue", "Timeout value must be greater than 0!"); }
        }

        /// <summary>
        /// Stops the timer object.
        /// </summary>
        /// <returns>True if the time has been stopped successfully, or false if it has already been timed out.</returns>
        public bool Stop()
        {
            if (this.timerDisposed) { throw new ObjectDisposedException("AdvancedTimer"); }
            this.stopTimer.Set();
            this.controllerPN.AttachThread(this.extTransitionsCallerThreadStop, this.callerThreadCallbacks);
            return this.stopSuccessFlag;
        }

        /// <summary>
        /// Disposes this timer object so it no longer can be used.
        /// </summary>
        public void Dispose()
        {
            this.timerDisposed = true;

            this.disposeTimer.Set();
            this.timerThread.Join();

            this.disposeTimer.Close();
            this.startTimer.Close();
            this.stopTimer.Close();
        }

        /// <summary>
        /// The starting function of the timer thread.
        /// </summary>
        private void TimerThreadProc()
        {
            while (true)
            {
                int startedOrDisposed = EventWaitHandle.WaitAny(new WaitHandle[2] { this.startTimer, this.disposeTimer });
                if (0 == startedOrDisposed)
                {
                    /// The timer has been started.
                    this.startTimer.Reset();
                    this.disposeTimer.Reset();
                    this.stopTimer.Reset();

                    this.controllerPN.AttachThread(this.extTransitionsTimerThreadStart, this.timerThreadCallbacks);

                    /// Wait for stop event or timeout.
                    if (this.stopTimer.WaitOne(this.currentTimeoutValue))
                    {
                        /// Timer stopped
                        this.controllerPN.AttachThread(this.extTransitionsTimerThreadStop, this.timerThreadCallbacks);
                    }
                    else
                    {
                        /// Timeout
                        this.currentTimeoutHandler(this);
                        this.controllerPN.AttachThread(this.extTransitionsTimerThreadTimeout, this.timerThreadCallbacks);
                    }
                }
                else
                {
                    /// The timer has been disposed.
                    this.controllerPN.Dispose();
                    break;
                }
            }
        }

        /// <summary>
        /// Callback function for the timer thread.
        /// </summary>
        /// <param name="whichCallbackTransition">The index of the fired callback transition.</param>
        private void TimerThreadCallback(int whichCallbackTransition)
        {
            // Do nothing
            //TraceManager.WriteAllTrace("Timer thread callback with transition " + whichCallbackTransition);
        }

        /// <summary>
        /// Callback function for the caller thread.
        /// </summary>
        /// <param name="whichCallbackTransition">The index of the fired callback transition.</param>
        private void CallerThreadCallback(int whichCallbackTransition)
        {
            if (whichCallbackTransition == TR0_STP_RE_SUCC)
            {
                this.stopSuccessFlag = true;
            }
            else if (whichCallbackTransition == TR0_STP_RE_FAIL)
            {
                this.stopSuccessFlag = false;
            }
            //TraceManager.WriteAllTrace("Caller thread callback with transition " + whichCallbackTransition);
        }

        /// <summary>
        /// Creates the controller Petri-network.
        /// </summary>
        private void ConstructPetriNetwork()
        {
            this.controllerPN = new PetriNet(17, 21, 2);

            /// Transitions of group 0
            this.controllerPN.CreateTransition(TR0_START, 0, PetriNet.PNTransitionType.EXTERNAL);
            this.controllerPN.CreateTransition(TR0_STOP, 0, PetriNet.PNTransitionType.EXTERNAL);
            this.controllerPN.CreateTransition(TR0_STP_FAIL1, 0, PetriNet.PNTransitionType.INTERNAL);
            this.controllerPN.CreateTransition(TR0_STP_FAIL2, 0, PetriNet.PNTransitionType.INTERNAL);
            this.controllerPN.CreateTransition(TR0_STP_RE_FAIL, 0, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR0_STP_RE_SUCC, 0, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR0_STRT_ERR, 0, PetriNet.PNTransitionType.INTERNAL);
            this.controllerPN.CreateTransition(TR0_STRT_OK1, 0, PetriNet.PNTransitionType.INTERNAL);
            this.controllerPN.CreateTransition(TR0_STRT_OK2, 0, PetriNet.PNTransitionType.INTERNAL);
            this.controllerPN.CreateTransition(TR0_STRT_RE_ERR, 0, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR0_STRT_RE_OK, 0, PetriNet.PNTransitionType.CALLBACK);

            /// Transitions of group 1
            this.controllerPN.CreateTransition(TR1_START_END, 1, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR1_STOP_EVT, 1, PetriNet.PNTransitionType.EXTERNAL);
            this.controllerPN.CreateTransition(TR1_STOP_HANDLED, 1, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR1_STRT_BEGIN, 1, PetriNet.PNTransitionType.EXTERNAL);
            this.controllerPN.CreateTransition(TR1_TIMEOUT_HANDLED, 1, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR1_TIMEOUT_HANDLED2, 1, PetriNet.PNTransitionType.CALLBACK);
            this.controllerPN.CreateTransition(TR1_TIMEOUT1, 1, PetriNet.PNTransitionType.EXTERNAL);
            this.controllerPN.CreateTransition(TR1_TIMEOUT2, 1, PetriNet.PNTransitionType.EXTERNAL);
            this.controllerPN.CreateTransition(TR1_WILL_FAIL, 1, PetriNet.PNTransitionType.INTERNAL);
            this.controllerPN.CreateTransition(TR1_WILL_STOP, 1, PetriNet.PNTransitionType.INTERNAL);

            /// Edges from transitions to places
            this.controllerPN.CreateTPEdge(TR1_START_END, RUNNING, 1);
            this.controllerPN.CreateTPEdge(TR1_START_END, STRT_OK, 1);
            this.controllerPN.CreateTPEdge(TR1_STRT_BEGIN, STARTING2, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT_HANDLED2, CALL_MUTEX, 1);
            this.controllerPN.CreateTPEdge(TR0_STRT_ERR, STRT_ERR, 1);
            this.controllerPN.CreateTPEdge(TR0_STRT_ERR, RUNNING, 1);
            this.controllerPN.CreateTPEdge(TR0_STRT_RE_ERR, CALL_MUTEX, 1);
            this.controllerPN.CreateTPEdge(TR0_STRT_RE_OK, CALL_MUTEX, 1);
            this.controllerPN.CreateTPEdge(TR0_START, STRT_PROC, 1);
            this.controllerPN.CreateTPEdge(TR1_STOP_EVT, STOPPING, 1);
            this.controllerPN.CreateTPEdge(TR1_STOP_HANDLED, STOPPED, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT1, TIMINGOUT, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT_HANDLED, TIMEDOUT, 1);
            this.controllerPN.CreateTPEdge(TR0_STOP, STP_PROC, 1);
            this.controllerPN.CreateTPEdge(TR1_WILL_STOP, STOPPING, 1);
            this.controllerPN.CreateTPEdge(TR1_WILL_FAIL, TIMINGOUT, 1);
            this.controllerPN.CreateTPEdge(TR1_WILL_STOP, STP_SUCCING, 1);
            this.controllerPN.CreateTPEdge(TR1_WILL_FAIL, STP_FAILING, 1);
            this.controllerPN.CreateTPEdge(TR1_STOP_HANDLED, STP_SUCCESS, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT_HANDLED, STP_FAILED, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT2, CALL_BLCKD, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT2, TIMINGOUT, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT_HANDLED2, TIMEDOUT, 1);
            this.controllerPN.CreateTPEdge(TR1_TIMEOUT1, STP_PROC, 1);
            this.controllerPN.CreateTPEdge(TR1_STOP_EVT, STP_PROC, 1);
            this.controllerPN.CreateTPEdge(TR0_STP_RE_SUCC, CALL_MUTEX, 1);
            this.controllerPN.CreateTPEdge(TR0_STP_RE_FAIL, CALL_MUTEX, 1);
            this.controllerPN.CreateTPEdge(TR0_STP_FAIL2, STP_FAILED, 1);
            this.controllerPN.CreateTPEdge(TR0_STP_FAIL1, STP_FAILED, 1);
            this.controllerPN.CreateTPEdge(TR0_STP_FAIL2, TIMEDOUT, 1);
            this.controllerPN.CreateTPEdge(TR0_STP_FAIL1, STOPPED, 1);
            this.controllerPN.CreateTPEdge(TR0_STRT_OK1, STARTING1, 1);
            this.controllerPN.CreateTPEdge(TR0_STRT_OK2, STARTING1, 1);

            /// Edges from places to transitions
            this.controllerPN.CreatePTEdge(STARTING2, TR1_START_END, 1);
            this.controllerPN.CreatePTEdge(STP_SUCCESS, TR0_STP_RE_SUCC, 1);
            this.controllerPN.CreatePTEdge(STP_FAILED, TR0_STP_RE_FAIL, 1);
            this.controllerPN.CreatePTEdge(CALL_MUTEX, TR1_TIMEOUT2, 1);
            this.controllerPN.CreatePTEdge(STRT_PROC, TR0_STRT_ERR, 1);
            this.controllerPN.CreatePTEdge(RUNNING, TR0_STRT_ERR, 1);
            this.controllerPN.CreatePTEdge(STARTING1, TR1_STRT_BEGIN, 1);
            this.controllerPN.CreatePTEdge(STRT_ERR, TR0_STRT_RE_ERR, 1);
            this.controllerPN.CreatePTEdge(STRT_OK, TR0_STRT_RE_OK, 1);
            this.controllerPN.CreatePTEdge(STRT_PROC, TR0_STRT_OK1, 1);
            this.controllerPN.CreatePTEdge(STRT_PROC, TR0_STRT_OK2, 1);
            this.controllerPN.CreatePTEdge(STOPPED, TR0_STRT_OK2, 1);
            this.controllerPN.CreatePTEdge(CALL_MUTEX, TR0_START, 1);
            this.controllerPN.CreatePTEdge(RUNNING, TR1_TIMEOUT1, 1);
            this.controllerPN.CreatePTEdge(RUNNING, TR1_STOP_EVT, 1);
            this.controllerPN.CreatePTEdge(CALL_MUTEX, TR0_STOP, 1);
            this.controllerPN.CreatePTEdge(STOPPING, TR1_STOP_HANDLED, 1);
            this.controllerPN.CreatePTEdge(TIMINGOUT, TR1_TIMEOUT_HANDLED, 1);
            this.controllerPN.CreatePTEdge(STP_PROC, TR1_WILL_STOP, 1);
            this.controllerPN.CreatePTEdge(STOPPING, TR1_WILL_STOP, 1);
            this.controllerPN.CreatePTEdge(TIMINGOUT, TR1_WILL_FAIL, 1);
            this.controllerPN.CreatePTEdge(STP_PROC, TR1_WILL_FAIL, 1);
            this.controllerPN.CreatePTEdge(STP_SUCCING, TR1_STOP_HANDLED, 1);
            this.controllerPN.CreatePTEdge(STP_FAILING, TR1_TIMEOUT_HANDLED, 1);
            this.controllerPN.CreatePTEdge(RUNNING, TR1_TIMEOUT2, 1);
            this.controllerPN.CreatePTEdge(CALL_BLCKD, TR1_TIMEOUT_HANDLED2, 1);
            this.controllerPN.CreatePTEdge(TIMINGOUT, TR1_TIMEOUT_HANDLED2, 1);
            this.controllerPN.CreatePTEdge(STP_PROC, TR1_STOP_EVT, 1);
            this.controllerPN.CreatePTEdge(STP_PROC, TR1_TIMEOUT1, 1);
            this.controllerPN.CreatePTEdge(TIMEDOUT, TR0_STRT_OK1, 1);
            this.controllerPN.CreatePTEdge(STP_PROC, TR0_STP_FAIL2, 1);
            this.controllerPN.CreatePTEdge(STP_PROC, TR0_STP_FAIL1, 1);
            this.controllerPN.CreatePTEdge(STOPPED, TR0_STP_FAIL1, 1);
            this.controllerPN.CreatePTEdge(TIMEDOUT, TR0_STP_FAIL2, 1);

            /// And finally commission the Petri-network.
            int[] initialTokens = new int[17];
            for (int i = 0; i < 17; i++)
            {
                initialTokens[i] = (i == CALL_MUTEX || i == STOPPED) ? 1 : 0;
            }
            this.controllerPN.CommissionNetwork(initialTokens);
        }

        /// <summary>
        /// This event object is used to stop the timer.
        /// </summary>
        private ManualResetEvent stopTimer;

        /// <summary>
        /// This event object is used to dispose the timer object.
        /// </summary>
        private ManualResetEvent disposeTimer;

        /// <summary>
        /// This event object is used to start the timer object.
        /// </summary>
        private ManualResetEvent startTimer;

        /// <summary>
        /// The timer thread that will send the timeout notification.
        /// </summary>
        private RCThread timerThread;

        /// <summary>
        /// The current value of the timeout in milliseconds.
        /// </summary>
        private int currentTimeoutValue;

        /// <summary>
        /// The current timeout handler function.
        /// </summary>
        private TimeoutHandler currentTimeoutHandler;

        /// <summary>
        /// This flag indicates if the timer object has been disposed.
        /// </summary>
        private bool timerDisposed;

        /// <summary>
        /// Indicates whether the timer has been stopped successfully or not.
        /// </summary>
        private bool stopSuccessFlag;

        /// <summary>
        /// The Petri-network that controls the timer object.
        /// </summary>
        private PetriNet controllerPN;

        /// <summary>
        /// Callback function definitions for the timer thread.
        /// </summary>
        private Dictionary<int, PetriNet.PNCallback> timerThreadCallbacks;

        /// <summary>
        /// Callback function definitions for the caller thread.
        /// </summary>
        private Dictionary<int, PetriNet.PNCallback> callerThreadCallbacks;

        /// <summary>
        /// Timer thread external transitions for start event.
        /// </summary>
        private int[] extTransitionsTimerThreadStart;

        /// <summary>
        /// Timer thread external transitions for stop event.
        /// </summary>
        private int[] extTransitionsTimerThreadStop;

        /// <summary>
        /// Timer thread external transitions for timeout.
        /// </summary>
        private int[] extTransitionsTimerThreadTimeout;

        /// <summary>
        /// Caller thread external transitions for start.
        /// </summary>
        private int[] extTransitionsCallerThreadStart;

        /// <summary>
        /// Caller thread external transitions for stop.
        /// </summary>
        private int[] extTransitionsCallerThreadStop;

        /// <summary>
        /// Places of the controller Petri-network.
        /// </summary>
        private const int CALL_MUTEX = 0;
        private const int STP_FAILED = 1;
        private const int STOPPED = 2;
        private const int TIMEDOUT = 3;
        private const int RUNNING = 4;
        private const int STP_SUCCESS = 5;
        private const int STOPPING = 6;
        private const int TIMINGOUT = 7;
        private const int STP_PROC = 8;
        private const int STP_SUCCING = 9;
        private const int STP_FAILING = 10;
        private const int CALL_BLCKD = 11;
        private const int STARTING1 = 12;
        private const int STARTING2 = 13;
        private const int STRT_PROC = 14;
        private const int STRT_OK = 15;
        private const int STRT_ERR = 16;

        /// <summary>
        /// Transitions of the controller Petri-network (Group 0)
        /// </summary>
        private const int TR0_STOP = 0;         /// external transition
        private const int TR0_START = 1;        /// external transition
        private const int TR0_STP_FAIL1 = 2;    /// internal transition
        private const int TR0_STP_FAIL2 = 3;    /// internal transition
        private const int TR0_STRT_OK1 = 4;     /// internal transition
        private const int TR0_STRT_OK2 = 5;     /// internal transition
        private const int TR0_STRT_ERR = 6;     /// internal transition
        private const int TR0_STP_RE_SUCC = 7;  /// callback transition
        private const int TR0_STP_RE_FAIL = 8;  /// callback transition
        private const int TR0_STRT_RE_OK = 9;   /// callback transition
        private const int TR0_STRT_RE_ERR = 10; /// callback transition

        /// <summary>
        /// Transitions of the controller Petri-network (Group 1)
        /// </summary>
        private const int TR1_STRT_BEGIN = 11;          /// external transition
        private const int TR1_STOP_EVT = 12;            /// external transition
        private const int TR1_TIMEOUT1 = 13;            /// external transition
        private const int TR1_TIMEOUT2 = 14;            /// external transition
        private const int TR1_WILL_STOP = 15;           /// internal transition
        private const int TR1_WILL_FAIL = 16;           /// internal transition
        private const int TR1_STOP_HANDLED = 17;        /// callback transition
        private const int TR1_TIMEOUT_HANDLED = 18;     /// callback transition
        private const int TR1_TIMEOUT_HANDLED2 = 19;    /// callback transition
        private const int TR1_START_END = 20;           /// callback transition
    }
}
