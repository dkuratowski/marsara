using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
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
    /// Implementation of views on the selection indicators of selected objects.
    /// </summary>
    class SelectionIndicatorView : MapViewBase, ISelectionIndicatorView
    {
        /// <summary>
        /// Constructs a SelectionIndicatorView instance.
        /// </summary>
        public SelectionIndicatorView()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
        }

        #region ISelectionIndicatorView

        /// <see cref="ISelectionIndicatorView.GetVisibleSelIndicators"/>
        public List<SelIndicatorInst> GetVisibleSelIndicators(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            HashSet<int> currentSelection = this.selectionManager.CurrentSelection;
            if (currentSelection.Count == 0) { return new List<SelIndicatorInst>(); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<SelIndicatorInst> retList = new List<SelIndicatorInst>();
            HashSet<Entity> visibleEntities = this.Scenario.VisibleEntities.GetContents(
                new RCNumRectangle(cellWindow.X - CoordTransformationHelper.HALF_VECT.X,
                                   cellWindow.Y - CoordTransformationHelper.HALF_VECT.Y,
                                   cellWindow.Width,
                                   cellWindow.Height));
            foreach (Entity entity in visibleEntities)
            {
                if (currentSelection.Contains(entity.ID.Read()))
                {
                    SelIndicatorTypeEnum indicatorType
                        = entity.Owner != null
                        ? (entity.Owner.PlayerIndex == (int)this.selectionManager.LocalPlayer ? SelIndicatorTypeEnum.Friendly : SelIndicatorTypeEnum.Enemy)
                        : SelIndicatorTypeEnum.Neutral;
                    retList.Add(new SelIndicatorInst()
                    {
                        SelIndicatorType = indicatorType,
                        IndicatorRect = (RCIntRectangle)((entity.BoundingBox - cellWindow.Location + CoordTransformationHelper.HALF_VECT) * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT) - displayOffset,
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
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;
    }
}
