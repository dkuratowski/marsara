using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// The implementation of the minimap view.
    /// </summary>
    class MinimapView : MapViewBase, IMinimapView
    {
        #region IMinimapView members

        /// <see cref="IMinimapView.GetIsoTileSprites"/>
        public List<SpriteInst> GetIsoTileSprites()
        {
            /// Collect the isometric tiles that need to be rendered.
            HashSet<IIsoTile> isoTilesToRender = new HashSet<IIsoTile>();
            List<SpriteInst> retList = new List<SpriteInst>();
            for (int column = this.MapWindowBC.FullWindow.QuadTileWindow.Left; column < this.MapWindowBC.FullWindow.QuadTileWindow.Right; column++)
            {
                for (int row = this.MapWindowBC.FullWindow.QuadTileWindow.Top; row < this.MapWindowBC.FullWindow.QuadTileWindow.Bottom; row++)
                {
                    /// Add the primary & secondary isometric tiles into the render list.
                    IQuadTile quadTileToUpdate = this.Map.GetQuadTile(new RCIntVector(column, row));
                    if (quadTileToUpdate.PrimaryIsoTile != null && isoTilesToRender.Add(quadTileToUpdate.PrimaryIsoTile))
                    {
                        retList.Add(this.ConvertIsoTileToSpriteInst(quadTileToUpdate.PrimaryIsoTile));
                    }
                    if (quadTileToUpdate.SecondaryIsoTile != null && isoTilesToRender.Add(quadTileToUpdate.SecondaryIsoTile))
                    {
                        retList.Add(this.ConvertIsoTileToSpriteInst(quadTileToUpdate.SecondaryIsoTile));
                    }
                }
            }
            return retList;
        }

        /// <see cref="IMinimapView.GetTerrainObjectSprites"/>
        public List<SpriteInst> GetTerrainObjectSprites()
        {
            /// Collect the terrain objects that need to be rendered.
            HashSet<ITerrainObject> terrainObjectsToRender = new HashSet<ITerrainObject>();
            List<SpriteInst> retList = new List<SpriteInst>();
            for (int column = this.MapWindowBC.FullWindow.QuadTileWindow.Left; column < this.MapWindowBC.FullWindow.QuadTileWindow.Right; column++)
            {
                for (int row = this.MapWindowBC.FullWindow.QuadTileWindow.Top; row < this.MapWindowBC.FullWindow.QuadTileWindow.Bottom; row++)
                {
                    /// Add the primary & secondary isometric tiles & the terrain objects into the render lists.
                    IQuadTile quadTileToUpdate = this.Map.GetQuadTile(new RCIntVector(column, row));
                    if (quadTileToUpdate.TerrainObject != null && terrainObjectsToRender.Add(quadTileToUpdate.TerrainObject))
                    {
                        retList.Add(this.ConvertTerrainObjectToSpriteInst(quadTileToUpdate.TerrainObject));
                    }
                }
            }
            return retList;
        }

        /// <see cref="IMinimapView.MapPixelSize"/>
        public RCIntRectangle WindowIndicator { get { return this.MapWindowBC.Minimap.WindowIndicator; } }

        /// <see cref="IMinimapView.MapPixelSize"/>
        public RCIntVector MapPixelSize { get { return this.MapWindowBC.FullWindow.PixelWindow.Size; } }

        /// <see cref="IMinimapView.MinimapPosition"/>
        public RCIntRectangle MinimapPosition { get { return this.MapWindowBC.Minimap.MinimapPosition; } }

        #endregion IMinimapView members

        /// <summary>
        /// Converts the given isometric tile into a SpriteInst structure.
        /// </summary>
        /// <param name="isotile">The isometric tile to convert.</param>
        /// <returns>The converted SpriteInst structure.</returns>
        private SpriteInst ConvertIsoTileToSpriteInst(IIsoTile isotile)
        {
            return new SpriteInst()
            {
                Index = isotile.Variant.Index,
                DisplayCoords = this.MapWindowBC.FullWindow.CellToWindowRect(new RCIntRectangle(isotile.GetCellMapCoords(new RCIntVector(0, 0)), isotile.CellSize)).Location,
                Section = RCIntRectangle.Undefined
            };
        }

        /// <summary>
        /// Converts the given terrain object into a SpriteInst structure.
        /// </summary>
        /// <param name="terrainObject">The terrain object to convert.</param>
        /// <returns>The converted SpriteInst structure.</returns>
        private SpriteInst ConvertTerrainObjectToSpriteInst(ITerrainObject terrainObject)
        {
            return new SpriteInst()
            {
                Index = terrainObject.Type.Index,
                DisplayCoords = this.MapWindowBC.FullWindow.QuadToWindowRect(new RCIntRectangle(terrainObject.MapCoords, new RCIntVector(1, 1))).Location,
                Section = RCIntRectangle.Undefined
            };
        }
    }
}
