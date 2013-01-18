using System;

namespace RC.DssServices
{
    /// <summary>
    /// Represents any exceptions that occur in the RC.DssServices.
    /// </summary>
    public class DssException : Exception
    {
        public DssException() { }
        public DssException(string message) : base(message) { }
        public DssException(string message, Exception inner) : base(message, inner) { }
        protected DssException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}