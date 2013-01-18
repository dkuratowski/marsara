using System;

namespace RC.Common.SMC
{
    public class SMException : Exception
    {
        public SMException() { }
        public SMException(string message) : base(message) { }
        public SMException(string message, Exception inner) : base(message, inner) { }
        protected SMException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
