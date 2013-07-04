using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a path computed by the pathfinder.
    /// </summary>
    class Path : IPath
    {
        /// <summary>
        /// Constructs a new Path instance.
        /// </summary>
        /// <param name="fromCoords">The coordinates of the starting cell of the path.</param>
        /// <param name="toCoords">The coordinates of the target cell of the path.</param>
        /// <param name="size">The size of the object that requested the pathfinding.</param>
        public Path(RCIntVector fromCoords, RCIntVector toCoords, RCNumVector size)
        {
        }

        #region IPath methods

        /// <see cref="IPath.FindPathSection<T>"/>
        public List<RCIntVector> FindPathSection<T>(RCIntVector fromCoords, IMapContentManager<T> mapContentMgr) where T : IMapContent
        {
            throw new NotImplementedException();
        }

        #endregion IPath methods
    }
}
