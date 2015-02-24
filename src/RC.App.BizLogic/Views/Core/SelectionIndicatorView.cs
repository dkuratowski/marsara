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
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region ISelectionIndicatorView

        /// <see cref="ISelectionIndicatorView.GetVisibleSelIndicators"/>
        public List<SelIndicatorInst> GetVisibleSelIndicators()
        {
            HashSet<int> currentSelection = this.selectionManager.CurrentSelection;
            if (currentSelection.Count == 0) { return new List<SelIndicatorInst>(); }

            /// Display the selection indicators of the currently visible entities inside the currently visible window of quadratic tiles.
            List<SelIndicatorInst> retList = new List<SelIndicatorInst>();
            foreach (Entity entity in this.fogOfWarBC.GetEntitiesToUpdate())
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
                        IndicatorRect = this.MapWindowBC.AttachedWindow.MapToWindowRect(entity.BoundingBox),
                        HpNorm = (RCNumber)1, // TODO: must base on real data
                        ShieldNorm = (RCNumber)1, // TODO: must base on real data
                        EnergyNorm = (RCNumber)1 // TODO: must base on real data
                    });
                }
            }
            return retList;
        }

        /// <see cref="ISelectionIndicatorView.LocalPlayer"/>
        public PlayerEnum LocalPlayer { get { return this.selectionManager.LocalPlayer; } }

        #endregion ISelectionIndicatorView

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private IFogOfWarBC fogOfWarBC;
    }
}
