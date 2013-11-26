using System;
using System.Collections.Generic;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.InternalInterfaces;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// This interface provides type-safe access to the simulation data stored on the simulation heap.
    /// The possible types of the simulation data are declared by the simulation metadata.
    /// This interface is used for
    ///     - accessing the simulation data stored on the simulation heap
    ///     - accessing the type informations
    /// There are three different groups of types that the simulation heap manager can handle:
    ///     - built-in types: byte - 8 bit unsigned integer
    ///                       short - 16 bit signed little-endian integer
    ///                       int - 32 bit signed little-endian integer
    ///                       long - 64 bit signed little-endian integer
    ///                       num - 32 bit signed fixed point number (see RCNumber)
    ///                       intvect - a 2D vector of integers (see RCIntVector)
    ///                       numvect - a 2D vector of fixed point numbers (see RCNumVector)
    ///                       intrect - a 2D rectangle with integer coordinates and size (see RCIntRectangle)
    ///                       numrect - a 2D rectangle with fixed point coordinates and size (see RCNumRectangle)
    ///     - composite types: user defined data structures. A composite type must have a unique name and 1 or more fields. Each field has
    ///                        a name - that is unique inside the containing composite type - and a type - that can be any of the built-in,
    ///                        composite or pointer types. The only restriction is that you cannot define a metadata with infinite cycle
    ///                        in the layout of a type.
    ///                        For example: Suppose that we have a composite type A and B, and A has a field of type B and B has a field of
    ///                                     type A. In this case you will get an exception when the metadata is being parsed.
    ///     - pointer types: the value of a pointer refers directly to another value of a given type stored elsewhere on the simulation heap.
    ///                      When defining a pointer type in the metadata you have to define the type of the data that will be referred by that
    ///                      pointer.
    ///                      For example: the type of pointers to int values shall be marked as int*
    ///                                   the type of pointers to pointers to int values shall be marked as int**
    ///                                   etc.
    /// Every type (including the built-in, composite and pointer types) will be given a unique ID when the metadata is being parsed. This ID
    /// shall be used when you want to store a data of a given type on the simulation heap.
    /// Every field of a composite type will be given an index when the metadata is being parsed. This index shall be used when you want to
    /// access one of the fields of a composite data.
    /// Informations of a type can be queried by its name or ID using this interface.
    /// </summary>
    [ComponentInterface]
    interface IHeapManagerInternals
    {
        /// <summary>
        /// Gets the inheritence hierarchy of the given type starting from the base class.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        IHeapType[] GetInheritenceHierarchy(string typeName);

        /// <summary>
        /// Gets whether the simulation heap is currently attached or not.
        /// </summary>
        bool IsHeapAttached { get; }

        /// <summary>
        /// This event is raised when a new simulation heap has been created and the HeapedObjects must be
        /// attached to the heap.
        /// </summary>
        event EventHandler AttachingHeapedObjects;

        /// <summary>
        /// This event is raised when a new simulation heap has been created and the HeapedObjects must
        /// synchronize their fields to the heap.
        /// </summary>
        event EventHandler SynchronizingHeapedObjects;

        /// <summary>
        /// This event is raised when the simulation heap has been destroyed and the HeapedObjects must be
        /// detached from the heap.
        /// </summary>
        event EventHandler DetachingHeapedObjects;

        /// <summary>
        /// Gets the heap type with the given name.
        /// </summary>
        /// <param name="typeName">The name of the type to get.</param>
        /// <returns>The heap type with the given name.</returns>
        IHeapType GetHeapType(string typeName);

        /// <summary>
        /// Gets the heap type with the given type ID.
        /// </summary>
        /// <param name="typeID">The ID of the type to get.</param>
        /// <returns>The heap type with the given type ID.</returns>
        IHeapType GetHeapType(short typeID);

        /// <summary>
        /// Allocates space on the simulation heap for storing simulation data of the given type.
        /// </summary>
        /// <param name="typeID">The ID of the type of the simulation data to to be stored.</param>
        /// <returns>An interface for accessing the allocated simulation data.</returns>
        /// <remarks>
        /// Use the IHeapConnector.Delete method on the returned interface for deleting the created simulation data when
        /// no longer needed.
        /// CAUTION!!! If a simulation data was allocated with IHeapConnector.New it must be deleted with IHeapConnector.Delete!
        /// If a simulation data was allocated with IHeapConnector.NewArray it must be deleted with IHeapConnector.DeleteArray!
        /// Any other usage leads to undefined behavior.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If there is no simulation heap created or loaded currently.</exception>
        IHeapConnector New(short typeID);

        /// <summary>
        /// Allocates space on the simulation heap for storing an array of simulation data of the given type.
        /// </summary>
        /// <param name="typeID">The ID of the type of the simulation data to be stored.</param>
        /// <param name="count">The count of the elements in the array.</param>
        /// <returns>An interface for accessing the first item in the array.</returns>
        /// <remarks>
        /// Use the IHeapConnector.DeleteArray method on the returned interface for deleting the created array when
        /// no longer needed.
        /// CAUTION!!! If a simulation data was allocated with IHeapConnector.New it must be deleted with IHeapConnector.Delete!
        /// If a simulation data was allocated with IHeapConnector.NewArray it must be deleted with IHeapConnector.DeleteArray!
        /// Any other usage leads to undefined behavior.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If there is no simulation heap created or loaded currently.</exception>
        IHeapConnector NewArray(short typeID, int count);
    }
}
