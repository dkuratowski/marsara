using System;

namespace RC.Common
{
    public class RCPackageException : Exception
    {
        public RCPackageException() { }
        public RCPackageException(string message) : base(message) { }
        public RCPackageException(string message, Exception inner) : base(message, inner) { }
        protected RCPackageException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
