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
        /// Computes the section of this path from the cell with the given coordinates to the next region on the path.
        /// </summary>
        /// <param name="fromCoords">The starting cell of the path section.</param>
        /// <param name="mapContentMgr">The map content manager used for collision detection.</param>
        /// <returns>The list of the coordinates of the cells of the computed path section to follow.</returns>
        List<RCIntVector> FindPathSection<T>(RCIntVector fromCoords, IMapContentManager<T> mapContentMgr) where T : IMapContent;

        /// <summary>
        /// 
        /// </summary>
        //void ForgetAbortedPaths();
    }
}
