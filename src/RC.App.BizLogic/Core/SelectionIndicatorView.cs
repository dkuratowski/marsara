using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on the selection indicators of selected objects.
    /// </summary>
    class SelectionIndicatorView : MapViewBase, ISelectionIndicatorView
    {
        /// <summary>
        /// Constructs a SelectionIndicatorView instance.
        /// </summary>
        /// <param name="selector">The subject of this view.</param>
        public SelectionIndicatorView(EntitySelector selector)
            : base(selector.TargetScenario.Map)
        {
            if (selector == null) { throw new ArgumentNullException("selector"); }
            this.selector = selector;
        }

        #region ISelectionIndicatorView

        /// <see cref="ISelectionIndicatorView.GetVisibleSelIndicators"/>
        public List<SelIndicatorInst> GetVisibleSelIndicators(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            HashSet<int> currentSelection = this.selector.CurrentSelection;
            if (currentSelection.Count == 0) { return new List<SelIndicatorInst>(); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<SelIndicatorInst> retList = new List<SelIndicatorInst>();
            HashSet<Entity> visibleEntities = this.selector.TargetScenario.VisibleEntities.GetContents(
                new RCNumRectangle(cellWindow.X - MapViewBase.HALF_VECT.X,
                                   cellWindow.Y - MapViewBase.HALF_VECT.Y,
                                   cellWindow.Width,
                                   cellWindow.Height));
            foreach (Entity entity in visibleEntities)
            {
                if (currentSelection.Contains(entity.ID.Read()))
                {
                    SelIndicatorTypeEnum indicatorType
                        = entity.Owner != null
                        ? (entity.Owner.PlayerIndex == (int)this.selector.Owner ? SelIndicatorTypeEnum.Friendly : SelIndicatorTypeEnum.Enemy)
                        : SelIndicatorTypeEnum.Neutral;
                    retList.Add(new SelIndicatorInst()
                    {
                        SelIndicatorType = indicatorType,
                        IndicatorRect = (RCIntRectangle)((entity.BoundingBox - cellWindow.Location + MapViewBase.HALF_VECT) * MapViewBase.PIXEL_PER_NAVCELL_VECT) - displayOffset,
                        HpNorm = (RCNumber)1, // TODO: must base on real data
                        ShieldNorm = (RCNumber)1, // TODO: must base on real data
                        EnergyNorm = (RCNumber)1 // TODO: must base on real data
                    });
                }
            }
            return retList;
        }

        #endregion ISelectionIndicatorView

        /// <summary>
        /// Reference to the entity selector.
        /// </summary>
        private EntitySelector selector;
    }
}
