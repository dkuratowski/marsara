using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchProto
{
    public class NavMesh
    {
        public NavMesh(bool[][] nodeInfo)
        {
            this.nodes = new NavMeshNode[nodeInfo.Length][];
            this.HEIGHT = nodeInfo.Length;

            for (int i = 0; i < nodeInfo.Length; i++)
            {
                if (this.WIDTH == -1)
                {
                    this.WIDTH = nodeInfo[i].Length;
                    if (this.WIDTH == 0) { throw new Exception("Inconsistent map line!"); }
                }
                else
                {
                    if (this.WIDTH != nodeInfo[i].Length) { throw new Exception("Inconsistent map line!"); }
                }

                this.nodes[i] = new NavMeshNode[nodeInfo[i].Length];
                for (int j = 0; j < nodeInfo[i].Length; j++)
                {
                    if (nodeInfo[i][j])
                    {
                        this.nodes[i][j] = new NavMeshNode(j, i);
                        SetNeighbours(i, j);
                    }
                    else
                    {
                        this.nodes[i][j] = null;
                    }
                }
            }

            this.ExploreRegions();
        }

        public Tuple<List<Tuple<NavMeshNode, int>>, List<NavMeshNode>> FindRoute(NavMeshNode source, NavMeshNode target)
        {
            FindRouteContext ctx = new FindRouteContext(source, target);
            while (ctx.ProcessNextNode()) ;
            return new Tuple<List<Tuple<NavMeshNode, int>>, List<NavMeshNode>>(ctx.Route, ctx.TouchedNodes);
        }

        public NavMeshNode this[int x, int y]
        {
            get
            {
                return this.nodes[y][x];
            }
        }

        public static int Backward(int direction)
        {
            return (direction + NavMesh.BACKWARD_DIRECTION) % NavMesh.MAX_DIRECTION;
        }

        public int Width { get { return this.WIDTH; } }
        public int Height { get { return this.HEIGHT; } }

        private void SetNeighbours(int i, int j)
        {
            /// North
            if (i > 0) { this.nodes[i][j].SetNeighbour(this.nodes[i - 1][j], NORTH); }

            /// North-West
            if (i > 0 && j > 0) { this.nodes[i][j].SetNeighbour(this.nodes[i - 1][j - 1], NORTH_WEST); }

            /// North-East
            if (i > 0 && j + 1 < WIDTH) { this.nodes[i][j].SetNeighbour(this.nodes[i - 1][j + 1], NORTH_EAST); }

            /// West
            if (j > 0) { this.nodes[i][j].SetNeighbour(this.nodes[i][j - 1], WEST); }
        }

        private void ExploreRegions()
        {
            this.ExploreHorizontal();
        }

        private void ExploreHorizontal()
        {
            List<Tuple<int, int>> corridorsBefore = this.ExploreRow(0);
            List<Tuple<int, int>> corridorsCurrent = null;
            List<NavMeshRegion> regionsBefore = this.DistributeRow(new List<Tuple<int,int>>(), corridorsBefore, new List<NavMeshRegion>(), 0);
            List<NavMeshRegion> regionsCurrent = null;
            for (int row = 1; row < this.HEIGHT; row++)
            {
                corridorsCurrent = this.ExploreRow(row);
                regionsCurrent = this.DistributeRow(corridorsBefore, corridorsCurrent, regionsBefore, row);

                /// Swap references
                corridorsBefore = corridorsCurrent;
                regionsBefore = regionsCurrent;
            }
        }

        /// <summary>
        /// Distributes the given row to regions
        /// </summary>
        /// <param name="corridorsBefore">List of the corridors in the previous row from left to right.</param>
        /// <param name="corridorsCurrent">List of the corridors in the current row from left to right.</param>
        /// <param name="regionsBefore">List of the regions of the corridors in the previous row from left to right.</param>
        /// <param name="row">The index of the current row.</param>
        /// <returns>List of the regions of the corridors in the current row from left to right.</returns>
        private List<NavMeshRegion> DistributeRow(List<Tuple<int, int>> corridorsBefore,
                                                  List<Tuple<int, int>> corridorsCurrent,
                                                  List<NavMeshRegion> regionsBefore,
                                                  int row)
        {
            List<NavMeshRegion> regionsCurrent = new List<NavMeshRegion>();
            int idxBefore = 0;
            int idxCurrent = 0;
            Tuple<int, List<int>> fork = null;
            Tuple<int, List<int>> join = null;
            Tuple<int, int> connection = idxBefore < corridorsBefore.Count &&
                                         idxCurrent < corridorsCurrent.Count &&
                                         this.CheckIntersection(corridorsBefore[idxBefore], corridorsCurrent[idxCurrent]) ?
                                         new Tuple<int, int>(idxBefore, idxCurrent) :
                                         null;

            while (idxCurrent < corridorsCurrent.Count)
            {
                if (fork == null && join == null && connection == null)
                {
                    /// We are in NoConnection state
                    if (idxCurrent + 1 < corridorsCurrent.Count &&
                        idxBefore + 1 < corridorsBefore.Count &&
                        corridorsCurrent[idxCurrent + 1].Item1 < corridorsBefore[idxBefore + 1].Item1 ||
                        idxBefore + 1 >= corridorsBefore.Count)
                    {
                        /// idxCurrent has to be incremented
                        if (this.nodes[row][corridorsCurrent[idxCurrent].Item1].Region == null)
                        {
                            NavMeshRegion newRegion = new NavMeshRegion();
                            regionsCurrent.Add(newRegion);
                            this.AttachCorridorToRegion(newRegion, corridorsCurrent[idxCurrent], row);
                        }
                        idxCurrent++;
                    }
                    else
                    {
                        /// idxBefore has to be incremented
                        idxBefore++;
                    }

                    if (idxBefore < corridorsBefore.Count &&
                        idxCurrent < corridorsCurrent.Count &&
                        this.CheckIntersection(corridorsBefore[idxBefore], corridorsCurrent[idxCurrent]))
                    {
                        /// Next state is InConnection, otherwise NoConnection again
                        connection = new Tuple<int, int>(idxBefore, idxCurrent);
                    }
                }
                else if (fork == null && join == null && connection != null)
                {
                    /// We are in InConnection state
                    if (idxCurrent + 1 < corridorsCurrent.Count &&
                        idxBefore + 1 < corridorsBefore.Count &&
                        corridorsCurrent[idxCurrent + 1].Item1 < corridorsBefore[idxBefore + 1].Item1 ||
                        idxBefore + 1 >= corridorsBefore.Count)
                    {
                        /// idxCurrent has to be incremented
                        if (idxCurrent + 1 < corridorsCurrent.Count && this.CheckIntersection(corridorsBefore[idxBefore], corridorsCurrent[idxCurrent + 1]))
                        {
                            /// Next state is InFork
                            fork = new Tuple<int, List<int>>(connection.Item1, new List<int>() { connection.Item2 });
                            fork.Item2.Add(idxCurrent + 1);
                            connection = null;
                        }
                        else
                        {
                            /// Next state is NoConnection
                            regionsCurrent.Add(regionsBefore[idxBefore]);
                            this.AttachCorridorToRegion(regionsBefore[idxBefore], corridorsCurrent[idxCurrent], row);
                            connection = null;
                        }

                        idxCurrent++;
                    }
                    else
                    {
                        /// idxBefore has to be incremented
                        if (this.CheckIntersection(corridorsBefore[idxBefore + 1], corridorsCurrent[idxCurrent]))
                        {
                            /// Next state is InJoin
                            join = new Tuple<int, List<int>>(connection.Item2, new List<int>() { connection.Item1 });
                            join.Item2.Add(idxBefore + 1);
                            connection = null;
                        }
                        else
                        {
                            /// Next state is NoConnection
                            regionsCurrent.Add(regionsBefore[idxBefore]);
                            this.AttachCorridorToRegion(regionsBefore[idxBefore], corridorsCurrent[idxCurrent], row);
                            connection = null;
                        }

                        idxBefore++;
                    }
                }
                else if (fork != null && join == null && connection == null)
                {
                    /// We are in InFork state
                    if (idxCurrent + 1 < corridorsCurrent.Count && this.CheckIntersection(corridorsBefore[idxBefore], corridorsCurrent[idxCurrent + 1]))
                    {
                        /// Continue the fork
                        fork.Item2.Add(idxCurrent + 1);
                        idxCurrent++;
                        continue;
                    }

                    if (idxBefore + 1 < corridorsBefore.Count && this.CheckIntersection(corridorsBefore[idxBefore + 1], corridorsCurrent[idxCurrent]))
                    {
                        /// Finish the fork and start a join
                        for (int i = 0; i < fork.Item2.Count - 1; i++)
                        {
                            NavMeshRegion newRegion = new NavMeshRegion();
                            regionsCurrent.Add(newRegion);
                            this.AttachCorridorToRegion(newRegion, corridorsCurrent[fork.Item2[i]], row);
                        }
                        join = new Tuple<int, List<int>>(idxCurrent, new List<int>() { fork.Item1 });
                        fork = null;
                        idxBefore++;
                        continue;
                    }

                    if (idxCurrent + 1 < corridorsCurrent.Count &&
                        idxBefore + 1 < corridorsBefore.Count &&
                        corridorsCurrent[idxCurrent + 1].Item1 < corridorsBefore[idxBefore + 1].Item1 ||
                        idxBefore + 1 >= corridorsBefore.Count)
                    {
                        /// idxCurrent has to be incremented
                        idxCurrent++;
                    }
                    else
                    {
                        /// idxBefore has to be incremented
                        idxBefore++;
                    }

                    /// Finish the fork
                    for (int i = 0; i < fork.Item2.Count; i++)
                    {
                        NavMeshRegion newRegion = new NavMeshRegion();
                        regionsCurrent.Add(newRegion);
                        this.AttachCorridorToRegion(newRegion, corridorsCurrent[fork.Item2[i]], row);
                    }
                    fork = null;
                }
                else if (fork == null && join != null && connection == null)
                {
                    /// We are in InJoin state
                    if (idxBefore + 1 < corridorsBefore.Count && this.CheckIntersection(corridorsBefore[idxBefore + 1], corridorsCurrent[idxCurrent]))
                    {
                        /// Continue the join
                        join.Item2.Add(idxBefore + 1);
                        idxBefore++;
                        continue;
                    }

                    if (idxCurrent + 1 < corridorsCurrent.Count && this.CheckIntersection(corridorsBefore[idxBefore], corridorsCurrent[idxCurrent + 1]))
                    {
                        /// Finish the join and start a fork
                        fork = new Tuple<int, List<int>>(idxBefore, new List<int>() { join.Item1 });
                        join = null;
                        idxCurrent++;
                        continue;
                    }

                    if (idxCurrent + 1 < corridorsCurrent.Count &&
                        idxBefore + 1 < corridorsBefore.Count &&
                        corridorsCurrent[idxCurrent + 1].Item1 < corridorsBefore[idxBefore + 1].Item1 ||
                        idxBefore + 1 >= corridorsBefore.Count)
                    {
                        /// idxCurrent has to be incremented
                        idxCurrent++;
                    }
                    else
                    {
                        /// idxBefore has to be incremented
                        idxBefore++;
                    }

                    /// Finish the join
                    NavMeshRegion newRegion = new NavMeshRegion();
                    regionsCurrent.Add(newRegion);
                    this.AttachCorridorToRegion(newRegion, corridorsCurrent[join.Item1], row);
                    join = null;
                }
                else
                {
                    throw new Exception("Unexpected state!");
                }
            }

            return regionsCurrent;
        }

        /// <summary>
        /// Explores the given row.
        /// </summary>
        /// <param name="row">The index of the row to explore.</param>
        private List<Tuple<int, int>> ExploreRow(int row)
        {
            List<Tuple<int, int>> corridorList = new List<Tuple<int, int>>();
            int currFirst = -1;
            int currLength = 0;

            for (int i = 0; i < this.WIDTH; i++)
            {
                if (currFirst != -1)
                {
                    /// We are inside a corridor
                    if (this.nodes[row][i] == null)
                    {
                        /// Leave the corridor
                        corridorList.Add(new Tuple<int, int>(currFirst, currFirst + currLength - 1));
                        currFirst = -1;
                    }
                    else
                    {
                        /// Stay in the corridor
                        currLength++;
                    }
                }
                else if (this.nodes[row][i] != null)
                {
                    /// Enter the corridor
                    currFirst = i;
                    currLength = 1;
                }
            }

            /// Save the last corridor
            if (currFirst != -1)
            {
                corridorList.Add(new Tuple<int, int>(currFirst, currFirst + currLength - 1));
            }
            return corridorList;
        }

        private void AttachCorridorToRegion(NavMeshRegion toRegion, Tuple<int, int> corridor, int row)
        {
            for (int i = corridor.Item1; i <= corridor.Item2; i++)
            {
                this.nodes[row][i].AttachToRegion(toRegion);
            }
        }

        private bool CheckIntersection(Tuple<int, int> first, Tuple<int, int> second)
        {
            return !(first.Item2 < second.Item1 || second.Item2 < first.Item1);
        }

        public const int NORTH = 0;
        public const int NORTH_EAST = 1;
        public const int EAST = 2;
        public const int SOUTH_EAST = 3;
        public const int SOUTH = 4;
        public const int SOUTH_WEST = 5;
        public const int WEST = 6;
        public const int NORTH_WEST = 7;

        public const int MAX_DIRECTION = 8;
        public const int BACKWARD_DIRECTION = 4;

        public const int SIDE_LENGTH = 2;
        public const int DIAGONAL_LENGTH = 3;

        private NavMeshNode[][] nodes;

        private int WIDTH = -1;
        private int HEIGHT = -1;
    }
}
