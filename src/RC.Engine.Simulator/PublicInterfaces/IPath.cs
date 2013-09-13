using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// The public interface of a path computed by the pathfinder component.
    /// </summary>
    public interface IPath
    {
        /// <summary>
        /// Gets a section of the computed path.
        /// </summary>
        /// <param name="index">The index of the section to get.</param>
        /// <returns>The area of the section.</returns>
        RCIntRectangle this[int index] { get; }

        /// <summary>
        /// The total number of sections on this computed path.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Forgets every blocked edges used to compute this path.
        /// </summary>
        void ForgetBlockedEdges();
    }
}