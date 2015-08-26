﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the command manager business component.
    /// </summary>
    /// TODO: derive from ScenarioDependentComponent!
    [Component("RC.App.BizLogic.CommandManagerBC")]
    class CommandManagerBC : ICommandManagerBC, IComponent
    {
        /// <summary>
        /// Constructs a CommandManagerBC instance.
        /// </summary>
        public CommandManagerBC()
        {
            this.commandPanelSlots = new CommandPanelSlot[BUTTON_ARRAY_ROWS, BUTTON_ARRAY_COLS];
            this.targetPositionInputSlot = null;
            this.rootListeners = new List<CommandInputListener>();
            this.completedListeners = new List<CommandInputListener>();
            this.activeListeners = new List<CommandInputListener>();
            this.allSpritePalettes = new List<ISpritePalette>();
            this.commandBuilder = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.commandBuilder = new CommandBuilder();

            /// Load the command input listeners from the configured directory.
            DirectoryInfo rootDir = new DirectoryInfo(BizLogicConstants.COMMAND_DIR);
            if (rootDir.Exists)
            {
                FileInfo[] commandFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
                foreach (FileInfo commandFile in commandFiles)
                {
                    string xmlStr = File.ReadAllText(commandFile.FullName);
                    string imageDir = commandFile.DirectoryName;

                    /// Load the XML document.
                    XDocument xmlDoc = XDocument.Parse(xmlStr);

                    /// Load the sprite palette if exists.
                    XElement spritePaletteElem = xmlDoc.Root.Element(SPRITEPALETTE_ELEM);
                    ISpritePalette spritePalette = spritePaletteElem != null ? XmlHelper.LoadSpritePalette(spritePaletteElem, imageDir) : null;
                    if (spritePalette != null)
                    {
                        spritePalette.SetIndex(this.allSpritePalettes.Count);
                        this.allSpritePalettes.Add(spritePalette);
                    }

                    /// Load the command input listeners.
                    foreach (XElement listenerElem in xmlDoc.Root.Elements())
                    {
                        CommandInputListener rootListener = CommandInputListener.LoadFromXml(listenerElem, spritePalette, this.commandBuilder);
                        if (rootListener != null)
                        {
                            this.rootListeners.Add(rootListener);
                            this.activeListeners.Add(rootListener);
                        }
                    }
                }
            }
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }
        
        #endregion IComponent methods

        #region ICommandManagerBC methods

        /// <see cref="ICommandManagerBC.SpritePalettes"/>
        public IEnumerable<ISpritePalette> SpritePalettes
        {
            get { foreach (ISpritePalette spritePalette in this.allSpritePalettes) { yield return spritePalette; } }
        }

        /// <see cref="ICommandManagerBC.PressCommandButton"/>
        public void PressCommandButton(RCIntVector panelPosition)
        {
            if (panelPosition == RCIntVector.Undefined) { throw new ArgumentNullException("panelPosition"); }

            CommandPanelSlot slot = this.commandPanelSlots[panelPosition.X, panelPosition.Y];
            if (slot == null) { throw new InvalidOperationException(string.Format("There is no command button on the command panel at {0}!", panelPosition)); }
            if (slot.ButtonState == CommandButtonStateEnum.Disabled) { throw new InvalidOperationException(string.Format("The command button at {0} is disabled!", panelPosition)); }

            this.CompleteListener(slot.Listener);
        }

        /// <see cref="ICommandManagerBC.SelectTargetPosition"/>
        public void SelectTargetPosition(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }
            if (this.targetPositionInputSlot == null) { throw new InvalidOperationException("Selecting target position is not allowed in the current state!"); }

            using (new CommandBuilderPermission(this.commandBuilder))
            {
                this.targetPositionInputSlot.TargetPositionListener.SelectTargetPosition(position);
            }

            this.CompleteListener(this.targetPositionInputSlot.Listener);
        }

        /// <see cref="ICommandManagerBC.CancelSelectingTargetPosition"/>
        public void CancelSelectingTargetPosition()
        {
            if (this.targetPositionInputSlot == null) { throw new InvalidOperationException("Cancel selecting target position is not allowed in the current state!"); }
            this.CompleteListener(this.targetPositionInputSlot.Listener);
        }

        /// <see cref="ICommandManagerBC.GetCmdButtonSprite"/>
        public SpriteInst GetCmdButtonSprite(RCIntVector panelPosition)
        {
            if (panelPosition == RCIntVector.Undefined) { throw new ArgumentNullException("panelPosition"); }
            if (this.commandPanelSlots[panelPosition.X, panelPosition.Y] == null) { throw new InvalidOperationException(string.Format("There is no command button on the command panel at {0}!", panelPosition)); }
            return this.commandPanelSlots[panelPosition.X, panelPosition.Y].ButtonSprite;
        }

        /// <see cref="ICommandManagerBC.GetCmdButtonState"/>
        public CommandButtonStateEnum GetCmdButtonState(RCIntVector panelPosition)
        {
            if (panelPosition == RCIntVector.Undefined) { throw new ArgumentNullException("panelPosition"); }
            if (this.commandPanelSlots[panelPosition.X, panelPosition.Y] == null) { return CommandButtonStateEnum.Invisible; }
            return this.commandPanelSlots[panelPosition.X, panelPosition.Y].ButtonState;
        }

        /// <see cref="ICommandManagerBC.Update"/>
        public void Update()
        {
            /// Check if the completion status is still valid of the completed listeners.
            bool isCancelled = false;
            foreach (CommandInputListener completedListener in this.completedListeners)
            {
                if (!completedListener.CheckCompletionStatus())
                {
                    isCancelled = true;
                    break;
                }
            }

            /// Cancel the current command input procedure if the completion status of a completed listener became invalid.
            if (isCancelled)
            {
                this.completedListeners.Clear();
                this.activeListeners = new List<CommandInputListener>(this.rootListeners);
                this.commandBuilder.Reset();
            }

            /// Update the command panel and the target position input slots.
            this.UpdateSlots();
        }

        /// <see cref="ICommandManagerBC.IsWaitingForTargetPosition"/>
        public bool IsWaitingForTargetPosition { get { return this.targetPositionInputSlot != null; } }

        /// <see cref="ICommandManagerBC.SelectedBuildingType"/>
        public string SelectedBuildingType
        {
            get
            {
                if (this.targetPositionInputSlot == null) { throw new InvalidOperationException("Command manager is not waiting for target position input!"); }
                return this.targetPositionInputSlot.SelectedBuildingType;
            }
        }

        /// <see cref="ICommandManagerBC.NewCommand"/>
        public event Action<RCCommand> NewCommand;

        #endregion ICommandManagerBC methods

        #region Internal methods

        /// <summary>
        /// Tries to complete the given listener and updates the command panel and the target position input slots accordingly.
        /// </summary>
        /// <param name="listener">The listener to be completed.</param>
        private void CompleteListener(CommandInputListener listener)
        {
            using (new CommandBuilderPermission(this.commandBuilder))
            {
                CommandInputListener.CompletionResultEnum completionResult = listener.TryComplete();
                if (completionResult == CommandInputListener.CompletionResultEnum.Succeeded)
                {
                    /// Listener completed successfully -> activate children.
                    List<CommandInputListener> children = new List<CommandInputListener>(listener.Children);
                    if (children.Count > 0)
                    {
                        /// Still has children -> continue the current command input procedure.
                        this.completedListeners.Add(listener);
                        this.activeListeners = children;
                    }
                    else
                    {
                        /// No children -> current command input procedure completed.
                        this.completedListeners.Clear();
                        this.activeListeners = new List<CommandInputListener>(this.rootListeners);
                        RCCommand command = this.commandBuilder.CreateCommand();
                        this.commandBuilder.Reset();
                        TraceManager.WriteAllTrace(command, BizLogicTraceFilters.INFO);
                        if (this.NewCommand != null) { this.NewCommand(command); }
                    }
                }
                else if (completionResult == CommandInputListener.CompletionResultEnum.FailedAndCancel)
                {
                    /// Listener completion failed and command input procedure shall be cancelled.
                    this.completedListeners.Clear();
                    this.activeListeners = new List<CommandInputListener>(this.rootListeners);
                    this.commandBuilder.Reset();
                }
                else if (completionResult == CommandInputListener.CompletionResultEnum.FailedButContinue)
                {
                    /// Listener completion failed but command input procedure shall be continued.
                }
            }

            /// Update the command panel and the target position input slots.
            this.UpdateSlots();
        }

        /// <summary>
        /// Updates the command panel and the target position input slots.
        /// </summary>
        private void UpdateSlots()
        {
            this.commandPanelSlots = new CommandPanelSlot[BUTTON_ARRAY_ROWS, BUTTON_ARRAY_COLS];
            this.targetPositionInputSlot = null;
            foreach (CommandInputListener activeListener in this.activeListeners)
            {
                this.TryAttachAsButtonListener(activeListener);
                this.TryAttachAsTargetPositionListener(activeListener);
            }

            /// Highlight the button with the highest priority.
            CommandPanelSlot slotToHighlight = null;
            for (int row = 0; row < BUTTON_ARRAY_ROWS; row++)
            {
                for (int col = 0; col < BUTTON_ARRAY_COLS; col++)
                {
                    if (this.commandPanelSlots[row, col] == null) { continue; }
                    if (this.commandPanelSlots[row, col].ButtonState != CommandButtonStateEnum.Enabled) { continue; }
                    if (!this.commandPanelSlots[row, col].ButtonListener.IsHighlighted) { continue; }
                    if (slotToHighlight == null || this.commandPanelSlots[row, col].ButtonListener.Priority > slotToHighlight.ButtonListener.Priority)
                    {
                        slotToHighlight = this.commandPanelSlots[row, col];
                    }
                    else if (this.commandPanelSlots[row, col].ButtonListener.Priority == slotToHighlight.ButtonListener.Priority)
                    {
                        throw new InvalidOperationException(string.Format("IButtonListeners with the same priority should be highlighted at command panel slots {0} and {1}!", new RCIntVector(row, col), slotToHighlight.ButtonListener.CommandPanelSlot));
                    }
                }
            }
            if (slotToHighlight != null) { slotToHighlight.ButtonState = CommandButtonStateEnum.Highlighted; }
        }

        /// <summary>
        /// Tries to attach the given command input listener to the appropriate command button slot if the given listener is a command button
        /// listener, and the state of the given listener is not CommandButtonStateEnum.Invisible.
        /// </summary>
        /// <param name="listener">The command input listener to attach.</param>
        /// <remarks>
        /// If the given command input listener is not a command button listener, or its state is CommandButtonStateEnum.Invisible then this
        /// function has no effect.
        /// </remarks>
        private void TryAttachAsButtonListener(CommandInputListener listener)
        {
            IButtonListener buttonListener = listener as IButtonListener;
            if (buttonListener == null) { return; }

            AvailabilityEnum buttonAvailability = buttonListener.ButtonAvailability;
            if (buttonAvailability == AvailabilityEnum.Unavailable) { return; }

            RCIntVector slotPosition = buttonListener.CommandPanelSlot;
            if (this.commandPanelSlots[slotPosition.X, slotPosition.Y] != null)
            {
                if (this.commandPanelSlots[slotPosition.X, slotPosition.Y].ButtonListener.Priority > buttonListener.Priority)
                {
                    /// Another IButtonListener with higher priority has already been attached to the command panel slot.
                    return;
                }
                else if (this.commandPanelSlots[slotPosition.X, slotPosition.Y].ButtonListener.Priority == buttonListener.Priority)
                {
                    throw new InvalidOperationException(string.Format("Another IButtonListener with the same priority has already been attached to command panel slot {0}!", slotPosition));
                }
            }

            this.commandPanelSlots[slotPosition.X, slotPosition.Y] = new CommandPanelSlot
            {
                ButtonState = buttonAvailability == AvailabilityEnum.Disabled ? CommandButtonStateEnum.Disabled : CommandButtonStateEnum.Enabled,
                ButtonSprite = buttonListener.ButtonSprite,
                ButtonListener = buttonListener,
                Listener = listener
            };
        }

        /// <summary>
        /// Tries to attach the given command input listener to the target position input slot if the given listener is a target position listener.
        /// </summary>
        /// <param name="listener">The command input listener to attach.</param>
        /// <remarks>
        /// If the given command input listener is not a target position listener then this function has no effect.
        /// </remarks>
        private void TryAttachAsTargetPositionListener(CommandInputListener listener)
        {
            ITargetPositionListener targetPositionListener = listener as ITargetPositionListener;
            if (targetPositionListener == null) { return; }

            if (this.targetPositionInputSlot != null) { throw new InvalidOperationException("ITargetPositionListener has already been attached to the target position input slot!"); }

            this.targetPositionInputSlot = new TargetPositionInputSlot
            {
                SelectedBuildingType = targetPositionListener.SelectedBuildingType,
                TargetPositionListener = targetPositionListener,
                Listener = listener
            };
        }

        #endregion Internal methods

        #region CommandBuilderPermission class

        /// <summary>
        /// Helper class to enable/disable a CommandBuilder instance.
        /// </summary>
        private class CommandBuilderPermission : IDisposable
        {
            /// <summary>
            /// Constructs a CommandBuilderPermission instance.
            /// </summary>
            /// <param name="commandBuilder">Reference to the CommandBuilder.</param>
            public CommandBuilderPermission(CommandBuilder commandBuilder)
            {
                this.commandBuilder = commandBuilder;
                this.commandBuilder.SetEnabled(true);
            }

            /// <see cref="IDisposable.Dispose"/>
            public void Dispose()
            {
                this.commandBuilder.SetEnabled(false);
            }

            /// <summary>
            /// Reference to the CommandBuilder.
            /// </summary>
            private CommandBuilder commandBuilder;
        }

        #endregion CommandBuilderPermission class

        /// <summary>
        /// A 2D array that stores informations about the slots on the command panel. The first coordinate in this array defines the row and
        /// the second coordinate defines the column in which the button is located. The first row is the row at the top,
        /// the first column is the column at the left side of the panel.
        /// </summary>
        private CommandPanelSlot[,] commandPanelSlots;

        /// <summary>
        /// Reference to the target position input slot or null if no target position input is needed currently.
        /// </summary>
        private TargetPositionInputSlot targetPositionInputSlot;

        /// <summary>
        /// List of the root listeners.
        /// </summary>
        private List<CommandInputListener> rootListeners;

        /// <summary>
        /// List of the listeners that have already completed in the order of their completion.
        /// </summary>
        private List<CommandInputListener> completedListeners;

        /// <summary>
        /// List of the currently active listeners.
        /// </summary>
        private List<CommandInputListener> activeListeners;

        /// <summary>
        /// List of all the loaded sprite palettes in the order of their indices.
        /// </summary>
        private List<ISpritePalette> allSpritePalettes = new List<ISpritePalette>();

        /// <summary>
        /// Reference to the command builder.
        /// </summary>
        private CommandBuilder commandBuilder;

        /// <summary>
        /// The size of the command button array.
        /// </summary>
        private const int BUTTON_ARRAY_ROWS = 3;
        private const int BUTTON_ARRAY_COLS = 3;

        /// <summary>
        /// The name of the XML-node that contains the sprite palettes for the command input tree.
        /// </summary>
        private const string SPRITEPALETTE_ELEM = "spritePalette";
    }
}