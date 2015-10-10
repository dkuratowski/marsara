using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Allows access to map functionalities for scenario elements.
    /// </summary>
    public class ScenarioMapContext
    {
        /// <summary>
        /// Constructs a ScenarioMapContext instance.
        /// </summary>
        /// <param name="mapObjectLayers">Reference to the layers of map objects.</param>
        /// <param name="fixedEntities">Reference to the array that stores the fixed Entities for each quadratic tile.</param>
        public ScenarioMapContext(Dictionary<MapObjectLayerEnum, ISearchTree<MapObject>> mapObjectLayers, Entity[,] fixedEntities)
        {
            this.mapObjectLayers = mapObjectLayers;
            this.fixedEntities = fixedEntities;
        }

        /// <summary>
        /// Gets a reference to the given layer of map objects.
        /// </summary>
        public ISearchTree<MapObject> GetMapObjectLayer(MapObjectLayerEnum layer)
        {
            return this.mapObjectLayers[layer];
        }

        /// <summary>
        /// Gets the array that stores the fixed Entities for each quadratic tile.
        /// </summary>
        public Entity[,] FixedEntities { get { return this.fixedEntities; } }

        /// <summary>
        /// Reference to the search tree of map objects.
        /// </summary>
        private readonly Dictionary<MapObjectLayerEnum, ISearchTree<MapObject>> mapObjectLayers;

        /// <summary>
        /// The array that stores the fixed Entities for each quadratic tile.
        /// </summary>
        private readonly Entity[,] fixedEntities;
    }
}
