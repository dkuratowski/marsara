using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.App.BizLogic.BusinessComponents.Core;

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
        }

        #region IMapTerrainView methods

        /// <see cref="IMapTerrainView.GetVisibleIsoTiles"/>
        public List<SpriteInst> GetVisibleIsoTiles(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<SpriteInst> retList = new List<SpriteInst>();
            HashSet<IIsoTile> tmpIsoTileList = new HashSet<IIsoTile>();

            RCIntVector topLeftNavCellCoords = new RCIntVector(Math.Max(0, cellWindow.Left), Math.Max(0, cellWindow.Top));
            RCIntVector bottomRightNavCellCoords = new RCIntVector(Math.Min(this.Map.CellSize.X - 1, cellWindow.Right - 1), Math.Min(this.Map.CellSize.Y - 1, cellWindow.Bottom - 1));
            IQuadTile topLeftQuadTile = this.Map.GetCell(topLeftNavCellCoords).ParentQuadTile;
            IQuadTile bottomRightQuadTile = this.Map.GetCell(bottomRightNavCellCoords).ParentQuadTile;

            for (int x = topLeftQuadTile.MapCoords.X; x <= bottomRightQuadTile.MapCoords.X; x++)
            {
                for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                {
                    IIsoTile isotile = this.Map.GetQuadTile(new RCIntVector(x, y)).IsoTile;
                    if (tmpIsoTileList.Contains(isotile)) { continue; }

                    tmpIsoTileList.Add(isotile);
                    retList.Add(
                        new SpriteInst()
                        {
                            Index = isotile.Variant.Index,
                            DisplayCoords = (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                                          * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                          - displayOffset,
                            Section = RCIntRectangle.Undefined
                        });
                }

                if (x == topLeftQuadTile.MapCoords.X)
                {
                    for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                    {
                        IQuadTile quadTile = this.Map.GetQuadTile(new RCIntVector(x, y));
                        for (int row = 0; row < quadTile.CellSize.Y; row++)
                        {
                            IIsoTile isotile = quadTile.GetCell(new RCIntVector(0, row)).ParentIsoTile;
                            if (tmpIsoTileList.Contains(isotile)) { continue; }

                            tmpIsoTileList.Add(isotile);
                            retList.Add(
                                new SpriteInst()
                                {
                                    Index = isotile.Variant.Index,
                                    DisplayCoords = (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                                  - displayOffset,
                                    Section = RCIntRectangle.Undefined
                                });
                        }
                    }
                }
                else if (x == bottomRightQuadTile.MapCoords.X)
                {
                    for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                    {
                        IQuadTile quadTile = this.Map.GetQuadTile(new RCIntVector(x, y));
                        for (int row = 0; row < quadTile.CellSize.Y; row++)
                        {
                            IIsoTile isotile = quadTile.GetCell(new RCIntVector(quadTile.CellSize.X - 1, row)).ParentIsoTile;
                            if (tmpIsoTileList.Contains(isotile)) { continue; }

                            tmpIsoTileList.Add(isotile);
                            retList.Add(
                                new SpriteInst()
                                {
                                    Index = isotile.Variant.Index,
                                    DisplayCoords = (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                                  - displayOffset,
                                    Section = RCIntRectangle.Undefined
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

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IIsoTile isotile = this.Map.GetCell(navCellCoords).ParentIsoTile;
            return (isotile.GetCellMapCoords(new RCIntVector(0, 0)) - cellWindow.Location)
                 * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                 - displayOffset;
        }

        /// <see cref="IMapTerrainView.GetVisibleTerrainObjects"/>
        public List<SpriteInst> GetVisibleTerrainObjects(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<SpriteInst> retList = new List<SpriteInst>();
            HashSet<ITerrainObject> visibleTerrainObjects = this.Map.TerrainObjects.GetContents(
                new RCNumRectangle(cellWindow.X - (RCNumber)1/(RCNumber)2,
                                   cellWindow.Y - (RCNumber)1/(RCNumber)2,
                                   cellWindow.Width,
                                   cellWindow.Height));
            foreach (ITerrainObject terrainObj in visibleTerrainObjects)
            {
                retList.Add(new SpriteInst()
                {
                    Index = terrainObj.Type.Index,
                    DisplayCoords = (this.Map.QuadToCellRect(new RCIntRectangle(terrainObj.MapCoords, new RCIntVector(1, 1))).Location - cellWindow.Location)
                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                  - displayOffset,
                    Section = RCIntRectangle.Undefined
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

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.Map.GetCell(navCellCoords).ParentQuadTile;
            foreach (ITerrainObject objToCheck in this.Map.TerrainObjects.GetContents(navCellCoords))
            {
                if (!objToCheck.Type.IsExcluded(quadTileAtPos.MapCoords - objToCheck.MapCoords))
                {
                    return (this.Map.QuadToCellRect(new RCIntRectangle(objToCheck.MapCoords, new RCIntVector(1, 1))).Location - cellWindow.Location)
                         * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                         - displayOffset;
                }
            }

            return RCIntVector.Undefined;
        }

        /// <see cref="IMapTerrainView.GetWalkableCells"/>
        public List<RCIntRectangle> GetWalkableCells(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<RCIntRectangle> retList = new List<RCIntRectangle>();
            for (int row = cellWindow.Top; row < cellWindow.Bottom; row++)
            {
                for (int col = cellWindow.Left; col < cellWindow.Right; col++)
                {
                    RCIntVector cellCoords = new RCIntVector(col, row);
                    if (this.Map.GetCell(cellCoords).IsBuildable)
                    {
                        retList.Add(new RCIntRectangle((cellCoords - cellWindow.Location) * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL) - displayOffset,
                                                       new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)));
                    }
                }
            }
            return retList;
        }

        #endregion IMapTerrainView methods
    }
}
