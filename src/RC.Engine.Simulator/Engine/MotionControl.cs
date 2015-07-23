using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Responsible for controlling the motion of a given entity.
    /// </summary>
    public class MotionControl : HeapedObject
    {
        /// <summary>
        /// Constructs a motion control instance for the given entity.
        /// </summary>
        /// <param name="owner">The owner of this motion control.</param>
        public MotionControl(Entity owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = this.ConstructField<Entity>("owner");
            this.position = this.ConstructField<RCNumVector>("position");
            this.velocity = this.ConstructField<RCNumVector>("velocity");
            this.pathTracker = this.ConstructField<PathTrackerBase>("pathTracker");

            this.owner.Write(owner);
            this.position.Write(RCNumVector.Undefined);
            this.velocity.Write(new RCNumVector(0, 0));
            this.pathTracker.Write(null);
            this.velocityGraph = null;
        }

        /// <summary>
        /// Gets whether this motion control is currently performing a move operation or not.
        /// </summary>
        public bool IsMoving
        {
            get
            {
                return this.pathTracker.Read() != null && this.pathTracker.Read().TargetPosition != RCNumVector.Undefined;
            }
        }

        /// <summary>
        /// Gets the position vector managed by this motion control.
        /// </summary>
        public IValueRead<RCNumVector> PositionVector { get { return this.position; } }

        /// <summary>
        /// Gets the velocity vector managed by this motion control.
        /// </summary>
        public IValueRead<RCNumVector> VelocityVector { get { return this.velocity; } }

        /// <summary>
        /// Orders this motion control to start moving to the given position.
        /// </summary>
        /// <param name="toCoords">The target position.</param>
        public void StartMoving(RCNumVector toCoords)
        {
            if (toCoords == RCNumVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (this.pathTracker.Read() != null) { this.pathTracker.Read().TargetPosition = toCoords; }
        }

        /// <summary>
        /// Orders this motion control to stop at its current position.
        /// </summary>
        public void StopMoving()
        {
            if (this.pathTracker.Read() != null) { this.pathTracker.Read().TargetPosition = RCNumVector.Undefined; }
        }
        
        /// <summary>
        /// Sets a new path-tracker for this motion control.
        /// </summary>
        /// <param name="pathTracker">The path-tracker to be set or null to dispose the current path-tracker.</param>
        /// <exception cref="InvalidOperationException">If this entity has already a path-tracker and the parameter is not null.</exception>
        public void SetPathTracker(PathTrackerBase pathTracker)
        {
            if (this.pathTracker.Read() != null && pathTracker != null) { throw new InvalidOperationException("The motion control already has a path-tracker!"); }

            if (this.pathTracker.Read() != null) { this.pathTracker.Read().Dispose(); }
            this.pathTracker.Write(pathTracker);
        }

        /// <summary>
        /// Sets a new velocity graph for this motion control.
        /// </summary>
        /// <param name="velocityGraph">The velocity graph to be set or null to remove the currently set velocity graph.</param>
        /// <exception cref="InvalidOperationException">If this motion control has already a velocity graph and the parameter is not null.</exception>
        public void SetVelocityGraph(VelocityGraph velocityGraph)
        {
            if (this.velocityGraph != null && velocityGraph != null) { throw new InvalidOperationException("The motion control already has a velocity graph!"); }

            this.velocityGraph = velocityGraph;
        }

        /// <summary>
        /// Updates the state of this motion control.
        /// </summary>
        public void UpdateState()
        {
            /// Update the state of this motion control if it has a velocity graph and a path-tracker and the path-tracker is active.
            if (this.velocityGraph != null &&
                this.pathTracker.Read() != null &&
                this.pathTracker.Read().IsActive)
            {
                /// Update the state of the path-tracker.
                this.pathTracker.Read().Update();

                /// Update the velocity vector.
                this.UpdateVelocity();

                /// Update the position based on the new velocity.
                this.UpdatePosition();
            }
        }

        /// <summary>
        /// Explicitly sets the position of this motion control. A call to this method will automatically stop any moving operation.
        /// </summary>
        /// <param name="newPosition">
        /// The new position or RCNumVector.Undefined to indicate that the owner of this motion control has been removed from the map.
        /// </param>
        /// <returns>True if the position has been set successfully; otherwise false.</returns>
        internal bool SetPosition(RCNumVector newPosition)
        {
            if (newPosition == RCNumVector.Undefined ||
                this.pathTracker.Read() == null ||
                this.pathTracker.Read().ValidatePosition(newPosition))
            {
                this.StopMoving();
                this.position.Write(newPosition);
                return true;
            }
            return false;
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.pathTracker.Read() != null)
            {
                this.pathTracker.Read().Dispose();
                this.pathTracker.Write(null);
            }
        }

        /// <summary>
        /// Updates the velocity vector.
        /// </summary>
        private void UpdateVelocity()
        {
            if (this.pathTracker.Read().IsActive)
            {
                /// Update the velocity of this entity if the path-tracker is still active.
                int currVelocityIndex = 0;
                int bestVelocityIndex = -1;
                RCNumber minPenalty = 0;
                List<RCNumVector> admissibleVelocities = new List<RCNumVector>(this.velocityGraph.GetAdmissibleVelocities(this.velocity.Read()));

                foreach (RCNumVector admissibleVelocity in admissibleVelocities)
                {
                    if (this.pathTracker.Read().ValidateVelocity(admissibleVelocity))
                    {
                        RCNumVector checkedVelocity = admissibleVelocity * 2 - this.velocity.Read(); /// We calculate with RVOs instead of VOs.
                        RCNumber timeToCollisionMin = -1;
                        foreach (DynamicObstacleInfo obstacle in this.pathTracker.Read().DynamicObstacles)
                        {
                            RCNumber timeToCollision = this.CalculateTimeToCollision(this.owner.Read().Area, checkedVelocity, obstacle.Position, obstacle.Velocity);
                            if (timeToCollision >= 0 && (timeToCollisionMin < 0 || timeToCollision < timeToCollisionMin)) { timeToCollisionMin = timeToCollision; }
                        }

                        if (timeToCollisionMin != 0)
                        {
                            RCNumber penalty = (timeToCollisionMin > 0 ? (RCNumber)1 / timeToCollisionMin : 0)
                                             + MapUtils.ComputeDistance(this.pathTracker.Read().PreferredVelocity, admissibleVelocity);
                            if (bestVelocityIndex == -1 || penalty < minPenalty)
                            {
                                minPenalty = penalty;
                                bestVelocityIndex = currVelocityIndex;
                            }
                        }
                    }
                    currVelocityIndex++;
                }

                /// Update the velocity based on the index of the best velocity.
                this.velocity.Write(bestVelocityIndex != -1 ? admissibleVelocities[bestVelocityIndex] : new RCNumVector(0, 0));
            }
            else
            {
                /// Stop the entity if the path-tracker became inactive.
                this.velocity.Write(new RCNumVector(0, 0));
            }
        }

        /// <summary>
        /// Updates the position vector.
        /// </summary>
        private void UpdatePosition()
        {
            if (this.velocity.Read() != new RCNumVector(0, 0))
            {
                RCNumVector newPosition = this.position.Read() + this.velocity.Read();
                if (this.pathTracker.Read().ValidatePosition(newPosition))
                {
                    this.position.Write(newPosition);
                }
                else
                {
                    this.velocity.Write(new RCNumVector(0, 0));
                }
            }
        }

        /// <summary>
        /// Calculates the time to collision between two moving rectangular objects.
        /// </summary>
        /// <param name="rectangleA">The rectangular area of the first object.</param>
        /// <param name="velocityA">The velocity of the first object.</param>
        /// <param name="rectangleB">The rectangular area of the second object.</param>
        /// <param name="velocityB">The velocity of the second object.</param>
        /// <returns>The time to the collision between the two objects or a negative number if the two objects won't collide in the future.</returns>
        private RCNumber CalculateTimeToCollision(RCNumRectangle rectangleA, RCNumVector velocityA, RCNumRectangle rectangleB, RCNumVector velocityB)
        {
            /// Calculate the relative velocity of A with respect to B.
            RCNumVector relativeVelocityOfA = velocityA - velocityB;
            if (relativeVelocityOfA == new RCNumVector(0, 0)) { return -1; }

            /// Calculate the center of the first object and enlarge the area of the second object with the size of the first object.
            RCNumVector centerOfA = new RCNumVector((rectangleA.Left + rectangleA.Right) / 2,
                                                    (rectangleA.Top + rectangleA.Bottom) / 2);
            RCNumRectangle enlargedB = new RCNumRectangle(rectangleB.X - rectangleA.Width / 2,
                                                          rectangleB.Y - rectangleA.Height / 2,
                                                          rectangleA.Width + rectangleB.Width,
                                                          rectangleA.Height + rectangleB.Height);

            /// Calculate the collision time interval in the X dimension.
            bool isParallelX = relativeVelocityOfA.X == 0;
            RCNumber collisionTimeLeft = !isParallelX ? (enlargedB.Left - centerOfA.X) / relativeVelocityOfA.X : 0;
            RCNumber collisionTimeRight = !isParallelX ? (enlargedB.Right - centerOfA.X) / relativeVelocityOfA.X : 0;
            RCNumber collisionTimeBeginX = !isParallelX ? (collisionTimeLeft < collisionTimeRight ? collisionTimeLeft : collisionTimeRight) : 0;
            RCNumber collisionTimeEndX = !isParallelX ? (collisionTimeLeft > collisionTimeRight ? collisionTimeLeft : collisionTimeRight) : 0;

            /// Calculate the collision time interval in the Y dimension.
            bool isParallelY = relativeVelocityOfA.Y == 0;
            RCNumber collisionTimeTop = !isParallelY ? (enlargedB.Top - centerOfA.Y) / relativeVelocityOfA.Y : 0;
            RCNumber collisionTimeBottom = !isParallelY ? (enlargedB.Bottom - centerOfA.Y) / relativeVelocityOfA.Y : 0;
            RCNumber collisionTimeBeginY = !isParallelY ? (collisionTimeTop < collisionTimeBottom ? collisionTimeTop : collisionTimeBottom) : 0;
            RCNumber collisionTimeEndY = !isParallelY ? (collisionTimeTop > collisionTimeBottom ? collisionTimeTop : collisionTimeBottom) : 0;

            if (!isParallelX && !isParallelY)
            {
                /// Both X and Y dimensions have finite collision time interval.
                if (collisionTimeBeginX <= collisionTimeBeginY && collisionTimeBeginY < collisionTimeEndX && collisionTimeEndX <= collisionTimeEndY)
                {
                    return collisionTimeBeginY;
                }
                else if (collisionTimeBeginY <= collisionTimeBeginX && collisionTimeBeginX < collisionTimeEndX && collisionTimeEndX <= collisionTimeEndY)
                {
                    return collisionTimeBeginX;
                }
                else if (collisionTimeBeginY <= collisionTimeBeginX && collisionTimeBeginX < collisionTimeEndY && collisionTimeEndY <= collisionTimeEndX)
                {
                    return collisionTimeBeginX;
                }
                else if (collisionTimeBeginX <= collisionTimeBeginY && collisionTimeBeginY < collisionTimeEndY && collisionTimeEndY <= collisionTimeEndX)
                {
                    return collisionTimeBeginY;
                }
                else
                {
                    return -1;
                }
            }
            else if (!isParallelX && isParallelY)
            {
                /// Only X dimension has finite collision time interval.
                return centerOfA.Y > enlargedB.Top && centerOfA.Y < enlargedB.Bottom ? collisionTimeBeginX : -1;
            }
            else if (isParallelX && !isParallelY)
            {
                /// Only Y dimension has finite collision time interval.
                return centerOfA.X > enlargedB.Left && centerOfA.X < enlargedB.Right ? collisionTimeBeginY : -1;
            }
            else
            {
                /// None of the dimensions have finite collision time interval.
                return -1;
            }
        }

        /// <summary>
        /// Reference to the owner of this motion control.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// The position of this motion control.
        /// </summary>
        private readonly HeapedValue<RCNumVector> position;

        /// <summary>
        /// The velocity of this motion control.
        /// </summary>
        private readonly HeapedValue<RCNumVector> velocity;

        /// <summary>
        /// Reference to the path-tracker of this motion control.
        /// </summary>
        private readonly HeapedValue<PathTrackerBase> pathTracker;

        /// <summary>
        /// Reference to the velocity graph of this motion control.
        /// </summary>
        private VelocityGraph velocityGraph;
    }
}
