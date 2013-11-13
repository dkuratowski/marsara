﻿using System;
using System.Collections.Generic;
using RC.Common.ComponentModel;
using System.IO;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.App.BizLogic.ComponentInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the TileSetStore component.
    /// </summary>
    [Component("RC.App.BizLogic.TileSetStore")]
    class TileSetStore : ITileSetStore, IComponent
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

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.tilesetLoader = ComponentManager.GetInterface<ITileSetLoader>();

            /// Load the tilesets from the configured directory
            DirectoryInfo rootDir = new DirectoryInfo(BizLogicConstants.TILESET_DIR);
            FileInfo[] tilesetFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo tilesetFile in tilesetFiles)
            {
                /// TODO: this is a hack! Later we will have binary tileset format.
                string xmlStr = File.ReadAllText(tilesetFile.FullName);
                string imageDir = tilesetFile.DirectoryName;
                RCPackage tilesetPackage = RCPackage.CreateCustomDataPackage(PackageFormats.TILESET_FORMAT);
                tilesetPackage.WriteString(0, xmlStr);
                tilesetPackage.WriteString(1, imageDir);

                byte[] buffer = new byte[tilesetPackage.PackageLength];
                tilesetPackage.WritePackageToBuffer(buffer, 0);
                ITileSet tileset = this.tilesetLoader.LoadTileSet(buffer);

                if (this.loadedTilesets.ContainsKey(tileset.Name))
                {
                    throw new InvalidOperationException(string.Format("Tileset with name '{0}' already loaded!", tileset.Name));
                }

                this.loadedTilesets.Add(tileset.Name, tileset);
            }
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        #endregion IComponent methods

        /// <summary>
        /// Reference to the RC.Engine.Maps.TileSetLoader component.
        /// </summary>
        private ITileSetLoader tilesetLoader;

        /// <summary>
        /// List of the loaded tilesets mapped by their names.
        /// </summary>
        private Dictionary<string, ITileSet> loadedTilesets;
    }
}
