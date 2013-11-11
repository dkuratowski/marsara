using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    public class HeapException : Exception
    {
        public HeapException() { }
        public HeapException(string message) : base(message) { }
        public HeapException(string message, Exception inner) : base(message, inner) { }
        protected HeapException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
