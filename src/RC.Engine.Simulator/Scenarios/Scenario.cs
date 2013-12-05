using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents an RC scenario.
    /// </summary>
    public class Scenario
    {
        /// <summary>
        /// Constructs a Scenario instance.
        /// </summary>
        /// <param name="map">The map of the scenario.</param>
        public Scenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }

            this.map = map;
            this.entities = new BspMapContentManager<Entity>(
                        new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                           -(RCNumber)1 / (RCNumber)2,
                                           this.map.CellSize.X,
                                           this.map.CellSize.Y),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspNodeCapacity"),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspMinNodeSize"));
            this.entitySet = new Dictionary<int, Entity>();
            this.nextID = 0;
        }

        #region Public members

        /// <summary>
        /// Gets the entity with the given ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>
        /// The entity with the given ID or null if no entity with the given ID is attached to this scenario.
        /// </returns>
        public T GetEntity<T>(int id) where T : Entity
        {
            return this.entitySet.ContainsKey(id) ? (T)this.entitySet[id] : null;
        }

        /// <summary>
        /// Attaches the given entity to this scenario.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>
        public void AttachEntity(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            int id = this.nextID++;
            this.entities.AttachContent(entity);
            this.entitySet.Add(id, entity);
            entity.OnAttached(this, id);
        }

        /// <summary>
        /// Detaches the given entity from this scenario.
        /// </summary>
        /// <param name="entity">The entity to be detached.</param>
        public void DetachEntity(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            this.entities.DetachContent(entity);
            this.entitySet.Remove(entity.ID.Read());
            entity.OnDetached();
        }

        /// <summary>
        /// Gets the map of the scenario.
        /// </summary>
        public IMapAccess Map { get { return this.map; } }

        /// <summary>
        /// Gets the entities of the scenario.
        /// </summary>
        public IMapContentManager<Entity> Entities { get { return this.entities; } }

        #endregion Public members

        /// <summary>
        /// The ID of the next entity.
        /// </summary>
        private int nextID;

        /// <summary>
        /// Reference to the map of the scenario.
        /// </summary>
        private IMapAccess map;

        /// <summary>
        /// The entities of the scenario.
        /// </summary>
        private IMapContentManager<Entity> entities;

        /// <summary>
        /// The entities of the scenario mapped by their IDs.
        /// </summary>
        private Dictionary<int, Entity> entitySet;
    }
}
