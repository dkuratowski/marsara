
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing stop commands.
    /// </summary>
    public class StopExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a StopExecution instance for the given entity.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        public StopExecution(Entity recipientEntity)
            : base(new HashSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.timeSinceLastSearch = this.ConstructField<int>("timeSinceLastSearch");
            this.recipientEntity.Write(recipientEntity);
            this.timeSinceLastSearch.Write(0);

            this.recipientEntity.Read().StopMoving();
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// TODO: check if the recipient entity is hit by an enemy or not!

            /// Select an enemy to be attacked.
            if (this.timeSinceLastSearch.Read() == TIME_BETWEEN_ENEMY_SEARCHES)
            {
                Entity enemyToAttack = this.SelectEnemyToAttack();
                if (enemyToAttack != null)
                {
                    /// Enemy found -> stop execution finished, attack the enemy.
                    new AttackExecution(this.recipientEntity.Read(), this.recipientEntity.Read().PositionValue.Read(), enemyToAttack.ID.Read());
                    return true;
                }
                this.timeSinceLastSearch.Write(0);
                return false;
            }

            this.timeSinceLastSearch.Write(this.timeSinceLastSearch.Read() + 1);
            return false;
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        public override string CommandBeingExecuted { get { return "Stop"; } }

        #endregion Overrides

        /// <summary>
        /// Selects an enemy that can be attacked by the recipient entity.
        /// </summary>
        /// <returns>An enemy that can be attacked by the recipient entity or null if no such enemy has been found.</returns>
        private Entity SelectEnemyToAttack()
        {
            /// TODO: check the real weapons of the recipient entity in the future instead of weapon metadata.
            IWeaponData airWeapon = this.recipientEntity.Read().ElementType.AirWeapon;
            IWeaponData groundWeapon = this.recipientEntity.Read().ElementType.GroundWeapon;
            if (airWeapon == null && groundWeapon == null)
            {
                /// The recipient entity has no weapons -> no enemy can be attacked.
                return null;
            }

            /// Select the nearest enemy.
            RCNumber nearestEnemyDistance = 0;
            Entity nearestEnemy = null;
            foreach (Entity locatedEntity in this.recipientEntity.Read().Locator.LocateEntities())
            {
                if (locatedEntity.Owner == null || locatedEntity.Owner == this.recipientEntity.Read().Owner) { continue; }
                if (locatedEntity.IsFlying && airWeapon == null) { continue; }
                if (!locatedEntity.IsFlying && groundWeapon == null) { continue; }

                if (nearestEnemy == null) { nearestEnemy = locatedEntity; }
                else
                {
                    RCNumber distance = MapUtils.ComputeDistance(this.recipientEntity.Read().BoundingBox, locatedEntity.BoundingBox);
                    if (distance < nearestEnemyDistance) { nearestEnemy = locatedEntity; }
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

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
