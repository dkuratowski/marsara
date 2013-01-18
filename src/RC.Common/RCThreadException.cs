using System;

namespace RC.Common
{
    public class RCThreadException : Exception
    {
        public RCThreadException() { }
        public RCThreadException(string message) : base(message) { }
        public RCThreadException(string message, Exception inner) : base(message, inner) { }
        protected RCThreadException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
