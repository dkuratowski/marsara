using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the Fog Of War business component.
    /// </summary>
    [Component("RC.App.BizLogic.FogOfWarBC")]
    class FogOfWarBC : IFogOfWarBC, IComponent
    {
        /// <summary>
        /// Constructs a FogOfWarBC instance.
        /// </summary>
        public FogOfWarBC()
        {
            this.cache = new CachedValue<FowVisibilityInfo>(this.CalculateVisibility);
            this.quadTileWindow = RCIntRectangle.Undefined;
            this.runningFows = new FogOfWar[Player.MAX_PLAYERS];
            this.runningFowsCount = 0;
            this.targetScenario = null;
            this.currentIterationIndex = 0;
            this.fowIndex = 0;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region IFogOfWarBC methods

        /// <see cref="IFogOfWarBC.StartFogOfWar"/>
        public void StartFogOfWar(PlayerEnum owner)
        {
            if (owner == PlayerEnum.Neutral) { throw new ArgumentException("Fog Of War owner cannot be PlayerEnum.Neutral!"); }
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.targetScenario != null && this.targetScenario != this.scenarioManager.ActiveScenario) { throw new InvalidOperationException("Fog Of War calculation is running for a different Scenario!"); }
            
            Player ownerPlayer = this.scenarioManager.ActiveScenario.GetPlayer((int)owner);
            if (ownerPlayer == null) { throw new InvalidOperationException(string.Format("Player {0} doesn't exist!", owner)); }

            if (this.runningFows[(int)owner] != null) { throw new InvalidOperationException(string.Format("Fog Of War for player {0} is already running!", owner)); }
            this.runningFows[(int)owner] = new FogOfWar(ownerPlayer);
            this.runningFowsCount++;

            if (this.targetScenario == null) { this.targetScenario = this.scenarioManager.ActiveScenario; }
            this.cache.Invalidate();
        }

        /// <see cref="IFogOfWarBC.StopFogOfWar"/>
        public void StopFogOfWar(PlayerEnum owner)
        {
            if (owner == PlayerEnum.Neutral) { throw new ArgumentException("Fog Of War owner cannot be PlayerEnum.Neutral!"); }
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.targetScenario != this.scenarioManager.ActiveScenario) { throw new InvalidOperationException("Fog Of War calculation is running for a different Scenario!"); }

            if (this.runningFows[(int)owner] == null) { throw new InvalidOperationException(string.Format("Fog Of War for player {0} is not running!", owner)); }
            this.runningFows[(int)owner] = null;
            this.runningFowsCount--;
            if (this.runningFowsCount == 0)
            {
                this.quadTileWindow = RCIntRectangle.Undefined;
                this.targetScenario = null;
                this.currentIterationIndex = 0;
                this.fowIndex = 0;
            }
            this.cache.Invalidate();
        }

        /// <see cref="IFogOfWarBC.GetIsoTilesToUpdate"/>
        public IEnumerable<IIsoTile> GetIsoTilesToUpdate(RCIntRectangle quadTileWindow)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.targetScenario != null && this.targetScenario != this.scenarioManager.ActiveScenario) { throw new InvalidOperationException("Fog Of War calculation is running for a different Scenario!"); }

            this.UpdateQuadTileWindow(quadTileWindow);
            return this.cache.Value.IsoTilesToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetTerrainObjectsToUpdate"/>
        public IEnumerable<ITerrainObject> GetTerrainObjectsToUpdate(RCIntRectangle quadTileWindow)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.targetScenario != null && this.targetScenario != this.scenarioManager.ActiveScenario) { throw new InvalidOperationException("Fog Of War calculation is running for a different Scenario!"); }

            this.UpdateQuadTileWindow(quadTileWindow);
            return this.cache.Value.TerrainObjectsToUpdate;
        }

        /// <see cref="IFogOfWarBC.GetQuadTilesToUpdate"/>
        public IEnumerable<IQuadTile> GetQuadTilesToUpdate(RCIntRectangle quadTileWindow)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.targetScenario != null && this.targetScenario != this.scenarioManager.ActiveScenario) { throw new InvalidOperationException("Fog Of War calculation is running for a different Scenario!"); }

            this.UpdateQuadTileWindow(quadTileWindow);
            return this.cache.Value.QuadTilesToUpdate;
        }

        /// <see cref="IFogOfWarBC.ExecuteUpdateIteration"/>
        public void ExecuteUpdateIteration()
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.targetScenario != null && this.targetScenario != this.scenarioManager.ActiveScenario) { throw new InvalidOperationException("Fog Of War calculation is running for a different Scenario!"); }
            if (this.runningFowsCount == 0) { return; }

            int remainingEntitiesInCurrIteration = MAX_ENTITIES_PER_ITERATION;
            HashSet<FogOfWar> fowsProcessedInCurrIteration = new HashSet<FogOfWar>();
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
        /// Implements visibility calculations.
        /// </summary>
        /// <returns>The results of the visibility calculations.</returns>
        private FowVisibilityInfo CalculateVisibility()
        {
            if (this.quadTileWindow == RCIntRectangle.Undefined) { throw new InvalidOperationException("Visibility window not defined!"); }

            /// Collect the isometric & quadratic tiles that need to be updated.
            IMapAccess map = this.scenarioManager.ActiveScenario.Map;
            HashSet<IIsoTile> isoTilesToUpdate = new HashSet<IIsoTile>();
            HashSet<IQuadTile> quadTilesToUpdate = new HashSet<IQuadTile>();
            for (int column = this.quadTileWindow.Left; column < this.quadTileWindow.Right; column++)
            {
                for (int row = this.quadTileWindow.Top; row < this.quadTileWindow.Bottom; row++)
                {
                    RCIntVector quadCoords = new RCIntVector(column, row);

                    /// If the FOW is full at the current quadratic tile -> continue with the next.
                    FOWTypeEnum fowAtQuadTile = this.GetFowAtQuadTile(quadCoords);
                    if (fowAtQuadTile == FOWTypeEnum.Full) { continue; }

                    /// Add the primary & secondary isometric tiles and all of their cutting quadratic tiles into the update lists.
                    IQuadTile quadTileToUpdate = map.GetQuadTile(quadCoords);
                    this.AddIsoTileToUpdate(quadTileToUpdate.PrimaryIsoTile, isoTilesToUpdate, quadTilesToUpdate);
                    this.AddIsoTileToUpdate(quadTileToUpdate.SecondaryIsoTile, isoTilesToUpdate, quadTilesToUpdate);
                }
            }

            /// Collect the terrain objects that need to be updated.
            HashSet<ITerrainObject> terrainObjectsToUpdate = new HashSet<ITerrainObject>();
            foreach (ITerrainObject terrainObj in map.TerrainObjects.GetContents((RCNumRectangle)map.QuadToCellRect(this.quadTileWindow) - HALF_VECTOR))
            {
                for (int col = 0; col < terrainObj.Type.QuadraticSize.X; col++)
                {
                    for (int row = 0; row < terrainObj.Type.QuadraticSize.Y; row++)
                    {
                        /// If the current quadratic position is excluded from the terrain object -> continue with the next.
                        IQuadTile quadTileToUpdate = terrainObj.GetQuadTile(new RCIntVector(col, row));
                        if (quadTileToUpdate == null) { continue; }

                        /// If the FOW is full at the current quadratic position -> continue with the next.
                        FOWTypeEnum fowAtQuadTile = this.GetFowAtQuadTile(quadTileToUpdate.MapCoords);
                        if (fowAtQuadTile == FOWTypeEnum.Full) { continue; }

                        /// Add the terrain object and all of its cutting quadratic tiles into the update lists.
                        this.AddTerrainObjectToUpdate(terrainObj, terrainObjectsToUpdate, quadTilesToUpdate);
                    }
                }
            }

            /// Create the calculated visibility info.
            return new FowVisibilityInfo
            {
                IsoTilesToUpdate = isoTilesToUpdate,
                TerrainObjectsToUpdate = terrainObjectsToUpdate,
                QuadTilesToUpdate = quadTilesToUpdate
            };
        }

        /// <summary>
        /// Adds the given isometric tile and all of its cutting quadratic tiles into the update lists.
        /// </summary>
        /// <param name="isoTile">The isometric tile to add.</param>
        /// <param name="isoTileUpdateList">The isometric update list.</param>
        /// <param name="quadTileUpdateList">The quadratic update list.</param>
        private void AddIsoTileToUpdate(IIsoTile isoTile, HashSet<IIsoTile> isoTileUpdateList, HashSet<IQuadTile> quadTileUpdateList)
        {
            if (isoTile != null && isoTileUpdateList.Add(isoTile))
            {
                foreach (IQuadTile cuttingQuadTile in isoTile.CuttingQuadTiles) { quadTileUpdateList.Add(cuttingQuadTile); }
            }
        }

        /// <summary>
        /// Adds the given terrain object and all of its cutting quadratic tiles into the update lists.
        /// </summary>
        /// <param name="terrainObj">The terrain object to add.</param>
        /// <param name="terrainObjUpdateList">The terrain object update list.</param>
        /// <param name="quadTileUpdateList">The quadratic update list.</param>
        private void AddTerrainObjectToUpdate(ITerrainObject terrainObj, HashSet<ITerrainObject> terrainObjUpdateList, HashSet<IQuadTile> quadTileUpdateList)
        {
            if (terrainObjUpdateList.Add(terrainObj))
            {
                for (int col = 0; col < terrainObj.Type.QuadraticSize.X; col++)
                {
                    for (int row = 0; row < terrainObj.Type.QuadraticSize.Y; row++)
                    {
                        IQuadTile quadTileToUpdate = terrainObj.GetQuadTile(new RCIntVector(col, row));
                        if (quadTileToUpdate != null) { quadTileUpdateList.Add(quadTileToUpdate); }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the visibility window and invalidates the cache if necessary.
        /// </summary>
        /// <param name="quadTileWindow">The new visibility window.</param>
        private void UpdateQuadTileWindow(RCIntRectangle quadTileWindow)
        {
            if (quadTileWindow == RCIntRectangle.Undefined) { throw new ArgumentNullException("quadTileWindow"); }
            if (quadTileWindow != this.quadTileWindow)
            {
                this.quadTileWindow = quadTileWindow;
                this.cache.Invalidate();
            }
        }

        /// <summary>
        /// Gets the current Fog Of War at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The current Fog Of War at the given quadratic tile.</returns>
        private FOWTypeEnum GetFowAtQuadTile(RCIntVector quadCoords)
        {
            if (this.runningFowsCount == 0) { return FOWTypeEnum.None; }

            FOWTypeEnum minimumFow = FOWTypeEnum.Full;
            for (int idx = 0; idx < Player.MAX_PLAYERS; idx++)
            {
                if (this.runningFows[idx] == null) { continue; }
                FOWTypeEnum fowAtQuadTile = this.runningFows[idx].GetFogOfWar(quadCoords);
                if (fowAtQuadTile < minimumFow) { minimumFow = fowAtQuadTile; }
            }

            return minimumFow;
        }

        #endregion Internal methods

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

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
        /// Reference to the target scenario for which the running Fog Of Wars are being calculated.
        /// </summary>
        private Scenario targetScenario;

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
        /// Represents the vector (0.5;0.5).
        /// </summary>
        private static readonly RCNumVector HALF_VECTOR = new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);

        /// <summary>
        /// The minimum number of iterations per FOW updates.
        /// </summary>
        private const int MIN_ITERATIONS_PER_UPDATE = 40;

        /// <summary>
        /// The maximum number of entities processed per FOW update-iterations.
        /// </summary>
        private const int MAX_ENTITIES_PER_ITERATION = 50;
    }
}
