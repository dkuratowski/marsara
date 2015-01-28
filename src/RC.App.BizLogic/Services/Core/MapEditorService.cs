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
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();
            this.mapEditor = ComponentManager.GetInterface<IMapEditor>();
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
            this.timeScheduler.AddScheduledFunction(() => { if (this.AnimationsUpdated != null) { this.AnimationsUpdated(); } });
        }

        /// <see cref="IMapEditorService.NewMap"/>
        public void LoadMap(string filename)
        {
            this.scenarioManager.OpenScenario(filename);
            this.timeScheduler = new Scheduler(MAPEDITOR_MS_PER_FRAMES);
            this.timeScheduler.AddScheduledFunction(this.scenarioManager.ActiveScenario.UpdateAnimations);
            this.timeScheduler.AddScheduledFunction(() => { if (this.AnimationsUpdated != null) { this.AnimationsUpdated(); } });
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
                this.timeScheduler.Dispose();
                this.timeScheduler = null;
                this.scenarioManager.CloseScenario();
            }
        }

        /// <see cref="IMapEditorService.AnimationsUpdated"/>
        public event Action AnimationsUpdated;

        /// <see cref="IMapEditorService.DrawTerrain"/>
        public void DrawTerrain(RCIntVector position, string terrainType)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }

            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IIsoTile isotile = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentIsoTile;

            IEnumerable<IIsoTile> affectedIsoTiles = this.mapEditor.DrawTerrain(this.scenarioManager.ActiveScenario.Map, isotile,
                                                                                this.scenarioManager.ActiveScenario.Map.Tileset.GetTerrainType(terrainType));

            foreach (IIsoTile affectedIsoTile in affectedIsoTiles)
            {
                RCNumRectangle isoTileRect = new RCNumRectangle(affectedIsoTile.GetCellMapCoords(new RCIntVector(0, 0)), affectedIsoTile.CellSize)
                                           - new RCNumVector(1, 1) / 2;
                foreach (QuadEntity affectedEntity in this.scenarioManager.ActiveScenario.GetEntitiesOnMap<QuadEntity>(isoTileRect))
                {
                    this.scenarioManager.ActiveScenario.DetachEntityFromMap(affectedEntity);
                    if (affectedEntity.ElementType.CheckConstraints(this.scenarioManager.ActiveScenario, affectedEntity.LastKnownQuadCoords).Count != 0)
                    {
                        this.scenarioManager.ActiveScenario.RemoveEntityFromScenario(affectedEntity);
                        affectedEntity.Dispose();
                    }
                    else
                    {
                        this.scenarioManager.ActiveScenario.AttachEntityToMap(affectedEntity, this.scenarioManager.ActiveScenario.Map.GetQuadTile(affectedEntity.LastKnownQuadCoords));
                    }
                }
            }
        }

        /// <see cref="IMapEditorService.PlaceTerrainObject"/>
        public bool PlaceTerrainObject(RCIntVector position, string terrainObject)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (terrainObject == null) { throw new ArgumentNullException("terrainObject"); }

            ITerrainObjectType terrainObjType = this.scenarioManager.ActiveScenario.Map.Tileset.GetTerrainObjectType(terrainObject);
            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
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
                foreach (QuadEntity affectedEntity in this.scenarioManager.ActiveScenario.GetEntitiesOnMap<QuadEntity>(terrObjRect))
                {
                    this.scenarioManager.ActiveScenario.DetachEntityFromMap(affectedEntity);
                    if (affectedEntity.ElementType.CheckConstraints(this.scenarioManager.ActiveScenario, affectedEntity.LastKnownQuadCoords).Count != 0)
                    {
                        this.scenarioManager.ActiveScenario.RemoveEntityFromScenario(affectedEntity);
                        affectedEntity.Dispose();
                    }
                    else
                    {
                        this.scenarioManager.ActiveScenario.AttachEntityToMap(affectedEntity, this.scenarioManager.ActiveScenario.Map.GetQuadTile(affectedEntity.LastKnownQuadCoords));
                    }
                }
            }
            return placedTerrainObject != null;
        }

        /// <see cref="IMapEditorService.RemoveTerrainObject"/>
        public bool RemoveTerrainObject(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            ITerrainObject objToCheck = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile.TerrainObject;
            if (objToCheck != null)
            {
                this.mapEditor.RemoveTerrainObject(this.scenarioManager.ActiveScenario.Map, objToCheck);
                return true;
            }
            return false;
        }

        /// <see cref="IMapEditorService.PlaceStartLocation"/>
        public bool PlaceStartLocation(RCIntVector position, int playerIndex)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioManager.Metadata.GetElementType(StartLocation.STARTLOCATION_TYPE_NAME);
            RCIntVector objQuadSize = this.scenarioManager.ActiveScenario.Map.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.scenarioManager.ActiveScenario, topLeftQuadCoords).Count != 0) { return false; }

            /// Check if a start location with the given player index already exists.
            HashSet<StartLocation> startLocations = this.scenarioManager.ActiveScenario.GetAllEntities<StartLocation>();
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
                this.scenarioManager.ActiveScenario.DetachEntityFromMap(startLocation);
            }
            else
            {
                startLocation = new StartLocation(playerIndex);
                this.scenarioManager.ActiveScenario.AddEntityToScenario(startLocation);
            }
            this.scenarioManager.ActiveScenario.AttachEntityToMap(startLocation, this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.PlaceMineralField"/>
        public bool PlaceMineralField(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioManager.Metadata.GetElementType(MineralField.MINERALFIELD_TYPE_NAME);
            RCIntVector objQuadSize = this.scenarioManager.ActiveScenario.Map.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.scenarioManager.ActiveScenario, topLeftQuadCoords).Count != 0) { return false; }

            MineralField placedMineralField = new MineralField();
            this.scenarioManager.ActiveScenario.AddEntityToScenario(placedMineralField);
            this.scenarioManager.ActiveScenario.AttachEntityToMap(placedMineralField, this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.PlaceVespeneGeyser"/>
        public bool PlaceVespeneGeyser(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            IQuadTile quadTileAtPos = this.scenarioManager.ActiveScenario.Map.GetCell(navCellCoords).ParentQuadTile;

            IScenarioElementType objectType = this.scenarioManager.Metadata.GetElementType(VespeneGeyser.VESPENEGEYSER_TYPE_NAME);
            RCIntVector objQuadSize = this.scenarioManager.ActiveScenario.Map.CellToQuadSize(objectType.Area.Read());
            RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
            if (objectType.CheckConstraints(this.scenarioManager.ActiveScenario, topLeftQuadCoords).Count != 0) { return false; }

            VespeneGeyser placedVespeneGeyser = new VespeneGeyser();
            this.scenarioManager.ActiveScenario.AddEntityToScenario(placedVespeneGeyser);
            this.scenarioManager.ActiveScenario.AttachEntityToMap(placedVespeneGeyser, this.scenarioManager.ActiveScenario.Map.GetQuadTile(topLeftQuadCoords));
            return true;
        }

        /// <see cref="IMapEditorService.RemoveEntity"/>
        public bool RemoveEntity(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector navCellCoords = this.mapWindowBC.AttachedWindow.WindowToMapCoords(position).Round();
            foreach (Entity entity in this.scenarioManager.ActiveScenario.GetEntitiesOnMap<Entity>(navCellCoords))
            {
                this.scenarioManager.ActiveScenario.DetachEntityFromMap(entity);
                this.scenarioManager.ActiveScenario.RemoveEntityFromScenario(entity);
                entity.Dispose();
                return true;
            }
            return false;
        }

        /// <see cref="IMapEditorService.RemoveEntity"/>
        public bool ChangeResourceAmount(int objectID, int delta)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
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

        /// <summary>
        /// Reference to the RC.Engine.Maps.MapEditor component.
        /// </summary>
        private IMapEditor mapEditor;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;

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
