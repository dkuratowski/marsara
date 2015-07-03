using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Stores a missile definition for a weapon.
    /// </summary>
    class MissileData : IMissileData
    {
        /// <summary>
        /// Constructs a MissileData instance.
        /// </summary>
        /// <param name="missileTypeName">The name of the corresponding missile type.</param>
        /// <param name="metadata"></param>
        public MissileData(string missileTypeName, ScenarioMetadata metadata)
        {
            if (missileTypeName == null) { throw new ArgumentNullException("missileTypeName"); }
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            this.missileType = null;
            this.missileTypeName = missileTypeName;
            this.metadata = metadata;
            this.relativeLaunchPositions = new Dictionary<MapDirection, RCNumVector>();
        }

        #region IMissileData members

        /// <see cref="IMissileData.MissileType"/>
        public IMissileType MissileType { get { return this.missileType; } }

        /// <see cref="IMissileData.GetRelativeLaunchPosition"/>
        public RCNumVector GetRelativeLaunchPosition(MapDirection direction)
        {
            if (!this.relativeLaunchPositions.ContainsKey(direction)) { throw new SimulatorException(string.Format("Relative launch position for map direction '{0}' is not defined!", direction)); }
            return this.relativeLaunchPositions[direction];
        }

        #endregion IMissileData members

        #region MissileData buildup methods

        /// <summary>
        /// Adds a relative launch position to this missile definition for a given map direction.
        /// </summary>
        /// <param name="direction">The map direction to which the relative launch position shall be assigned.</param>
        /// <param name="launchPos">The relative lauch position to assign.</param>
        public void AddRelativeLaunchPosition(MapDirection direction, RCNumVector launchPos)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (launchPos == RCNumVector.Undefined) { throw new ArgumentNullException("launchPos"); }
            if (this.relativeLaunchPositions.ContainsKey(direction)) { throw new SimulatorException(string.Format("Relative launch position for map direction '{0}' already defined!", direction)); }

            this.relativeLaunchPositions[direction] = launchPos;
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {
                if (!this.metadata.HasMissileType(this.missileTypeName)) { throw new SimulatorException(string.Format("MissileType '{0}' doesn't exist!", this.missileTypeName)); }
                this.missileType = this.metadata.GetMissileTypeImpl(this.missileTypeName);
                if (this.relativeLaunchPositions.Count == 0) { throw new SimulatorException("A missile data must have at least 1 relative launch position!"); }
            }
        }

        #endregion MissileData buildup methods

        /// <summary>
        /// Reference to the missile type that this missile definition belongs to.
        /// </summary>
        private MissileType missileType;

        /// <summary>
        /// The name of the missile type that this missile defition belongs to.
        /// </summary>
        private readonly string missileTypeName;

        /// <summary>
        /// The relative launch positions mapped by their corresponding map directions.
        /// </summary>
        private readonly Dictionary<MapDirection, RCNumVector> relativeLaunchPositions;

        /// <summary>
        /// Reference to the metadata object that this missile definition belongs to.
        /// </summary>
        private readonly ScenarioMetadata metadata;
    }
}
