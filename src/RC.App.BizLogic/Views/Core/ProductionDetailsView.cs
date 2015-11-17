using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views for providing production details of the currently selected map object.
    /// </summary>
    class ProductionDetailsView : MapViewBase, IProductionDetailsView
    {
        /// <summary>
        /// Constructs a production line view.
        /// </summary>
        public ProductionDetailsView()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();
        }

        #region IProductionLineView members

        /// <see cref="IProductionDetailsView.ProductionLineCapacity"/>
        public int ProductionLineCapacity
        {
            get
            {
                ProductionLine activeProductionLine = this.GetActiveProductionLine();
                return activeProductionLine != null ? activeProductionLine.Capacity : 0;
            }
        }

        /// <see cref="IProductionDetailsView.ProductionLineItemCount"/>
        public int ProductionLineItemCount
        {
            get
            {
                ProductionLine activeProductionLine = this.GetActiveProductionLine();
                return activeProductionLine != null ? activeProductionLine.ItemCount : 0;
            }
        }

        /// <see cref="IProductionDetailsView.ProductionLineProgressNormalized"/>
        public RCNumber ProductionLineProgressNormalized
        {
            get
            {
                ProductionLine activeProductionLine = this.GetActiveProductionLine();
                if (activeProductionLine == null) { return -1; }

                int progress = activeProductionLine.Progress;
                int totalBuildTime = activeProductionLine.GetProduct(0).BuildTime.Read();
                return (RCNumber)progress/(RCNumber)totalBuildTime;
            }
        }

        /// <see cref="IProductionDetailsView.ConstructionProgressNormalized"/>
        public RCNumber ConstructionProgressNormalized
        {
            get
            {
                Entity selectedEntity = this.GetSelectedEntity();
                if (selectedEntity == null || !selectedEntity.Biometrics.IsUnderConstruction) { return -1; }

                int progress = selectedEntity.Biometrics.ConstructionProgress;
                int totalBuildTime = selectedEntity.ElementType.BuildTime.Read();
                return (RCNumber)progress / (RCNumber)totalBuildTime;
            }
        }

        /// <see cref="IProductionDetailsView.GetProductionJobIcon"/>
        public SpriteRenderInfo GetProductionJobIcon(int itemIndex)
        {
            ProductionLine activeProductionLine = this.GetActiveProductionLine();
            if (activeProductionLine == null) { throw new InvalidOperationException("Active production line of the current selection cannot be accessed!"); }

            IScenarioElementType job = activeProductionLine.GetProduct(itemIndex);
            if (job == null) { throw new InvalidOperationException(string.Format("There is no production job at index {0} int the active production line of the current selection!", itemIndex)); }

            /// Retrieve the icon of the job from the CommandManagerBC!
            return this.commandManager.GetProductButtonSprite(job.Name);
        }

        #endregion IProductionLineView members

        /// <summary>
        /// Gets the active production line of the currently selected entity.
        /// </summary>
        /// <returns>
        /// The active production line of the currently selected entity or null in the following cases:
        ///     - There is no selected entity.
        ///     - More than 1 entities are selected.
        ///     - The selected entity has no active production line.
        ///     - The owner of the selected entity is not the local player.
        /// </returns>
        private ProductionLine GetActiveProductionLine()
        {
            /// Get the currently selected entity.
            Entity selectedEntity = this.GetSelectedEntity();

            /// Return the active production line of the selected entity.
            return selectedEntity != null ? selectedEntity.ActiveProductionLine : null;
        }

        /// <summary>
        /// Gets the currently selected entity.
        /// </summary>
        /// <returns>
        /// The currently selected entity or null in the following cases:
        ///     - There is no selected entity.
        ///     - More than 1 entities are selected.
        ///     - The owner of the selected entity is not the local player.
        /// </returns>
        private Entity GetSelectedEntity()
        {
            /// Check if exactly 1 entity is selected.
            int[] currentSelection = this.selectionManager.CurrentSelection.ToArray();
            if (currentSelection.Length != 1) { return null; }

            /// Check if the owner of the selected entity is the local player.
            Entity selectedEntity = this.GetEntity(currentSelection[0]);
            if (selectedEntity.Owner == null || selectedEntity.Owner.PlayerIndex != (int)this.selectionManager.LocalPlayer) { return null; }

            return selectedEntity;
        }

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private readonly ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private readonly ICommandManagerBC commandManager;
    }
}
