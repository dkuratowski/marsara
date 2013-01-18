using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
{
    public class MapException : Exception
    {
        public MapException() { }
        public MapException(string message) : base(message) { }
        public MapException(string message, Exception inner) : base(message, inner) { }
        protected MapException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
