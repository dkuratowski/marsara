using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Engine.Simulator.Scenarios;

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
            this.processedEntities = new HashSet<int>();
            this.fowExpirationTimes = new int[owner.Scenario.Map.Size.X, owner.Scenario.Map.Size.Y];
            for (int col = 0; col < this.owner.Scenario.Map.Size.X; col++)
            {
                for (int row = 0; row < this.owner.Scenario.Map.Size.Y; row++)
                {
                    this.fowExpirationTimes[col, row] = -1;
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
        /// Restarts to update the Fog Of War expiration times based on the current positions of the owner's entities.
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
        /// Continues to update the Fog Of War expiration times based on the current positions of the owner's entities.
        /// </summary>
        /// <param name="maxEntities">The maximum number of entities to be processed.</param>
        /// <returns>The actual number of processed entities.</returns>
        /// <remarks>
        /// The already processed entities won't be processed again. Use FogOfWar.RestartUpdate to restart the update procedure.
        /// </remarks>
        public int ContinueUpdate(int maxEntities)
        {
            if (maxEntities < 0) { throw new ArgumentOutOfRangeException("maxEntities", "maxEntities shall be non-negative!"); }

            int processedEntitiesCount = 0;
            using (IEnumerator<Entity> entitiesIterator = this.owner.Entities.GetEnumerator())
            {
                while (processedEntitiesCount < maxEntities && entitiesIterator.MoveNext())
                {
                    Entity entity = entitiesIterator.Current;
                    if (!this.processedEntities.Contains(entity.ID.Read()))
                    {
                        foreach (RCIntVector visibleQuadCoord in entity.VisibleQuadCoords)
                        {
                            this.fowExpirationTimes[visibleQuadCoord.X, visibleQuadCoord.Y] =
                                this.owner.Scenario.CurrentFrameIndex + PARTIAL_FOW_EXPIRATION_TIME;
                        }
                        this.processedEntities.Add(entity.ID.Read());
                        processedEntitiesCount++;
                    }
                }
            }
            return processedEntitiesCount;
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
        /// The IDs of the already processed entities.
        /// </summary>
        private readonly HashSet<int> processedEntities;

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