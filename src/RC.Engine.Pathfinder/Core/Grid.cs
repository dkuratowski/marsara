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
    class Grid : IDisposable
    {
        /// <summary>
        /// Constructs a Grid instance from the given walkability informations.
        /// </summary>
        /// <param name="walkabilityReader">Reference to the object that provides the walkability informations.</param>
        /// <param name="maxMovingSize">The maximum size of moving agents.</param>
        public Grid(IWalkabilityReader walkabilityReader, int maxMovingSize)
        {
            this.agents = new RCSet<Agent>();
            this.cells = new Cell[walkabilityReader.Width, walkabilityReader.Height];
            this.width = walkabilityReader.Width;
            this.height = walkabilityReader.Height;
            this.maxMovingSize = maxMovingSize;

            /// Create the cells.
            for (int row = 0; row < this.height; row++)
            {
                for (int column = 0; column < this.width; column++)
                {
                    this.cells[column, row] = new Cell(new RCIntVector(column, row), this);
                }
            }
        }

        /// <summary>
        /// Gets the maximum size of moving agents.
        /// </summary>
        public int MaxMovingSize { get { return this.maxMovingSize; } }

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
            Agent newStaticAgent = new Agent(area, this, client);
            this.agents.Add(newStaticAgent);
            return newStaticAgent;
        }

        /// <summary>
        /// Removes the given agent from this grid.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        public void RemoveAgent(Agent agent)
        {
            this.agents.Remove(agent);
        }

        /// <summary>
        /// Updates the agents on this grid.
        /// </summary>
        public void UpdateAgents()
        {
            foreach (Agent agent in this.agents)
            {
                agent.Update();
            }
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion IDisposable members

        /// <summary>
        /// The list of agents on this grid.
        /// </summary>
        private readonly RCSet<Agent> agents;

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
    }
}
