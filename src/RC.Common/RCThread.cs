using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common.Diagnostics;

namespace RC.Common
{
    /// <summary>
    /// This class is a wrapper over the System.Threading.Thread class with additional features for tracing
    /// fork and join events.
    /// </summary>
    public class RCThread
    {
        /// <summary>
        /// Class level constructor.
        /// </summary>
        static RCThread()
        {            
            lockObject = new object();
            registeredThreads = new Dictionary<string, RCThread>();
            wrappedThreads = new Dictionary<Thread, RCThread>();
            mainThread = new RCThread();
        }

        #region Static methods, properties and fields

        /// <summary>
        /// Returns the current domain in which the current thread is running.
        /// </summary>
        /// <returns>
        /// An System.AppDomain representing the current application domain of the running thread.
        /// </returns>
        public static AppDomain GetDomain()
        {
            return Thread.GetDomain();
        }

        /// <summary>
        /// Returns a unique application domain identifier.
        /// </summary>
        /// <returns>A 32-bit signed integer uniquely identifying the application domain.</returns>
        public static int GetDomainID()
        {
            return Thread.GetDomainID();
        }

        /// <summary>
        /// Suspends the current thread for a specified time.
        /// </summary>
        /// <param name="milliseconds">
        /// The number of milliseconds for which the thread is blocked. Specify zero (0) to indicate that this thread
        /// should be suspended to allow other waiting threads to execute. Specify System.Threading.Timeout.Infinite
        /// to block the thread indefinitely.
        /// </param>
        public static void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        /// <summary>
        /// Blocks the current thread for a specified time.
        /// </summary>
        /// <param name="timeout">
        /// A System.TimeSpan set to the amount of time for which the thread is blocked. Specify zero to indicate that
        /// this thread should be suspended to allow other waiting threads to execute. Specify
        /// System.Threading.Timeout.Infinite to block the thread indefinitely.
        /// </param>
        public static void Sleep(TimeSpan timeout)
        {
            Thread.Sleep(timeout);
        }

        /// <summary>
        /// Gets the currently running RCThread.
        /// </summary>
        public static RCThread CurrentThread
        {
            get
            {
                lock (lockObject)
                {
                    Thread wrappedCurrentThread = Thread.CurrentThread;
                    if (!wrappedThreads.ContainsKey(wrappedCurrentThread))
                    {
                        throw new RCThreadException(string.Format("No RCThread has been found for underlying System.Threading.Thread with ID {0}!", wrappedCurrentThread.ManagedThreadId));
                    }

                    return wrappedThreads[wrappedCurrentThread];
                }
            }
        }

        /// <summary>
        /// Gets the main thread of the application. This will be the thread from where you first use the RCThread class.
        /// </summary>
        public static RCThread MainThread
        {
            get
            {
                return mainThread;
            }
        }

        /// <summary>
        /// This list contains the registered RCThread objects mapped by their name.
        /// </summary>
        private static Dictionary<string, RCThread> registeredThreads;

        /// <summary>
        /// This list contains the registered RCThread objects mapped by their wrapped System.Threading.Threads.
        /// </summary>
        private static Dictionary<Thread, RCThread> wrappedThreads;

        /// <summary>
        /// Reference to the main thread.
        /// </summary>
        private static RCThread mainThread;

        /// <summary>
        /// This object is used as a mutex for thread administration.
        /// </summary>
        private static object lockObject;

        #endregion

        /// <summary>
        /// Initializes a new instance of RCThread class, specifying a delegate that allows an object to be passed to
        /// the thread when the thread is started.
        /// </summary>
        /// <param name="start">
        /// A System.Threading.ParameterizedThreadStart delegate that represents the methods to be invoked when this
        /// thread begins executing.
        /// </param>
        /// <param name="name">The name of the created RCThread. This name must be unique.</param>
        public RCThread(ParameterizedThreadStart start, string name)
        {
            if (start == null) { throw new ArgumentNullException("start"); }

            this.parentThread = null;
            this.name = name;
            this.normalStart = null;
            this.paramStart = start;
            this.wrappedThread = new Thread(this.ParametrizedThreadProc);

            if (name == null || name.Length == 0)
            {
                this.name = string.Format("Thread-{0}", this.wrappedThread.ManagedThreadId);
            }
            else
            {
                this.name = string.Format("{0}-{1}", name, this.wrappedThread.ManagedThreadId);
            }

            lock (lockObject)
            {
                if (registeredThreads.ContainsKey(this.name))
                {
                    throw new RCThreadException(string.Format("An RCThread with name '{0}' already exists!", this.name));
                }

                registeredThreads.Add(this.name, this);
                wrappedThreads.Add(this.wrappedThread, this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the System.Threading.Thread class.
        /// </summary>
        /// <param name="start">
        /// A System.Threading.ThreadStart delegate that represents the methods to be invoked when this thread begins
        /// executing.
        /// </param>
        /// <param name="name">The name of the created RCThread. This name must be unique.</param>
        public RCThread(ThreadStart start, string name)
        {
            lock (lockObject)
            {
                if (start == null) { throw new ArgumentNullException("start"); }

                this.parentThread = null;
                this.name = name;
                this.normalStart = start;
                this.paramStart = null;
                this.wrappedThread = new Thread(this.NormalThreadProc);

                if (name == null || name.Length == 0)
                {
                    this.name = string.Format("Thread-{0}", this.wrappedThread.ManagedThreadId);
                }
                else
                {
                    this.name = string.Format("{0}-{1}", name, this.wrappedThread.ManagedThreadId);
                }

                if (registeredThreads.ContainsKey(this.name))
                {
                    throw new RCThreadException(string.Format("An RCThread with name '{0}' already exists!", this.name));
                }

                registeredThreads.Add(this.name, this);
                wrappedThreads.Add(this.wrappedThread, this);
            }
        }

        /// <summary>
        /// Causes the operating system to change the state of the current instance to System.Threading.ThreadState.Running,
        /// and optionally supplies an object containing data to be used by the method the thread executes.
        /// </summary>
        /// <param name="parameter">An object that contains data to be used by the method the thread executes.</param>
        public void Start(object parameter)
        {
            this.parentThread = RCThread.CurrentThread;
            this.wrappedThread.Start(parameter);
        }

        /// <summary>
        /// Causes the operating system to change the state of the current instance to System.Threading.ThreadState.Running.
        /// </summary>
        public void Start()
        {
            this.parentThread = RCThread.CurrentThread;
            this.wrappedThread.Start();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates, while continuing to perform standard COM and SendMessage
        /// pumping.
        /// </summary>
        public void Join()
        {
            this.wrappedThread.Join();
            TraceManager.WriteJoinAllTrace(this, RCThread.CurrentThread);

            lock (lockObject)
            {
                registeredThreads.Remove(this.name);
                wrappedThreads.Remove(this.wrappedThread);
            }
        }

        /// <summary>
        /// Gets the underlying System.Threading.Thread object.
        /// </summary>
        public Thread WrappedThread
        {
            get { return this.wrappedThread; }
        }

        /// <summary>
        /// Gets the name of this RCThread.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Internal constructor to create the main application thread.
        /// </summary>
        private RCThread()
        {
            lock (lockObject)
            {
                this.parentThread = null;
                this.name = "MainThread";
                this.normalStart = null;
                this.paramStart = null;
                this.wrappedThread = Thread.CurrentThread;

                registeredThreads.Add(this.name, this);
                wrappedThreads.Add(this.wrappedThread, this);
            }
        }

        /// <summary>
        /// This function will be executed by this RCThread in case of parametrized start.
        /// </summary>
        /// <param name="parameter">The parameter passed to the thread.</param>
        private void ParametrizedThreadProc(object parameter)
        {
            TraceManager.WriteForkAllTrace(this, this.parentThread);

            try
            {
                paramStart(parameter);
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, true);
                throw;
            }
        }

        /// <summary>
        /// This function will be executed by this RCThread in case of non-parametrized start.
        /// </summary>
        private void NormalThreadProc()
        {
            TraceManager.WriteForkAllTrace(this, this.parentThread);

            try
            {
                normalStart();
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, true);
                throw;
            }
        }

        /// <summary>
        /// Reference to the wrapped Thread object.
        /// </summary>
        private Thread wrappedThread;

        /// <summary>
        /// The function that must be executed by this RCThread if it has been started parametrized or null otherwise.
        /// </summary>
        private ParameterizedThreadStart paramStart;

        /// <summary>
        /// The function that must be executed by this RCThread if it has been started non-parametrized or null otherwise.
        /// </summary>
        private ThreadStart normalStart;

        /// <summary>
        /// The name of this RCThread.
        /// </summary>
        private string name;

        /// <summary>
        /// Reference to the RCThread that called the Start() method on this RCThread.
        /// </summary>
        private RCThread parentThread;
    }
}
