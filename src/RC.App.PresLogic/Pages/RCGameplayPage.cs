using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.App.PresLogic.Controls;
using RC.Common.ComponentModel;
using RC.App.PresLogic.Panels;
using RC.Common;
using RC.UI;
using RC.App.BizLogic.ComponentInterfaces;
using System.Threading;
using RC.Common.Diagnostics;
using System.Diagnostics;

namespace RC.App.PresLogic.Pages
{
    /// <summary>
    /// The Gameplay page of the RC application.
    /// </summary>
    public class RCGameplayPage : RCAppPage
    {
        /// <summary>
        /// Enumerates the possible connection statuses of the RCGameplayPage.
        /// </summary>
        public enum ConnectionStatus
        {
            Offline = 0,        /// The page is offline.
            Connecting = 1,     /// The page is connecting to the currently active game.
            Online = 2,         /// The page is online.
            Disconnecting = 3   /// The page is disconnecting from the currently active game.
        }

        /// <summary>
        /// Constructs a gameplay page.
        /// </summary>
        public RCGameplayPage() : base()
        {
            this.mapDisplay = null;
            this.mapDisplayBasic = null;
            this.mapWalkabilityDisplay = null;
            this.mapObjectDisplayEx = null;
            this.selectionDisplayEx = null;
            this.objectPlacementDisplayEx = null;
            this.currentConnectionStatus = ConnectionStatus.Offline;

            this.minimapPanel = new RCMinimapPanel(new RCIntRectangle(0, 120, 80, 80),
                                                   new RCIntRectangle(1, 1, 78, 78),
                                                   "RC.App.Sprites.MinimapPanel");
            this.detailsPanel = new RCDetailsPanel(new RCIntRectangle(80, 148, 170, 52),
                                                   new RCIntRectangle(0, 1, 170, 50),
                                                   "RC.App.Sprites.DetailsPanel");
            this.commandPanel = new RCCommandPanel(new RCIntRectangle(250, 130, 70, 70),
                                                   new RCIntRectangle(3, 3, 64, 64),
                                                   "RC.App.Sprites.CommandPanel");
            this.tooltipBar = new RCTooltipBar(new RCIntRectangle(0, 0, 209, 13),
                                               new RCIntRectangle(1, 1, 207, 11),
                                               "RC.App.Sprites.TooltipBar");
            this.resourceBar = new RCResourceBar(new RCIntRectangle(209, 0, 111, 13),
                                                 new RCIntRectangle(0, 1, 110, 11),
                                                 "RC.App.Sprites.ResourceBar");
            this.menuButtonPanel = new RCMenuButtonPanel(new RCIntRectangle(226, 140, 24, 8),
                                                         new RCIntRectangle(0, 0, 24, 8),
                                                         "RC.App.Sprites.MenuButton");
            this.RegisterPanel(this.minimapPanel);
            this.RegisterPanel(this.detailsPanel);
            this.RegisterPanel(this.commandPanel);
            this.RegisterPanel(this.tooltipBar);
            this.RegisterPanel(this.resourceBar);
            this.RegisterPanel(this.menuButtonPanel);
        }

        #region Game connection management

        /// <summary>
        /// Starts connecting this RCGameplayPage to the currently active game. The event RCGameplayPage.CurrentConnectionStatus will be raised
        /// when the connection status has been changed.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no active game or if the page has already been connected.</exception>
        public void Connect()
        {
            if (this.currentConnectionStatus != ConnectionStatus.Offline) { throw new InvalidOperationException("The gameplay page is not offline!"); }
            
            /// TODO: A scenario shall be running at this point!
            ComponentManager.GetInterface<IGameplayBE>().StartTestScenario();

            /// Create and start the map display control.
            this.mapDisplayBasic = new RCMapDisplayBasic(new RCIntVector(0, 13), new RCIntVector(320, 135));
            //this.mapWalkabilityDisplay = new RCMapWalkabilityDisplay(this.mapDisplayBasic);
            this.mapObjectDisplayEx = new RCMapObjectDisplay(this.mapDisplayBasic);
            this.selectionDisplayEx = new RCSelectionDisplay(this.mapObjectDisplayEx);
            this.objectPlacementDisplayEx = new RCObjectPlacementDisplay(this.selectionDisplayEx);
            this.mapDisplay = this.objectPlacementDisplayEx;
            this.mapDisplay.ConnectorOperationFinished += this.OnMapDisplayConnected;
            this.mapDisplay.Connect();

            /// Set the connection status to ConnectionStatus.Connecting.
            this.CurrentConnectionStatus = ConnectionStatus.Connecting;
        }

        /// <summary>
        /// Starts disconnecting this RCGameplayPage from the currently active game. The event RCGameplayPage.CurrentConnectionStatus will be raised
        /// when the connection status has been changed.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the page was not connected.</exception>
        public void Disconnect()
        {
            if (this.currentConnectionStatus != ConnectionStatus.Online) { throw new InvalidOperationException("The gameplay page is not online!"); }

            ComponentManager.GetInterface<IGameplayBE>().StopTestScenario();

            /// TODO: deactivate mouse handling
            this.menuButtonPanel.MouseSensor.ButtonDown -= this.OnMenuButtonPressed;
            this.mouseHandler.MouseActivityStarted -= this.OnMouseActivityStarted;
            this.mouseHandler.MouseActivityFinished -= this.OnMouseActivityFinished;
            this.scrollHandler.MouseActivityStarted -= this.OnMouseActivityStarted;
            this.scrollHandler.MouseActivityFinished -= this.OnMouseActivityFinished;
            this.mouseHandler.DeactivateMouseHandling();
            this.scrollHandler.DeactivateMouseHandling();

            /// Detach the map display control from this page.
            this.DetachSensitive(this.mapDisplay);
            this.Detach(this.mapDisplay);

            /// Stop the map display control.
            this.mapDisplay.ConnectorOperationFinished += this.OnMapDisplayDisconnected;
            this.mapDisplay.Disconnect();

            /// Set the connection status to ConnectionStatus.Disconnecting.
            this.CurrentConnectionStatus = ConnectionStatus.Disconnecting;
        }

        /// <summary>
        /// Gets the current connection status of the page.
        /// </summary>
        public ConnectionStatus CurrentConnectionStatus
        {
            get { return this.currentConnectionStatus; }

            private set
            {
                bool valueChanged = this.currentConnectionStatus != value;
                this.currentConnectionStatus = value;
                if (valueChanged && this.CurrentConnectionStatusChanged != null) { this.CurrentConnectionStatusChanged(this, new EventArgs()); }
            }
        }

        /// <summary>
        /// This event is raised when the connection status of the page has been changed.
        /// </summary>
        public event EventHandler CurrentConnectionStatusChanged;

        #endregion Game connection management

        /// <see cref="RCAppPage.OnActivated"/>
        protected override void OnActivated()
        {
            this.minimapPanel.Show();
            this.detailsPanel.Show();
            this.commandPanel.Show();
            this.tooltipBar.Show();
            this.resourceBar.Show();
            this.menuButtonPanel.Show();

            /// TODO: connect shall be performed before activation
            this.Connect();
        }

        /// <see cref="RCAppPage.OnInactivating"/>
        protected override void OnInactivating()
        {
            this.Disconnect();
        }

        /// <summary>
        /// This method is called when the map display started successfully.
        /// </summary>
        private void OnMapDisplayConnected(IGameConnector sender)
        {
            this.mapDisplay.ConnectorOperationFinished -= this.OnMapDisplayConnected;

            /// Attach the map display control to this page.
            this.Attach(this.mapDisplay);
            this.AttachSensitive(this.mapDisplay);
            this.mapDisplay.SendToBottom();

            /// Create the mouse handlers for the map display.
            this.scrollHandler = new ScrollHandler(this, this.mapDisplay);
            this.mouseHandler = new MouseHandler(this.selectionDisplayEx, this.selectionDisplayEx);
            this.scrollHandler.ActivateMouseHandling();
            this.mouseHandler.ActivateMouseHandling();
            this.scrollHandler.MouseActivityStarted += this.OnMouseActivityStarted;
            this.scrollHandler.MouseActivityFinished += this.OnMouseActivityFinished;
            this.mouseHandler.MouseActivityStarted += this.OnMouseActivityStarted;
            this.mouseHandler.MouseActivityFinished += this.OnMouseActivityFinished;

            this.menuButtonPanel.MouseSensor.ButtonDown += this.OnMenuButtonPressed;

            /// The page is now online.
            this.CurrentConnectionStatus = ConnectionStatus.Online;
        }
        
        /// <summary>
        /// This method is called when the map display started successfully.
        /// </summary>
        private void OnMapDisplayDisconnected(IGameConnector sender)
        {
            this.mapDisplay.ConnectorOperationFinished -= this.OnMapDisplayDisconnected;

            /// Remove the map display control.
            this.mapDisplayBasic = null;
            this.mapWalkabilityDisplay = null;
            this.mapObjectDisplayEx = null;
            this.selectionDisplayEx = null;
            this.objectPlacementDisplayEx = null;
            this.mapDisplay = null;
            this.scrollHandler = null;
            this.mouseHandler = null;

            /// The page is now offline.
            this.CurrentConnectionStatus = ConnectionStatus.Offline;

            /// TODO: later we don't need to stop the render loop here!
            UIRoot.Instance.GraphicsPlatform.RenderLoop.Stop();
        }

        /// <summary>
        /// Called when a mouse activity has been started on the map object display.
        /// </summary>
        private void OnMouseActivityStarted(object sender, EventArgs evtArgs)
        {
            if (sender == this.mouseHandler)
            {
                this.scrollHandler.DeactivateMouseHandling();
            }
            else if (sender == this.scrollHandler)
            {
                this.mouseHandler.DeactivateMouseHandling();
            }
        }

        /// <summary>
        /// Called when a mouse activity has been finished on the map object display.
        /// </summary>
        private void OnMouseActivityFinished(object sender, EventArgs evtArgs)
        {
            if (sender == this.mouseHandler)
            {
                this.scrollHandler.ActivateMouseHandling();
            }
            else if (sender == this.scrollHandler)
            {
                this.mouseHandler.ActivateMouseHandling();
            }
        }

        /// <summary>
        /// This method is called when the "Menu" button has been pressed.
        /// </summary>
        /// <param name="sender">Reference to the button.</param>
        private void OnMenuButtonPressed(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            this.Deactivate();
        }

        /// <summary>
        /// Reference to the minimap panel.
        /// </summary>
        private RCMinimapPanel minimapPanel;

        /// <summary>
        /// Reference to the details panel.
        /// </summary>
        private RCDetailsPanel detailsPanel;

        /// <summary>
        /// Reference to the command panel.
        /// </summary>
        private RCCommandPanel commandPanel;

        /// <summary>
        /// Reference to the tooltip bar.
        /// </summary>
        private RCTooltipBar tooltipBar;

        /// <summary>
        /// Reference to the resource bar.
        /// </summary>
        private RCResourceBar resourceBar;

        /// <summary>
        /// Reference to the panel that contains the gameplay menu button.
        /// </summary>
        private RCMenuButtonPanel menuButtonPanel;

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;

        /// <summary>
        /// The basic part of the map display.
        /// </summary>
        private RCMapDisplayBasic mapDisplayBasic;

        /// <summary>
        /// Extension of the map display that displays the walkability of the map cells.
        /// </summary>
        private RCMapWalkabilityDisplay mapWalkabilityDisplay;

        /// <summary>
        /// Extension of the map display that displays the map objects.
        /// </summary>
        private RCMapObjectDisplay mapObjectDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the selection box and the selection indicators of the selected
        /// map objects.
        /// </summary>
        private RCSelectionDisplay selectionDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the object placement boxes.
        /// </summary>
        private RCObjectPlacementDisplay objectPlacementDisplayEx;

        /// <summary>
        /// The current connection status of the page.
        /// </summary>
        private ConnectionStatus currentConnectionStatus;

        /// <summary>
        /// Reference to the object that controls the scrolling of the map display.
        /// </summary>
        private ScrollHandler scrollHandler;

        /// <summary>
        /// Reference to the mouse handler of the map display.
        /// </summary>
        private MouseHandler mouseHandler;
    }
}
