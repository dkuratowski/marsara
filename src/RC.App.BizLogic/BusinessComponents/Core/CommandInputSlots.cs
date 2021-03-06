﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a slot on the command panel.
    /// </summary>
    class CommandPanelSlot
    {
        /// <summary>
        /// Gets the state of the command button at this slot.
        /// </summary>
        public CommandButtonStateEnum ButtonState { get; set; }

        /// <summary>
        /// Gets the sprite of the command button to be displayed at this slot.
        /// </summary>
        public SpriteRenderInfo ButtonSprite { get; set; }

        /// <summary>
        /// Reference to the button listener that is waiting for trigger from the command panel.
        /// </summary>
        public IButtonListener ButtonListener { get; set; }

        /// <summary>
        /// Reference to the listener to complete when the button is pressed.
        /// </summary>
        public CommandInputListener Listener { get; set; }
    }

    /// <summary>
    /// Represents a target position input slot.
    /// </summary>
    class TargetPositionInputSlot
    {
        /// <summary>
        /// True if the selected building has to be placed, false otherwise.
        /// </summary>
        public bool PlaceSelectedBuilding { get; set; }

        /// <summary>
        /// The name of the building type to be placed or null if there is no building type to be placed or if the selected building has to be placed.
        /// </summary>
        public string BuildingType { get; set; }

        /// <summary>
        /// The name of the addon type to be placed together with the appropriate building or null if there is no addon type to be placed.
        /// </summary>
        public string AddonType { get; set; }

        /// <summary>
        /// Reference to the listener that is waiting for target position.
        /// </summary>
        public ITargetPositionListener TargetPositionListener { get; set; }

        /// <summary>
        /// Reference to the listener to complete when the target position input arrived.
        /// </summary>
        public CommandInputListener Listener { get; set; }
    }
}
