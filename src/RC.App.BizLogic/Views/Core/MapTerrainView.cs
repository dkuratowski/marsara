using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Common.ComponentModel;
using RC.App.BizLogic.BusinessComponents;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views on the terrain of the currently opened map.
    /// </summary>
    class MapTerrainView : MapViewBase, IMapTerrainView
    {
        /// <summary>
        /// Constructs a MapTerrainView instance.
        /// </summary>
        public MapTerrainView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region IMapTerrainView methods

        /// <see cref="IMapTerrainView.GetVisibleTerrainSprites"/>
        public List<SpriteRenderInfo> GetVisibleTerrainSprites()
        {
            List<SpriteRenderInfo> retList = new List<SpriteRenderInfo>();
            this.CollectVisibleIsoTiles(ref retList);
            this.CollectVisibleTerrainObjects(ref retList);
            return retList;
        }

        /// <see cref="IMapTerrainView.GetIsoTileDisplayCoords"/>
        public RCIntVector GetIsoTileDisplayCoords(RCIntVector position)
        {
            RCIntVector navCellCoords = this.MapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IIsoTile isotile = this.Map.GetCell(navCellCoords).ParentIsoTile;
            return this.MapWindowBC.AttachedWindow.CellToWindowRect(new RCIntRectangle(isotile.GetCellMapCoords(new RCIntVector(0, 0)), isotile.CellSize)).Location;
        }

        /// <see cref="IMapTerrainView.GetTerrainObjectDisplayCoords"/>
        public RCIntVector GetTerrainObjectDisplayCoords(RCIntVector position)
        {
            RCIntVector navCellCoords = this.MapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            ITerrainObject objToCheck = this.Map.GetCell(navCellCoords).ParentQuadTile.TerrainObject;
            if (objToCheck != null)
            {
                return this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(objToCheck.MapCoords, new RCIntVector(1, 1))).Location;
            }

            return RCIntVector.Undefined;
        }

        /// <see cref="IMapTerrainView.GetWalkableCells"/>
        public List<RCIntRectangle> GetWalkableCells()
        {
            List<RCIntRectangle> retList = new List<RCIntRectangle>();
            for (int row = this.MapWindowBC.AttachedWindow.CellWindow.Top; row < this.MapWindowBC.AttachedWindow.CellWindow.Bottom; row++)
            {
                for (int col = this.MapWindowBC.AttachedWindow.CellWindow.Left; col < this.MapWindowBC.AttachedWindow.CellWindow.Right; col++)
                {
                    RCIntVector cellCoords = new RCIntVector(col, row);
                    if (this.Map.GetCell(cellCoords).IsBuildable)
                    {
                        retList.Add(this.MapWindowBC.AttachedWindow.CellToWindowRect(new RCIntRectangle(cellCoords, new RCIntVector(1, 1))));
                    }
                }
            }
            return retList;
        }

        #endregion IMapTerrainView methods

        /// <summary>
        /// Collects the isometric tiles to update into the given list.
        /// </summary>
        /// <param name="targetList">The target list.</param>
        private void CollectVisibleIsoTiles(ref List<SpriteRenderInfo> targetList)
        {
            /// Collect the currently visible isometric tiles.
            foreach (IIsoTile isoTile in this.fogOfWarBC.GetIsoTilesToUpdate())
            {
                targetList.Add(
                    new SpriteRenderInfo()
                    {
                        SpriteGroup = SpriteGroupEnum.IsoTileSpriteGroup,
                        Index = isoTile.Variant.Index,
                        DisplayCoords = this.MapWindowBC.AttachedWindow.CellToWindowRect(new RCIntRectangle(isoTile.GetCellMapCoords(new RCIntVector(0, 0)), isoTile.CellSize)).Location,
                        Section = RCIntRectangle.Undefined
                    });
            }

        }

        /// <summary>
        /// Collects the terrain objects to update into the given list.
        /// </summary>
        /// <param name="targetList">The target list.</param>
        private void CollectVisibleTerrainObjects(ref List<SpriteRenderInfo> targetList)
        {
            foreach (ITerrainObject terrainObj in this.fogOfWarBC.GetTerrainObjectsToUpdate())
            {
                targetList.Add(new SpriteRenderInfo()
                {
                    SpriteGroup = SpriteGroupEnum.TerrainObjectSpriteGroup,
                    Index = terrainObj.Type.Index,
                    DisplayCoords = this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(terrainObj.MapCoords, new RCIntVector(1, 1))).Location,
                    Section = RCIntRectangle.Undefined
                });
            }
        }

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
