using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a connected area on the walkability grid. Each cell inside this area has the same walkability.
    /// </summary>
    class WalkabilityGridArea
    {
        /// <summary>
        /// Constructs a new WalkabilityGridArea explored from the given starting node.
        /// </summary>
        /// <param name="walkabilityGrid">The grid that contains the walkability informations.</param>
        /// <param name="startingNode">The node in the walkability quad-tree from which the area has to be explored.</param>
        /// TODO: remove maxError argument!
        public WalkabilityGridArea(IWalkabilityGrid walkabilityGrid, WalkabilityQuadTreeNode startingNode, RCNumber maxError)
        {
            if (walkabilityGrid == null) { throw new ArgumentNullException("walkabilityGrid"); }
            if (startingNode == null) { throw new ArgumentNullException("startingNode"); }
            if (!startingNode.IsLeafNode) { throw new ArgumentException("The starting node must be a leaf node in the walkability quad-tree!", "startingNode"); }

            this.children = new List<WalkabilityGridArea>();
            this.containedNodes = new RCSet<WalkabilityQuadTreeNode>();
            this.isWalkable = startingNode.IsWalkable;
            this.topLeftCell = startingNode.AreaOnGrid.Location;

            /// Visit the reachable nodes from the starting node and collect the candidates for the child areas.
            RCSet<WalkabilityQuadTreeNode> childAreaCandidates = new RCSet<WalkabilityQuadTreeNode>();
            RCSet<WalkabilityQuadTreeNode> nodesToVisit = new RCSet<WalkabilityQuadTreeNode>() { startingNode };
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

            /// Create the border of this area.
            if ((new RCIntRectangle(0, 0, walkabilityGrid.Width, walkabilityGrid.Height)).Contains(startingNode.AreaOnGrid))
            {
                this.CreateBorder(walkabilityGrid, maxError);
            }

            /// Create the child areas recursively.
            while (childAreaCandidates.Count > 0)
            {
                WalkabilityGridArea childArea = new WalkabilityGridArea(walkabilityGrid, childAreaCandidates.First(), maxError);
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
        /// Gets the vertices of the border of this area. The walkable part is on the right-hand side of the border.
        /// </summary>
        public List<RCNumVector> Border { get { return this.border; } }

        /// <summary>
        /// Gets the list of the child areas that are completely contained by this area.
        /// </summary>
        public IEnumerable<WalkabilityGridArea> Children { get { return this.children; } }

        #region Border polygon buildup methods

        /// <summary>
        /// Internal method for creating the border of this area.
        /// </summary>
        /// <param name="walkabilityGrid">The grid that contains the walkability informations.</param>
        /// TODO: remove maxError argument!
        private void CreateBorder(IWalkabilityGrid walkabilityGrid, RCNumber maxError)
        {
            if (walkabilityGrid == null) { throw new ArgumentNullException("grid"); }

            List<RCNumVector> vertices = new List<RCNumVector>();

            /// Initialize the search algorithm.
            RCIntVector currentPos = this.topLeftCell + new RCIntVector(-1, -1);
            StepDirection nextStep = StepDirection.None;
            StepDirection previousStep = StepDirection.None;
            if (walkabilityGrid[this.topLeftCell] && !walkabilityGrid[this.topLeftCell + new RCIntVector(0, -1)] && !walkabilityGrid[this.topLeftCell + new RCIntVector(-1, 0)])
            {
                if (WalkabilityGridArea.GetIndexAt(walkabilityGrid, currentPos) == 5) { previousStep = StepDirection.Up; }
            }
            else if (!(!walkabilityGrid[this.topLeftCell] && walkabilityGrid[this.topLeftCell + new RCIntVector(0, -1)] && walkabilityGrid[this.topLeftCell + new RCIntVector(-1, 0)] && walkabilityGrid[this.topLeftCell + new RCIntVector(-1, -1)]))
            {
                throw new ArgumentException("The given cell must be an upper left corner cell in the area being contoured!", "upperLeftCorner");
            }

            /// Make steps until we get back to the starting point.
            do
            {
                /// Evaluate our state, and set up our next direction
                nextStep = WalkabilityGridArea.Step(walkabilityGrid, vertices, currentPos, previousStep);

                if (nextStep == StepDirection.Up) { currentPos += new RCIntVector(0, -1); }
                else if (nextStep == StepDirection.Left) { currentPos += new RCIntVector(-1, 0); }
                else if (nextStep == StepDirection.Down) { currentPos += new RCIntVector(0, 1); }
                else if (nextStep == StepDirection.Right) { currentPos += new RCIntVector(1, 0); }
                previousStep = nextStep;

            } while (nextStep != StepDirection.None);

            /// Finished.
            this.border = vertices;
        }

        /// <summary>
        /// A simple enumeration to represent the direction we just moved, and the direction we will next move when executing
        /// the "Marching squares" algorithm.
        /// </summary>
        private enum StepDirection
        {
            None = 0,
            Up = 1,
            Left = 2,
            Down = 3,
            Right = 4
        }

        /// <summary>
        /// Makes the next step of the "Marching squares" algorithm from the current position.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="vertexList">The vertex list currently being built.</param>
        /// <param name="currentPos">The current position.</param>
        /// <param name="previousStep">The direction of the previous step.</param>
        /// <returns>The direction of the next step.</returns>
        private static StepDirection Step(IWalkabilityGrid grid, List<RCNumVector> vertexList, RCIntVector currentPos, StepDirection previousStep)
        {
            int currentIndex = WalkabilityGridArea.GetIndexAt(grid, currentPos);
            if (currentIndex == 1 || currentIndex == 7)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Left;
            }
            else if (currentIndex == 2 || currentIndex == 14)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Down;
            }
            else if (currentIndex == 3)
            {
                return StepDirection.Left;
            }
            else if (currentIndex == 4 || currentIndex == 13)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Right;
            }
            else if (currentIndex == 5)
            {
                if (previousStep == StepDirection.None || previousStep == StepDirection.Down)
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 0);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector(0, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Left;
                }
                else
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 1);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector(1, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Right;
                }
            }
            else if (currentIndex == 6)
            {
                return StepDirection.Down;
            }
            else if (currentIndex == 8 || currentIndex == 11)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Up;
            }
            else if (currentIndex == 9)
            {
                return StepDirection.Up;
            }
            else if (currentIndex == 10)
            {
                if (previousStep == StepDirection.None || previousStep == StepDirection.Right)
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector(0, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 1);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Down;
                }
                else
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector(1, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 0);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Up;
                }
            }
            else if (currentIndex == 12)
            {
                return StepDirection.Right;
            }
            else
            {
                throw new InvalidOperationException("Unexpected case!");
            }
        }

        /// <summary>
        /// Constructs a 4-bit integer of the given 2x2 square that takes its bits (starting from the LSB) from the walkability informations at the top-left,
        /// top-right, bottom-left and bottom-right corners of the square, respectively. The appropriate bit will be 0 if the corresponding corner is walkable;
        /// otherwise 1.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="position">The position of the top-left corner of the 2x2 square.</param>
        /// <returns>The index of the given square.</returns>
        private static int GetIndexAt(IWalkabilityGrid grid, RCIntVector position)
        {
            bool topLeft = !grid[position];
            bool topRight = !grid[position + new RCIntVector(1, 0)];
            bool bottomRight = !grid[position + new RCIntVector(1, 1)];
            bool bottomLeft = !grid[position + new RCIntVector(0, 1)];
            return (topLeft ? 0x08 : 0x00) | (topRight ? 0x04 : 0x00) | (bottomRight ? 0x02 : 0x00) | (bottomLeft ? 0x01 : 0x00);
        }

        #endregion Border polygon buildup methods

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
        /// The vertices of the border of this area. The walkable part is on the right-hand side of this border.
        /// </summary>
        private List<RCNumVector> border;

        /// <summary>
        /// The nodes of this area.
        /// </summary>
        private RCSet<WalkabilityQuadTreeNode> containedNodes;

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
