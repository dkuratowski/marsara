using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a node in the pathfinder tree.
    /// </summary>
    class PFTreeNode
    {
        /// <summary>
        /// Constructs the root node of a pathfinder tree.
        /// </summary>
        /// <param name="subdivisionLevels">Number of subdivision levels.</param>
        public PFTreeNode(int subdivisionLevels)
        {
            if (subdivisionLevels <= 0) { throw new ArgumentOutOfRangeException("subdivisionLevels", "Number of subdivision levels must be greater than 0!"); }
            if (subdivisionLevels > MAX_LEVELS) { throw new ArgumentOutOfRangeException("subdivisionLevels", string.Format("Number of subdivision levels must be less than {0}!", MAX_LEVELS)); }

            this.areaOnMap = new RCIntRectangle(0, 0, (int)Math.Pow(2, subdivisionLevels), (int)Math.Pow(2, subdivisionLevels));
            this.center = new RCIntVector((this.areaOnMap.Left + this.areaOnMap.Right - 1) / 2, (this.areaOnMap.Top + this.areaOnMap.Bottom - 1) / 2);
            this.walkability = Walkability.Walkable;
            this.children = new PFTreeNode[4];
            this.parent = null;
            this.root = this;
            this.index = -1;
            this.leafCount = 1;
            this.containerRegions = new HashSet<Region>();
        }

        /// <summary>
        /// Adds an obstacle to the given cell.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        public void AddObstacle(RCIntVector cellCoords)
        {
            this.root.AddObstacleImpl(cellCoords);
        }

        /// <summary>
        /// Gets the leaf node that contains the given cell.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the cell.</param>
        /// <returns>The leaf node that contains the given cell.</returns>
        public PFTreeNode GetLeafNode(RCIntVector cellCoords)
        {
            return this.root.GetLeafNodeImpl(cellCoords);
        }

        /// <summary>
        /// Gets all the leaf nodes in the pathfinder tree.
        /// </summary>
        /// <returns>The list of the leaf nodes in the pathfinder tree.</returns>
        public HashSet<PFTreeNode> GetAllLeafNodes()
        {
            HashSet<PFTreeNode> retList = new HashSet<PFTreeNode>();
            this.root.GetAllLeafNodesImpl(retList);
            return retList;
        }

        /// <summary>
        /// Gets all the leaf nodes in the pathfinder tree having intersection with the given area.
        /// </summary>
        /// <param name="area">The area to intersect.</param>
        /// <returns>The list of the intersecting leaf nodes in the pathfinder tree.</returns>
        public HashSet<PFTreeNode> GetAllLeafNodes(RCIntRectangle area)
        {
            HashSet<PFTreeNode> retList = new HashSet<PFTreeNode>();
            this.root.GetAllLeafNodesImpl(retList, area);
            return retList;
        }

        /// <summary>
        /// Checks whether the given area intersects a map obstacle or not.
        /// </summary>
        /// <param name="area">The area to check.</param>
        /// <returns>True if the area intersects any map obstacle, false otherwise.</returns>
        public bool CheckObstacleIntersection(RCIntRectangle area)
        {
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }

            if (this.areaOnMap.IntersectsWith(area))
            {
                if (this.walkability == Walkability.Mixed)
                {
                    return this.children[0].CheckObstacleIntersection(area) ||
                           this.children[1].CheckObstacleIntersection(area) ||
                           this.children[2].CheckObstacleIntersection(area) ||
                           this.children[3].CheckObstacleIntersection(area);
                }
                else
                {
                    return this.walkability == Walkability.NonWalkable;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the neighbours of this node.
        /// </summary>
        public IEnumerable<PFTreeNode> Neighbours
        {
            get
            {
                if (this.walkability == Walkability.Mixed) { throw new InvalidOperationException("Non leaf node!"); }
                if (this.neighboursCache == null) { this.SearchNeighbours(); }
                return this.neighboursCache;
            }
        }

        /// <summary>
        /// Gets the area on the map covered by this node.
        /// </summary>
        public RCIntRectangle AreaOnMap { get { return this.areaOnMap; } }

        /// <summary>
        /// Gets the coordinates of the center of this node.
        /// </summary>
        public RCIntVector Center { get { return this.center; } }

        /// <summary>
        /// The index of this PFTreeNode.
        /// </summary>
        public int Index { get { return this.index; } }

        /// <summary>
        /// Gets the total number of leaf nodes in the pathfinding tree.
        /// </summary>
        public int LeafCount { get { return this.root.leafCount; } }

        /// <summary>
        /// Gets whether this PFTreeNode is a leaf node or not.
        /// </summary>
        public bool IsLeafNode { get { return this.walkability != Walkability.Mixed; } }

        /// <summary>
        /// Gets whether this node is walkable or not.
        /// </summary>
        public bool IsWalkable
        {
            get
            {
                if (this.walkability == Walkability.Mixed) { throw new InvalidOperationException("Non leaf node!"); }
                return this.walkability == Walkability.Walkable;
            }
        }

        /// <summary>
        /// Enumerates the possible values of the walkability property.
        /// </summary>
        private enum Walkability
        {
            NonWalkable = 0,
            Walkable = 1,
            Mixed = 2
        }

        /// <summary>
        /// Internal ctor.
        /// </summary>
        private PFTreeNode(RCIntRectangle areaOnMap, PFTreeNode parent, Walkability walkability)
        {
            this.areaOnMap = areaOnMap;
            this.center = new RCIntVector((this.areaOnMap.Left + this.areaOnMap.Right - 1) / 2, (this.areaOnMap.Top + this.areaOnMap.Bottom - 1) / 2);
            this.walkability = walkability;
            this.children = new PFTreeNode[4];
            this.parent = parent;
            this.root = parent.root;
            this.index = -1;
            this.containerRegions = new HashSet<Region>();
        }

        /// <summary>
        /// Sets the indices of the leaf nodes in the pathfinding tree.
        /// </summary>
        internal void SetLeafNodeIndices()
        {
            int currentIndex = 0;
            foreach (PFTreeNode node in this.GetAllLeafNodes())
            {
                if (node.index != -1) { throw new InvalidOperationException("Node index already set!"); }
                node.index = currentIndex;
                currentIndex++;
            }
        }

        /// <summary>
        /// Searches the neighbours of this node.
        /// </summary>
        internal void SearchNeighbours()
        {
            this.neighboursCache = new HashSet<PFTreeNode>();

            /// Search above this node if necessary.
            if (this.areaOnMap.Top != this.root.areaOnMap.Top)
            {
                for (int col = this.areaOnMap.Left; col < this.areaOnMap.Right && col < this.root.areaOnMap.Right; )
                {
                    PFTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(col, this.areaOnMap.Top - 1));
                    this.neighboursCache.Add(neighbour);
                    col = neighbour.areaOnMap.Right;
                }
            }

            /// Search on the right side of this node if necessary.
            if (this.areaOnMap.Right != this.root.areaOnMap.Right)
            {
                for (int row = this.areaOnMap.Top; row < this.areaOnMap.Bottom && row < this.root.areaOnMap.Bottom; )
                {
                    PFTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnMap.Right, row));
                    this.neighboursCache.Add(neighbour);
                    row = neighbour.areaOnMap.Bottom;
                }
            }

            /// Search below this node if necessary.
            if (this.areaOnMap.Bottom != this.root.areaOnMap.Bottom)
            {
                for (int col = this.areaOnMap.Right - 1; col >= this.areaOnMap.Left && col >= this.root.areaOnMap.Left; )
                {
                    PFTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(col, this.areaOnMap.Bottom));
                    this.neighboursCache.Add(neighbour);
                    col = neighbour.areaOnMap.Left - 1;
                }
            }

            /// Search on the left side of this node if necessary.
            if (this.areaOnMap.Left != this.root.areaOnMap.Left)
            {
                for (int row = this.areaOnMap.Bottom - 1; row >= this.areaOnMap.Top && row >= this.root.areaOnMap.Top; )
                {
                    PFTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnMap.Left - 1, row));
                    this.neighboursCache.Add(neighbour);
                    row = neighbour.areaOnMap.Top - 1;
                }
            }
        }

        /// <summary>
        /// Adds this node to the given region.
        /// </summary>
        /// <param name="region">The region to which this node shall be added.</param>
        internal void AddToRegion(Region region)
        {
            this.containerRegions.Add(region);
        }

        /// <summary>
        /// Removes this node from the given region.
        /// </summary>
        /// <param name="region">The region from which this node shall be removed.</param>
        internal void RemoveFromRegion(Region region)
        {
            this.containerRegions.Remove(region);
        }

        /// <summary>
        /// Gets the regions containing this node.
        /// </summary>
        internal IEnumerable<Region> Regions { get { return this.containerRegions; } }

        /// <summary>
        /// The internal implementation of the PFTreeNode.AddObstacle method.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        private void AddObstacleImpl(RCIntVector cellCoords)
        {
            if (!this.areaOnMap.Contains(cellCoords)) { throw new ArgumentOutOfRangeException("cellCoords"); }

            if (this.areaOnMap.Width == 1)
            {
                /// End of recursion.
                this.walkability = Walkability.NonWalkable;
                return;
            }

            if (this.walkability == Walkability.Walkable)
            {
                /// Subdivide the node.
                this.Subdivide();
                if (this.children[NORTHWEST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { this.children[NORTHWEST_CHILD_IDX].AddObstacleImpl(cellCoords); }
                else if (this.children[NORTHEAST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { this.children[NORTHEAST_CHILD_IDX].AddObstacleImpl(cellCoords); }
                else if (this.children[SOUTHEAST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { this.children[SOUTHEAST_CHILD_IDX].AddObstacleImpl(cellCoords); }
                else if (this.children[SOUTHWEST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { this.children[SOUTHWEST_CHILD_IDX].AddObstacleImpl(cellCoords); }
                this.walkability = Walkability.Mixed;
                this.root.leafCount += 3;
            }
            else if (this.walkability == Walkability.Mixed)
            {
                /// Merge the node if necessary.
                int childIdx = -1;
                if (this.children[NORTHWEST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { childIdx = NORTHWEST_CHILD_IDX; }
                else if (this.children[NORTHEAST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { childIdx = NORTHEAST_CHILD_IDX; }
                else if (this.children[SOUTHEAST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { childIdx = SOUTHEAST_CHILD_IDX; }
                else if (this.children[SOUTHWEST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { childIdx = SOUTHWEST_CHILD_IDX; }

                this.children[childIdx].AddObstacleImpl(cellCoords);
                if (this.children[NORTHWEST_CHILD_IDX].walkability == Walkability.NonWalkable &&
                    this.children[NORTHEAST_CHILD_IDX].walkability == Walkability.NonWalkable &&
                    this.children[SOUTHEAST_CHILD_IDX].walkability == Walkability.NonWalkable &&
                    this.children[SOUTHWEST_CHILD_IDX].walkability == Walkability.NonWalkable)
                {
                    this.children = new PFTreeNode[4];
                    this.walkability = Walkability.NonWalkable;
                    this.root.leafCount -= 3;
                }
            }
        }

        /// <summary>
        /// The internal implementation of the PFTreeNode.GetLeafNode method.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        private PFTreeNode GetLeafNodeImpl(RCIntVector cellCoords)
        {
            if (!this.areaOnMap.Contains(cellCoords)) { throw new ArgumentOutOfRangeException("cellCoords"); }

            if (this.walkability != Walkability.Mixed) { return this; }

            if (this.children[NORTHWEST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { return this.children[NORTHWEST_CHILD_IDX].GetLeafNodeImpl(cellCoords); }
            else if (this.children[NORTHEAST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { return this.children[NORTHEAST_CHILD_IDX].GetLeafNodeImpl(cellCoords); }
            else if (this.children[SOUTHEAST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { return this.children[SOUTHEAST_CHILD_IDX].GetLeafNodeImpl(cellCoords); }
            else if (this.children[SOUTHWEST_CHILD_IDX].areaOnMap.Contains(cellCoords)) { return this.children[SOUTHWEST_CHILD_IDX].GetLeafNodeImpl(cellCoords); }
            
            /// Error case.
            return null;
        }

        /// <summary>
        /// The internal implementation of PFTreeNode.GetAllLeafNodes.
        /// </summary>
        /// <param name="leafNodes">The list that contains the collected leaf nodes.</param>
        private void GetAllLeafNodesImpl(HashSet<PFTreeNode> leafNodes)
        {
            if (this.walkability != Walkability.Mixed)
            {
                /// Leaf node found -> end of recursion.
                leafNodes.Add(this);
                return;
            }

            /// Call this method recursively on all children.
            foreach (PFTreeNode child in this.children)
            {
                child.GetAllLeafNodesImpl(leafNodes);
            }
        }

        /// <summary>
        /// The internal implementation of PFTreeNode.GetAllLeafNodes.
        /// </summary>
        /// <param name="leafNodes">The list that contains the collected leaf nodes.</param>
        private void GetAllLeafNodesImpl(HashSet<PFTreeNode> leafNodes, RCIntRectangle area)
        {
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }

            if (this.areaOnMap.IntersectsWith(area))
            {
                if (this.walkability == Walkability.Mixed)
                {
                    /// Not leaf node -> Call this method recursively on all children.
                    foreach (PFTreeNode child in this.children)
                    {
                        child.GetAllLeafNodesImpl(leafNodes, area);
                    }
                }
                else
                {
                    /// Leaf node -> Add to the list if walkable.
                    if (this.walkability == Walkability.Walkable) { leafNodes.Add(this); }
                }
            }
        }

        /// <summary>
        /// Subdivides this node.
        /// </summary>
        private void Subdivide()
        {
            this.children[NORTHWEST_CHILD_IDX] = new PFTreeNode(new RCIntRectangle(this.areaOnMap.X, this.areaOnMap.Y, this.areaOnMap.Width / 2, this.areaOnMap.Height / 2), this, this.walkability);
            this.children[NORTHEAST_CHILD_IDX] = new PFTreeNode(new RCIntRectangle(this.areaOnMap.X + this.areaOnMap.Width / 2, this.areaOnMap.Y, this.areaOnMap.Width / 2, this.areaOnMap.Height / 2), this, this.walkability);
            this.children[SOUTHEAST_CHILD_IDX] = new PFTreeNode(new RCIntRectangle(this.areaOnMap.X + this.areaOnMap.Width / 2, this.areaOnMap.Y + this.areaOnMap.Height / 2, this.areaOnMap.Width / 2, this.areaOnMap.Height / 2), this, this.walkability);
            this.children[SOUTHWEST_CHILD_IDX] = new PFTreeNode(new RCIntRectangle(this.areaOnMap.X, this.areaOnMap.Y + this.areaOnMap.Height / 2, this.areaOnMap.Width / 2, this.areaOnMap.Height / 2), this, this.walkability);
        }

        /// <summary>
        /// The area on the map covered by this node.
        /// </summary>
        private RCIntRectangle areaOnMap;

        /// <summary>
        /// The coordinates of the center of this node.
        /// </summary>
        private RCIntVector center;

        /// <summary>
        /// The walkability property of this node.
        /// </summary>
        private Walkability walkability;

        /// <summary>
        /// Reference to the 4 child nodes (order: NW, NE, SE, SW).
        /// </summary>
        private PFTreeNode[] children;

        /// <summary>
        /// Reference to the parent node.
        /// </summary>
        private PFTreeNode parent;

        /// <summary>
        /// Reference to the root node.
        /// </summary>
        private PFTreeNode root;

        /// <summary>
        /// The index of this PFTreeNode if this is a leaf node; -1 otherwise.
        /// </summary>
        private int index;

        /// <summary>
        /// The total number of the leaf nodes in the pathfinding tree if this is the root node; -1 otherwise.
        /// </summary>
        private int leafCount;

        /// <summary>
        /// Reference to the neighbours of this node.
        /// </summary>
        private HashSet<PFTreeNode> neighboursCache;

        /// <summary>
        /// List of the regions containing this node.
        /// </summary>
        private HashSet<Region> containerRegions;

        /// <summary>
        /// The indices of the child nodes depending on their location.
        /// </summary>
        private const int NORTHWEST_CHILD_IDX = 0;
        private const int NORTHEAST_CHILD_IDX = 1;
        private const int SOUTHEAST_CHILD_IDX = 2;
        private const int SOUTHWEST_CHILD_IDX = 3;

        /// <summary>
        /// The maximum number of subdivision levels in a pathfinder tree.
        /// </summary>
        private const int MAX_LEVELS = 16;
    }
}
