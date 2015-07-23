using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a missile.
    /// </summary>
    class Missile : ScenarioElement
    {
        /// <summary>
        /// Constructs a Missile instance of the given missile type.
        /// </summary>
        /// <param name="missileData">Reference to the missile data.</param>
        /// <param name="sourceEntity">Reference to the entity that is the source of this missile.</param>
        /// <param name="targetEntity">Reference to the entity that is the target of this missile.</param>
        public Missile(IMissileData missileData, Entity sourceEntity, Entity targetEntity)
            : base(missileData.MissileType.Name)
        {
            if (missileData == null) { throw new ArgumentNullException("missileData"); }
            this.missileData = missileData;

            if (sourceEntity == null) { throw new ArgumentNullException("sourceEntity"); }
            if (!sourceEntity.HasMapObject) { throw new ArgumentException("Source entity is not attached to the map!", "sourceEntity"); }
            if (targetEntity == null) { throw new ArgumentNullException("sourceEntity"); }
            if (!targetEntity.HasMapObject) { throw new ArgumentException("Target entity is not attached to the map!", "targetEntity"); }

            this.sourceEntity = this.ConstructField<Entity>("sourceEntity");
            this.targetEntity = this.ConstructField<Entity>("targetEntity");
            this.lastKnownSourceEntityPos = this.ConstructField<RCNumVector>("lastKnownSourceEntityPos");
            this.lastKnownTargetEntityPos = this.ConstructField<RCNumVector>("lastKnownTargetEntityPos");
            this.missilePosition = this.ConstructField<RCNumVector>("missilePosition");
            this.missileVelocity = this.ConstructField<RCNumVector>("missileHeading");
            this.timer = this.ConstructField<int>("timer");
            this.currentStatus = this.ConstructField<byte>("currentStatus");

            this.sourceEntity.Write(sourceEntity);
            this.targetEntity.Write(targetEntity);
            this.lastKnownSourceEntityPos.Write(RCNumVector.Undefined);
            this.lastKnownTargetEntityPos.Write(RCNumVector.Undefined);
            this.missilePosition.Write(RCNumVector.Undefined);
            this.missileVelocity.Write(RCNumVector.Undefined);
            this.timer.Write(0);
            this.currentStatus.Write((byte)Status.Launching);

            this.launchIndicator = null;
            this.missileIndicator = null;
            //this.trailIndicators = new List<MapObject>();
            this.impactIndicator = null;

            this.sourceEntityHeading = new MapDirValueSrcWrapper(new HeadingToMapDirConverter(sourceEntity.Armour.TargetVector));
            this.velocityGraph = this.missileData.MissileType.Speed != null
                ? new HexadecagonalVelocityGraph(this.missileData.MissileType.Speed.Read())
                : null;
        }

        /// <summary>
        /// This event is raised when this missile has been launched successfully.
        /// </summary>
        public event Action<Missile> Launch;

        /// <summary>
        /// This event is raised when the launch of this missile has been cancelled for some reason.
        /// </summary>
        public event Action<Missile> LaunchCancel;

        /// <summary>
        /// This event is raised when this missile has impacted.
        /// </summary>
        public event Action<Missile> Impact;

        /// <see cref="ScenarioElement.UpdateStateImpl"/>
        protected override RCSet<ScenarioElement> UpdateStateImpl()
        {
            RCSet<ScenarioElement> response = null;
            if ((Status)this.currentStatus.Read() == Status.Launching)
            {
                response = this.UpdateLaunchingState();
            }
            else if ((Status)this.currentStatus.Read() == Status.Launched)
            {
                response = this.UpdateLaunchedState();
            }
            else
            {
                response = this.UpdateImpactedState();
            }

            this.timer.Write(this.timer.Read() + 1);

            /// Do not exceeed the maximum lifetime!
            if (this.timer.Read() >= MAX_LIFETIME)
            {
                if (this.launchIndicator != null)
                {
                    this.DestroyMapObject(this.launchIndicator);
                    this.launchIndicator = null;
                }
                if (this.missileIndicator != null)
                {
                    this.DestroyMapObject(this.missileIndicator);
                    this.missileIndicator = null;
                }
                if (this.impactIndicator != null)
                {
                    this.DestroyMapObject(this.impactIndicator);
                    this.impactIndicator = null;
                }
                return null;
            }

            return response;
        }

        /// <see cref="ScenarioElement.UpdateMapObjectsImpl"/>
        protected override void UpdateMapObjectsImpl()
        {
            /// Remove the launch indicator if the source entity is removed from the map.
            if (!this.sourceEntity.Read().HasMapObject)
            {
                if (this.launchIndicator != null)
                {
                    this.DestroyMapObject(this.launchIndicator);
                    this.launchIndicator = null;
                }
            }
            else if (this.launchIndicator != null)
            {
                /// Remove the launch indicator if any of its animations has finished.
                if (this.launchIndicator.CurrentAnimations.Any(anim => anim.IsFinished))
                {
                    this.DestroyMapObject(this.launchIndicator);
                    this.launchIndicator = null;
                }
                /// TODO: update the launch indicator position if the target vector or the position of the source entity has changed!
            }

            /// Remove the impact indicator if any of its animations has finished.
            if (this.impactIndicator != null && this.impactIndicator.CurrentAnimations.Any(anim => anim.IsFinished))
            {
                this.DestroyMapObject(this.impactIndicator);
                this.impactIndicator = null;
            }
        }

        /// <summary>
        /// Enumerates the possible states of a missile.
        /// </summary>
        private enum Status
        {
            Launching = 0,      /// The missile is preparing to launch.
            Launched = 1,       /// The missile has been launched.
            Impacted = 2,       /// The missile has been impacted.
        }

        /// <summary>
        /// Updates this missile if it is in Launching state.
        /// </summary>
        private RCSet<ScenarioElement> UpdateLaunchingState()
        {
            /// Cancel the launch procedure if necessary.
            if (!this.sourceEntity.Read().HasMapObject || !this.targetEntity.Read().HasMapObject)
            {
                if (this.LaunchCancel != null) { this.LaunchCancel(this); }
                if (this.launchIndicator != null)
                {
                    this.DestroyMapObject(this.launchIndicator);
                    this.launchIndicator = null;
                }
                return null;
            }

            /// Create a launch indicator map object if it has not yet been created and the missile type defines a launch animation.
            if (this.launchIndicator == null && this.missileData.MissileType.LaunchAnimation != null)
            {
                this.launchIndicator = this.CreateMapObject(this.CalculateArea(this.CalculateLaunchPosition()));
                this.launchIndicator.SetCurrentAnimation(this.missileData.MissileType.LaunchAnimation, this.sourceEntity.Read().Armour.TargetVector);
                this.lastKnownSourceEntityPos.Write(this.sourceEntity.Read().MotionControl.PositionVector.Read());
            }

            /// Launch the missile if the timer reached the launch delay; otherwise increment the timer.
            if (this.timer.Read() >= this.missileData.MissileType.LaunchDelay)
            {
                if (this.Launch != null) { this.Launch(this); }
                this.lastKnownTargetEntityPos.Write(this.targetEntity.Read().MotionControl.PositionVector.Read());
                this.missilePosition.Write(this.CalculateLaunchPosition());
                this.UpdateVelocity();
                this.currentStatus.Write((byte)Status.Launched);
            }

            return new RCSet<ScenarioElement>();
        }

        /// <summary>
        /// Updates this missile if it is in Launched state.
        /// </summary>
        private RCSet<ScenarioElement> UpdateLaunchedState()
        {
            /// Update the target entity position if it is still on the map.
            if (this.targetEntity.Read().HasMapObject) { this.lastKnownTargetEntityPos.Write(this.targetEntity.Read().MotionControl.PositionVector.Read()); }

            /// Move immediately to the Impacted state if this is an instant missile.
            /// Note: a missile is instant if it has no speed defined.
            if (this.missileData.MissileType.Speed == null)
            {
                if (this.Impact != null) { this.Impact(this); }
                this.currentStatus.Write((byte)Status.Impacted);
                if (this.missileIndicator != null)
                {
                    this.DestroyMapObject(this.missileIndicator);
                    this.missileIndicator = null;
                }
                return new RCSet<ScenarioElement>();
            }

            /// Check if the missile impacts.
            if (MapUtils.ComputeDistance(this.missilePosition.Read(), this.lastKnownTargetEntityPos.Read()) <=
                this.missileData.MissileType.Speed.Read())
            {
                if (this.Impact != null) { this.Impact(this); }
                this.currentStatus.Write((byte)Status.Impacted);
                if (this.missileIndicator != null)
                {
                    this.DestroyMapObject(this.missileIndicator);
                    this.missileIndicator = null;
                }
                return new RCSet<ScenarioElement>();
            }

            /// Calculate the new position and velocity of the missile.
            this.UpdateVelocity();
            this.missilePosition.Write(this.missilePosition.Read() + this.missileVelocity.Read());

            /// Create a missile indicator map object if it has not yet been created and the missile type defines a flying animation...
            if (this.missileIndicator == null && this.missileData.MissileType.FlyingAnimation != null)
            {
                this.missileIndicator = this.CreateMapObject(this.CalculateArea(this.missilePosition.Read()));
                this.missileIndicator.SetCurrentAnimation(this.missileData.MissileType.FlyingAnimation, this.missileVelocity);
            }
            else if (this.missileIndicator != null)
            {
                /// ... or update its position if already exists.
                this.missileIndicator.SetLocation(this.CalculateArea(this.missilePosition.Read()));
            }
            return new RCSet<ScenarioElement>();
        }

        /// <summary>
        /// Updates this missile if it is in Impacted state.
        /// </summary>
        private RCSet<ScenarioElement> UpdateImpactedState()
        {
            /// Update the target entity position if it is still on the map.
            if (this.targetEntity.Read().HasMapObject) { this.lastKnownTargetEntityPos.Write(this.targetEntity.Read().MotionControl.PositionVector.Read()); }

            /// Create an impact indicator map object if it has not yet been created and the missile type defines an impact animation.
            if (this.impactIndicator == null && this.missileData.MissileType.ImpactAnimation != null)
            {
                this.impactIndicator = this.CreateMapObject(this.CalculateArea(this.lastKnownTargetEntityPos.Read()));
                this.impactIndicator.SetCurrentAnimation(this.missileData.MissileType.ImpactAnimation, this.missileVelocity);
            }

            /// Check if the lifecycle of this missile has already ended.
            if (this.launchIndicator == null && this.missileIndicator == null && this.impactIndicator == null)
            {
                return null;
            }

            return new RCSet<ScenarioElement>();
        }

        /// <summary>
        /// Calculates the launch position of this missile.
        /// </summary>
        /// <returns>The calculated launch position of this missile.</returns>
        private RCNumVector CalculateLaunchPosition()
        {
            return this.sourceEntity.Read().MotionControl.PositionVector.Read() +
                   this.missileData.GetRelativeLaunchPosition(this.sourceEntityHeading.Read());
        }

        /// <summary>
        /// Updates the velocity of this missile.
        /// </summary>
        private void UpdateVelocity()
        {
            RCNumVector vectorToTarget = this.lastKnownTargetEntityPos.Read() - this.missilePosition.Read();

            int currVelocityIndex = 0;
            int bestVelocityIndex = -1;
            RCNumber minDistanceToTarget = 0;
            List<RCNumVector> admissibleVelocities =
                new List<RCNumVector>(
                    this.velocityGraph.GetAdmissibleVelocities(this.missileVelocity.Read() != RCNumVector.Undefined
                        ? this.missileVelocity.Read()
                        : vectorToTarget));
            foreach (RCNumVector admissibleVelocity in admissibleVelocities)
            {
                RCNumber distanceToTarget = MapUtils.ComputeDistance(vectorToTarget, admissibleVelocity);
                if (bestVelocityIndex == -1 || distanceToTarget < minDistanceToTarget)
                {
                    minDistanceToTarget = distanceToTarget;
                    bestVelocityIndex = currVelocityIndex;
                }
                currVelocityIndex++;
            }

            if (bestVelocityIndex == -1) { throw new InvalidOperationException("Unable to select best velocity for missile!"); }
            this.missileVelocity.Write(admissibleVelocities[bestVelocityIndex]);
        }

        #region Heaped members

        /// <summary>
        /// Reference to the entity that is the source of this missile.
        /// </summary>
        private readonly HeapedValue<Entity> sourceEntity;

        /// <summary>
        /// Reference to the entity that is the target of this missile.
        /// </summary>
        private readonly HeapedValue<Entity> targetEntity;

        /// <summary>
        /// The last known position of the source entity.
        /// </summary>
        private readonly HeapedValue<RCNumVector> lastKnownSourceEntityPos;

        /// <summary>
        /// The last known position of the target entity.
        /// </summary>
        private readonly HeapedValue<RCNumVector> lastKnownTargetEntityPos;

        /// <summary>
        /// The current position of this missile or RCNumVector.Undefined if it has not yet been launched.
        /// </summary>
        private readonly HeapedValue<RCNumVector> missilePosition;

        /// <summary>
        /// The current velocity vector of this missile or RCNumVector.Undefined if it has not yet been launched.
        /// </summary>
        private readonly HeapedValue<RCNumVector> missileVelocity;

        /// <summary>
        /// The timer that is used to delay launch and to delay trail animations.
        /// </summary>
        private readonly HeapedValue<int> timer;

        /// <summary>
        /// The current status of this missile.
        /// </summary>
        private readonly HeapedValue<byte> currentStatus;

        #endregion Heaped members

        #region Indicators

        /// <summary>
        /// Reference to the map object that indicates the launch of this missile.
        /// </summary>
        private MapObject launchIndicator;

        /// <summary>
        /// Reference to the map object that indicates this missile itself.
        /// </summary>
        private MapObject missileIndicator;

        ///// <summary>
        ///// Reference to the map objects that indicates the trail of this missile.
        ///// </summary>
        /// TODO: Trail indicators are not displayed currently to reduce complexity!
        //private readonly List<MapObject> trailIndicators;

        /// <summary>
        /// Reference to the map object that indicates the impact of this missile.
        /// </summary>
        private MapObject impactIndicator;

        #endregion Indicators

        /// <summary>
        /// Reference to the missile data.
        /// </summary>
        private readonly IMissileData missileData;

        /// <summary>
        /// The heading of the source entity.
        /// </summary>
        private readonly IValueRead<MapDirection> sourceEntityHeading;

        /// <summary>
        /// The velocity graph of this missile.
        /// </summary>
        private readonly VelocityGraph velocityGraph;

        /// <summary>
        /// The maximum duration of the lifetime of a missile.
        /// </summary>
        private const int MAX_LIFETIME = 250;
    }
}
