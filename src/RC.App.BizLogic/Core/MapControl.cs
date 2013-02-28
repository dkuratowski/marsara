using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;
using RC.Engine;
using System.Threading;
using RC.Common.Diagnostics;
using RC.Engine.PublicInterfaces;
using Eng = RC.Engine.ComponentInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the MapControl component.
    /// </summary>
    [Component("RC.App.BizLogic.MapControl")]
    class MapControl : IMapEditor, IMapDisplayInfo, IMapGeneralInfo, IDisposable
    {
        /// <summary>
        /// Constructs a MapControl instance.
        /// </summary>
        public MapControl()
        {
            this.mapEditor = null;
            this.mapLoader = null;
            this.activeMap = null;
            this.window = RCIntRectangle.Undefined;
            this.isoTileDisplayInfos = new List<IsoTileDisplayInfo>();
            this.tmpIsoTileList = new HashSet<IIsoTile>();
        }

        #region IMapEditor methods

        /// <see cref="IMapEditor.CreateMap"/>
        public MapEditorErrorCode CreateMap(string tilesetName, string defaultTerrain, RCIntVector mapSize)
        {
            /// TODO: error handling!
            /// TODO: ITileSetStore.GetTileSet is a hack!!!
            this.activeMap = this.mapLoader.NewMap(this.tilesetStore.GetTileSet(tilesetName), defaultTerrain, mapSize);
            return MapEditorErrorCode.Success;
        }

        /// <see cref="IMapEditor.LoadMap"/>
        public MapEditorErrorCode LoadMap(string filename)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMapEditor.SaveMap"/>
        public MapEditorErrorCode SaveMap(string filename)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMapEditor.CloseMap"/>
        public MapEditorErrorCode CloseMap()
        {
            if (this.activeMap != null)
            {
                /// TODO: error handling
                this.activeMap.Close();
                this.activeMap = null;
                this.window = RCIntRectangle.Undefined;
                this.isoTileDisplayInfos.Clear();
                this.tmpIsoTileList.Clear();
            }

            return MapEditorErrorCode.Success;
        }

        /// <see cref="IMapEditor.DrawTerrain"/>
        public MapEditorErrorCode DrawTerrain(RCIntVector position, string terrainName)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            RCIntVector navCellCoords = this.window.Location + position;
            IIsoTile isotile = this.activeMap.GetCell(navCellCoords).ParentIsoTile;

            this.mapEditor.DrawTerrain(this.activeMap, isotile, this.activeMap.Tileset.GetTerrainType(terrainName));
            this.RefreshDisplayInfos();
            return MapEditorErrorCode.Success;
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
                    this.RefreshDisplayInfos();
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
            IIsoTile isotile = this.activeMap.GetCell(navCellCoords).ParentIsoTile;
            return isotile.GetCellMapCoords(new RCIntVector(0, 0)) - this.window.Location;
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
                return this.activeMap.CellSize;
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

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.CloseMap();
        }

        #endregion IDisposable methods

        private void RefreshDisplayInfos()
        {
            this.isoTileDisplayInfos.Clear();
            this.tmpIsoTileList.Clear();

            if (this.window != RCIntRectangle.Undefined)
            {
                RCIntVector topLeftNavCellCoords = new RCIntVector(Math.Max(0, this.window.Left), Math.Max(0, this.window.Top));
                RCIntVector bottomRightNavCellCoords = new RCIntVector(Math.Min(this.activeMap.CellSize.X - 1, this.window.Right - 1), Math.Min(this.activeMap.CellSize.Y - 1, this.window.Bottom - 1));
                IQuadTile topLeftQuadTile = this.activeMap.GetCell(topLeftNavCellCoords).ParentQuadTile;
                IQuadTile bottomRightQuadTile = this.activeMap.GetCell(bottomRightNavCellCoords).ParentQuadTile;

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
                                DisplayCoords = isotile.GetCellMapCoords(new RCIntVector(0, 0)) - this.window.Location
                            });
                    }

                    if (x == topLeftQuadTile.MapCoords.X)
                    {
                        for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                        {
                            IQuadTile quadTile = this.activeMap.GetQuadTile(new RCIntVector(x, y));
                            for (int row = 0; row < quadTile.CellSize.Y; row++)
                            {
                                IIsoTile isotile = quadTile.GetCell(new RCIntVector(0, row)).ParentIsoTile;
                                if (this.tmpIsoTileList.Contains(isotile)) { continue; }

                                this.tmpIsoTileList.Add(isotile);
                                this.isoTileDisplayInfos.Add(
                                    new IsoTileDisplayInfo()
                                    {
                                        TileTypeIndex = isotile.Variant.Index,
                                        DisplayCoords = isotile.GetCellMapCoords(new RCIntVector(0, 0)) - this.window.Location
                                    });
                            }
                        }
                    }
                    else if (x == bottomRightQuadTile.MapCoords.X)
                    {
                        for (int y = topLeftQuadTile.MapCoords.Y; y <= bottomRightQuadTile.MapCoords.Y; y++)
                        {
                            IQuadTile quadTile = this.activeMap.GetQuadTile(new RCIntVector(x, y));
                            for (int row = 0; row < quadTile.CellSize.Y; row++)
                            {
                                IIsoTile isotile = quadTile.GetCell(new RCIntVector(quadTile.CellSize.X - 1, row)).ParentIsoTile;
                                if (this.tmpIsoTileList.Contains(isotile)) { continue; }

                                this.tmpIsoTileList.Add(isotile);
                                this.isoTileDisplayInfos.Add(
                                    new IsoTileDisplayInfo()
                                    {
                                        TileTypeIndex = isotile.Variant.Index,
                                        DisplayCoords = isotile.GetCellMapCoords(new RCIntVector(0, 0)) - this.window.Location
                                    });
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reference to the RC.Engine.MapEditor component.
        /// </summary>
        [ComponentReference]
        private Eng.IMapEditor mapEditor;

        /// <summary>
        /// Reference to the RC.Engine.MapLoader component.
        /// </summary>
        [ComponentReference]
        private Eng.IMapLoader mapLoader;

        /// <summary>
        /// Reference to the RC.App.BizLogic.TileSetStore component.
        /// </summary>
        [ComponentReference]
        private ITileSetStore tilesetStore;

        /// <summary>
        /// Reference to the currently active map.
        /// </summary>
        private IMapAccess activeMap;

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
