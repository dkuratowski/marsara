using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Scenario elements that have activities on the map.
    /// </summary>
    public abstract class Entity : HeapedObject, IMapContent
    {
        /// <summary>
        /// Constructs an entity instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of this entity.</param>
        public Entity(string elementTypeName)
        {
            if (elementTypeName == null) { throw new ArgumentNullException("elementTypeName"); }

            this.position = this.ConstructField<RCNumVector>("position");
            this.id = this.ConstructField<int>("id");
            this.typeID = this.ConstructField<int>("typeID");

            this.elementType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetElementType(elementTypeName);
            this.scenario = null;
            this.currentAnimations = new List<AnimationPlayer>();

            this.position.Write(RCNumVector.Undefined);
            this.id.Write(-1);
            this.typeID.Write(this.elementType.ID);
        }

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
        protected Scenario Scenario { get { return this.scenario; } }

        /// <summary>
        /// Sets the position of this entity.
        /// </summary>
        /// <param name="newPos">The new position of this entity.</param>
        protected void SetPosition(RCNumVector newPos)
        {
            if (newPos == RCNumVector.Undefined) { throw new ArgumentNullException("newPos"); }

            if (this.PositionChanging != null) { this.PositionChanging(this); }
            this.position.Write(newPos);
            if (this.PositionChanged != null) { this.PositionChanged(this); }
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

        #region IMapContent members

        /// <see cref="IMapContent.Position"/>
        public RCNumRectangle Position
        {
            get { return new RCNumRectangle(this.position.Read(), this.elementType.Area.Read()); }
        }

        /// <see cref="IMapContent.PositionChanging"/>
        public event MapContentPropertyChangeHdl PositionChanging;

        /// <see cref="IMapContent.PositionChanged"/>
        public event MapContentPropertyChangeHdl PositionChanged;

        #endregion IMapContent members

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
        /// This method is called when this entity has been added to a scenario. Can be overriden in the derived classes.
        /// </summary>
        protected virtual void OnAddedToScenarioImpl() { }

        /// <summary>
        /// This method is called when this entity has been removed from it's scenario. Can be overriden in the derived classes.
        /// </summary>
        protected virtual void OnRemovedFromScenarioImpl() { }

        #endregion Internal members

        #region Heaped members

        /// <summary>
        /// The position of this entity.
        /// </summary>
        private HeapedValue<RCNumVector> position;

        /// <summary>
        /// The ID of this entity.
        /// </summary>
        private HeapedValue<int> id;

        /// <summary>
        /// The ID of the element type of this entity.
        /// </summary>
        private HeapedValue<int> typeID;

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
    }
}
