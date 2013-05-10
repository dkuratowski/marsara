using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Interface for accessing the simulation data stored on the simulation heap and the metadata used for
    /// interpretation of the simulation data.
    /// </summary>
    public interface ISimulationHeapMgr
    {
        /// <summary>
        /// Gets the ID that uniquely identifies the given type.
        /// </summary>
        /// <param name="type">The type string.</param>
        /// <returns>The ID of the given type.</returns>
        int GetTypeID(string type);

        /// <summary>
        /// Gets the index of the given field in the given composite type.
        /// </summary>
        /// <param name="typeID">The ID of the type.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns></returns>
        int GetFieldIdx(int typeID, string fieldName);

        /// <summary>
        /// Get the type of the given field in the given composite type.
        /// </summary>
        /// <param name="typeID">The ID of the composite type.</param>
        /// <param name="fieldIdx">The index of the field.</param>
        /// <returns>The type of the given field.</returns>
        string GetFieldType(int typeID, int fieldIdx);

        /// <summary>
        /// Get the type ID of the given field in the given composite type.
        /// </summary>
        /// <param name="typeID">The ID of the composite type.</param>
        /// <param name="fieldIdx">The index of the field.</param>
        /// <returns>The type ID of the given field.</returns>
        int GetFieldTypeID(int typeID, int fieldIdx);

        /// <summary>
        /// Creates a simulation element of the given type.
        /// </summary>
        /// <param name="typeID">The ID of the type of the element to create.</param>
        /// <returns>Reference to the created simulation element.</returns>
        ISimElement New(int typeID);

        /// <summary>
        /// Creates the given count of simulation elements of the given type and puts them into an array.
        /// </summary>
        /// <param name="typeID">The ID of the type of the elements to create.</param>
        /// <param name="count">The count of the elements in the array.</param>
        /// <returns>Reference to the simulation element of the first item in the array.</returns>
        ISimElement NewArray(int typeID, int count);
    }
}
