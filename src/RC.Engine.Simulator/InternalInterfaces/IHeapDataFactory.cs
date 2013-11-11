using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.InternalInterfaces
{
    /// <summary>
    /// Interface for creating objects for accessing heap data.
    /// </summary>
    interface IHeapDataFactory
    {
        /// <summary>
        /// Creates an object for accessing heap data of the given type at the given address.
        /// </summary>
        /// <param name="address">The address of the data on the heap to be accessed.</param>
        /// <param name="typeID">The type ID of the data to be accessed.</param>
        /// <returns>A reference to the created access object.</returns>
        IHeapData CreateHeapData(int address, short typeID);
    }
}
