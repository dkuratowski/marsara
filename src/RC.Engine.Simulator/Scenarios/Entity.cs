using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
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
            this.id = this.ConstructField<int>("id");
            this.typeID = this.ConstructField<int>("typeID");
            this.pathTracker = this.ConstructField<PathTrackerBase>("pathTracker");

            this.elementType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetElementType(elementTypeName);
            this.scenario = null;
            this.currentAnimations = new List<AnimationPlayer>();
            this.entityActuator = null;
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();

            this.position.Write(RCNumVector.Undefined);
            this.velocity.Write(new RCNumVector(0, 0));
            this.id.Write(-1);
            this.typeID.Write(this.elementType.ID);
            this.pathTracker.Write(null);
        }

        #region Public interface

        /// <summary>
        /// Gets the ID of the entity.
        /// </summary>
        public IValueRead<int> ID { get { return this.id; } }

        /// <summary>
        /// Gets the metadata type definition of the entity.
        /// </summary>
        public IScenarioElementType ElementType { get { return this.elementType; } }

        /// <summary>
        /// Gets the players of the currently active animations of this entity.
        /// </summary>
        public IEnumerable<AnimationPlayer> CurrentAnimations { get { return this.currentAnimations; } }

        /// <summary>
        /// Gets the owner of this entity or null if this entity is neutral or is a start location.
        /// </summary>
        public Player Owner { get { return this.owner; } }

        /// <summary>
        /// Gets the scenario that this entity belongs to.
        /// </summary>
        public Scenario Scenario { get { return this.scenario; } }

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
        /// Gets the velocity of this entity.
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
            if (this.scenario != null) { throw new InvalidOperationException("The entity is already added to a scenario!"); }
            this.scenario = scenario;
            this.id.Write(id);
            this.OnAddedToScenarioImpl();
        }

        /// <summary>
        /// This method is called when this entity has been removed from the scenario it is currently belongs to.
        /// </summary>
        internal void OnRemovedFromScenario()
        {
            if (this.scenario == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.scenario.VisibleEntities.HasContent(this)) { this.scenario.VisibleEntities.DetachContent(this); }
            this.scenario = null;
            this.id.Write(-1);
            this.OnRemovedFromScenarioImpl();
        }

        /// <summary>
        /// The method is called when this entity has been added to a player.
        /// </summary>
        /// <param name="owner">The player that owns this entity.</param>
        internal void OnAddedToPlayer(Player owner)
        {
            if (this.owner != null) { throw new InvalidOperationException("The entity is already added to a player!"); }
            this.owner = owner;
        }

        /// <summary>
        /// The method is called when this entity has been removed from the player it is currently owned by.
        /// </summary>
        internal void OnRemovedFromPlayer()
        {
            if (this.owner == null) { throw new InvalidOperationException("The entity doesn't not belong to a player!"); }
            this.owner = null;
        }

        /// <summary>
        /// This method is called on every simulation frame updates.
        /// </summary>
        /// <param name="frameIndex">The index of the current frame.</param>
        internal void OnUpdateFrame(int frameIndex)
        {
            /// Update the velocity and position of this entity only if it's visible on the map, has
            /// an actuator and a path-tracker and the path-tracker's target position is defined.
            if (this.scenario.VisibleEntities.HasContent(this) &&
                this.entityActuator != null &&
                this.pathTracker.Read() != null &&
                this.pathTracker.Read().TargetPosition != RCNumVector.Undefined)
            {
                MotionController.UpdateVelocity(this, this.entityActuator, this.pathTracker.Read());
                if (this.velocity.Read() != new RCNumVector(0, 0))
                {
                    /// Check if the new position would remain inside the walkable area of the map.
                    /// TODO: put this check into a virtual method to be able to override in case of flying entities!
                    RCNumVector newPosition = this.position.Read() + this.velocity.Read();
                    if (!this.pathFinder.IsWalkable(newPosition))
                    {
                        throw new InvalidOperationException("Entity wants to go to a non-walkable position on the map!");
                    }

                    /// Check if the entity wouldn't collide with other entities at the new position.
                    /// TODO: put this check into a virtual method to be able to override in the derived classes.
                    RCNumRectangle newEntityArea =
                        new RCNumRectangle(newPosition - this.ElementType.Area.Read() / 2, this.elementType.Area.Read());
                    bool collision = false;
                    foreach (Entity collidingEntity in this.scenario.GetVisibleEntities<Entity>(newEntityArea))
                    {
                        if (collidingEntity != this) { collision = true; break; }
                    }

                    /// In case of collision reduce the velocity to (0,0); otherwise set the new position of the entity
                    /// based on the velocity.
                    if (!collision) { this.SetPosition(newPosition); }
                    else { this.velocity.Write(new RCNumVector(0, 0)); }
                }
            }
            this.OnUpdateFrameImpl(frameIndex);
        }

        /// <summary>
        /// This method is called when this entity has been added to a scenario. Can be overriden in the derived classes.
        /// </summary>
        protected virtual void OnAddedToScenarioImpl() { }

        /// <summary>
        /// This method is called when this entity has been removed from it's scenario. Can be overriden in the derived classes.
        /// </summary>
        protected virtual void OnRemovedFromScenarioImpl() { }

        /// <summary>
        /// This method is called on every simulation frame updates. Can be overriden in the derived classes.
        /// </summary>
        /// <param name="frameIndex">The index of the current frame.</param>
        protected virtual void OnUpdateFrameImpl(int frameIndex) { }

        #endregion Internal members

        #region Heaped members

        /// <summary>
        /// The position of this entity.
        /// </summary>
        private HeapedValue<RCNumVector> position;

        /// <summary>
        /// The velocity of this entity.
        /// </summary>
        private HeapedValue<RCNumVector> velocity;

        /// <summary>
        /// The ID of this entity.
        /// </summary>
        private HeapedValue<int> id;

        /// <summary>
        /// The ID of the element type of this entity.
        /// </summary>
        private HeapedValue<int> typeID;

        /// <summary>
        /// Reference to the path-tracker of this entity.
        /// </summary>
        private HeapedValue<PathTrackerBase> pathTracker;

        #endregion Heaped members

        /// <summary>
        /// The player of the currently active animations of this entity.
        /// </summary>
        private List<AnimationPlayer> currentAnimations;

        /// <summary>
        /// Reference to the player who owns this entity or null if this entity is neutral or is a start location.
        /// </summary>
        private Player owner;

        /// <summary>
        /// Reference to the element type of this entity.
        /// </summary>
        private IScenarioElementType elementType;

        /// <summary>
        /// Reference to the scenario that this entity belongs to or null if this entity doesn't belong to any scenario.
        /// to a scenario.
        /// </summary>
        private Scenario scenario;

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
