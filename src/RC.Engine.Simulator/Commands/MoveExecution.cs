using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing move commands.
    /// </summary>
    public class MoveExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a MoveExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the entity to follow or -1 if no such entity is defined.</param>
        public MoveExecution(Entity recipientEntity, RCNumVector targetPosition, int targetEntityID)
            : base(new HashSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.targetEntity = this.ConstructField<Entity>("targetEntity");
            this.timeSinceLastCheck = this.ConstructField<int>("timeSinceLastCheck");
            this.recipientEntity.Write(recipientEntity);
            this.timeSinceLastCheck.Write(0);

            this.targetEntity.Write(this.LocateTargetEntity(targetEntityID));
            if (this.targetEntity.Read() == null)
            {
                /// Target entity is not defined or could not be located -> simply move to the target position.
                this.recipientEntity.Read().StartMoving(targetPosition);
            }
            else
            {
                /// Target entity is defined and could be located -> calculate its distance from the recipient entity.
                RCNumber distance = MapUtils.ComputeDistance(this.recipientEntity.Read().BoundingBox, this.targetEntity.Read().BoundingBox);
                if (distance <= MAX_DISTANCE)
                {
                    /// Close enough -> not necessary to start approaching.
                    this.recipientEntity.Read().StopMoving();
                }
                else
                {
                    /// Too far -> start approaching
                    this.recipientEntity.Read().StartMoving(this.targetEntity.Read().PositionValue.Read());
                }
            }
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// Check if we have to do anything in this frame.
            if (this.timeSinceLastCheck.Read() < TIME_BETWEEN_DISTANCE_CHECKS)
            {
                /// Nothing to do now.
                this.timeSinceLastCheck.Write(this.timeSinceLastCheck.Read() + 1);
                return false;
            }

            /// Perform a state refresh in this frame.
            this.timeSinceLastCheck.Write(0);
            if (this.targetEntity.Read() == null)
            {
                /// No target to follow -> simple move operation without any target entity.
                return this.ContinueMove();
            }
            else
            {
                /// Continue follow the target.
                this.ContinueFollow();
                return false;
            }
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        public override string CommandBeingExecuted { get { return "Move"; } }

        #endregion Overrides

        /// <summary>
        /// Continue the execution in case of a simple move command without any target entity.
        /// </summary>
        /// <returns>True if execution is finished; otherwise false.</returns>
        private bool ContinueMove()
        {
            if (!this.recipientEntity.Read().IsMoving)
            {
                new StopExecution(this.recipientEntity.Read());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Continue the execution in case of a follow command.
        /// </summary>
        /// <returns>True if execution is finished; otherwise false.</returns>
        private void ContinueFollow()
        {
            /// Check if target entity still can be located.
            this.targetEntity.Write(this.LocateTargetEntity(this.targetEntity.Read().ID.Read()));
            if (this.targetEntity.Read() == null) { return; }

            /// Calculate its distance from the recipient entity.
            RCNumber distance = MapUtils.ComputeDistance(this.recipientEntity.Read().BoundingBox, this.targetEntity.Read().BoundingBox);
            if (distance <= MAX_DISTANCE)
            {
                /// Close enough -> stop the recipient entity.
                this.recipientEntity.Read().StopMoving();
            }
            else
            {
                /// Too far -> start approaching again.
                this.recipientEntity.Read().StartMoving(this.targetEntity.Read().PositionValue.Read());
            }
        }

        /// <summary>
        /// Tries to locate the target entity using the locators of friendly entities.
        /// </summary>
        /// <returns>The target entity or null if the target entity could not be located by friendly entities.</returns>
        private Entity LocateTargetEntity(int targetEntityID)
        {
            /// First we check if the target entity is even on the map.
            Entity targetEntity = this.recipientEntity.Read().Scenario.GetEntityOnMap<Entity>(targetEntityID);
            if (targetEntity == null) { return null; }

            /// Check if the target entity is friendly.
            if (targetEntity.Owner == this.recipientEntity.Read().Owner) { return targetEntity; }

            /// Otherwise we search for friendly entities nearby the target entity and ask their locators.
            foreach (Entity nearbyEntity in targetEntity.Locator.SearchNearbyEntities(TARGET_ENTITY_SEARCH_RADIUS))
            {
                /// Ignore nearby entity if non-friendly.
                if (nearbyEntity.Owner != this.recipientEntity.Read().Owner) { continue; }

                /// Target entity located successfully if any of the nearby friendly entities can locate it.
                if (nearbyEntity.Locator.LocateEntities().Contains(targetEntity)) { return targetEntity; }
            }

            /// Target entity could not be located by any of nearby friendly entities.
            return null;
        }

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// Reference to the target entity to follow or null if the target entity has not yet been found.
        /// </summary>
        private readonly HeapedValue<Entity> targetEntity;

        /// <summary>
        /// The elapsed time since last distance check operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastCheck;

        /// <summary>
        /// The radius of the search area around the target entity when locating it given in quadratic tiles.
        /// </summary>
        private static readonly int TARGET_ENTITY_SEARCH_RADIUS = 15;

        /// <summary>
        /// The maximum allowed distance between the recipient and the target entities in cells.
        /// </summary>
        private static readonly RCNumber MAX_DISTANCE = 12;

        /// <summary>
        /// The time between distance check operations.
        /// </summary>
        private const int TIME_BETWEEN_DISTANCE_CHECKS = 12;
    }
}
