using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.PublicInterfaces;
using RC.Common;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on the terrain of the currently opened map.
    /// </summary>
    class MapTerrainView : IMapTerrainView
    {
        /// <summary>
        /// Constructs a MapTerrainView instance.
        /// </summary>
        /// <param name="map">The subject of this view.</param>
        public MapTerrainView(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            this.map = map;
        }

        #region IMapTerrainView methods

        /// <see cref="IMapView.MapSize"/>
        public RCIntVector MapSize { get { return this.map.CellSize * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL); } }

        /// <see cref="IMapTerrainView.GetVisibleIsoTiles"/>
        public List<MapSpriteInstance> GetVisibleIsoTiles(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow = new RCIntRectangle(displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           (displayedArea.Right - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL + 1,
                                                           (displayedArea.Bottom - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL + 1);
            RCIntVector displayOffset = new RCIntVector(displayedArea.X % BizLogicConstants.PIXEL_PER_NAVCELL, displayedArea.Y % BizLogicConstants.PIXEL_PER_NAVCELL);

            List<MapSpriteInstance> retList = new List<MapSpriteInstance>();
            HashSet<IIsoTile> tmpIsoTileList = new HashSet<IIsoTile>();

            RCIntVector topLeftNavCellCoords = new RCIntVector(Math.Max(0, cellWindow.Left), Math.Max(0, cellWindow.Top));
            RCIntVector bottomRightNavCellCoords = new RCIntVector(Math.Min(this.map.CellSize.X - 1, cellWindow.Right - 1), Math.Min(this.map.CellSize.Y - 1, cellWindow.Bottom - 1));
            IQuadTile topLeftQuadTile = this.map.GetCell(topLeftNavCellCoords).ParentQuadTile;
            IQuadTile bottomRightQuadTile = this.map.GetCell(bottomRightNavCellCoords).ParentQuadTile;

            for (int x = topLeftQuadTile.MapCoords.X; x <= bottomRightQuadTile.MapCoords.X; x++)
            {
                for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                {
                    IIsoTile isotile = this.map.GetQuadTile(new RCIntVector(x, y)).IsoTile;
                    if (tmpIsoTileList.Contains(isotile)) { continue; }

                    tmpIsoTileList.Add(isotile);
                    retList.Add(
                        new MapSpriteInstance()
                        {
                            Index = isotile.Variant.Index,
                            DisplayCoords = (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                                          * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                          - displayOffset
                        });
                }

                if (x == topLeftQuadTile.MapCoords.X)
                {
                    for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                    {
                        IQuadTile quadTile = this.map.GetQuadTile(new RCIntVector(x, y));
                        for (int row = 0; row < quadTile.CellSize.Y; row++)
                        {
                            IIsoTile isotile = quadTile.GetCell(new RCIntVector(0, row)).ParentIsoTile;
                            if (tmpIsoTileList.Contains(isotile)) { continue; }

                            tmpIsoTileList.Add(isotile);
                            retList.Add(
                                new MapSpriteInstance()
                                {
                                    Index = isotile.Variant.Index,
                                    DisplayCoords = (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                                  - displayOffset
                                });
                        }
                    }
                }
                else if (x == bottomRightQuadTile.MapCoords.X)
                {
                    for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                    {
                        IQuadTile quadTile = this.map.GetQuadTile(new RCIntVector(x, y));
                        for (int row = 0; row < quadTile.CellSize.Y; row++)
                        {
                            IIsoTile isotile = quadTile.GetCell(new RCIntVector(quadTile.CellSize.X - 1, row)).ParentIsoTile;
                            if (tmpIsoTileList.Contains(isotile)) { continue; }

                            tmpIsoTileList.Add(isotile);
                            retList.Add(
                                new MapSpriteInstance()
                                {
                                    Index = isotile.Variant.Index,
                                    DisplayCoords = (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                                  - displayOffset
                                });
                        }
                    }
                }
            }

            return retList;
        }

        /// <see cref="IMapTerrainView.GetIsoTileDisplayCoords"/>
        public RCIntVector GetIsoTileDisplayCoords(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(position)) { throw new ArgumentOutOfRangeException("displayedArea"); }
            
            RCIntRectangle cellWindow = new RCIntRectangle(displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           (displayedArea.Right - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL + 1,
                                                           (displayedArea.Bottom - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL + 1);
            RCIntVector displayOffset = new RCIntVector(displayedArea.X % BizLogicConstants.PIXEL_PER_NAVCELL, displayedArea.Y % BizLogicConstants.PIXEL_PER_NAVCELL);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IIsoTile isotile = this.map.GetCell(navCellCoords).ParentIsoTile;
            return (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                 * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                 - displayOffset;
        }

        /// <see cref="IMapTerrainView.GetVisibleTerrainObjects"/>
        public List<MapSpriteInstance> GetVisibleTerrainObjects(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow = new RCIntRectangle(displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           (displayedArea.Right - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL + 1,
                                                           (displayedArea.Bottom - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL + 1);
            RCIntVector displayOffset = new RCIntVector(displayedArea.X % BizLogicConstants.PIXEL_PER_NAVCELL, displayedArea.Y % BizLogicConstants.PIXEL_PER_NAVCELL);

            List<MapSpriteInstance> retList = new List<MapSpriteInstance>();
            HashSet<ITerrainObject> visibleTerrainObjects = this.map.TerrainObjects.GetContents(
                new RCNumRectangle(cellWindow.X - (RCNumber)1/(RCNumber)2,
                                   cellWindow.Y - (RCNumber)1/(RCNumber)2,
                                   cellWindow.Width,
                                   cellWindow.Height));
            foreach (ITerrainObject terrainObj in visibleTerrainObjects)
            {
                retList.Add(new MapSpriteInstance()
                {
                    Index = terrainObj.Type.Index,
                    DisplayCoords = (this.map.QuadToCellRect(new RCIntRectangle(terrainObj.MapCoords, new RCIntVector(1, 1))).Location - cellWindow.Location)
                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                  - displayOffset
                });
            }
            return retList;
        }

        /// <see cref="IMapTerrainView.GetTerrainObjectDisplayCoords"/>
        public RCIntVector GetTerrainObjectDisplayCoords(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(position)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow = new RCIntRectangle(displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                           (displayedArea.Right - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL + 1,
                                                           (displayedArea.Bottom - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL + 1);
            RCIntVector displayOffset = new RCIntVector(displayedArea.X % BizLogicConstants.PIXEL_PER_NAVCELL, displayedArea.Y % BizLogicConstants.PIXEL_PER_NAVCELL);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.map.GetCell(navCellCoords).ParentQuadTile;
            foreach (ITerrainObject objToCheck in this.map.TerrainObjects.GetContents(navCellCoords))
            {
                if (!objToCheck.Type.IsExcluded(quadTileAtPos.MapCoords - objToCheck.MapCoords))
                {
                    return (this.map.QuadToCellRect(new RCIntRectangle(objToCheck.MapCoords, new RCIntVector(1, 1))).Location - cellWindow.Location)
                         * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                         - displayOffset;
                }
            }

            return RCIntVector.Undefined;
        }

        #endregion IMapTerrainView methods

        /// <summary>
        /// The subject of this view.
        /// </summary>
        private IMapAccess map;
    }
}
