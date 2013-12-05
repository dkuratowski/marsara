using System;
using RC.Common.ComponentModel;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Maps.ComponentInterfaces;
using RC.App.BizLogic.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using System.IO;
using RC.Engine.Simulator.Scenarios;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the map editor backend component.
    /// </summary>
    [Component("RC.App.BizLogic.MapEditorBE")]
    class MapEditorBE : IMapEditorBE, IComponent
    {
        /// <summary>
        /// Constructs a MapEditorBE instance.
        /// </summary>
        public MapEditorBE()
        {
            this.activeMap = null;
            this.activeScenario = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.mapLoader = ComponentManager.GetInterface<IMapLoader>();
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
            this.mapEditor = ComponentManager.GetInterface<IMapEditor>();
            this.tilesetStore = ComponentManager.GetInterface<ITileSetStore>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        #endregion IComponent methods

        #region IMapEditorBE methods

        /// <see cref="IMapEditorBE.NewMap"/>
        public void NewMap(string mapName, string tilesetName, string defaultTerrain, RCIntVector mapSize)
        {
            if (this.activeMap != null) { throw new InvalidOperationException("Another map is already opened!"); }

            this.activeMap = this.mapLoader.NewMap(mapName, this.tilesetStore.GetTileSet(tilesetName), defaultTerrain, mapSize);
            this.activeScenario = this.scenarioLoader.NewScenario(this.activeMap);
        }

        /// <see cref="IMapEditorBE.NewMap"/>
        public void LoadMap(string filename)
        {
            if (this.activeMap != null) { throw new InvalidOperationException("Another map is already opened!"); }
            if (filename == null) { throw new ArgumentNullException("fileName"); }

            byte[] mapBytes = File.ReadAllBytes(filename);
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            this.activeMap = this.mapLoader.LoadMap(this.tilesetStore.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.activeScenario = this.scenarioLoader.LoadScenario(this.activeMap, mapBytes);
        }

        /// <see cref="IMapEditorBE.NewMap"/>
        public void SaveMap(string filename)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (filename == null) { throw new ArgumentNullException("fileName"); }

            byte[] mapBytes = this.mapLoader.SaveMap(this.activeMap);
            byte[] scenarioBytes = this.scenarioLoader.SaveScenario(this.activeScenario);

            int outIdx = 0;
            byte[] outputBytes = new byte[mapBytes.Length + scenarioBytes.Length];
            for (int i = 0; i < mapBytes.Length; i++, outIdx++) { outputBytes[outIdx] = mapBytes[i]; }
            for (int i = 0; i < scenarioBytes.Length; i++, outIdx++) { outputBytes[outIdx] = scenarioBytes[i]; }
            File.WriteAllBytes(filename, outputBytes);
        }

        /// <see cref="IMapEditorBE.NewMap"/>
        public void CloseMap()
        {
            if (this.activeMap != null)
            {
                this.activeMap.Close();
                this.activeMap = null;
                this.activeScenario = null;
            }
        }

        /// <see cref="IMapEditorBE.CreateMapTerrainView"/>
        public IMapTerrainView CreateMapTerrainView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new MapTerrainView(this.activeMap);
        }

        /// <see cref="IMapEditorBE.CreateMapObjectView"/>
        public IMapObjectView CreateMapObjectView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new MapObjectView(this.activeScenario);
        }

        /// <see cref="IMapEditorBE.CreateTileSetView"/>
        public ITileSetView CreateTileSetView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new TileSetView(this.activeMap.Tileset);
        }

        /// <see cref="IMapEditorBE.CreateMetadataView"/>
        public IMetadataView CreateMetadataView()
        {
            return new MetadataView(this.scenarioLoader.Metadata);
        }

        /// <see cref="IMapEditorBE.CreateTerrainObjectPlacementView"/>
        public IObjectPlacementView CreateTerrainObjectPlacementView(string terrainObjectName)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (terrainObjectName == null) { throw new ArgumentNullException("terrainObjectName"); }

            ITerrainObjectType terrainObjectType = this.activeMap.Tileset.GetTerrainObjectType(terrainObjectName);
            return new TerrainObjectPlacementView(terrainObjectType, this.activeMap);
        }

        /// <see cref="IMapEditorBE.CreateMapObjectPlacementView"/>
        public IObjectPlacementView CreateMapObjectPlacementView(string mapObjectTypeName)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (mapObjectTypeName == null) { throw new ArgumentNullException("mapObjectTypeName"); }

            IScenarioElementType elementType = this.scenarioLoader.Metadata.GetElementType(mapObjectTypeName);
            return new MapObjectPlacementView(elementType, this.activeScenario);
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
        private ITileSetStore tilesetStore;

        /// <summary>
        /// Reference to the RC.Engine.Maps.MapEditor component.
        /// </summary>
        private IMapEditor mapEditor;

        /// <summary>
        /// Reference to the RC.Engine.Maps.MapLoader component.
        /// </summary>
        private IMapLoader mapLoader;

        /// <summary>
        /// Reference to the RC.Engine.Scenarios.ScenarioLoader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;

        /// <summary>
        /// Reference to the currently active map.
        /// </summary>
        private IMapAccess activeMap;

        /// <summary>
        /// Reference to the currently active scenario.
        /// </summary>
        private Scenario activeScenario;
    }
}
