using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Common base class of object placement views.
    /// </summary>
    abstract class ObjectPlacementView : MapViewBase
    {
        /// <summary>
        /// Constructs an ObjectPlacementView instance.
        /// </summary>
        public ObjectPlacementView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region IObjectPlacementView members

        /// <see cref="IObjectPlacementView.GetObjectPlacementBox"/>
        public ObjectPlacementBox GetObjectPlacementBox(RCIntVector position)
        {
            RCIntVector navCellCoords = this.MapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IQuadTile quadTileAtPos = this.Map.GetCell(navCellCoords).ParentQuadTile;
            RCIntVector objectQuadraticSize = this.GetObjectQuadraticSize();
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objectQuadraticSize / 2;

            List<SpriteInst> spritesToDisplay = this.GetObjectSprites();
            RCIntVector topLeftDisplayCoords =
                this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(topLeftQuadCoords, new RCIntVector(1, 1))).Location;
            for (int i = 0; i < spritesToDisplay.Count; i++)
            {
                spritesToDisplay[i] = new SpriteInst()
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

            RCSet<RCIntVector> violatingQuadCoords = this.CheckObjectConstraints(topLeftQuadCoords);
            for (int x = 0; x < objectQuadraticSize.X; x++)
            {
                for (int y = 0; y < objectQuadraticSize.Y; y++)
                {
                    RCIntVector relativeQuadCoords = new RCIntVector(x, y);
                    RCIntVector absQuadCoords = topLeftQuadCoords + relativeQuadCoords;
                    RCIntRectangle partRect =
                        this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(absQuadCoords, new RCIntVector(1, 1)));
                    if (violatingQuadCoords.Contains(relativeQuadCoords) || this.fogOfWarBC.GetFullFowTileFlags(absQuadCoords).HasFlag(FOWTileFlagsEnum.Current))
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

        /// <see cref="IObjectPlacementView.StepPreviewAnimation"/>
        public virtual void StepPreviewAnimation() { }

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
        protected abstract RCSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftCoords);

        /// <summary>
        /// Gets the sprites of the object to be displayed.
        /// </summary>
        /// <returns>
        /// A list of sprites with coordinates relative to the top left corner of the area of the object.
        /// </returns>
        protected abstract List<SpriteInst> GetObjectSprites();

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
