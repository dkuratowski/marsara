using System;

namespace RC.Common.Diagnostics
{
    /// <summary>
    /// Represents a trace that is writing to the console.
    /// </summary>
    class ConsoleTrace : ITrace
    {
        public ConsoleTrace(string name)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }

            trcName = name;
        }

        /// <see cref="ITrace.TrcName"/>
        public string TrcName
        {
            get { return trcName; }
        }

        /// <see cref="ITrace.WriteLine"/>
        public void WriteLine(object obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            Console.WriteLine(string.Format("{0} @@@({1}): {2}", RCThread.CurrentThread.Name, DateTime.Now, obj.ToString()));
        }

        /// <see cref="ITrace.WriteFork"/>
        public void WriteFork(RCThread newThread, RCThread parentThread)
        {
            if (newThread == null) { throw new ArgumentNullException("newThread"); }
            if (parentThread == null) { throw new ArgumentNullException("parentThread"); }

            Console.WriteLine(string.Format("THREAD_FORK @@@({0}): {1} --> {2}", DateTime.Now, parentThread.Name, newThread.Name));
        }

        /// <see cref="ITrace.WriteJoin"/>
        public void WriteJoin(RCThread runningThread, RCThread waitingThread)
        {
            if (runningThread == null) { throw new ArgumentNullException("runningThread"); }
            if (waitingThread == null) { throw new ArgumentNullException("waitingThread"); }

            Console.WriteLine(string.Format("THREAD_JOIN @@@({0}): {1} <-- {2}", DateTime.Now, waitingThread.Name, runningThread.Name));
        }

        /// <see cref="ITrace.WriteException"/>
        public void WriteException(Exception ex, bool isFatal)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            if (isFatal)
            {
                Console.WriteLine(string.Format("{0} FATAL_EXCEPTION @@@({1}): {2}", RCThread.CurrentThread.Name, DateTime.Now, ex.ToString()));
            }
            else
            {
                Console.WriteLine(string.Format("{0} EXCEPTION @@@({1}): {2}", RCThread.CurrentThread.Name, DateTime.Now, ex.ToString()));
            }
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
        }

        /// <summary>
        /// The name of this trace.
        /// </summary>
        private string trcName;
    }
}
