using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchProto
{
    public class NavMeshNode
    {
        public NavMeshNode(int x, int y)
        {
            this.neighbours = new NavMeshNode[NavMesh.MAX_DIRECTION];
            this.coordX = x;
            this.coordY = y;
            this.containingRegion = null;
        }

        public void SetNeighbour(NavMeshNode otherNode, int direction)
        {
            direction %= NavMesh.MAX_DIRECTION;

            NavMeshNode oldNeighbour = this.neighbours[direction];
            if (oldNeighbour != null)
            {
                oldNeighbour.neighbours[NavMesh.Backward(direction)] = null;
            }

            this.neighbours[direction] = otherNode;

            if (otherNode != null)
            {
                NavMeshNode otherNodeOldNeighbour = otherNode.neighbours[NavMesh.Backward(direction)];
                if (otherNodeOldNeighbour != null)
                {
                    otherNodeOldNeighbour.neighbours[direction] = null;
                }
                otherNode.neighbours[NavMesh.Backward(direction)] = this;
            }
        }

        public NavMeshNode this[int dir]
        {
            get
            {
                return this.neighbours[dir];
            }
        }

        public int ComputeHeuristic(NavMeshNode target)
        {
            int horz = Math.Abs(this.coordX - target.coordX);
            int vert = Math.Abs(this.coordY - target.coordY);
            int diff = Math.Abs(horz - vert);
            return Math.Min(horz, vert) * NavMesh.DIAGONAL_LENGTH + diff * NavMesh.SIDE_LENGTH;
        }

        public void AttachToRegion(NavMeshRegion region)
        {
            this.containingRegion = region;
        }

        public int X { get { return this.coordX; } }
        public int Y { get { return this.coordY; } }

        public NavMeshRegion Region { get { return this.containingRegion; } }

        private NavMeshNode[] neighbours;
        private NavMeshRegion containingRegion;

        private int coordX;
        private int coordY;
    }
}
