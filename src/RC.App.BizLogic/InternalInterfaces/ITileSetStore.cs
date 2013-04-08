using System.Collections.Generic;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.InternalInterfaces
{
    /// <summary>
    /// This component interface is used to access informations about the available tilesets.
    /// </summary>
    [ComponentInterface]
    interface ITileSetStore
    {
        /// <summary>
        /// Checks whether a tileset with the given name exists in the system or not.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset.</param>
        /// <returns>True if a tileset with the given name exists in the system, false otherwise.</returns>
        bool HasTileSet(string tilesetName);

        /// <summary>
        /// Gets the list of the names of the available tilesets.
        /// </summary>
        IEnumerable<string> TileSets { get; }

        /// <summary>
        /// Gets the terrain types defined by the given tileset.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset.</param>
        /// <returns>The list of the names of the terrain types.</returns>
        IEnumerable<string> GetTerrainTypes(string tilesetName);

        /// <summary>
        /// TODO: This is a hack for the MapControl.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ITileSet GetTileSet(string name);
    }
}
