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
            this.gridLayers = null;
        }

        #region IPathfinder methods

        /// <see cref="IPathfinder.Initialize"/>
        public void Initialize(IWalkabilityReader walkabilityReader, int maxMovingObstacleSize)
        {
            if (walkabilityReader == null) { throw new ArgumentNullException("walkabilityReader"); }
            if (maxMovingObstacleSize < 1) { throw new ArgumentOutOfRangeException("maxMovingObstacleSize", "The value of maxMovingObstacleSize shall be greater than 0!"); }

            /// Destroy the existing grid-layers if exist.
            if (this.gridLayers != null)
            {
                this.gridLayers[(int)PathfindingLayerEnum.Ground].Dispose();
                this.gridLayers[(int)PathfindingLayerEnum.Air].Dispose();
            }

            /// Create the new grid-layers.
            this.gridLayers = new GridLayer[2];
            this.gridLayers[(int)PathfindingLayerEnum.Ground] = new GridLayer(walkabilityReader, maxMovingObstacleSize);
            this.gridLayers[(int)PathfindingLayerEnum.Air] = new GridLayer(new ConstantWalkabilityReader(true, walkabilityReader.Width, walkabilityReader.Height),
                                                                           maxMovingObstacleSize);
        }

        /// <see cref="IPathfinder.PlaceMovingObstacle"/>
        public IMovingObstacle PlaceMovingObstacle(PathfindingLayerEnum layer, RCIntVector position, int size, IObstacleClient client)
        {
            if (this.gridLayers == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (size < 1 || size > this.gridLayers[(int)layer].MaxObjectSize) { throw new ArgumentOutOfRangeException("size", "Size of moving obstacles shall be greater that 0 and less than or equal with the maximum obstacle size with which the pathfinder component was initialized!"); }
            if (client == null) { throw new ArgumentNullException("client"); }

            return this.gridLayers[(int)layer].CreateAgent(position, size, client);
        }

        /// <see cref="IPathfinder.PlaceObstacle"/>
        public IObstacle PlaceObstacle(PathfindingLayerEnum layer, RCIntRectangle area, IObstacleClient client)
        {
            if (this.gridLayers == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (client == null) { throw new ArgumentNullException("client"); }

            return this.gridLayers[(int)layer].CreateStaticAgent(area, client);
        }

        /// <see cref="IPathfinder.RemoveObstacle"/>
        public void RemoveObstacle(IObstacle obstacle)
        {
            if (this.gridLayers == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }
            if (obstacle == null) { throw new ArgumentNullException("obstacle"); }

            this.gridLayers[(int)PathfindingLayerEnum.Ground].RemoveAgent((Agent)obstacle);
            this.gridLayers[(int)PathfindingLayerEnum.Air].RemoveAgent((Agent)obstacle);
        }

        /// <see cref="IPathfinder.Update"/>
        public void Update()
        {
            if (this.gridLayers == null) { throw new InvalidOperationException("The pathfinder component is not initialized!"); }

            this.gridLayers[(int)PathfindingLayerEnum.Ground].UpdateAgents();
            this.gridLayers[(int)PathfindingLayerEnum.Air].UpdateAgents();
        }

        #endregion IPathfinder methods

        /// <summary>
        /// Reference to the ground and air layers of the pathfinding grid (in this order) or null if the pathfinder component has not yet been initialized.
        /// </summary>
        private GridLayer[] gridLayers;
    }
}
