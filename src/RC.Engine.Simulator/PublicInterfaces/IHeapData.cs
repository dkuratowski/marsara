using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Interface for accessing simulation data stored on the simulation-heap.
    /// </summary>
    /// <remarks>
    /// CAUTION !!! Using this interface after the corresponding simulation data has been deleted from the simulation-heap leads to undefined behavior.
    /// </remarks>
    public interface IHeapData
    {
        /// <summary>
        /// Writes the address of the target simulation data to the pointer on the heap referred by this interface.
        /// </summary>
        /// <param name="target">The target simulation data or null for setting null-pointer.</param>
        /// <exception cref="HeapException">
        /// If the type of the data referred by this interface is not a pointer.
        /// If the type of the target simulation data doesn't match with the type of this pointer.
        /// </exception>
        void PointTo(IHeapData target);

        /// <summary>
        /// Gets the simulation data addressed by the pointer referred by this interface.
        /// </summary>
        /// <returns>
        /// The access interface of the simulation data addressed by the pointer referred by this interface or null
        /// if the pointer is 0.
        /// </returns>
        /// <exception cref="HeapException">
        /// If the type of the data referred by this interface is not a pointer.
        /// If the pointer referred by this interface points to an invalid part of the heap.
        /// </exception>
        IHeapData Dereference();

        /// <summary>
        /// Deletes the simulation data from the heap referred by this interface.
        /// </summary>
        /// <remarks>Don't use this interface after deletion!</remarks>
        void Delete();

        /// <summary>
        /// Deletes the array of simulation data from the heap referred by this interface.
        /// </summary>
        void DeleteArray();

        /// <summary>
        /// Accesses the given field of the composite structure referred by this interface.
        /// </summary>
        /// <param name="fieldIdx">The index of the field of the composite structure to get.</param>
        /// <returns>The interface for accessing the given field.</returns>
        /// <exception cref="HeapException">
        /// If the type of the data referred by this interface is not a composite structure.
        /// </exception>
        IHeapData AccessField(int fieldIdx);

        /// <summary>
        /// Accesses an item in the array referred by this interface.
        /// </summary>
        /// <param name="itemIdx">The index of the array item to get.</param>
        /// <returns>The interface for accessing the given array item.</returns>
        /// <exception cref="HeapException">
        /// If this interface is not referring to the first item of an array on the heap.
        /// </exception>
        IHeapData AccessArrayItem(int itemIdx);
    }
}
