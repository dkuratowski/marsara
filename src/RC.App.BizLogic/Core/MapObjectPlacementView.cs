using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of object placement views for map objects.
    /// </summary>
    class MapObjectPlacementView : MapViewBase, IObjectPlacementView
    {
        /// <summary>
        /// Constructs a MapObjectPlacementView instance.
        /// </summary>
        /// <param name="objectType">Reference to the type of the map object being placed.</param>
        /// <param name="scenario">Reference to the scenario.</param>
        public MapObjectPlacementView(IScenarioElementType objectType, IScenario scenario)
            : base(scenario.Map)
        {
            if (objectType == null) { throw new ArgumentNullException("objectType"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }

            this.scenario = scenario;
            this.objectType = objectType;
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

            RCNumVector objectArea = this.objectType.Area.Read();
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - this.scenario.Map.CellToQuadSize(objectArea) / 2;

            MapSpriteInstance spriteInstance =
                new MapSpriteInstance()
                {
                    Index = this.objectType.SpritePalette.Index,
                    DisplayCoords = (this.Map.QuadToCellRect(new RCIntRectangle(topLeftQuadCoords, new RCIntVector(1, 1))).Location - cellWindow.Location)
                                  * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
                                  - displayOffset,
                    Section = RCIntRectangle.Undefined // TODO
                };

            ObjectPlacementBox placementBox = new ObjectPlacementBox()
            {
                Sprite = spriteInstance,
                IllegalParts = new List<RCIntRectangle>(),
                LegalParts = new List<RCIntRectangle>()
            };

            // TODO
            //HashSet<RCIntVector> violatingQuadCoords = this.terrainObjectType.CheckConstraints(this.Map, topLeftQuadCoords);
            //violatingQuadCoords.UnionWith(this.terrainObjectType.CheckTerrainObjectIntersections(this.Map, topLeftQuadCoords));
            //for (int x = 0; x < this.terrainObjectType.QuadraticSize.X; x++)
            //{
            //    for (int y = 0; y < this.terrainObjectType.QuadraticSize.Y; y++)
            //    {
            //        RCIntVector relativeQuadCoords = new RCIntVector(x, y);
            //        RCIntVector absQuadCoords = topLeftQuadCoords + relativeQuadCoords;
            //        RCIntRectangle partRect = (this.Map.QuadToCellRect(new RCIntRectangle(absQuadCoords, new RCIntVector(1, 1))) - cellWindow.Location)
            //                                * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL)
            //                                - displayOffset;
            //        if (violatingQuadCoords.Contains(relativeQuadCoords))
            //        {
            //            placementBox.IllegalParts.Add(partRect);
            //        }
            //        else
            //        {
            //            placementBox.LegalParts.Add(partRect);
            //        }
            //    }
            //}

            return placementBox;
        }

        #endregion IObjectPlacementView members

        /// <summary>
        /// Reference to the scenario.
        /// </summary>
        private IScenario scenario;

        /// <summary>
        /// Reference to the type of the map object being placed.
        /// </summary>
        private IScenarioElementType objectType;
    }
}
