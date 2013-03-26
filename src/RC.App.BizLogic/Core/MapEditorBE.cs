using System;
using RC.Common.ComponentModel;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.ComponentInterfaces;
using RC.App.BizLogic.InternalInterfaces;
using RC.Engine.PublicInterfaces;
using RC.Common;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the map editor backend component.
    /// </summary>
    [Component("RC.App.BizLogic.MapEditorBE")]
    class MapEditorBE : IMapEditorBE
    {
        /// <summary>
        /// Constructs a MapEditorBE instance.
        /// </summary>
        public MapEditorBE()
        {
            this.activeMap = null;
        }

        #region IMapEditorBE methods

        /// <see cref="IMapEditorBE.NewMap"/>
        public void NewMap(string tilesetName, string defaultTerrain, RCIntVector mapSize)
        {
            if (this.activeMap != null) { throw new InvalidOperationException("Another map is already opened!"); }

            this.activeMap = this.mapLoader.NewMap(this.tilesetStore.GetTileSet(tilesetName), defaultTerrain, mapSize);
        }

        /// <see cref="IMapEditorBE.NewMap"/>
        public void LoadMap(string filename)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMapEditorBE.NewMap"/>
        public void SaveMap(string filename)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMapEditorBE.NewMap"/>
        public void CloseMap()
        {
            if (this.activeMap != null)
            {
                this.activeMap.Close();
                this.activeMap = null;
            }
        }

        /// <see cref="IMapEditorBE.CreateMapTerrainView"/>
        public IMapTerrainView CreateMapTerrainView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new MapTerrainView(this.activeMap);
        }

        /// <see cref="IMapEditorBE.CreateTileSetView"/>
        public ITileSetView CreateTileSetView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new TileSetView(this.activeMap.Tileset);
        }

        /// <see cref="IMapEditorBE.CreateTerrainObjectPlacementView"/>
        public IObjectPlacementView CreateTerrainObjectPlacementView(string terrainObjectName)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (terrainObjectName == null) { throw new ArgumentNullException("terrainObjectName"); }

            ITerrainObjectType terrainObjectType = this.activeMap.Tileset.GetTerrainObjectType(terrainObjectName);
            return new TerrainObjectPlacementView(terrainObjectType, this.activeMap);
        }

        /// <see cref="IMapEditorBE.DrawTerrain"/>
        public void DrawTerrain(RCIntRectangle displayedArea, RCIntVector position, string terrainType)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IIsoTile isotile = this.activeMap.GetCell(navCellCoords).ParentIsoTile;

            this.mapEditor.DrawTerrain(this.activeMap, isotile, this.activeMap.Tileset.GetTerrainType(terrainType));
        }

        /// <see cref="IMapEditorBE.PlaceTerrainObject"/>
        public bool PlaceTerrainObject(RCIntRectangle displayedArea, RCIntVector position, string terrainObject)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainObject == null) { throw new ArgumentNullException("terrainObject"); }

            ITerrainObjectType terrainObjType = this.activeMap.Tileset.GetTerrainObjectType(terrainObject);
            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.activeMap.GetCell(navCellCoords).ParentQuadTile;
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - terrainObjType.QuadraticSize / 2;

            ITerrainObject placedTerrainObject = null;
            if (topLeftQuadCoords.X >= 0 && topLeftQuadCoords.Y >= 0 &&
                topLeftQuadCoords.X < this.activeMap.Size.X && topLeftQuadCoords.Y < this.activeMap.Size.Y)
            {
                IQuadTile targetQuadTile = this.activeMap.GetQuadTile(topLeftQuadCoords);
                placedTerrainObject = this.mapEditor.PlaceTerrainObject(this.activeMap, targetQuadTile, terrainObjType);
            }
            return placedTerrainObject != null;
        }

        /// <see cref="IMapEditorBE.RemoveTerrainObject"/>
        public bool RemoveTerrainObject(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.activeMap.GetCell(navCellCoords).ParentQuadTile;
            foreach (ITerrainObject objToCheck in this.activeMap.TerrainObjects.GetContents(navCellCoords))
            {
                if (!objToCheck.Type.IsExcluded(quadTileAtPos.MapCoords - objToCheck.MapCoords))
                {
                    this.mapEditor.RemoveTerrainObject(this.activeMap, objToCheck);
                    return true;
                }
            }
            return false;
        }

        #endregion IMapEditorBE methods

        /// <summary>
        /// Reference to the RC.App.BizLogic.TileSetStore component.
        /// </summary>
        [ComponentReference]
        private ITileSetStore tilesetStore;

        /// <summary>
        /// Reference to the RC.Engine.MapEditor component.
        /// </summary>
        [ComponentReference]
        private IMapEditor mapEditor;

        /// <summary>
        /// Reference to the RC.Engine.MapLoader component.
        /// </summary>
        [ComponentReference]
        private IMapLoader mapLoader;

        /// <summary>
        /// Reference to the currently active map.
        /// </summary>
        private IMapAccess activeMap;
    }
}
