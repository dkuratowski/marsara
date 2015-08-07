using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System.Collections.Generic;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing attack commands.
    /// </summary>
    public class AttackExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates an AttackExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the entity to attack or -1 if no such entity is defined.</param>
        public AttackExecution(Entity recipientEntity, RCNumVector targetPosition, int targetEntityID)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.targetEntityID = this.ConstructField<int>("targetEntityID");
            this.attackedEntity = this.ConstructField<Entity>("attackedEntity");
            this.timeSinceLastCheck = this.ConstructField<int>("timeSinceLastCheck");

            this.recipientEntity.Write(recipientEntity);
            this.targetPosition.Write(targetPosition);
            this.targetEntityID.Write(targetEntityID);
            this.attackedEntity.Write(null);
            this.timeSinceLastCheck.Write(0);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// Check if we have to do anything in this frame.
            if (this.timeSinceLastCheck.Read() < TIME_BETWEEN_CHECKS)
            {
                /// Nothing to do now.
                this.timeSinceLastCheck.Write(this.timeSinceLastCheck.Read() + 1);
                return false;
            }

            /// Perform a state refresh in this frame.
            this.timeSinceLastCheck.Write(0);
            return this.targetEntityID.Read() != -1 ? this.ContinueAttack() : this.ContinueAttackMove();
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new StopExecution(this.recipientEntity.Read());
        }

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            /// If target entity is not given -> this is an attack-move execution.
            if (this.targetEntityID.Read() == -1) { return; }

            /// If target entity is given but target position is not visible -> this is an attack execution.
            if (!this.LocatePosition(this.targetPosition.Read())) { return; }

            /// Target entity is given, target position is visible and target entity can be located -> this is an attack execution.
            this.attackedEntity.Write(this.LocateEntity(this.targetEntityID.Read()));
            if (this.attackedEntity.Read() != null) { return; }

            /// Target position is visible and target entity is given but it cannot be located -> this is an attack-move execution.
            this.targetEntityID.Write(-1);

            if (this.attackedEntity.Read() == null)
            {
                /// We have no entity to be attacked...
                if (this.targetEntityID.Read() != -1)
                {
                    /// ...in case of attack execution -> start move to the target position.
                    this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
                    return;
                }

                /// ...in case of attack-move execution -> Select an enemy.
                this.attackedEntity.Write(this.recipientEntity.Read().Armour.SelectEnemy());
                if (this.attackedEntity.Read() == null)
                {
                    /// No enemy could be selected -> start move to the target position.
                    this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
                    return;
                }
            }

            /// We have an entity to be attacked -> Start attack with a standard weapon.
            this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
            if (this.recipientEntity.Read().Armour.Target != null)
            {
                /// Close enough -> not necessary to start approaching.
                //this.recipientEntity.Read().MotionControl.StopMoving(); // StopMoving already called in CmdExecutionBase.Initialize
                return;
            }

            /// Too far -> start approaching.
            this.recipientEntity.Read().MotionControl.StartMoving(this.attackedEntity.Read().MotionControl.PositionVector.Read());
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Attack"; }

        #endregion Overrides

        /// <summary>
        /// Continue the attack execution.
        /// </summary>
        /// <returns>True if execution is finished; otherwise false.</returns>
        private bool ContinueAttack()
        {
            if (this.attackedEntity.Read() != null)
            {
                /// We have an entity to be attacked -> Check if it still can be located.
                this.attackedEntity.Write(this.LocateEntity(this.attackedEntity.Read().ID.Read()));
                if (this.attackedEntity.Read() == null)
                {
                    /// Attacked entity cannot be located anymore -> attack execution finished.
                    return true;
                }

                /// Attacked entity still can be located -> Check if its still in attack range.
                this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
                if (this.recipientEntity.Read().Armour.Target != null)
                {
                    /// Still in attack range -> continue attack with a standard weapon.
                    this.recipientEntity.Read().MotionControl.StopMoving();
                    return false;
                }

                /// Too far -> start approaching again.
                this.recipientEntity.Read().MotionControl.StartMoving(this.attackedEntity.Read().MotionControl.PositionVector.Read());
                return false;
            }

            /// If target entity is given but target position is not visible -> check if we are still moving towards the target position.
            if (!this.LocatePosition(this.targetPosition.Read()))
            {
                if (!this.recipientEntity.Read().MotionControl.IsMoving)
                {
                    /// Was unable to reach the target position -> attack execution finished.
                    return true;
                }

                /// Still moving -> continue execution.
                return false;
            }

            /// Target position became visible -> check if the target entity is there.
            foreach (Entity entityAtTargetPosition in this.Scenario.GetElementsOnMap<Entity>(this.targetPosition.Read()))
            {
                if (entityAtTargetPosition.ID.Read() == this.targetEntityID.Read())
                {
                    /// Target entity found at target position -> start attack with a standard weapon.
                    this.attackedEntity.Write(entityAtTargetPosition);
                    this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
                    if (this.recipientEntity.Read().Armour.Target != null)
                    {
                        /// Close enough -> not necessary to start approaching.
                        this.recipientEntity.Read().MotionControl.StopMoving();
                        return false;
                    }

                    /// Too far -> start approaching.
                    this.recipientEntity.Read().MotionControl.StartMoving(this.attackedEntity.Read().MotionControl.PositionVector.Read());
                    return false;
                }
            }

            /// Target entity is not at the target position -> attack execution finished.
            return true;
        }

        /// <summary>
        /// Continue the attack-move execution.
        /// </summary>
        /// <returns>True if execution is finished; otherwise false.</returns>
        private bool ContinueAttackMove()
        {
            if (this.attackedEntity.Read() != null)
            {
                /// We have an entity to be attacked -> Check if it still can be located.
                this.attackedEntity.Write(this.LocateEntity(this.attackedEntity.Read().ID.Read()));
                if (this.attackedEntity.Read() == null)
                {
                    /// Attacked entity cannot be located anymore -> continue attack-move towards the target position.
                    this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
                    return false;
                }

                /// Attacked entity still can be located -> Check if it's still in attack range.
                this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
                if (this.recipientEntity.Read().Armour.Target != null)
                {
                    /// Still in attack range -> continue attack with a standard weapon.
                    this.recipientEntity.Read().MotionControl.StopMoving();
                    return false;
                }

                /// Too far -> start approaching again.
                this.recipientEntity.Read().MotionControl.StartMoving(this.attackedEntity.Read().MotionControl.PositionVector.Read());
                return false;
            }

            /// No entity to be attacked -> check if we have reached the target position.
            if (!this.recipientEntity.Read().MotionControl.IsMoving)
            {
                /// Target position reached or unable to reach -> attack execution finished.
                return true;
            }

            /// Still moving towards the target position -> select a nearby enemy if possible.
            this.attackedEntity.Write(this.recipientEntity.Read().Armour.SelectEnemy());
            if (this.attackedEntity.Read() == null)
            {
                /// No enemy could be selected -> continue move to the target position.
                this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
                return false;
            }

            /// We have a new enemy to be attacked -> Start attack with a standard weapon.
            this.recipientEntity.Read().Armour.StartAttack(this.attackedEntity.Read().ID.Read());
            if (this.recipientEntity.Read().Armour.Target != null)
            {
                /// Close enough -> not necessary to start approaching.
                this.recipientEntity.Read().MotionControl.StopMoving();
                return false;
            }

            /// Too far -> start approaching.
            this.recipientEntity.Read().MotionControl.StartMoving(this.attackedEntity.Read().MotionControl.PositionVector.Read());
            return false;
        }

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The target position where the target entity shall be located if given or the target position of the attack-move operation
        /// if target entity is not given.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The ID of the target entity to attack or -1 in case of attack-move execution.
        /// </summary>
        private readonly HeapedValue<int> targetEntityID;

        /// <summary>
        /// Reference to the entity currently being attacked or null if there is no entity currently being attacked.
        /// </summary>
        private readonly HeapedValue<Entity> attackedEntity;

        /// <summary>
        /// The elapsed time since last check operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastCheck;

        /// <summary>
        /// The time between check operations.
        /// </summary>
        private const int TIME_BETWEEN_CHECKS = 12;
    }
}
