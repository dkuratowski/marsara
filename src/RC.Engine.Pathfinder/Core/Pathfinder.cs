using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Pathfinder.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// The implementation of the pathfinder component.
    /// </summary>
    [Component("RC.Engine.Pathfinder")]
    class Pathfinder : IPathfinder
    {
        /// <summary>
        /// Constructs a Pathfinder instance.
        /// </summary>
        public Pathfinder()
        {
            this.grid = null;
        }

        #region IPathfinder methods

        /// <see cref="IPathfinder.Initialize"/>
        public void Initialize(IWalkabilityReader walkabilityReader, int maxMovingSize)
        {
            if (walkabilityReader == null) { throw new ArgumentNullException("walkabilityReader"); }
            if (maxMovingSize < 1) { throw new ArgumentOutOfRangeException("maxMovingSize", "The value of maxMovingSize shall be greater than 0!"); }

            /// Create the new grid.
            this.grid = new Grid(walkabilityReader, maxMovingSize);
        }

        /// <see cref="IPathfinder.PlaceAgent"/>
        public IAgent PlaceAgent(RCIntRectangle area, IAgentClient client)
        {
            if (this.grid == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (client == null) { throw new ArgumentNullException("client"); }

            return this.grid.CreateAgent(area, client);
        }

        /// <see cref="IPathfinder.RemoveAgent"/>
        public void RemoveAgent(IAgent agent)
        {
            if (this.grid == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }
            if (agent == null) { throw new ArgumentNullException("agent"); }

            this.grid.RemoveAgent((Agent)agent);
        }

        /// <see cref="IPathfinder.Update"/>
        public void Update()
        {
            if (this.grid == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }

            this.grid.Update();
        }

        #endregion IPathfinder methods

        /// <summary>
        /// Gets the pathfinding grid.
        /// TODO: This is only for debugging!
        /// </summary>
        internal Grid Grid { get { return this.grid; } }

        /// <summary>
        /// Reference to the pathfinding grid or null if the pathfinder component has not yet been initialized.
        /// </summary>
        private Grid grid;
    }
}
