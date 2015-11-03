using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.App.PresLogic.SpriteGroups;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Internal class for handling mouse events in select target input mode.
    /// </summary>
    class SelectTargetMouseHandler : MouseHandlerBase
    {
        /// <summary>
        /// Constructs a SelectTargetMouseHandler instance.
        /// </summary>
        /// <param name="scrollEventSource">The UISensitiveObject that will raise the events for scrolling.</param>
        /// <param name="mapDisplay">Reference to the target map display.</param>
        /// <param name="normalMouseEventSource">The UISensitiveObject that will raise the additional mouse events.</param>
        /// <param name="sprites">The sprites to be used when displaying object placement box.</param>
        public SelectTargetMouseHandler(UISensitiveObject scrollEventSource, IMapDisplay mapDisplay,
                                        UISensitiveObject normalMouseEventSource, SpriteGroup sprites)
            : base(scrollEventSource, mapDisplay)
        {
            if (normalMouseEventSource == null) { throw new ArgumentNullException("normalMouseEventSource"); }
            if (sprites == null) { throw new ArgumentNullException("sprites"); }
            if (this.CommandView.TargetSelectionMode == TargetSelectionModeEnum.NoTargetSelection) { throw new InvalidOperationException("Target selection is not possible currently!"); }

            this.targetSelectionMode = this.CommandView.TargetSelectionMode;
            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();
            this.normalMouseEventSource = normalMouseEventSource;
            this.objectPlacementInfo = this.targetSelectionMode == TargetSelectionModeEnum.BuildingLocationSelection
                ? new ObjectPlacementInfo(
                    ComponentManager.GetInterface<IViewService>().CreateView<INormalModeMapObjectPlacementView>(),
                    sprites)
                : null;

            this.normalMouseEventSource.MouseSensor.ButtonDown += this.OnMouseDown;
            this.multiplayerService.GameUpdated += this.OnGameUpdate;
        }

        #region Overrides from MouseHandlerBase

        /// <see cref="MouseHandlerBase.DisplayCrosshairs"/>
        public override bool DisplayCrosshairs
        {
            get { return this.CommandView.TargetSelectionMode == TargetSelectionModeEnum.TargetPositionSelection; }
        }

        /// <see cref="MouseHandlerBase.ObjectPlacementInfo"/>
        public override ObjectPlacementInfo ObjectPlacementInfo { get { return this.objectPlacementInfo; } }

        /// <see cref="MouseHandlerBase.Inactivate_i"/>
        protected override void Inactivate_i()
        {
            this.multiplayerService.GameUpdated -= this.OnGameUpdate;
            this.normalMouseEventSource.MouseSensor.ButtonDown -= this.OnMouseDown;
        }

        #endregion Overrides from MouseHandlerBase

        /// <summary>
        /// Called when a mouse button has been pushed over the display.
        /// </summary>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (!this.IsStillValid()) { return; }
            if (evtArgs.Button == UIMouseButton.Right)
            {
                /// Target selection cancel.
                TraceManager.WriteAllTrace(string.Format("CANCEL_SELECT_TARGET {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                this.CommandService.CancelSelectingTargetPosition();
            }
            else if (evtArgs.Button == UIMouseButton.Left)
            {
                /// Target selection complete
                TraceManager.WriteAllTrace(string.Format("SELECT_TARGET {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                this.CommandService.SelectTargetPosition(evtArgs.Position);
            }
        }

        /// <summary>
        /// This method is called on every frame updates and invalidates this mouse handler if necessary.
        /// </summary>
        private void OnGameUpdate()
        {
            if (this.objectPlacementInfo != null) { this.objectPlacementInfo.View.StepPreviewAnimation(); }
            if (!this.IsStillValid()) { this.Inactivate(); }
        }

        /// <summary>
        /// Checks whether this mouse handler is still valid.
        /// </summary>
        /// <returns>True if this mouse handler is still valid; otherwise false.</returns>
        private bool IsStillValid()
        {
            return this.CommandView.TargetSelectionMode == this.targetSelectionMode;
        }

        /// <summary>
        /// The event source for additional mouse events.
        /// </summary>
        private readonly UISensitiveObject normalMouseEventSource;

        /// <summary>
        /// Reference to the informations about the currently active object placement operation or null if no object placement operation is active.
        /// </summary>
        private readonly ObjectPlacementInfo objectPlacementInfo;

        /// <summary>
        /// Reference to the multiplayer service.
        /// </summary>
        private readonly IMultiplayerService multiplayerService;

        /// <summary>
        /// The target selection mode that was active when this mouse handler was created.
        /// </summary>
        private readonly TargetSelectionModeEnum targetSelectionMode;
    }
}
