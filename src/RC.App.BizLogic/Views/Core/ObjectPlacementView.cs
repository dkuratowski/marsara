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
        #region IObjectPlacementView members

        /// <see cref="IObjectPlacementView.GetObjectPlacementBox"/>
        public ObjectPlacementBox GetObjectPlacementBox(RCIntVector position)
        {
            /// Calculate the top-left quadratic coordinates based on the retrieved object rectangles relative to the quadratic tile at the incoming position.
            RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> objRectsRelativeToQuadTileAtPos = this.GetObjectRelativeQuadRectangles();
            if (objRectsRelativeToQuadTileAtPos.Count == 0)
            {
                return new ObjectPlacementBox
                {
                    Sprites = new List<SpriteRenderInfo>(),
                    IllegalParts = new List<RCIntRectangle>(),
                    LegalParts = new List<RCIntRectangle>()
                };
            }
            RCIntVector navCellCoords = this.MapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IQuadTile quadTileAtPos = this.Map.GetCell(navCellCoords).ParentQuadTile;
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords;
            foreach (Tuple<RCIntRectangle, SpriteRenderInfo[]> relativeRect in objRectsRelativeToQuadTileAtPos)
            {
                RCIntVector rectTopLeftQuadCoords = topLeftQuadCoords + relativeRect.Item1.Location;
                if (rectTopLeftQuadCoords.X < topLeftQuadCoords.X && rectTopLeftQuadCoords.Y < topLeftQuadCoords.Y ||
                    rectTopLeftQuadCoords.X < topLeftQuadCoords.X && rectTopLeftQuadCoords.Y == topLeftQuadCoords.Y ||
                    rectTopLeftQuadCoords.X == topLeftQuadCoords.X && rectTopLeftQuadCoords.Y < topLeftQuadCoords.Y)
                {
                    topLeftQuadCoords = rectTopLeftQuadCoords;
                }
            }

            /// Calculate the object rectangles relative to the calculated top-left quadratic coordinates.
            RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> objRectsRelativeToTopLeftQuadTile = new RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>>();
            foreach (Tuple<RCIntRectangle, SpriteRenderInfo[]> relativeRect in objRectsRelativeToQuadTileAtPos)
            {
                objRectsRelativeToTopLeftQuadTile.Add(Tuple.Create(
                    new RCIntRectangle(
                        relativeRect.Item1.Location + quadTileAtPos.MapCoords - topLeftQuadCoords,
                        relativeRect.Item1.Size),
                    relativeRect.Item2));
            }

            /// Get the sprites to be displayed, translate their DisplayCoordinates accordingly from the top-left quadratic tile,
            /// and collect the violating quadratic coordinates.
            ObjectPlacementBox placementBox = new ObjectPlacementBox
            {
                Sprites = new List<SpriteRenderInfo>(),
                IllegalParts = new List<RCIntRectangle>(),
                LegalParts = new List<RCIntRectangle>()
            };
            RCSet<RCIntVector> violatingQuadCoords = this.CheckObjectConstraints(topLeftQuadCoords);
            foreach (Tuple<RCIntRectangle, SpriteRenderInfo[]> relativeRect in objRectsRelativeToTopLeftQuadTile)
            {
                RCIntVector topLeftDisplayCoords =
                    this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(topLeftQuadCoords + relativeRect.Item1.Location, new RCIntVector(1, 1))).Location;
                for (int i = 0; i < relativeRect.Item2.Length; i++)
                {
                    relativeRect.Item2[i].DisplayCoords += topLeftDisplayCoords;
                    placementBox.Sprites.Add(relativeRect.Item2[i]);
                }
                for (int x = relativeRect.Item1.Left; x < relativeRect.Item1.Right; x++)
                {
                    for (int y = relativeRect.Item1.Top; y < relativeRect.Item1.Bottom; y++)
                    {
                        RCIntVector relativeQuadCoords = new RCIntVector(x, y);
                        RCIntVector absQuadCoords = topLeftQuadCoords + relativeQuadCoords;
                        RCIntRectangle partRect =
                            this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(absQuadCoords, new RCIntVector(1, 1)));
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
            }

            return placementBox;
        }

        /// <see cref="IObjectPlacementView.StepPreviewAnimation"/>
        public virtual void StepPreviewAnimation() { }

        #endregion IObjectPlacementView members

        /// <summary>
        /// Constructs an ObjectPlacementView instance.
        /// </summary>
        protected ObjectPlacementView()
        {
        }

        /// <summary>
        /// Gets the quadratic rectangles of the object to be placed relative to the quadratic tile pointed by the mouse pointer and
        /// the corresponding sprites to be displayed.
        /// </summary>
        /// <returns>
        /// The quadratic rectangles of the object to be placed relative to the quadratic tile pointed by the mouse pointer and the
        /// corresponding sprites to be displayed.
        /// </returns>
        protected abstract RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> GetObjectRelativeQuadRectangles();

        /// <summary>
        /// Collects all the quadratic coordinates that violate the placement constraints of the object
        /// if it were placed to the given position on the map.
        /// </summary>
        /// <param name="topLeftQuadCoords">The target quadratic position of the top-left corner of the object.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the placement constraints
        /// of the object at the given position on the map.
        /// </returns>
        protected abstract RCSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftQuadCoords);

        ///// <summary>
        ///// Gets the sprites of the object to be displayed.
        ///// </summary>
        ///// <returns>
        ///// A list of sprites with coordinates relative to the top left corner of the area of the object.
        ///// </returns>
        //protected abstract SpriteRenderInfo[] GetObjectSprites();
    }
}
