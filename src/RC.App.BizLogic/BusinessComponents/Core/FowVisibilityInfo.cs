using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Stores cached results of FogOfWarBC visibility calculations.
    /// </summary>
    struct FowVisibilityInfo
    {
        /// <summary>
        /// Gets the isometric tiles that are not entirely hidden by the Fog Of War.
        /// </summary>
        public IEnumerable<IIsoTile> IsoTilesToUpdate
        {
            get { return this.isoTilesToUpdate; }
            set
            {
                if (this.isoTilesToUpdate != null) { throw new InvalidOperationException("FogOfWarBCCache.IsoTilesToUpdate has already been set!"); }
                this.isoTilesToUpdate = value;
            }
        }

        /// <summary>
        /// Gets the terrain objects that are not entirely hidden by the Fog Of War.
        /// </summary>
        public IEnumerable<ITerrainObject> TerrainObjectsToUpdate
        {
            get { return this.terrainObjectsToUpdate; }
            set
            {
                if (this.terrainObjectsToUpdate != null) { throw new InvalidOperationException("FogOfWarBCCache.TerrainObjectsToUpdate has already been set!"); }
                this.terrainObjectsToUpdate = value;
            }
        }

        /// <summary>
        /// Gets the quadratic tiles on which the Fog Of War shall be updated.
        /// </summary>
        public IEnumerable<IQuadTile> QuadTilesToUpdate
        {
            get { return this.quadTilesToUpdate; }
            set
            {
                if (this.quadTilesToUpdate != null) { throw new InvalidOperationException("FogOfWarBCCache.QuadTilesToUpdate has already been set!"); }
                this.quadTilesToUpdate = value;
            }
        }

        /// <summary>
        /// The isometric tiles that are not entirely hidden by the Fog Of War.
        /// </summary>
        private IEnumerable<IIsoTile> isoTilesToUpdate;

        /// <summary>
        /// The terrain objects that are not entirely hidden by the Fog Of War.
        /// </summary>
        private IEnumerable<ITerrainObject> terrainObjectsToUpdate;

        /// <summary>
        /// The quadratic tiles on which the Fog Of War shall be updated.
        /// </summary>
        private IEnumerable<IQuadTile> quadTilesToUpdate;
    }
}
