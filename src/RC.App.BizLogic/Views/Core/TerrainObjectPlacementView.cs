using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.Common.ComponentModel;
using RC.App.BizLogic.BusinessComponents;

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
        /// <param name="terrainObjectName">The name of the type of the terrain object being placed.</param>
        public TerrainObjectPlacementView(string terrainObjectName)
        {
            if (terrainObjectName == null) { throw new ArgumentNullException("terrainObjectName"); }

            this.terrainObjectType = this.Map.Tileset.GetTerrainObjectType(terrainObjectName);
        }

        #region ObjectPlacementView overrides

        /// <see cref="ObjectPlacementView.CheckObjectConstraints"/>
        protected override RCSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftQuadCoords)
        {
            RCSet<RCIntVector> violatingQuadCoords = this.terrainObjectType.CheckConstraints(this.Map, topLeftQuadCoords);
            violatingQuadCoords.UnionWith(this.terrainObjectType.CheckTerrainObjectIntersections(this.Map, topLeftQuadCoords));
            return violatingQuadCoords;
        }

        /// <see cref="ObjectPlacementView.GetObjectRelativeQuadRectangles"/>
        protected override RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> GetObjectRelativeQuadRectangles()
        {
            return new RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>>
            {
                Tuple.Create(
                    new RCIntRectangle(-1*this.terrainObjectType.QuadraticSize/2, this.terrainObjectType.QuadraticSize),
                    new SpriteRenderInfo[]
                    {
                        new SpriteRenderInfo()
                        {
                            SpriteGroup = SpriteGroupEnum.TerrainObjectSpriteGroup,
                            Index = this.terrainObjectType.Index,
                            DisplayCoords = new RCIntVector(0, 0),
                            Section = RCIntRectangle.Undefined
                        }
                    })
            };
        }

        #endregion ObjectPlacementView overrides

        /// <summary>
        /// Reference to the type of the terrain object being placed.
        /// </summary>
        private readonly ITerrainObjectType terrainObjectType;
    }
}
