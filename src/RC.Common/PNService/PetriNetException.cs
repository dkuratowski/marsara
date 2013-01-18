using System;

namespace RC.Common.PNService
{
    public class PetriNetException : Exception
    {
        public PetriNetException() { }
        public PetriNetException(string message) : base(message) { }
        public PetriNetException(string message, Exception inner) : base(message, inner) { }
        protected PetriNetException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
