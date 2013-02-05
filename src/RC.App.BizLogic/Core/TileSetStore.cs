using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine;
using System.IO;

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
            this.tilesetManager = null;
            this.loadedTilesets = new HashSet<string>();
        }

        #region ITileSetStore methods

        /// <see cref="ITileSetStore.HasTileSet"/>
        public bool HasTileSet(string tilesetName)
        {
            return this.loadedTilesets.Contains(tilesetName);
        }

        /// <see cref="ITileSetStore.TileSets"/>
        public IEnumerable<string> TileSets
        {
            get { return this.loadedTilesets; }
        }

        /// <see cref="ITileSetStore.GetTerrainTypes"/>
        public IEnumerable<string> GetTerrainTypes(string tilesetName)
        {
            TileSet tileset = this.tilesetManager.GetTileSet(tilesetName);
            return tileset.TerrainTypes;
        }

        /// <see cref="ITileSetStore.GetTileTypes"/>
        public IEnumerable<TileTypeInfo> GetTileTypes(string tilesetName)
        {
            TileSet tileset = this.tilesetManager.GetTileSet(tilesetName);
            List<TileTypeInfo> retList = new List<TileTypeInfo>();
            foreach (TileVariant tile in tileset.TileVariants)
            {
                byte[] imageData = new byte[tile.ImageData.Length];
                Array.Copy(tile.ImageData, imageData, tile.ImageData.Length);
                TileTypeInfo info = new TileTypeInfo();
                info.ImageData = imageData;
                info.Properties = new Dictionary<string, string>();
                if (tile[BizLogicConstants.TILEPROP_TRANSPARENTCOLOR] != null)
                {
                    info.Properties.Add(BizLogicConstants.TILEPROP_TRANSPARENTCOLOR,
                                        tile[BizLogicConstants.TILEPROP_TRANSPARENTCOLOR]);
                }
                retList.Add(info);
            }
            return retList;
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
                string tilesetName = this.tilesetManager.LoadTileSet(tilesetFile.FullName);
                if (!this.loadedTilesets.Add(tilesetName))
                {
                    throw new InvalidOperationException(string.Format("Tileset with name '{0}' already loaded!", tilesetName));
                }
            }
        }

        #endregion IComponentStart methods

        /// <summary>
        /// Reference to the RC.Engine.TileSetManager component.
        /// </summary>
        [ComponentReference]
        private ITileSetManager tilesetManager;

        /// <summary>
        /// List of the name of the loaded tilesets.
        /// </summary>
        private HashSet<string> loadedTilesets;
    }
}
