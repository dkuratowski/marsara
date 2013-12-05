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
        /// <param name="initialPosition">The initial position of the entity instance on the map.</param>
        public Entity(string elementTypeName, RCNumVector initialPosition)
        {
            if (elementTypeName == null) { throw new ArgumentNullException("elementTypeName"); }
            if (initialPosition == RCNumVector.Undefined) { throw new ArgumentNullException("initialPosition"); }

            this.position = this.ConstructField<RCNumVector>("position");
            this.id = this.ConstructField<int>("id");
            this.typeID = this.ConstructField<int>("typeID");

            this.elementType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetElementType(elementTypeName);
            this.scenario = null;

            this.position.Write(initialPosition);
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
        /// This method is called when this entity has been attached to a scenario.
        /// </summary>
        /// <param name="scenario">The scenario where this entity is attached to.</param>
        /// <param name="id">The ID of this entity.</param>
        internal void OnAttached(Scenario scenario, int id)
        {
            if (this.scenario != null) { throw new InvalidOperationException("The entity is already attached to a scenario!"); }
            this.scenario = scenario;
            this.id.Write(id);
        }

        /// <summary>
        /// This method is called when this entity has been detached from the scenario it is currently attached to.
        /// </summary>
        internal void OnDetached()
        {
            if (this.scenario == null) { throw new InvalidOperationException("The entity is not attached to a scenario!"); }
            this.scenario = null;
            this.id.Write(-1);
        }

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
        /// Reference to the element type of this entity.
        /// </summary>
        private IScenarioElementType elementType;

        /// <summary>
        /// Reference to the scenario where this entity is attached to or null if this entity is not attached
        /// to a scenario.
        /// </summary>
        private Scenario scenario;
    }
}
