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
            this.timeScheduler = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.mapEditor = ComponentManager.GetInterface<IMapEditor>();
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
            this.scenarioManager.NewScenario(mapName, tilesetName, defaultTerrain, mapSize);
            this.timeScheduler = new Scheduler(MAPEDITOR_MS_PER_FRAMES);
            this.timeScheduler.AddScheduledFunction(this.scenarioManager.ActiveScenario.UpdateAnimations);

            this.RegisterFactoryMethods();
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void LoadMap(string filename)
        {
            this.scenarioManager.OpenScenario(filename);
            this.timeScheduler = new Scheduler(MAPEDITOR_MS_PER_FRAMES);
            this.timeScheduler.AddScheduledFunction(this.scenarioManager.ActiveScenario.UpdateAnimations);

            this.RegisterFactoryMethods();
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void SaveMap(string filename)
        {
            this.scenarioManager.SaveScenario(filename);
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void CloseMap()
        {
            if (this.scenarioManager.ActiveScenario != null)
            {
                this.UnregisterFactoryMethods();

                this.timeScheduler.Dispose();
                this.timeScheduler = null;
                this.scenarioManager.CloseScenario();
            }
        }

        /// <see cref="IMapEditorService.DrawTerrain"/>
        public void DrawTerrain(RCIntRectangle displayedArea, RCIntVector position, string terrainType)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IIsoTile isotile = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentIsoTile;

            IEnumerable<IIsoTile> affectedIsoTiles = this.mapEditor.DrawTerrain(this.scenarioManager.ActiveScenario.Map, isotile,
                                                                                this.scenarioManager.ActiveScenario.Map.Tileset.GetTerrainType(terrainType));

            foreach (IIsoTile affectedIsoTile in affectedIsoTiles)
            {
                RCNumRectangle isoTileRect = new RCNumRectangle(affectedIsoTile.GetCellMapCoords(new RCIntVector(0, 0)), affectedIsoTile.CellSize)
                                           - new RCNumVector(1, 1) / 2;
                foreach (QuadEntity affectedEntity in this.scenarioManager.ActiveScenario.GetVisibleEntities<QuadEntity>(isoTileRect))
                {
                    affectedEntity.RemoveFromMap();
                    if (affectedEntity.ElementType.CheckConstraints(this.scenarioManager.ActiveScenario, affectedEntity.LastKnownQuadCoords).Count != 0)
                    {
                        this.scenarioManager.ActiveScenario.RemoveEntity(affectedEntity);
                        affectedEntity.Dispose();
                    }
                    else
                    {
                        affectedEntity.AddToMap(this.scenarioManager.ActiveScenario.Map.GetQuadTile(affectedEntity.LastKnownQuadCoords));
                    }
                }
            }
        }

        /// <see cref="IMapEditorService.PlaceTerrainObject"/>
        public bool PlaceTerrainObject(RCIntRectangle displayedArea, RCIntVector position, string terrainObject)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainObject == null) { throw new ArgumentNullException("terrainObject"); }

            ITerrainObjectType terrainObjType = this.scenarioManager.ActiveScenario.Map.Tileset.GetTerrainObjectType(terrainObject);
            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - terrainObjType.QuadraticSize / 2;

            ITerrainObject placedTerrainObject = null;
            if (topLeftQuadCoords.X >= 0 && topLeftQuadCoords.Y >= 0 &&
                topLeftQuadCoords.X < this.scenarioManager.ActiveScenario.Map.Size.X && topLeftQuadCoords.Y < this.scenarioManager.ActiveScenario.Map.Size.Y)
            {
                IQuadTile targetQuadTile = this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords);
                placedTerrainObject = this.mapEditor.PlaceTerrainObject(this.scenarioManager.ActiveScenario.Map, targetQuadTile, terrainObjType);
            }

            if (placedTerrainObject != null)
            {
                RCNumRectangle terrObjRect = new RCNumRectangle(this.scenarioManager.ActiveScenario.Map.GetQuadTile(placedTerrainObject.MapCoords).GetCell(new RCIntVector(0, 0)).MapCoords, placedTerrainObject.CellSize)
                                           - new RCNumVector(1, 1) / 2;
                foreach (QuadEntity affectedEntity in this.scenarioManager.ActiveScenario.GetVisibleEntities<QuadEntity>(terrObjRect))
                {
                    affectedEntity.RemoveFromMap();
                    if (affectedEntity.ElementType.CheckConstraints(this.scenarioManager.ActiveScenario, affectedEntity.LastKnownQuadCoords).Count != 0)
                    {
                        this.scenarioManager.ActiveScenario.RemoveEntity(affectedEntity);
                        affectedEntity.Dispose();
                    }
                    else
                    {
                        affectedEntity.AddToMap(this.scenarioManager.ActiveScenario.Map.GetQuadTile(affectedEntity.LastKnownQuadCoords));
                    }
                }
            }
            return placedTerrainObject != null;
        }

        /// <see cref="IMapEditorService.RemoveTerrainObject"/>
        public bool RemoveTerrainObject(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;
            foreach (ITerrainObject objToCheck in this.scenarioManager.ActiveScenario.Map.TerrainObjects.GetContents(navCellCoords))
            {
                if (!objToCheck.Type.IsExcluded(quadTileAtPos.MapCoords - objToCheck.MapCoords))
                {
                    this.mapEditor.RemoveTerrainObject(this.scenarioManager.ActiveScenario.Map, objToCheck);
                    return true;
                }
            }
            return false;
        }

        /// <see cref="IMapEditorService.PlaceStartLocation"/>
        public bool PlaceStartLocation(RCIntRectangle displayedArea, RCIntVector position, int playerIndex)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioManager.Metadata.GetElementType(StartLocation.STARTLOCATION_TYPE_NAME);
            RCIntVector objQuadSize = this.scenarioManager.ActiveScenario.Map.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.scenarioManager.ActiveScenario, topLeftQuadCoords).Count != 0) { return false; }

            /// Check if a start location with the given player index already exists.
            List<StartLocation> startLocations = this.scenarioManager.ActiveScenario.GetAllEntities<StartLocation>();
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
                this.scenarioManager.ActiveScenario.AddEntity(startLocation);
            }
            startLocation.AddToMap(this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.PlaceMineralField"/>
        public bool PlaceMineralField(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioManager.Metadata.GetElementType(MineralField.MINERALFIELD_TYPE_NAME);
            RCIntVector objQuadSize = this.scenarioManager.ActiveScenario.Map.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.scenarioManager.ActiveScenario, topLeftQuadCoords).Count != 0) { return false; }

            MineralField placedMineralField = new MineralField();
            this.scenarioManager.ActiveScenario.AddEntity(placedMineralField);
            placedMineralField.AddToMap(this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.PlaceVespeneGeyser"/>
        public bool PlaceVespeneGeyser(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioManager.Metadata.GetElementType(VespeneGeyser.VESPENEGEYSER_TYPE_NAME);
            RCIntVector objQuadSize = this.scenarioManager.ActiveScenario.Map.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.scenarioManager.ActiveScenario, topLeftQuadCoords).Count != 0) { return false; }

            VespeneGeyser placedVespeneGeyser = new VespeneGeyser();
            this.scenarioManager.ActiveScenario.AddEntity(placedVespeneGeyser);
            placedVespeneGeyser.AddToMap(this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.RemoveEntity"/>
        public bool RemoveEntity(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            foreach (Entity entity in this.scenarioManager.ActiveScenario.VisibleEntities.GetContents(navCellCoords))
            {
                entity.RemoveFromMap();
                this.scenarioManager.ActiveScenario.RemoveEntity(entity);
                entity.Dispose();
                return true;
            }
            return false;
        }

        /// <see cref="IMapEditorService.RemoveEntity"/>
        public bool ChangeResourceAmount(int objectID, int delta)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            MineralField mineralField = this.scenarioManager.ActiveScenario.GetEntity<MineralField>(objectID);
            VespeneGeyser vespeneGeyser = this.scenarioManager.ActiveScenario.GetEntity<VespeneGeyser>(objectID);
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

        #region View factory methods (TODO: move to ScenarioManagerBC)

        /// <summary>
        /// Creates a view of type IMapObjectPlacementView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapObjectPlacementView CreateMapObjectPlacementView(string mapObjectTypeName)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("There is no opened map!"); }
            if (mapObjectTypeName == null) { throw new ArgumentNullException("mapObjectTypeName"); }

            return new MapObjectPlacementView(mapObjectTypeName, this.timeScheduler);
        }

        /// <summary>
        /// Registers the implemented factory methods to the view factory.
        /// </summary>
        private void RegisterFactoryMethods()
        {
            this.viewFactoryRegistry.RegisterViewFactory<IMapObjectPlacementView, string>(this.CreateMapObjectPlacementView);
        }

        /// <summary>
        /// Unregisters the implemented factory methods from the view factory.
        /// </summary>
        private void UnregisterFactoryMethods()
        {
            this.viewFactoryRegistry.UnregisterViewFactory<IMapObjectPlacementView>();
        }

        #endregion View factory methods (TODO: move to ScenarioManagerBC)

        /// <summary>
        /// Reference to the RC.Engine.Maps.MapEditor component.
        /// </summary>
        private IMapEditor mapEditor;

        /// <summary>
        /// Reference to the registry interface of the RC.App.BizLogic.ViewFactory component.
        /// </summary>
        private IViewFactoryRegistry viewFactoryRegistry;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the scheduler of the map editor.
        /// </summary>
        private Scheduler timeScheduler;

        /// <summary>
        /// The elapsed time between frames in the map editor in frames.
        /// </summary>
        private const int MAPEDITOR_MS_PER_FRAMES = 40;
    }
}
