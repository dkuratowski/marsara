using System;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Represents any exceptions that occur in the RC.NetworkingSystem.
    /// </summary>
    public class NetworkingSystemException : Exception
    {
        public NetworkingSystemException() { }
        public NetworkingSystemException(string message) : base(message) { }
        public NetworkingSystemException(string message, Exception inner) : base(message, inner) { }
        protected NetworkingSystemException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}