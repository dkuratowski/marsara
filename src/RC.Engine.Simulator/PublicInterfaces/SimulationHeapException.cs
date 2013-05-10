using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    public class SimulationHeapException : Exception
    {
        public SimulationHeapException() { }
        public SimulationHeapException(string message) : base(message) { }
        public SimulationHeapException(string message, Exception inner) : base(message, inner) { }
        protected SimulationHeapException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
