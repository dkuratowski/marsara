using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.Engine
{
    /// <summary>
    /// Implementation of the map manager component.
    /// </summary>
    [Component("RC.Engine.MapManager")]
    class MapManager : IMapManager, IDisposable
    {
        /// <summary>
        /// Constructs a MapManager object.
        /// </summary>
        public MapManager()
        {
            this.tilesetManager = null;
            this.currentMap = null;
            this.objectDisposed = false;
        }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            lock (this.lockObject)
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("MapManager"); }

                TraceManager.WriteAllTrace("Destroying map manager", EngineTraceFilters.INFO);

                if (this.currentMap != null)
                {
                    this.CloseMap();
                    this.currentMap.Dispose();
                    this.currentMap = null;
                }

                this.objectDisposed = true;
                TraceManager.WriteAllTrace("Map manager destroyed", EngineTraceFilters.INFO);
            }
        }

        #endregion IDisposable methods

        #region IMapManager methods

        /// <see cref="IMapManager.Initialize"/>
        public void Initialize()
        {
            lock (this.lockObject)
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("MapManager"); }
                if (this.currentMap != null) { throw new InvalidOperationException("MapManager already initialized!"); }

                this.currentMap = new Map();
                this.currentMap.InitStructure();
            }
        }

        /// <see cref="IMapManager.CreateMap"/>
        public IMapEdit CreateMap(string tilesetName, string defaultTerrain, RCIntVector size)
        {
            lock (this.lockObject)
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("MapManager"); }
                if (this.currentMap == null) { throw new InvalidOperationException("MapManager is not initialized!"); }
                if (this.currentMap.Status != MapStatus.Closed) { throw new InvalidOperationException("A map is already opened!"); }
                if (tilesetName == null) { throw new ArgumentNullException("tilesetName"); }
                if (defaultTerrain == null) { throw new ArgumentNullException("defaultTerrain"); }
                if (size == RCIntVector.Undefined) { throw new ArgumentNullException("size"); }

                TileSet tileset = this.tilesetManager.GetTileSet(tilesetName);
                this.currentMap.BeginOpen(tileset, size, defaultTerrain);
                this.currentMap.ReadyToEdit();

                return this.currentMap;
            }
        }

        /// <see cref="IMapManager.LoadMapForEdit"/>
        public IMapEdit LoadMapForEdit(string fileName)
        {
            lock (this.lockObject)
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("MapManager"); }
                if (this.currentMap == null) { throw new InvalidOperationException("MapManager is not initialized!"); }
                if (this.currentMap.Status != MapStatus.Closed) { throw new InvalidOperationException("A map is already opened!"); }
                throw new NotImplementedException();
            }
        }

        /// <see cref="IMapManager.LoadMap"/>
        public IMap LoadMap(string fileName)
        {
            lock (this.lockObject)
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("MapManager"); }
                if (this.currentMap == null) { throw new InvalidOperationException("MapManager is not initialized!"); }
                if (this.currentMap.Status != MapStatus.Closed) { throw new InvalidOperationException("A map is already opened!"); }
                throw new NotImplementedException();
            }
        }

        /// <see cref="IMapManager.CloseMap"/>
        public void CloseMap()
        {
            lock (this.lockObject)
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("MapManager"); }
                if (this.currentMap == null) { throw new InvalidOperationException("MapManager is not initialized!"); }
                if (this.currentMap.Status != MapStatus.ReadyToEdit && this.currentMap.Status != MapStatus.ReadyToUse) { throw new InvalidOperationException("No opened map!"); }

                this.currentMap.Close();
            }
        }

        #endregion IMapManager methods

        /// <summary>
        /// Reference to the tileset manager component. This reference is automatically set by the
        /// component manager.
        /// </summary>
        [ComponentReference]
        private ITileSetManager tilesetManager;

        /// <summary>
        /// Reference to the current map or null if there is no opened map.
        /// </summary>
        private Map currentMap;

        /// <summary>
        /// Used as a lock for thread safety.
        /// </summary>
        private object lockObject = new object();

        /// <summary>
        /// This flag indicates whether this object has been disposed or not.
        /// </summary>
        private bool objectDisposed;
    }
}
