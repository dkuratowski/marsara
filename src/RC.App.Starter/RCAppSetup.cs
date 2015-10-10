using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.Starter
{
    /// <summary>
    /// Enumerates the possible starting modes of the RC application.
    /// </summary>
    enum RCAppMode
    {
        Normal = 0,     /// Start the RC application in normal mode.
        NewMap = 1,     /// Start the map editor and create a new map file.
        LoadMap = 2,    /// Start the map editor and load an existing map file.
    }

    /// <summary>
    /// Static class that contains the startup-settings for the RC application.
    /// </summary>
    static class RCAppSetup
    {
        /// <summary>
        /// Gets or sets the starting mode of the RC application.
        /// </summary>
        public static RCAppMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Gets or sets the name of the file that the new map will be saved to in case of RCAppMode.NewMap or
        /// the name of the file that contains the map to be loaded in case of RCAppMode.LoadMap.
        /// </summary>
        public static string MapFile
        {
            get { return mapFile; }
            set { mapFile = value; }
        }

        /// <summary>
        /// Gets or sets the name of the new map in case of RCAppMode.NewMap.
        /// </summary>
        public static string MapName
        {
            get { return mapName; }
            set { mapName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the tileset of the new map in case of RCAppMode.NewMap.
        /// </summary>
        public static string TilesetName
        {
            get { return tilesetName; }
            set { tilesetName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the default terrain in case of RCAppMode.NewMap.
        /// </summary>
        public static string DefaultTerrain
        {
            get { return defaultTerrain; }
            set { defaultTerrain = value; }
        }

        /// <summary>
        /// Gets or sets the size of the new map in case of RCAppMode.NewMap.
        /// </summary>
        public static RCIntVector MapSize
        {
            get { return mapSize; }
            set { mapSize = value; }
        }

        /// <summary>
        /// The index of the screen on which to startup the application.
        /// </summary>
        public static int ScreenIndex
        {
            get { return screenIndex; }
            set { screenIndex = value; }
        }

        /// <summary>
        /// Gets the string representation of the contents of the RCAppSetup.
        /// </summary>
        public static new string ToString()
        {
            if (mode == RCAppMode.NewMap)
            {
                return string.Format("NEW MAP: screen-index={0} map-file={1} tileset-file={2} default-terrain={3} size={4}", screenIndex, mapFile, tilesetName, defaultTerrain, mapSize);
            }
            else if (mode == RCAppMode.LoadMap)
            {
                return string.Format("LOAD MAP: screen-index={0} map-file={1} tileset-file={2}", screenIndex, mapFile, tilesetName);
            }
            else
            {
                return string.Format("NORMAL MODE: screen-index={0}", screenIndex);
            }
        }

        /// <summary>
        /// The starting mode of the application.
        /// </summary>
        private static RCAppMode mode = RCAppMode.Normal;

        /// <summary>
        /// The name of the file that the new map will be saved to in case of MapEditorMode.NewMap or
        /// the name of the file that contains the map to be loaded in case of MapEditorMode.LoadMap.
        /// </summary>
        private static string mapFile;

        /// <summary>
        /// The name of the new map in case of MapEditorMode.NewMap.
        /// </summary>
        private static string mapName;

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

        /// <summary>
        /// The index of the screen on which to startup the application.
        /// </summary>
        private static int screenIndex;
    }
}
