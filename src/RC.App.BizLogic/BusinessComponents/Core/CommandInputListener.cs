using System;
using System.Collections.Generic;
using System.Xml.Linq;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The abstract base class for command input listeners.
    /// </summary>
    abstract class CommandInputListener
    {
        /// <summary>
        /// Enumerates the possible results of the CommandInputListener.TryComplete method.
        /// </summary>
        public enum CompletionResultEnum 
        {
            FailedButContinue = 0,  /// The completion was failed, but continue the current command input procedure.
            FailedAndCancel = 1,    /// The completion was failed, and cancel the current command input procedure.
            Succeeded = 2,          /// The completion was successful.
        }

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
        /// <returns>The result of the completion.</returns>
        /// <remarks>
        /// The default implementation always returns CompletionResultEnum.Succeeded.
        /// </remarks>
        public virtual CompletionResultEnum TryComplete() { return CompletionResultEnum.Succeeded; }

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
        private static readonly Dictionary<string, Type> commandInputListenerTypes = new Dictionary<string, Type>
        {
            { "commandButton", typeof(CommandButtonListener) },
            { "buildingButton", typeof(BuildingButtonListener) },
            { "cancelButton", typeof(CancelButtonListener) },
            { "selectTargetPosition", typeof(TargetPositionListener) },
            { "productionButton", typeof(ProductionButtonListener) },
            { "cancelProductionButton", typeof(CancelProductionButtonListener) },
        };
    }

    /// <summary>
    /// Interface for listeners that are waiting for a target position.
    /// </summary>
    interface ITargetPositionListener
    {
        /// <summary>
        /// True if the selected building has to be placed, false otherwise.
        /// </summary>
        bool PlaceSelectedBuilding { get; }

        /// <summary>
        /// The name of the building type to be placed or null if there is no building type to be placed or if the selected building has to be placed.
        /// </summary>
        string BuildingType { get; }

        /// <summary>
        /// The name of the addon type to be placed together with the appropriate building or null if there is no addon type to be placed.
        /// </summary>
        string AddonType { get; }

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
    interface IButtonListener
    {
        /// <summary>
        /// Gets the slot on the command panel from which this button listener is waiting for trigger.
        /// </summary>
        RCIntVector CommandPanelSlot { get; }

        /// <summary>
        /// Gets the sprite of the button to be displayed for this listener.
        /// </summary>
        SpriteRenderInfo ButtonSprite { get; }

        /// <summary>
        /// Gets the availability of the button for this listener.
        /// </summary>
        AvailabilityEnum ButtonAvailability { get; }

        /// <summary>
        /// Gets whether the button shall be highlighted or not.
        /// </summary>
        bool IsHighlighted { get; }

        /// <summary>
        /// Gets the priority of this button listener. The higher this number is, the higher the priority this listener has.
        /// </summary>
        int Priority { get; }
    }
}