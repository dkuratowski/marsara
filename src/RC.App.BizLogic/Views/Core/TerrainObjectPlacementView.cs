using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of object placement views for terrain objects.
    /// </summary>
    class TerrainObjectPlacementView : ObjectPlacementView, ITerrainObjectPlacementView
    {
        /// <summary>
        /// Constructs a TerrainObjectPlacementView instance.
        /// </summary>
        /// <param name="terrainObjectType">Reference to the type of the terrain object being placed.</param>
        /// <param name="map">Reference to the map.</param>
        public TerrainObjectPlacementView(ITerrainObjectType terrainObjectType, IMapAccess map) : base(map)
        {
            if (terrainObjectType == null) { throw new ArgumentNullException("terrainObjectType"); }
            this.terrainObjectType = terrainObjectType;
        }

        #region ObjectPlacementView overrides

        /// <see cref="ObjectPlacementView.CheckObjectConstraints"/>
        protected override HashSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftCoords)
        {
            HashSet<RCIntVector> violatingQuadCoords = this.terrainObjectType.CheckConstraints(this.Map, topLeftCoords);
            violatingQuadCoords.UnionWith(this.terrainObjectType.CheckTerrainObjectIntersections(this.Map, topLeftCoords));
            return violatingQuadCoords;
        }

        /// <see cref="ObjectPlacementView.GetObjectQuadraticSize"/>
        protected override RCIntVector GetObjectQuadraticSize()
        {
            return this.terrainObjectType.QuadraticSize;
        }

        /// <see cref="ObjectPlacementView.GetObjectSprites"/>
        protected override List<SpriteInst> GetObjectSprites()
        {
            return new List<SpriteInst>()
            {
                new SpriteInst()
                {
                    Index = this.terrainObjectType.Index,
                    DisplayCoords = new RCIntVector(0, 0),
                    Section = RCIntRectangle.Undefined
                }
            };
        }

        #endregion ObjectPlacementView overrides

        /// <summary>
        /// Reference to the type of the terrain object being placed.
        /// </summary>
        private ITerrainObjectType terrainObjectType;
    }
}
