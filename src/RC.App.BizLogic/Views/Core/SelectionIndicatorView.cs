using RC.Common;
using System.Collections.Generic;
using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

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
        public List<SelIndicatorRenderInfo> GetVisibleSelIndicators()
        {
            RCSet<int> currentSelection = this.selectionManager.CurrentSelection;
            if (currentSelection.Count == 0) { return new List<SelIndicatorRenderInfo>(); }

            /// Display the selection indicators of the currently visible entities inside the currently visible window of quadratic tiles.
            List<SelIndicatorRenderInfo> retList = new List<SelIndicatorRenderInfo>();
            foreach (MapObject mapObject in this.fogOfWarBC.GetAllMapObjectsToUpdate())
            {
                Entity entity = mapObject.Owner as Entity;
                if (entity != null && currentSelection.Contains(entity.ID.Read()))
                {
                    SelIndicatorTypeEnum indicatorType
                        = entity.Owner != null
                        ? (entity.Owner.PlayerIndex == (int)this.selectionManager.LocalPlayer ? SelIndicatorTypeEnum.Friendly : SelIndicatorTypeEnum.Enemy)
                        : SelIndicatorTypeEnum.Neutral;
                    RCNumber hpNorm = entity.Biometrics.HP != -1
                        ? entity.Biometrics.HP/entity.ElementType.MaxHP.Read()
                        : -1;

                    RCNumber energyNorm = -1;
                    if (entity.Owner != null && entity.Owner.PlayerIndex == (int)this.selectionManager.LocalPlayer)
                    {
                        energyNorm = entity.Biometrics.Energy != -1
                                   ? entity.Biometrics.Energy/entity.ElementType.MaxEnergy.Read()
                                   : -1;
                    }

                    retList.Add(new SelIndicatorRenderInfo()
                    {
                        ObjectID = entity.ID.Read(),
                        SelIndicatorType = indicatorType,
                        IndicatorRect = this.MapWindowBC.AttachedWindow.MapToWindowRect(entity.Area),
                        HpNormalized = hpNorm,
                        EnergyNormalized = energyNorm,
                        ShieldNormalized = -1, // TODO: must be based on real data after Protoss will have been implemented!
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
        private readonly ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
