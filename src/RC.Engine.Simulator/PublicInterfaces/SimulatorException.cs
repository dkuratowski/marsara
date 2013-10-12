using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    public class SimulatorException : Exception
    {
        public SimulatorException() { }
        public SimulatorException(string message) : base(message) { }
        public SimulatorException(string message, Exception inner) : base(message, inner) { }
        protected SimulatorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
