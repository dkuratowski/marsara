using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Enumerates the possible layers of map objects.
    /// </summary>
    public enum MapObjectLayerEnum
    {
        /// <summary>
        /// This layer is used by entities to indicate their actual position on the ground.
        /// </summary>
        GroundObjects = 0,

        /// <summary>
        /// This layer is used by entities to reserve a position on the ground for landing.
        /// </summary>
        GroundReservations = 1,

        /// <summary>
        /// This layer is used by missiles attacking entities on the ground.
        /// </summary>
        GroundMissiles = 2,

        /// <summary>
        /// This layer is used by entities to indicate their actual position in the air.
        /// </summary>
        AirObjects = 3,

        /// <summary>
        /// This layer is used by entities to reserve a position in the air for takeoff.
        /// </summary>
        AirReservations = 4,

        /// <summary>
        /// This layer is used by missiles attacking entities in the air.
        /// </summary>
        AirMissiles = 5
    }
}
