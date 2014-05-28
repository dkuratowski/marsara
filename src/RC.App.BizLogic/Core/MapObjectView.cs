using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on the objects of the currently opened map.
    /// </summary>
    class MapObjectView : MapViewBase, IMapObjectView
    {
        /// <summary>
        /// Constructs a MapObjectView instance.
        /// </summary>
        /// <param name="scenario">The subject of this view.</param>
        public MapObjectView(Scenario scenario)
            : base(scenario.Map)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            this.scenario = scenario;
        }

        #region IMapObjectView methods

        /// <see cref="IMapObjectView.GetVisibleMapObjects"/>
        public List<ObjectInst> GetVisibleMapObjects(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<ObjectInst> retList = new List<ObjectInst>();
            HashSet<Entity> visibleEntities = this.scenario.VisibleEntities.GetContents(
                new RCNumRectangle(cellWindow.X - MapViewBase.HALF_VECT.X,
                                   cellWindow.Y - MapViewBase.HALF_VECT.Y,
                                   cellWindow.Width,
                                   cellWindow.Height));
            foreach (Entity entity in visibleEntities)
            {
                RCIntRectangle displayRect =
                    (RCIntRectangle)((entity.BoundingBox - cellWindow.Location + MapViewBase.HALF_VECT) * MapViewBase.PIXEL_PER_NAVCELL_VECT) - displayOffset;
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
            return retList;
        }

        /// <see cref="IMapObjectView.GetMapObjectID"/>
        public int GetMapObjectID(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(position)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            foreach (Entity entity in this.scenario.VisibleEntities.GetContents(navCellCoords))
            {
                return entity.ID.Read();
            }
            return -1;
        }

        #endregion IMapObjectView methods

        /// <summary>
        /// Reference to the scenario.
        /// </summary>
        private Scenario scenario;
    }
}
