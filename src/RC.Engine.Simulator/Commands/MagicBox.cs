using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Helper class for command executions that must keep formation of the recipient entities.
    /// </summary>
    public class MagicBox
    {
        /// <summary>
        /// Constructs a MagicBox instance for the given recipient entities and target position.
        /// </summary>
        /// <param name="recipientEntities">The recipient entities.</param>
        /// <param name="targetPosition">The target position.</param>
        public MagicBox(RCSet<Entity> recipientEntities, RCNumVector targetPosition)
        {
            this.commonTargetPosition = RCNumVector.Undefined;
            this.targetPositions = new Dictionary<Entity, RCNumVector>();

            /// Check if we shall keep formation of the recipient entities.
            RCNumRectangle boundingBox = this.CalculateBoundingBox(recipientEntities);
            if (!boundingBox.Contains(targetPosition))
            {
                RCNumVector boundingBoxCenter = (2 * boundingBox.Location + boundingBox.Size) / 2;
                foreach (Entity entity in recipientEntities)
                {
                    RCNumVector boxLocationToEntityVector = entity.MotionControl.PositionVector.Read() - boundingBox.Location;
                    RCNumVector magicBox = entity.MotionControl.IsFlying ? AIR_MAGIC_BOX : GROUND_MAGIC_BOX;
                    if (boxLocationToEntityVector.X > magicBox.X || boxLocationToEntityVector.Y > magicBox.Y)
                    {
                        /// Entity is outside of the magic box -> don't keep formation.
                        this.commonTargetPosition = targetPosition;
                        break;
                    }

                    /// Calculate the target position of the entity.
                    this.targetPositions[entity] = targetPosition + entity.MotionControl.PositionVector.Read() - boundingBoxCenter;
                }
            }
            else
            {
                /// Target position is inside the bounding box -> don't keep formation.
                this.commonTargetPosition = targetPosition;
            }
        }

        /// <summary>
        /// Gets the target position of the given entity calculated by this magic box.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The target position of the given entity calculated by this magic box.</returns>
        public RCNumVector GetTargetPosition(Entity entity)
        {
            if (this.commonTargetPosition != RCNumVector.Undefined) { return this.commonTargetPosition; }

            if (!this.targetPositions.ContainsKey(entity)) { throw new InvalidOperationException("The given entity is not found for this magic box!"); }
            return this.targetPositions[entity];
        }

        /// <summary>
        /// Calculates the bounding box of the given entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>The bounding box of the given entities.</returns>
        private RCNumRectangle CalculateBoundingBox(RCSet<Entity> entities)
        {
            RCNumber top = -1, left = -1, bottom = -1, right = -1;
            foreach (Entity entity in entities)
            {
                RCNumVector entityPosition = entity.MotionControl.PositionVector.Read();
                if (top == -1 || entityPosition.Y < top) { top = entityPosition.Y; }
                if (left == -1 || entityPosition.X < left) { left = entityPosition.X; }
                if (bottom == -1 || entityPosition.Y > bottom) { bottom = entityPosition.Y; }
                if (right == -1 || entityPosition.X > right) { right = entityPosition.X; }
            }

            if (right - left == 0)
            {
                left -= (RCNumber)1 / (RCNumber)2;
                right += (RCNumber)1 / (RCNumber)2;
            }
            if (bottom - top == 0)
            {
                top -= (RCNumber)1 / (RCNumber)2;
                bottom += (RCNumber)1 / (RCNumber)2;
            }
            return new RCNumRectangle(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// The common target position of the recipient entities if formation doesn't have to be kept; otherwise RCNumVector.Undefined.
        /// </summary>
        private readonly RCNumVector commonTargetPosition;

        /// <summary>
        /// The target positions of the individual entities.
        /// </summary>
        private readonly Dictionary<Entity, RCNumVector> targetPositions;

        /// <summary>
        /// The hardcoded size of ground unit magic boxes.
        /// </summary>
        private static readonly RCNumVector GROUND_MAGIC_BOX = new RCNumVector(195, 195) / 8;

        /// <summary>
        /// The hardcoded size of air unit magic boxes.
        /// </summary>
        private static readonly RCNumVector AIR_MAGIC_BOX = new RCNumVector(255, 255) / 8;
    }
}
