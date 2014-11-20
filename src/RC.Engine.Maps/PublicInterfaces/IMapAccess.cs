using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface for accessing the map.
    /// </summary>
    public interface IMapAccess
    {
        /// <summary>
        /// Gets the name of the map.
        /// </summary>
        string MapName { get; }

        /// <summary>
        /// Gets the size of the map in quadratic tiles.
        /// </summary>
        RCIntVector Size { get; }

        /// <summary>
        /// Gets the size of the map in cells.
        /// </summary>
        RCIntVector CellSize { get; }

        /// <summary>
        /// Gets the tileset of this map.
        /// </summary>
        ITileSet Tileset { get; }

        /// <summary>
        /// Gets whether the terrain of this map has been finalized or not.
        /// </summary>
        bool IsFinalized { get; }

        /// <summary>Gets the quadratic tile at the given coordinates.</summary>
        /// <param name="coords">The coordinates of the quadratic tile to get.</param>
        /// <returns>The quadratic tile at the given coordinates.</returns>
        IQuadTile GetQuadTile(RCIntVector coords);

        /// <summary>
        /// Gets the isometric tile at the given coordinates or null if there is no isometric tile at that coordinates.
        /// </summary>
        /// <param name="coords">The coordinates of the isometric tile to get.</param>
        /// <returns>
        /// The isometric tile at the given coordinates or null if there is no isometric tile at that coordinates.
        /// </returns>
        IIsoTile GetIsoTile(RCIntVector coords);

        /// <summary>Gets the cell of this map at the given coordinates.</summary>
        /// <param name="coords">The coordinates of the cell to get.</param>
        /// <returns>The cell at the given coordinates.</returns>
        ICell GetCell(RCIntVector coords);

        /// <summary>
        /// Converts a rectangle of quadratic tiles to a rectangle of cells.
        /// </summary>
        /// <param name="quadRect">The quadratic rectangle to convert.</param>
        /// <returns>The cell rectangle.</returns>
        RCIntRectangle QuadToCellRect(RCIntRectangle quadRect);

        /// <summary>
        /// Computes the minimum size of a quadratic rectangle that can cover an area on the map with the given size.
        /// </summary>
        /// <param name="cellSize">The size of the area in cells.</param>
        /// <returns>The minimum size of a covering quadratic rectangle.</returns>
        RCIntVector CellToQuadSize(RCNumVector cellSize);

        /// <summary>
        /// Begines a tile exchanging operation.
        /// </summary>
        void BeginExchangingTiles();

        /// <summary>
        /// Indicates that the tile exchanging operation is finished.
        /// </summary>
        IEnumerable<IIsoTile> EndExchangingTiles();

        /// <summary>
        /// Finalizes this map.
        /// </summary>
        void FinalizeMap();

        /// <summary>
        /// Closes this map.
        /// </summary>
        void Close();

        /// TODO: only for debugging!
        IEnumerable<IIsoTile> IsometricTiles { get; }

        /// <summary>
        /// Gets the list of the terrain objects attached to this map.
        /// </summary>
        IEnumerable<ITerrainObject> TerrainObjects { get; }
    }
}
