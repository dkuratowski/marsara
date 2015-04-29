using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing hold position commands.
    /// </summary>
    public class HoldExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a HoldExecution instance for the given entity.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        public HoldExecution(Entity recipientEntity)
            : base(new HashSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.attackedEntity = this.ConstructField<Entity>("attackedEntity");
            this.timeSinceLastSearch = this.ConstructField<int>("timeSinceLastSearch");
            this.recipientEntity.Write(recipientEntity);
            this.timeSinceLastSearch.Write(0);
            this.attackedEntity.Write(null);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// Check if we have to do anything in this frame.
            if (this.timeSinceLastSearch.Read() < TIME_BETWEEN_ENEMY_SEARCHES)
            {
                /// Nothing to do now.
                this.timeSinceLastSearch.Write(this.timeSinceLastSearch.Read() + 1);
                return false;
            }

            /// Perform a state refresh in this frame.
            this.timeSinceLastSearch.Write(0);

            if (this.attackedEntity.Read() != null)
            {
                /// We have an entity to be attacked -> Check if it's still in attack range.
                this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
                if (this.recipientEntity.Read().Armour.Target == null)
                {
                    /// Out of attack range -> nothing to do.
                    this.attackedEntity.Write(null);
                }

                return false;
            }

            /// We have no entity to attack -> select a nearby enemy to attack if possible.
            this.attackedEntity.Write(this.recipientEntity.Read().Armour.SelectEnemy());
            if (this.attackedEntity.Read() == null)
            {
                /// No enemy could be selected -> nothing to do.
                return false;
            }

            /// We have a new enemy to be attacked -> Start fire.
            this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
            if (this.recipientEntity.Read().Armour.Target == null)
            {
                /// Out of attack range -> nothing to do.
                this.attackedEntity.Write(null);
                return false;
            }

            /// In attack range -> continue fire.
            return false;
        }

        /// <see cref="CmdExecutionBase.Initialize"/>
        protected override void Initialize()
        {
            this.recipientEntity.Read().StopMoving();

            /// Select a nearby enemy to attack if possible.
            this.attackedEntity.Write(this.recipientEntity.Read().Armour.SelectEnemy());
            if (this.attackedEntity.Read() == null)
            {
                /// No enemy could be selected -> nothing to do.
                return;
            }

            /// We have a new enemy to be attacked -> Start fire.
            this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
            if (this.recipientEntity.Read().Armour.Target == null)
            {
                /// Out of attack range -> nothing to do.
                this.attackedEntity.Write(null);
            }
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Hold"; }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// Reference to the entity currently being attacked or null if there is no entity currently being attacked.
        /// </summary>
        private readonly HeapedValue<Entity> attackedEntity;

        /// <summary>
        /// The elapsed time since last enemy search operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastSearch;

        /// <summary>
        /// The time between enemy search operations.
        /// </summary>
        private const int TIME_BETWEEN_ENEMY_SEARCHES = 12;
    }
}
