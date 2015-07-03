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
        /// <param name="mapObjects">Reference to the search tree of map objects.</param>
        /// <param name="boundQuadEntities">Reference to the array that stores the bound QuadEntities for each quadratic tile.</param>
        public ScenarioMapContext(ISearchTree<MapObject> mapObjects, QuadEntity[,] boundQuadEntities)
        {
            this.mapObjects = mapObjects;
            this.boundQuadEntities = boundQuadEntities;
        }

        /// <summary>
        /// Gets a reference to the search tree of map objects.
        /// </summary>
        public ISearchTree<MapObject> MapObjects { get { return this.mapObjects; } }

        /// <summary>
        /// Gets the array that stores the bound QuadEntities for each quadratic tile.
        /// </summary>
        public QuadEntity[,] BoundQuadEntities { get { return this.boundQuadEntities; } }

        /// <summary>
        /// Reference to the search tree of map objects.
        /// </summary>
        private readonly ISearchTree<MapObject> mapObjects;

        /// <summary>
        /// The array that stores the bound QuadEntities for each quadratic tile.
        /// </summary>
        private readonly QuadEntity[,] boundQuadEntities;
    }
}
