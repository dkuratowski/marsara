using System;
using System.Collections.Generic;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Stores the Fog Of War informations for a specific player.
    /// </summary>
    class FogOfWar
    {
        /// <summary>
        /// Constructs a FogOfWar instance for the given player.
        /// </summary>
        /// <param name="owner">The owner of this FogOfWar instance.</param>
        public FogOfWar(Player owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = owner;
            this.processedEntities = new RCSet<int>();
            this.monitoredEntities = new RCSet<int>();
            this.fowExpirationTimes = new int[owner.Scenario.Map.Size.X, owner.Scenario.Map.Size.Y];
            this.entitySnapshots = new EntitySnapshot[owner.Scenario.Map.Size.X, owner.Scenario.Map.Size.Y];
            for (int col = 0; col < this.owner.Scenario.Map.Size.X; col++)
            {
                for (int row = 0; row < this.owner.Scenario.Map.Size.Y; row++)
                {
                    this.fowExpirationTimes[col, row] = -1;
                    this.entitySnapshots[col, row] = null;
                }
            }
        }

        /// <summary>
        /// Gets the actual Fog Of War at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The coordinates of the quadratic tile.</param>
        /// <returns>The type of the Fog Of War at the given quadratic tile.</returns>
        public FOWTypeEnum GetFogOfWar(RCIntVector quadCoords)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (quadCoords.X < 0 || quadCoords.X >= this.owner.Scenario.Map.Size.X ||
                quadCoords.Y < 0 || quadCoords.Y >= this.owner.Scenario.Map.Size.Y)
            {
                throw new ArgumentOutOfRangeException("quadCoords");
            }

            if (this.fowExpirationTimes[quadCoords.X, quadCoords.Y] == -1) { return FOWTypeEnum.Full; }
            return this.owner.Scenario.CurrentFrameIndex > this.fowExpirationTimes[quadCoords.X, quadCoords.Y] ?
                   FOWTypeEnum.Partial :
                   FOWTypeEnum.None;
        }

        /// <summary>
        /// Gets the expiration time of the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The coordinates of the quadratic tile.</param>
        /// <returns>The expiration time of the given quadratic tile.</returns>
        public int GetExpirationTime(RCIntVector quadCoords)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (quadCoords.X < 0 || quadCoords.X >= this.owner.Scenario.Map.Size.X ||
                quadCoords.Y < 0 || quadCoords.Y >= this.owner.Scenario.Map.Size.Y)
            {
                throw new ArgumentOutOfRangeException("quadCoords");
            }

            return this.fowExpirationTimes[quadCoords.X, quadCoords.Y];
        }

        /// <summary>
        /// Gets the entity snapshot at the given quadratic tile or null if there is no entity snapshot at the given tile.
        /// </summary>
        /// <param name="quadCoords">The coordinates of the quadratic tile.</param>
        /// <returns>The entity snapshot at the given quadratic tile or null if there is no entity snapshot at the given tile.</returns>
        public EntitySnapshot GetEntitySnapshot(RCIntVector quadCoords)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (quadCoords.X < 0 || quadCoords.X >= this.owner.Scenario.Map.Size.X ||
                quadCoords.Y < 0 || quadCoords.Y >= this.owner.Scenario.Map.Size.Y)
            {
                throw new ArgumentOutOfRangeException("quadCoords");
            }

            return this.entitySnapshots[quadCoords.X, quadCoords.Y];
        }

        /// <summary>
        /// Restarts to update the Fog Of War informations based on the current positions of the owner's entities.
        /// </summary>
        /// <param name="maxEntities">The maximum number of entities to be processed.</param>
        /// <returns>The actual number of processed entities.</returns>
        /// <remarks>
        /// Use FogOfWar.ContinueUpdate to continue processing the remaining entities in a later point in time.
        /// </remarks>
        public int RestartUpdate(int maxEntities)
        {
            this.processedEntities.Clear();
            return this.ContinueUpdate(maxEntities);
        }

        /// <summary>
        /// Continues to update the Fog Of War informations based on the current positions of the owner's entities.
        /// </summary>
        /// <param name="maxEntities">The maximum number of entities to be processed.</param>
        /// <returns>The actual number of processed entities.</returns>
        /// <remarks>
        /// The already processed entities won't be processed again. Use FogOfWar.RestartUpdate to restart the update procedure.
        /// </remarks>
        public int ContinueUpdate(int maxEntities)
        {
            if (maxEntities < 0) { throw new ArgumentOutOfRangeException("maxEntities", "maxEntities shall be non-negative!"); }

            /// Update every quadratic tiles that are visible by the friendly entities.
            int processedEntitiesCount = 0;
            using (IEnumerator<Entity> entitiesIterator = this.owner.Entities.GetEnumerator())
            {
                while (processedEntitiesCount < maxEntities && entitiesIterator.MoveNext())
                {
                    Entity entity = entitiesIterator.Current;
                    if (!this.processedEntities.Contains(entity.ID.Read()))
                    {
                        foreach (RCIntVector visibleQuadCoord in entity.Locator.VisibleQuadCoords)
                        {
                            /// Update the expiration time of the quadratic tile.
                            this.fowExpirationTimes[visibleQuadCoord.X, visibleQuadCoord.Y] =
                                this.owner.Scenario.CurrentFrameIndex + PARTIAL_FOW_EXPIRATION_TIME;

                            /// If there was a snapshot at the given quadratic tile -> remove it.
                            this.RemoveEntitySnapshot(visibleQuadCoord);

                            /// If there is a non-friendly Entity fixed on the given quadratic tile -> start monitoring.
                            Entity entityAtQuadTile = this.owner.Scenario.GetFixedEntity<Entity>(visibleQuadCoord);
                            if (entityAtQuadTile != null && entityAtQuadTile.Owner != this.owner)
                            {
                                this.monitoredEntities.Add(entityAtQuadTile.ID.Read());
                            }
                        }
                        this.processedEntities.Add(entity.ID.Read());
                        processedEntitiesCount++;
                    }
                }
            }

            /// Check the monitored entities if we still need to monitor them.
            List<int> monitoredEntitiesCopy = new List<int>(this.monitoredEntities);
            foreach (int monitoredEntityId in monitoredEntitiesCopy)
            {
                Entity monitoredEntity = this.owner.Scenario.GetElementOnMap<Entity>(monitoredEntityId, MapObjectLayerEnum.GroundObjects);
                if (monitoredEntity != null && monitoredEntity.Owner != this.owner && monitoredEntity.MotionControl.Status == MotionControlStatusEnum.Fixed)
                {
                    /// Check if the monitored entity is still visible.
                    bool isStillVisible = false;
                    for (int col = monitoredEntity.MapObject.QuadraticPosition.Left; !isStillVisible && col < monitoredEntity.MapObject.QuadraticPosition.Right; col++)
                    {
                        for (int row = monitoredEntity.MapObject.QuadraticPosition.Top; row < monitoredEntity.MapObject.QuadraticPosition.Bottom; row++)
                        {
                            if (this.GetFogOfWar(new RCIntVector(col, row)) == FOWTypeEnum.None)
                            {
                                /// Found at least 1 quadratic tile where the monitored entity is still visible.
                                isStillVisible = true;
                                break;
                            }
                        }
                    }

                    if (!isStillVisible)
                    {
                        /// Take a snapshot...
                        EntitySnapshot snapshot = new EntitySnapshot(monitoredEntity);
                        for (int col = snapshot.QuadraticPosition.Left; col < snapshot.QuadraticPosition.Right; col++)
                        {
                            for (int row = snapshot.QuadraticPosition.Top; row < snapshot.QuadraticPosition.Bottom; row++)
                            {
                                this.entitySnapshots[col, row] = snapshot;
                            }
                        }

                        /// ...and stop monitoring.
                        this.monitoredEntities.Remove(monitoredEntityId);
                    }
                }
                else
                {
                    /// The monitored entity is not bound to the quadratic grid or became friendly -> stop monitoring.
                    this.monitoredEntities.Remove(monitoredEntityId);
                }
            }

            return processedEntitiesCount;
        }

        /// <summary>
        /// Removes the entity snapshot refered from the given quadratic coordinates.
        /// </summary>
        /// <param name="quadCoord">The quadratic coordinates of the entity snapshot to remove.</param>
        private void RemoveEntitySnapshot(RCIntVector quadCoord)
        {
            EntitySnapshot snapshotToRemove = this.entitySnapshots[quadCoord.X, quadCoord.Y];
            if (snapshotToRemove == null) { return; }

            for (int col = snapshotToRemove.QuadraticPosition.Left; col < snapshotToRemove.QuadraticPosition.Right; col++)
            {
                for (int row = snapshotToRemove.QuadraticPosition.Top; row < snapshotToRemove.QuadraticPosition.Bottom; row++)
                {
                    this.entitySnapshots[col, row] = null;
                }
            }
        }

        /// <summary>
        /// An array of integers that stores the Fog Of War expiration times for each quadratic tiles. A value of -1 means that the
        /// corresponding quadratic tile is hidden with full Fog Of War. A non-negative value means the index of the simulation frame
        /// in which the Fog Of War for the corresponding quadratic tile will expire. If the value is less than or equals with the
        /// current frame index of the currently active scenario then the corresponding quadratic tile is hidden with partial Fog Of
        /// War. Otherwise no Fog Of War shall be displayed for the quadratic tile.
        /// </summary>
        private readonly int[,] fowExpirationTimes;

        /// <summary>
        /// An array of references that refer to the entity snapshots at the corresponding quadratic tiles.
        /// </summary>
        private readonly EntitySnapshot[,] entitySnapshots;

        /// <summary>
        /// The IDs of the already processed entities.
        /// </summary>
        private readonly RCSet<int> processedEntities;

        /// <summary>
        /// The IDs of the currently quadratic entities being monitored.
        /// </summary>
        private readonly RCSet<int> monitoredEntities;

        /// <summary>
        /// Reference to the owner of this FogOfWar instance.
        /// </summary>
        private readonly Player owner;

        /// <summary>
        /// The number of frames after a partial Fog Of War expires for a quadratic tile.
        /// </summary>
        private const int PARTIAL_FOW_EXPIRATION_TIME = 48;
    }
}