using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents one node in the quad-tree over the walkability grid.
    /// </summary>
    class WalkabilityQuadTreeNode
    {
        /// <summary>
        /// Constructs a quad-tree from the given walkability grid.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <returns>The root node of the created quad-tree.</returns>
        public static WalkabilityQuadTreeNode CreateQuadTree(IWalkabilityGrid grid)
        {
            if (grid == null) { throw new ArgumentNullException("grid"); }

            /// Find the number of subdivision levels.
            int boundingBoxSize = Math.Max(grid.Width, grid.Height);
            int subdivisionLevels = 1;
            while (boundingBoxSize > (int)Math.Pow(2, subdivisionLevels))
            {
                subdivisionLevels++;
                if (subdivisionLevels > MAX_LEVELS) { throw new ArgumentException("Grid size is too big!", "grid"); }
            }

            /// Create the root node of the quad-tree.
            WalkabilityQuadTreeNode rootNode = new WalkabilityQuadTreeNode(subdivisionLevels);

            /// Add the obstacles to the quad-tree.
            WalkabilityQuadTreeNode relevantNode = rootNode.children[NORTHWEST_CHILD_IDX].children[SOUTHEAST_CHILD_IDX];
            for (int row = 0; row < relevantNode.areaOnGrid.Height; row++)
            {
                for (int column = 0; column < relevantNode.areaOnGrid.Width; column++)
                {
                    if (row >= grid.Height || column >= grid.Width)
                    {
                        /// Everything out of the grid range is considered to be obstacle.
                        relevantNode.AddObstacle(new RCIntVector(column, row));
                    }
                    else
                    {
                        /// Add obstacle depending on the walkability of the cell.
                        if (!grid[new RCIntVector(column, row)])
                        {
                            relevantNode.AddObstacle(new RCIntVector(column, row));
                        }
                    }
                }
            }

            /// Search the neighbours of the leaf nodes.
            RCSet<WalkabilityQuadTreeNode> leafNodes = new RCSet<WalkabilityQuadTreeNode>();
            rootNode.CollectLeafNodes(leafNodes);
            foreach (WalkabilityQuadTreeNode leafNode in leafNodes)
            {
                leafNode.SetNeighbours();
            }

            return rootNode;
        }

        /// <summary>
        /// Gets the neighbours of this node.
        /// </summary>
        public IEnumerable<WalkabilityQuadTreeNode> Neighbours
        {
            get
            {
                if (this.walkability == Walkability.Mixed) { throw new InvalidOperationException("Non leaf node!"); }
                if (this.neighbours == null) { throw new InvalidOperationException("Neighbours has not yet been set!"); }
                return this.neighbours;
            }
        }

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
        /// Gets the area on the walkability grid covered by this node.
        /// </summary>
        public RCIntRectangle AreaOnGrid { get { return this.areaOnGrid; } }

        /// <summary>
        /// Gets whether this node is a leaf node or not.
        /// </summary>
        public bool IsLeafNode { get { return this.walkability != Walkability.Mixed; } }

        /// <summary>
        /// Gets whether this node has already been visited or not.
        /// </summary>
        public bool HasBeenVisited { get { return this.hasBeenVisited; } }

        /// <summary>
        /// Gets the leaf node that contains the given cell.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the cell.</param>
        /// <returns>The leaf node that contains the given cell.</returns>
        public WalkabilityQuadTreeNode GetLeafNode(RCIntVector cellCoords)
        {
            if (!this.areaOnGrid.Contains(cellCoords)) { throw new ArgumentOutOfRangeException("cellCoords"); }

            if (this.walkability != Walkability.Mixed) { return this; }

            if (this.children[NORTHWEST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { return this.children[NORTHWEST_CHILD_IDX].GetLeafNode(cellCoords); }
            else if (this.children[NORTHEAST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { return this.children[NORTHEAST_CHILD_IDX].GetLeafNode(cellCoords); }
            else if (this.children[SOUTHEAST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { return this.children[SOUTHEAST_CHILD_IDX].GetLeafNode(cellCoords); }
            else if (this.children[SOUTHWEST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { return this.children[SOUTHWEST_CHILD_IDX].GetLeafNode(cellCoords); }

            /// Error case.
            return null;
        }

        /// <summary>
        /// Visits this node.
        /// </summary>
        public void SetVisited()
        {
            if (!this.IsLeafNode) { throw new InvalidOperationException("Only leaf nodes can be visited!"); }
            if (this.hasBeenVisited) { throw new InvalidOperationException("The node has already been visited!"); }
            this.hasBeenVisited = true;
        }

        #region Private methods

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
        /// Constructs the root node of the quad-tree.
        /// </summary>
        /// <param name="subdivisionLevels">Number of subdivision levels.</param>
        private WalkabilityQuadTreeNode(int subdivisionLevels)
        {
            if (subdivisionLevels <= 0) { throw new ArgumentOutOfRangeException("subdivisionLevels", "Number of subdivision levels must be greater than 0!"); }
            if (subdivisionLevels > MAX_LEVELS) { throw new ArgumentOutOfRangeException("subdivisionLevels", string.Format("Number of subdivision levels must be less than {0}!", MAX_LEVELS)); }

            RCIntRectangle relevantAreaOnGrid = new RCIntRectangle(0, 0, (int)Math.Pow(2, subdivisionLevels), (int)Math.Pow(2, subdivisionLevels));
            this.areaOnGrid = new RCIntRectangle(-relevantAreaOnGrid.Width, -relevantAreaOnGrid.Height, 4 * relevantAreaOnGrid.Width, 4 * relevantAreaOnGrid.Height);
            this.walkability = Walkability.Mixed;
            this.parent = null;
            this.root = this;
            this.children = new WalkabilityQuadTreeNode[4]
            {
                new WalkabilityQuadTreeNode(new RCIntRectangle(relevantAreaOnGrid.Location + new RCIntVector(-relevantAreaOnGrid.Width, -relevantAreaOnGrid.Height), 2 * relevantAreaOnGrid.Size), this, false),
                new WalkabilityQuadTreeNode(new RCIntRectangle(relevantAreaOnGrid.Location + new RCIntVector(relevantAreaOnGrid.Width, -relevantAreaOnGrid.Height), 2 * relevantAreaOnGrid.Size), this, false),
                new WalkabilityQuadTreeNode(new RCIntRectangle(relevantAreaOnGrid.Location + new RCIntVector(relevantAreaOnGrid.Width, relevantAreaOnGrid.Height), 2 * relevantAreaOnGrid.Size), this, false),
                new WalkabilityQuadTreeNode(new RCIntRectangle(relevantAreaOnGrid.Location + new RCIntVector(-relevantAreaOnGrid.Width, relevantAreaOnGrid.Height), 2 * relevantAreaOnGrid.Size), this, false)
            };
            this.children[NORTHWEST_CHILD_IDX].walkability = Walkability.Mixed;
            this.children[NORTHWEST_CHILD_IDX].children = new WalkabilityQuadTreeNode[4]
            {
                new WalkabilityQuadTreeNode(relevantAreaOnGrid + new RCIntVector(-relevantAreaOnGrid.Width, -relevantAreaOnGrid.Height), this.children[NORTHWEST_CHILD_IDX], false),
                new WalkabilityQuadTreeNode(relevantAreaOnGrid + new RCIntVector(0, -relevantAreaOnGrid.Height), this.children[NORTHWEST_CHILD_IDX], false),
                new WalkabilityQuadTreeNode(relevantAreaOnGrid, this.children[NORTHWEST_CHILD_IDX], true),
                new WalkabilityQuadTreeNode(relevantAreaOnGrid + new RCIntVector(-relevantAreaOnGrid.Width, 0), this.children[NORTHWEST_CHILD_IDX], false)
            };
        }
        
        /// <summary>
        /// Internal ctor to create the internal nodes of the quad-tree.
        /// </summary>
        /// <param name="areaOnGrid">The area on the walkability grid covered by the node.</param>
        /// <param name="parent">The parent of the node.</param>
        /// <param name="isWalkable">True if the node is walkable; otherwise false.</param>
        private WalkabilityQuadTreeNode(RCIntRectangle areaOnGrid, WalkabilityQuadTreeNode parent, bool isWalkable)
        {
            this.areaOnGrid = areaOnGrid;
            this.walkability = isWalkable ? Walkability.Walkable : Walkability.NonWalkable;
            this.children = new WalkabilityQuadTreeNode[4];
            this.parent = parent;
            this.root = parent.root;
        }

        /// <summary>
        /// Collects all the leaf nodes from the quad-tree that are below this node.
        /// </summary>
        /// <param name="leafNodes">The list that contains the collected leaf nodes.</param>
        internal void CollectLeafNodes(RCSet<WalkabilityQuadTreeNode> leafNodes)
        {
            if (this.walkability != Walkability.Mixed)
            {
                /// Leaf node found -> end of recursion.
                leafNodes.Add(this);
                return;
            }

            /// Call this method recursively on all children.
            foreach (WalkabilityQuadTreeNode child in this.children)
            {
                child.CollectLeafNodes(leafNodes);
            }
        }

        /// <summary>
        /// Adds an obstacle to the given cell.
        /// </summary>
        /// <param name="cellCoords">The coordinates of the obstacle cell.</param>
        private void AddObstacle(RCIntVector cellCoords)
        {
            if (!this.areaOnGrid.Contains(cellCoords)) { throw new ArgumentOutOfRangeException("cellCoords"); }

            if (this.areaOnGrid.Width == 1)
            {
                /// End of recursion.
                this.walkability = Walkability.NonWalkable;
                return;
            }

            if (this.walkability == Walkability.Walkable)
            {
                /// Subdivide the node.
                this.Subdivide();
                if (this.children[NORTHWEST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { this.children[NORTHWEST_CHILD_IDX].AddObstacle(cellCoords); }
                else if (this.children[NORTHEAST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { this.children[NORTHEAST_CHILD_IDX].AddObstacle(cellCoords); }
                else if (this.children[SOUTHEAST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { this.children[SOUTHEAST_CHILD_IDX].AddObstacle(cellCoords); }
                else if (this.children[SOUTHWEST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { this.children[SOUTHWEST_CHILD_IDX].AddObstacle(cellCoords); }
                this.walkability = Walkability.Mixed;
            }
            else if (this.walkability == Walkability.Mixed)
            {
                /// Merge the node if necessary.
                int childIdx = -1;
                if (this.children[NORTHWEST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { childIdx = NORTHWEST_CHILD_IDX; }
                else if (this.children[NORTHEAST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { childIdx = NORTHEAST_CHILD_IDX; }
                else if (this.children[SOUTHEAST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { childIdx = SOUTHEAST_CHILD_IDX; }
                else if (this.children[SOUTHWEST_CHILD_IDX].areaOnGrid.Contains(cellCoords)) { childIdx = SOUTHWEST_CHILD_IDX; }

                this.children[childIdx].AddObstacle(cellCoords);
                if (this.children[NORTHWEST_CHILD_IDX].walkability == Walkability.NonWalkable &&
                    this.children[NORTHEAST_CHILD_IDX].walkability == Walkability.NonWalkable &&
                    this.children[SOUTHEAST_CHILD_IDX].walkability == Walkability.NonWalkable &&
                    this.children[SOUTHWEST_CHILD_IDX].walkability == Walkability.NonWalkable)
                {
                    this.children = new WalkabilityQuadTreeNode[4];
                    this.walkability = Walkability.NonWalkable;
                }
            }
        }

        /// <summary>
        /// Subdivides this node.
        /// </summary>
        private void Subdivide()
        {
            if (this.walkability != Walkability.NonWalkable && this.walkability != Walkability.Walkable) { throw new InvalidOperationException("Unable to subdivide a node with mixed walkability!"); }
            this.children[NORTHWEST_CHILD_IDX] = new WalkabilityQuadTreeNode(new RCIntRectangle(this.areaOnGrid.X, this.areaOnGrid.Y, this.areaOnGrid.Width / 2, this.areaOnGrid.Height / 2), this, this.walkability == Walkability.Walkable);
            this.children[NORTHEAST_CHILD_IDX] = new WalkabilityQuadTreeNode(new RCIntRectangle(this.areaOnGrid.X + this.areaOnGrid.Width / 2, this.areaOnGrid.Y, this.areaOnGrid.Width / 2, this.areaOnGrid.Height / 2), this, this.walkability == Walkability.Walkable);
            this.children[SOUTHEAST_CHILD_IDX] = new WalkabilityQuadTreeNode(new RCIntRectangle(this.areaOnGrid.X + this.areaOnGrid.Width / 2, this.areaOnGrid.Y + this.areaOnGrid.Height / 2, this.areaOnGrid.Width / 2, this.areaOnGrid.Height / 2), this, this.walkability == Walkability.Walkable);
            this.children[SOUTHWEST_CHILD_IDX] = new WalkabilityQuadTreeNode(new RCIntRectangle(this.areaOnGrid.X, this.areaOnGrid.Y + this.areaOnGrid.Height / 2, this.areaOnGrid.Width / 2, this.areaOnGrid.Height / 2), this, this.walkability == Walkability.Walkable);
        }

        /// <summary>
        /// Searches the neighbours of this node.
        /// </summary>
        private void SetNeighbours()
        {
            this.neighbours = new RCSet<WalkabilityQuadTreeNode>();

            /// Search above this node if necessary.
            if (this.areaOnGrid.Top != this.root.areaOnGrid.Top)
            {
                for (int col = this.areaOnGrid.Left; col < this.areaOnGrid.Right && col < this.root.areaOnGrid.Right; )
                {
                    WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(col, this.areaOnGrid.Top - 1));
                    this.neighbours.Add(neighbour);
                    col = neighbour.areaOnGrid.Right;
                }
            }

            /// Search on the right side of this node if necessary.
            if (this.areaOnGrid.Right != this.root.areaOnGrid.Right)
            {
                for (int row = this.areaOnGrid.Top; row < this.areaOnGrid.Bottom && row < this.root.areaOnGrid.Bottom; )
                {
                    WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnGrid.Right, row));
                    this.neighbours.Add(neighbour);
                    row = neighbour.areaOnGrid.Bottom;
                }
            }

            /// Search below this node if necessary.
            if (this.areaOnGrid.Bottom != this.root.areaOnGrid.Bottom)
            {
                for (int col = this.areaOnGrid.Right - 1; col >= this.areaOnGrid.Left && col >= this.root.areaOnGrid.Left; )
                {
                    WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(col, this.areaOnGrid.Bottom));
                    this.neighbours.Add(neighbour);
                    col = neighbour.areaOnGrid.Left - 1;
                }
            }

            /// Search on the left side of this node if necessary.
            if (this.areaOnGrid.Left != this.root.areaOnGrid.Left)
            {
                for (int row = this.areaOnGrid.Bottom - 1; row >= this.areaOnGrid.Top && row >= this.root.areaOnGrid.Top; )
                {
                    WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnGrid.Left - 1, row));
                    this.neighbours.Add(neighbour);
                    row = neighbour.areaOnGrid.Top - 1;
                }
            }

            /// Search in diagonal directions if necessary.
            if (this.areaOnGrid.Left != this.root.areaOnGrid.Left && this.areaOnGrid.Top != this.root.areaOnGrid.Top)
            {
                WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnGrid.Left - 1, this.areaOnGrid.Top - 1));
                this.neighbours.Add(neighbour);
            }
            if (this.areaOnGrid.Right != this.root.areaOnGrid.Right && this.areaOnGrid.Top != this.root.areaOnGrid.Top)
            {
                WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnGrid.Right, this.areaOnGrid.Top - 1));
                this.neighbours.Add(neighbour);
            }
            if (this.areaOnGrid.Right != this.root.areaOnGrid.Right && this.areaOnGrid.Bottom != this.root.areaOnGrid.Bottom)
            {
                WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnGrid.Right, this.areaOnGrid.Bottom));
                this.neighbours.Add(neighbour);
            }
            if (this.areaOnGrid.Left != this.root.areaOnGrid.Left && this.areaOnGrid.Bottom != this.root.areaOnGrid.Bottom)
            {
                WalkabilityQuadTreeNode neighbour = this.root.GetLeafNode(new RCIntVector(this.areaOnGrid.Left - 1, this.areaOnGrid.Bottom));
                this.neighbours.Add(neighbour);
            }
        }

        #endregion Private methods

        #region Private fields

        /// <summary>
        /// The area on the grid covered by this node.
        /// </summary>
        private RCIntRectangle areaOnGrid;

        /// <summary>
        /// The walkability property of this node.
        /// </summary>
        private Walkability walkability;

        /// <summary>
        /// Reference to the 4 child nodes (order: NW, NE, SE, SW).
        /// </summary>
        private WalkabilityQuadTreeNode[] children;

        /// <summary>
        /// Reference to the parent node.
        /// </summary>
        private WalkabilityQuadTreeNode parent;

        /// <summary>
        /// Reference to the root node.
        /// </summary>
        private WalkabilityQuadTreeNode root;

        /// <summary>
        /// Reference to the neighbours of this node.
        /// </summary>
        private RCSet<WalkabilityQuadTreeNode> neighbours;

        /// <summary>
        /// This flag indicates whether this node has already been visited or not.
        /// </summary>
        private bool hasBeenVisited;

        /// <summary>
        /// The indices of the child nodes depending on their location.
        /// </summary>
        private const int NORTHWEST_CHILD_IDX = 0;
        private const int NORTHEAST_CHILD_IDX = 1;
        private const int SOUTHEAST_CHILD_IDX = 2;
        private const int SOUTHWEST_CHILD_IDX = 3;

        /// <summary>
        /// The maximum number of subdivision levels in a walkability quad-tree.
        /// </summary>
        private const int MAX_LEVELS = 16;

        #endregion Private fields
    }
}
