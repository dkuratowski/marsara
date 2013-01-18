using System;
using System.Collections.Generic;
using RC.Common.Configuration;
using System.Text.RegularExpressions;

namespace RC.Common.Diagnostics
{
    /// <summary>
    /// Interface for tracing events.
    /// </summary>
    public interface ITrace : IDisposable
    {
        /// <summary>Gets the name of this trace.</summary>
        string TrcName { get; }

        /// <summary>Writes an object into the trace.</summary>
        /// <param name="message">The object you want to write to the trace.</param>
        void WriteLine(object obj);

        /// <summary>
        /// Writes a thread fork operation into the trace.
        /// </summary>
        /// <param name="newThread">Reference to the new thread that has been started.</param>
        /// <param name="parentThread">Reference to the thread that initiated the start of newThread.</param>
        void WriteFork(RCThread newThread, RCThread parentThread);
        
        /// <summary>
        /// Writes a thread join operation into the trace.
        /// </summary>
        /// <param name="runningThread">Reference to the thread that waitingThread is waiting for.</param>
        /// <param name="waitingThread">Reference to the thread that is waiting for runningThread.</param>
        void WriteJoin(RCThread runningThread, RCThread waitingThread);

        /// <summary>
        /// Writes an exception to the into the trace.
        /// </summary>
        /// <param name="ex">The exception to write.</param>
        /// <param name="isFatal">True if the exception was fatal, false otherwise.</param>
        void WriteException(Exception ex, bool isFatal);
    }

    /// <summary>
    /// Manager class for controling trace objects.
    /// </summary>
    public static class TraceManager
    {
        static TraceManager()
        {
            traceRegistry = new Dictionary<string, ITrace>();
            traceFilterStatuses = new List<bool>();
            traceFilterIDs = new Dictionary<string, int>();
            //ConsoleTrace consTrc = new ConsoleTrace("DefaultTrace");
            //traceRegistry.Add(consTrc.TrcName, consTrc);
        }

        /// <summary>
        /// Returns the default trace which is the console.
        /// </summary>
        //public static ITrace GetTrace()
        //{
        //    return GetTrace("DefaultTrace");
        //}

        /// <summary>
        /// Returns the trace with the given name.
        /// </summary>
        /// <param name="traceName">Name of the trace you want to get.</param>
        /// <returns>The trace object with the given name.</returns>
        /// <exception cref="Exception">If the trace with the given name doesn't exist.</exception>
        public static ITrace GetTrace(string traceName)
        {
            if (traceName == null || traceName.Length == 0) { throw new ArgumentNullException("traceName"); }

            ITrace searchedTrace = null;
            bool traceFound = traceRegistry.TryGetValue(traceName, out searchedTrace);
            if (!traceFound)
            {
                throw new Exception(string.Format("Error: Unable to find trace with the given name: '{0}'!", traceName));
            }
            else
            {
                return searchedTrace;
            }
        }

        /// <summary>
        /// Registers a trace object with the given name.
        /// </summary>
        /// <param name="trace">The trace object you want to register.</param>
        /// <exception cref="Exception">If the trace with the given name already exists.</exception>
        public static void RegisterTrace(ITrace trace)
        {
            if (trace == null) { throw new ArgumentNullException("trace"); }
            if (trace.TrcName == null || trace.TrcName.Length == 0) { throw new ArgumentNullException("trace.TrcName"); }
            if (RCThread.CurrentThread != RCThread.MainThread)
            {
                throw new RCThreadException("Registering trace object is only allowed from the MainThread!");
            }

            if (traceRegistry.ContainsKey(trace.TrcName))
            {
                throw new Exception(string.Format("Error: Trace with the given name already exists: '{0}'!", trace.TrcName));
            }
            else
            {
                traceRegistry.Add(trace.TrcName, trace);
            }
        }

        /// <summary>
        /// Unregisters the trace object with the given name.
        /// </summary>
        /// <param name="traceName">Name of the trace you want to unregister.</param>
        /// <exception cref="Exception">If the trace with the given name doesn't exist.</exception>
        public static void UnregisterTrace(string traceName)
        {
            if (traceName == null || traceName.Length == 0) { throw new ArgumentNullException("traceName"); }
            if (RCThread.CurrentThread != RCThread.MainThread)
            {
                throw new RCThreadException("Unregistering trace object is only allowed from the MainThread!");
            }

            if (!traceRegistry.ContainsKey(traceName))
            {
                throw new Exception(string.Format("Error: Unable to find trace with the given name: '{0}'!", traceName));
            }
            else
            {
                ITrace trcToRemove;
                if (traceRegistry.TryGetValue(traceName, out trcToRemove))
                {
                    trcToRemove.Dispose();
                    traceRegistry.Remove(traceName);
                }
            }
        }

        /// <summary>
        /// Unregisters all traces previously registered to the manager. If there are no registered
        /// traces, this function has no effect.
        /// </summary>
        public static void UnregisterAllTraces()
        {
            if (RCThread.CurrentThread != RCThread.MainThread)
            {
                throw new RCThreadException("Unregistering trace objects is only allowed from the MainThread!");
            }

            foreach (KeyValuePair<string, ITrace> item in traceRegistry)
            {
                item.Value.Dispose();
            }
            traceRegistry.Clear();
        }

        /// <summary>
        /// Writes the given object to all registered traces.
        /// </summary>
        /// <param name="obj">The object you want to trace.</param>
        /// <param name="traceFilterID">The ID of the trace filter.</param>
        /// <remarks>This function has no effect if there is no registered traces or the given filter is not activated.</remarks>
        public static void WriteAllTrace(object obj, int traceFilterID)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }
            if (traceFilterID < 0 || traceFilterID >= traceFilterStatuses.Count) { throw new ArgumentOutOfRangeException("traceFilterID"); }

            /// Trace only if the given trace filter is activated.
            if (traceFilterStatuses[traceFilterID])
            {
                //string prefix = "Thread-" + Thread.CurrentThread.ManagedThreadId + " @@@(" + DateTime.Now + "): ";
                IEnumerator<KeyValuePair<string, ITrace>> trcIt = traceRegistry.GetEnumerator();
                while (trcIt.MoveNext())
                {
                    trcIt.Current.Value.WriteLine(obj);
                }
            }
        }

        /// <summary>
        /// Writes a thread fork operation to all registered traces.
        /// </summary>
        /// <param name="newThread">Reference to the new thread that has been started.</param>
        /// <param name="parentThread">Reference to the thread that initiated the start of newThread.</param>
        public static void WriteForkAllTrace(RCThread newThread, RCThread parentThread)
        {
            if (newThread == null) { throw new ArgumentNullException("newThread"); }
            if (parentThread == null) { throw new ArgumentNullException("parentThread"); }

            IEnumerator<KeyValuePair<string, ITrace>> trcIt = traceRegistry.GetEnumerator();
            while (trcIt.MoveNext())
            {
                trcIt.Current.Value.WriteFork(newThread, parentThread);
            }
        }

        /// <summary>
        /// Writes a thread join operation to all registered traces.
        /// </summary>
        /// <param name="runningThread">Reference to the thread that waitingThread is waiting for.</param>
        /// <param name="waitingThread">Reference to the thread that is waiting for runningThread.</param>
        public static void WriteJoinAllTrace(RCThread runningThread, RCThread waitingThread)
        {
            if (runningThread == null) { throw new ArgumentNullException("runningThread"); }
            if (waitingThread == null) { throw new ArgumentNullException("waitingThread"); }

            IEnumerator<KeyValuePair<string, ITrace>> trcIt = traceRegistry.GetEnumerator();
            while (trcIt.MoveNext())
            {
                trcIt.Current.Value.WriteJoin(runningThread, waitingThread);
            }
        }

        /// <summary>
        /// Writes an exception to all registered traces.
        /// </summary>
        /// <param name="ex">The exception you want to write.</param>
        /// <param name="isFatal">True if it was a fatal exception, false otherwise.</param>
        public static void WriteExceptionAllTrace(Exception ex, bool isFatal)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            IEnumerator<KeyValuePair<string, ITrace>> trcIt = traceRegistry.GetEnumerator();
            while (trcIt.MoveNext())
            {
                trcIt.Current.Value.WriteException(ex, isFatal);
            }
        }

        /// <summary>
        /// Checks whether the manager has any registered traces.
        /// </summary>
        /// <returns>True if the manager has registered traces, false otherwise.</returns>
        public static bool HasAnyTrace()
        {
            return (traceRegistry.Count != 0);
        }

        /// <summary>
        /// Checks whether the trace with the given name has been registered to the manager.
        /// </summary>
        /// <param name="traceName">Name of the trace you want to check.</param>
        /// <returns>True if the trace has been registered, false otherwise.</returns>
        public static bool HasTrace(string traceName)
        {
            if (traceName == null || traceName.Length == 0) { throw new ArgumentNullException("traceName"); }

            return traceRegistry.ContainsKey(traceName);
        }

        /// <summary>
        /// Registers the given trace filter to the trace manager.
        /// </summary>
        /// <param name="filterName">Name of the trace filter you want to register.</param>
        public static void RegisterTraceFilter(string filterName)
        {
            if (filterName == null || filterName.Length == 0) { throw new ArgumentNullException("filterName"); }
            if (RCThread.CurrentThread != RCThread.MainThread)
            {
                throw new RCThreadException("Registering trace filter is only allowed from the MainThread!");
            }

            if (!traceFilterIDs.ContainsKey(filterName))
            {
                traceFilterIDs.Add(filterName, traceFilterStatuses.Count);
                traceFilterStatuses.Add(false);
            }
            else
            {
                throw new ConfigurationException(string.Format("Trace filter {0} has already been registered!", filterName));
            }
        }

        /// <summary>
        /// Unregisters every registered trace filters from the TraceManager.
        /// </summary>
        public static void UnregisterAllTraceFilters()
        {
            if (RCThread.CurrentThread != RCThread.MainThread)
            {
                throw new RCThreadException("Unregistering trace filters is only allowed from the MainThread!");
            }
            traceFilterStatuses.Clear();
            traceFilterIDs.Clear();
        }

        /// <summary>
        /// Gets the ID of the given trace filter.
        /// </summary>
        /// <param name="filterName">The name of the trace filter whose ID you want to get.</param>
        /// <returns>
        /// The ID of the given trace filter or -1 if no trace filter exists with the given name.
        /// </returns>
        public static int GetTraceFilterID(string filterName)
        {
            if (filterName == null || filterName.Length == 0) { throw new ArgumentNullException("filterName"); }

            if (traceFilterIDs.ContainsKey(filterName))
            {
                /// Trace filter found.
                return traceFilterIDs[filterName];
            }
            else
            {
                /// Trace filter not found.
                return -1;
            }
        }

        /// <summary>
        /// Activates/deactivates the trace filters matching the given wildcard pattern.
        /// </summary>
        /// <param name="wildcardPattern">
        /// The pattern of the names of the trace filters you want to activate/deactivate.
        /// </param>
        /// <param name="activate">True in case of activation, false in case of deactivation.</param>
        public static void SwitchTraceFilters(string wildcardPattern, bool activate)
        {
            if (RCThread.CurrentThread != RCThread.MainThread)
            {
                throw new RCThreadException("Switching trace filters is only allowed from the MainThread!");
            }
            Wildcard wildcard = new Wildcard(wildcardPattern);
            foreach (KeyValuePair<string, int> trcFilter in traceFilterIDs)
            {
                if (wildcard.IsMatch(trcFilter.Key))
                {
                    traceFilterStatuses[trcFilter.Value] = activate;
                }
            }
        }

        /// <summary>
        /// List of the registered trace objects.
        /// </summary>
        private static Dictionary<string, ITrace> traceRegistry;

        /// <summary>
        /// The status bits of the registered trace filters.
        /// </summary>
        private static List<bool> traceFilterStatuses;

        /// <summary>
        /// IDs of the registered trace filters mapped by their names.
        /// </summary>
        private static Dictionary<string, int> traceFilterIDs;
    }
}
