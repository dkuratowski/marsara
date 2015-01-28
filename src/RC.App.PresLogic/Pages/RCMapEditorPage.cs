using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.App.BizLogic;
using RC.Common.ComponentModel;
using RC.App.PresLogic.Panels;
using RC.App.PresLogic.Controls;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;

namespace RC.App.PresLogic.Pages
{
    /// <summary>
    /// This page will display the map editor.
    /// </summary>
    public class RCMapEditorPage : RCAppPage
    {
        /// <summary>
        /// Constructs an RCMapEditorPage instance for create and edit a new map.
        /// </summary>
        public RCMapEditorPage(string fileName, string mapName, string tilesetName, string defaultTerrain, RCIntVector mapSize)
            : this()
        {
            if (fileName == null) { throw new ArgumentNullException("fileName"); }
            if (mapName == null) { throw new ArgumentNullException("mapName"); }
            if (tilesetName == null) { throw new ArgumentNullException("tilesetName"); }
            if (defaultTerrain == null) { throw new ArgumentNullException("defaultTerrain"); }
            if (mapSize == RCIntVector.Undefined) { throw new ArgumentNullException("mapSize"); }

            this.fileName = fileName;
            this.mapName = mapName;
            this.tilesetName = tilesetName;
            this.defaultTerrain = defaultTerrain;
            this.mapSize = mapSize;
        }

        /// <summary>
        /// Constructs an RCMapEditorPage instance for load and edit an existing map.
        /// </summary>
        public RCMapEditorPage(string fileName)
            : this()
        {
            if (fileName == null) { throw new ArgumentNullException("fileName"); }

            this.fileName = fileName;
        }

        /// <summary>
        /// Common initialization tasks.
        /// </summary>
        private RCMapEditorPage() : base()
        {
            this.mapEditorService = ComponentManager.GetInterface<IMapEditorService>();
            this.scrollService = ComponentManager.GetInterface<IScrollService>();
            this.viewService = ComponentManager.GetInterface<IViewService>();
            this.activatorBtn = UIMouseButton.Undefined;
            this.mouseHandler = null;
        }

        #region RCMapEditorPage state handling

        /// <see cref="RCAppPage.OnActivated"/>
        protected override void OnActivated()
        {
            this.loadMapTask = UITaskManager.StartParallelTask(this.LoadMapTask, "LoadMapTask");
            this.loadMapTask.Finished += this.OnLoadMapFinished;
            this.loadMapTask.Failed += this.OnLoadMapFailed;
        }

        /// <summary>
        /// This method is called if loading the map has been finished successfully.
        /// </summary>
        private void OnLoadMapFinished(IUIBackgroundTask sender, object message)
        {
            /// Unsubscribe from the events of the background task.
            this.loadMapTask.Finished -= this.OnLoadMapFinished;
            this.loadMapTask.Failed -= this.OnLoadMapFailed;

            /// To avoid recursive call on the UITaskManager.
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnUpdateAfterMapLoaded;
        }

        /// <summary>
        /// This method is called if loading the map has been failed.
        /// </summary>
        private void OnLoadMapFailed(IUIBackgroundTask sender, object message)
        {
            this.loadMapTask.Finished -= this.OnLoadMapFinished;
            this.loadMapTask.Failed -= this.OnLoadMapFailed;

            throw (Exception)message;
        }

        /// <summary>
        /// To avoid recursive call on the UITaskManager.
        /// </summary>
        private void OnUpdateAfterMapLoaded()
        {
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnUpdateAfterMapLoaded;

            /// Create the map display control.
            this.mapDisplayBasic = new RCMapDisplayBasic(new RCIntVector(0, 0), UIWorkspace.Instance.WorkspaceSize - new RCIntVector(97, 0));
            //this.mapWalkabilityDisplay = new RCMapWalkabilityDisplay(this.mapDisplayBasic);
            this.mapObjectDisplayEx = new RCMapObjectDisplay(this.mapDisplayBasic);
            this.isotileDisplayEx = new RCIsoTileDisplay(this.mapObjectDisplayEx);
            this.objectPlacementDisplayEx = new RCObjectPlacementDisplay(this.isotileDisplayEx);
            this.resourceAmountDisplayEx = new RCResourceAmountDisplay(this.objectPlacementDisplayEx);
            this.mapDisplay = this.resourceAmountDisplayEx;
            this.mapDisplay.ConnectorOperationFinished += this.OnMapDisplayConnected;
            this.mapDisplay.Connect();

            /// Attach the map display to the scroll service.
            this.scrollService.AttachWindow(this.mapDisplay.PixelSize);

            /// Create the necessary views.
            this.mapTerrainView = this.viewService.CreateView<IMapTerrainView>();
            this.mapObjectView = this.viewService.CreateView<IMapObjectView>();
        }

        /// <summary>
        /// This method is called when the map display connected successfully.
        /// </summary>
        private void OnMapDisplayConnected(IGameConnector sender)
        {
            this.mapDisplay.ConnectorOperationFinished -= this.OnMapDisplayConnected;

            /// Attach the map display control to this page.
            this.Attach(this.mapDisplay);
            this.AttachSensitive(this.mapDisplay);

            /// Subscribe to the events of the appropriate mouse sensors & create the mouse handler.
            this.mouseHandler = new MapEditorMouseHandler(this, this.mapDisplay);
            this.mapDisplay.MouseSensor.Move += this.OnMouseMoveOverDisplay;
            this.mapDisplay.MouseSensor.ButtonDown += this.OnMouseDown;
            this.mapDisplay.MouseSensor.ButtonUp += this.OnMouseUp;
            this.mapDisplay.MouseSensor.Wheel += this.OnMouseWheel;

            /// Create and register the map editor panel.
            this.mapEditorPanel = new RCMapEditorPanel(new RCIntRectangle(this.Range.Right - 97, 0, 97, 200),
                                                       new RCIntRectangle(0, 0, 97, 200),
                                                       UIPanel.ShowMode.Appear, UIPanel.HideMode.Disappear,
                                                       0, 0,
                                                       "RC.MapEditor.Sprites.CtrlPanel");
            this.RegisterPanel(this.mapEditorPanel);

            /// Subscribe to the events of the map editor panel.
            this.mapEditorPanel.EditModeChanged += this.OnEditModeChanged;
            this.mapEditorPanel.SelectedItemChanged += this.OnSelectedItemChanged;
            this.mapEditorPanel.SaveButton.Pressed += this.OnSaveMapPressed;
            this.mapEditorPanel.ExitButton.Pressed += this.OnExitPressed;
            this.isotileDisplayEx.HighlightIsoTile = this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.DrawTerrain;

            /// Show the map editor panel.
            this.mapEditorPanel.Show();
            this.mapEditorPanel.ResetControls();
        }

        /// <summary>
        /// Called when the status of this page has changed.
        /// </summary>
        /// <param name="sender">Reference to this page.</param>
        /// <param name="newState">The new state of this page.</param>
        private void OnPageStatusChanged(UIPage sender, Status newState)
        {
            if (sender != this) { throw new InvalidOperationException("Unexpected event!"); }

            if (newState == Status.Inactive)
            {
                this.StatusChanged -= this.OnPageStatusChanged;

                this.mapDisplay.ConnectorOperationFinished += this.OnMapDisplayDisconnected;
                this.mapDisplay.Disconnect();
            }
        }

        /// <summary>
        /// This method is called when the map display disconnected successfully.
        /// </summary>
        private void OnMapDisplayDisconnected(IGameConnector sender)
        {
            this.mapDisplay.ConnectorOperationFinished -= this.OnMapDisplayDisconnected;

            this.mapEditorService.CloseMap();
            UIRoot.Instance.GraphicsPlatform.RenderLoop.Stop();
        }

        #endregion RCMapEditorPage state handling

        #region RCMapEditorPanel event handlers

        /// <summary>
        /// This method is called when the edit mode selection has been changed.
        /// </summary>
        private void OnEditModeChanged()
        {
            this.isotileDisplayEx.HighlightIsoTile = this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.DrawTerrain;
            
            if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
            {
                this.mouseHandler.StartPlacingObject(
                    this.viewService.CreateView<ITerrainObjectPlacementView, string>(this.mapEditorPanel.SelectedItem),
                    this.mapDisplayBasic.TerrainObjectSprites);
            }
            else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceStartLocation)
            {
                this.mouseHandler.StartPlacingObject(
                    this.viewService.CreateView<IMapObjectPlacementView, string>(STARTLOCATION_NAME),
                    this.mapObjectDisplayEx.GetMapObjectSprites((PlayerEnum)(this.mapEditorPanel.SelectedIndex)));
            }
            else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceResource)
            {
                this.mouseHandler.StartPlacingObject(
                    this.viewService.CreateView<IMapObjectPlacementView, string>(this.mapEditorPanel.SelectedItem),
                    this.mapObjectDisplayEx.GetMapObjectSprites(PlayerEnum.Neutral));
            }
            else
            {
                this.mouseHandler.StopPlacingObject();
            }
        }

        /// <summary>
        /// This method is called when the palette listbox selection has been changed.
        /// </summary>
        private void OnSelectedItemChanged()
        {
            if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
            {
                this.mouseHandler.StartPlacingObject(
                    this.viewService.CreateView<ITerrainObjectPlacementView, string>(this.mapEditorPanel.SelectedItem),
                    this.mapDisplayBasic.TerrainObjectSprites);
            }
            else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceStartLocation)
            {
                this.mouseHandler.StartPlacingObject(
                    this.viewService.CreateView<IMapObjectPlacementView, string>(STARTLOCATION_NAME),
                    this.mapObjectDisplayEx.GetMapObjectSprites((PlayerEnum)(this.mapEditorPanel.SelectedIndex)));
            }
            else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceResource)
            {
                this.mouseHandler.StartPlacingObject(
                    this.viewService.CreateView<IMapObjectPlacementView, string>(this.mapEditorPanel.SelectedItem),
                    this.mapObjectDisplayEx.GetMapObjectSprites(PlayerEnum.Neutral));
            }
        }

        /// <summary>
        /// Used during save map task.
        /// </summary>
        /// TODO: display a "Please wait..." dialog instead!!!
        private UISensitiveObject workspaceTmp = null;

        /// <summary>
        /// This method is called when the "Save" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnSaveMapPressed(UISensitiveObject sender)
        {
            // TODO: display a "Please wait..." dialog instead!!!
            this.workspaceTmp = this.SensitiveParent;
            this.workspaceTmp.DetachSensitive(this);

            this.saveMapTask = UITaskManager.StartParallelTask((param) => this.mapEditorService.SaveMap(this.fileName), "SaveMapTask");
            this.saveMapTask.Finished += this.OnSaveMapFinished;
            this.saveMapTask.Failed += this.OnSaveMapFailed;
        }

        /// <summary>
        /// This method is called if saving the map has been finished successfully.
        /// </summary>
        private void OnSaveMapFinished(IUIBackgroundTask sender, object message)
        {
            /// Unsubscribe from the events of the background task.
            this.saveMapTask.Finished -= this.OnSaveMapFinished;
            this.saveMapTask.Failed -= this.OnSaveMapFailed;

            // TODO: display a "Please wait..." dialog instead!!!
            this.workspaceTmp.AttachSensitive(this);
            this.workspaceTmp = null;
        }

        /// <summary>
        /// This method is called if saving the map has been failed.
        /// </summary>
        private void OnSaveMapFailed(IUIBackgroundTask sender, object message)
        {
            this.saveMapTask.Finished -= this.OnSaveMapFinished;
            this.saveMapTask.Failed -= this.OnSaveMapFailed;

            // TODO: display a "Please wait..." dialog instead!!!
            this.workspaceTmp.AttachSensitive(this);
            this.workspaceTmp = null;

            throw (Exception)message;
        }

        /// <summary>
        /// This method is called when the "Exit" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnExitPressed(UISensitiveObject sender)
        {
            this.mouseHandler.Inactivate();
            this.mapDisplay.MouseSensor.Wheel -= this.OnMouseWheel;
            this.mapDisplay.MouseSensor.ButtonDown -= this.OnMouseDown;
            this.mapDisplay.MouseSensor.ButtonUp -= this.OnMouseUp;
            this.mapDisplay.MouseSensor.Move -= this.OnMouseMoveOverDisplay;

            this.mapEditorPanel.EditModeChanged -= this.OnEditModeChanged;
            this.mapEditorPanel.SelectedItemChanged -= this.OnSelectedItemChanged;
            this.mapEditorPanel.SaveButton.Pressed -= this.OnSaveMapPressed;
            this.mapEditorPanel.ExitButton.Pressed -= this.OnExitPressed;

            this.StatusChanged += this.OnPageStatusChanged;
            this.Deactivate();
        }

        #endregion RCMapEditorPanel event handlers

        #region Mouse event handlers
        
        /// <summary>
        /// Called when the mouse cursor is moved over the area of the map display control.
        /// </summary>
        private void OnMouseMoveOverDisplay(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == UIMouseButton.Undefined && this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
            {
                if (this.mapTerrainView.GetTerrainObjectDisplayCoords(evtArgs.Position) != RCIntVector.Undefined)
                {
                    this.mouseHandler.StopPlacingObject();
                }
                else
                {
                    if (!this.mouseHandler.IsPlacingObject)
                    {
                        this.mouseHandler.StartPlacingObject(
                            this.viewService.CreateView<ITerrainObjectPlacementView, string>(this.mapEditorPanel.SelectedItem),
                            this.mapDisplayBasic.TerrainObjectSprites);
                    }
                }
            }
            else if (this.activatorBtn == UIMouseButton.Undefined && this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceResource)
            {
                int objectID = this.mapObjectView.GetMapObjectID(evtArgs.Position);
                if (objectID != -1)
                {
                    this.mouseHandler.StopPlacingObject();
                    if (objectID != this.resourceAmountDisplayEx.MapObjectID)
                    {
                        this.resourceAmountDisplayEx.StopReadingMapObject();
                        this.resourceAmountDisplayEx.StartReadingMapObject(objectID);
                    }
                }
                else
                {
                    this.resourceAmountDisplayEx.StopReadingMapObject();
                    if (!this.mouseHandler.IsPlacingObject)
                    {
                        this.mouseHandler.StartPlacingObject(
                            this.viewService.CreateView<IMapObjectPlacementView, string>(this.mapEditorPanel.SelectedItem),
                            this.mapObjectDisplayEx.GetMapObjectSprites(PlayerEnum.Neutral));
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been pushed over the page.
        /// </summary>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == UIMouseButton.Undefined && evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = evtArgs.Button;
                if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.DrawTerrain)
                {
                    this.mapEditorService.DrawTerrain(evtArgs.Position, this.mapEditorPanel.SelectedItem);
                }
                else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
                {
                    this.mapEditorService.PlaceTerrainObject(evtArgs.Position, this.mapEditorPanel.SelectedItem);
                }
                else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceStartLocation)
                {
                    this.mapEditorService.PlaceStartLocation(evtArgs.Position, this.mapEditorPanel.SelectedIndex);
                }
                else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceResource)
                {
                    if (this.mapEditorPanel.SelectedItem == RCMapEditorPage.MINERALFIELD_NAME)
                    {
                        this.mapEditorService.PlaceMineralField(evtArgs.Position);
                    }
                    else if (this.mapEditorPanel.SelectedItem == RCMapEditorPage.VESPENEGEYSER_NAME)
                    {
                        this.mapEditorService.PlaceVespeneGeyser(evtArgs.Position);
                    }
                }
            }
            else if (this.activatorBtn == UIMouseButton.Undefined && evtArgs.Button == UIMouseButton.Right)
            {
                this.activatorBtn = evtArgs.Button;
                if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
                {
                    this.mapEditorService.RemoveTerrainObject(evtArgs.Position);
                }
                else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceStartLocation ||
                         this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceResource)
                {
                    if (this.mapEditorService.RemoveEntity(evtArgs.Position))
                    {
                        this.resourceAmountDisplayEx.StopReadingMapObject();
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the page.
        /// </summary>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == evtArgs.Button)
            {
                this.activatorBtn = UIMouseButton.Undefined;
            }
        }

        /// <summary>
        /// Called when the mouse wheel has been rotated.
        /// </summary>
        private void OnMouseWheel(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == UIMouseButton.Undefined && this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceResource)
            {
                int objectID = this.mapObjectView.GetMapObjectID(evtArgs.Position);
                if (objectID != -1)
                {
                    this.mapEditorService.ChangeResourceAmount(objectID, Math.Sign(evtArgs.WheelDelta) * RESOURCE_AMOUNT_DELTA);
                }
            }
        }

        #endregion Mouse event handlers

        /// <summary>
        /// Background task for initialize or load the map.
        /// </summary>
        private void LoadMapTask(object parameter)
        {
            if (this.mapName != null)
            {
                /// Create a new map.
                this.mapEditorService.NewMap(this.mapName, this.tilesetName, this.defaultTerrain, this.mapSize);
            }
            else
            {
                /// Load an existing map.
                this.mapEditorService.LoadMap(this.fileName);
            }
        }

        /// <summary>
        /// Reference to the map editor service.
        /// </summary>
        private IMapEditorService mapEditorService;

        /// <summary>
        /// Reference to the scroll service.
        /// </summary>
        private IScrollService scrollService;

        /// <summary>
        /// Reference to the view service.
        /// </summary>
        private IViewService viewService;

        /// <summary>
        /// Reference to the map terrain view.
        /// </summary>
        private IMapTerrainView mapTerrainView;

        /// <summary>
        /// Reference to the map object view.
        /// </summary>
        private IMapObjectView mapObjectView;

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;

        /// <summary>
        /// The basic part of the map display.
        /// </summary>
        private RCMapDisplayBasic mapDisplayBasic;

        /// <summary>
        /// Extension of the map display that displays the walkability of map cells.
        /// </summary>
        private RCMapWalkabilityDisplay mapWalkabilityDisplay;

        /// <summary>
        /// Extension of the map display that displays the map objects.
        /// </summary>
        private RCMapObjectDisplay mapObjectDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the isometric tiles.
        /// </summary>
        private RCIsoTileDisplay isotileDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the object placement boxes.
        /// </summary>
        private RCObjectPlacementDisplay objectPlacementDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the amount of resource in a mineral field or vespene geyser.
        /// </summary>
        private RCResourceAmountDisplay resourceAmountDisplayEx;

        /// <summary>
        /// Reference to the panel with the controls.
        /// </summary>
        private RCMapEditorPanel mapEditorPanel;

        /// <summary>
        /// Reference to the background task that loads the map.
        /// </summary>
        private IUIBackgroundTask loadMapTask;

        /// <summary>
        /// Reference to the background task that saves the map.
        /// </summary>
        private IUIBackgroundTask saveMapTask;

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of the map display.
        /// </summary>
        private UIMouseButton activatorBtn;

        /// <summary>
        /// Reference to the object that handles the mouse events.
        /// </summary>
        private MapEditorMouseHandler mouseHandler;

        /// <summary>
        /// Name of the scenario element types that can be placed on the scenario.
        /// </summary>
        public const string STARTLOCATION_NAME = "StartLocation";
        public const string MINERALFIELD_NAME = "MineralField";
        public const string VESPENEGEYSER_NAME = "VespeneGeyser";
        public const int RESOURCE_AMOUNT_DELTA = 10;

        #region Map editor settings

        /// <summary>
        /// The name of the file of the map to be loaded/created.
        /// </summary>
        private string fileName;

        /// <summary>
        /// The name of the new map, or null if an existing map is loaded.
        /// </summary>
        private string mapName;

        /// <summary>
        /// The name of the tileset of the new map, or null if an existing map is loaded.
        /// </summary>
        private string tilesetName;

        /// <summary>
        /// The name of the default terrain of the new map, or null if an existing map is loaded.
        /// </summary>
        private string defaultTerrain;

        /// <summary>
        /// The size of the new map, or RCIntVector.Undefined if an existing map is loaded.
        /// </summary>
        private RCIntVector mapSize;

        #endregion Map editor settings
    }
}
