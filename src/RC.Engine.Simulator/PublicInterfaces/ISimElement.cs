using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Interface for accessing the simulation elements stored on the simulation-heap.
    /// </summary>
    /// <remarks>
    /// CAUTION !!! Using an instance of this interface after the corresponding simulation element has been deleted
    /// from the simulation-heap leads to undefined behavior.
    /// </remarks>
    public interface ISimElement
    {
        /// <summary>
        /// Reads the value behind this simulation element.
        /// </summary>
        /// <typeparam name="T">The type of the value to be read.</typeparam>
        /// <returns>The value behind this simulation element.</returns>
        /// <exception cref="SimulationHeapException">If the type of this element doesn't match with T.</exception>
        T Read<T>() where T : struct;

        /// <summary>
        /// Writes the given value in this simulation element.
        /// </summary>
        /// <typeparam name="T">The type of the value to be written.</typeparam>
        /// <param name="val">The value to be written.</param>
        /// <exception cref="SimulationHeapException">If the type of this element doesn't match with T.</exception>
        void Write<T>(T val) where T : struct;

        /// <summary>
        /// Creates a pointer that contains the address of the given simulation element and stores that pointer in this simulation element.
        /// </summary>
        /// <exception cref="SimulationHeapException">If the type of this element is not a pointer.</exception>
        void PointTo(ISimElement target);

        /// <summary>
        /// Gets the simulation element at the given index in the array addressed by the pointer behind this simulation element.
        /// </summary>
        /// <param name="arrayIdx">The index of the element to get inside the array.</param>
        /// <returns>The simulation element at the given index in the array addressed by the pointer behind this simulation element.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of this element is not a pointer.
        /// </exception>
        ISimElement Dereference(int arrayIdx);

        /// <summary>
        /// Deletes the element addressed by the pointer behind this simulation element.
        /// </summary>
        /// <exception cref="SimulationHeapException">If the type of this element is not a pointer.</exception>
        void Delete();

        /// <summary>
        /// Deletes the array addressed by the pointer behind this simulation element.
        /// </summary>
        /// <exception cref="SimulationHeapException">If the type of this element is not a pointer.</exception>
        void DeleteArray();

        /// <summary>
        /// Accesses the given field of the composite structure behind this simulation element.
        /// </summary>
        /// <param name="fieldIdx">The index of the field to get inside the composite structure.</param>
        /// <returns>The simulation element for reading the given field.</returns>
        /// <exception cref="SimulationHeapException">If the type of this element is not a composite structure.</exception>
        ISimElement AccessField(int fieldIdx);
    }
}
