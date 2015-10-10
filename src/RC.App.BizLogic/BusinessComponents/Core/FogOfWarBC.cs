using System;
using System.Collections.Generic;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the Fog Of War business component.
    /// </summary>
    [Component("RC.App.BizLogic.FogOfWarBC")]
    class FogOfWarBC : ScenarioDependentComponent, IFogOfWarBC
    {
        /// <summary>
        /// Constructs a FogOfWarBC instance.
        /// </summary>
        public FogOfWarBC()
        {
            this.Reset();
        }

        #region Overrides from ScenarioDependentComponent

        /// <see cref="ScenarioDependentComponent.StartImpl"/>
        protected override void StartImpl()
        {
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();
        }

        /// <see cref="ScenarioDependentComponent.OnActiveScenarioChanged"/>
        protected override void OnActiveScenarioChanged(Scenario activeScenario)
        {
            this.Reset();
        }

        #endregion Overrides from ScenarioDependentComponent

        #region IFogOfWarBC methods

        /// <see cref="IFogOfWarBC.StartFogOfWar"/>
        public void StartFogOfWar(PlayerEnum owner)
        {
            if (owner == PlayerEnum.Neutral) { throw new ArgumentException("Fog Of War owner cannot be PlayerEnum.Neutral!"); }
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            
            Player ownerPlayer = this.ActiveScenario.GetPlayer((int)owner);
            if (ownerPlayer == null) { throw new InvalidOperationException(string.Format("Player {0} doesn't exist!", owner)); }

            if (this.runningFows[(int)owner] != null) { throw new InvalidOperationException(string.Format("Fog Of War for player {0} is already running!", owner)); }
            this.runningFows[(int)owner] = new FogOfWar(ownerPlayer);
            this.runningFowsCount++;

            if (this.runningFowsCount == 1)
            {
                this.fowCacheMatrix = new FowCacheMatrix(this.runningFows, this.ActiveScenario);
                this.cache = new CachedValue<FowVisibilityInfo>(this.CalculateVisibilityWithFow);
            }
            else
            {
                this.cache.Invalidate();
            }
        }

        /// <see cref="IFogOfWarBC.StopFogOfWar"/>
        public void StopFogOfWar(PlayerEnum owner)
        {
            if (owner == PlayerEnum.Neutral) { throw new ArgumentException("Fog Of War owner cannot be PlayerEnum.Neutral!"); }
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            if (this.runningFows[(int)owner] == null) { throw new InvalidOperationException(string.Format("Fog Of War for player {0} is not running!", owner)); }
            this.runningFows[(int)owner] = null;
            this.runningFowsCount--;
            if (this.runningFowsCount == 0)
            {
                this.quadTileWindow = RCIntRectangle.Undefined;
                this.fowCacheMatrix = null;
                this.currentIterationIndex = 0;
                this.fowIndex = 0;
                this.cache = new CachedValue<FowVisibilityInfo>(this.CalculateVisibilityWithoutFow);
            }
            else
            {
                this.cache.Invalidate();
            }
        }

        /// <see cref="IFogOfWarBC.GetIsoTilesToUpdate"/>
        public IEnumerable<IIsoTile> GetIsoTilesToUpdate()
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.UpdateQuadTileWindow();
            return this.cache.Value.IsoTilesToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetTerrainObjectsToUpdate"/>
        public IEnumerable<ITerrainObject> GetTerrainObjectsToUpdate()
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.UpdateQuadTileWindow();
            return this.cache.Value.TerrainObjectsToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetQuadTilesToUpdate"/>
        public IEnumerable<IQuadTile> GetQuadTilesToUpdate()
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.UpdateQuadTileWindow();
            return this.cache.Value.QuadTilesToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetEntitySnapshotsToUpdate"/>
        public IEnumerable<EntitySnapshot> GetEntitySnapshotsToUpdate()
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.UpdateQuadTileWindow();
            return this.cache.Value.EntitySnapshotsToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetMapObjectsToUpdate"/>
        public IEnumerable<MapObject> GetMapObjectsToUpdate()
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.UpdateQuadTileWindow();
            return this.cache.Value.MapObjectsToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetEntitySnapshotsInWindow"/>
        public IEnumerable<EntitySnapshot> GetEntitySnapshotsInWindow(RCIntRectangle quadWindow)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.runningFowsCount == 0) { yield break; }

            for (int column = quadWindow.Left; column < quadWindow.Right; column++)
            {
                for (int row = quadWindow.Top; row < quadWindow.Bottom; row++)
                {
                    RCIntVector quadCoords = new RCIntVector(column, row);

                    /// If the FOW is full at the current quadratic tile -> continue with the next.
                    FOWTypeEnum fowAtQuadTile = this.fowCacheMatrix.GetFowStateAtQuadTile(quadCoords);
                    if (fowAtQuadTile == FOWTypeEnum.Full) { continue; }

                    /// Add the entity snapshot into the returned list.
                    EntitySnapshot snapshot = this.fowCacheMatrix.GetEntitySnapshotAtQuadTile(quadCoords);
                    if (snapshot != null) { yield return snapshot; }
                }
            }
        }

        /// <see cref="IFogOfWarBC.GetMapObjectsInWindow"/>
        public IEnumerable<MapObject> GetMapObjectsInWindow(RCIntRectangle quadWindow)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            /// Collect the currently visible entities inside the given window.
            RCNumRectangle cellWindow = (RCNumRectangle)this.ActiveScenario.Map.QuadToCellRect(quadWindow) - new RCNumVector(1, 1) / 2;
            RCSet<MapObject> mapObjectsOnMap = this.ActiveScenario.GetMapObjects(cellWindow, MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects);
            foreach (MapObject mapObject in mapObjectsOnMap)
            {
                if (this.runningFowsCount == 0)
                {
                    yield return mapObject;
                }
                else
                {
                    bool breakLoop = false;
                    for (int col = mapObject.QuadraticPosition.Left; !breakLoop && col < mapObject.QuadraticPosition.Right; col++)
                    {
                        for (int row = mapObject.QuadraticPosition.Top; !breakLoop && row < mapObject.QuadraticPosition.Bottom; row++)
                        {
                            if (this.fowCacheMatrix.GetFowStateAtQuadTile(new RCIntVector(col, row)) == FOWTypeEnum.None)
                            {
                                /// Found at least 1 quadratic tile where the entity is visible.
                                yield return mapObject;
                                breakLoop = true;
                            }
                        }
                    }
                }
            }
        }

        /// <see cref="IFogOfWarBC.IsMapObjectVisible"/>
        public bool IsMapObjectVisible(MapObject mapObjects)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.UpdateQuadTileWindow();
            return this.cache.Value.MapObjectsToUpdate.Contains(mapObjects);
        }

        /// <see cref="IFogOfWarBC.GetFullFowTileFlags"/>
        public FOWTileFlagsEnum GetFullFowTileFlags(RCIntVector quadCoords)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
                        
            return this.runningFowsCount > 0 ? this.fowCacheMatrix.GetFullFowFlagsAtQuadTile(quadCoords) : FOWTileFlagsEnum.None;
        }

        /// <see cref="IFogOfWarBC.GetPartialFowTileFlags"/>
        public FOWTileFlagsEnum GetPartialFowTileFlags(RCIntVector quadCoords)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            return this.runningFowsCount > 0 ? this.fowCacheMatrix.GetPartialFowFlagsAtQuadTile(quadCoords) : FOWTileFlagsEnum.None;
        }

        /// <see cref="IFogOfWarBC.GetPartialFowTileFlags"/>
        public FOWTypeEnum GetFowState(RCIntVector quadCoords)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            return this.runningFowsCount > 0 ? this.fowCacheMatrix.GetFowStateAtQuadTile(quadCoords) : FOWTypeEnum.None;
        }

        /// <see cref="IFogOfWarBC.ExecuteUpdateIteration"/>
        public void ExecuteUpdateIteration()
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.runningFowsCount == 0) { return; }

            int remainingEntitiesInCurrIteration = MAX_ENTITIES_PER_ITERATION;
            RCSet<FogOfWar> fowsProcessedInCurrIteration = new RCSet<FogOfWar>();
            while (remainingEntitiesInCurrIteration > 0)
            {
                /// Search the first running FOW starting from the current index.
                while (this.runningFows[this.fowIndex] == null) { this.fowIndex = (this.fowIndex + 1) % this.runningFows.Length; }

                /// First running FOW found. Finish the iteration if that FOW has already been processed in this iteration.
                FogOfWar fowToProcess = this.runningFows[this.fowIndex];
                if (fowsProcessedInCurrIteration.Contains(fowToProcess)) { break; }

                /// Start or continue updating the FOW based on whether this is the first iteration or not.
                remainingEntitiesInCurrIteration -= this.currentIterationIndex == 0
                                                  ? fowToProcess.RestartUpdate(remainingEntitiesInCurrIteration)
                                                  : fowToProcess.ContinueUpdate(remainingEntitiesInCurrIteration);

                /// Add the updated FOW to the set of already processed FOWs and move to the next FOW-index.
                fowsProcessedInCurrIteration.Add(fowToProcess);
                this.fowIndex = (this.fowIndex + 1) % this.runningFows.Length;
            }

            /// Move to the next iteration.
            this.currentIterationIndex++;

            /// If all the running FOWs have been updated in this iteration, finish the update if necessary.
            if (fowsProcessedInCurrIteration.Count == this.runningFowsCount &&
                this.currentIterationIndex == MIN_ITERATIONS_PER_UPDATE)
            {
                this.currentIterationIndex = 0;
            }

            /// Invalidate the cache if necessary.
            if (fowsProcessedInCurrIteration.Count > 0) { this.cache.Invalidate(); }
        }

        #endregion IFogOfWarBC methods

        #region Internal methods

        /// <summary>
        /// Implements visibility calculations when at least 1 FogOfWar is running.
        /// </summary>
        /// <returns>The results of the visibility calculations.</returns>
        private FowVisibilityInfo CalculateVisibilityWithFow()
        {
            if (this.quadTileWindow == RCIntRectangle.Undefined) { throw new InvalidOperationException("Visibility window not defined!"); }

            /// Collect the isometric & quadratic tiles that need to be updated.
            IMapAccess map = this.ActiveScenario.Map;
            RCSet<IIsoTile> isoTilesToUpdate = new RCSet<IIsoTile>();
            RCSet<IQuadTile> quadTilesToUpdate = new RCSet<IQuadTile>();
            RCSet<ITerrainObject> terrainObjectsToUpdate = new RCSet<ITerrainObject>();
            RCSet<EntitySnapshot> entitySnapshotsToUpdate = new RCSet<EntitySnapshot>();
            for (int column = this.quadTileWindow.Left; column < this.quadTileWindow.Right; column++)
            {
                for (int row = this.quadTileWindow.Top; row < this.quadTileWindow.Bottom; row++)
                {
                    RCIntVector quadCoords = new RCIntVector(column, row);

                    /// If the FOW is full at the current quadratic tile -> continue with the next.
                    FOWTypeEnum fowAtQuadTile = this.fowCacheMatrix.GetFowStateAtQuadTile(quadCoords);
                    if (fowAtQuadTile == FOWTypeEnum.Full) { continue; }

                    /// Add the primary & secondary isometric tiles and all of their cutting quadratic tiles into the update lists.
                    IQuadTile quadTileToUpdate = map.GetQuadTile(quadCoords);
                    this.AddIsoTileToUpdate(quadTileToUpdate.PrimaryIsoTile, isoTilesToUpdate, quadTilesToUpdate);
                    this.AddIsoTileToUpdate(quadTileToUpdate.SecondaryIsoTile, isoTilesToUpdate, quadTilesToUpdate);

                    /// Add the terrain object and all of its cutting quadratic tiles into the update lists.
                    this.AddTerrainObjectToUpdate(quadTileToUpdate.TerrainObject, terrainObjectsToUpdate, quadTilesToUpdate);

                    /// Add the entity snapshot and all of its cutting quadratic tiles into the update lists.
                    EntitySnapshot entitySnapshotAtQuadTile = this.fowCacheMatrix.GetEntitySnapshotAtQuadTile(quadCoords);
                    this.AddEntitySnapshotToUpdate(entitySnapshotAtQuadTile, entitySnapshotsToUpdate, quadTilesToUpdate);
                }
            }

            /// Collect the currently visible map objects.
            RCSet<MapObject> objectsOnMap = this.ActiveScenario.GetMapObjects(
                this.mapWindowBC.AttachedWindow.WindowMapCoords,
                MapObjectLayerEnum.GroundObjects,
                MapObjectLayerEnum.GroundMissiles,
                MapObjectLayerEnum.AirObjects,
                MapObjectLayerEnum.AirMissiles);
            RCSet<MapObject> mapObjectsToUpdate = new RCSet<MapObject>();
            foreach (MapObject mapObj in objectsOnMap)
            {
                bool breakLoop = false;
                for (int col = mapObj.QuadraticPosition.Left; !breakLoop && col < mapObj.QuadraticPosition.Right; col++)
                {
                    for (int row = mapObj.QuadraticPosition.Top; !breakLoop && row < mapObj.QuadraticPosition.Bottom; row++)
                    {
                        if (this.fowCacheMatrix.GetFowStateAtQuadTile(new RCIntVector(col, row)) == FOWTypeEnum.None)
                        {
                            /// Found at least 1 quadratic tile where the map objects is visible.
                            this.AddMapObjectToUpdate(mapObj, mapObjectsToUpdate, quadTilesToUpdate);
                            breakLoop = true;
                        }
                    }
                }
            }

            /// Create the calculated visibility info.
            return new FowVisibilityInfo
            {
                IsoTilesToUpdate = isoTilesToUpdate,
                TerrainObjectsToUpdate = terrainObjectsToUpdate,
                QuadTilesToUpdate = quadTilesToUpdate,
                EntitySnapshotsToUpdate = entitySnapshotsToUpdate,
                MapObjectsToUpdate = mapObjectsToUpdate
            };
        }

        /// <summary>
        /// Implements visibility calculations when there is no running FogOfWars.
        /// </summary>
        /// <returns>The results of the visibility calculations.</returns>
        private FowVisibilityInfo CalculateVisibilityWithoutFow()
        {
            /// Collect the isometric & quadratic tiles that need to be updated.
            IMapAccess map = this.ActiveScenario.Map;
            RCSet<IIsoTile> isoTilesToUpdate = new RCSet<IIsoTile>();
            RCSet<ITerrainObject> terrainObjectsToUpdate = new RCSet<ITerrainObject>();
            for (int column = this.quadTileWindow.Left; column < this.quadTileWindow.Right; column++)
            {
                for (int row = this.quadTileWindow.Top; row < this.quadTileWindow.Bottom; row++)
                {
                    /// Add the primary & secondary isometric tiles into the update lists.
                    IQuadTile quadTileToUpdate = map.GetQuadTile(new RCIntVector(column, row));
                    if (quadTileToUpdate.PrimaryIsoTile != null) { isoTilesToUpdate.Add(quadTileToUpdate.PrimaryIsoTile); }
                    if (quadTileToUpdate.SecondaryIsoTile != null) { isoTilesToUpdate.Add(quadTileToUpdate.SecondaryIsoTile); }
                    if (quadTileToUpdate.TerrainObject != null) { terrainObjectsToUpdate.Add(quadTileToUpdate.TerrainObject); }
                }
            }

            /// Collect the currently visible map objects.
            RCSet<MapObject> mapObjectsToUpdate = this.ActiveScenario.GetMapObjects(
                this.mapWindowBC.AttachedWindow.WindowMapCoords,
                MapObjectLayerEnum.GroundObjects,
                MapObjectLayerEnum.GroundMissiles,
                MapObjectLayerEnum.AirObjects,
                MapObjectLayerEnum.AirMissiles);

            /// Create the calculated visibility info.
            return new FowVisibilityInfo
            {
                IsoTilesToUpdate = isoTilesToUpdate,
                TerrainObjectsToUpdate = terrainObjectsToUpdate,
                QuadTilesToUpdate = new List<IQuadTile>(),
                EntitySnapshotsToUpdate = new List<EntitySnapshot>(),
                MapObjectsToUpdate = mapObjectsToUpdate
            };
        }

        /// <summary>
        /// Adds the given isometric tile and all of its cutting quadratic tiles into the update lists.
        /// </summary>
        /// <param name="isoTile">The isometric tile to add.</param>
        /// <param name="isoTileUpdateList">The isometric update list.</param>
        /// <param name="quadTileUpdateList">The quadratic update list.</param>
        private void AddIsoTileToUpdate(IIsoTile isoTile, RCSet<IIsoTile> isoTileUpdateList, RCSet<IQuadTile> quadTileUpdateList)
        {
            if (isoTile != null && isoTileUpdateList.Add(isoTile))
            {
                foreach (IQuadTile cuttingQuadTile in isoTile.CuttingQuadTiles)
                {
                    if (cuttingQuadTile != null &&
                        (this.fowCacheMatrix.GetFullFowFlagsAtQuadTile(cuttingQuadTile.MapCoords) != FOWTileFlagsEnum.None ||
                         this.fowCacheMatrix.GetPartialFowFlagsAtQuadTile(cuttingQuadTile.MapCoords) != FOWTileFlagsEnum.None))
                    {
                        quadTileUpdateList.Add(cuttingQuadTile);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given terrain object and all of its cutting quadratic tiles into the update lists.
        /// </summary>
        /// <param name="terrainObj">The terrain object to add.</param>
        /// <param name="terrainObjUpdateList">The terrain object update list.</param>
        /// <param name="quadTileUpdateList">The quadratic update list.</param>
        private void AddTerrainObjectToUpdate(ITerrainObject terrainObj, RCSet<ITerrainObject> terrainObjUpdateList, RCSet<IQuadTile> quadTileUpdateList)
        {
            if (terrainObj != null && terrainObjUpdateList.Add(terrainObj))
            {
                for (int col = 0; col < terrainObj.Type.QuadraticSize.X; col++)
                {
                    for (int row = 0; row < terrainObj.Type.QuadraticSize.Y; row++)
                    {
                        IQuadTile quadTileToUpdate = terrainObj.GetQuadTile(new RCIntVector(col, row));
                        if (quadTileToUpdate != null &&
                            (this.fowCacheMatrix.GetFullFowFlagsAtQuadTile(quadTileToUpdate.MapCoords) != FOWTileFlagsEnum.None ||
                             this.fowCacheMatrix.GetPartialFowFlagsAtQuadTile(quadTileToUpdate.MapCoords) != FOWTileFlagsEnum.None))
                        {
                            quadTileUpdateList.Add(quadTileToUpdate);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given entity snapshot and all of its cutting quadratic tiles into the update lists.
        /// </summary>
        /// <param name="snapshot">The entity snapshot to add.</param>
        /// <param name="snapshotUpdateList">The snapshot update list.</param>
        /// <param name="quadTileUpdateList">The quadratic update list.</param>
        private void AddEntitySnapshotToUpdate(EntitySnapshot snapshot, RCSet<EntitySnapshot> snapshotUpdateList, RCSet<IQuadTile> quadTileUpdateList)
        {
            if (snapshot != null && snapshotUpdateList.Add(snapshot))
            {
                for (int col = snapshot.QuadraticPosition.Left; col < snapshot.QuadraticPosition.Right; col++)
                {
                    for (int row = snapshot.QuadraticPosition.Top; row < snapshot.QuadraticPosition.Bottom; row++)
                    {
                        IQuadTile quadTileToUpdate = this.ActiveScenario.Map.GetQuadTile(new RCIntVector(col, row));
                        if (quadTileToUpdate != null &&
                            (this.fowCacheMatrix.GetFullFowFlagsAtQuadTile(quadTileToUpdate.MapCoords) != FOWTileFlagsEnum.None ||
                             this.fowCacheMatrix.GetPartialFowFlagsAtQuadTile(quadTileToUpdate.MapCoords) != FOWTileFlagsEnum.None))
                        {
                            quadTileUpdateList.Add(quadTileToUpdate);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given map object and all of its cutting quadratic tiles into the update lists.
        /// </summary>
        /// <param name="mapObj">The map object to add.</param>
        /// <param name="mapObjectUpdateList">The map object update list.</param>
        /// <param name="quadTileUpdateList">The quadratic update list.</param>
        private void AddMapObjectToUpdate(MapObject mapObj, RCSet<MapObject> mapObjectUpdateList, RCSet<IQuadTile> quadTileUpdateList)
        {
            if (mapObj != null && mapObjectUpdateList.Add(mapObj))
            {
                for (int col = mapObj.QuadraticPosition.Left; col < mapObj.QuadraticPosition.Right; col++)
                {
                    for (int row = mapObj.QuadraticPosition.Top; row < mapObj.QuadraticPosition.Bottom; row++)
                    {
                        IQuadTile quadTileToUpdate = this.ActiveScenario.Map.GetQuadTile(new RCIntVector(col, row));
                        if (quadTileToUpdate != null &&
                            (this.fowCacheMatrix.GetFullFowFlagsAtQuadTile(quadTileToUpdate.MapCoords) != FOWTileFlagsEnum.None ||
                             this.fowCacheMatrix.GetPartialFowFlagsAtQuadTile(quadTileToUpdate.MapCoords) != FOWTileFlagsEnum.None))
                        {
                            quadTileUpdateList.Add(quadTileToUpdate);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the visibility window and invalidates the cache if necessary.
        /// </summary>
        private void UpdateQuadTileWindow()
        {
            if (!this.ActiveScenario.Map.IsFinalized || this.quadTileWindow != this.mapWindowBC.AttachedWindow.QuadTileWindow)
            {
                this.quadTileWindow = this.mapWindowBC.AttachedWindow.QuadTileWindow;
                this.cache.Invalidate();
            }
        }

        /// <summary>
        /// Resets the state of this business component.
        /// </summary>
        private void Reset()
        {
            this.cache = new CachedValue<FowVisibilityInfo>(this.CalculateVisibilityWithoutFow);
            this.quadTileWindow = RCIntRectangle.Undefined;
            this.runningFows = new FogOfWar[Player.MAX_PLAYERS];
            this.runningFowsCount = 0;
            this.fowCacheMatrix = null;
            this.currentIterationIndex = 0;
            this.fowIndex = 0;
        }

        #endregion Internal methods

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;

        /// <summary>
        /// List of the running Fog Of Wars indexed by the players they belong to. If a given player has no running
        /// Fog Of War then the corresponding item is null in this array.
        /// </summary>
        private FogOfWar[] runningFows;

        /// <summary>
        /// The number of running Fog Of Wars.
        /// </summary>
        private int runningFowsCount;

        /// <summary>
        /// Reference to the cache matrix that is used to lazy calculation and caching the Fog Of War state and FOW-flags for the quadratic tiles.
        /// </summary>
        private FowCacheMatrix fowCacheMatrix;

        /// <summary>
        /// The index of the current iteration of the current update.
        /// </summary>
        private int currentIterationIndex;

        /// <summary>
        /// The index of the FogOfWar which to start processing in the next iteration.
        /// </summary>
        private int fowIndex;

        /// <summary>
        /// Cache for storing the results of visibility calculations.
        /// </summary>
        private CachedValue<FowVisibilityInfo> cache;

        /// <summary>
        /// The rectangular area of the map in quadratic tile coordinates for which to calculate the visibility informations.
        /// </summary>
        private RCIntRectangle quadTileWindow;

        /// <summary>
        /// The minimum number of iterations per FOW updates.
        /// </summary>
        private const int MIN_ITERATIONS_PER_UPDATE = 24;

        /// <summary>
        /// The maximum number of entities processed per FOW update-iterations.
        /// </summary>
        private const int MAX_ENTITIES_PER_ITERATION = 50;
    }
}
