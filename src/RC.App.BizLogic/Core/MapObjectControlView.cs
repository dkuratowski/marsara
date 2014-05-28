using RC.App.BizLogic.ComponentInterfaces;
using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views for controlling the objects of the currently opened map.
    /// </summary>
    class MapObjectControlView : MapViewBase, IMapObjectControlView
    {
        /// <summary>
        /// Constructs a MapObjectControlView instance.
        /// </summary>
        /// <param name="scenario">The subject of this view.</param>
        /// <param name="selector">The selector of the local player.</param>
        public MapObjectControlView(Scenario scenario, EntitySelector selector)
            : base(scenario.Map)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (selector == null) { throw new ArgumentNullException("selector"); }
            this.scenario = scenario;
            this.selector = selector;

            /// PROTOTYPE CODE
            this.gameManager = ComponentManager.GetInterface<IMultiplayerGameManager>();
        }

        #region IMapObjectControlView methods

        /// <see cref="IMapObjectControlView.LeftClick"/>
        public void LeftClick(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCNumVector mapCoords = new RCNumVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                    (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL) - HALF_VECT;
            this.selector.Select(mapCoords);
        }

        /// <see cref="IMapObjectControlView.RightClick"/>
        public void RightClick(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: This is a PROTOTYPE CODE that acts on currently visible entities.
            ///       Update this method after the selection management is implemented!
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            if (this.selector.CurrentSelection.Count != 0)
            {
                RCIntRectangle cellWindow;
                RCIntVector displayOffset;
                this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

                RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                            (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
                
                this.gameManager.PostCommand(RCCommand.Create(FastCommand.MNEMONIC, this.selector.CurrentSelection.ToArray(), navCellCoords, -1, null));
            }
        }

        /// <see cref="IMapObjectControlView.DoubleClick"/>
        public void DoubleClick(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: implement!
        }

        /// <see cref="IMapObjectControlView.SelectionBox"/>
        public void SelectionBox(RCIntRectangle displayedArea, RCIntRectangle selectionBox)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCNumRectangle mapCoords =
                new RCNumRectangle(
                    new RCNumVector((displayedArea + selectionBox.Location).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                    (displayedArea + selectionBox.Location).Y / BizLogicConstants.PIXEL_PER_NAVCELL) - HALF_VECT,
                    new RCNumVector((RCNumber)selectionBox.Size.X / (RCNumber)BizLogicConstants.PIXEL_PER_NAVCELL,
                                    (RCNumber)selectionBox.Size.Y / (RCNumber)BizLogicConstants.PIXEL_PER_NAVCELL));
            this.selector.Select(mapCoords);
        }

        #endregion IMapObjectControlView methods

        /// PROTOTYPE CODE
        private IMultiplayerGameManager gameManager;

        /// <summary>
        /// Reference to the scenario.
        /// </summary>
        private Scenario scenario;

        /// <summary>
        /// Reference to the entity selector of the local player.
        /// </summary>
        private EntitySelector selector;
    }
}
