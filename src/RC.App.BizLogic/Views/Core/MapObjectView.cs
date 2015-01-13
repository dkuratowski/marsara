using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Scenarios;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views on the objects of the currently opened map.
    /// </summary>
    class MapObjectView : MapViewBase, IMapObjectView
    {
        /// <summary>
        /// Constructs a MapObjectView instance.
        /// </summary>
        public MapObjectView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region IMapObjectView methods

        /// <see cref="IMapObjectView.GetVisibleMapObjects"/>
        public List<ObjectInst> GetVisibleMapObjects(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            /// Calculate the currently visible window of cells and quadratic tiles.
            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);
            RCIntRectangle quadTileWindow = this.Map.CellToQuadRect(cellWindow);

            /// Display the currently visible entities inside the currently visible window of quadratic tiles.
            List<ObjectInst> retList = new List<ObjectInst>();
            foreach (Entity entity in this.fogOfWarBC.GetEntitiesToUpdate(quadTileWindow))
            {
                RCIntRectangle displayRect =
                    (RCIntRectangle)((entity.BoundingBox - cellWindow.Location + CoordTransformationHelper.HALF_VECT) * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT) - displayOffset;
                List<SpriteInst> entitySprites = new List<SpriteInst>();
                foreach (AnimationPlayer animation in entity.CurrentAnimations)
                {
                    foreach (int spriteIdx in animation.CurrentFrame)
                    {
                        entitySprites.Add(new SpriteInst()
                        {
                            Index = entity.ElementType.SpritePalette.Index,
                            DisplayCoords = displayRect.Location + entity.ElementType.SpritePalette.GetOffset(spriteIdx),
                            Section = entity.ElementType.SpritePalette.GetSection(spriteIdx)
                        });
                    }
                }

                StartLocation entityAsStartLoc = entity as StartLocation;
                retList.Add(new ObjectInst()
                {
                    Owner = entityAsStartLoc != null
                          ? (PlayerEnum)entityAsStartLoc.PlayerIndex
                          : (entity.Owner != null ? (PlayerEnum)entity.Owner.PlayerIndex : PlayerEnum.Neutral),
                    Sprites = entitySprites
                });
            }

            /// Display the currently visible entity snapshots.
            foreach (EntitySnapshot entitySnapshot in this.fogOfWarBC.GetEntitySnapshotsToUpdate(quadTileWindow))
            {
                RCIntRectangle displayRect =
                    (RCIntRectangle)((entitySnapshot.Position - cellWindow.Location + CoordTransformationHelper.HALF_VECT) * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT) - displayOffset;
                List<SpriteInst> snapshotSprites = new List<SpriteInst>();
                foreach (int spriteIdx in entitySnapshot.AnimationFrame)
                {
                    snapshotSprites.Add(new SpriteInst()
                    {
                        Index = entitySnapshot.EntityType.SpritePalette.Index,
                        DisplayCoords = displayRect.Location + entitySnapshot.EntityType.SpritePalette.GetOffset(spriteIdx),
                        Section = entitySnapshot.EntityType.SpritePalette.GetSection(spriteIdx)
                    });
                }
                retList.Add(new ObjectInst()
                {
                    Owner = entitySnapshot.Owner,
                    Sprites = snapshotSprites
                });
            }

            return retList;
        }

        /// <see cref="IMapObjectView.GetMapObjectID"/>
        public int GetMapObjectID(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(position)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            /// Calculate the currently visible window of cells and quadratic tiles.
            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);
            RCIntRectangle quadTileWindow = this.Map.CellToQuadRect(cellWindow);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            foreach (Entity entity in this.Scenario.GetEntitiesOnMap<Entity>(navCellCoords))
            {
                /// Get the ID of the entity only if it's not hidden by FOW.
                if (this.fogOfWarBC.IsEntityVisible(quadTileWindow, entity))
                {
                    return entity.ID.Read();
                }
            }
            return -1;
        }

        #endregion IMapObjectView methods

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private IFogOfWarBC fogOfWarBC;
    }
}
