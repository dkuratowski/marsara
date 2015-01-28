using System;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the map window business component.
    /// </summary>
    [Component("RC.App.BizLogic.MapWindowBC")]
    class MapWindowBC : ScenarioDependentComponent, IMapWindowBC
    {
        /// <summary>
        /// Constructs a MapWindowBC instance.
        /// </summary>
        public MapWindowBC()
        {
            this.attachedWindow = null;
            this.fullWindow = null;
            this.desiredCenterPositionOfAttachedWindow = RCNumVector.Undefined;
        }

        #region Overrides from ScenarioDependentComponent

        /// <see cref="ScenarioDependentComponent.OnActiveScenarioChanged"/>
        protected override void OnActiveScenarioChanged(Scenario activeScenario)
        {
            if (this.attachedWindow != null)
            {
                this.attachedWindow.Dispose();
                this.attachedWindow = null;
            }

            if (this.fullWindow != null)
            {
                this.fullWindow.Dispose();
                this.fullWindow = null;
            }

            if (activeScenario != null)
            {
                this.desiredCenterPositionOfAttachedWindow = new RCNumVector(0, 0);
                this.fullWindow = new FullMapWindow(activeScenario);
            }
        }

        #endregion Overrides from ScenarioDependentComponent

        #region IMapWindowBC methods

        /// <see cref="IMapWindowBC.AttachWindow"/>
        public void AttachWindow(RCIntVector windowPixelSize)
        {
            if (windowPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("windowPixelSize"); }
            if (this.ActiveScenario == null) { throw new InvalidOperationException("There is no active scenario!"); }
            if (this.attachedWindow != null) { throw new InvalidOperationException("Window already attached!"); }

            this.attachedWindow = new PartialMapWindow(this.ActiveScenario, this.desiredCenterPositionOfAttachedWindow, windowPixelSize);
        }

        /// <see cref="IMapWindowBC.AttachedWindow"/>
        public IMapWindow AttachedWindow { get { return this.attachedWindow; } }

        /// <see cref="IMapWindowBC.FullWindow"/>
        public IMapWindow FullWindow { get { return this.fullWindow; } }

        #endregion IMapWindowBC methods

        /// <summary>
        /// Reference to the currently attached window.
        /// </summary>
        private PartialMapWindow attachedWindow;

        /// <summary>
        /// Reference to the current full window.
        /// </summary>
        private FullMapWindow fullWindow;

        /// <summary>
        /// The desired center position of the window when it will be attached or RCNumVector.Undefined if there is no active scenario
        /// or if the window has already been attached.
        /// </summary>
        private RCNumVector desiredCenterPositionOfAttachedWindow;
    }
}
