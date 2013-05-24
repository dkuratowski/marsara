using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents an item in a linked-list of simulation heap sections.
    /// </summary>
    class SimHeapSection
    {
        /// <summary>
        /// The start address of the section.
        /// </summary>
        public int Address;

        /// <summary>
        /// The length of the section (-1 if the section goes on to the end of the heap).
        /// </summary>
        public int Length;

        /// <summary>
        /// Reference to the next section in the list.
        /// </summary>
        public SimHeapSection Next;

        /// <summary>
        /// Reference to the previous section in the list.
        /// </summary>
        public SimHeapSection Prev;
    }
}
