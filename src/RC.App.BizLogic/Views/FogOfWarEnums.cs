using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Enumerates the possible types of the Fog Of War.
    /// </summary>
    public enum FOWTypeEnum
    {
        /// <summary>
        /// No Fog Of War.
        /// </summary>
        None = 0,

        /// <summary>
        /// Partial Fog Of War:
        ///     - Terrain
        ///     - Snapshot of non-friendly buildings from their last known state.
        /// </summary>
        [EnumMapping("Partial")]
        Partial = 1,

        /// <summary>
        /// Full Fog Of War: nothing is visible.
        /// </summary>
        [EnumMapping("Full")]
        Full = 2
    }

    /// <summary>
    /// Flags for indicating whether a given tile and its neighbours are filled with Fog Of War or not.
    /// You can combine these flags to indicate combined FOW-tiles.
    /// </summary>
    [Flags]
    public enum FOWTileFlagsEnum
    {
        None = 0x000,       // Neither the current tile nor its neighbours are filled with Fog Of War.
        Current = 0x001,    // The current tile is filled with Fog Of War.
        North = 0x002,      // The neighbour of the current tile to the north is filled with Fog Of War.
        NorthEast = 0x004,  // The neighbour of the current tile to the north-east is filled with Fog Of War.
        East = 0x008,       // The neighbour of the current tile to the east is filled with Fog Of War.
        SouthEast = 0x010,  // The neighbour of the current tile to the south-east is filled with Fog Of War.
        South = 0x020,      // The neighbour of the current tile to the south is filled with Fog Of War.
        SouthWest = 0x040,  // The neighbour of the current tile to the south-west is filled with Fog Of War.
        West = 0x080,       // The neighbour of the current tile to the west is filled with Fog Of War.
        NorthWest = 0x100,  // The neighbour of the current tile to the north-west is filled with Fog Of War.
        All = Current | North | NorthEast | East | SouthEast | South | SouthWest | West | NorthWest
    }
}
