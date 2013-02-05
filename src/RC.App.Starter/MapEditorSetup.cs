using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.Starter
{
    /// <summary>
    /// Enumerates the possible starting modes of the map editor.
    /// </summary>
    enum MapEditorMode
    {
        Off = 0,        /// Don't start the map editor.
        NewMap = 1,     /// Start the map editor and create a new map file.
        LoadMap = 2,    /// Start the map editor and load an existing map file.
    }

    /// <summary>
    /// Static class that contains the startup-settings of the map editor. By default the map editor is not started.
    /// </summary>
    static class MapEditorSetup
    {
        /// <summary>
        /// Gets the starting mode of the map editor.
        /// </summary>
        public static MapEditorMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Gets the name of the file that the new map will be saved to in case of MapEditorMode.NewMap or
        /// the name of the file that contains the map to be loaded in case of MapEditorMode.LoadMap.
        /// </summary>
        public static string MapFile
        {
            get { return mapFile; }
            set { mapFile = value; }
        }

        /// <summary>
        /// Gets the name of the tileset of the new map in case of MapEditorMode.NewMap.
        /// </summary>
        public static string TilesetName
        {
            get { return tilesetName; }
            set { tilesetName = value; }
        }

        /// <summary>
        /// Gets the name of the default terrain in case of MapEditorMode.NewMap.
        /// </summary>
        public static string DefaultTerrain
        {
            get { return defaultTerrain; }
            set { defaultTerrain = value; }
        }

        /// <summary>
        /// Gets the size of the new map in case of MapEditorMode.NewMap.
        /// </summary>
        public static RCIntVector MapSize
        {
            get { return mapSize; }
            set { mapSize = value; }
        }

        /// <summary>
        /// Gets the string representation of the contents of the MapEditorSetup.
        /// </summary>
        public static new string ToString()
        {
            if (mode == MapEditorMode.NewMap)
            {
                return string.Format("NEW MAP: map-file={0} tileset-file={1} default-terrain={2} size={3}", mapFile, tilesetName, defaultTerrain, mapSize);
            }
            else if (mode == MapEditorMode.LoadMap)
            {
                return string.Format("LOAD MAP: map-file={0} tileset-file={1}", mapFile, tilesetName);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The starting mode of the map editor.
        /// </summary>
        private static MapEditorMode mode = MapEditorMode.Off;

        /// <summary>
        /// The name of the file that the new map will be saved to in case of MapEditorMode.NewMap or
        /// the name of the file that contains the map to be loaded in case of MapEditorMode.LoadMap.
        /// </summary>
        private static string mapFile;

        /// <summary>
        /// The name of the tileset of the new map in case of MapEditorMode.NewMap.
        /// </summary>
        private static string tilesetName;

        /// <summary>
        /// The name of the default terrain in case of MapEditorMode.NewMap.
        /// </summary>
        private static string defaultTerrain;

        /// <summary>
        /// The size of the new map in case of MapEditorMode.NewMap.
        /// </summary>
        private static RCIntVector mapSize;
    }
}
