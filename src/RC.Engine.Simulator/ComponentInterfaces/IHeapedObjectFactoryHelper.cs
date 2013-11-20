using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.InternalInterfaces;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// Internal component interface that contains helper methods for constructing HeapedObjects.
    /// </summary>
    [ComponentInterface]
    interface IHeapedObjectFactoryHelper
    {
        /// <summary>
        /// Gets a reference to the heap manager.
        /// </summary>
        IHeapManager HeapManager { get; }

        /// <summary>
        /// Gets the inheritence hierarchy of the given type starting from the base class.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        IHeapType[] GetInheritenceHierarchy(string typeName);
    }
}
