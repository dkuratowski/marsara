using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// The implementation of the minimap view.
    /// </summary>
    class MinimapView : MapViewBase, IMinimapView
    {
        /// <summary>
        /// Constructs a MinimapView instance.
        /// </summary>
        public MinimapView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
        }

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

        /// <see cref="IMinimapView.RefreshPixelInfos"/>
        public void RefreshPixelInfos(int firstRowIndex, int rowsCount, MinimapPixelInfo[,] pixelInfos)
        {
            if (firstRowIndex < 0) { throw new ArgumentOutOfRangeException("firstRowIndex", "The index of the first row must be non-negative!"); }
            if (firstRowIndex >= this.MapWindowBC.Minimap.MinimapPosition.Height) { throw new ArgumentOutOfRangeException("firstRowIndex", "The index of the first row must be less than the height of the minimap image!"); }
            if (rowsCount < 1) { throw new ArgumentOutOfRangeException("rowsCount", "The number of scanned rows must be positive!"); }

            /// Collect the FOW-status of the scanned pixels.
            for (int row = firstRowIndex; row < firstRowIndex + rowsCount && row < this.MapWindowBC.Minimap.MinimapPosition.Height; row++)
            {
                for (int col = 0; col < this.MapWindowBC.Minimap.MinimapPosition.Width; col++)
                {
                    RCIntVector pixelCoords = new RCIntVector(col, row);
                    pixelInfos[col, row].PixelCoords = pixelCoords;
                    pixelInfos[col, row].FOWStatus = this.GetFowStateAtPixel(pixelCoords);
                    pixelInfos[col, row].EntityIndicatorType = MinimapPixelInfo.EntityIndicatorTypeEnum.None;
                }
            }

            /// Collect the entity informations.
            RCIntRectangle topLeftQuadRect =
                this.MapWindowBC.Minimap.GetMinimapPixel(new RCIntVector(0, firstRowIndex)).CoveredQuadTiles;
            RCIntRectangle bottomRightQuadRect =
                this.MapWindowBC.Minimap.GetMinimapPixel(new RCIntVector(this.MapWindowBC.Minimap.MinimapPosition.Width - 1, firstRowIndex + rowsCount - 1)).CoveredQuadTiles;
            RCIntVector topLeftQuadTile = topLeftQuadRect.Location;
            RCIntVector bottomRightQuadTile = bottomRightQuadRect.Location + bottomRightQuadRect.Size;
            RCIntRectangle scannedQuadWindow = new RCIntRectangle(topLeftQuadTile, bottomRightQuadTile - topLeftQuadTile);
            foreach (Entity entity in this.fogOfWarBC.GetEntitiesInWindow(scannedQuadWindow))
            {
                this.AddEntityInfo(entity.QuadraticPosition, BizLogicHelpers.GetEntityOwner(entity), pixelInfos);
            }
            foreach (EntitySnapshot entitySnapshot in this.fogOfWarBC.GetEntitySnapshotsInWindow(scannedQuadWindow))
            {
                this.AddEntityInfo(entitySnapshot.QuadraticPosition, entitySnapshot.Owner, pixelInfos);
            }
        }

        /// <see cref="IMinimapView.WindowIndicator"/>
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

        /// <summary>
        /// Gets the FOW-status at the given minimap pixel.
        /// </summary>
        /// <param name="pixelCoords">The coordinates of the pixel on the minimap image.</param>
        /// <returns>The FOW-status at the given minimap pixel.</returns>
        private FOWTypeEnum GetFowStateAtPixel(RCIntVector pixelCoords)
        {
            RCIntRectangle quadRect = this.MapWindowBC.Minimap.GetMinimapPixel(pixelCoords).CoveredQuadTiles;
            FOWTypeEnum lowestFowState = FOWTypeEnum.Full;
            for (int column = quadRect.Left; column < quadRect.Right; column++)
            {
                for (int row = quadRect.Top; row < quadRect.Bottom; row++)
                {
                    FOWTypeEnum currentFowState = this.fogOfWarBC.GetFowState(new RCIntVector(column, row));
                    if (currentFowState == FOWTypeEnum.None)
                    {
                        lowestFowState = currentFowState;
                        break;
                    }
                    if ((int)currentFowState < (int)lowestFowState)
                    {
                        lowestFowState = currentFowState;
                    }
                }
            }
            return lowestFowState;
        }

        /// <summary>
        /// Adds the given entity informations to the minimap pixels.
        /// </summary>
        /// <param name="quadraticPosition">The quadratic position of the entity.</param>
        /// <param name="owner">The owner of the entity.</param>
        /// <param name="pixelInfos">The 2D array of the minimap pixels.</param>
        private void AddEntityInfo(RCIntRectangle quadraticPosition, PlayerEnum owner, MinimapPixelInfo[,] pixelInfos)
        {
            for (int quadTileY = quadraticPosition.Top; quadTileY < quadraticPosition.Bottom; quadTileY++)
            {
                for (int quadTileX = quadraticPosition.Left; quadTileX < quadraticPosition.Right; quadTileX++)
                {
                    IMinimapPixel pixelAtQuadTile = this.MapWindowBC.Minimap.GetMinimapPixelAtQuadTile(new RCIntVector(quadTileX, quadTileY));
                    if (this.selectionManagerBC.LocalPlayer != PlayerEnum.Neutral && owner == this.selectionManagerBC.LocalPlayer)
                    {
                        /// TODO: distinguish between friendly and attacked-friendly entities!
                        pixelInfos[pixelAtQuadTile.PixelCoords.X, pixelAtQuadTile.PixelCoords.Y].EntityIndicatorType = MinimapPixelInfo.EntityIndicatorTypeEnum.Friendly;
                    }
                    else
                    {
                        pixelInfos[pixelAtQuadTile.PixelCoords.X, pixelAtQuadTile.PixelCoords.Y].EntityIndicatorType = MinimapPixelInfo.EntityIndicatorTypeEnum.NonFriendly;
                    }
                    pixelInfos[pixelAtQuadTile.PixelCoords.X, pixelAtQuadTile.PixelCoords.Y].EntityOwner = owner;
                }
            }
        }

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;

        /// <summary>
        /// Reference to the selection manager business component.
        /// TODO: Only needed to get the local player! Remove this dependency when possible!
        /// </summary>
        private readonly ISelectionManagerBC selectionManagerBC;
    }
}
