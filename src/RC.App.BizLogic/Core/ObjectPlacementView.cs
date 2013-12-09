using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Common base class of object placement views.
    /// </summary>
    abstract class ObjectPlacementView : MapViewBase, IObjectPlacementView
    {
        /// <summary>
        /// Constructs an ObjectPlacementView instance.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        public ObjectPlacementView(IMapAccess map)
            : base(map)
        {
        }

        #region IObjectPlacementView members

        /// <see cref="IObjectPlacementView.GetObjectPlacementBox"/>
        public ObjectPlacementBox GetObjectPlacementBox(RCIntRectangle displayedArea, RCIntVector position)
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
            IQuadTile quadTileAtPos = this.Map.GetCell(navCellCoords).ParentQuadTile;
            RCIntVector objectQuadraticSize = this.GetObjectQuadraticSize();
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objectQuadraticSize / 2;

            List<MapSpriteInstance> spritesToDisplay = this.GetObjectSprites();
            RCIntVector topLeftDisplayCoords =
                    (this.Map.QuadToCellRect(new RCIntRectangle(topLeftQuadCoords, new RCIntVector(1, 1))).Location - cellWindow.Location)
                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                  - displayOffset;
            for (int i = 0; i < spritesToDisplay.Count; i++)
            {
                spritesToDisplay[i] = new MapSpriteInstance()
                {
                    Index = spritesToDisplay[i].Index,
                    DisplayCoords = topLeftDisplayCoords + spritesToDisplay[i].DisplayCoords,
                    Section = spritesToDisplay[i].Section
                };
            }

            ObjectPlacementBox placementBox = new ObjectPlacementBox()
            {
                Sprites = spritesToDisplay,
                IllegalParts = new List<RCIntRectangle>(),
                LegalParts = new List<RCIntRectangle>()
            };

            HashSet<RCIntVector> violatingQuadCoords = this.CheckObjectConstraints(topLeftQuadCoords);
            for (int x = 0; x < objectQuadraticSize.X; x++)
            {
                for (int y = 0; y < objectQuadraticSize.Y; y++)
                {
                    RCIntVector relativeQuadCoords = new RCIntVector(x, y);
                    RCIntVector absQuadCoords = topLeftQuadCoords + relativeQuadCoords;
                    RCIntRectangle partRect = (this.Map.QuadToCellRect(new RCIntRectangle(absQuadCoords, new RCIntVector(1, 1))) - cellWindow.Location)
                                            * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                            - displayOffset;
                    if (violatingQuadCoords.Contains(relativeQuadCoords))
                    {
                        placementBox.IllegalParts.Add(partRect);
                    }
                    else
                    {
                        placementBox.LegalParts.Add(partRect);
                    }
                }
            }

            return placementBox;
        }

        /// <see cref="IObjectPlacementView.StepAnimation"/>
        public virtual void StepAnimation() { }

        #endregion IObjectPlacementView members

        /// <summary>
        /// Gets the quadratic size of the object to be placed.
        /// </summary>
        /// <returns>The quadratic size of the object to be placed.</returns>
        protected abstract RCIntVector GetObjectQuadraticSize();

        /// <summary>
        /// Collects all the quadratic coordinates that violate the placement constraints of the object
        /// if it were placed to the given position on the map.
        /// </summary>
        /// <param name="topLeftCoords">The target quadratic position of the top-left corner of the object.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the placement constraints
        /// of the object at the given position on the map.
        /// </returns>
        protected abstract HashSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftCoords);

        /// <summary>
        /// Gets the sprites of the object to be displayed.
        /// </summary>
        /// <returns>
        /// A list of sprites with coordinates relative to the top left corner of the area of the object.
        /// </returns>
        protected abstract List<MapSpriteInstance> GetObjectSprites();
    }
}
