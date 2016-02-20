using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing patrol commands.
    /// </summary>
    public class PatrolExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a PatrolExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        public PatrolExecution(Entity recipientEntity, RCNumVector targetPosition)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.patrolStartPosition = this.ConstructField<RCNumVector>("patrolStartPosition");
            this.patrolEndPosition = this.ConstructField<RCNumVector>("patrolEndPosition");
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.timeSinceLastSearch = this.ConstructField<int>("timeSinceLastSearch");
            
            this.recipientEntity.Write(recipientEntity);
            this.patrolStartPosition.Write(this.recipientEntity.Read().MotionControl.PositionVector.Read());
            this.patrolEndPosition.Write(targetPosition);
            this.targetPosition.Write(targetPosition);
            this.timeSinceLastSearch.Write(0);
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

            /// Check if we have to change patrol direction.
            if (!this.recipientEntity.Read().MotionControl.IsMoving)
            {
                /// Change patrol direction.
                this.targetPosition.Write(
                    this.targetPosition.Read() == this.patrolStartPosition.Read()
                        ? this.patrolEndPosition.Read()
                        : this.patrolStartPosition.Read());
                this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
            }

            /// Check if an enemy became into sight-range.
            Entity enemyToAttack = this.recipientEntity.Read().Armour.SelectEnemyForStandardWeapons();
            if (enemyToAttack != null)
            {
                /// Enemy found -> start an attack sub-execution on the enemy.
                this.StartSubExecution(new AttackExecution(this.recipientEntity.Read(), enemyToAttack.MotionControl.PositionVector.Read(), enemyToAttack.ID.Read()));
            }
            return false;
        }

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Patrol"; }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The start position of the patrol execution.
        /// </summary>
        private readonly HeapedValue<RCNumVector> patrolStartPosition;

        /// <summary>
        /// The end position of the patrol execution.
        /// </summary>
        private readonly HeapedValue<RCNumVector> patrolEndPosition;

        /// <summary>
        /// The current target position.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

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
