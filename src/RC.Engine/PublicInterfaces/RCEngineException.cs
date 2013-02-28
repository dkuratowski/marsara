using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.PublicInterfaces
{
    public class RCEngineException : Exception
    {
        public RCEngineException() { }
        public RCEngineException(string message) : base(message) { }
        public RCEngineException(string message, Exception inner) : base(message, inner) { }
        protected RCEngineException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
