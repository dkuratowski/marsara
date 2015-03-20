using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.MotionControl;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Scenario elements that have activities on the map.
    /// </summary>
    public abstract class Entity : HeapedObject, ISearchTreeContent, IMotionControlTarget
    {
        /// <summary>
        /// Constructs an entity instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of this entity.</param>
        public Entity(string elementTypeName)
        {
            if (elementTypeName == null) { throw new ArgumentNullException("elementTypeName"); }

            this.position = this.ConstructField<RCNumVector>("position");
            this.velocity = this.ConstructField<RCNumVector>("velocity");
            this.isFlying = this.ConstructField<byte>("isFlying");
            this.id = this.ConstructField<int>("id");
            this.typeID = this.ConstructField<int>("typeID");
            this.pathTracker = this.ConstructField<PathTrackerBase>("pathTracker");
            this.owner = this.ConstructField<Player>("owner");
            this.scenario = this.ConstructField<Scenario>("scenario");
            this.affectingCmdExecution = this.ConstructField<CmdExecutionBase>("affectingCmdExecution");
            this.locator = this.ConstructField<Locator>("locator");

            this.elementType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetElementType(elementTypeName);
            this.scenario.Write(null);
            this.currentAnimations = new List<AnimationPlayer>();
            this.entityActuator = null;
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
            this.quadraticPositionCache = new CachedValue<RCIntRectangle>(() =>
                {
                    if (this.position.Read() == RCNumVector.Undefined) { return RCIntRectangle.Undefined; }
                    RCIntVector topLeft = this.Position.Location.Round();
                    RCIntVector bottomRight = (this.Position.Location + this.Position.Size).Round();
                    RCIntRectangle cellRect = new RCIntRectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                    return this.Scenario.Map.CellToQuadRect(cellRect);
                });

            this.position.Write(RCNumVector.Undefined);
            this.position.ValueChanged += (sender, args) => this.quadraticPositionCache.Invalidate();
            this.velocity.Write(new RCNumVector(0, 0));
            this.isFlying.Write(0x00);
            this.id.Write(-1);
            this.typeID.Write(this.elementType.ID);
            this.pathTracker.Write(null);
            this.owner.Write(null);
            this.affectingCmdExecution.Write(null);
            this.locator.Write(new Locator(this));
        }

        #region Public interface

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
        public bool IsMoving { get { return this.pathTracker.Read() != null && this.pathTracker.Read().TargetPosition != RCNumVector.Undefined; } }

        /// <summary>
        /// Orders this entity to stop at its current position.
        /// </summary>
        public void StopMoving()
        {
            if (this.pathTracker.Read() != null) { this.pathTracker.Read().TargetPosition = RCNumVector.Undefined; }
        }

        /// <summary>
        /// Gets the ID of the entity.
        /// </summary>
        public IValueRead<int> ID { get { return this.id; } }

        /// <summary>
        /// Gets the position value of this entity.
        /// </summary>
        public IValueRead<RCNumVector> PositionValue { get { return this.position; } }

        /// <summary>
        /// Gets whether this entity is currently flying or not.
        /// </summary>
        public bool IsFlying { get { return this.isFlying.Read() != 0x00; } }

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
        /// Gets the metadata type definition of the entity.
        /// </summary>
        public IScenarioElementType ElementType { get { return this.elementType; } }

        /// <summary>
        /// Gets the players of the currently active animations of this entity.
        /// </summary>
        public IEnumerable<AnimationPlayer> CurrentAnimations { get { return this.currentAnimations; } }

        /// <summary>
        /// Gets the quadratic position of this Entity or RCIntRectangle.Undefined if this entity is not attached to the map.
        /// </summary>
        public RCIntRectangle QuadraticPosition { get { return this.quadraticPositionCache.Value; } }

        /// <summary>
        /// Gets the owner of this entity or null if this entity is neutral or is a start location.
        /// </summary>
        public Player Owner { get { return this.owner.Read(); } }

        /// <summary>
        /// Gets the scenario that this entity belongs to.
        /// </summary>
        public Scenario Scenario { get { return this.scenario.Read(); } }

        /// <summary>
        /// Gets the locator of this entity.
        /// </summary>
        public Locator Locator { get { return this.locator.Read(); } }

        #endregion Public interface

        #region Protected methods for the derived classes

        /// <summary>
        /// Sets the position of this entity.
        /// </summary>
        /// <param name="newPos">The new position of this entity.</param>
        protected void SetPosition(RCNumVector newPos)
        {
            if (newPos == RCNumVector.Undefined) { throw new ArgumentNullException("newPos"); }

            if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
            this.position.Write(newPos);
            if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
        }

        /// <summary>
        /// Sets the current animation of this entity with undefined direction.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        protected void SetCurrentAnimation(string animationName)
        {
            this.SetCurrentAnimation(animationName, MapDirection.Undefined);
        }

        /// <summary>
        /// Sets the current animation of this entity with the given direction.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="direction">The direction of the animation.</param>
        protected void SetCurrentAnimation(string animationName, MapDirection direction)
        {
            if (animationName == null) { throw new ArgumentNullException("animationName"); }
            this.SetCurrentAnimations(new List<string>() { animationName }, direction);
        }

        /// <summary>
        /// Sets the current animations of this entity with undefined direction.
        /// </summary>
        /// <param name="animationNames">The names of the animations to play.</param>
        protected void SetCurrentAnimations(List<string> animationNames)
        {
            this.SetCurrentAnimations(animationNames, MapDirection.Undefined);
        }

        /// <summary>
        /// Sets the current animations of this entity with the given direction.
        /// </summary>
        /// <param name="animationNames">The names of the animations to play.</param>
        /// <param name="direction">The direction of the animations.</param>
        protected void SetCurrentAnimations(List<string> animationNames, MapDirection direction)
        {
            if (animationNames == null) { throw new ArgumentNullException("animationNames"); }

            this.currentAnimations.Clear();
            foreach (string name in animationNames)
            {
                this.currentAnimations.Add(new AnimationPlayer(this.elementType.AnimationPalette.GetAnimation(name), direction));
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

        #region ISearchTreeContent members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox
        {
            get { return new RCNumRectangle(this.position.Read() - this.ElementType.Area.Read() / 2, this.elementType.Area.Read()); }
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeContent members

        #region IMotionControlTarget members

        /// <see cref="IMotionControlTarget.Position"/>
        public RCNumRectangle Position { get { return this.BoundingBox; } }

        /// <see cref="IMotionControlTarget.Velocity"/>
        public RCNumVector Velocity { get { return this.velocity.Read(); } }

        #endregion IMotionControlTarget members

        #region Internal members

        /// <summary>
        /// This method is called when this entity has been added to a scenario.
        /// </summary>
        /// <param name="scenario">The scenario where this entity is added to.</param>
        /// <param name="id">The ID of this entity.</param>
        internal void OnAddedToScenario(Scenario scenario, int id)
        {
            if (this.scenario.Read() != null) { throw new InvalidOperationException("The entity is already added to a scenario!"); }
            this.scenario.Write(scenario);
            this.id.Write(id);
        }

        /// <summary>
        /// This method is called when this entity has been removed from the scenario it is currently belongs to.
        /// </summary>
        internal void OnRemovedFromScenario()
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.position.Read() != RCNumVector.Undefined) { throw new InvalidOperationException("The entity is not removed from the map!"); }

            this.scenario.Write(null);
            this.id.Write(-1);
        }

        /// <summary>
        /// The method is called when this entity has been added to a player.
        /// </summary>
        /// <param name="owner">The player that owns this entity.</param>
        internal void OnAddedToPlayer(Player owner)
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.owner.Read() != null) { throw new InvalidOperationException("The entity is already added to a player!"); }
            this.owner.Write(owner);
        }

        /// <summary>
        /// The method is called when this entity has been removed from the player it is currently owned by.
        /// </summary>
        internal void OnRemovedFromPlayer()
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.owner.Read() == null) { throw new InvalidOperationException("The entity doesn't not belong to a player!"); }
            this.owner.Write(null);
        }

        /// <summary>
        /// This method is called when a command execution starts to affecting this entity.
        /// </summary>
        /// <param name="cmdExecution">The command execution.</param>
        internal void OnCommandExecutionStarted(CmdExecutionBase cmdExecution)
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
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
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.affectingCmdExecution.Read() == null) { throw new InvalidOperationException("The entity is not being affected by any command executions!"); }
            this.affectingCmdExecution.Write(null);
        }

        /// <summary>
        /// This method is called when this entity is being added to the map.
        /// </summary>
        /// <param name="position">The position of this entity on the map.</param>
        /// <returns>True if the given position is valid for this entity; otherwise false.</returns>
        internal bool OnAttachingToMap(RCNumVector position)
        {
            bool isValidPosition = this.ValidatePosition(position);
            if (isValidPosition)
            {
                this.SetPosition(position);
            }
            return isValidPosition;
        }

        /// <summary>
        /// This method is called when this entity has been removed from the map.
        /// </summary>
        internal void OnDetachedFromMap()
        {
            this.position.Write(RCNumVector.Undefined);
        }

        /// <summary>
        /// Updates the state of this entity.
        /// </summary>
        internal void UpdateState()
        {
            /// Update the velocity and position of this entity only if it's visible on the map, has
            /// an actuator and a path-tracker and the path-tracker is active.
            if (this.scenario.Read().GetEntityOnMap<Entity>(this.ID.Read()) != null &&
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
                    RCNumVector newPosition = this.position.Read() + this.velocity.Read();
                    if (this.ValidatePosition(newPosition)) { this.SetPosition(newPosition); }
                    else { this.velocity.Write(new RCNumVector(0, 0)); }
                }
            }
        }

        /// <summary>
        /// Checks whether the given position is valid for this entity.
        /// </summary>
        /// <param name="position">The position to be checked.</param>
        /// <returns>True if the given position is valid for this entity; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        /// TODO: Make this method abstract and implement it in the derived classes! This is only a temporary solution.
        protected virtual bool ValidatePosition(RCNumVector position)
        {
            if (this.pathFinder.GetNavMeshNode(position) == null)
            {
                /// The entity wants to go to a non-walkable position on the map -> invalid position
                return false;
            }

            /// Detect collision with other entities.
            RCNumRectangle newEntityArea =
                new RCNumRectangle(position - this.ElementType.Area.Read() / 2, this.elementType.Area.Read());
            bool collision = false;
            foreach (Entity collidingEntity in this.scenario.Read().GetEntitiesOnMap<Entity>(newEntityArea))
            {
                if (collidingEntity != this) { collision = true; break; }
            }

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
        }

        #endregion Internal members

        #region Heaped members

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
        /// The ID of this entity.
        /// </summary>
        private readonly HeapedValue<int> id;

        /// <summary>
        /// The ID of the element type of this entity.
        /// </summary>
        private readonly HeapedValue<int> typeID;

        /// <summary>
        /// Reference to the path-tracker of this entity.
        /// </summary>
        private readonly HeapedValue<PathTrackerBase> pathTracker;

        /// <summary>
        /// Reference to the player who owns this entity or null if this entity is neutral or is a start location.
        /// </summary>
        private readonly HeapedValue<Player> owner;

        /// <summary>
        /// Reference to the scenario that this entity belongs to or null if this entity doesn't belong to any scenario.
        /// to a scenario.
        /// </summary>
        private readonly HeapedValue<Scenario> scenario;

        /// <summary>
        /// Reference to the command execution that is affecting this entity or null if this entity is not affected by
        /// any command execution.
        /// </summary>
        private readonly HeapedValue<CmdExecutionBase> affectingCmdExecution;

        /// <summary>
        /// Reference to the locator of this entity.
        /// </summary>
        private readonly HeapedValue<Locator> locator;

        #endregion Heaped members

        /// <summary>
        /// The player of the currently active animations of this entity.
        /// </summary>
        private List<AnimationPlayer> currentAnimations;

        /// <summary>
        /// Reference to the element type of this entity.
        /// </summary>
        private IScenarioElementType elementType;

        /// <summary>
        /// Reference to the actuator of this entity.
        /// </summary>
        private EntityActuatorBase entityActuator;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;

        /// <summary>
        /// The cached value of the quadratic position of this Entity.
        /// </summary>
        private CachedValue<RCIntRectangle> quadraticPositionCache;
    }
}
