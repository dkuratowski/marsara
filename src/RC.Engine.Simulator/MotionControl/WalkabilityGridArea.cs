using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a connected area on the walkability grid. Each cell inside this area has the same walkability.
    /// </summary>
    /// TODO: THE BUILD-UP ALGORITHM IS SLOW!!! NEEDS TO BE ENHANCED WITH A SEARCH ON A QUADTREE-LIKE STRUCTURE (SIMILAR TO PFTreeNode)!!!
    class WalkabilityGridArea
    {
        /// <summary>
        /// Constructs a new WalkabilityGridArea explored from the given starting node.
        /// </summary>
        /// <param name="startingNode">The node in the walkability quad-tree from which the area has to be explored.</param>
        public WalkabilityGridArea(WalkabilityQuadTreeNode startingNode)
        {
            if (startingNode == null) { throw new ArgumentNullException("startingNode"); }
            if (!startingNode.IsLeafNode) { throw new ArgumentException("The starting node must be a leaf node in the walkability quad-tree!", "startingNode"); }

            this.children = new List<WalkabilityGridArea>();
            this.containedNodes = new HashSet<WalkabilityQuadTreeNode>();
            this.isWalkable = startingNode.IsWalkable;
            this.topLeftCell = startingNode.AreaOnGrid.Location;

            HashSet<WalkabilityQuadTreeNode> childAreaCandidates = new HashSet<WalkabilityQuadTreeNode>();
            HashSet<WalkabilityQuadTreeNode> nodesToVisit = new HashSet<WalkabilityQuadTreeNode>() { startingNode };

            while (nodesToVisit.Count > 0)
            {
                WalkabilityQuadTreeNode currNode = nodesToVisit.First();
                nodesToVisit.Remove(currNode);

                if (currNode.AreaOnGrid.Y < this.topLeftCell.Y || currNode.AreaOnGrid.Y == this.topLeftCell.Y && currNode.AreaOnGrid.X < this.topLeftCell.X)
                {
                    this.topLeftCell = currNode.AreaOnGrid.Location;
                }

                List<WalkabilityQuadTreeNode> neighbours = WalkabilityGridArea.GetNeighbours(currNode);
                foreach (WalkabilityQuadTreeNode neighbour in neighbours)
                {
                    if (neighbour.IsWalkable == this.isWalkable)
                    {
                        if (!neighbour.HasBeenVisited && !nodesToVisit.Contains(neighbour)) { nodesToVisit.Add(neighbour); }
                    }
                    else if (!neighbour.HasBeenVisited)
                    {
                        /// Neighbour is a cell from another area? Let's see later.
                        childAreaCandidates.Add(neighbour);
                    }
                }

                currNode.SetVisited();
                this.containedNodes.Add(currNode);
            }

            /// Create the child areas recursively.
            while (childAreaCandidates.Count > 0)
            {
                WalkabilityGridArea childArea = new WalkabilityGridArea(childAreaCandidates.First());
                this.children.Add(childArea);
                foreach (WalkabilityQuadTreeNode node in childArea.containedNodes) { childAreaCandidates.Remove(node); }
            }
        }

        /// <summary>
        /// Gets whether this area is walkable, false otherwise.
        /// </summary>
        public bool IsWalkable { get { return this.isWalkable; } }

        /// <summary>
        /// Gets the coordinates of the leftmost cell in the topmost row of this area.
        /// </summary>
        public RCIntVector TopLeftCell { get { return this.topLeftCell; } }

        /// <summary>
        /// Gets the list of the child areas that are completely contained by this area.
        /// </summary>
        public IEnumerable<WalkabilityGridArea> Children { get { return this.children; } }

        /// <summary>
        /// Gets the neighbours or the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The neighbours or the given node.</returns>
        private static List<WalkabilityQuadTreeNode> GetNeighbours(WalkabilityQuadTreeNode node)
        {
            List<WalkabilityQuadTreeNode> neighbourList = new List<WalkabilityQuadTreeNode>();
            foreach (WalkabilityQuadTreeNode neighbour in node.Neighbours)
            {
                /// In case of walkable nodes we filter out the diagonal neighbours.
                if (!node.IsWalkable ||
                    !(neighbour.AreaOnGrid.Right == node.AreaOnGrid.Left && neighbour.AreaOnGrid.Bottom == node.AreaOnGrid.Top ||
                      neighbour.AreaOnGrid.Left == node.AreaOnGrid.Right && neighbour.AreaOnGrid.Bottom == node.AreaOnGrid.Top ||
                      neighbour.AreaOnGrid.Left == node.AreaOnGrid.Right && neighbour.AreaOnGrid.Top == node.AreaOnGrid.Bottom ||
                      neighbour.AreaOnGrid.Right == node.AreaOnGrid.Left && neighbour.AreaOnGrid.Top == node.AreaOnGrid.Bottom))
                {
                    neighbourList.Add(neighbour);
                }
            }
            return neighbourList;
        }

        /// <summary>
        /// The coordinates of the leftmost cell in the topmost row of this area.
        /// </summary>
        private RCIntVector topLeftCell;

        /// <summary>
        /// The nodes of this area.
        /// </summary>
        private HashSet<WalkabilityQuadTreeNode> containedNodes;

        /// <summary>
        /// List of the child areas that are completely contained by this area.
        /// </summary>
        private List<WalkabilityGridArea> children;

        /// <summary>
        /// True if this area is walkable, false otherwise.
        /// </summary>
        private bool isWalkable;
    }
}
