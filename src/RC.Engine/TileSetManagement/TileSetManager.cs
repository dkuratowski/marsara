using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common.ComponentModel;

namespace RC.Engine
{
    /// <summary>
    /// Implementation of the tileset manager component.
    /// </summary>
    [Component("RC.Engine.TileSetManager")]
    class TileSetManager : ITileSetManager, IDisposable
    {
        /// <summary>
        /// Constructs a TileSetManager object.
        /// </summary>
        public TileSetManager()
        {
            this.loadedTileSets = new Dictionary<string, TileSet>();
            this.objectDisposed = false;
        }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("TileSetManager"); }
            TraceManager.WriteAllTrace("Destroying tileset manager", EngineTraceFilters.INFO);

            /// TODO: implement disposing procedure here!

            this.objectDisposed = true;
            TraceManager.WriteAllTrace("Tileset manager destroyed", EngineTraceFilters.INFO);
        }

        #endregion IDisposable methods

        #region ITileSetManager methods

        /// <see cref="ITileSetManager.LoadTileSet"/>
        public string LoadTileSet(string filename)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("TileSetManager"); }
            if (filename == null) { throw new ArgumentNullException("filename"); }

            TileSet tileset = XmlTileSetReader.Read(filename);
            if (this.loadedTileSets.ContainsKey(tileset.Name)) { throw new InvalidOperationException(string.Format("Tileset with the name '{0}' already exists!", tileset.Name)); }

            this.loadedTileSets.Add(tileset.Name, tileset);
            return tileset.Name;
        }

        /// <see cref="ITileSetManager.GetTileSet"/>
        public TileSet GetTileSet(string name)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("TileSetManager"); }
            if (name == null) { throw new ArgumentNullException("name"); }
            if (!this.loadedTileSets.ContainsKey(name)) { throw new InvalidOperationException(string.Format("Tileset with the name '{0}' doesn't exist!", name)); }

            return this.loadedTileSets[name];
        }

        #endregion ITileSetManager methods

        /// <summary>
        /// List of the installed tilesets mapped by their names.
        /// </summary>
        private Dictionary<string, TileSet> loadedTileSets;

        /// <summary>
        /// This flag indicates whether this object has been disposed or not.
        /// </summary>
        private bool objectDisposed;
    }
}
