using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.App.PresLogic.SpriteGroups;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Mouse handler for the map editor.
    /// </summary>
    class MapEditorMouseHandler : MouseHandlerBase
    {
        /// <summary>
        /// Constructs a MapEditorMouseHandler instance.
        /// </summary>
        /// <param name="scrollEventSource">The UISensitiveObject that will raise the events for scrolling.</param>
        /// <param name="mapDisplay">Reference to the target map display.</param>
        public MapEditorMouseHandler(UISensitiveObject scrollEventSource, IMapDisplay mapDisplay)
            : base(scrollEventSource, mapDisplay)
        {
            this.mapEditorService = ComponentManager.GetInterface<IMapEditorService>();
            this.objectPlacementInfo = null;
        }

        /// <summary>
        /// Starts placing object with the given view & sprite group. If another object placement is currently in progress then
        /// it will be stopped automatically.
        /// </summary>
        /// <param name="view">The view to be used.</param>
        /// <param name="sprites">The sprite group to be used.</param>
        public void StartPlacingObject(IObjectPlacementView view, ISpriteGroup sprites)
        {
            this.StopPlacingObject();
            this.objectPlacementInfo = new ObjectPlacementInfo(view, sprites);
            this.mapEditorService.AnimationsUpdated += this.objectPlacementInfo.View.StepPreviewAnimation;
        }

        /// <summary>
        /// Stops the currently active object placement process if exists.
        /// </summary>
        public void StopPlacingObject()
        {
            if (this.objectPlacementInfo != null)
            {
                this.mapEditorService.AnimationsUpdated -= this.objectPlacementInfo.View.StepPreviewAnimation;
                this.objectPlacementInfo = null;
            }
        }

        /// <summary>
        /// Gets whether an object placement operation is currently in progress or not.
        /// </summary>
        public bool IsPlacingObject { get { return this.objectPlacementInfo != null; } }

        /// <see cref="MouseHandlerBase.ObjectPlacementInfo"/>
        public override ObjectPlacementInfo ObjectPlacementInfo { get { return this.objectPlacementInfo; } }

        /// <summary>
        /// Reference to the currently active object placement information object.
        /// </summary>
        private ObjectPlacementInfo objectPlacementInfo;

        /// <summary>
        /// Reference to the map editor service.
        /// </summary>
        private IMapEditorService mapEditorService;
    }
}
