using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing defensive stop commands.
    /// </summary>
    public class DefensiveStopExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a DefensiveStopExecution instance for the given entity.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        public DefensiveStopExecution(Entity recipientEntity)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.enemyToAttack = this.ConstructField<Entity>("enemyToAttack");
            this.timeSinceLastSearch = this.ConstructField<int>("timeSinceLastSearch");
            this.recipientEntity.Write(recipientEntity);
            this.enemyToAttack.Write(null);
            this.timeSinceLastSearch.Write(0);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// Select an enemy to be attacked.
            if (this.timeSinceLastSearch.Read() == TIME_BETWEEN_ENEMY_SEARCHES)
            {
                this.enemyToAttack.Write(this.recipientEntity.Read().Armour.SelectEnemy());
                if (enemyToAttack.Read() != null)
                {
                    /// Enemy found -> stop execution finished, attack the enemy.
                    return true;
                }
                this.timeSinceLastSearch.Write(0);
                return false;
            }

            this.timeSinceLastSearch.Write(this.timeSinceLastSearch.Read() + 1);
            return false;
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new DefensiveAttackExecution(
                this.recipientEntity.Read(),
                this.enemyToAttack.Read().ID.Read());
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Stop"; }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// Reference to the enemy to be attacked if found one; otherwise null.
        /// </summary>
        private readonly HeapedValue<Entity> enemyToAttack;

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
