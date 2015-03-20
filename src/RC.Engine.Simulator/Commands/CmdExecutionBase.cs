using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// The abstract base class of every classes representing a command execution.
    /// </summary>
    public abstract class CmdExecutionBase : HeapedObject
    {
        #region Public interface

        /// <summary>
        /// Continues this command execution.
        /// </summary>
        /// <returns>True if this command execution has finished; otherwise false.</returns>
        public bool Continue()
        {
            if (this.scenario.Read() == null) { throw new ObjectDisposedException("CmdExecutionBase"); }
            if (this.recipientEntities.Count == 0) { return true; }

            /// TODO: if some of the recipient entities has detached from the map, remove them from the list!
            return this.ContinueImpl();
        }

        #endregion Public interface

        #region Internal methods

        /// <summary>
        /// Unregisters the given entity from this command execution.
        /// </summary>
        /// <param name="entity"></param>
        internal void RemoveEntity(Entity entity)
        {
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

            this.typeOfRecipientEntities = null;
            this.scenario = this.ConstructField<Scenario>("scenario");
            this.owner = this.ConstructField<Player>("owner");
            this.scenario.Write(null);
            this.owner.Write(null);
            this.recipientEntities = new HashSet<Entity>();
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
                if (this.typeOfRecipientEntities == null)
                {
                    this.typeOfRecipientEntities = entity.ElementType;
                }
                else if (this.typeOfRecipientEntities != entity.ElementType)
                {
                    throw new InvalidOperationException("All of the recipient entities must have the same type!");
                }

                /// Register the recipient entity to this command execution.
                this.recipientEntities.Add(entity);

                /// Register this command execution to the recipient entity.
                entity.OnCommandExecutionStarted(this);
            }

            /// Register this command execution to the scenario of the recipient entities.
            this.scenario.Read().OnCommandExecutionStarted(this);
        }

        /// <summary>
        /// Gets the recipient entities of this command execution.
        /// </summary>
        protected HashSet<Entity> RecipientEntities { get { return new HashSet<Entity>(this.recipientEntities); } }

        /// <summary>
        /// Gets the owner of the recipient entities.
        /// </summary>
        protected Player Owner { get { return this.owner.Read(); } }

        #endregion Protected methods for the derived classes

        #region Overrides

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            /// Unregister this command execution from the scenario of the recipient entities.
            this.scenario.Read().OnCommandExecutionStopped(this);
            this.scenario.Write(null);

            /// Unregister this command execution from the recipient entities.
            foreach (Entity entity in this.recipientEntities)
            {
                entity.OnCommandExecutionStopped();
            }
            this.recipientEntities.Clear();
        }

        #endregion Overrides

        #region Overridables

        /// <summary>
        /// Gets the type of the command that is currently being executed by this execution or null if there is no command currently
        /// being executed by this execution.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes. The default implementation constantly returns null.</remarks>
        public virtual string CommandBeingExecuted { get { return null; } }

        /// <summary>
        /// The internal implementation of CmdExecutionBase.Continue.
        /// </summary>
        /// <returns>True if this command execution has finished; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        protected abstract bool ContinueImpl();

        #endregion Overridables

        /// <summary>
        /// Reference to the recipient entities of this command execution.
        /// </summary>
        /// TODO: store these entities in a HeapedArray!
        private readonly HashSet<Entity> recipientEntities;

        /// <summary>
        /// Reference to the scenario of the recipient entities.
        /// </summary>
        private readonly HeapedValue<Scenario> scenario;

        /// <summary>
        /// Reference to the owner of the recipient entities.
        /// </summary>
        private readonly HeapedValue<Player> owner;

        /// <summary>
        /// The type of the recipient entities.
        /// </summary>
        private readonly IScenarioElementType typeOfRecipientEntities;
    }
}
