﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The abstract base class for command input listeners.
    /// </summary>
    abstract class CommandInputListener
    {
        /// <summary>
        /// Loads a command input listener (and all of its children recursively) from the given XML-node.
        /// </summary>
        /// <param name="listenerElem">The XML-element to load from.</param>
        /// <param name="spritePalette">The sprite palette to be used by the listener.</param>
        /// <param name="commandBuilder">Reference to the command builder interface.</param>
        /// <returns>The loaded command input listener or null if command input listener could not be loaded from the given XML-node.</returns>
        public static CommandInputListener LoadFromXml(XElement listenerElem, ISpritePalette spritePalette, ICommandBuilder commandBuilder)
        {
            if (listenerElem == null) { throw new ArgumentNullException("listenerElem"); }
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }
            if (commandBuilder == null) { throw new ArgumentNullException("commandBuilder"); }

            /// Check if the XML-node is supported.
            if (!commandInputListenerTypes.ContainsKey(listenerElem.Name.ToString())) { return null; }

            /// Load the listener from the XML-node.
            CommandInputListener loadedListener =
                (CommandInputListener)Activator.CreateInstance(commandInputListenerTypes[listenerElem.Name.ToString()]);

            /// Load the children of the loaded listener recursively.
            List<CommandInputListener> children = new List<CommandInputListener>();
            foreach (XElement childElem in listenerElem.Elements())
            {
                CommandInputListener childNode = LoadFromXml(childElem, spritePalette, commandBuilder);
                if (childNode != null) { children.Add(childNode); }
            }
            loadedListener.children = children;
            loadedListener.spritePalette = spritePalette;
            loadedListener.commandBuilder = commandBuilder;
            loadedListener.Init(listenerElem);

            /// Return the loaded listener.
            return loadedListener;
        }

        /// <summary>
        /// This method is called for completed input listeners to check if their completion status is still valid.
        /// </summary>
        /// <returns>True if the completion status of this input listener is still valid; otherwise false.</returns>
        /// <remarks>
        /// If the completion status of a completed input listener becomes invalid then the current command input procedure
        /// will be cancelled.
        /// The default implementation always returns true.
        /// </remarks>
        public virtual bool CheckCompletionStatus() { return true; }

        /// <summary>
        /// Tries to complete this input listener.
        /// </summary>
        /// <returns>True if the completion was successful; otherwise false.</returns>
        /// <remarks>
        /// If the completion was unsuccessful then the current command input procedure will be cancelled.
        /// The default implementation always returns true.
        /// </remarks>
        public virtual bool TryComplete() { return true; }

        /// <summary>
        /// Gets the children of this command input listener.
        /// </summary>
        public IEnumerable<CommandInputListener> Children
        {
            get { foreach (CommandInputListener childListener in this.children) { yield return childListener; } }
        }
        
        /// <summary>
        /// Constructs a CommandInputListener instance.
        /// </summary>
        protected CommandInputListener() { }

        /// <summary>
        /// Gets the sprite palette used by this listener.
        /// </summary>
        protected ISpritePalette SpritePalette { get { return this.spritePalette; } }

        /// <summary>
        /// Gets a reference to the command builder interface.
        /// </summary>
        protected ICommandBuilder CommandBuilder { get { return this.commandBuilder; } }

        /// <summary>
        /// Initialization method that can be overriden by the derived classes.
        /// </summary>
        /// <param name="listenerElem">The XML-node used for initialization.</param>
        /// <remarks>The default implementation does nothing.</remarks>
        protected virtual void Init(XElement listenerElem) { }

        /// <summary>
        /// The children of this command input listener or an empty list if this listener has no children.
        /// </summary>
        private List<CommandInputListener> children;

        /// <summary>
        /// The sprite palette used by this listener.
        /// </summary>
        private ISpritePalette spritePalette;

        /// <summary>
        /// Reference to the command builder interface.
        /// </summary>
        private ICommandBuilder commandBuilder;

        /// <summary>
        /// The list of the command input listener types mapped by the name of the XML-nodes from which to load them.
        /// </summary>
        private static Dictionary<string, Type> commandInputListenerTypes = new Dictionary<string, Type>
        {
            { "commandButton", typeof(CommandButtonListener) },
            { "buildingButton", typeof(BuildingButtonListener) },
            { "cancelButton", typeof(CancelButtonListener) },
            { "selectTargetPosition", typeof(TargetPositionListener) },
        };
    }

    /// <summary>
    /// Interface for listeners that are waiting for a target position.
    /// </summary>
    interface ITargetPositionListener
    {
        /// <summary>
        /// The name of the selected building type if this listener is waiting for target position for a build command; otherwise null.
        /// </summary>
        string SelectedBuildingType { get; }

        /// <summary>
        /// Indicates to the listener that a target position has been selected.
        /// </summary>
        /// <param name="targetPosition">The selected target position.</param>
        /// <exception cref="ArgumentNullException">If targetPosition is RCIntVector.Undefined.</exception>
        /// <exception cref="InvalidOperationException">If a target position has already been selected.</exception>
        void SelectTargetPosition(RCNumVector targetPosition);
    }

    /// <summary>
    /// Interface for listeners that are waiting for a trigger from the command panel.
    /// </summary>
    interface ICommandButtonListener
    {
        /// <summary>
        /// Gets the slot on the command panel from which this listener is waiting for trigger.
        /// </summary>
        RCIntVector CommandPanelSlot { get; }

        /// <summary>
        /// Gets the sprite of the command button to be displayed for this listener.
        /// </summary>
        SpriteInst ButtonSprite { get; }

        /// <summary>
        /// Gets the state of the command button for this listener.
        /// </summary>
        CommandButtonStateEnum ButtonState { get; }
    }
}