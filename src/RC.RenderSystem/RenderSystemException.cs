using System;

namespace RC.RenderSystem
{
    /// <summary>
    /// Represents any exceptions that occur in the RC RenderSystem.
    /// </summary>
    public class RenderSystemException : Exception
    {
        public RenderSystemException() { }
        public RenderSystemException(string message) : base(message) { }
        public RenderSystemException(string message, Exception inner) : base(message, inner) { }
        protected RenderSystemException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}