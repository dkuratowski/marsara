using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// The abstract base class of the elements of a scenario.
    /// </summary>
    public abstract class ScenarioElement : HeapedObject
    {
        #region Public interface

        /// <summary>
        /// Gets a reference to the type of this scenario element.
        /// </summary>
        public IScenarioElementType ElementType { get { return this.elementType; } }

        /// <summary>
        /// Gets the ID of the map object.
        /// </summary>
        public IValueRead<int> ID { get { return this.id; } }

        /// <summary>
        /// Gets the owner of this entity or null if this entity is neutral or is a start location.
        /// </summary>
        public Player Owner { get { return this.owner.Read(); } }

        /// <summary>
        /// Gets a reference to the scenario that this scenario element belongs to or null if this scenario element doesn't belong to any scenario.
        /// </summary>
        public Scenario Scenario { get { return this.scenario.Read(); } }

        /// <summary>
        /// Gets whether this scenario element has at least one map object.
        /// </summary>
        public bool HasMapObject { get { return this.mapObjectsOfThisElement.Count > 0; } }

        /// <summary>
        /// Attaches this scenario element to the map into the given position.
        /// </summary>
        /// <param name="position">The position where to attach this scenario element.</param>
        /// <returns>True if this scenario element has successfully been attached to the map; otherwise false.</returns>
        /// <remarks>Can be overriden in the derived classes, the default implementation does nothing.</remarks>
        public virtual bool AttachToMap(RCNumVector position)
        {
            throw new NotSupportedException("ScenarioElement.AttachToMap is not supported for this scenario element!");
        }

        /// <summary>
        /// Detaches this scenario element from the map.
        /// </summary>
        /// <returns>The current position of this scenario element on the map.</returns>
        /// <remarks>Can be overriden in the derived classes, the default implementation does nothing.</remarks>
        public virtual RCNumVector DetachFromMap()
        {
            throw new NotSupportedException("ScenarioElement.DetachFromMap is not supported for this scenario element!");
        }

        /// <summary>
        /// Updates the state of this scenario element.
        /// </summary>
        /// <returns>
        /// The list of new scenario elements that shall be added to the scenario after the current simulation frame update. If the returned
        /// list is empty, then no new elements will be added to the scenario. Returning a null reference indicates that this scenario element
        /// has finished its lifecycle and can be removed from the scenario after the current simulation frame update.
        /// </returns>
        public abstract HashSet<ScenarioElement> UpdateState();

        /// <summary>
        /// Updates the animations of this scenario element if it has any map objects.
        /// </summary>
        public void UpdateAnimations()
        {
            foreach (MapObject mapObject in this.mapObjectsOfThisElement)
            {
                foreach (AnimationPlayer animation in mapObject.CurrentAnimations) { animation.Step(); }
            }
        }

        #endregion Public interface
        
        /// <summary>
        /// Constructs a ScenarioElement instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the type of this element.</param>
        protected ScenarioElement(string elementTypeName)
        {
            if (elementTypeName == null) { throw new ArgumentNullException("elementTypeName"); }

            this.elementType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetElementType(elementTypeName);

            this.typeID = this.ConstructField<int>("typeID");
            this.id = this.ConstructField<int>("id");
            this.owner = this.ConstructField<Player>("owner");
            this.scenario = this.ConstructField<Scenario>("scenario");
            this.mapObjectsOfThisElement = new HashSet<MapObject>();

            this.typeID.Write(this.elementType.ID);
            this.id.Write(-1);
            this.owner.Write(null);
            this.scenario.Write(null);
            this.mapContext = null;
        }

        #region Protected members for the derived classes

        /// <summary>
        /// Creates a map object for this scenario element to the given location on the map.
        /// </summary>
        /// <param name="location">The location of the created map object on the map.</param>
        /// <returns>The created map object.</returns>
        protected MapObject CreateMapObject(RCNumRectangle location)
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("This scenario element doesn't not belong to a scenario!"); }
            if (location == RCNumRectangle.Undefined) { throw new ArgumentNullException("location"); }

            MapObject mapObject = new MapObject(this);
            mapObject.SetLocation(location);
            this.mapContext.MapObjects.AttachContent(mapObject);
            this.mapObjectsOfThisElement.Add(mapObject);
            return mapObject;
        }

        /// <summary>
        /// Destroys the given map object of this scenario element.
        /// </summary>
        /// <param name="mapObject">The map object to destroy.</param>
        protected void DestroyMapObject(MapObject mapObject)
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("This scenario element doesn't not belong to a scenario!"); }
            if (mapObject == null) { throw new ArgumentNullException("mapObject"); }
            if (!this.mapObjectsOfThisElement.Contains(mapObject)) { throw new InvalidOperationException("The given map object doesn't belong to this scenario element!"); }

            this.mapObjectsOfThisElement.Remove(mapObject);
            this.mapContext.MapObjects.DetachContent(mapObject);
            mapObject.Dispose();
        }

        /// <summary>
        /// Gets the map context of the scenario that this element is added or null if this element is not added to a scenario.
        /// </summary>
        protected ScenarioMapContext MapContext { get { return this.mapContext; } }

        #endregion Protected members for the derived classes

        #region Internal members

        /// <summary>
        /// This method is called when this scenario element has been added to a scenario.
        /// </summary>
        /// <param name="scenario">The scenario where this scenario element is added to.</param>
        /// <param name="id">The ID of this scenario element.</param>
        /// <param name="mapContext">Allows access to map functionalities.</param>
        internal void OnAddedToScenario(Scenario scenario, int id, ScenarioMapContext mapContext)
        {
            if (this.scenario.Read() != null) { throw new InvalidOperationException("This scenario element already belongs to a scenario!"); }

            this.scenario.Write(scenario);
            this.id.Write(id);
            this.mapContext = mapContext;
        }

        /// <summary>
        /// This method is called when this scenario element has been removed from the scenario it is currently belongs to.
        /// </summary>
        internal void OnRemovedFromScenario()
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("This scenario element doesn't not belong to a scenario!"); }
            if (this.HasMapObject) { throw new InvalidOperationException("This scenario element has still some map objects attached to the map!"); }

            /// Reset the scenario context.
            this.scenario.Write(null);
            this.id.Write(-1);
            this.mapContext = null;
        }

        /// <summary>
        /// The method is called when this scenario element has been added to a player.
        /// </summary>
        /// <param name="owner">The player that owns this scenario element.</param>
        internal void OnAddedToPlayer(Player owner)
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The scenario element doesn't not belong to a scenario!"); }
            if (this.owner.Read() != null) { throw new InvalidOperationException("The scenario element is already added to a player!"); }
            this.owner.Write(owner);
        }

        /// <summary>
        /// The method is called when this scenario element has been removed from the player it is currently owned by.
        /// </summary>
        internal void OnRemovedFromPlayer()
        {
            if (this.scenario.Read() == null) { throw new InvalidOperationException("The scenario element doesn't not belong to a scenario!"); }
            if (this.owner.Read() == null) { throw new InvalidOperationException("The scenario element doesn't not belong to a player!"); }
            this.owner.Write(null);
        }

        #endregion Internal members

        #region Heaped members

        /// <summary>
        /// The ID of this scenario element.
        /// </summary>
        private readonly HeapedValue<int> id;

        /// <summary>
        /// The ID of the type of this scenario element.
        /// </summary>
        private readonly HeapedValue<int> typeID;

        /// <summary>
        /// Reference to the player who owns this scenario element or null if this scenario element is neutral or is a start location.
        /// </summary>
        private readonly HeapedValue<Player> owner;

        /// <summary>
        /// Reference to the scenario that this scenario element belongs to or null if this scenario element doesn't belong to any scenario.
        /// </summary>
        private readonly HeapedValue<Scenario> scenario;

        #endregion Heaped members

        /// <summary>
        /// Allows access to map functionalities.
        /// </summary>
        /// TODO: make this reference heaped!
        private ScenarioMapContext mapContext;

        /// <summary>
        /// The list of map objects of this scenario element.
        /// </summary>
        private readonly HashSet<MapObject> mapObjectsOfThisElement;

        /// <summary>
        /// Reference to the type of this scenario element.
        /// </summary>
        private readonly IScenarioElementType elementType;
    }
}
