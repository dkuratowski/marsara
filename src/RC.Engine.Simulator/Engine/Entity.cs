using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using RC.Engine.Simulator.MotionControl;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Scenario elements that have activities on the map.
    /// </summary>
    public abstract class Entity : ScenarioElement, IMotionControlTarget
    {
        /// <summary>
        /// Constructs an entity instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of this entity.</param>
        protected Entity(string elementTypeName) : base(elementTypeName)
        {
            this.mapObject = this.ConstructField<MapObject>("mapObject");
            this.position = this.ConstructField<RCNumVector>("position");
            this.velocity = this.ConstructField<RCNumVector>("velocity");
            this.isFlying = this.ConstructField<byte>("isFlying");
            this.pathTracker = this.ConstructField<PathTrackerBase>("pathTracker");
            this.affectingCmdExecution = this.ConstructField<CmdExecutionBase>("affectingCmdExecution");
            this.locator = this.ConstructField<Locator>("locator");
            this.armour = this.ConstructField<Armour>("armour");

            this.entityActuator = null;
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();

            this.mapObject.Write(null);
            this.position.Write(RCNumVector.Undefined);
            this.velocity.Write(new RCNumVector(0, 0));
            this.isFlying.Write(0x00);
            this.pathTracker.Write(null);
            this.affectingCmdExecution.Write(null);
            this.locator.Write(new Locator(this));
            this.armour.Write(new Armour(this));
        }

        #region Public interface

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            bool isValidPosition = this.ValidatePosition(position);
            if (isValidPosition)
            {
                this.SetPosition(position);
                this.mapObject.Write(this.CreateMapObject(this.Position));
            }
            return isValidPosition;
        }

        /// <see cref="ScenarioElement.DetachFromMap"/>
        public override RCNumVector DetachFromMap()
        {
            RCNumVector currentPosition = this.position.Read();
            this.position.Write(RCNumVector.Undefined);
            this.DestroyMapObject(this.mapObject.Read());
            this.mapObject.Write(null);
            return currentPosition;
        }

        /// <summary>
        /// Orders this entity to start moving to the given position.
        /// </summary>
        /// <param name="toCoords">The target position.</param>
        public void StartMoving(RCNumVector toCoords)
        {
            if (toCoords == RCNumVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (this.pathTracker.Read() != null) { this.pathTracker.Read().TargetPosition = toCoords; }
        }

        /// <summary>
        /// Gets whether this entity is currently performing a move operation or not.
        /// </summary>
        public bool IsMoving
        {
            get
            {
                return this.pathTracker.Read() != null && this.pathTracker.Read().TargetPosition != RCNumVector.Undefined;
            }
        }

        /// <summary>
        /// Orders this entity to stop at its current position.
        /// </summary>
        public void StopMoving()
        {
            if (this.pathTracker.Read() != null) { this.pathTracker.Read().TargetPosition = RCNumVector.Undefined; }
        }

        /// <summary>
        /// Gets whether this entity is currently flying or not.
        /// </summary>
        public bool IsFlying
        {
            get
            {
                return this.isFlying.Read() != 0x00;
            }
        }

        /// <summary>
        /// Gets the type of the command that is currently being executed by this entity or null if there is no command currently
        /// being executed by this entity.
        /// </summary>
        public string CommandBeingExecuted
        {
            get
            {
                return this.affectingCmdExecution.Read() != null
                    ? this.affectingCmdExecution.Read().CommandBeingExecuted
                    : null;
            }
        }

        /// <summary>
        /// Gets the locator of this entity.
        /// </summary>
        public Locator Locator { get { return this.locator.Read(); } }

        /// <summary>
        /// Gets the armour of this entity.
        /// </summary>
        public Armour Armour { get { return this.armour.Read(); } }

        /// <summary>
        /// Gets the position value of this entity.
        /// </summary>
        public IValueRead<RCNumVector> PositionValue { get { return this.position; } }

        /// <summary>
        /// Gets the map object that represents this entity on the map or null if this entity is not attached to the map.
        /// </summary>
        public MapObject MapObject { get { return this.mapObject.Read(); } }

        #endregion Public interface

        #region Protected methods for the derived classes

        /// <summary>
        /// Sets the position of this entity.
        /// </summary>
        /// <param name="newPos">The new position of this entity.</param>
        protected void SetPosition(RCNumVector newPos)
        {
            if (newPos == RCNumVector.Undefined) { throw new ArgumentNullException("newPos"); }

            this.position.Write(newPos);
            if (this.mapObject.Read() != null)
            {
                this.mapObject.Read().SetLocation(this.Position);
            }
        }

        /// <summary>
        /// Sets a new actuator for this entity.
        /// </summary>
        /// <param name="actuator">The actuator to be set or null to dispose the current actuator.</param>
        /// <exception cref="InvalidOperationException">If this entity has already an actuator and the parameter is not null.</exception>
        protected void SetActuator(EntityActuatorBase actuator)
        {
            if (this.entityActuator != null && actuator != null) { throw new InvalidOperationException("The entity already has an actuator!"); }
            
            if (this.entityActuator != null) { this.entityActuator.Dispose(); }
            this.entityActuator = actuator;
        }

        /// <summary>
        /// Sets a new path-tracker for this entity.
        /// </summary>
        /// <param name="pathTracker">The path-tracker to be set or null to dispose the current path-tracker.</param>
        /// <exception cref="InvalidOperationException">If this entity has already a path-tracker and the parameter is not null.</exception>
        protected void SetPathTracker(PathTrackerBase pathTracker)
        {
            if (this.pathTracker.Read() != null && pathTracker != null) { throw new InvalidOperationException("The entity already has a path-tracker!"); }

            if (this.pathTracker.Read() != null) { this.pathTracker.Read().Dispose(); }
            this.pathTracker.Write(pathTracker);
        }

        /// <summary>
        /// Gets the velocity value of this entity.
        /// </summary>
        protected IValue<RCNumVector> VelocityValue { get { return this.velocity; } }

        #endregion Protected methods for the derived classes

        #region Overrides

        /// <see cref="ScenarioElement.UpdateState"/>
        public override HashSet<ScenarioElement> UpdateState()
        {
            /// Update the velocity and position of this entity only if it's visible on the map, has
            /// an actuator and a path-tracker and the path-tracker is active.
            if (this.HasMapObject &&
                this.entityActuator != null &&
                this.pathTracker.Read() != null &&
                this.pathTracker.Read().IsActive)
            {
                /// Update the state of the path-tracker of this entity.
                this.pathTracker.Read().Update();

                if (this.pathTracker.Read().IsActive)
                {
                    /// Update the velocity of this entity if the path-tracker is still active.
                    MotionController.UpdateVelocity(this, this.entityActuator, this.pathTracker.Read());
                }
                else
                {
                    /// Stop the entity if the path-tracker became inactive.
                    this.velocity.Write(new RCNumVector(0, 0));
                }

                /// If the velocity is not (0,0) update the position of this entity.
                if (this.velocity.Read() != new RCNumVector(0, 0))
                {
                    RCNumVector newPosition = this.PositionValue.Read() + this.velocity.Read();
                    if (this.ValidatePosition(newPosition))
                    {
                        this.SetPosition(newPosition);
                    }
                    else
                    {
                        this.velocity.Write(new RCNumVector(0, 0));
                    }
                }
            }

            return new HashSet<ScenarioElement>();
        }

        /// <summary>
        /// Checks whether the given position is valid for this map object.
        /// </summary>
        /// <param name="position">The position to be checked.</param>
        /// <returns>True if the given position is valid for this map object; otherwise false.</returns>
        /// <remarks>Can be overriden in the derived classes.</remarks>
        /// TODO: Override this method in the derived classes! This is only a temporary solution.
        protected virtual bool ValidatePosition(RCNumVector position)
        {
            if (this.pathFinder.GetNavMeshNode(position) == null)
            {
                /// The entity wants to go to a non-walkable position on the map -> invalid position
                return false;
            }

            /// Detect collision with other entities.
            RCNumRectangle newEntityArea =
                new RCNumRectangle(position - this.ElementType.Area.Read() / 2, this.ElementType.Area.Read());
            bool collision = this.Scenario.GetElementsOnMap<Entity>(newEntityArea).Any(collidingEntity => collidingEntity != this);

            /// Return true if there is no collision with other entities.
            return !collision;
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.affectingCmdExecution.Read() != null)
            {
                /// Unregister this entity from the command execution it is currently being affected.
                this.affectingCmdExecution.Read().RemoveEntity(this);
                this.affectingCmdExecution.Write(null);
            }
            if (this.pathTracker.Read() != null) { this.pathTracker.Read().Dispose(); this.pathTracker.Write(null); }
            if (this.entityActuator != null) { this.entityActuator.Dispose(); this.entityActuator = null; }

            this.locator.Read().Dispose();
            this.locator.Write(null);
            this.armour.Read().Dispose();
            this.armour.Write(null);
        }

        #endregion Overrides

        #region IMotionControlTarget members

        /// <see cref="IMotionControlTarget.Position"/>
        public RCNumRectangle Position { get { return new RCNumRectangle(this.position.Read() - this.ElementType.Area.Read() / 2, this.ElementType.Area.Read()); } }

        /// <see cref="IMotionControlTarget.Velocity"/>
        public RCNumVector Velocity { get { return this.velocity.Read(); } }

        #endregion IMotionControlTarget members

        #region Internal members

        /// <summary>
        /// This method is called when a command execution starts to affecting this entity.
        /// </summary>
        /// <param name="cmdExecution">The command execution.</param>
        internal void OnCommandExecutionStarted(CmdExecutionBase cmdExecution)
        {
            if (this.Scenario == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.affectingCmdExecution.Read() != null)
            {
                /// Unregister this entity from the command execution it is currently being affected.
                this.affectingCmdExecution.Read().RemoveEntity(this);
            }
            this.affectingCmdExecution.Write(cmdExecution);
        }

        /// <summary>
        /// This method is called when the currently affecting command execution stops affecting this entity.
        /// </summary>
        internal void OnCommandExecutionStopped()
        {
            if (this.Scenario == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.affectingCmdExecution.Read() == null) { throw new InvalidOperationException("The entity is not being affected by any command executions!"); }
            this.affectingCmdExecution.Write(null);
        }

        #endregion Internal members

        #region Heaped members

        /// <summary>
        /// Reference to the map object that represents this entity on the map or null if this entity is not currently
        /// attached to the map.
        /// </summary>
        private readonly HeapedValue<MapObject> mapObject;

        /// <summary>
        /// The position of this entity.
        /// </summary>
        private readonly HeapedValue<RCNumVector> position;

        /// <summary>
        /// The velocity of this entity.
        /// </summary>
        private readonly HeapedValue<RCNumVector> velocity;

        /// <summary>
        /// This flag indicates whether the entity is on the ground (0x00) or is flying (any other value).
        /// </summary>
        private readonly HeapedValue<byte> isFlying; 

        /// <summary>
        /// Reference to the path-tracker of this entity.
        /// </summary>
        private readonly HeapedValue<PathTrackerBase> pathTracker;

        /// <summary>
        /// Reference to the command execution that is affecting this entity or null if this entity is not affected by
        /// any command execution.
        /// </summary>
        private readonly HeapedValue<CmdExecutionBase> affectingCmdExecution;

        /// <summary>
        /// Reference to the locator of this entity.
        /// </summary>
        private readonly HeapedValue<Locator> locator;

        /// <summary>
        /// Reference to the armour of this entity.
        /// </summary>
        private readonly HeapedValue<Armour> armour;

        #endregion Heaped members

        /// <summary>
        /// Reference to the actuator of this entity.
        /// </summary>
        private EntityActuatorBase entityActuator;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;
    }
}
