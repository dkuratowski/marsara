using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Responsible for locating other entities in the sight range of a given entity.
    /// </summary>
    public class Locator : HeapedObject
    {
        /// <summary>
        /// Constructs a locator instance for the given entity.
        /// </summary>
        /// <param name="owner">The owner of this locator.</param>
        public Locator(Entity owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = this.ConstructField<Entity>("owner");
            this.owner.Write(owner);
            this.sightRangeCache = null;
        }

        /// <summary>
        /// Locates the entities that are in the sight-range of the owner entity.
        /// </summary>
        /// <returns>The entities that are in the sight-range of the owner entity.</returns>
        public RCSet<Entity> LocateEntities()
        {
            /// Collect the entities in sight-range.
            RCSet<Entity> retList = new RCSet<Entity>();
            RCSet<Entity> entitiesToCheck = this.owner.Read().Scenario.GetElementsOnMap<Entity>(this.owner.Read().MotionControl.PositionVector.Read(), this.GetSightRangeOfOwner(), MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects);
            entitiesToCheck.Remove(this.owner.Read());
            RCSet<RCIntVector> visibleQuadCoords = this.VisibleQuadCoords;
            foreach (Entity entity in entitiesToCheck)
            {
                bool breakFlag = false;
                for (int row = entity.MapObject.QuadraticPosition.Top; !breakFlag && row < entity.MapObject.QuadraticPosition.Bottom; row++)
                {
                    for (int col = entity.MapObject.QuadraticPosition.Left; !breakFlag && col < entity.MapObject.QuadraticPosition.Right; col++)
                    {
                        if (visibleQuadCoords.Contains(new RCIntVector(col, row)))
                        {
                            retList.Add(entity);
                            breakFlag = true;
                        }
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// Checks whether the given position is in the sight-range of the owner entity.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the given position is in the sight-range of the owner entity; otherwise false.</returns>
        public bool LocatePosition(RCNumVector position)
        {
            RCIntVector quadCoordAtPosition = this.owner.Read().Scenario.Map.GetCell(position.Round()).ParentQuadTile.MapCoords;
            return this.VisibleQuadCoords.Contains(quadCoordAtPosition);
        }

        /// <summary>
        /// Searches the entities that are in a rectangular area around the owner entity with the given radius.
        /// </summary>
        /// <param name="searchAreaRadius">The radius of the search area given in quadratic tiles.</param>
        /// <returns>The entities that are in the search area.</returns>
        public RCSet<Entity> SearchNearbyEntities(int searchAreaRadius)
        {
            RCSet<Entity> nearbyEntities = this.owner.Read().Scenario.GetElementsOnMap<Entity>(this.owner.Read().MotionControl.PositionVector.Read(), searchAreaRadius, MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects);
            nearbyEntities.Remove(this.owner.Read());
            return nearbyEntities;
        }

        /// <summary>
        /// Gets the coordinates of the quadratic tiles that are currently visible by this entity.
        /// </summary>
        /// TODO: sight-range can vary based on the upgrades!
        public RCSet<RCIntVector> VisibleQuadCoords
        {
            get
            {
                IQuadTile currentQuadTile = this.owner.Read().Scenario.Map.GetCell(this.owner.Read().MotionControl.PositionVector.Read().Round()).ParentQuadTile;
                if (this.sightRangeCache == null || this.sightRangeCache.Item1 != currentQuadTile)
                {
                    this.sightRangeCache = new Tuple<IQuadTile, RCSet<RCIntVector>>(currentQuadTile, this.CalculateVisibleQuadCoords());
                }

                return new RCSet<RCIntVector>(this.sightRangeCache.Item2);
            }
        }

        /// <summary>
        /// Calculates the quadratic coordinates currently visible by the owner entity.
        /// </summary>
        /// <returns>The quadratic coordinates currently visible by the owner entity.</returns>
        private RCSet<RCIntVector> CalculateVisibleQuadCoords()
        {
            IQuadTile currentQuadTile = this.owner.Read().Scenario.Map.GetCell(this.owner.Read().MotionControl.PositionVector.Read().Round()).ParentQuadTile;
            RCSet<RCIntVector> retList = new RCSet<RCIntVector>();
            foreach (RCIntVector relativeQuadCoord in this.owner.Read().ElementType.RelativeQuadCoordsInSight)
            {
                RCIntVector otherQuadCoords = currentQuadTile.MapCoords + relativeQuadCoord;
                if (otherQuadCoords.X >= 0 && otherQuadCoords.X < this.owner.Read().Scenario.Map.Size.X &&
                    otherQuadCoords.Y >= 0 && otherQuadCoords.Y < this.owner.Read().Scenario.Map.Size.Y)
                {
                    IQuadTile otherQuadTile = this.owner.Read().Scenario.Map.GetQuadTile(otherQuadCoords);
                    if (this.owner.Read().MotionControl.IsFlying || currentQuadTile.GroundLevel >= otherQuadTile.GroundLevel)
                    {
                        retList.Add(otherQuadTile.MapCoords);
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// Gets the actual sight range of the owner entity in quadratic tiles.
        /// </summary>
        /// <returns>The actual sight range of the owner entity.</returns>
        private int GetSightRangeOfOwner()
        {
            /// TODO: sight-range can vary based on the upgrades!
            return this.owner.Read().ElementType.SightRange.Read();
        }

        /// <summary>
        /// Reference to the owner of this locator.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// Data structure to store the calculated sight range of the owner entity for the last known quadratic tile.
        /// </summary>
        private Tuple<IQuadTile, RCSet<RCIntVector>> sightRangeCache;
    }
}
