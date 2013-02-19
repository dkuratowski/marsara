using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of a map being edited.
    /// </summary>
    public interface IMapEdit : IMap
    {
        /// <summary>
        /// Draws the given terrain at the given isometric tile.
        /// </summary>
        /// <param name="tile">The tile to draw.</param>
        /// <param name="terrain">The terrain to draw.</param>
        IEnumerable<IIsoTile> DrawTerrain(IIsoTile tile, TerrainType terrain); // TODO: make this method void.

        /// <summary>
        /// Saves the map into the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to save to.</param>
        /// <remarks>If the file already exists, it will be overwritten.</remarks>
        void Save(string fileName);

        /// <summary>
        /// Gets the terrain object editor of the map.
        /// </summary>
        ITerrainObjectEdit TerrainObjectEditor { get; }

        /// TODO: only for debugging!
        IEnumerable<IIsoTile> IsometricTiles { get; }
    }
}
