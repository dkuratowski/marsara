using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Common.ComponentModel;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents an RC scenario.
    /// </summary>
    public class Scenario // TODO: derive from HeapedObject
    {
        /// <summary>
        /// Constructs a Scenario instance.
        /// </summary>
        /// <param name="map">The map of the scenario.</param>
        /// <param name="initializers">The registered star location initializers.</param>
        internal Scenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }

            this.map = map;
            this.playerInitializer = ComponentManager.GetInterface<IPlayerInitializer>();
            this.visibleEntities = new BspMapContentManager<Entity>(
                        new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                           -(RCNumber)1 / (RCNumber)2,
                                           this.map.CellSize.X,
                                           this.map.CellSize.Y),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspNodeCapacity"),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspMinNodeSize"));
            this.entitySet = new Dictionary<int, Entity>();
            this.nextID = 0;
            this.players = new Player[Player.MAX_PLAYERS];
            this.playersFinalized = false;
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
        /// Gets the visible entities of the given type from the whole map.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <returns>A list that contains the visible entities of the given type from the whole map.</returns>
        public List<T> GetVisibleEntities<T>() where T : Entity
        {
            return GetVisibleEntities<T>(RCNumRectangle.Undefined);
        }

        /// <summary>
        /// Gets the visible entities of the given type from the given area of the map.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <param name="area">
        /// The area to search. Call this function with RCNumRectangle.Undefined to search on the whole map.
        /// </param>
        /// <returns>A list that contains the visible entities of the given type from the given area of the map.</returns>
        public List<T> GetVisibleEntities<T>(RCNumRectangle area) where T : Entity
        {
            List<T> retList = new List<T>();
            foreach (Entity entity in area != RCNumRectangle.Undefined ? this.visibleEntities.GetContents(area) : this.visibleEntities.GetContents())
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the entity of the given type with the given ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <returns>
        /// The entity with the given ID or null if no entity of the given type with the given ID is added to this scenario.
        /// </returns>
        public T GetEntity<T>(int id) where T : Entity
        {
            return this.entitySet.ContainsKey(id) ? this.entitySet[id] as T : null;
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
        /// Creates a new player to this scenario.
        /// </summary>
        /// <param name="index">The index of the new player.</param>
        /// <param name="startLocation">The start location of the new player.</param>
        /// <param name="race">The race of the new player.</param>
        public void CreatePlayer(int index, StartLocation startLocation, RaceEnum race)
        {
            if (this.playersFinalized) { throw new InvalidOperationException("Players already finalized!"); }
            if (startLocation == null) { throw new ArgumentNullException("startLocation"); }
            if (startLocation.Scenario != this) { throw new SimulatorException("The given start location doesn't belong to the scenario!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index] != null) { throw new SimulatorException(string.Format("Player with index {0} already exists!", index)); }

            Player newPlayer = new Player(index, startLocation);
            this.visibleEntities.DetachContent(startLocation);
            this.players[index] = newPlayer;
            this.playerInitializer.Initialize(newPlayer, race);
        }

        /// <summary>
        /// Deletes the given player and all of it's entities from this scenario.
        /// </summary>
        /// <param name="index">The index of the player to delete.</param>
        public void DeletePlayer(int index)
        {
            if (this.playersFinalized) { throw new InvalidOperationException("Players already finalized!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index] == null) { throw new SimulatorException(string.Format("Player with index {0} doesn't exist!", index)); }

            this.visibleEntities.AttachContent(this.players[index].StartLocation);
            this.players[index].Dispose();
            this.players[index] = null;
        }

        /// <summary>
        /// Finalize the current players of this scenario. Creating or deleting players will be
        /// unavailable after calling this method.
        /// </summary>
        public void FinalizePlayers()
        {
            if (this.playersFinalized) { throw new InvalidOperationException("Players already finalized!"); }

            foreach (StartLocation startLocation in this.GetVisibleEntities<StartLocation>())
            {
                this.visibleEntities.DetachContent(startLocation);
            }
            this.playersFinalized = true;
        }

        /// <summary>
        /// Gets the player of this scenario with the given index.
        /// </summary>
        /// <param name="index">The index of the player to get.</param>
        /// <returns>
        /// The player of this scenario with the given index or null if the scenario has no player with such index.
        /// </returns>
        public Player GetPlayer(int index)
        {
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            return this.players[index];
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
            foreach (Entity entity in this.entitySet.Values)
            {
                foreach (AnimationPlayer animation in entity.CurrentAnimations) { animation.Step(); }
            }
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
        /// List of the players mapped by their IDs.
        /// </summary>
        private Player[] players;

        /// <summary>
        /// This flag indicates whether the players are finalized on this scenario or not.
        /// </summary>
        private bool playersFinalized;

        /// <summary>
        /// The entities of the scenario that are visible on the map.
        /// </summary>
        private IMapContentManager<Entity> visibleEntities;

        /// <summary>
        /// The entities of the scenario mapped by their IDs.
        /// </summary>
        private Dictionary<int, Entity> entitySet;

        /// <summary>
        /// Reference to the player initializer component.
        /// </summary>
        private IPlayerInitializer playerInitializer;
    }
}
