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
            this.visibleEntities = new BspMapContentManager<Entity>(
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
        /// Gets all of the entities of the given type added to this scenario.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <returns>A list that contains all of the entities of the given type added to this scenario.</returns>
        public List<T> GetAllEntities<T>() where T : Entity
        {
            List<T> retList = new List<T>();
            foreach (Entity entity in this.entitySet.Values)
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the visible entities of the given type from the given area of the map.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <param name="area"></param>
        /// <returns>A list that contains the visible entities of the given type from the given area of the map.</returns>
        public List<T> GetVisibleEntities<T>(RCNumRectangle area) where T : Entity
        {
            List<T> retList = new List<T>();
            foreach (Entity entity in this.visibleEntities.GetContents(area))
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the entity with the given ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>
        /// The entity with the given ID or null if no entity with the given ID is added to this scenario.
        /// </returns>
        public T GetEntity<T>(int id) where T : Entity
        {
            return this.entitySet.ContainsKey(id) ? (T)this.entitySet[id] : null;
        }

        /// <summary>
        /// Adds the given entity to this scenario.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        public void AddEntity(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            int id = this.nextID++;
            this.entitySet.Add(id, entity);
            entity.OnAddedToScenario(this, id);
        }

        /// <summary>
        /// Remove the given entity from this scenario.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void RemoveEntity(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            this.entitySet.Remove(entity.ID.Read());
            entity.OnRemovedFromScenario();
        }

        /// <summary>
        /// Gets the map of the scenario.
        /// </summary>
        public IMapAccess Map { get { return this.map; } }

        /// <summary>
        /// Gets the entities of the scenario that are visible on the map.
        /// </summary>
        public IMapContentManager<Entity> VisibleEntities { get { return this.visibleEntities; } }

        /// <summary>
        /// Steps the animations of the scenario.
        /// </summary>
        public void StepAnimations()
        {
            foreach (Entity entity in this.entitySet.Values) { if (entity.CurrentAnimation != null) { entity.CurrentAnimation.Step(); } }
        }

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
        /// The entities of the scenario that are visible on the map.
        /// </summary>
        private IMapContentManager<Entity> visibleEntities;

        /// <summary>
        /// The entities of the scenario mapped by their IDs.
        /// </summary>
        private Dictionary<int, Entity> entitySet;
    }
}
