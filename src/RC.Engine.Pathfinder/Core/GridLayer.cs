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
    /// Represents a layer on the pathfinder grid.
    /// </summary>
    class GridLayer : IDisposable
    {
        /// <summary>
        /// Constructs a GridLayer instance from the given walkability informations.
        /// </summary>
        /// <param name="walkabilityReader">Reference to the object that provides the walkability informations.</param>
        /// <param name="maxObjectSize">The maximum size of objects that can be placed onto this grid-layer.</param>
        public GridLayer(IWalkabilityReader walkabilityReader, int maxObjectSize)
        {
            this.agents = new RCSet<Agent>();
            this.cells = new Cell[walkabilityReader.Width, walkabilityReader.Height];
            this.width = walkabilityReader.Width;
            this.height = walkabilityReader.Height;
            this.maxObjectSize = maxObjectSize;

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
        /// Gets the maximum size of objects that can be placed onto this grid-layer.
        /// </summary>
        public int MaxObjectSize { get { return this.maxObjectSize; } }

        /// <summary>
        /// Gets the cell of this grid-layer at the given coordinates.
        /// </summary>
        /// <param name="x">The X-coordinate of the cell.</param>
        /// <param name="y">The Y-coordinate of the cell.</param>
        /// <returns>The cell at the given coordinates or null if the given coordinates are outside of this grid-layer.</returns>
        public Cell this[int x, int y] { get { return x >= 0 && x < this.width && y >= 0 && y < this.height ? this.cells[x, y] : null; } }

        /// <summary>
        /// Creates and places an agent of the given size to this grid-layer into the given position.
        /// </summary>
        /// <param name="position">The position of the top-left corner of the agent.</param>
        /// <param name="size">The size of the agent.</param>
        /// <param name="client">The client of the agent that will provide additional informations for the pathfinder component.</param>
        /// <returns>A reference to the agent or null if the placement failed.</returns>
        public Agent CreateAgent(RCIntVector position, int size, IObstacleClient client)
        {
            Agent newAgent = new Agent(position, size, this, client);
            this.agents.Add(newAgent);
            return newAgent;
        }

        /// <summary>
        /// Creates and places a static agent with the given area to this grid-layer.
        /// </summary>
        /// <param name="area">The rectangular area of the static agent.</param>
        /// <param name="client">The client of the placed agent that will provide additional informations for the pathfinder component.</param>
        /// <returns>A reference to the agent or null if the placement failed.</returns>
        public Agent CreateStaticAgent(RCIntRectangle area, IObstacleClient client)
        {
            Agent newStaticAgent = new Agent(area, this, client);
            this.agents.Add(newStaticAgent);
            return newStaticAgent;
        }

        /// <summary>
        /// Removes the given agent from this grid-layer.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        public void RemoveAgent(Agent agent)
        {
            this.agents.Remove(agent);
        }

        /// <summary>
        /// Updates the agents on this grid-layer.
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
        /// The list of agents on this grid-layer.
        /// </summary>
        private readonly RCSet<Agent> agents;

        /// <summary>
        /// The 2D array that contains the cells of this grid-layer.
        /// </summary>
        private readonly Cell[,] cells;

        /// <summary>
        /// The width of this grid-layer.
        /// </summary>
        private readonly int width;

        /// <summary>
        /// The height of this grid-layer.
        /// </summary>
        private readonly int height;

        /// <summary>
        /// The maximum size of objects that can be placed onto this grid-layer.
        /// </summary>
        private readonly int maxObjectSize;
    }
}
