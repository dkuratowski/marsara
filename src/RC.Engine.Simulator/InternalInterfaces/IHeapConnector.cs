using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.InternalInterfaces
{
    /// <summary>
    /// Common interface of simulation-heap connector objects.
    /// </summary>
    /// <remarks>
    /// CAUTION !!! Using this interface after the connected simulation data has been deleted from the simulation-heap leads to undefined behavior.
    /// </remarks>
    interface IHeapConnector
    {
        /// <summary>
        /// Writes the address of the given simulation data to the pointer on the heap referred by the connector.
        /// </summary>
        /// <param name="target">The given simulation data or null for setting null-pointer.</param>
        /// <exception cref="HeapException">
        /// If the type of the connected data referred by the connector is not a pointer.
        /// If the type of the target data doesn't match with the type of the pointer referred by the connector.
        /// </exception>
        void PointTo(IHeapConnector target);

        /// <summary>
        /// Gets the simulation data addressed by the pointer referred by the connector.
        /// </summary>
        /// <returns>
        /// The connector of the simulation data addressed by the pointer referred by this connector or null
        /// if the pointer is a null-pointer.
        /// </returns>
        /// <exception cref="HeapException">
        /// If the type of the connected data is not a pointer.
        /// If the pointer referred by the connector points to an invalid part of the heap.
        /// </exception>
        IHeapConnector Dereference();

        /// <summary>
        /// Deletes the simulation data from the heap referred by the connector.
        /// </summary>
        /// <remarks>Don't use this interface after the deletion!</remarks>
        void Delete();

        /// <summary>
        /// Deletes the array of simulation data from the heap referred by the connector.
        /// </summary>
        void DeleteArray();

        /// <summary>
        /// Accesses the given field of the composite data structure referred by the connector.
        /// </summary>
        /// <param name="fieldIdx">The index of the field of the composite data structure to get.</param>
        /// <returns>The connector of the given field.</returns>
        /// <exception cref="HeapException">
        /// If the type of the data referred by the connector is not a composite structure.
        /// </exception>
        IHeapConnector AccessField(int fieldIdx);

        /// <summary>
        /// Accesses an item in the array referred by the connector.
        /// </summary>
        /// <param name="itemIdx">The index of the array item to get.</param>
        /// <returns>The connector of the given array item.</returns>
        /// <exception cref="HeapException">
        /// If the connector is not referring to the first item of an array on the heap.
        /// </exception>
        IHeapConnector AccessArrayItem(int itemIdx);
    }
}
