using RC.Common;
using RC.Engine.Pathfinder.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents the pathfinding grid.
    /// </summary>
    class Grid
    {
        /// <summary>
        /// Constructs a Grid instance from the given walkability informations.
        /// </summary>
        /// <param name="walkabilityReader">Reference to the object that provides the walkability informations.</param>
        /// <param name="maxMovingSize">The maximum size of moving agents.</param>
        public Grid(IWalkabilityReader walkabilityReader, int maxMovingSize)
        {
            this.agents = new RCSet<Agent>();
            this.pathfindingAlgorithms = new RCSet<IPathfindingAlgorithm>();
            this.cells = new Cell[walkabilityReader.Width, walkabilityReader.Height];
            this.width = walkabilityReader.Width;
            this.height = walkabilityReader.Height;
            this.maxMovingSize = maxMovingSize;
            this.obstacleEnvironment = new ObstacleEnvironment(this.maxMovingSize);

            /// Create the array of sectors
            int horzSectorCount = this.Width / Sector.SECTOR_SIZE;
            int vertSectorCount = this.Height / Sector.SECTOR_SIZE;
            if (this.Width % Sector.SECTOR_SIZE > 0) { horzSectorCount++; }
            if (this.Height % Sector.SECTOR_SIZE > 0) { vertSectorCount++; }
            Sector[,] sectors = new Sector[horzSectorCount, vertSectorCount];

            /// Create the cells.
            for (int row = 0; row < this.height; row++)
            {
                for (int column = 0; column < this.width; column++)
                {
                    /// Get the sector of the cell to be created of create the sector if not yet been created.
                    RCIntVector sectorIndex = new RCIntVector(column / Sector.SECTOR_SIZE, row / Sector.SECTOR_SIZE);
                    Sector sectorOfCell = sectors[sectorIndex.X, sectorIndex.Y];
                    if (sectorOfCell == null)
                    {
                        RCIntRectangle sectorArea = new RCIntRectangle(sectorIndex.X * Sector.SECTOR_SIZE, sectorIndex.Y * Sector.SECTOR_SIZE, Sector.SECTOR_SIZE, Sector.SECTOR_SIZE);
                        sectorOfCell = new Sector(sectorArea, this);
                        sectors[sectorIndex.X, sectorIndex.Y] = sectorOfCell;
                    }

                    /// Create the cell and add it to its sector.
                    this.cells[column, row] = new Cell(new RCIntVector(column, row), walkabilityReader[column, row], this, sectorOfCell);
                }
            }

            /// Calculate the intial subdivisions of the sectors.
            for (int sectorIndexX = 0; sectorIndexX < horzSectorCount; sectorIndexX++)
            {
                for (int sectorIndexY = 0; sectorIndexY < vertSectorCount; sectorIndexY++)
                {
                    sectors[sectorIndexX, sectorIndexY].CreateInitialSubdivisions();
                }
            }
        }

        /// <summary>
        /// Gets the maximum size of moving agents.
        /// </summary>
        public int MaxMovingSize { get { return this.maxMovingSize; } }

        /// <summary>
        /// Gets the width of this grid.
        /// </summary>
        public int Width { get { return this.width; } }

        /// <summary>
        /// Gets the height of this grid.
        /// </summary>
        public int Height { get { return this.height; } }

        /// <summary>
        /// Gets the environment information of rectangular obstacles.
        /// </summary>
        public ObstacleEnvironment ObstacleEnvironment { get { return this.obstacleEnvironment; } }

        /// <summary>
        /// Gets the cell of this grid at the given coordinates.
        /// </summary>
        /// <param name="x">The X-coordinate of the cell.</param>
        /// <param name="y">The Y-coordinate of the cell.</param>
        /// <returns>The cell at the given coordinates or null if the given coordinates are outside of this grid.</returns>
        public Cell this[int x, int y] { get { return x >= 0 && x < this.width && y >= 0 && y < this.height ? this.cells[x, y] : null; } }

        /// <summary>
        /// Creates and places an agent with the given area to this grid.
        /// </summary>
        /// <param name="area">The rectangular area of the agent.</param>
        /// <param name="client">The client of the placed agent that will provide additional informations for the pathfinder component.</param>
        /// <returns>A reference to the agent or null if the placement failed.</returns>
        public Agent CreateAgent(RCIntRectangle area, IAgentClient client)
        {
            Agent newAgent = new Agent(area, this, client);
            RCSet<Agent> collidingAgents = null;
            if (this.ValidatePositionForAgent(newAgent, this[newAgent.Area.X, newAgent.Area.Y], out collidingAgents))
            {
                this.PlaceAgentOnGrid(newAgent);
                this.agents.Add(newAgent);
                newAgent.MovingStatusChanged += this.OnAgentMovingStatusChanged;
                return newAgent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Steps the given agent towards the given direction.
        /// </summary>
        /// <param name="agent">The agent to be stepped.</param>
        /// <param name="stepDirection">The direction of the step.</param>
        /// <param name="collidingAgents">The output list of colliding agents if the step failed; otherwise an empty set.</param>
        /// <returns>True if the agent has been stepped successfully; otherwise false.</returns>
        public bool StepAgent(Agent agent, int stepDirection, out RCSet<Agent> collidingAgents)
        {
            if (agent == null) { throw new ArgumentNullException("agent"); }
            if (!this.agents.Contains(agent)) { throw new InvalidOperationException("The given agent is not placed on the pathfinding grid!"); }
            if (agent.MovingStatus != AgentMovingStatusEnum.Moving) { throw new InvalidOperationException("The moving status of the given agent is not Moving!"); }
            if (stepDirection < 0 || stepDirection >= GridDirections.DIRECTION_COUNT) { throw new ArgumentOutOfRangeException("stepDirection"); }

            /// Validate the desired new top-left cell of the agent and collect the colliding agents.
            Cell desiredNewTopLeftCell = this[agent.Area.X, agent.Area.Y].GetNeighbour(stepDirection);
            if (!this.ValidatePositionForAgent(agent, desiredNewTopLeftCell, out collidingAgents)) { return false; }

            /// Step the agent towards the given direction.
            this.RemoveAgentFromGrid(agent);
            agent.Step(stepDirection);
            this.PlaceAgentOnGrid(agent);
            return true;
        }

        /// <summary>
        /// Removes the given agent from this grid.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        public void RemoveAgent(Agent agent)
        {
            if (!this.agents.Remove(agent)) { throw new InvalidOperationException("The given agent is not placed on the pathfinding grid!"); }
            this.RemoveAgentFromGrid(agent);
            agent.MovingStatusChanged -= this.OnAgentMovingStatusChanged;
        }

        /// <summary>
        /// Updates this grid.
        /// </summary>
        public void Update()
        {
            foreach (Agent agent in this.agents)
            {
                agent.Update();
            }

            foreach (IPathfindingAlgorithm pathfindingAlgorithm in this.pathfindingAlgorithms)
            {
                pathfindingAlgorithm.Execute();
            }
            this.pathfindingAlgorithms.Clear();
        }

        /// <summary>
        /// Adds the given pathfinding algorithm for execution.
        /// </summary>
        /// <param name="pathfindingAlgorithm">The pathfinding algorithm to be added.</param>
        public void AddPathfindingAlgorithm(IPathfindingAlgorithm pathfindingAlgorithm)
        {
            this.pathfindingAlgorithms.Add(pathfindingAlgorithm);
        }

        /// <summary>
        /// Removes the given pathfinding algorithm.
        /// </summary>
        /// <param name="pathfindingAlgorithm">The pathfinding algorithm to be removed.</param>
        public void RemovePathfindingAlgorithm(IPathfindingAlgorithm pathfindingAlgorithm)
        {
            this.pathfindingAlgorithms.Remove(pathfindingAlgorithm);
        }

        /// <summary>
        /// Places the given agent on the grid.
        /// </summary>
        /// <param name="agent">The agent to be placed.</param>
        private void PlaceAgentOnGrid(Agent agent)
        {
            for (int row = agent.Area.Top - (this.maxMovingSize - 1); row < agent.Area.Bottom; row++)
            {
                for (int column = agent.Area.Left - (this.maxMovingSize - 1); column < agent.Area.Right; column++)
                {
                    Cell cell = this[column, row];
                    if (cell != null)
                    {
                        int agentCellDistance = this.obstacleEnvironment[column - agent.Area.Left, row - agent.Area.Top];
                        cell.AddAgent(agentCellDistance + 1, agent);
                        if (agent.MovingStatus == AgentMovingStatusEnum.Static)
                        {
                            cell.Sector.AddStaticAgent(agentCellDistance + 1, agent);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the given agent from the grid.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        private void RemoveAgentFromGrid(Agent agent)
        {
            /// Remove the agent.
            for (int row = agent.Area.Top - (this.maxMovingSize - 1); row < agent.Area.Bottom; row++)
            {
                for (int column = agent.Area.Left - (this.maxMovingSize - 1); column < agent.Area.Right; column++)
                {
                    Cell cell = this[column, row];
                    if (cell != null)
                    {
                        cell.RemoveAgent(agent);
                        if (agent.MovingStatus == AgentMovingStatusEnum.Static)
                        {
                            cell.Sector.RemoveStaticAgent(agent);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the given desired position for the given agent.
        /// </summary>
        /// <param name="agent">The agent to be validated.</param>
        /// <param name="desiredPosition">The desired position of the agent.</param>
        /// <param name="collidingAgents">The output list of colliding agents if the validation failed; otherwise an empty set.</param>
        /// <returns>True if the desired position is OK for the given agent; otherwise false.</returns>
        private bool ValidatePositionForAgent(Agent agent, Cell desiredPosition, out RCSet<Agent> collidingAgents)
        {
            collidingAgents = new RCSet<Agent>();
            if (desiredPosition == null) { return false; }

            if (agent.MovingSize != -1)
            {
                /// This is a moving agent.
                if (desiredPosition.WallCellDistance < agent.MovingSize) { return false; }
                foreach (Agent overlappingAgent in desiredPosition.GetAgents(agent.MovingSize))
                {
                    if (agent != overlappingAgent && !agent.Client.IsOverlapEnabled(overlappingAgent.Client) && !overlappingAgent.Client.IsOverlapEnabled(agent.Client))
                    {
                        collidingAgents.Add(overlappingAgent);
                    }
                }
            }
            else
            {
                /// This is a static agent.
                for (int row = desiredPosition.Coords.Y; row < desiredPosition.Coords.Y + agent.Area.Height; row++)
                {
                    for (int column = desiredPosition.Coords.X; column < desiredPosition.Coords.X + agent.Area.Width; column++)
                    {
                        Cell cell = this[column, row];
                        if (cell == null || cell.WallCellDistance == 0) { return false; }
                        foreach (Agent overlappingAgent in cell.GetAgents(1))
                        {
                            if (agent != overlappingAgent && !agent.Client.IsOverlapEnabled(overlappingAgent.Client) && !overlappingAgent.Client.IsOverlapEnabled(agent.Client))
                            {
                                collidingAgents.Add(overlappingAgent);
                            }
                        }
                    }
                }
            }

            return collidingAgents.Count == 0;
        }

        /// <summary>
        /// This method is called when the moving status of an agent has been changed.
        /// </summary>
        /// <param name="agent">The agent whose moving status has been changed.</param>
        /// <param name="oldMovingStatus">The old moving status of the agent.</param>
        private void OnAgentMovingStatusChanged(Agent agent, AgentMovingStatusEnum oldMovingStatus)
        {
            if (oldMovingStatus == AgentMovingStatusEnum.Static || agent.MovingStatus == AgentMovingStatusEnum.Static)
            {
                for (int row = agent.Area.Top - (this.maxMovingSize - 1); row < agent.Area.Bottom; row++)
                {
                    for (int column = agent.Area.Left - (this.maxMovingSize - 1); column < agent.Area.Right; column++)
                    {
                        Cell cell = this[column, row];
                        if (cell != null)
                        {
                            int agentCellDistance = this.obstacleEnvironment[column - agent.Area.Left, row - agent.Area.Top];
                            if (agent.MovingStatus == AgentMovingStatusEnum.Static)
                            {
                                cell.Sector.AddStaticAgent(agentCellDistance + 1, agent);
                            }
                            else
                            {
                                cell.Sector.RemoveStaticAgent(agent);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The list of agents on this grid.
        /// </summary>
        private readonly RCSet<Agent> agents;

        /// <summary>
        /// The list of pathfinding algorithms to be executed.
        /// </summary>
        private readonly RCSet<IPathfindingAlgorithm> pathfindingAlgorithms;

        /// <summary>
        /// The 2D array that contains the cells of this grid.
        /// </summary>
        private readonly Cell[,] cells;

        /// <summary>
        /// The width of this grid.
        /// </summary>
        private readonly int width;

        /// <summary>
        /// The height of this grid.
        /// </summary>
        private readonly int height;

        /// <summary>
        /// The maximum size of moving agents.
        /// </summary>
        private readonly int maxMovingSize;

        /// <summary>
        /// The environment information of rectangular obstacles.
        /// </summary>
        private readonly ObstacleEnvironment obstacleEnvironment;

        /// <summary>
        /// The unit distances in straight and diagonal directions used by the pathfinding algorithms.
        /// </summary>
        public const int STRAIGHT_UNIT_DISTANCE = 2;
        public const int DIAGONAL_UNIT_DISTANCE = 3;
    }
}
