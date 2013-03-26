using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.ComponentInterfaces;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// Implementation of the map loader component.
    /// </summary>
    [Component("RC.Engine.MapLoader")]
    class MapLoader : IMapLoader, IComponentStart
    {
        /// <summary>
        /// Constructs a MapLoader object.
        /// </summary>
        public MapLoader()
        {
            this.mapStructure = null;
            this.initThread = new RCThread(this.InitThreadProc, "RC.Engine.MapLoader.InitThread");
            this.initThreadStarted = false;
        }

        #region IComponentStart methods

        /// <see cref="IComponentStart.Start"/>
        public void Start()
        {
            if (!this.initThreadStarted)
            {
                this.initThreadStarted = true;
                this.initThread.Start();
            }
        }

        /// <summary>
        /// Internal method executed by the background initializer thread.
        /// </summary>
        private void InitThreadProc()
        {
            TraceManager.WriteAllTrace("RC.Engine.MapLoader initializing...", RCEngineTraceFilters.INFO);

            this.mapStructure = new MapStructure();
            this.mapStructure.Initialize();

            TraceManager.WriteAllTrace("RC.Engine.MapLoader initialization finished.", RCEngineTraceFilters.INFO);
        }

        #endregion IComponentStart methods

        #region IMapLoader methods

        /// <see cref="IMapLoader.NewMap"/>
        public IMapAccess NewMap(ITileSet tileset, string defaultTerrain, RCIntVector size)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (defaultTerrain == null) { throw new ArgumentNullException("defaultTerrain"); }
            if (size == RCIntVector.Undefined) { throw new ArgumentNullException("size"); }

            MapAccess retObj = new MapAccess(this.mapStructure);
            this.mapStructure.BeginOpen(tileset, size, defaultTerrain);
            this.mapStructure.EndOpen();
            return retObj;
        }

        /// <see cref="IMapLoader.LoadMap"/>
        public IMapAccess LoadMap(ITileSet tileset, RCPackage data)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            /// TODO: check parameters!

            throw new NotImplementedException();
        }

        /// <see cref="IMapLoader.SaveMap"/>
        public RCPackage SaveMap(IMapAccess map)
        {
            throw new NotImplementedException();
        }

        #endregion IMapLoader methods

        /// <summary>
        /// Reference to the map structure.
        /// </summary>
        private MapStructure mapStructure;

        /// <summary>
        /// Reference to the initializer thread.
        /// </summary>
        private RCThread initThread;

        /// <summary>
        /// This flag indicates whether the initializer thread has been started or not.
        /// </summary>
        private bool initThreadStarted;
    }
}
