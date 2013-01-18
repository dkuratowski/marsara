using System;

namespace RC.Common
{
    public class FifoException : Exception
    {
        public FifoException() { }
        public FifoException(string message) : base(message) { }
        public FifoException(string message, Exception inner) : base(message, inner) { }
        protected FifoException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
