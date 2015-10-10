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
    public class Missile : ScenarioElement
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
            if (!sourceEntity.HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects)) { throw new ArgumentException("Source entity is not attached to the map!", "sourceEntity"); }
            if (targetEntity == null) { throw new ArgumentNullException("sourceEntity"); }
            if (!targetEntity.HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects)) { throw new ArgumentException("Target entity is not attached to the map!", "targetEntity"); }

            this.sourceEntity = this.ConstructField<Entity>("sourceEntity");
            this.targetEntity = this.ConstructField<Entity>("targetEntity");
            this.lastKnownTargetEntityPos = this.ConstructField<RCNumVector>("lastKnownTargetEntityPos");
            this.lastKnownTargetEntityIsFlying = this.ConstructField<byte>("lastKnownTargetEntityIsFlying");
            this.missilePosition = this.ConstructField<RCNumVector>("missilePosition");
            this.launchPosition = this.ConstructField<RCNumVector>("launchPosition");
            this.launchedFromAir = this.ConstructField<byte>("launchedFromAir");
            this.missileVelocity = this.ConstructField<RCNumVector>("missileVelocity");
            this.timer = this.ConstructField<int>("timer");
            this.currentStatus = this.ConstructField<byte>("currentStatus");

            this.sourceEntity.Write(sourceEntity);
            this.targetEntity.Write(targetEntity);
            this.lastKnownTargetEntityPos.Write(RCNumVector.Undefined);
            this.lastKnownTargetEntityIsFlying.Write(0x00);
            this.missilePosition.Write(RCNumVector.Undefined);
            this.launchPosition.Write(RCNumVector.Undefined);
            this.launchedFromAir.Write(0x00);
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

        #region Public interface

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

        /// <summary>
        /// Gets the target entity of this missile.
        /// </summary>
        public Entity TargetEntity { get { return this.targetEntity.Read(); } }

        /// <summary>
        /// Gets the launch position of this missile or RCNumVector.Undefined if this missile has not yet been launched.
        /// </summary>
        public RCNumVector LaunchPosition { get { return this.launchPosition.Read(); } }

        /// <summary>
        /// Gets whether this missile has been launched from air or not.
        /// </summary>
        public bool LaunchedFromAir { get { return this.launchedFromAir.Read() == 0x01; } }

        #endregion Public interface

        #region Overrides

        /// <see cref="ScenarioElement.UpdateStateImpl"/>
        public override RCNumVector DetachFromMap()
        {
            if (this.Owner != null) { this.OnRemovedFromPlayer(); }

            if (this.launchIndicator != null && !this.launchIndicator.IsDestroyed)
            {
                this.DestroyMapObject(this.launchIndicator);
            }
            if (this.missileIndicator != null && !this.missileIndicator.IsDestroyed)
            {
                this.DestroyMapObject(this.missileIndicator);
            }
            if (this.impactIndicator != null && !this.impactIndicator.IsDestroyed)
            {
                this.DestroyMapObject(this.impactIndicator);
            }

            return this.missilePosition.Read();
        }

        /// <see cref="ScenarioElement.UpdateStateImpl"/>
        protected override void UpdateStateImpl()
        {
            bool keepAlive = false;
            if ((Status)this.currentStatus.Read() == Status.Launching)
            {
                keepAlive = this.UpdateLaunchingState();
            }
            else if ((Status)this.currentStatus.Read() == Status.Launched)
            {
                keepAlive = this.UpdateLaunchedState();
            }
            else
            {
                keepAlive = this.UpdateImpactedState();
            }

            if (!keepAlive)
            {
                this.DetachFromMap();
                this.Scenario.RemoveElementFromScenario(this);
                return;
            }

            this.timer.Write(this.timer.Read() + 1);

            /// Do not exceeed the maximum lifetime!
            if (this.timer.Read() >= MAX_LIFETIME)
            {
                this.DetachFromMap();
                this.Scenario.RemoveElementFromScenario(this);
                return;
            }
        }

        /// <see cref="ScenarioElement.UpdateMapObjectsImpl"/>
        protected override void UpdateMapObjectsImpl()
        {
            /// Remove the launch indicator if the source entity is removed from the map.
            if (!this.sourceEntity.Read().HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects))
            {
                if (this.launchIndicator != null && !this.launchIndicator.IsDestroyed)
                {
                    this.DestroyMapObject(this.launchIndicator);
                }
            }
            else if (this.launchIndicator != null && !this.launchIndicator.IsDestroyed)
            {
                /// Remove the launch indicator if any of its animations has finished.
                if (!this.launchIndicator.HasAnyAnimations)
                {
                    this.DestroyMapObject(this.launchIndicator);
                }
                /// TODO: update the launch indicator position if the target vector or the position of the source entity has changed!
            }

            /// Remove the impact indicator if any of its animations has finished.
            if (this.impactIndicator != null && !this.impactIndicator.IsDestroyed && !this.impactIndicator.HasAnyAnimations)
            {
                this.DestroyMapObject(this.impactIndicator);
            }
        }

        #endregion Overrides

        #region Private methods

        /// <summary>
        /// Updates this missile if it is in Launching state.
        /// </summary>
        private bool UpdateLaunchingState()
        {
            /// Cancel the launch procedure if necessary.
            if (this.LaunchHasToBeCancelled())
            {
                if (this.LaunchCancel != null) { this.LaunchCancel(this); }
                if (this.launchIndicator != null && !this.launchIndicator.IsDestroyed)
                {
                    this.DestroyMapObject(this.launchIndicator);
                }
                return false;
            }

            /// Create a launch indicator map object if it has not yet been created and the missile type defines a launch animation.
            if (this.launchIndicator == null && this.missileData.MissileType.LaunchAnimation != null)
            {
                this.launchIndicator = this.CreateMapObject(this.CalculateArea(this.CalculateLaunchPosition()), this.sourceEntity.Read().MotionControl.IsFlying ? MapObjectLayerEnum.AirMissiles : MapObjectLayerEnum.GroundMissiles);
                this.launchIndicator.StartAnimation(this.missileData.MissileType.LaunchAnimation, this.sourceEntity.Read().Armour.TargetVector);
            }

            /// Launch the missile if the timer reached the launch delay; otherwise increment the timer.
            if (this.timer.Read() >= this.missileData.MissileType.LaunchDelay)
            {
                if (this.Launch != null) { this.Launch(this); }
                this.lastKnownTargetEntityPos.Write(this.targetEntity.Read().MotionControl.PositionVector.Read());
                this.lastKnownTargetEntityIsFlying.Write(this.targetEntity.Read().MotionControl.IsFlying ? (byte)0x01 : (byte)0x00);
                this.missilePosition.Write(this.CalculateLaunchPosition());
                this.launchPosition.Write(this.missilePosition.Read());
                this.launchedFromAir.Write(this.sourceEntity.Read().MotionControl.IsFlying ? (byte)0x01 : (byte)0x00);
                if (this.sourceEntity.Read().Owner != null) { this.OnAddedToPlayer(this.sourceEntity.Read().Owner); }
                this.UpdateVelocity();
                this.currentStatus.Write((byte)Status.Launched);
            }

            return true;
        }

        /// <summary>
        /// Updates this missile if it is in Launched state.
        /// </summary>
        private bool UpdateLaunchedState()
        {
            /// Update the target entity position and height if it is still on the map.
            if (this.targetEntity.Read().HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects))
            {
                this.lastKnownTargetEntityPos.Write(this.targetEntity.Read().MotionControl.PositionVector.Read());
                this.lastKnownTargetEntityIsFlying.Write(this.targetEntity.Read().MotionControl.IsFlying ? (byte)0x01 : (byte)0x00);
            }

            /// Move immediately to the Impacted state if this is an instant missile.
            /// Note: a missile is instant if it has no speed defined.
            if (this.missileData.MissileType.Speed == null)
            {
                if (this.Impact != null) { this.Impact(this); }
                this.currentStatus.Write((byte)Status.Impacted);
                if (this.missileIndicator != null && !this.missileIndicator.IsDestroyed)
                {
                    this.DestroyMapObject(this.missileIndicator);
                }
                return true;
            }

            /// Calculate the new position and velocity of the missile.
            this.UpdateVelocity();
            this.missilePosition.Write(this.missilePosition.Read() + this.missileVelocity.Read());

            /// Check if the missile impacts.
            if (MapUtils.ComputeDistance(this.missilePosition.Read(), this.lastKnownTargetEntityPos.Read()) <= this.missileData.MissileType.Speed.Read())
            {
                if (this.Impact != null) { this.Impact(this); }
                this.currentStatus.Write((byte)Status.Impacted);
                if (this.missileIndicator != null && !this.missileIndicator.IsDestroyed)
                {
                    this.DestroyMapObject(this.missileIndicator);
                }
                return true;
            }

            /// Create a missile indicator map object if it has not yet been created and the missile type defines a flying animation...
            if (this.missileIndicator == null && this.missileData.MissileType.FlyingAnimation != null)
            {
                this.missileIndicator = this.CreateMapObject(this.CalculateArea(this.missilePosition.Read()), this.lastKnownTargetEntityIsFlying.Read() == 0x01 ? MapObjectLayerEnum.AirMissiles : MapObjectLayerEnum.GroundMissiles);
                this.missileIndicator.StartAnimation(this.missileData.MissileType.FlyingAnimation, this.missileVelocity);
            }
            else if (this.missileIndicator != null && !this.missileIndicator.IsDestroyed)
            {
                /// ... or update its position if already exists.
                this.missileIndicator.SetLocation(this.CalculateArea(this.missilePosition.Read()));
            }
            return true;
        }

        /// <summary>
        /// Updates this missile if it is in Impacted state.
        /// </summary>
        private bool UpdateImpactedState()
        {
            /// Update the target entity position and height if it is still on the map.
            if (this.targetEntity.Read().HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects))
            {
                this.lastKnownTargetEntityPos.Write(this.targetEntity.Read().MotionControl.PositionVector.Read());
                this.lastKnownTargetEntityIsFlying.Write(this.targetEntity.Read().MotionControl.IsFlying ? (byte)0x01 : (byte)0x00);
            }

            /// Create an impact indicator map object if it has not yet been created and the missile type defines an impact animation.
            if (this.impactIndicator == null && this.missileData.MissileType.ImpactAnimation != null)
            {
                RCNumVector impactIndicatorPos = this.missileData.MissileType.Speed != null
                    ? this.missilePosition.Read()
                    : this.lastKnownTargetEntityPos.Read();
                this.impactIndicator = this.CreateMapObject(this.CalculateArea(impactIndicatorPos), this.lastKnownTargetEntityIsFlying.Read() == 0x01 ? MapObjectLayerEnum.AirMissiles : MapObjectLayerEnum.GroundMissiles);
                this.impactIndicator.StartAnimation(this.missileData.MissileType.ImpactAnimation, this.missileVelocity);
            }

            /// Check if the lifecycle of this missile has already ended.
            if ((this.launchIndicator == null || this.launchIndicator.IsDestroyed) &&
                (this.missileIndicator == null || this.missileIndicator.IsDestroyed) &&
                (this.impactIndicator == null || this.impactIndicator.IsDestroyed))
            {
                return false;
            }

            return true;
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

        /// <summary>
        /// Checks whether the launching of this missile has to be cancelled.
        /// </summary>
        /// <returns>True if the launching of this missile has to be cancelled; otherwise false.</returns>
        private bool LaunchHasToBeCancelled()
        {
            /// Missile launch cancelled if...
            return !this.sourceEntity.Read().HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects) ||    /// ... the source entity has been removed from the map or...
                   !this.targetEntity.Read().HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects) ||    /// ... the target entity has been removed from the map or...
                   this.sourceEntity.Read().Armour.Target != this.targetEntity.Read();  /// ... the target of the source entity has been changed.
        }

        #endregion Private methods

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
        /// The last known position of the target entity.
        /// </summary>
        private readonly HeapedValue<RCNumVector> lastKnownTargetEntityPos;

        /// <summary>
        /// This flag indicates the last known flying state of the target entity.
        /// Meanings: 0x00 - on the ground or fixed; 0x01 - in the air, landing or taking off.
        /// </summary>
        private readonly HeapedValue<byte> lastKnownTargetEntityIsFlying;

        /// <summary>
        /// The current position of this missile or RCNumVector.Undefined if it has not yet been launched.
        /// </summary>
        private readonly HeapedValue<RCNumVector> missilePosition;

        /// <summary>
        /// The launch position of this missile or RCNumVector.Undefined if this missile has not yet been launched.
        /// </summary>
        private readonly HeapedValue<RCNumVector> launchPosition;

        /// <summary>
        /// This flag indicates whether this missile has been launched from air (0x01) or from ground (0x00).
        /// </summary>
        private readonly HeapedValue<byte> launchedFromAir; 

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
        /// Enumerates the possible states of a missile.
        /// </summary>
        private enum Status
        {
            Launching = 0,      /// The missile is preparing to launch.
            Launched = 1,       /// The missile has been launched.
            Impacted = 2,       /// The missile has been impacted.
        }

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
