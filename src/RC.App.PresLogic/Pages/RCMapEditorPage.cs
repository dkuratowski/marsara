using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.App.BizLogic;
using RC.Common.ComponentModel;

namespace RC.App.PresLogic
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
            this.mapEditor = ComponentManager.GetInterface<IMapEditor>();
            this.mapDisplay = new RCMapDisplay(new RCIntVector(0, 0), UIWorkspace.Instance.WorkspaceSize);
            this.Attach(this.mapDisplay);

            this.mapEditorPanel = new RCMapEditorPanel(new RCIntRectangle(213, 0, 107, 190),
                                                       new RCIntRectangle(0, 25, 90, 165),
                                                       UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToRight,
                                                       300, 300,
                                                       "RC.MapEditor.Sprites.CtrlPanel");
            this.RegisterPanel(this.mapEditorPanel);
            this.currMapScrollDir = MapScrollDirection.NoScroll;
            this.timeSinceLastScroll = 0;
            this.lastKnownMousePosition = RCIntVector.Undefined;
            this.activatorBtn = UIMouseButton.Undefined;
        }

        /// <see cref="RCAppPage.OnActivated"/>
        protected override void OnActivated()
        {
            this.mapDisplay.TextIfNoMap = "Loading...";
            this.loadMapTask = UITaskManager.StartParallelTask(this.LoadMapTask, "LoadMapTask");
            this.loadMapTask.Finished += this.LoadMapFinished;
            this.loadMapTask.Failed += this.LoadMapFailed;
            //this.mapEditorPanel.Show();
        }

        /// <summary>
        /// Background task for initialize the loading of the map.
        /// </summary>
        private void LoadMapTask(object parameter)
        {
            /// TODO: Here we can choose between loading/creating later...
            this.mapEditor.CreateMap(this.tilesetName, this.defaultTerrain, this.mapSize);
        }

        /// <summary>
        /// This method is called if loading the map has been finished successfully.
        /// </summary>
        private void LoadMapFinished(IUIBackgroundTask sender, object message)
        {
            this.loadMapTask.Finished -= this.LoadMapFinished;
            this.loadMapTask.Failed -= this.LoadMapFailed;

            this.mapDisplay.DisplayingMap = true;
            this.MouseSensor.Move += this.OnMouseMove;
            this.MouseSensor.ButtonDown += this.OnMouseDown;
            this.MouseSensor.ButtonUp += this.OnMouseUp;

            this.mapEditorPanel.EditModeChanged += this.OnEditModeChanged;
            this.mapEditorPanel.SaveButton.Pressed += this.OnSaveMapPressed;
            this.mapEditorPanel.ExitButton.Pressed += this.OnExitPressed;
            this.mapEditorPanel.Show();
            this.mapEditorPanel.ResetControls();
        }

        /// <summary>
        /// This method is called if loading the map has been failed.
        /// </summary>
        private void LoadMapFailed(IUIBackgroundTask sender, object message)
        {
            this.loadMapTask.Finished -= this.LoadMapFinished;
            this.loadMapTask.Failed -= this.LoadMapFailed;

            throw (Exception)message;
        }

        /// <summary>
        /// This method is called when the "Save" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnSaveMapPressed(UISensitiveObject sender)
        {
        }

        /// <summary>
        /// This method is called when the "Exit" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnExitPressed(UISensitiveObject sender)
        {
            this.MouseSensor.Move -= this.OnMouseMove;
            this.MouseSensor.ButtonDown -= this.OnMouseDown;
            this.MouseSensor.ButtonUp -= this.OnMouseUp;

            this.mapEditorPanel.EditModeChanged -= this.OnEditModeChanged;
            this.mapEditorPanel.SaveButton.Pressed -= this.OnSaveMapPressed;
            this.mapEditorPanel.ExitButton.Pressed -= this.OnExitPressed;

            this.StatusChanged += this.OnPageStatusChanged;
            this.Deactivate();
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
                this.mapDisplay.DisplayingMap = false;
                this.StatusChanged -= this.OnPageStatusChanged;
                UIRoot.Instance.GraphicsPlatform.RenderLoop.Stop();
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
                    this.mapEditor.DrawTerrain(this.mapDisplay.TransformPixelToNavCoords(this.lastKnownMousePosition - this.mapDisplay.Position),
                                               this.mapEditorPanel.SelectedItem);
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the page.
        /// </summary>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == UIMouseButton.Left && evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = UIMouseButton.Undefined;
            }
        }

        /// <summary>
        /// Called when the mouse cursor is moved over the area of the page.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
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

            if (this.lastKnownMousePosition != evtArgs.Position)
            {
                this.lastKnownMousePosition = evtArgs.Position;
                if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.DrawTerrain)
                {
                    this.mapDisplay.HighlightIsoTileAt(this.lastKnownMousePosition - this.mapDisplay.Position);
                }
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
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            this.timeSinceLastScroll += evtArgs.TimeSinceLastUpdate;
            if (this.timeSinceLastScroll > TIME_BETWEEN_MAP_SCROLLS)
            {
                this.timeSinceLastScroll = 0;
                if (this.currMapScrollDir == MapScrollDirection.North) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(0, -1)); }
                if (this.currMapScrollDir == MapScrollDirection.NorthEast) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(1, -1)); }
                if (this.currMapScrollDir == MapScrollDirection.East) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(1, 0)); }
                if (this.currMapScrollDir == MapScrollDirection.SouthEast) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(1, 1)); }
                if (this.currMapScrollDir == MapScrollDirection.South) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(0, 1)); }
                if (this.currMapScrollDir == MapScrollDirection.SouthWest) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(-1, 1)); }
                if (this.currMapScrollDir == MapScrollDirection.West) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(-1, 0)); }
                if (this.currMapScrollDir == MapScrollDirection.NorthWest) { this.mapDisplay.ScrollTo(this.mapDisplay.DisplayedArea.Location + new RCIntVector(-1, -1)); }
                
                if (this.mapEditorPanel.SelectedMode == RCMapEditorPanel.EditMode.DrawTerrain)
                {
                    this.mapDisplay.HighlightIsoTileAt(this.lastKnownMousePosition - this.mapDisplay.Position);
                }
            }
        }

        /// <summary>
        /// This method is called when the edit mode selection has been changed.
        /// </summary>
        private void OnEditModeChanged()
        {
            if (this.mapEditorPanel.SelectedMode != RCMapEditorPanel.EditMode.DrawTerrain) { this.mapDisplay.UnhighlightIsoTile(); }
        }

        /// <summary>
        /// Reference to the map editor component.
        /// </summary>
        private IMapEditor mapEditor;

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;

        /// <summary>
        /// Reference to the panel with the controls.
        /// </summary>
        private RCMapEditorPanel mapEditorPanel;

        /// <summary>
        /// Reference to the background task that loads the map.
        /// </summary>
        private IUIBackgroundTask loadMapTask;

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

        /// <summary>
        /// The current scrolling direction.
        /// </summary>
        private MapScrollDirection currMapScrollDir;

        /// <summary>
        /// Elapsed time since last scroll in milliseconds.
        /// </summary>
        private int timeSinceLastScroll;

        /// <summary>
        /// The last known position of the mouse cursor in the coordinate system of the page.
        /// </summary>
        private RCIntVector lastKnownMousePosition;

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of this page.
        /// </summary>
        private UIMouseButton activatorBtn;

        /// <summary>
        /// The time between map-scrolling operations.
        /// </summary>
        private const int TIME_BETWEEN_MAP_SCROLLS = 30;
    }
}
