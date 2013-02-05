using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;
using RC.Engine;
using System.Threading;
using RC.Common.Diagnostics;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the MapControl component.
    /// </summary>
    [Component("RC.App.BizLogic.MapControl")]
    class MapControl : IMapEditor, IMapDisplayInfo, IMapGeneralInfo, IComponentStart, IDisposable
    {
        /// <summary>
        /// Constructs a MapControl instance.
        /// </summary>
        public MapControl()
        {
            this.mapManager = null;
            this.activeMap = null;
            this.activeEditedMap = null;
            this.initThread = new RCThread(this.InitMapMgrThreadProc, "MapManagerInitializer");
            this.initThreadStarted = false;
            this.window = RCIntRectangle.Undefined;
            this.isoTileDisplayInfos = new List<IsoTileDisplayInfo>();
            this.tmpIsoTileList = new HashSet<IIsoTile>();
        }

        #region IMapEditor methods

        /// <see cref="IMapEditor.CreateMap"/>
        public MapEditorErrorCode CreateMap(string tilesetName, string defaultTerrain, RCIntVector mapSize)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            /// TODO: error handling!
            this.activeEditedMap = this.mapManager.CreateMap(tilesetName, defaultTerrain, mapSize);
            this.activeMap = this.activeEditedMap;
            return MapEditorErrorCode.Success;
        }

        /// <see cref="IMapEditor.LoadMap"/>
        public MapEditorErrorCode LoadMap(string filename)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            throw new NotImplementedException();
        }

        /// <see cref="IMapEditor.SaveMap"/>
        public MapEditorErrorCode SaveMap(string filename)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            throw new NotImplementedException();
        }

        /// <see cref="IMapEditor.CloseMap"/>
        public MapEditorErrorCode CloseMap()
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            if (this.activeEditedMap != null && this.activeMap != null)
            {
                /// TODO: error handling
                this.mapManager.CloseMap();
                this.window = RCIntRectangle.Undefined;
                this.isoTileDisplayInfos.Clear();
                this.tmpIsoTileList.Clear();
                this.activeEditedMap = null;
                this.activeMap = null;
            }

            return MapEditorErrorCode.Success;
        }

        /// <see cref="IMapEditor.DrawTerrain"/>
        public MapEditorErrorCode DrawTerrain(RCIntVector position, string terrainName)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            throw new NotImplementedException();
        }

        #endregion IMapEditor methods

        #region IMapDisplayInfo methods

        /// <see cref="IMapDisplayInfo.Window"/>
        public RCIntRectangle Window
        {
            get
            {
                if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
                return this.window;
            }
            set
            {
                if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }

                if (this.window != value)
                {
                    this.window = value;
                    this.isoTileDisplayInfos.Clear();
                    this.tmpIsoTileList.Clear();

                    if (this.window != RCIntRectangle.Undefined)
                    {
                        RCIntVector topLeftNavCellCoords = new RCIntVector(Math.Max(0, this.window.Left), Math.Max(0, this.window.Top));
                        RCIntVector bottomRightNavCellCoords = new RCIntVector(Math.Min(this.activeMap.NavSize.X - 1, this.window.Right - 1), Math.Min(this.activeMap.NavSize.Y - 1, this.window.Bottom - 1));
                        IQuadTile topLeftQuadTile = this.activeMap.GetNavCell(topLeftNavCellCoords).ParentQuadTile;
                        IQuadTile bottomRightQuadTile = this.activeMap.GetNavCell(bottomRightNavCellCoords).ParentQuadTile;

                        for (int x = topLeftQuadTile.MapCoords.X; x <= bottomRightQuadTile.MapCoords.X; x++)
                        {
                            for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                            {
                                IIsoTile isotile = this.activeMap.GetQuadTile(new RCIntVector(x, y)).IsoTile;
                                if (this.tmpIsoTileList.Contains(isotile)) { continue; }

                                this.tmpIsoTileList.Add(isotile);
                                this.isoTileDisplayInfos.Add(
                                    new IsoTileDisplayInfo()
                                    {
                                        TileTypeIndex = isotile.Variant.Index,
                                        DisplayCoords = isotile.GetNavCellCoords(new RCIntVector(0, 0)) - this.window.Location
                                    });
                            }

                            if (x == topLeftQuadTile.MapCoords.X)
                            {
                                for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                                {
                                    IQuadTile quadTile = this.activeMap.GetQuadTile(new RCIntVector(x, y));
                                    for (int row = 0; row < quadTile.NavCellDims.Y; row++)
                                    {
                                        IIsoTile isotile = quadTile.GetNavCell(new RCIntVector(0, row)).ParentIsoTile;
                                        if (this.tmpIsoTileList.Contains(isotile)) { continue; }

                                        this.tmpIsoTileList.Add(isotile);
                                        this.isoTileDisplayInfos.Add(
                                            new IsoTileDisplayInfo()
                                            {
                                                TileTypeIndex = isotile.Variant.Index,
                                                DisplayCoords = isotile.GetNavCellCoords(new RCIntVector(0, 0)) - this.window.Location
                                            });
                                    }
                                }
                            }
                            else if (x == bottomRightQuadTile.MapCoords.X)
                            {
                                for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                                {
                                    IQuadTile quadTile = this.activeMap.GetQuadTile(new RCIntVector(x, y));
                                    for (int row = 0; row < quadTile.NavCellDims.Y; row++)
                                    {
                                        IIsoTile isotile = quadTile.GetNavCell(new RCIntVector(quadTile.NavCellDims.X - 1, row)).ParentIsoTile;
                                        if (this.tmpIsoTileList.Contains(isotile)) { continue; }

                                        this.tmpIsoTileList.Add(isotile);
                                        this.isoTileDisplayInfos.Add(
                                            new IsoTileDisplayInfo()
                                            {
                                                TileTypeIndex = isotile.Variant.Index,
                                                DisplayCoords = isotile.GetNavCellCoords(new RCIntVector(0, 0)) - this.window.Location
                                            });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <see cref="IMapDisplayInfo.IsoTileDisplayInfos"/>
        public IEnumerable<IsoTileDisplayInfo> IsoTileDisplayInfos
        {
            get
            {
                if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
                return this.isoTileDisplayInfos;
            }
        }

        /// <see cref="IMapDisplayInfo.GetIsoTileDisplayCoords"/>
        public RCIntVector GetIsoTileDisplayCoords(RCIntVector position)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = this.window.Location + position;
            IIsoTile isotile = this.activeMap.GetNavCell(navCellCoords).ParentIsoTile;
            return isotile.GetNavCellCoords(new RCIntVector(0, 0)) - this.window.Location;
        }

        #endregion IMapDisplayInfo methods

        #region IMapGeneralInfo methods

        /// <see cref="IMapGeneralInfo.IsMapOpened"/>
        public bool IsMapOpened { get { return this.activeMap != null; } }

        /// <see cref="IMapGeneralInfo.Size"/>
        public RCIntVector Size
        {
            get
            {
                if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
                return this.activeMap.Size;
            }
        }

        /// <see cref="IMapGeneralInfo.NavSize"/>
        public RCIntVector NavSize
        {
            get
            {
                if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
                return this.activeMap.NavSize;
            }
        }

        /// <see cref="IMapGeneralInfo.TilesetName"/>
        public string TilesetName
        {
            get
            {
                if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
                return this.activeMap.Tileset.Name;
            }
        }

        #endregion IMapGeneralInfo methods

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
        private void InitMapMgrThreadProc()
        {
            TraceManager.WriteAllTrace("MapManager.Initialize starting...", BizLogicTraceFilters.INFO);
            this.mapManager.Initialize();
            TraceManager.WriteAllTrace("MapManager.Initialize finished.", BizLogicTraceFilters.INFO);
        }

        #endregion IComponentStart methods

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.initThreadStarted)
            {
                this.initThread.Join();
            }
        }

        #endregion IDisposable methods

        /// <summary>
        /// Reference to the RC.Engine.MapManager component.
        /// </summary>
        [ComponentReference]
        private IMapManager mapManager;

        /// <summary>
        /// Reference to the currently active map.
        /// </summary>
        private IMap activeMap;

        /// <summary>
        /// Reference to the currently active map being edited.
        /// </summary>
        private IMapEdit activeEditedMap;

        /// <summary>
        /// Reference to the initializer thread.
        /// </summary>
        private RCThread initThread;

        /// <summary>
        /// This flag indicates whether the initializer thread has been started or not.
        /// </summary>
        private bool initThreadStarted;

        /// <summary>
        /// This window designates the displayed area of the opened map.
        /// </summary>
        private RCIntRectangle window;

        /// <summary>
        /// List of the isometric tile display informations.
        /// </summary>
        private List<IsoTileDisplayInfo> isoTileDisplayInfos;

        /// <summary>
        /// Temporary list of displayed isometric tiles.
        /// </summary>
        private HashSet<IIsoTile> tmpIsoTileList;
    }
}
