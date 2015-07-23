using System;
using System.Collections.Generic;
using RC.Common;
using System.Threading;
using RC.NetworkingSystem;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// This is a queue where the threads should place their generated events. These events will be handled
    /// by the DSS-thread.
    /// </summary>
    class DssEventQueue : ILobbyListener, IAlarmClockFinished, IDisposable
    {
        /// <summary>
        /// Enumerates the possible event types.
        /// </summary>
        private enum DssEventType
        {
            PACKAGE_ARRIVED = 0,                /// (package : RCPackage, sender : int)
            CONTROL_PACKAGE_FROM_SERVER = 1,    /// (package : RCPackage)
            CONTROL_PACKAGE_FROM_CLIENT = 2,    /// (package : RCPackage, sender : int)
            LINE_OPENED = 3,                    /// (lineIdx : int)
            LINE_CLOSED = 4,                    /// (lineIdx : int)
            LINE_ENGAGED = 5,                   /// (lineIdx : int)
            LOBBY_LOST = 6,                     /// ()
            LOBBY_IS_RUNNING = 7,               /// (idOfThisPeer : int)
            ALARM_CLOCK_FINISHED = 8            /// (whichClock : AlarmClock)
        }

        /// <summary>
        /// Constructs a DssEventQueue object.
        /// </summary>
        public DssEventQueue()
        {
            this.readSemaphore = new Semaphore(0, DssConstants.EVENT_QUEUE_CAPACITY);

            this.eventTypeFifo = new Fifo<DssEventType>(DssConstants.EVENT_QUEUE_CAPACITY);
            this.packageArgFifo = new Fifo<RCPackage>(DssConstants.EVENT_QUEUE_CAPACITY);
            this.intArgFifo = new Fifo<int>(DssConstants.EVENT_QUEUE_CAPACITY);
            this.alarmClkArgFifo = new Fifo<AlarmClock>(DssConstants.EVENT_QUEUE_CAPACITY);
            this.eventTimestamps = new Fifo<int>(DssConstants.EVENT_QUEUE_CAPACITY);

            //this.exitEventLoop = new ManualResetEvent(false);
            this.exitEventLoop = false;
            //this.eventLoopFinished = new ManualResetEvent(false);
            //this.disposeLock = new object();
            this.queueLock = new object();
            //this.objectDisposed = false;
            this.eventLoopThread = RCThread.CurrentThread;

            /// Create the preprocessor list and register a default preprocessor.
            this.eventPreprocessors = new RCSet<DssEventPreprocessor>();
            //this.eventPreprocessors.Add(new DssEventPreprocessor());

            /// Create the event handler list and register a default handler.
            this.eventHandlers = new RCSet<DssEventHandler>();
            //this.eventHandlers.Add(new DssEventHandler());

            this.currentLineStates = null;
        }

        #region ILobbyListener implementation

        /// <see cref="ILobbyListener.PackageArrived"/>
        public void PackageArrived(RCPackage package, int senderID)
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }

            int timestamp = DssRoot.Time;

            /// Preprocess the event in the context of the caller thread.
            lock (this.eventPreprocessors)
            {
                foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                {
                    if (!preprocessor.PackageArrivedPre(package, senderID))
                    {
                        TraceManager.WriteAllTrace(string.Format("Ignoring PACKAGE_ARRIVED({0} ; {1})", package, senderID), DssTraceFilters.EVENT_QUEUE_INFO);
                        return;
                    }
                }
            }

            /// Insert the event to the queue after a successfull preprocess.
            lock (this.queueLock)
            {
                this.eventTypeFifo.Push(DssEventType.PACKAGE_ARRIVED);
                this.packageArgFifo.Push(package);
                this.intArgFifo.Push(senderID);
                this.eventTimestamps.Push(timestamp);
                this.readSemaphore.Release();
            }
        }

        /// <see cref="ILobbyListener.ControlPackageArrived"/>
        public void ControlPackageArrived(RCPackage package)
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }

            int timestamp = DssRoot.Time;

            /// Preprocess the event in the context of the caller thread.
            lock (this.eventPreprocessors)
            {
                foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                {
                    if (!preprocessor.ControlPackageFromServerPre(package))
                    {
                        TraceManager.WriteAllTrace(string.Format("Ignoring CONTROL_PACKAGE_FROM_SERVER({0})", package), DssTraceFilters.EVENT_QUEUE_INFO);
                        return;
                    }
                }
            }

            /// Insert the event to the queue after a successfull preprocess.
            lock (this.queueLock)
            {
                this.eventTypeFifo.Push(DssEventType.CONTROL_PACKAGE_FROM_SERVER);
                this.packageArgFifo.Push(package);
                this.eventTimestamps.Push(timestamp);
                this.readSemaphore.Release();
            }
        }

        /// <see cref="ILobbyListener.ControlPackageArrived"/>
        public void ControlPackageArrived(RCPackage package, int senderID)
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }

            int timestamp = DssRoot.Time;

            /// Preprocess the event in the context of the caller thread.
            lock (this.eventPreprocessors)
            {
                foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                {
                    if (!preprocessor.ControlPackageFromClientPre(package, senderID))
                    {
                        TraceManager.WriteAllTrace(string.Format("Ignoring CONTROL_PACKAGE_FROM_CLIENT({0} ; {1})", package, senderID), DssTraceFilters.EVENT_QUEUE_INFO);
                        return;
                    }
                }
            }

            /// Insert the event to the queue after a successfull preprocess.
            lock (this.queueLock)
            {
                this.eventTypeFifo.Push(DssEventType.CONTROL_PACKAGE_FROM_CLIENT);
                this.packageArgFifo.Push(package);
                this.intArgFifo.Push(senderID);
                this.eventTimestamps.Push(timestamp);
                this.readSemaphore.Release();
            }
        }

        /// <see cref="ILobbyListener.LineStateReport"/>
        public void LineStateReport(int idOfThisPeer, LobbyLineState[] lineStates)
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }

            int timestamp = DssRoot.Time;

            lock (this.eventPreprocessors)
            {
                if (this.currentLineStates != null)
                {
                    /// This is not the first line state report
                    if (lineStates.Length == this.currentLineStates.Length)
                    {
                        for (int i = 0; i < lineStates.Length; i++)
                        {
                            if (lineStates[i] != this.currentLineStates[i])
                            {
                                if (lineStates[i] == LobbyLineState.Closed)
                                {
                                    /// Preprocessing LINE_CLOSED event
                                    foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                                    {
                                        if (!preprocessor.LineClosedPre(i))
                                        {
                                            TraceManager.WriteAllTrace(string.Format("Ignoring LINE_CLOSED({0})", i), DssTraceFilters.EVENT_QUEUE_INFO);
                                            return;
                                        }
                                    }

                                    /// Inserting the event to the queue
                                    lock (this.queueLock)
                                    {
                                        this.eventTypeFifo.Push(DssEventType.LINE_CLOSED);
                                        this.intArgFifo.Push(i);
                                        this.eventTimestamps.Push(timestamp);
                                        this.readSemaphore.Release();
                                    }
                                }
                                else if (lineStates[i] == LobbyLineState.Engaged)
                                {
                                    /// Preprocessing LINE_ENGAGED event
                                    foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                                    {
                                        if (!preprocessor.LineEngagedPre(i))
                                        {
                                            TraceManager.WriteAllTrace(string.Format("Ignoring LINE_ENGAGED({0})", i), DssTraceFilters.EVENT_QUEUE_INFO);
                                            return;
                                        }
                                    }

                                    /// Inserting the event to the queue
                                    lock (this.queueLock)
                                    {
                                        this.eventTypeFifo.Push(DssEventType.LINE_ENGAGED);
                                        this.intArgFifo.Push(i);
                                        this.eventTimestamps.Push(timestamp);
                                        this.readSemaphore.Release();
                                    }
                                }
                                else if (lineStates[i] == LobbyLineState.Opened)
                                {
                                    /// Preprocessing LINE_OPENED event
                                    foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                                    {
                                        if (!preprocessor.LineOpenedPre(i))
                                        {
                                            TraceManager.WriteAllTrace(string.Format("Ignoring LINE_OPENED({0})", i), DssTraceFilters.EVENT_QUEUE_INFO);
                                            return;
                                        }
                                    }

                                    /// Inserting the event to the queue
                                    lock (this.queueLock)
                                    {
                                        this.eventTypeFifo.Push(DssEventType.LINE_OPENED);
                                        this.intArgFifo.Push(i);
                                        this.eventTimestamps.Push(timestamp);
                                        this.readSemaphore.Release();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new DssException("Line state report incosistence!");
                    }
                }
                else
                {
                    /// This is the first line state report
                    /// Preprocessing LOBBY_IS_RUNNING event
                    foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                    {
                        if (!preprocessor.LobbyIsRunningPre(idOfThisPeer, lineStates.Length))
                        {
                            TraceManager.WriteAllTrace(string.Format("Ignoring LOBBY_IS_RUNNING({0} ; {1})", idOfThisPeer, lineStates.Length), DssTraceFilters.EVENT_QUEUE_INFO);
                            return;
                        }
                    }
                    /// Inserting the event to the queue
                    lock (this.queueLock)
                    {
                        this.eventTypeFifo.Push(DssEventType.LOBBY_IS_RUNNING);
                        this.intArgFifo.Push(idOfThisPeer);
                        this.intArgFifo.Push(lineStates.Length);
                        this.eventTimestamps.Push(timestamp);
                        this.readSemaphore.Release();
                    }

                    /// Process the first line events
                    for (int i = 0; i < lineStates.Length; i++)
                    {
                        if (lineStates[i] == LobbyLineState.Closed)
                        {
                            /// Preprocessing LINE_CLOSED event
                            foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                            {
                                if (!preprocessor.LineClosedPre(i))
                                {
                                    TraceManager.WriteAllTrace(string.Format("Ignoring LINE_CLOSED({0})", i), DssTraceFilters.EVENT_QUEUE_INFO);
                                    return;
                                }
                            }

                            /// Inserting the event to the queue
                            lock (this.queueLock)
                            {
                                this.eventTypeFifo.Push(DssEventType.LINE_CLOSED);
                                this.intArgFifo.Push(i);
                                this.eventTimestamps.Push(timestamp);
                                this.readSemaphore.Release();
                            }
                        }
                        else if (lineStates[i] == LobbyLineState.Engaged)
                        {
                            /// Preprocessing LINE_ENGAGED event
                            foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                            {
                                if (!preprocessor.LineEngagedPre(i))
                                {
                                    TraceManager.WriteAllTrace(string.Format("Ignoring LINE_ENGAGED({0})", i), DssTraceFilters.EVENT_QUEUE_INFO);
                                    return;
                                }
                            }

                            /// Inserting the event to the queue
                            lock (this.queueLock)
                            {
                                this.eventTypeFifo.Push(DssEventType.LINE_ENGAGED);
                                this.intArgFifo.Push(i);
                                this.eventTimestamps.Push(timestamp);
                                this.readSemaphore.Release();
                            }
                        }
                        else if (lineStates[i] == LobbyLineState.Opened)
                        {
                            /// Preprocessing LINE_OPENED event
                            foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                            {
                                if (!preprocessor.LineOpenedPre(i))
                                {
                                    TraceManager.WriteAllTrace(string.Format("Ignoring LINE_OPENED({0})", i), DssTraceFilters.EVENT_QUEUE_INFO);
                                    return;
                                }
                            }

                            /// Inserting the event to the queue
                            lock (this.queueLock)
                            {
                                this.eventTypeFifo.Push(DssEventType.LINE_OPENED);
                                this.intArgFifo.Push(i);
                                this.eventTimestamps.Push(timestamp);
                                this.readSemaphore.Release();
                            }
                        }
                    }
                }
                this.currentLineStates = lineStates;
            } /// end-lock
        }

        /// <see cref="ILobbyListener.LobbyLost"/>
        public void LobbyLost()
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }

            int timestamp = DssRoot.Time;

            /// Preprocess the event in the context of the caller thread.
            lock (this.eventPreprocessors)
            {
                foreach (DssEventPreprocessor preprocessor in this.eventPreprocessors)
                {
                    if (!preprocessor.LobbyLostPre())
                    {
                        TraceManager.WriteAllTrace("Ignoring LOBBY_LOST()", DssTraceFilters.EVENT_QUEUE_INFO);
                        return;
                    }
                }
            }

            /// Insert the event to the queue after a successfull preprocess.
            lock (this.queueLock)
            {
                this.eventTypeFifo.Push(DssEventType.LOBBY_LOST);
                this.eventTimestamps.Push(timestamp);
                this.readSemaphore.Release();
            }
        }

        /// <summary>
        /// The current state of the lines of the lobby.
        /// </summary>
        private LobbyLineState[] currentLineStates;

        #endregion

        #region IAlarmClockFinished implementation

        /// <see cref="IAlarmClockFinished.AlarmClockFinished"/>
        public void AlarmClockFinished(AlarmClock whichClock)
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }

            /// Insert the alarm clock event to the queue without preprocess.
            lock (this.queueLock)
            {
                this.eventTypeFifo.Push(DssEventType.ALARM_CLOCK_FINISHED);
                this.alarmClkArgFifo.Push(whichClock);
                this.readSemaphore.Release();
            }
        }

        #endregion

        /// <summary>
        /// Enters an infinite event loop.
        /// </summary>
        /// <remarks>Called by the DSS-thread.</remarks>
        public void EventLoop()
        {
            //lock (this.disposeLock) { if (this.objectDisposed) { throw new ObjectDisposedException("DssEventQueue"); } }
            if (this.eventLoopThread != RCThread.CurrentThread) { throw new DssException("DssEventQueue.EventLoop access denied from this thread!"); }
            if (this.exitEventLoop) { throw new ObjectDisposedException("DssEventQueue"); }

            TraceManager.WriteAllTrace("Starting event loop", DssTraceFilters.EVENT_QUEUE_INFO);

            //WaitHandle[] readOrExit = new WaitHandle[2] { this.readSemaphore, this.exitEventLoop };
            while (!this.exitEventLoop)//0 == WaitHandle.WaitAny(readOrExit))
            {
                /// Wait for the next event
                this.readSemaphore.WaitOne();

                lock (this.eventHandlers)
                {
                    DssEventType nextEventType;
                    RCPackage nextPackageArg;
                    int nextIntArg0;
                    int nextIntArg1;
                    AlarmClock nextAlarmClkArg;
                    int nextTimestamp;
                    ReadNextEvent(out nextEventType,
                                  out nextPackageArg,
                                  out nextIntArg0,
                                  out nextIntArg1,
                                  out nextAlarmClkArg,
                                  out nextTimestamp);

                    if (nextEventType == DssEventType.ALARM_CLOCK_FINISHED)
                    {
                        /// Handling ALARM_CLOCK_FINISHED events
                        nextAlarmClkArg.InvokeIfNecessary();
                    }
                    else
                    {
                        /// Handling other events
                        foreach (DssEventHandler handler in this.eventHandlers)
                        {
                            switch (nextEventType)
                            {
                                case DssEventType.PACKAGE_ARRIVED:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("PACKAGE_ARRIVED({0} ; {1})@{2}", nextPackageArg.ToString(), nextIntArg0, nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.PackageArrivedHdl(nextTimestamp, nextPackageArg, nextIntArg0);
                                        break;
                                    }
                                case DssEventType.CONTROL_PACKAGE_FROM_SERVER:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("CONTROL_PACKAGE_FROM_SERVER({0})@{1}", nextPackageArg.ToString(), nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.ControlPackageFromServerHdl(nextTimestamp, nextPackageArg);
                                        break;
                                    }
                                case DssEventType.CONTROL_PACKAGE_FROM_CLIENT:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("CONTROL_PACKAGE_FROM_CLIENT({0} ; {1})@{2}", nextPackageArg.ToString(), nextIntArg0, nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.ControlPackageFromClientHdl(nextTimestamp, nextPackageArg, nextIntArg0);
                                        break;
                                    }
                                case DssEventType.LINE_OPENED:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("LINE_OPENED({0})@{1}", nextIntArg0, nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.LineOpenedHdl(nextTimestamp, nextIntArg0);
                                        break;
                                    }
                                case DssEventType.LINE_CLOSED:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("LINE_CLOSED({0})@{1}", nextIntArg0, nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.LineClosedHdl(nextTimestamp, nextIntArg0);
                                        break;
                                    }
                                case DssEventType.LINE_ENGAGED:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("LINE_ENGAGED({0})@{1}", nextIntArg0, nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.LineEngagedHdl(nextTimestamp, nextIntArg0);
                                        break;
                                    }
                                case DssEventType.LOBBY_LOST:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("LOBBY_LOST()@{0}", nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.LobbyLostHdl(nextTimestamp);
                                        break;
                                    }
                                case DssEventType.LOBBY_IS_RUNNING:
                                    {
                                        TraceManager.WriteAllTrace(string.Format("LOBBY_IS_RUNNING({0}; {1})@{2}", nextIntArg0, nextIntArg1, nextTimestamp), DssTraceFilters.EVENT_QUEUE_INFO);
                                        handler.LobbyIsRunningHdl(nextTimestamp, nextIntArg0, nextIntArg1);
                                        break;
                                    }
                                default:
                                    {
                                        throw new DssException("Unexpected event type");
                                    }
                            } /// end-switch
                        } /// end-foreach (DssEventHandler handler in this.eventHandlers)
                    } /// end-else
                } /// end-lock (this.eventHandlers)
            } /// end-while

            TraceManager.WriteAllTrace("Event loop finished", DssTraceFilters.EVENT_QUEUE_INFO);
        }

        /// <summary>
        /// Asks the DSS-thread to exit the event loop after handling the current event.
        /// </summary>
        public void ExitEventLoop()
        {
            if (this.eventLoopThread != RCThread.CurrentThread) { throw new DssException("DssEventQueue.ExitEventLoop access denied from this thread!"); }
            this.exitEventLoop = true;
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.eventLoopThread != RCThread.CurrentThread) { throw new DssException("DssEventQueue.Dispose access denied from this thread!"); }
            this.readSemaphore.Close();
        }

        /// <summary>
        /// Registers the given preprocessor to the event queue.
        /// </summary>
        /// <param name="preprocessor">The preprocessor object you want to register.</param>
        public void RegisterPreprocessor(DssEventPreprocessor preprocessor)
        {
            lock (this.eventPreprocessors)
            {
                if (!this.eventPreprocessors.Contains(preprocessor))
                {
                    this.eventPreprocessors.Add(preprocessor);
                }
            }
        }

        /// <summary>
        /// Registers the given handler to the event queue.
        /// </summary>
        /// <param name="handler">The handler object you want to register.</param>
        public void RegisterHandler(DssEventHandler handler)
        {
            lock (this.eventHandlers)
            {
                if (!this.eventHandlers.Contains(handler))
                {
                    this.eventHandlers.Add(handler);
                }
            }
        }

        /// <summary>
        /// Internal function for the event loop thread to read the next event and all of it's arguments.
        /// </summary>
        private void ReadNextEvent(out DssEventType nextEventType,
                                   out RCPackage nextPackageArg,
                                   out int nextIntArg0,
                                   out int nextIntArg1,
                                   out AlarmClock nextAlarmClkArg,
                                   out int nextTimestamp)
        {
            nextEventType = DssEventType.PACKAGE_ARRIVED;
            nextPackageArg = null;
            nextIntArg0 = -1;
            nextIntArg1 = -1;
            nextAlarmClkArg = null;
            nextTimestamp = -1;

            lock (this.queueLock)
            {
                nextEventType = this.eventTypeFifo.Get();
                switch (nextEventType)
                {
                    case DssEventType.PACKAGE_ARRIVED:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextPackageArg = this.packageArgFifo.Get();
                            nextIntArg0 = this.intArgFifo.Get();
                            break;
                        }
                    case DssEventType.CONTROL_PACKAGE_FROM_SERVER:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextPackageArg = this.packageArgFifo.Get();
                            break;
                        }
                    case DssEventType.CONTROL_PACKAGE_FROM_CLIENT:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextPackageArg = this.packageArgFifo.Get();
                            nextIntArg0 = this.intArgFifo.Get();
                            break;
                        }
                    case DssEventType.LINE_OPENED:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextIntArg0 = this.intArgFifo.Get();
                            break;
                        }
                    case DssEventType.LINE_CLOSED:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextIntArg0 = this.intArgFifo.Get();
                            break;
                        }
                    case DssEventType.LINE_ENGAGED:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextIntArg0 = this.intArgFifo.Get();
                            break;
                        }
                    case DssEventType.LOBBY_LOST:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            break;
                        }
                    case DssEventType.LOBBY_IS_RUNNING:
                        {
                            nextTimestamp = this.eventTimestamps.Get();
                            nextIntArg0 = this.intArgFifo.Get();
                            nextIntArg1 = this.intArgFifo.Get();
                            break;
                        }
                    case DssEventType.ALARM_CLOCK_FINISHED:
                        {
                            nextAlarmClkArg = this.alarmClkArgFifo.Get();
                            break;
                        }
                    default:
                        {
                            throw new DssException("Unexpected event type");
                        }
                } /// end-switch
            }
        }

        /// <summary>
        /// The FIFO queue that contains the types of the events.
        /// </summary>
        private Fifo<DssEventType> eventTypeFifo;

        /// <summary>
        /// The FIFO queue that contains the event arguments of type RCPackage.
        /// </summary>
        private Fifo<RCPackage> packageArgFifo;

        /// <summary>
        /// The FIFO queue that contains the event arguments of type int.
        /// </summary>
        private Fifo<int> intArgFifo;

        /// <summary>
        /// The FIFO queue that contains the event arguments of type AdvancedTimer.
        /// </summary>
        private Fifo<AlarmClock> alarmClkArgFifo;

        /// <summary>
        /// The FIFO queue that contains the timestamps of the events.
        /// </summary>
        private Fifo<int> eventTimestamps;

        /// <summary>
        /// The registered event preprocessors.
        /// </summary>
        private RCSet<DssEventPreprocessor> eventPreprocessors;

        /// <summary>
        /// The registered event handlers.
        /// </summary>
        private RCSet<DssEventHandler> eventHandlers;

        /// <summary>
        /// The semaphore that is opened when the event queue is not empty.
        /// </summary>
        private Semaphore readSemaphore;

        /// <summary>
        /// This flag is true if the DSS-thread has to leave the event loop.
        /// </summary>
        private bool exitEventLoop;

        /// <summary>
        /// This object is locked when reading and writing the event-queue.
        /// </summary>
        private object queueLock;

        /// <summary>
        /// Reference to the thread that is currently running the event loop.
        /// </summary>
        private RCThread eventLoopThread;
    }
}
