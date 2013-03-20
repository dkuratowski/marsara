using System;
using System.Collections.Generic;
using RC.Common.ComponentModel;
using System.IO;
using RC.Engine.ComponentInterfaces;
using RC.Common;
using RC.Engine.PublicInterfaces;
using RC.App.BizLogic.InternalInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the TileSetStore component.
    /// </summary>
    [Component("RC.App.BizLogic.TileSetStore")]
    class TileSetStore : ITileSetStore, IComponentStart
    {
        /// <summary>
        /// Constructs a TileSetStore instance.
        /// </summary>
        public TileSetStore()
        {
            this.tilesetLoader = null;
            this.loadedTilesets = new Dictionary<string, ITileSet>();
        }

        #region ITileSetStore methods

        /// <see cref="ITileSetStore.HasTileSet"/>
        public bool HasTileSet(string tilesetName)
        {
            return this.loadedTilesets.ContainsKey(tilesetName);
        }

        /// <see cref="ITileSetStore.TileSets"/>
        public IEnumerable<string> TileSets
        {
            get { return this.loadedTilesets.Keys; }
        }

        /// <see cref="ITileSetStore.GetTerrainTypes"/>
        public IEnumerable<string> GetTerrainTypes(string tilesetName)
        {
            ITileSet tileset = this.loadedTilesets[tilesetName];
            List<string> retList = new List<string>();
            foreach (ITerrainType terrainType in tileset.TerrainTypes)
            {
                retList.Add(terrainType.Name);
            }
            return retList;
        }

        /// TODO: this is hack for MapControl.
        public ITileSet GetTileSet(string tilesetName)
        {
            return this.loadedTilesets[tilesetName];
        }

        #endregion ITileSetStore methods

        #region IComponentStart methods

        /// <see cref="IComponentStart.Start"/>
        public void Start()
        {
            /// Load the tilesets from the configured directory
            DirectoryInfo rootDir = new DirectoryInfo(BizLogicConstants.TILESET_DIR);
            FileInfo[] tilesetFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo tilesetFile in tilesetFiles)
            {
                /// TODO: this is a hack!
                string xmlStr = File.ReadAllText(tilesetFile.FullName);
                string imageDir = tilesetFile.DirectoryName;
                RCPackage tilesetPackage = RCPackage.CreateCustomDataPackage(RCEngineFormats.TILESET_FORMAT);
                tilesetPackage.WriteString(0, xmlStr);
                tilesetPackage.WriteString(1, imageDir);

                ITileSet tileset = this.tilesetLoader.LoadTileSet(tilesetPackage);

                if (this.loadedTilesets.ContainsKey(tileset.Name))
                {
                    throw new InvalidOperationException(string.Format("Tileset with name '{0}' already loaded!", tileset.Name));
                }

                this.loadedTilesets.Add(tileset.Name, tileset);
            }
        }

        #endregion IComponentStart methods

        /// <summary>
        /// Reference to the RC.Engine.TileSetLoader component.
        /// </summary>
        [ComponentReference]
        private ITileSetLoader tilesetLoader;

        /// <summary>
        /// List of the loaded tilesets mapped by their names.
        /// </summary>
        private Dictionary<string, ITileSet> loadedTilesets;
    }
}
