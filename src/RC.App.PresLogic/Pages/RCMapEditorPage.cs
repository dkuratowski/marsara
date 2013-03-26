using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.App.BizLogic;
using RC.Common.ComponentModel;
using RC.App.BizLogic.PublicInterfaces;
using RC.App.PresLogic.Panels;
using RC.App.PresLogic.Controls;

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
        public RCMapEditorPage(string fileName, string tilesetName, string defaultTerrain, RCIntVector mapSize)
            : this()
        {
            if (fileName == null) { throw new ArgumentNullException("fileName"); }
            if (tilesetName == null) { throw new ArgumentNullException("tilesetName"); }
            if (defaultTerrain == null) { throw new ArgumentNullException("defaultTerrain"); }
            if (mapSize == RCIntVector.Undefined) { throw new ArgumentNullException("mapSize"); }

            this.fileName = fileName;
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
            this.mapEditorBE = ComponentManager.GetInterface<IMapEditorBE>();
            this.currMapScrollDir = MapScrollDirection.NoScroll;
            this.timeSinceLastScroll = 0;
            this.activatorBtn = UIMouseButton.Undefined;
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
            UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnUpdateAfterMapLoaded);
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
        private void OnUpdateAfterMapLoaded(UIUpdateSystemEventArgs evtArgs)
        {
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnUpdateAfterMapLoaded);

            /// Create the necessary views.
            this.mapTerrainView = this.mapEditorBE.CreateMapTerrainView();
            this.tilesetView = this.mapEditorBE.CreateTileSetView();

            /// Create the map display control.
            this.mapDisplayBasic = new RCMapDisplayBasic(new RCIntVector(0, 0), UIWorkspace.Instance.WorkspaceSize, mapTerrainView, tilesetView);
            this.isotileDisplayEx = new RCIsoTileDisplay(this.mapDisplayBasic, this.mapTerrainView);
            this.objectPlacementDisplayEx = new RCObjectPlacementDisplay(this.isotileDisplayEx, this.mapTerrainView);
            this.mapDisplay = this.objectPlacementDisplayEx;
            this.mapDisplay.Started += this.OnMapDisplayStarted;
            this.mapDisplay.Start();
        }

        /// <summary>
        /// This method is called when the map display started successfully.
        /// </summary>
        private void OnMapDisplayStarted(object sender, EventArgs args)
        {
            this.mapDisplay.Started -= this.OnMapDisplayStarted;

            /// Attach the map display control to this page.
            this.Attach(this.mapDisplay);
            this.AttachSensitive(this.mapDisplay);

            /// Subscribe to the events of the appropriate mouse sensors.
            this.MouseSensor.Move += this.OnMouseMoveOverPage;
            this.mapDisplay.MouseSensor.Move += this.OnMouseMoveOverDisplay;
            this.mapDisplay.MouseSensor.ButtonDown += this.OnMouseDown;
            this.mapDisplay.MouseSensor.ButtonUp += this.OnMouseUp;

            /// Create and register the map editor panel.
            this.mapEditorPanel = new RCMapEditorPanel(new RCIntRectangle(213, 0, 107, 190),
                                                       new RCIntRectangle(0, 25, 90, 165),
                                                       UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToRight,
                                                       300, 300,
                                                       "RC.MapEditor.Sprites.CtrlPanel",
                                                       this.tilesetView);
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

                this.mapDisplay.Stopped += this.OnMapDisplayStopped;
                this.mapDisplay.Stop();
            }
        }

        /// <summary>
        /// This method is called when the map display stopped successfully.
        /// </summary>
        private void OnMapDisplayStopped(object sender, EventArgs args)
        {
            this.mapDisplay.Stopped -= this.OnMapDisplayStopped;

            this.mapEditorBE.CloseMap();
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
                this.objectPlacementDisplayEx.StartPlacingObject(
                    this.mapEditorBE.CreateTerrainObjectPlacementView(this.mapEditorPanel.SelectedItem),
                    this.mapDisplayBasic.TerrainObjectSprites);
            }
            else
            {
                this.objectPlacementDisplayEx.StopPlacingObject();
            }
        }

        /// <summary>
        /// This method is called when the palette listbox selection has been changed.
        /// </summary>
        private void OnSelectedItemChanged()
        {
            if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
            {
                this.objectPlacementDisplayEx.StopPlacingObject();
                this.objectPlacementDisplayEx.StartPlacingObject(
                    this.mapEditorBE.CreateTerrainObjectPlacementView(this.mapEditorPanel.SelectedItem),
                    this.mapDisplayBasic.TerrainObjectSprites);
            }
        }

        /// <summary>
        /// This method is called when the "Save" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnSaveMapPressed(UISensitiveObject sender)
        {
            /// TODO: implement map saving logic here
        }

        /// <summary>
        /// This method is called when the "Exit" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnExitPressed(UISensitiveObject sender)
        {
            this.MouseSensor.Move -= this.OnMouseMoveOverPage;
            this.mapDisplay.MouseSensor.ButtonDown -= this.OnMouseDown;
            this.mapDisplay.MouseSensor.ButtonUp -= this.OnMouseUp;
            this.mapDisplay.MouseSensor.Move -= this.OnMouseMoveOverDisplay;

            this.mapEditorPanel.EditModeChanged -= this.OnEditModeChanged;
            this.mapEditorPanel.SaveButton.Pressed -= this.OnSaveMapPressed;
            this.mapEditorPanel.ExitButton.Pressed -= this.OnExitPressed;

            this.StatusChanged += this.OnPageStatusChanged;
            this.Deactivate();
        }

        #endregion RCMapEditorPanel event handlers

        #region Mouse event handlers

        /// <summary>
        /// Called when the mouse cursor is moved over the area of the page.
        /// </summary>
        private void OnMouseMoveOverPage(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            MapScrollDirection newScrollDir = MapScrollDirection.NoScroll;

            if (evtArgs.Position.X == 0 || evtArgs.Position.X == this.Range.Width - 1 ||
                evtArgs.Position.Y == 0 || evtArgs.Position.Y == this.Range.Height - 1)
            {
                if (evtArgs.Position.X == 0 && evtArgs.Position.Y == 0) { newScrollDir = MapScrollDirection.NorthWest; }
                else if (evtArgs.Position.X == this.Range.Width - 1 && evtArgs.Position.Y == 0) { newScrollDir = MapScrollDirection.NorthEast; }
                else if (evtArgs.Position.X == this.Range.Width - 1 && evtArgs.Position.Y == this.Range.Height - 1) { newScrollDir = MapScrollDirection.SouthEast; }
                else if (evtArgs.Position.X == 0 && evtArgs.Position.Y == this.Range.Height - 1) { newScrollDir = MapScrollDirection.SouthWest; }
                else if (evtArgs.Position.X == 0) { newScrollDir = MapScrollDirection.West; }
                else if (evtArgs.Position.X == this.Range.Width - 1) { newScrollDir = MapScrollDirection.East; }
                else if (evtArgs.Position.Y == 0) { newScrollDir = MapScrollDirection.North; }
                else if (evtArgs.Position.Y == this.Range.Height - 1) { newScrollDir = MapScrollDirection.South; }
            }

            if (this.currMapScrollDir != newScrollDir)
            {
                this.currMapScrollDir = newScrollDir;
                if (this.currMapScrollDir != MapScrollDirection.NoScroll)
                {
                    UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                }
                else
                {
                    UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                }
            }
        }
        
        /// <summary>
        /// Called when the mouse cursor is moved over the area of the map display control.
        /// </summary>
        private void OnMouseMoveOverDisplay(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == UIMouseButton.Undefined && this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
            {
                if (this.mapTerrainView.GetTerrainObjectDisplayCoords(this.mapDisplay.DisplayedArea, evtArgs.Position) != RCIntVector.Undefined)
                {
                    this.objectPlacementDisplayEx.StopPlacingObject();
                }
                else
                {
                    if (!this.objectPlacementDisplayEx.PlacingObject)
                    {
                        this.objectPlacementDisplayEx.StartPlacingObject(
                            this.mapEditorBE.CreateTerrainObjectPlacementView(this.mapEditorPanel.SelectedItem),
                            this.mapDisplayBasic.TerrainObjectSprites);
                    }
                }
            }
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            this.timeSinceLastScroll += evtArgs.TimeSinceLastUpdate;
            if (this.timeSinceLastScroll > TIME_BETWEEN_MAP_SCROLLS)
            {
                this.timeSinceLastScroll = 0;
                if (this.currMapScrollDir == MapScrollDirection.North) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(0, -PIXELS_PER_SCROLLS)); }
                if (this.currMapScrollDir == MapScrollDirection.NorthEast) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, -PIXELS_PER_SCROLLS)); }
                if (this.currMapScrollDir == MapScrollDirection.East) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, 0)); }
                if (this.currMapScrollDir == MapScrollDirection.SouthEast) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, PIXELS_PER_SCROLLS)); }
                if (this.currMapScrollDir == MapScrollDirection.South) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(0, PIXELS_PER_SCROLLS)); }
                if (this.currMapScrollDir == MapScrollDirection.SouthWest) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, PIXELS_PER_SCROLLS)); }
                if (this.currMapScrollDir == MapScrollDirection.West) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, 0)); }
                if (this.currMapScrollDir == MapScrollDirection.NorthWest) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, -PIXELS_PER_SCROLLS)); }
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
                    this.mapEditorBE.DrawTerrain(this.mapDisplay.DisplayedArea, evtArgs.Position, this.mapEditorPanel.SelectedItem);
                }
                else if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
                {
                    this.mapEditorBE.PlaceTerrainObject(this.mapDisplay.DisplayedArea, evtArgs.Position, this.mapEditorPanel.SelectedItem);
                }
            }
            else if (this.activatorBtn == UIMouseButton.Undefined && evtArgs.Button == UIMouseButton.Right)
            {
                this.activatorBtn = evtArgs.Button;
                if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.PlaceTerrainObject)
                {
                    this.mapEditorBE.RemoveTerrainObject(this.mapDisplay.DisplayedArea, evtArgs.Position);
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

        #endregion Mouse event handlers

        /// <summary>
        /// Background task for initialize the loading of the map.
        /// </summary>
        private void LoadMapTask(object parameter)
        {
            /// TODO: Here we can choose between loading/creating later...
            this.mapEditorBE.NewMap(this.tilesetName, this.defaultTerrain, this.mapSize);
        }

        /// <summary>
        /// Reference to the map editor backend component.
        /// </summary>
        private IMapEditorBE mapEditorBE;

        /// <summary>
        /// Reference to the map terrain view.
        /// </summary>
        private IMapTerrainView mapTerrainView;

        /// <summary>
        /// Reference to the tileset view.
        /// </summary>
        private ITileSetView tilesetView;

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;

        /// <summary>
        /// The basic part of the map display.
        /// </summary>
        private RCMapDisplayBasic mapDisplayBasic;

        /// <summary>
        /// Extension of the map display that displays the isometric tiles.
        /// </summary>
        private RCIsoTileDisplay isotileDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the object placement boxes.
        /// </summary>
        private RCObjectPlacementDisplay objectPlacementDisplayEx;

        /// <summary>
        /// Reference to the panel with the controls.
        /// </summary>
        private RCMapEditorPanel mapEditorPanel;

        /// <summary>
        /// Reference to the background task that loads the map.
        /// </summary>
        private IUIBackgroundTask loadMapTask;

        /// <summary>
        /// The current scrolling direction.
        /// </summary>
        private MapScrollDirection currMapScrollDir;

        /// <summary>
        /// Elapsed time since last scroll in milliseconds.
        /// </summary>
        private int timeSinceLastScroll;

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of the map display.
        /// </summary>
        private UIMouseButton activatorBtn;

        /// <summary>
        /// The time between map-scrolling operations.
        /// </summary>
        private const int TIME_BETWEEN_MAP_SCROLLS = 20;

        /// <summary>
        /// The number of pixels to scroll in map-scrolling operations.
        /// </summary>
        private const int PIXELS_PER_SCROLLS = 5;

        #region Map editor settings

        /// <summary>
        /// The name of the file of the map to be loaded/created.
        /// </summary>
        private string fileName;

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
