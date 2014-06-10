using System;
using RC.Common.ComponentModel;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using System.IO;
using RC.Engine.Simulator.Scenarios;
using RC.Engine.Simulator.ComponentInterfaces;
using System.Collections.Generic;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.MotionControl;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Views.Core;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// The implementation of the map editor backend component.
    /// </summary>
    [Component("RC.App.BizLogic.MapEditorService")]
    class MapEditorService : IMapEditorService, IComponent
    {
        /// <summary>
        /// Constructs a MapEditorBE instance.
        /// </summary>
        public MapEditorService()
        {
            this.activeMap = null;
            this.activeScenario = null;
            this.timeScheduler = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.mapLoader = ComponentManager.GetInterface<IMapLoader>();
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
            this.navmeshLoader = ComponentManager.GetInterface<INavMeshLoader>();
            this.mapEditor = ComponentManager.GetInterface<IMapEditor>();
            this.tilesetStore = ComponentManager.GetInterface<ITilesetManagerBC>();
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
            this.viewFactoryRegistry = ComponentManager.GetInterface<IViewFactoryRegistry>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            if (this.timeScheduler != null) { this.timeScheduler.Dispose(); }
        }

        #endregion IComponent methods

        #region IMapEditorService methods

        /// <see cref="IMapEditorService.NewMap"/>
        public void NewMap(string mapName, string tilesetName, string defaultTerrain, RCIntVector mapSize)
        {
            if (this.activeMap != null) { throw new InvalidOperationException("Another map is already opened!"); }

            this.activeMap = this.mapLoader.NewMap(mapName, this.tilesetStore.GetTileSet(tilesetName), defaultTerrain, mapSize);
            this.activeScenario = this.scenarioLoader.NewScenario(this.activeMap);
            this.timeScheduler = new Scheduler(MAPEDITOR_MS_PER_FRAMES);
            this.timeScheduler.AddScheduledFunction(this.activeScenario.UpdateAnimations);

            this.RegisterFactoryMethods();
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void LoadMap(string filename)
        {
            if (this.activeMap != null) { throw new InvalidOperationException("Another map is already opened!"); }
            if (filename == null) { throw new ArgumentNullException("fileName"); }

            byte[] mapBytes = File.ReadAllBytes(filename);
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            this.activeMap = this.mapLoader.LoadMap(this.tilesetStore.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.pathFinder.Initialize(this.navmeshLoader.LoadNavMesh(mapBytes), MAX_PATHFINDING_ITERATIONS_PER_FRAMES);
            this.activeScenario = this.scenarioLoader.LoadScenario(this.activeMap, mapBytes);
            this.timeScheduler = new Scheduler(MAPEDITOR_MS_PER_FRAMES);
            this.timeScheduler.AddScheduledFunction(this.activeScenario.UpdateAnimations);

            this.RegisterFactoryMethods();
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void SaveMap(string filename)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (filename == null) { throw new ArgumentNullException("fileName"); }

            /// Try to create a navmesh from the map but do not crash.
            INavMesh navmesh = null;
            try { navmesh = this.navmeshLoader.NewNavMesh(new MapWalkabilityReader(this.activeMap)); }
            catch (Exception ex) { TraceManager.WriteExceptionAllTrace(ex, false); }

            /// Serialize the map, the scenario and the navmesh if it has been successfully created.
            byte[] mapBytes = this.mapLoader.SaveMap(this.activeMap);
            byte[] scenarioBytes = this.scenarioLoader.SaveScenario(this.activeScenario);
            byte[] navmeshBytes = navmesh != null ? this.navmeshLoader.SaveNavMesh(navmesh) : new byte[0];

            /// Write the serialized data into the output file.
            int outIdx = 0;
            byte[] outputBytes = new byte[mapBytes.Length + scenarioBytes.Length + navmeshBytes.Length];
            for (int i = 0; i < mapBytes.Length; i++, outIdx++) { outputBytes[outIdx] = mapBytes[i]; }
            for (int i = 0; i < scenarioBytes.Length; i++, outIdx++) { outputBytes[outIdx] = scenarioBytes[i]; }
            for (int i = 0; i < navmeshBytes.Length; i++, outIdx++) { outputBytes[outIdx] = navmeshBytes[i]; }
            File.WriteAllBytes(filename, outputBytes);
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void CloseMap()
        {
            if (this.activeMap != null)
            {
                this.UnregisterFactoryMethods();

                this.timeScheduler.Dispose();
                this.timeScheduler = null;
                this.activeMap.Close();
                this.activeMap = null;
                this.activeScenario = null;
            }
        }

        /// <see cref="IMapEditorService.DrawTerrain"/>
        public void DrawTerrain(RCIntRectangle displayedArea, RCIntVector position, string terrainType)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IIsoTile isotile = this.activeMap.GetCell(navCellCoords).ParentIsoTile;

            IEnumerable<IIsoTile> affectedIsoTiles = this.mapEditor.DrawTerrain(this.activeMap, isotile, this.activeMap.Tileset.GetTerrainType(terrainType));

            foreach (IIsoTile affectedIsoTile in affectedIsoTiles)
            {
                RCNumRectangle isoTileRect = new RCNumRectangle(affectedIsoTile.GetCellMapCoords(new RCIntVector(0, 0)), affectedIsoTile.CellSize)
                                           - new RCNumVector(1, 1) / 2;
                foreach (QuadEntity affectedEntity in this.activeScenario.GetVisibleEntities<QuadEntity>(isoTileRect))
                {
                    this.activeScenario.VisibleEntities.DetachContent(affectedEntity);
                    bool violatingConstraints = false;
                    if (affectedEntity.ElementType.CheckConstraints(this.activeScenario, affectedEntity.LastKnownQuadCoords).Count != 0)
                    {
                        this.activeScenario.RemoveEntity(affectedEntity);
                        affectedEntity.Dispose();
                        violatingConstraints = true;
                    }
                    if (!violatingConstraints) { this.activeScenario.VisibleEntities.AttachContent(affectedEntity); }
                }
            }
        }

        /// <see cref="IMapEditorService.PlaceTerrainObject"/>
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

            if (placedTerrainObject != null)
            {
                RCNumRectangle terrObjRect = new RCNumRectangle(this.activeMap.GetQuadTile(placedTerrainObject.MapCoords).GetCell(new RCIntVector(0, 0)).MapCoords, placedTerrainObject.CellSize)
                                           - new RCNumVector(1, 1) / 2;
                foreach (QuadEntity affectedEntity in this.activeScenario.GetVisibleEntities<QuadEntity>(terrObjRect))
                {
                    this.activeScenario.VisibleEntities.DetachContent(affectedEntity);
                    bool violatingConstraints = false;
                    if (affectedEntity.ElementType.CheckConstraints(this.activeScenario, affectedEntity.LastKnownQuadCoords).Count != 0)
                    {
                        this.activeScenario.RemoveEntity(affectedEntity);
                        affectedEntity.Dispose();
                        violatingConstraints = true;
                    }
                    if (!violatingConstraints) { this.activeScenario.VisibleEntities.AttachContent(affectedEntity); }
                }
            }
            return placedTerrainObject != null;
        }

        /// <see cref="IMapEditorService.RemoveTerrainObject"/>
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

        /// <see cref="IMapEditorService.PlaceStartLocation"/>
        public bool PlaceStartLocation(RCIntRectangle displayedArea, RCIntVector position, int playerIndex)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.activeMap.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioLoader.Metadata.GetElementType(StartLocation.STARTLOCATION_TYPE_NAME);
            RCIntVector objQuadSize = this.activeMap.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.activeScenario, topLeftQuadCoords).Count != 0) { return false; }

            /// Check if a start location with the given player index already exists.
            List<StartLocation> startLocations = this.activeScenario.GetAllEntities<StartLocation>();
            StartLocation startLocation = null;
            foreach (StartLocation sl in startLocations)
            {
                if (sl.PlayerIndex == playerIndex)
                {
                    startLocation = sl;
                    break;
                }
            }

            /// If a start location with the given player index already exists, change its quadratic coordinates,
            /// otherwise create a new start location.
            if (startLocation != null)
            {
                startLocation.RemoveFromMap();
            }
            else
            {
                startLocation = new StartLocation(playerIndex);
                this.activeScenario.AddEntity(startLocation);
            }
            startLocation.AddToMap(this.activeMap.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.PlaceMineralField"/>
        public bool PlaceMineralField(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.activeMap.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioLoader.Metadata.GetElementType(MineralField.MINERALFIELD_TYPE_NAME);
            RCIntVector objQuadSize = this.activeMap.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.activeScenario, topLeftQuadCoords).Count != 0) { return false; }

            MineralField placedMineralField = new MineralField();
            this.activeScenario.AddEntity(placedMineralField);
            placedMineralField.AddToMap(this.activeMap.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.PlaceVespeneGeyser"/>
        public bool PlaceVespeneGeyser(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.activeMap.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioLoader.Metadata.GetElementType(VespeneGeyser.VESPENEGEYSER_TYPE_NAME);
            RCIntVector objQuadSize = this.activeMap.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.activeScenario, topLeftQuadCoords).Count != 0) { return false; }

            VespeneGeyser placedVespeneGeyser = new VespeneGeyser();
            this.activeScenario.AddEntity(placedVespeneGeyser);
            placedVespeneGeyser.AddToMap(this.activeMap.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.RemoveEntity"/>
        public bool RemoveEntity(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            foreach (Entity entity in this.activeScenario.VisibleEntities.GetContents(navCellCoords))
            {
                entity.RemoveFromMap();
                this.activeScenario.RemoveEntity(entity);
                entity.Dispose();
                return true;
            }
            return false;
        }

        /// <see cref="IMapEditorService.RemoveEntity"/>
        public bool ChangeResourceAmount(int objectID, int delta)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            MineralField mineralField = this.activeScenario.GetEntity<MineralField>(objectID);
            VespeneGeyser vespeneGeyser = this.activeScenario.GetEntity<VespeneGeyser>(objectID);
            if (mineralField != null)
            {
                int currentResourceAmount = mineralField.ResourceAmount.Read();
                mineralField.ResourceAmount.Write(Math.Max(MineralField.MINIMUM_RESOURCE_AMOUNT, currentResourceAmount + delta));
                return true;
            }
            else if (vespeneGeyser != null)
            {
                int currentResourceAmount = vespeneGeyser.ResourceAmount.Read();
                vespeneGeyser.ResourceAmount.Write(Math.Max(VespeneGeyser.MINIMUM_RESOURCE_AMOUNT, currentResourceAmount + delta));
                return true;
            }
            return false;
        }

        #endregion IMapEditorService methods

        #region View factory methods

        /// <summary>
        /// Creates a view of type IMapTerrainView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapTerrainView CreateMapTerrainView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new MapTerrainView(this.activeMap);
        }

        /// <summary>
        /// Creates a view of type ITileSetView.
        /// </summary>
        /// <returns>The created view.</returns>
        private ITileSetView CreateTileSetView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new TileSetView(this.activeMap.Tileset);
        }

        /// <summary>
        /// Creates a view of type IMetadataView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMetadataView CreateMetadataView()
        {
            return new MetadataView(this.scenarioLoader.Metadata);
        }

        /// <summary>
        /// Creates a view of type IMapObjectView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapObjectView CreateMapObjectView()
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            return new MapObjectView(this.activeScenario);
        }

        /// <summary>
        /// Creates a view of type IMapObjectDataView.
        /// </summary>
        /// <param name="objectID">The ID of the map object to read.</param>
        /// <returns>The created view.</returns>
        private IMapObjectDataView CreateMapObjectDataView(int objectID)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.activeScenario.GetEntity<Entity>(objectID);
            return entity != null ? new MapObjectDataView(entity) : null;
        }

        /// <summary>
        /// Creates a view of type ITerrainObjectPlacementView.
        /// </summary>
        /// <returns>The created view.</returns>
        private ITerrainObjectPlacementView CreateTerrainObjectPlacementView(string terrainObjectName)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (terrainObjectName == null) { throw new ArgumentNullException("terrainObjectName"); }

            ITerrainObjectType terrainObjectType = this.activeMap.Tileset.GetTerrainObjectType(terrainObjectName);
            return new TerrainObjectPlacementView(terrainObjectType, this.activeMap);
        }

        /// <summary>
        /// Creates a view of type IMapObjectPlacementView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapObjectPlacementView CreateMapObjectPlacementView(string mapObjectTypeName)
        {
            if (this.activeMap == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (mapObjectTypeName == null) { throw new ArgumentNullException("mapObjectTypeName"); }

            IScenarioElementType elementType = this.scenarioLoader.Metadata.GetElementType(mapObjectTypeName);
            return new MapObjectPlacementView(elementType, this.activeScenario, this.timeScheduler);
        }

        /// <summary>
        /// Registers the implemented factory methods to the view factory.
        /// </summary>
        private void RegisterFactoryMethods()
        {
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMapTerrainView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateTileSetView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMetadataView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMapObjectView);
            this.viewFactoryRegistry.RegisterViewFactory<IMapObjectDataView, int>(this.CreateMapObjectDataView);
            this.viewFactoryRegistry.RegisterViewFactory<ITerrainObjectPlacementView, string>(this.CreateTerrainObjectPlacementView);
            this.viewFactoryRegistry.RegisterViewFactory<IMapObjectPlacementView, string>(this.CreateMapObjectPlacementView);
        }

        /// <summary>
        /// Unregisters the implemented factory methods from the view factory.
        /// </summary>
        private void UnregisterFactoryMethods()
        {
            this.viewFactoryRegistry.UnregisterViewFactory<IMapTerrainView>();
            this.viewFactoryRegistry.UnregisterViewFactory<ITileSetView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMetadataView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMapObjectView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMapObjectDataView>();
            this.viewFactoryRegistry.UnregisterViewFactory<ITerrainObjectPlacementView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMapObjectPlacementView>();
        }

        #endregion View factory methods

        /// <summary>
        /// Reference to the RC.App.BizLogic.TilesetManagerBC business component.
        /// </summary>
        private ITilesetManagerBC tilesetStore;

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
        /// Reference to the RC.Engine.Maps.NavMeshLoader component.
        /// </summary>
        private INavMeshLoader navmeshLoader;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;

        /// <summary>
        /// Reference to the registry interface of the RC.App.BizLogic.ViewFactory component.
        /// </summary>
        private IViewFactoryRegistry viewFactoryRegistry;

        /// <summary>
        /// Reference to the currently active map.
        /// </summary>
        private IMapAccess activeMap;

        /// <summary>
        /// Reference to the currently active scenario.
        /// </summary>
        private Scenario activeScenario;

        /// <summary>
        /// Reference to the scheduler of the map editor.
        /// </summary>
        private Scheduler timeScheduler;

        /// <summary>
        /// The elapsed time between frames in the map editor in frames.
        /// </summary>
        private const int MAPEDITOR_MS_PER_FRAMES = 40;

        /// <summary>
        /// The maximum number of pathfinding iterations per frames.
        /// </summary>
        private const int MAX_PATHFINDING_ITERATIONS_PER_FRAMES = 2500;
    }
}
