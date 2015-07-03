using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// The abstract base class of every classes representing a command execution.
    /// </summary>
    public abstract class CmdExecutionBase : HeapedObject
    {
        #region Internal methods

        /// <summary>
        /// Gets the type of the command that is currently being executed by this execution or null if there is no command currently
        /// being executed by this execution.
        /// </summary>
        internal string CommandBeingExecuted
        {
            get
            {
                return this.subExecution.Read() == null
                    ? this.GetCommandBeingExecuted()
                    : this.subExecution.Read().CommandBeingExecuted;
            }
        }

        /// <summary>
        /// Attaches this command execution to the scenario of the recipient entities.
        /// </summary>
        internal void AttachToScenario()
        {
            if (this.parentExecution.Read() != null) { throw new InvalidOperationException("Sub-executions cannot be attached to a scenario!"); }

            /// Register this command execution to the recipient entities.
            foreach (Entity entity in this.recipientEntities)
            {
                entity.OnCommandExecutionStarted(this);
            }

            /// Register this command execution to the scenario of the recipient entities.
            this.scenario.Read().OnCommandExecutionStarted(this);

            /// Initialize this command execution.
            this.Initialize();
            this.isInitialized.Write(0x01);
        }

        /// <summary>
        /// Continues this command execution.
        /// </summary>
        /// <returns>True if this command execution has finished; otherwise false.</returns>
        internal void Continue()
        {
            if (this.parentExecution.Read() != null) { throw new InvalidOperationException("Sub-executions can only be continued by its parent execution!"); }
            if (this.scenario.Read() == null) { throw new ObjectDisposedException("CmdExecutionBase"); }

            if (this.ContinueInternal()) { this.Dispose(); }
        }

        /// <summary>
        /// Unregisters the given entity from this command execution.
        /// </summary>
        /// <param name="entity"></param>
        internal void RemoveEntity(Entity entity)
        {
            if (this.parentExecution.Read() != null) { throw new InvalidOperationException("Sub-executions cannot be unregistered from an entity!"); }

            if (entity == null) { throw new ArgumentNullException("entity"); }
            if (!this.recipientEntities.Remove(entity)) { throw new ArgumentException("The given entity is not registered to this command execution!", "entity"); }
        }

        #endregion Internal methods

        #region Protected methods for the derived classes

        /// <summary>
        /// Constructs a command execution instance.
        /// </summary>
        /// <param name="recipientEntities">The recipient entities of this command execution.</param>
        protected CmdExecutionBase(HashSet<Entity> recipientEntities)
        {
            if (recipientEntities == null) { throw new ArgumentNullException("recipientEntities"); }
            if (recipientEntities.Count == 0) { throw new ArgumentException("No recipient entities for command execution!", "recipientEntities"); }

            this.scenario = this.ConstructField<Scenario>("scenario");
            this.owner = this.ConstructField<Player>("owner");
            this.parentExecution = this.ConstructField<CmdExecutionBase>("parentExecution");
            this.subExecution = this.ConstructField<CmdExecutionBase>("subExecution");
            this.isInitialized = this.ConstructField<byte>("isInitialized");
            this.scenario.Write(null);
            this.owner.Write(null);
            this.parentExecution.Write(null);
            this.subExecution.Write(null);
            this.isInitialized.Write(0x00);
            this.recipientEntities = new HashSet<Entity>();

            IScenarioElementType typeOfRecipientEntities = null;
            foreach (Entity entity in recipientEntities)
            {
                /// Check whether the entity is added to a scenario and a player.
                if (entity.Scenario == null) { throw new InvalidOperationException("One of the recipient entities does not belong to a scenario!"); }
                if (entity.Owner == null) { throw new InvalidOperationException("One of the recipient entities does not belong to a player!"); }

                /// Check whether every recipient entities are added to the same scenario.
                if (this.scenario.Read() == null)
                {
                    this.scenario.Write(entity.Scenario);
                }
                else if (this.scenario.Read() != entity.Scenario)
                {
                    throw new InvalidOperationException("All of the recipient entities must belong to the same scenario!");
                }

                /// Check whether every recipient entities are owned by the same player.
                if (this.owner.Read() == null)
                {
                    this.owner.Write(entity.Owner);
                }
                else if (this.owner.Read() != entity.Owner)
                {
                    throw new InvalidOperationException("All of the recipient entities must be owned by the same player!");
                }

                /// Check whether every recipient entities has the same type.
                if (typeOfRecipientEntities == null)
                {
                    typeOfRecipientEntities = entity.ElementType;
                }
                else if (typeOfRecipientEntities != entity.ElementType)
                {
                    throw new InvalidOperationException("All of the recipient entities must have the same type!");
                }

                /// Register the recipient entity to this command execution.
                this.recipientEntities.Add(entity);
            }
        }

        /// <summary>
        /// Starts the given command execution as a sub-execution of this command execution.
        /// </summary>
        /// <param name="subExecution">The sub-execution to start.</param>
        protected void StartSubExecution(CmdExecutionBase subExecution)
        {
            if (subExecution == null) { throw new ArgumentNullException("subExecution"); }
            if (this.subExecution.Read() != null) { throw new InvalidOperationException("This command execution is already running a sub-execution!"); }
            if (subExecution.isInitialized.Read() != 0x00) { throw new ArgumentException("The given command execution has already been initialized!", "subExecution"); }
            if (!subExecution.RecipientEntities.SetEquals(this.RecipientEntities)) { throw new ArgumentException("The given command execution has different recipient entities than this one!", "subExecution"); }

            subExecution.scenario.Write(null);
            subExecution.owner.Write(null);
            subExecution.recipientEntities = null;
            subExecution.subExecution.Write(null);
            subExecution.parentExecution.Write(this);
            this.subExecution.Write(subExecution);

            /// Initialize the sub execution.
            this.subExecution.Read().Initialize();
            this.subExecution.Read().isInitialized.Write(0x01);
        }

        /// <summary>
        /// Tries to locate the entity with the given ID using the locators of friendly entities.
        /// </summary>
        /// <param name="entityID">The ID of the entity to locate.</param>
        /// <returns>The located entity or null if the entity could not be located by friendly entities.</returns>
        protected Entity LocateEntity(int entityID)
        {
            /// First we check if the target entity is even on the map.
            Entity targetEntity = this.Scenario.GetElementOnMap<Entity>(entityID);
            if (targetEntity == null) { return null; }

            /// Check if the target entity is friendly.
            if (targetEntity.Owner == this.Owner) { return targetEntity; }

            /// Otherwise we search for friendly entities nearby the target entity and ask their locators.
            foreach (Entity nearbyEntity in targetEntity.Locator.SearchNearbyEntities(ENTITY_SEARCH_RADIUS))
            {
                /// Ignore nearby entity if non-friendly.
                if (nearbyEntity.Owner != this.Owner) { continue; }

                /// Target entity located successfully if any of the nearby friendly entities can locate it.
                if (nearbyEntity.Locator.LocateEntities().Contains(targetEntity)) { return targetEntity; }
            }

            /// Target entity could not be located by any of nearby friendly entities.
            return null;
        }

        /// <summary>
        /// Tries to locate the given position on the map using the locators of friendly entities.
        /// </summary>
        /// <param name="position">The position on the map to locate.</param>
        /// <returns>True if the given position could be located by friendly entities; otherwise false.</returns>
        protected bool LocatePosition(RCNumVector position)
        {
            /// Search for friendly entities nearby the target position and ask their locators.
            foreach (Entity nearbyEntity in this.Scenario.GetElementsOnMap<Entity>(position, ENTITY_SEARCH_RADIUS))
            {
                /// Ignore nearby entity if non-friendly.
                if (nearbyEntity.Owner != this.Owner) { continue; }

                /// Target position located successfully if any of the nearby friendly entities can locate it.
                if (nearbyEntity.Locator.LocatePosition(position)) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Gets the scenario of the recipient entities.
        /// </summary>
        protected Scenario Scenario
        {
            get
            {
                return this.parentExecution.Read() == null
                    ? this.scenario.Read()
                    : this.parentExecution.Read().Scenario;
            }
        }

        #endregion Protected methods for the derived classes

        #region Overrides

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            /// Dispose the sub-execution of this command execution if exists.
            if (this.subExecution.Read() != null) { this.subExecution.Read().Dispose(); }

            /// Detach this command execution from the scenario if this is not a sub-execution.
            if (this.parentExecution.Read() == null)
            {
                /// Unregister this command execution from the scenario of the recipient entities.
                this.scenario.Read().OnCommandExecutionStopped(this);

                /// Unregister this command execution from the recipient entities.
                foreach (Entity entity in this.recipientEntities)
                {
                    entity.OnCommandExecutionStopped();
                }

                /// Remove the references.
                this.scenario.Write(null);
                this.recipientEntities.Clear();
            }
        }

        #endregion Overrides

        #region Overridables

        /// <summary>
        /// Gets the type of the command that is currently being executed by this execution or null if there is no command currently
        /// being executed by this execution.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes. The default implementation constantly returns null.</remarks>
        protected virtual string GetCommandBeingExecuted() { return null; }

        /// <summary>
        /// The internal implementation of CmdExecutionBase.Continue.
        /// </summary>
        /// <returns>True if this command execution has finished; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        protected abstract bool ContinueImpl();

        /// <summary>
        /// The derived class can define a continuation command execution when this command execution has been finished.
        /// </summary>
        /// <returns>The continuation command execution or null if no continuation is defined.</returns>
        /// <remarks>This method is not called for sub-executions.</remarks>
        protected virtual CmdExecutionBase GetContinuation() { return null; }

        /// <summary>
        /// When overriden in a derived class this method shall initialize the state of this command execution.
        /// </summary>
        protected virtual void Initialize() { }

        #endregion Overridables

        #region Private methods

        /// <summary>
        /// Gets the recipient entities of this command execution.
        /// </summary>
        private HashSet<Entity> RecipientEntities
        {
            get
            {
                return this.parentExecution.Read() == null
                    ? this.recipientEntities
                    : this.parentExecution.Read().RecipientEntities;
            }
        }

        /// <summary>
        /// Gets the owner of the recipient entities.
        /// </summary>
        private Player Owner
        {
            get
            {
                return this.parentExecution.Read() == null
                    ? this.owner.Read()
                    : this.parentExecution.Read().Owner;
            }
        }

        /// <summary>
        /// The internal implementation of CmdExecutionBase.Continue.
        /// </summary>
        /// <returns>True if this command execution has finished; otherwise false.</returns>
        private bool ContinueInternal()
        {
            if (this.isInitialized.Read() == 0x00) { throw new InvalidOperationException("Command execution is not initialized!"); }

            /// TODO: if some of the recipient entities has detached from the map, remove them from the list!

            /// If there are no more recipient entities -> execution finished.
            if (this.RecipientEntities.Count == 0) { return true; }

            /// Check if we have a sub-execution currently in progress.
            if (this.subExecution.Read() != null)
            {
                /// We have a sub-execution currently in progress -> continue the sub execution.
                if (this.subExecution.Read().ContinueInternal())
                {
                    /// Sub-execution finished -> Dispose the sub-execution.
                    this.subExecution.Read().Dispose();
                    this.subExecution.Write(null);
                }
                return false;
            }

            /// No sub-execution currently in progress -> continue the current execution.
            if (this.ContinueImpl())
            {
                if (this.parentExecution.Read() == null)
                {
                    /// Current execution finished and this is not a sub-execution -> ask for continuation.
                    CmdExecutionBase continuation = this.GetContinuation();
                    if (continuation != null)
                    {
                        /// Attach the continuation to the scenario.
                        continuation.AttachToScenario();
                    }
                }

                /// Current execution finished.
                return true;
            }

            /// Not finished yet.
            return false;
        }

        #endregion Private methods

        /// <summary>
        /// Reference to the recipient entities of this command execution.
        /// </summary>
        /// TODO: store these entities in a HeapedArray!
        private HashSet<Entity> recipientEntities;

        /// <summary>
        /// Reference to the scenario of the recipient entities.
        /// </summary>
        private readonly HeapedValue<Scenario> scenario;

        /// <summary>
        /// Reference to the owner of the recipient entities.
        /// </summary>
        private readonly HeapedValue<Player> owner;

        /// <summary>
        /// Reference to the parent command execution if this is a sub-execution; otherwise null.
        /// </summary>
        private readonly HeapedValue<CmdExecutionBase> parentExecution;

        /// <summary>
        /// Reference to the currently running sub-execution or null if there is no sub execution currently in progress.
        /// </summary>
        private readonly HeapedValue<CmdExecutionBase> subExecution;

        /// <summary>
        /// A flag that indicates whether this command execution has already been initialized.
        /// A value of 0x00 means false; any other value means true.
        /// </summary>
        private readonly HeapedValue<byte> isInitialized;

        /// <summary>
        /// The radius of search area around located positions given in quadratic tiles.
        /// </summary>
        private const int ENTITY_SEARCH_RADIUS = 15;
    }
}
