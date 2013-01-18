using System;
using System.IO;
using System.Diagnostics;

namespace RC.Common.Diagnostics
{
    /// <summary>
    /// Represents a trace that is writing to a logfile.
    /// </summary>
    public class LogfileTrace : ITrace
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the trace.</param>
        public LogfileTrace(string name, string filePrefix)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }
            if (filePrefix == null || filePrefix.Length == 0) { throw new ArgumentNullException("filePrefix"); }

            this.trcName = name;
            this.output = new StreamWriter(string.Format("{0}_{1}.log", filePrefix, Process.GetCurrentProcess().Id));
        }

        /// <see cref="ITrace.WriteLine"/>
        public void WriteLine(object obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            output.WriteLine(string.Format("{0} @@@({1}): {2}", RCThread.CurrentThread.Name, DateTime.Now, obj.ToString()));
            output.Flush();
        }

        /// <see cref="ITrace.WriteFork"/>
        public void WriteFork(RCThread newThread, RCThread parentThread)
        {
            if (newThread == null) { throw new ArgumentNullException("newThread"); }
            if (parentThread == null) { throw new ArgumentNullException("parentThread"); }

            output.WriteLine(string.Format("THREAD_FORK @@@({0}): {1} --> {2}", DateTime.Now, parentThread.Name, newThread.Name));
            output.Flush();
        }

        /// <see cref="ITrace.WriteJoin"/>
        public void WriteJoin(RCThread runningThread, RCThread waitingThread)
        {
            if (runningThread == null) { throw new ArgumentNullException("runningThread"); }
            if (waitingThread == null) { throw new ArgumentNullException("waitingThread"); }

            output.WriteLine(string.Format("THREAD_JOIN @@@({0}): {1} <-- {2}", DateTime.Now, waitingThread.Name, runningThread.Name));
            output.Flush();
        }

        /// <see cref="ITrace.WriteException"/>
        public void WriteException(Exception ex, bool isFatal)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            if (isFatal)
            {
                output.WriteLine(string.Format("{0} FATAL_EXCEPTION @@@({1}): {2}", RCThread.CurrentThread.Name, DateTime.Now, ex.ToString()));
            }
            else
            {
                output.WriteLine(string.Format("{0} EXCEPTION @@@({1}): {2}", RCThread.CurrentThread.Name, DateTime.Now, ex.ToString()));
            }
            output.Flush();
        }

        /// <summary>
        /// Stops the log file trace.
        /// </summary>
        public void Dispose()
        {
            this.output.Flush();
            this.output.Close();
            this.output.Dispose();
        }

        /// <see cref="ITrace.TrcName"/>
        public string TrcName { get { return this.trcName; } }

        /// <summary>
        /// The output stream.
        /// </summary>
        private StreamWriter output;

        /// <summary>
        /// Name of the trace.
        /// </summary>
        private string trcName;
    }
}
