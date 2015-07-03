using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of a missile definition for a weapon.
    /// </summary>
    public interface IMissileData
    {
        /// <summary>
        /// Gets the missile type that this missile definition belongs to.
        /// </summary>
        IMissileType MissileType { get; }

        /// <summary>
        /// Gets the launch position of the corresponding missile relative to the launching entity based on the map direction of the entity.
        /// </summary>
        /// <param name="direction">The map direction of the entity.</param>
        /// <returns>The launch position relative to the launching entity in map coordinates.</returns>
        RCNumVector GetRelativeLaunchPosition(MapDirection direction);
    }
}
