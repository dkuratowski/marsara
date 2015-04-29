using System.Security.Policy;
using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
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
    public class Scenario : HeapedObject
    {
        /// <summary>
        /// Constructs a Scenario instance.
        /// </summary>
        /// <param name="map">The map of the scenario.</param>
        /// <param name="initializers">The registered star location initializers.</param>
        internal Scenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }

            /// Create the heaped members.
            this.nextID = this.ConstructField<int>("nextID");
            this.currentFrameIndex = this.ConstructField<int>("currentFrameIndex");
            this.playersFinalized = this.ConstructField<byte>("playersFinalized");
            this.players = this.ConstructArrayField<Player>("players");

            /// Initialize the heaped members.
            this.nextID.Write(0);
            this.currentFrameIndex.Write(0);
            this.playersFinalized.Write(0x00);
            this.players.New(Player.MAX_PLAYERS);

            /// Initialize the non-heaped members.
            this.map = map;
            this.playerInitializer = ComponentManager.GetInterface<IPlayerInitializer>();
            this.entitiesOnMap = new BspSearchTree<Entity>(
                        new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                           -(RCNumber)1 / (RCNumber)2,
                                           this.map.CellSize.X,
                                           this.map.CellSize.Y),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspNodeCapacity"),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspMinNodeSize"));
            this.entitySet = new Dictionary<int, Entity>();
            this.boundQuadEntities = new QuadEntity[this.map.Size.X, this.map.Size.Y];
            this.commandExecutions = new HashSet<CmdExecutionBase>();
        }

        #region Public members: Player management

        /// <summary>
        /// Creates a new player to this scenario.
        /// </summary>
        /// <param name="index">The index of the new player.</param>
        /// <param name="startLocation">The start location of the new player.</param>
        /// <param name="race">The race of the new player.</param>
        public void CreatePlayer(int index, StartLocation startLocation, RaceEnum race)
        {
            if (this.playersFinalized.Read() != 0x00) { throw new InvalidOperationException("Players already finalized!"); }
            if (startLocation == null) { throw new ArgumentNullException("startLocation"); }
            if (startLocation.Scenario != this) { throw new SimulatorException("The given start location doesn't belong to the scenario!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index].Read() != null) { throw new SimulatorException(string.Format("Player with index {0} already exists!", index)); }

            Player newPlayer = new Player(index, startLocation);
            this.DetachEntityFromMap(startLocation);
            this.players[index].Write(newPlayer);
            this.playerInitializer.Initialize(newPlayer, race);
        }

        /// <summary>
        /// Deletes the given player and all of it's entities from this scenario.
        /// </summary>
        /// <param name="index">The index of the player to delete.</param>
        public void DeletePlayer(int index)
        {
            if (this.playersFinalized.Read() != 0x00) { throw new InvalidOperationException("Players already finalized!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index].Read() == null) { throw new SimulatorException(string.Format("Player with index {0} doesn't exist!", index)); }

            StartLocation startLoc = this.players[index].Read().StartLocation;
            this.AttachEntityToMap(startLoc, this.map.GetQuadTile(startLoc.LastKnownQuadCoords));
            HashSet<Entity> entitiesOfPlayer = new HashSet<Entity>(this.players[index].Read().Entities);
            this.players[index].Read().Dispose();
            this.players[index].Write(null);

            /// Destroy entities of the player.
            foreach (Entity entity in entitiesOfPlayer)
            {
                if (entity.PositionValue.Read() != RCNumVector.Undefined) { this.DetachEntityFromMap(entity); }
                this.RemoveEntityFromScenario(entity);
                entity.Dispose();
            }
        }

        /// <summary>
        /// Finalize the current players of this scenario. Creating or deleting players will be
        /// unavailable after calling this method.
        /// </summary>
        public void FinalizePlayers()
        {
            if (this.playersFinalized.Read() != 0x00) { throw new InvalidOperationException("Players already finalized!"); }

            foreach (StartLocation startLocation in this.GetEntitiesOnMap<StartLocation>())
            {
                this.DetachEntityFromMap(startLocation);
            }
            this.playersFinalized.Write(0x01);
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
            return this.players[index].Read();
        }

        /// <summary>
        /// Gets whether this Scenario has already at least 1 player or not.
        /// </summary>
        public bool HasPlayers { get { return this.players.Any(player => player.Read() != null); } }

        #endregion Public members: Player management

        #region Public members: Entity management

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
        /// Gets all of the entities of the given type added to this scenario.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <returns>A list that contains all of the entities of the given type added to this scenario.</returns>
        public HashSet<T> GetAllEntities<T>() where T : Entity
        {
            HashSet<T> retList = new HashSet<T>();
            foreach (Entity entity in this.entitySet.Values)
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Adds the given entity to this scenario.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        public void AddEntityToScenario(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            int id = this.nextID.Read();
            this.nextID.Write(id + 1);
            this.entitySet.Add(id, entity);
            entity.OnAddedToScenario(this, id);
        }

        /// <summary>
        /// Remove the given entity from this scenario.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void RemoveEntityFromScenario(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            this.entitySet.Remove(entity.ID.Read());
            entity.OnRemovedFromScenario();
        }

        #endregion Public members: Entity management

        #region Public members: Map management

        /// <summary>
        /// Gets the map of the scenario.
        /// </summary>
        public IMapAccess Map { get { return this.map; } }

        /// <summary>
        /// Gets the entity of the given type with the given ID that is attached to the map.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <returns>
        /// The entity with the given ID or null if no entity of the given type with the given ID is attached to the map.
        /// </returns>
        public T GetEntityOnMap<T>(int id) where T : Entity
        {
            if (this.entitySet.ContainsKey(id))
            {
                T retEntity = this.entitySet[id] as T;
                return this.entitiesOnMap.HasContent(retEntity) ? retEntity : null;
            }
            return null;
        }

        /// <summary>
        /// Gets the entities of the given type that are attached to the map.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <returns>A list that contains the entities of the given type that are attached to the map.</returns>
        public HashSet<T> GetEntitiesOnMap<T>() where T : Entity
        {
            HashSet<T> retList = new HashSet<T>();
            foreach (Entity entity in this.entitiesOnMap.GetContents())
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the entities of the given type that are attached to the map at the given position.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <param name="position">The position to search.</param>
        /// <returns>A list that contains the entities of the given type that are attached to the map at the given position.</returns>
        public HashSet<T> GetEntitiesOnMap<T>(RCNumVector position) where T : Entity
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            HashSet<T> retList = new HashSet<T>();
            foreach (Entity entity in this.entitiesOnMap.GetContents(position))
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the entities of the given type that are attached to the map inside the search area around the given position.
        /// </summary>
        /// <param name="position">The given position.</param>
        /// <param name="searchRadius">The radius of the search area given in quadratic tiles.</param>
        /// <returns>A list that contains the entities of the given type that are attached to the map inside the search area.</returns>
        public HashSet<T> GetEntitiesOnMap<T>(RCNumVector position, int searchRadius) where T : Entity
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }
            if (searchRadius <= 0) { throw new ArgumentOutOfRangeException("searchRadius", "The radius of the search area shall be greater than 0!"); }

            RCIntVector quadCoordAtPosition = this.Map.GetCell(position.Round()).ParentQuadTile.MapCoords;
            RCIntVector topLeftQuadCoord = quadCoordAtPosition - new RCIntVector(searchRadius - 1, searchRadius - 1);
            RCIntVector bottomRightQuadCoord = quadCoordAtPosition + new RCIntVector(searchRadius - 1, searchRadius - 1);
            RCIntRectangle quadRect = new RCIntRectangle(topLeftQuadCoord, bottomRightQuadCoord - topLeftQuadCoord + new RCIntVector(1, 1));

            HashSet<T> retList = new HashSet<T>();
            foreach (Entity entity in this.entitiesOnMap.GetContents((RCNumRectangle)this.Map.QuadToCellRect(quadRect) - new RCNumVector(1, 1) / 2))
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the entities of the given type that are attached to the map inside the given area.
        /// </summary>
        /// <typeparam name="T">The type of the entities to get.</typeparam>
        /// <param name="area">
        /// The area to search.
        /// </param>
        /// <returns>A list that contains the entities of the given type that are attached to the map inside the given area.</returns>
        public HashSet<T> GetEntitiesOnMap<T>(RCNumRectangle area) where T : Entity
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }

            HashSet<T> retList = new HashSet<T>();
            foreach (Entity entity in this.entitiesOnMap.GetContents(area))
            {
                T entityAsT = entity as T;
                if (entityAsT != null) { retList.Add(entityAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Gets the QuadEntity that is bound to the quadratic grid at the given position.
        /// </summary>
        /// <param name="quadCoords">The quadratic position on the map.</param>
        /// <returns>
        /// The QuadEntity that is bound to the quadratic grid at the given position or null if there is no QuadEntity bound to the map at the given position.
        /// </returns>
        public QuadEntity GetBoundQuadEntity(RCIntVector quadCoords)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            return this.boundQuadEntities[quadCoords.X, quadCoords.Y];
        }

        /// <summary>
        /// Attaches the given entity to the map into the given position.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>
        /// <param name="position">The position of the entity on the map.</param>
        /// <returns>True if the entity has been attached to the map successfully; otherwise false.</returns>
        public bool AttachEntityToMap(Entity entity, RCNumVector position)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }
            if (!this.entitySet.ContainsKey(entity.ID.Read())) { throw new InvalidOperationException("The entity has not yet been added to this scenario!"); }
            if (entity.PositionValue.Read() != RCNumVector.Undefined) { throw new InvalidOperationException("The entity has already been attached to the map!"); }

            bool isValidPosition = entity.OnAttachingToMap(position);
            if (isValidPosition)
            {
                this.entitiesOnMap.AttachContent(entity);
            }
            return isValidPosition;
        }

        /// <summary>
        /// Attaches the given QuadEntity to the given quadratic tile on the map.
        /// </summary>
        /// <param name="entity">The QuadEntity to be attached.</param>
        /// <param name="topLeftTile">The quadratic tile at the top-left corner of the QuadEntity.</param>
        /// <returns>True if the given QuadEntity was successfully attached to the map; otherwise false.</returns>
        public bool AttachEntityToMap(QuadEntity entity, IQuadTile topLeftTile)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }
            if (topLeftTile == null) { throw new ArgumentNullException("topLeftTile"); }
            if (!this.entitySet.ContainsKey(entity.ID.Read())) { throw new InvalidOperationException("The entity has not yet been added to a scenario!"); }
            if (entity.PositionValue.Read() != RCNumVector.Undefined) { throw new InvalidOperationException("The entity has already been attached to the map!"); }

            bool isValidPosition = entity.OnAttachingToMap(topLeftTile);
            if (isValidPosition)
            {
                this.entitiesOnMap.AttachContent(entity);
                for (int col = entity.QuadraticPosition.Left; col < entity.QuadraticPosition.Right; col++)
                {
                    for (int row = entity.QuadraticPosition.Top; row < entity.QuadraticPosition.Bottom; row++)
                    {
                        if (this.boundQuadEntities[col, row] != null) { throw new InvalidOperationException(string.Format("Another QuadEntity is already bound to quadratic tile at ({0};{1})!", col, row)); }
                        this.boundQuadEntities[col, row] = entity;
                    }
                }
            }
            return isValidPosition;
        }

        /// <summary>
        /// Detaches this entity from the map.
        /// </summary>
        /// <param name="entity">The entity to be detached.</param>
        public RCNumVector DetachEntityFromMap(Entity entity)
        {
            if (!this.entitySet.ContainsKey(entity.ID.Read())) { throw new InvalidOperationException("The entity has not yet been added to this scenario!"); }
            if (entity.PositionValue.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The entity has already been detached from the map!"); }

            QuadEntity entityAsQuadEntity = entity as QuadEntity;
            if (entityAsQuadEntity != null && entityAsQuadEntity.IsBoundToGrid)
            {
                for (int col = entityAsQuadEntity.QuadraticPosition.Left; col < entityAsQuadEntity.QuadraticPosition.Right; col++)
                {
                    for (int row = entityAsQuadEntity.QuadraticPosition.Top; row < entityAsQuadEntity.QuadraticPosition.Bottom; row++)
                    {
                        if (this.boundQuadEntities[col, row] != entityAsQuadEntity) { throw new InvalidOperationException(string.Format("QuadEntity is not bound to quadratic tile at ({0};{1})!", col, row)); }
                        this.boundQuadEntities[col, row] = null;
                    }
                }
            }

            RCNumVector currentPosition = entity.PositionValue.Read();
            this.entitiesOnMap.DetachContent(entity);
            entity.OnDetachedFromMap();
            return currentPosition;
        }

        #endregion Public members: Map management

        #region Public members: Simulation management

        /// <summary>
        /// Gets the index of the current simulation frame.
        /// </summary>
        public int CurrentFrameIndex { get { return this.currentFrameIndex.Read(); } }

        /// <summary>
        /// Updates the animations of the scenario.
        /// </summary>
        public void UpdateAnimations()
        {
            foreach (Entity entity in this.entitySet.Values)
            {
                foreach (AnimationPlayer animation in entity.CurrentAnimations) { animation.Step(); }
            }
        }

        /// <summary>
        /// Updates the state of the scenario.
        /// </summary>
        public void UpdateState()
        {
            HashSet<CmdExecutionBase> commandExecutionsCopy = new HashSet<CmdExecutionBase>(this.commandExecutions);
            foreach (CmdExecutionBase cmdExecution in commandExecutionsCopy)
            {
                cmdExecution.Continue();
            }
            foreach (Entity entity in this.entitySet.Values) { entity.UpdateState(); }
            this.currentFrameIndex.Write(this.currentFrameIndex.Read() + 1);
        }

        #endregion Public members: Simulation management

        #region Internal members: Command execution management

        /// <summary>
        /// Notifies this Scenario that a new command execution has been started.
        /// </summary>
        /// <param name="cmdExecution">The new command execution.</param>
        internal void OnCommandExecutionStarted(CmdExecutionBase cmdExecution)
        {
            this.commandExecutions.Add(cmdExecution);
        }

        /// <summary>
        /// Notifies this Scenario that an existing command execution has been stopped.
        /// </summary>
        /// <param name="cmdExecution">The stopped command execution.</param>
        internal void OnCommandExecutionStopped(CmdExecutionBase cmdExecution)
        {
            this.commandExecutions.Remove(cmdExecution);
        }

        #endregion Internal members: Command execution management

        #region IDisposable methods

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            /// Destroy command executions
            HashSet<CmdExecutionBase> commandExecutionsCopy = new HashSet<CmdExecutionBase>(this.commandExecutions);
            foreach (CmdExecutionBase cmdExecution in commandExecutionsCopy) { cmdExecution.Dispose(); }
            this.commandExecutions.Clear();

            /// Destroy players
            foreach (IValue<Player> player in this.players)
            {
                if (player.Read() != null)
                {
                    player.Read().Dispose();
                    player.Write(null);
                }
            }

            /// Destroy entities
            HashSet<Entity> entitySetCopy = new HashSet<Entity>(this.entitySet.Values);
            foreach (Entity entity in entitySetCopy)
            {
                if (entity.PositionValue.Read() != RCNumVector.Undefined) { this.DetachEntityFromMap(entity); }
                this.RemoveEntityFromScenario(entity);
                entity.Dispose();
            }
            this.entitySet.Clear();
        }

        #endregion IDisposable methods

        #region Heaped members

        /// <summary>
        /// The ID of the next entity.
        /// </summary>
        private readonly HeapedValue<int> nextID;

        /// <summary>
        /// The index of the current frame.
        /// </summary>
        private readonly HeapedValue<int> currentFrameIndex;

        /// <summary>
        /// This flag indicates whether the players are finalized (0x00) or not (any other value) on this scenario.
        /// </summary>
        private readonly HeapedValue<byte> playersFinalized;

        /// <summary>
        /// List of the players mapped by their IDs.
        /// </summary>
        private readonly HeapedArray<Player> players;

        #endregion Heaped members

        /// <summary>
        /// Reference to the map of the scenario.
        /// </summary>
        private IMapAccess map;

        /// <summary>
        /// The entities of the scenario that are attached to the map.
        /// </summary>
        /// TODO: store these entities also in a HeapedArray!
        private ISearchTree<Entity> entitiesOnMap;

        /// <summary>
        /// The entities of the scenario mapped by their IDs.
        /// </summary>
        /// TODO: store these entities also in a HeapedArray!
        private Dictionary<int, Entity> entitySet;

        /// <summary>
        /// This array stores for all quadratic tile the QuadEntity that is bound to that QuadTile.
        /// </summary>
        /// TODO: store these entities also in a HeapedArray!
        private QuadEntity[,] boundQuadEntities;

        /// <summary>
        /// The command executions currently in progress.
        /// </summary>
        private HashSet<CmdExecutionBase> commandExecutions;

        /// <summary>
        /// Reference to the player initializer component.
        /// </summary>
        private IPlayerInitializer playerInitializer;
    }
}
