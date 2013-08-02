using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.Core
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
            this.center = new RCNumVector((RCNumber)(this.areaOnMap.Left + this.areaOnMap.Right - 1) / 2, (RCNumber)(this.areaOnMap.Top + this.areaOnMap.Bottom - 1) / 2);
            this.walkability = Walkability.Walkable;
            this.children = new PFTreeNode[4];
            this.parent = null;
            this.root = this;
        }

        /// <summary>
        /// Adds an obstacle to the given cell.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        public void AddObstacle(RCIntVector cellCoords)
        {
            if (this.parent != null) { throw new InvalidOperationException("Non root node!"); }
            if (!this.areaOnMap.Contains(cellCoords)) { throw new ArgumentOutOfRangeException("cellCoords"); }
            this.AddObstacleImpl(cellCoords);
        }

        /// <summary>
        /// Gets the leaf node that contains the given cell.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the cell.</param>
        /// <returns>The leaf node that contains the given cell.</returns>
        public PFTreeNode GetLeafNode(RCIntVector cellCoords)
        {
            if (this.parent != null) { return this.root.GetLeafNodeImpl(cellCoords); }
            if (!this.areaOnMap.Contains(cellCoords)) { throw new ArgumentOutOfRangeException("cellCoords"); }
            else { return this.GetLeafNodeImpl(cellCoords); }
        }

        /// <summary>
        /// Gets all the leaf nodes in the pathfinder tree.
        /// </summary>
        /// <returns>The list of the leaf nodes in the pathfinder tree.</returns>
        public HashSet<PFTreeNode> GetAllLeafNodes()
        {
            HashSet<PFTreeNode> retList = new HashSet<PFTreeNode>();
            if (this.parent != null) { this.root.GetAllLeafNodesImpl(retList); }
            else { this.GetAllLeafNodesImpl(retList); }
            return retList;
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
        public RCNumVector Center { get { return this.center; } }

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
            this.center = new RCNumVector((RCNumber)(this.areaOnMap.Left + this.areaOnMap.Right - 1) / 2, (RCNumber)(this.areaOnMap.Top + this.areaOnMap.Bottom - 1) / 2);
            this.walkability = walkability;
            this.children = new PFTreeNode[4];
            this.parent = parent;
            this.root = parent.root;
        }

        /// <summary>
        /// Searches the neighbours of this node.
        /// </summary>
        private void SearchNeighbours()
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
        /// The internal implementation of the PFTreeNode.AddObstacle method.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        private void AddObstacleImpl(RCIntVector cellCoords)
        {
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
                }
            }
        }

        /// <summary>
        /// The internal implementation of the PFTreeNode.GetLeafNode method.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        private PFTreeNode GetLeafNodeImpl(RCIntVector cellCoords)
        {
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
        /// <param name="leafNodes">The list that contains the collected. leaf nodes.</param>
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
        private RCNumVector center;

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
        /// Reference to the neighbours of this node.
        /// </summary>
        private HashSet<PFTreeNode> neighboursCache;

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
