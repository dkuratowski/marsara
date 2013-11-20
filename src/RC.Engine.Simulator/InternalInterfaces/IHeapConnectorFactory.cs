using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.InternalInterfaces
{
    /// <summary>
    /// Interface for creating heap connectors.
    /// </summary>
    interface IHeapConnectorFactory
    {
        /// <summary>
        /// Creates heap connector object for the given heap type that connects to the given address on the heap.
        /// </summary>
        /// <param name="address">The address of the connected data on the heap.</param>
        /// <param name="typeID">The type ID of the connected data on the heap.</param>
        /// <returns>A reference to the created connector.</returns>
        IHeapConnector CreateHeapConnector(int address, short typeID);
    }
}
