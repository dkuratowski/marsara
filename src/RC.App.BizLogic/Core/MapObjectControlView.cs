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
        public MapObjectControlView(Scenario scenario)
            : base(scenario.Map)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            this.scenario = scenario;

            /// PROTOTYPE CODE
            this.gameManager = ComponentManager.GetInterface<IMultiplayerGameManager>();
        }

        #region IMapObjectView methods

        /// <see cref="IMapObjectControlView.LeftClick"/>
        public void LeftClick(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: implement!
        }

        /// <see cref="IMapObjectControlView.RightClick"/>
        public void RightClick(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: This is a PROTOTYPE CODE that acts on currently visible entities.
            ///       Update this method after the selection management is implemented!
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            int[] commandRecipients = new int[5] { 49, 50, 51, 52, 53 };
            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            this.gameManager.PostCommand(RCCommand.Create(MoveCommand.MNEMONIC, commandRecipients, navCellCoords, -1, null));
        }

        /// <see cref="IMapObjectControlView.DoubleClick"/>
        public void DoubleClick(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: implement!
        }

        /// <see cref="IMapObjectControlView.SelectionBox"/>
        public void SelectionBox(RCIntRectangle displayedArea, RCIntRectangle selectionBox)
        {
            /// TODO: implement!
        }

        #endregion IMapObjectView methods

        /// PROTOTYPE CODE
        private IMultiplayerGameManager gameManager;

        /// <summary>
        /// Reference to the scenario.
        /// </summary>
        private Scenario scenario;

        /// <summary>
        /// Constants for coordinate transformations.
        /// </summary>
        private static readonly RCNumVector HALF_VECT = new RCNumVector(1, 1) / 2;
    }
}
