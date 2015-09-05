using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// View for providing detailed informations about the current selection.
    /// </summary>
    class SelectionDetailsView : ISelectionDetailsView
    {
        /// <summary>
        /// Constructs a SelectionDetailsView instance.
        /// </summary>
        public SelectionDetailsView()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
        }

        #region ISelectionDetailsView methods

        /// <see cref="ISelectionDetailsView.SelectionCount"/>
        public int SelectionCount { get { return this.selectionManager.CurrentSelection.Count; } }

        /// <see cref="ISelectionDetailsView.GetObjectID"/>
        public int GetObjectID(int selectionOrdinal)
        {
            int[] currentSelectionArray = this.selectionManager.CurrentSelection.ToArray();
            return currentSelectionArray[selectionOrdinal];
        }

        #endregion ISelectionDetailsView methods

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private readonly ISelectionManagerBC selectionManager;
    }
}
