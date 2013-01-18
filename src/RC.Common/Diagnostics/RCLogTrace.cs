using System;
using System.Diagnostics;
using System.IO;
using RC.Common.Configuration;

namespace RC.Common.Diagnostics
{
    /// <summary>
    /// Represents a trace that is writing to an RCTrace file.
    /// </summary>
    public class RCLogTrace : ITrace
    {
        /// <summary>
        /// Class level constructor.
        /// </summary>
        static RCLogTrace()
        {
            RCL_EVENT_FORMAT = RCPackageFormatMap.Get("RC.Common.RclEvent");
            RCL_FORK_FORMAT = RCPackageFormatMap.Get("RC.Common.RclFork");
            RCL_JOIN_FORMAT = RCPackageFormatMap.Get("RC.Common.RclJoin");
            RCL_EXCEPTION_FORMAT = RCPackageFormatMap.Get("RC.Common.RclException");

            timer = new Stopwatch();
            timer.Start();
        }

        /// <summary>
        /// Constructs an RCLogTrace object.
        /// </summary>
        /// <param name="name">The name of this trace.</param>
        /// <param name="filePrefix">The prefix of the name of the output file.</param>
        public RCLogTrace(string name, string filePrefix)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }
            if (filePrefix == null || filePrefix.Length == 0) { throw new ArgumentNullException("filePrefix"); }

            this.trcName = name;
            //this.outputFileName = string.Format("{0}_{1}.rcl", filePrefix, Process.GetCurrentProcess().Id);
            this.outputStream = new FileStream(string.Format("{0}_{1}.rcl", filePrefix, Process.GetCurrentProcess().Id), FileMode.Create);
            this.outputWriter = new BinaryWriter(this.outputStream);
        }

        /// <see cref="ITrace.TrcName"/>
        public string TrcName
        {
            get { return this.trcName; }
        }

        /// <see cref="ITrace.WriteLine"/>
        public void WriteLine(object obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            string strToWrite = obj.ToString();
            if (strToWrite == null)
            {
                strToWrite = string.Empty;
            }

            RCPackage packageToWrite = RCPackage.CreateCustomDataPackage(RCL_EVENT_FORMAT);
            packageToWrite.WriteInt(0, RCThread.CurrentThread.WrappedThread.ManagedThreadId);
            packageToWrite.WriteString(1, RCThread.CurrentThread.Name);
            packageToWrite.WriteLong(2, timer.ElapsedMilliseconds);
            packageToWrite.WriteString(3, strToWrite);

            byte[] buffer = new byte[packageToWrite.PackageLength];
            packageToWrite.WritePackageToBuffer(buffer, 0);

            this.outputWriter.Write(buffer);
            this.outputWriter.Flush();
            this.outputStream.Flush();
        }

        /// <see cref="ITrace.WriteFork"/>
        public void WriteFork(RCThread newThread, RCThread parentThread)
        {
            if (newThread == null) { throw new ArgumentNullException("newThread"); }
            if (parentThread == null) { throw new ArgumentNullException("parentThread"); }

            RCPackage packageToWrite = RCPackage.CreateCustomDataPackage(RCL_FORK_FORMAT);
            packageToWrite.WriteInt(0, newThread.WrappedThread.ManagedThreadId);
            packageToWrite.WriteString(1, newThread.Name);
            packageToWrite.WriteInt(2, parentThread.WrappedThread.ManagedThreadId);
            packageToWrite.WriteString(3, parentThread.Name);
            packageToWrite.WriteLong(4, timer.ElapsedMilliseconds);

            byte[] buffer = new byte[packageToWrite.PackageLength];
            packageToWrite.WritePackageToBuffer(buffer, 0);

            this.outputWriter.Write(buffer);
            this.outputWriter.Flush();
            this.outputStream.Flush();
        }

        /// <see cref="ITrace.WriteJoin"/>
        public void WriteJoin(RCThread runningThread, RCThread waitingThread)
        {
            if (runningThread == null) { throw new ArgumentNullException("runningThread"); }
            if (waitingThread == null) { throw new ArgumentNullException("waitingThread"); }

            RCPackage packageToWrite = RCPackage.CreateCustomDataPackage(RCL_JOIN_FORMAT);
            packageToWrite.WriteInt(0, runningThread.WrappedThread.ManagedThreadId);
            packageToWrite.WriteString(1, runningThread.Name);
            packageToWrite.WriteInt(2, waitingThread.WrappedThread.ManagedThreadId);
            packageToWrite.WriteString(3, waitingThread.Name);
            packageToWrite.WriteLong(4, timer.ElapsedMilliseconds);

            byte[] buffer = new byte[packageToWrite.PackageLength];
            packageToWrite.WritePackageToBuffer(buffer, 0);

            this.outputWriter.Write(buffer);
            this.outputWriter.Flush();
            this.outputStream.Flush();
        }

        /// <see cref="ITrace.WriteLine"/>
        public void WriteException(Exception ex, bool isFatal)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            string strToWrite = ex.ToString();
            if (strToWrite == null)
            {
                strToWrite = string.Empty;
            }

            RCPackage packageToWrite = RCPackage.CreateCustomDataPackage(RCL_EXCEPTION_FORMAT);
            packageToWrite.WriteInt(0, RCThread.CurrentThread.WrappedThread.ManagedThreadId);
            packageToWrite.WriteString(1, RCThread.CurrentThread.Name);
            packageToWrite.WriteLong(2, timer.ElapsedMilliseconds);
            packageToWrite.WriteByte(3, isFatal ? (byte)0x01 : (byte)0x00);
            packageToWrite.WriteString(4, strToWrite);

            byte[] buffer = new byte[packageToWrite.PackageLength];
            packageToWrite.WritePackageToBuffer(buffer, 0);

            this.outputWriter.Write(buffer);
            this.outputWriter.Flush();
            this.outputStream.Flush();
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.outputWriter.Flush();
            this.outputStream.Flush();
            this.outputWriter.Close();
            this.outputWriter.Dispose();
            this.outputStream.Close();
            this.outputStream.Dispose();
        }

        /// <summary>
        /// ID of the RCL-event RCPackageFormat.
        /// </summary>
        private static readonly int RCL_EVENT_FORMAT;

        /// <summary>
        /// ID of the RCL-fork RCPackageFormat.
        /// </summary>
        private static readonly int RCL_FORK_FORMAT;

        /// <summary>
        /// ID of the RCL-join RCPackageFormat.
        /// </summary>
        private static readonly int RCL_JOIN_FORMAT;

        /// <summary>
        /// ID of the RCL-exception RCPackageFormat.
        /// </summary>
        private static readonly int RCL_EXCEPTION_FORMAT;

        /// <summary>
        /// Timer that is used to measure the timestamp of the logged events.
        /// </summary>
        private static Stopwatch timer;

        /// <summary>
        /// Name of the trace.
        /// </summary>
        private string trcName;

        /// <summary>
        /// Name of the output file.
        /// </summary>
        //private string outputFileName;

        /// <summary>
        /// The output filestream.
        /// </summary>
        private FileStream outputStream;

        /// <summary>
        /// The output writer.
        /// </summary>
        private BinaryWriter outputWriter;
    }
}
