using System;
using System.Collections.Generic;
using RC.Common.ComponentModel;
using System.IO;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the TilesetManager business component.
    /// </summary>
    [Component("RC.App.BizLogic.TilesetManagerBC")]
    class TilesetManagerBC : ITilesetManagerBC, IComponent
    {
        /// <summary>
        /// Constructs a TileSetStore instance.
        /// </summary>
        public TilesetManagerBC()
        {
            this.tilesetLoader = null;
            this.loadedTilesets = new Dictionary<string, ITileSet>();
        }

        #region ITileSetStore methods

        /// <see cref="ITileSetStore.GetTileSet"/>
        public ITileSet GetTileSet(string tilesetName)
        {
            return this.loadedTilesets.ContainsKey(tilesetName) ? this.loadedTilesets[tilesetName] : null;
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
