using RC.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// The public interface of the heap manager component.
    /// </summary>
    [ComponentInterface]
    public interface IHeapManager
    {
        /// <summary>
        /// Creates a new simulation heap.
        /// </summary>
        /// <exception cref="InvalidOperationException">If a simulation heap already created.</exception>
        void CreateHeap();

        /// <summary>
        /// Unloads the simulation heap.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no simulation heap created currently.</exception>
        void UnloadHeap();

        /// <summary>
        /// Computes the hash value of the current state of the simulation heap.
        /// </summary>
        /// <returns>The byte array that contains the hash.</returns>
        /// <exception cref="InvalidOperationException">If there is no simulation heap created or loaded currently.</exception>
        byte[] ComputeHash();
    }
}
