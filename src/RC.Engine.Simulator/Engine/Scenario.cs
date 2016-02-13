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

namespace RC.Engine.Simulator.Engine
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
            this.elementFactory = ComponentManager.GetInterface<IElementFactory>();
            this.mapObjects = new Dictionary<MapObjectLayerEnum, ISearchTree<MapObject>>
            {
                { MapObjectLayerEnum.GroundObjects, this.CreateSearchTree() },
                { MapObjectLayerEnum.GroundReservations, this.CreateSearchTree() },
                { MapObjectLayerEnum.GroundMissiles, this.CreateSearchTree() },
                { MapObjectLayerEnum.AirObjects, this.CreateSearchTree() },
                { MapObjectLayerEnum.AirReservations, this.CreateSearchTree() },
                { MapObjectLayerEnum.AirMissiles, this.CreateSearchTree() }
            };
            this.idToScenarioElementMap = new Dictionary<int, ScenarioElement>();
            this.scenarioElements = new RCSet<ScenarioElement>();
            this.elementsToAddAfterUpdate = new RCSet<ScenarioElement>();
            this.elementsToRemoveAfterUpdate = new RCSet<ScenarioElement>();
            this.fixedEntities = new Entity[this.map.Size.X, this.map.Size.Y];
            this.commandExecutions = new RCSet<CmdExecutionBase>();
            this.addRemoveElementForbidden = false;
            this.updateInProgress = false;
        }

        #region Public members: Player management

        /// <summary>
        /// Initializes the player with the given index at the given start location.
        /// Initializing a player is only allowed if players have not yet been finalized.
        /// Players can be finalized using the Scenario.FinalizePlayers method.
        /// </summary>
        /// <param name="index">The index of the player to initialize.</param>
        /// <param name="startLocation">The start location of the player to initialize.</param>
        /// <param name="race">The race of the player to initialize.</param>
        public void InitializePlayer(int index, StartLocation startLocation, RaceEnum race)
        {
            if (this.playersFinalized.Read() != 0x00) { throw new InvalidOperationException("Players already finalized!"); }
            if (startLocation == null) { throw new ArgumentNullException("startLocation"); }
            if (startLocation.Scenario != this) { throw new SimulatorException("The given start location doesn't belong to the scenario!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index].Read() != null) { throw new SimulatorException(string.Format("Player with index {0} has already been initialized!", index)); }

            Player newPlayer = new Player(index, startLocation);
            startLocation.DetachFromMap();
            this.players[index].Write(newPlayer);
            this.elementFactory.InitializePlayer(newPlayer, race);
        }

        /// <summary>
        /// Uninitializes the player with the given index.
        /// Uninitializing a player is only allowed if players have not yet been finalized.
        /// Players can be finalized using the Scenario.FinalizePlayers method.
        /// </summary>
        /// <param name="index">The index of the player to uninitialize.</param>
        public void UninitializePlayer(int index)
        {
            if (this.playersFinalized.Read() != 0x00) { throw new InvalidOperationException("Players already finalized!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index].Read() == null) { throw new SimulatorException(string.Format("Player with index {0} has not yet been initialized!", index)); }

            StartLocation startLoc = this.players[index].Read().StartLocation;
            startLoc.AttachToMap(this.map.GetQuadTile(this.players[index].Read().QuadraticStartPosition.Location));
            RCSet<Entity> entitiesOfPlayer = new RCSet<Entity>(this.players[index].Read().Entities);
            this.players[index].Read().Dispose();
            this.players[index].Write(null);

            /// Destroy entities of the player.
            foreach (Entity entity in entitiesOfPlayer)
            {
                if (entity.HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects)) { entity.DetachFromMap(); }
                this.RemoveElementFromScenario(entity);
                entity.Dispose();
            }
        }

        /// <summary>
        /// Destroys the player with the given index.
        /// Destroying a player is only allowed if the players have already been finalized.
        /// Players can be finalized using the Scenario.FinalizePlayers method.
        /// </summary>
        /// <param name="index">The index of the player to be destroyed.</param>
        public void DestroyPlayer(int index)
        {
            if (this.playersFinalized.Read() == 0x00) { throw new InvalidOperationException("Players has not yet been finalized!"); }
            if (index < 0 || index >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("index"); }
            if (this.players[index].Read() == null) { throw new SimulatorException(string.Format("Player with index {0} doesn't exist!", index)); }

            /// Remove all jobs from the active production line of every entity of the player.
            RCSet<Entity> entitiesOfPlayer = new RCSet<Entity>(this.players[index].Read().Entities);
            foreach (Entity entityOfPlayer in entitiesOfPlayer)
            {
                if (entityOfPlayer.ActiveProductionLine != null) { entityOfPlayer.ActiveProductionLine.RemoveAllJobs(); }
            }

            /// Destroy the player itself.
            this.players[index].Read().Dispose();
            this.players[index].Write(null);

            /// Destroy entities of the player.
            foreach (Entity entity in entitiesOfPlayer)
            {
                if (entity.HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects)) { entity.DetachFromMap(); }
                this.RemoveElementFromScenario(entity);
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

            foreach (StartLocation startLocation in this.GetElementsOnMap<StartLocation>(MapObjectLayerEnum.GroundObjects))
            {
                startLocation.DetachFromMap();
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

        #region Public members: Scenario element management

        /// <summary>
        /// Gets the scenario element of the given type with the given ID.
        /// </summary>
        /// <param name="id">The ID of the scenario element.</param>
        /// <typeparam name="T">The type of the scenario element.</typeparam>
        /// <returns>
        /// The scenario element with the given ID or null if no scenario element of the given type with the given ID is added to this scenario.
        /// </returns>
        public T GetElement<T>(int id) where T : ScenarioElement
        {
            return this.idToScenarioElementMap.ContainsKey(id) ? this.idToScenarioElementMap[id] as T : null;
        }
        
        /// <summary>
        /// Gets all of the scenario elements of the given type added to this scenario.
        /// </summary>
        /// <typeparam name="T">The type of the scenario elements to get.</typeparam>
        /// <returns>A list that contains all of the scenario elements of the given type added to this scenario.</returns>
        public RCSet<T> GetAllElements<T>() where T : ScenarioElement
        {
            RCSet<T> retList = new RCSet<T>();
            foreach (ScenarioElement element in this.scenarioElements)
            {
                if (this.elementsToRemoveAfterUpdate.Contains(element)) { continue; }

                T elementAsT = element as T;
                if (elementAsT != null) { retList.Add(elementAsT); }
            }

            foreach (ScenarioElement element in this.elementsToAddAfterUpdate)
            {
                T elementAsT = element as T;
                if (elementAsT != null) { retList.Add(elementAsT); }
            }
            return retList;
        }

        /// <summary>
        /// Adds the given element to this scenario.
        /// </summary>
        /// <param name="element">The scenario element to be added.</param>
        public void AddElementToScenario(ScenarioElement element)
        {
            if (this.addRemoveElementForbidden) { throw new InvalidOperationException("Adding element to this scenario is currently forbidden!"); }
            if (element == null) { throw new ArgumentNullException("element"); }

            int id = this.nextID.Read();
            this.nextID.Write(id + 1);
            this.idToScenarioElementMap.Add(id, element);

            if (!this.updateInProgress)
            {
                this.scenarioElements.Add(element);
            }
            else
            {
                this.elementsToRemoveAfterUpdate.Remove(element);
                this.elementsToAddAfterUpdate.Add(element);
            }
            this.addRemoveElementForbidden = true;
            element.OnAddedToScenario(this, id, new ScenarioMapContext(this.mapObjects, this.fixedEntities));
            this.addRemoveElementForbidden = false;
        }

        /// <summary>
        /// Remove the given element from this scenario.
        /// </summary>
        /// <param name="element">The element to be removed.</param>
        /// <remarks>
        /// Disposing the removed element is always the responsibility of the caller, except when the element is removed during a
        /// scenario update procedure.
        /// </remarks>
        public void RemoveElementFromScenario(ScenarioElement element)
        {
            if (this.addRemoveElementForbidden) { throw new InvalidOperationException("Removing element from this scenario is currently forbidden!"); }
            if (element == null) { throw new ArgumentNullException("element"); }
            
            this.idToScenarioElementMap.Remove(element.ID.Read());

            if (!this.updateInProgress)
            {
                this.scenarioElements.Remove(element);
            }
            else
            {
                this.elementsToRemoveAfterUpdate.Add(element);
                this.elementsToAddAfterUpdate.Remove(element);
            }

            this.addRemoveElementForbidden = true;
            element.OnRemovedFromScenario();
            this.addRemoveElementForbidden = false;
        }

        #endregion Public members: Scenario element management

        #region Public members: Map management

        /// <summary>
        /// Gets the map of the scenario.
        /// </summary>
        public IMapAccess Map { get { return this.map; } }

        /// <summary>
        /// Gets the scenario element of the given type with the given ID that is attached to at least one of the given layers of the map.
        /// </summary>
        /// <typeparam name="T">The type of the scenario element.</typeparam>
        /// <param name="id">The ID of the scenario element.</param>
        /// <param name="firstLayer">The first layer to search in.</param>
        /// <param name="furtherLayers">List of the further layers to search in.</param>
        /// <returns>
        /// The scenario element with the given ID or null if no scenario element of the given type with the given ID is attached to at
        /// least one of the given layers of the map.
        /// </returns>
        public T GetElementOnMap<T>(int id, MapObjectLayerEnum firstLayer, params MapObjectLayerEnum[] furtherLayers) where T : ScenarioElement
        {
            if (furtherLayers == null) { throw new ArgumentNullException("furtherLayers"); }

            if (this.idToScenarioElementMap.ContainsKey(id))
            {
                T retElement = this.idToScenarioElementMap[id] as T;
                return retElement != null && retElement.HasMapObject(firstLayer, furtherLayers)
                    ? retElement
                    : null;
            }
            return null;
        }

        /// <summary>
        /// Gets the scenario elements of the given type that are attached to at least one of the given layers of the map.
        /// </summary>
        /// <typeparam name="T">The type of the scenario elements to get.</typeparam>
        /// <param name="firstLayer">The first layer to search in.</param>
        /// <param name="furtherLayers">List of the further layers to search in.</param>
        /// <returns>
        /// A list that contains the scenario elements of the given type that are attached to at least one of the given layers of the map.
        /// </returns>
        public RCSet<T> GetElementsOnMap<T>(MapObjectLayerEnum firstLayer, params MapObjectLayerEnum[] furtherLayers) where T : ScenarioElement
        {
            if (furtherLayers == null) { throw new ArgumentNullException("furtherLayers"); }

            RCSet<T> retList = new RCSet<T>();
            foreach (MapObject mapObj in this.mapObjects[firstLayer].GetContents())
            {
                T elementAsT = mapObj.Owner as T;
                if (elementAsT != null) { retList.Add(elementAsT); }
            }
            foreach (MapObjectLayerEnum furtherLayer in furtherLayers)
            {
                foreach (MapObject mapObj in this.mapObjects[furtherLayer].GetContents())
                {
                    T elementAsT = mapObj.Owner as T;
                    if (elementAsT != null) { retList.Add(elementAsT); }
                }
            }
            return retList;
        }

        /// <summary>
        /// Gets the scenario elements of the given type that are attached to at least one of the given layers of the map at the given position.
        /// </summary>
        /// <typeparam name="T">The type of the scenario elements to get.</typeparam>
        /// <param name="position">The position to search.</param>
        /// <param name="firstLayer">The first layer to search in.</param>
        /// <param name="furtherLayers">List of the further layers to search in.</param>
        /// <returns>
        /// A list that contains the scenario elements of the given type that are attached to at least one of the given layers of the map at
        /// the given position.
        /// </returns>
        public RCSet<T> GetElementsOnMap<T>(RCNumVector position, MapObjectLayerEnum firstLayer, params MapObjectLayerEnum[] furtherLayers) where T : ScenarioElement
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }
            if (furtherLayers == null) { throw new ArgumentNullException("furtherLayers"); }

            RCSet<T> retList = new RCSet<T>();
            foreach (MapObject mapObj in this.mapObjects[firstLayer].GetContents(position))
            {
                T elementAsT = mapObj.Owner as T;
                if (elementAsT != null) { retList.Add(elementAsT); }
            }
            foreach (MapObjectLayerEnum furtherLayer in furtherLayers)
            {
                foreach (MapObject mapObj in this.mapObjects[furtherLayer].GetContents(position))
                {
                    T elementAsT = mapObj.Owner as T;
                    if (elementAsT != null) { retList.Add(elementAsT); }
                }
            }
            return retList;
        }

        /// <summary>
        /// Gets the scenario elements of the given type that are attached to at least one of the given layers of the map inside the given area.
        /// </summary>
        /// <typeparam name="T">The type of the scenario elements to get.</typeparam>
        /// <param name="area">
        /// <param name="firstLayer">The first layer to search in.</param>
        /// <param name="furtherLayers">List of the further layers to search in.</param>
        /// The area to search.
        /// </param>
        /// <returns>
        /// A list that contains the scenario elements of the given type that are attached to at least one of the given layers of the map inside 
        /// the given area.
        /// </returns>
        public RCSet<T> GetElementsOnMap<T>(RCNumRectangle area, MapObjectLayerEnum firstLayer, params MapObjectLayerEnum[] furtherLayers) where T : ScenarioElement
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (furtherLayers == null) { throw new ArgumentNullException("furtherLayers"); }

            RCSet<T> retList = new RCSet<T>();
            foreach (MapObject mapObj in this.mapObjects[firstLayer].GetContents(area))
            {
                T elementAsT = mapObj.Owner as T;
                if (elementAsT != null) { retList.Add(elementAsT); }
            }
            foreach (MapObjectLayerEnum furtherLayer in furtherLayers)
            {
                foreach (MapObject mapObj in this.mapObjects[furtherLayer].GetContents(area))
                {
                    T elementAsT = mapObj.Owner as T;
                    if (elementAsT != null) { retList.Add(elementAsT); }
                }
            }
            return retList;
        }

        /// <summary>
        /// Gets the scenario elements of the given type that are attached to at least one of the given layers of the map inside the search area
        /// around the given position.
        /// </summary>
        /// <typeparam name="T">The type of the scenario elements to get.</typeparam>
        /// <param name="position">The given position.</param>
        /// <param name="searchRadius">The radius of the search area given in quadratic tiles.</param>
        /// <param name="firstLayer">The first layer to search in.</param>
        /// <param name="furtherLayers">List of the further layers to search in.</param>
        /// <returns>
        /// A list that contains the scenario elements of the given type that are attached to at least one of the given layers of the map inside
        /// the search area.
        /// </returns>
        public RCSet<T> GetElementsOnMap<T>(RCNumVector position, int searchRadius, MapObjectLayerEnum firstLayer, params MapObjectLayerEnum[] furtherLayers) where T : ScenarioElement
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }
            if (searchRadius <= 0) { throw new ArgumentOutOfRangeException("searchRadius", "The radius of the search area shall be greater than 0!"); }
            if (furtherLayers == null) { throw new ArgumentNullException("furtherLayers"); }

            RCIntVector quadCoordAtPosition = this.Map.GetCell(position.Round()).ParentQuadTile.MapCoords;
            RCIntVector topLeftQuadCoord = quadCoordAtPosition - new RCIntVector(searchRadius - 1, searchRadius - 1);
            RCIntVector bottomRightQuadCoord = quadCoordAtPosition + new RCIntVector(searchRadius - 1, searchRadius - 1);
            RCIntRectangle quadRect = new RCIntRectangle(topLeftQuadCoord, bottomRightQuadCoord - topLeftQuadCoord + new RCIntVector(1, 1));

            RCSet<T> retList = new RCSet<T>();
            foreach (MapObject mapObj in this.mapObjects[firstLayer].GetContents((RCNumRectangle)this.Map.QuadToCellRect(quadRect) - new RCNumVector(1, 1) / 2))
            {
                T elementAsT = mapObj.Owner as T;
                if (elementAsT != null) { retList.Add(elementAsT); }
            }
            foreach (MapObjectLayerEnum furtherLayer in furtherLayers)
            {
                foreach (MapObject mapObj in this.mapObjects[furtherLayer].GetContents((RCNumRectangle)this.Map.QuadToCellRect(quadRect) - new RCNumVector(1, 1) / 2))
                {
                    T elementAsT = mapObj.Owner as T;
                    if (elementAsT != null) { retList.Add(elementAsT); }
                }
            }
            return retList;
        }

        /// <summary>
        /// Gets the map objects inside the given area of the given layers.
        /// </summary>
        /// <param name="area">The area to search.</param>
        /// <param name="firstLayer">The first layer to search in.</param>
        /// <param name="furtherLayers">List of the further layers to search in.</param>
        /// <returns>A list that contains the map objects inside the given area of the given layers.</returns>
        public RCSet<MapObject> GetMapObjects(RCNumRectangle area, MapObjectLayerEnum firstLayer, params MapObjectLayerEnum[] furtherLayers)
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (furtherLayers == null) { throw new ArgumentNullException("furtherLayers"); }

            RCSet<MapObject> retList = this.mapObjects[firstLayer].GetContents(area);
            foreach (MapObjectLayerEnum furtherLayer in furtherLayers)
            {
                retList.UnionWith(this.mapObjects[furtherLayer].GetContents(area));
            }
            return retList;
        }

        /// <summary>
        /// Gets the entity of the given type that is fixed to the quadratic grid at the given position.
        /// </summary>
        /// <typeparam name="T">The type of the entity to get.</typeparam>
        /// <param name="quadCoords">The quadratic position on the map.</param>
        /// <returns>
        /// The entity of the given type that is fixed to the quadratic grid at the given position or null if there is no entity of that type
        /// fixed to the map at the given position.
        /// </returns>
        public T GetFixedEntity<T>(RCIntVector quadCoords) where T : Entity
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            return this.fixedEntities[quadCoords.X, quadCoords.Y] as T;
        }

        #endregion Public members: Map management

        #region Public members: Simulation management

        /// <summary>
        /// Gets the index of the current simulation frame.
        /// </summary>
        public int CurrentFrameIndex { get { return this.currentFrameIndex.Read(); } }

        /// <summary>
        /// Updates the state of the scenario.
        /// </summary>
        public void Update()
        {
            if (this.updateInProgress) { throw new InvalidOperationException("Updating the scenario is currently forbidden!"); }

            this.updateInProgress = true;

            /// Update the command executions.
            List<CmdExecutionBase> commandExecutionsCopy = new List<CmdExecutionBase>(this.commandExecutions);
            foreach (CmdExecutionBase cmdExecution in commandExecutionsCopy) { cmdExecution.Continue(); }

            /// Update the scenario elements.
            foreach (ScenarioElement element in this.scenarioElements)
            {
                if (this.elementsToRemoveAfterUpdate.Contains(element)) { continue; }
                element.UpdateState();
            }
            this.currentFrameIndex.Write(this.currentFrameIndex.Read() + 1);
            this.updateInProgress = false;

            /// Perform element additions and removals.
            foreach (ScenarioElement elementToRemove in this.elementsToRemoveAfterUpdate)
            {
                this.scenarioElements.Remove(elementToRemove);
                elementToRemove.Dispose();  // Automatically dispose elements added during the update procedure.
            }
            foreach (ScenarioElement elementToAdd in this.elementsToAddAfterUpdate)
            {
                this.scenarioElements.Add(elementToAdd);
            }
            this.elementsToRemoveAfterUpdate.Clear();
            this.elementsToAddAfterUpdate.Clear();
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
            if (this.updateInProgress) { throw new InvalidOperationException("Updating the scenario is currently in progress!"); }

            /// Destroy command executions.
            RCSet<CmdExecutionBase> commandExecutionsCopy = new RCSet<CmdExecutionBase>(this.commandExecutions);
            foreach (CmdExecutionBase cmdExecution in commandExecutionsCopy) { cmdExecution.Dispose(); }
            this.commandExecutions.Clear();

            /// Destroy players.
            foreach (IValue<Player> player in this.players)
            {
                if (player.Read() != null) { this.DestroyPlayer(player.Read().PlayerIndex); }
            }

            /// Destroy the remaining scenario elements.
            List<ScenarioElement> elementSetCopy = new List<ScenarioElement>(this.scenarioElements);
            foreach (ScenarioElement element in elementSetCopy)
            {
                element.DetachFromMap();
                this.RemoveElementFromScenario(element);
                element.Dispose();
            }
            this.scenarioElements.Clear();
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
        /// Creates a search tree for map objects with the size of the map of this scenario.
        /// </summary>
        /// <returns>The created search tree.</returns>
        private ISearchTree<MapObject> CreateSearchTree()
        {
            return new BspSearchTree<MapObject>(
                new RCNumRectangle(-(RCNumber) 1/(RCNumber) 2,
                    -(RCNumber) 1/(RCNumber) 2,
                    this.map.CellSize.X,
                    this.map.CellSize.Y),
                ConstantsTable.Get<int>("RC.Engine.Maps.BspNodeCapacity"),
                ConstantsTable.Get<int>("RC.Engine.Maps.BspMinNodeSize"));
        }

        /// <summary>
        /// Reference to the map of the scenario.
        /// </summary>
        private readonly IMapAccess map;

        /// <summary>
        /// The map objects of the scenario that are attached to the map mapped by the layers they are attached to.
        /// </summary>
        /// TODO: store these entities also in a HeapedArray!
        private readonly Dictionary<MapObjectLayerEnum, ISearchTree<MapObject>> mapObjects;

        /// <summary>
        /// This array stores for all quadratic tile the Entity that is fixed to that QuadTile.
        /// </summary>
        /// TODO: store these entities also in a HeapedArray!
        private readonly Entity[,] fixedEntities;

        /// <summary>
        /// The command executions currently in progress.
        /// </summary>
        private readonly RCSet<CmdExecutionBase> commandExecutions;

        /// <summary>
        /// The elements of the scenario mapped by their IDs.
        /// </summary>
        /// TODO: store these objects also in a HeapedArray!
        private readonly Dictionary<int, ScenarioElement> idToScenarioElementMap;

        /// <summary>
        /// The elements of the scenario.
        /// </summary>
        /// TODO: store these objects also in a HeapedArray!
        private readonly RCSet<ScenarioElement> scenarioElements;

        /// <summary>
        /// Temporary set of elements to add after the current update procedure has finished.
        /// </summary>
        private readonly RCSet<ScenarioElement> elementsToAddAfterUpdate;

        /// <summary>
        /// Temporary set of elements to remove after the current update procedure has finished.
        /// </summary>
        private readonly RCSet<ScenarioElement> elementsToRemoveAfterUpdate;

        /// <summary>
        /// Reference to the element factory component.
        /// </summary>
        private readonly IElementFactory elementFactory;

        /// <summary>
        /// This flag indicates if adding and removing scenario elements is currently forbidden or not.
        /// </summary>
        private bool addRemoveElementForbidden;

        /// <summary>
        /// This flag indicates if updating this scenario is currently in progress.
        /// </summary>
        private bool updateInProgress;
    }
}
