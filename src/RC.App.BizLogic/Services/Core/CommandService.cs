﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Commands;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// The implementation of the command service.
    /// </summary>
    [Component("RC.App.BizLogic.CommandService")]
    class CommandService : ICommandService, IComponent
    {
        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();

            /// TODO: Temporary reference for sending fast commands!
            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region ICommandService methods

        /// <see cref="ICommandService.SendFastCommand"/>
        public void SendFastCommand(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            this.commandManager.SendFastCommand(this.mapWindowBC.AttachedWindow.WindowToMapCoords(position));

            /// TODO: This is a PROTOTYPE CODE!
            //if (this.selectionManager.CurrentSelection.Count != 0)
            //{
            //    this.multiplayerService.PostCommand(new RCCommand(null,
            //                                                      this.selectionManager.CurrentSelection.ToArray(),
            //                                                      this.mapWindowBC.AttachedWindow.WindowToMapCoords(position),
            //                                                      -1,
            //                                                      null));
            //}
        }

        /// <see cref="ICommandService.SendFastCommandOnMinimap"/>
        public void SendFastCommandOnMinimap(RCIntVector position)
        {
            /// TODO: This is a PROTOTYPE CODE!
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.selectionManager.LocalPlayer == PlayerEnum.Neutral) { return; }

            RCIntVector minimapPixelCoords = position - this.mapWindowBC.Minimap.MinimapPosition.Location;
            minimapPixelCoords = new RCIntVector(Math.Min(this.mapWindowBC.Minimap.MinimapPosition.Width - 1, Math.Max(0, minimapPixelCoords.X)),
                                                 Math.Min(this.mapWindowBC.Minimap.MinimapPosition.Height - 1, Math.Max(0, minimapPixelCoords.Y)));

            IMinimapPixel minimapPixel = this.mapWindowBC.Minimap.GetMinimapPixel(minimapPixelCoords);
            RCNumVector pixelCenterOnMap = minimapPixel.CoveredArea.Location + minimapPixel.CoveredArea.Size / 2;

            if (this.selectionManager.CurrentSelection.Count != 0)
            {
                this.multiplayerService.PostCommand(new RCCommand(null,
                                                                  this.selectionManager.CurrentSelection.ToArray(),
                                                                  pixelCenterOnMap,
                                                                  -1,
                                                                  null));
            }
        }

        /// <see cref="ICommandService.PressCommandButton"/>
        public void PressCommandButton(RCIntVector panelPosition)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.commandManager.PressCommandButton(panelPosition);
        }

        /// <see cref="ICommandService.PressProductionButton"/>
        public void PressProductionButton(int panelPosition)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.commandManager.PressProductionButton(panelPosition);
        }

        /// <see cref="ICommandService.SelectTargetPosition"/>
        public void SelectTargetPosition(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            this.commandManager.SelectTargetPosition(this.mapWindowBC.AttachedWindow.WindowToMapCoords(position));
        }

        /// <see cref="ICommandService.SelectTargetPositionOnMinimap"/>
        public void SelectTargetPositionOnMinimap(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector minimapPixelCoords = position - this.mapWindowBC.Minimap.MinimapPosition.Location;
            minimapPixelCoords = new RCIntVector(Math.Min(this.mapWindowBC.Minimap.MinimapPosition.Width - 1, Math.Max(0, minimapPixelCoords.X)),
                                                 Math.Min(this.mapWindowBC.Minimap.MinimapPosition.Height - 1, Math.Max(0, minimapPixelCoords.Y)));

            IMinimapPixel minimapPixel = this.mapWindowBC.Minimap.GetMinimapPixel(minimapPixelCoords);
            RCNumVector pixelCenterOnMap = minimapPixel.CoveredArea.Location + minimapPixel.CoveredArea.Size / 2;
            this.commandManager.SelectTargetPosition(pixelCenterOnMap);
        }

        /// <see cref="ICommandService.CancelSelectingTargetPosition"/>
        public void CancelSelectingTargetPosition()
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.commandManager.CancelSelectingTargetPosition();
        }

        #endregion ICommandService methods

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private ICommandManagerBC commandManager;

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;

        /// <summary>
        /// TODO: Temporary reference for sending fast commands!
        /// </summary>
        private IMultiplayerService multiplayerService;
    }
}
