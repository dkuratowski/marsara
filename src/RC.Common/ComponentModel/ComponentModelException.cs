using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.ComponentModel
{
    public class ComponentModelException : Exception
    {
        public ComponentModelException() { }
        public ComponentModelException(string message) : base(message) { }
        public ComponentModelException(string message, Exception inner) : base(message, inner) { }
        protected ComponentModelException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
