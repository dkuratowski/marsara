using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the pathfinder component.
    /// </summary>
    [Component("RC.Engine.Simulator.PathFinder")]
    class PathFinder : IPathFinder
    {
        /// <summary>
        /// Constructs a PathFinder object.
        /// </summary>
        public PathFinder()
        {
            this.map = null;
            this.pathfinderTreeRoot = null;
        }

        /// <see cref="IPathFinder.Initialize"/>
        public void Initialize(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }

            /// Find the number of subdivision levels.
            int boundingBoxSize = Math.Max(map.CellSize.X, map.CellSize.Y);
            int subdivisionLevels = 1;
            while (boundingBoxSize > (int)Math.Pow(2, subdivisionLevels)) { subdivisionLevels++; }

            /// Create the root node of the pathfinder tree.
            this.pathfinderTreeRoot = new PFTreeNode(subdivisionLevels);

            /// Add the obstacles to the pathfinder tree.
            for (int row = 0; row < this.pathfinderTreeRoot.AreaOnMap.Height; row++)
            {
                for (int column = 0; column < this.pathfinderTreeRoot.AreaOnMap.Width; column++)
                {
                    if (row >= map.CellSize.Y || column >= map.CellSize.X)
                    {
                        /// Everything out of the map range is considered to be obstacle.
                        this.pathfinderTreeRoot.AddObstacle(new RCIntVector(column, row));
                    }
                    else
                    {
                        /// Add obstacle depending on the "IsWalkable" flag of the cell.
                        if (!map.GetCell(new RCIntVector(column, row)).IsWalkable)
                        {
                            this.pathfinderTreeRoot.AddObstacle(new RCIntVector(column, row));
                        }
                    }
                }
            }

            this.map = map;
        }

        /// <see cref="IPathFinder.FindPath"/>
        public IPath FindPath(RCIntVector fromCoords, RCIntVector toCoords, RCNumVector size)
        {
            if (this.map == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (fromCoords == RCIntVector.Undefined) { throw new ArgumentNullException("fromCoords"); }
            if (toCoords == RCIntVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (size == RCNumVector.Undefined) { throw new ArgumentNullException("size"); }

            PFTreeNode fromNode = this.pathfinderTreeRoot.GetLeafNode(fromCoords);
            Path retPath = new Path(fromNode, toCoords, size);
            return retPath;
        }

        /// <see cref="IPathFinder.FindAlternativePath"/>
        public IPath FindAlternativePath(IPath originalPath, int abortedSectionIdx)
        {
            if (this.map == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (originalPath == null) { throw new ArgumentNullException("originalPath"); }
            if (abortedSectionIdx < 0 || abortedSectionIdx >= originalPath.Length - 1) { throw new ArgumentOutOfRangeException("abortedSectionIdx"); }

            Path retPath = new Path((Path)originalPath, abortedSectionIdx);
            return retPath;
        }

        /// <see cref="IPathFinder.CheckObstacleIntersection"/>
        public bool CheckObstacleIntersection(RCNumRectangle area)
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }

            int left = area.Left.Round();
            int top = area.Top.Round();
            int right = area.Right.Round();
            int bottom = area.Bottom.Round();
            RCIntRectangle areaCells = new RCIntRectangle(left, top, right - left + 1, bottom - top + 1);
            return this.pathfinderTreeRoot.CheckObstacleIntersection(areaCells);
        }

        /// <see cref="IPathFinder.GetTreeNodes"/>
        public List<RCIntRectangle> GetTreeNodes(RCIntRectangle area)
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }

            List<RCIntRectangle> retList = new List<RCIntRectangle>();
            foreach (PFTreeNode treeNode in this.pathfinderTreeRoot.GetAllLeafNodes(area))
            {
                retList.Add(treeNode.AreaOnMap);
            }
            return retList;
        }

        /// <summary>
        /// Reference to the searched map.
        /// </summary>
        private IMapAccess map;

        /// <summary>
        /// Reference to the root of the pathfinder tree.
        /// </summary>
        private PFTreeNode pathfinderTreeRoot;

        /// <summary>
        /// Name of the cell data field that indicates walkability.
        /// </summary>
        private const string ISWALKABLE_FIELD_NAME = "IsWalkable";
    }
}
