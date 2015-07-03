using System;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Responsible for lazy calculation and caching the Fog Of War state and FOW-flags for the quadratic tiles.
    /// </summary>
    class FowCacheMatrix
    {
        /// <summary>
        /// Constructs a FowCacheMatrix instance.
        /// </summary>
        /// <param name="runningFows">
        /// List of the running Fog Of Wars indexed by the players they belong to. If a given player has no running Fog Of War then
        /// the corresponding item is null in this array.
        /// </param>
        /// <param name="scenario">Reference to the currently active scenario.</param>
        public FowCacheMatrix(FogOfWar[] runningFows, Scenario scenario)
        {
            if (runningFows == null) { throw new ArgumentNullException("runningFows"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }

            this.runningFows = runningFows;
            this.scenario = scenario;
            this.fowCacheMatrix = new Item[this.scenario.Map.Size.X, this.scenario.Map.Size.Y];
            for (int col = 0; col < this.scenario.Map.Size.X; col++)
            {
                for (int row = 0; row < this.scenario.Map.Size.Y; row++)
                {
                    this.fowCacheMatrix[col, row] = new Item
                    {
                        LastFowStateUpdate = -1,
                        LastFowFlagsUpdate = -1,
                        FowState = FOWTypeEnum.None,
                        FullFowFlags = FOWTileFlagsEnum.None,
                        PartialFowFlags = FOWTileFlagsEnum.None
                    };
                }
            }
        }

        /// <summary>
        /// Gets the current FOW-state at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The current FOW-state at the given quadratic tile.</returns>
        public FOWTypeEnum GetFowStateAtQuadTile(RCIntVector quadCoords)
        {
            /// Consider FOWTypeEnum.None at coordinates outside of the map.
            if (quadCoords.X < 0 || quadCoords.X >= this.scenario.Map.Size.X || quadCoords.Y < 0 || quadCoords.Y >= this.scenario.Map.Size.Y) { return FOWTypeEnum.None; }

            /// If the FOW-state of the given tile has not yet been cached or is invalid, then calculate and save it to the cache.
            if (this.scenario.CurrentFrameIndex != this.fowCacheMatrix[quadCoords.X, quadCoords.Y].LastFowStateUpdate)
            {
                bool isFowRunning = false;
                FOWTypeEnum minimumFow = FOWTypeEnum.Full;
                for (int idx = 0; idx < Player.MAX_PLAYERS; idx++)
                {
                    if (this.runningFows[idx] != null)
                    {
                        FOWTypeEnum fowAtQuadTile = this.runningFows[idx].GetFogOfWar(quadCoords);
                        if (fowAtQuadTile < minimumFow) { minimumFow = fowAtQuadTile; }
                        isFowRunning = true;
                    }
                }

                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].LastFowStateUpdate = this.scenario.CurrentFrameIndex;
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].FowState = isFowRunning ? minimumFow : FOWTypeEnum.None;
            }

            /// Return with the cached value.
            return this.fowCacheMatrix[quadCoords.X, quadCoords.Y].FowState;
        }

        /// <summary>
        /// Gets the full FOW-flags at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The full FOW-flags at the given quadratic tile.</returns>
        public FOWTileFlagsEnum GetFullFowFlagsAtQuadTile(RCIntVector quadCoords)
        {
            /// If the FOW-flags of the given tile has not yet been cached or is invalid, then calculate and save it to the cache.
            if (this.scenario.CurrentFrameIndex != this.fowCacheMatrix[quadCoords.X, quadCoords.Y].LastFowFlagsUpdate)
            {
                this.CalculateFowFlags(quadCoords);
            }

            /// Return with the cached value.
            return this.fowCacheMatrix[quadCoords.X, quadCoords.Y].FullFowFlags;
        }

        /// <summary>
        /// Gets the partial FOW-flags at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The partial FOW-flags at the given quadratic tile.</returns>
        public FOWTileFlagsEnum GetPartialFowFlagsAtQuadTile(RCIntVector quadCoords)
        {
            /// If the FOW-flags of the given tile has not yet been cached or is invalid, then calculate and save it to the cache.
            if (this.scenario.CurrentFrameIndex != this.fowCacheMatrix[quadCoords.X, quadCoords.Y].LastFowFlagsUpdate)
            {
                this.CalculateFowFlags(quadCoords);
            }

            /// Return with the cached value.
            return this.fowCacheMatrix[quadCoords.X, quadCoords.Y].PartialFowFlags;
        }

        /// <summary>
        /// Gets the entity snapshot at the given quadratic tile or null if there is no entity snapshot at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The entity snapshot at the given quadratic tile or null if there is no entity snapshot at the given quadratic tile.</returns>
        public EntitySnapshot GetEntitySnapshotAtQuadTile(RCIntVector quadCoords)
        {
            /// If the entity snapshots at the given tile has not yet been cached or is invalid, then calculate and save it to the cache.
            if (this.scenario.CurrentFrameIndex != this.fowCacheMatrix[quadCoords.X, quadCoords.Y].LastEntitySnapshotUpdate)
            {
                this.CalculateEntitySnapshot(quadCoords);
            }

            /// Return with the cached value.
            return this.fowCacheMatrix[quadCoords.X, quadCoords.Y].EntitySnapshot;
        }

        /// <summary>
        /// Represents an item in the FOW-cache matrix.
        /// </summary>
        private struct Item
        {
            /// <summary>
            /// The frame index when the FOW-state of this item has been updated last time.
            /// </summary>
            public int LastFowStateUpdate;

            /// <summary>
            /// The frame index when the FOW-flags of this item has been updated last time.
            /// </summary>
            public int LastFowFlagsUpdate;

            /// <summary>
            /// The frame index when the entity snapshot of this item has been updated last time.
            /// </summary>
            public int LastEntitySnapshotUpdate;

            /// <summary>
            /// The last known FOW-state of this item.
            /// </summary>
            public FOWTypeEnum FowState;

            /// <summary>
            /// The last known full FOW-flags of this item.
            /// </summary>
            public FOWTileFlagsEnum FullFowFlags;

            /// <summary>
            /// The last known partial FOW-flags of this item.
            /// </summary>
            public FOWTileFlagsEnum PartialFowFlags;

            /// <summary>
            /// The last known entity snapshot of this item.
            /// </summary>
            public EntitySnapshot EntitySnapshot;
        }

        /// <summary>
        /// Calculates the FOW-flags for the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        private void CalculateFowFlags(RCIntVector quadCoords)
        {
            FOWTypeEnum fowAtQuadCoords = this.GetFowStateAtQuadTile(quadCoords);
            if (fowAtQuadCoords == FOWTypeEnum.Full)
            {
                /// Full FOW-state -> draw only full FOW.
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].FullFowFlags = FOWTileFlagsEnum.Current;
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].PartialFowFlags = FOWTileFlagsEnum.None;
            }
            else if (fowAtQuadCoords == FOWTypeEnum.Partial)
            {
                /// Partial FOW-state -> draw partial FOW and calculate full FOW-flags based on the neighbours.
                FOWTileFlagsEnum fullFowFlags = FOWTileFlagsEnum.None;
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(0, -1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.North; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, -1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.NorthEast; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, 0)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.East; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, 1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.SouthEast; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(0, 1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.South; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, 1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.SouthWest; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, 0)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.West; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, -1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.NorthWest; }
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].FullFowFlags = fullFowFlags;
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].PartialFowFlags = FOWTileFlagsEnum.Current;
            }
            else if (fowAtQuadCoords == FOWTypeEnum.None)
            {
                /// No FOW -> calculate full & partial FOW-flags based on the neighbours.
                FOWTileFlagsEnum fullFowFlags = FOWTileFlagsEnum.None;
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(0, -1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.North; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, -1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.NorthEast; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, 0)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.East; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, 1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.SouthEast; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(0, 1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.South; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, 1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.SouthWest; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, 0)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.West; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, -1)) == FOWTypeEnum.Full) { fullFowFlags |= FOWTileFlagsEnum.NorthWest; }
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].FullFowFlags = fullFowFlags;

                FOWTileFlagsEnum partialFowFlags = FOWTileFlagsEnum.None;
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(0, -1)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.North; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, -1)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.NorthEast; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, 0)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.East; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(1, 1)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.SouthEast; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(0, 1)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.South; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, 1)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.SouthWest; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, 0)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.West; }
                if (this.GetFowStateAtQuadTile(quadCoords + new RCIntVector(-1, -1)) != FOWTypeEnum.None) { partialFowFlags |= FOWTileFlagsEnum.NorthWest; }
                this.fowCacheMatrix[quadCoords.X, quadCoords.Y].PartialFowFlags = partialFowFlags;
            }
        }
                
        /// <summary>
        /// Calculates the entity snapshot for the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        private void CalculateEntitySnapshot(RCIntVector quadCoords)
        {
            bool isFowRunning = false;
            EntitySnapshot entitySnapshot = null;
            int greatestExpirationTime = -1;
            for (int idx = 0; idx < Player.MAX_PLAYERS; idx++)
            {
                if (this.runningFows[idx] != null)
                {
                    int expirationTime = this.runningFows[idx].GetExpirationTime(quadCoords);
                    if (expirationTime > greatestExpirationTime)
                    {
                        entitySnapshot = this.runningFows[idx].GetEntitySnapshot(quadCoords);
                        greatestExpirationTime = expirationTime;
                    }
                    isFowRunning = true;
                }
            }

            this.fowCacheMatrix[quadCoords.X, quadCoords.Y].LastEntitySnapshotUpdate = this.scenario.CurrentFrameIndex;
            this.fowCacheMatrix[quadCoords.X, quadCoords.Y].EntitySnapshot = isFowRunning ? entitySnapshot : null;
        }

        /// <summary>
        /// List of the running Fog Of Wars indexed by the players they belong to. If a given player has no running Fog Of War then
        /// the corresponding item is null in this array.
        /// </summary>
        private FogOfWar[] runningFows;

        /// <summary>
        /// Reference to the currently active scenario.
        /// </summary>
        private Scenario scenario;

        /// <summary>
        /// The matrix that stores the calculated FOW informations.
        /// </summary>
        private Item[,] fowCacheMatrix;
    }
}
