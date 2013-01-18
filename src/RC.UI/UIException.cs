using System;

namespace RC.UI
{
    /// <summary>
    /// Represents any exceptions that occur in the RC.UI.
    /// </summary>
    public class UIException : Exception
    {
        public UIException() { }
        public UIException(string message) : base(message) { }
        public UIException(string message, Exception inner) : base(message, inner) { }
        protected UIException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
