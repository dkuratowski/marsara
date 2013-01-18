using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of the map manager component. Using the map manager you can create a new map for editing
    /// or load an existing map for editing or playing.
    /// </summary>
    [ComponentInterface]
    public interface IMapManager
    {
        /// <summary>
        /// Initializes the map manager. This method must be called before any other operations.
        /// </summary>
        /// <remarks>
        /// Initializing the map manager is a long running process. If you don't want to block the UI in the
        /// meantime, you can call this method from a background thread.
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Creates a new map for editing.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset of the new map.</param>
        /// <param name="defaultTerrain">
        /// Name of a terrain type defined by the tileset. This will be the default terrain of the new map.
        /// </param>
        /// <param name="size">
        /// Size of the map in QuadTiles. The first coordinate of the vector is the width, the second coordinate of
        /// the vector is the height of the new map. The constraints are the followings: the width of the map must be
        /// a multiple of Map.QUAD_PER_ISO_VERT, the height of the map must be a multiple of Map.QUAD_PER_ISO_HORZ.
        /// </param>
        /// <returns>The editing interface of the created map.</returns>
        IMapEdit CreateMap(string tilesetName, string defaultTerrain, RCIntVector size);

        /// <summary>
        /// Loads an existing map from the given file for editing.
        /// </summary>
        /// <param name="fileName">The name of the file to load from.</param>
        /// <returns>The editing interface of the loaded map.</returns>
        IMapEdit LoadMapForEdit(string fileName);

        /// <summary>
        /// Loads an existing map from the given file for playing.
        /// </summary>
        /// <param name="fileName">The name of the file to load from.</param>
        /// <returns>The playing interface of the loaded map.</returns>
        IMapPlay LoadMapForPlay(string fileName);

        /// <summary>
        /// Closes the current map. If the map is being edited, every unsaved data will be lost.
        /// </summary>
        void CloseMap();
    }
}
