using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views for providing detailed informations about the local player.
    /// </summary>
    class PlayerView : MapViewBase, IPlayerView
    {
        /// <summary>
        /// Constructs a PlayerView instance.
        /// </summary>
        public PlayerView()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
        }

        #region IPlayerView members

        /// <see cref="IPlayerView.Minerals"/>
        public int Minerals
        {
            get
            {
                if (this.Scenario == null) { throw new InvalidOperationException("No active scenario!"); }
                return (int)this.Scenario.GetPlayer((int)this.selectionManager.LocalPlayer).Minerals;
            }
        }

        /// <see cref="IPlayerView.VespeneGas"/>
        public int VespeneGas
        {
            get
            {
                if (this.Scenario == null) { throw new InvalidOperationException("No active scenario!"); }
                return (int)this.Scenario.GetPlayer((int)this.selectionManager.LocalPlayer).VespeneGas;
            }
        }

        /// <see cref="IPlayerView.UsedSupply"/>
        public int UsedSupply
        {
            get
            {
                if (this.Scenario == null) { throw new InvalidOperationException("No active scenario!"); }
                return this.Scenario.GetPlayer((int)this.selectionManager.LocalPlayer).UsedSupply;
            }
        }

        /// <see cref="IPlayerView.TotalSupply"/>
        public int TotalSupply
        {
            get
            {
                if (this.Scenario == null) { throw new InvalidOperationException("No active scenario!"); }
                return this.Scenario.GetPlayer((int)this.selectionManager.LocalPlayer).TotalSupply;
            }
        }

        /// <see cref="IPlayerView.MaxSupply"/>
        public int MaxSupply
        {
            get
            {
                if (this.Scenario == null) { throw new InvalidOperationException("No active scenario!"); }
                return this.Scenario.GetPlayer((int)this.selectionManager.LocalPlayer).MaxSupply;
            }
        }
    
        #endregion IPlayerView members

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;
    }
}
