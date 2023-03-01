using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.PresLogic.Controls;
using RC.App.PresLogic.SpriteGroups;
using RC.Common.ComponentModel;
using RC.App.PresLogic.Panels;
using RC.Common;
using RC.UI;
using System.Threading;
using RC.Common.Diagnostics;
using System.Diagnostics;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;

namespace RC.App.PresLogic.Pages
{
    /// <summary>
    /// The Gameplay page of the RC application.
    /// </summary>
    public class RCGameplayPage : RCAppPage, IGameConnector
    {
        /// <summary>
        /// Constructs a gameplay page.
        /// </summary>
        public RCGameplayPage() : base()
        {
            /// Create the sprite group loaders for loading the shared sprite groups.
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            SpriteGroupLoader isoTileSpriteGroupLoader = new SpriteGroupLoader(
                () => new IsoTileSpriteGroup(viewService.CreateView<ITileSetView>()));
            SpriteGroupLoader terrainObjectSpriteGroupLoader = new SpriteGroupLoader(
                () => new TerrainObjectSpriteGroup(viewService.CreateView<ITileSetView>()));
            SpriteGroupLoader disabledCommandButtonSpriteGroupLoader = new SpriteGroupLoader(
                    () => new CmdButtonSpriteGroup(viewService.CreateView<ICommandView>(), CommandButtonStateEnum.Disabled));
            SpriteGroupLoader enabledCommandButtonSpriteGroupLoader = new SpriteGroupLoader(
                    () => new CmdButtonSpriteGroup(viewService.CreateView<ICommandView>(), CommandButtonStateEnum.Enabled));
            SpriteGroupLoader highlightedCommandButtonSpriteGroupLoader = new SpriteGroupLoader(
                    () => new CmdButtonSpriteGroup(viewService.CreateView<ICommandView>(), CommandButtonStateEnum.Highlighted));
            Dictionary<CommandButtonStateEnum, ISpriteGroup> commandButtonSprites = new Dictionary<CommandButtonStateEnum, ISpriteGroup>
            {
                { CommandButtonStateEnum.Disabled, disabledCommandButtonSpriteGroupLoader },
                { CommandButtonStateEnum.Enabled, enabledCommandButtonSpriteGroupLoader },
                { CommandButtonStateEnum.Highlighted, highlightedCommandButtonSpriteGroupLoader },
            };

            /// Create the map display and its extensions.
            this.mapDisplayBasic = new RCMapDisplayBasic(isoTileSpriteGroupLoader,
                                                         terrainObjectSpriteGroupLoader,
                                                         new RCIntVector(0, 13),
                                                         new RCIntVector(320, 135));
            //this.mapWalkabilityDisplay = new RCMapWalkabilityDisplay(this.mapDisplayBasic);
            this.mapObjectDisplayEx = new RCMapObjectDisplay(this.mapDisplayBasic);
            this.suggestionBoxDisplayEx = new RCSuggestionBoxDisplay(this.mapObjectDisplayEx);
            this.selectionDisplayEx = new RCSelectionDisplay(this.suggestionBoxDisplayEx);
            this.fogOfWarDisplayEx = new RCFogOfWarDisplay(this.selectionDisplayEx);
            this.objectPlacementDisplayEx = new RCObjectPlacementDisplay(this.fogOfWarDisplayEx);
            this.selectionBoxDisplayEx = new RCSelectionBoxDisplay(this.objectPlacementDisplayEx);
            this.mapDisplay = this.selectionBoxDisplayEx;

            this.mouseHandler = null;

            this.minimapPanel = new RCMinimapPanel(isoTileSpriteGroupLoader,
                                                   terrainObjectSpriteGroupLoader,
                                                   new RCIntRectangle(0, 128, 72, 72),
                                                   new RCIntRectangle(1, 1, 70, 70),
                                                   "RC.App.Sprites.MinimapPanel");
            this.detailsPanel = new RCDetailsPanel(enabledCommandButtonSpriteGroupLoader,
                                                   new RCIntRectangle(72, 148, 178, 52),
                                                   new RCIntRectangle(0, 1, 178, 50),
                                                   "RC.App.Sprites.DetailsPanel");
            this.commandPanel = new RCCommandPanel(commandButtonSprites,
                                                   new RCIntRectangle(250, 130, 70, 70),
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

            this.gameConnection = new SequentialGameConnector(
                new ConcurrentGameConnector(isoTileSpriteGroupLoader,
                                            terrainObjectSpriteGroupLoader,
                                            disabledCommandButtonSpriteGroupLoader,
                                            enabledCommandButtonSpriteGroupLoader,
                                            highlightedCommandButtonSpriteGroupLoader),
                new ConcurrentGameConnector(this.mapDisplay, this.commandPanel, this.minimapPanel, this.detailsPanel, this.resourceBar));
        }

        #region IGameConnector methods

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.gameConnection.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The gameplay page is not offline!"); }
            
            /// TODO: A scenario shall be running at this point!
            ComponentManager.GetInterface<IMultiplayerService>().CreateNewGame("./maps/testmap4b.rcm", GameTypeEnum.Melee, GameSpeedEnum.Fastest);
            ComponentManager.GetInterface<IScrollService>().AttachWindow(this.mapDisplay.PixelSize);

            /// Create and start the map display control.
            this.gameConnection.ConnectorOperationFinished += this.OnConnected;
            this.gameConnection.Connect();
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (this.gameConnection.ConnectionStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("The gameplay page is not online!"); }

            this.commandView = null;
            this.selectionIndicatorView = null;
            ComponentManager.GetInterface<IMultiplayerService>().LeaveCurrentGame();

            /// Deactivate mouse handling.
            this.menuButtonPanel.MouseSensor.ButtonDown -= this.OnMenuButtonPressed;
            this.mouseHandler.Inactivated -= this.CreateMouseHandler;
            this.mouseHandler.Inactivate();

            /// Detach the map display control from this page.
            this.DetachSensitive(this.mapDisplay);
            this.Detach(this.mapDisplay);

            /// Stop the map display control.
            this.gameConnection.ConnectorOperationFinished += this.OnDisconnected;
            this.gameConnection.Disconnect();
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
        {
            get { return this.gameConnection.ConnectionStatus; }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector methods

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
        private void OnConnected(IGameConnector sender)
        {
            this.commandView = ComponentManager.GetInterface<IViewService>().CreateView<ICommandView>();
            this.selectionIndicatorView = ComponentManager.GetInterface<IViewService>().CreateView<ISelectionIndicatorView>();
            this.gameConnection.ConnectorOperationFinished -= this.OnConnected;

            /// Attach the map display control to this page.
            this.Attach(this.mapDisplay);
            this.AttachSensitive(this.mapDisplay);
            this.mapDisplay.SendToBottom();

            /// Create the mouse handler for the map display.
            this.CreateMouseHandler();

            this.menuButtonPanel.MouseSensor.ButtonDown += this.OnMenuButtonPressed;

            if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
        }
        
        /// <summary>
        /// This method is called when the map display stopped successfully.
        /// </summary>
        private void OnDisconnected(IGameConnector sender)
        {
            this.gameConnection.ConnectorOperationFinished -= this.OnDisconnected;
            if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }

            /// TODO: later we don't need to stop the render loop here!
            UIRoot.Instance.GraphicsPlatform.RenderLoop.Stop();
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
        /// Creates a new mouse handler based on the current state available from the command view.
        /// </summary>
        private void CreateMouseHandler()
        {
            if (this.mouseHandler != null) { this.mouseHandler.Inactivated -= this.CreateMouseHandler; }
            if (this.commandView.TargetSelectionMode == TargetSelectionModeEnum.NoTargetSelection)
            {
                this.mouseHandler = new NormalMouseHandler(this, this.mapDisplay, this.selectionBoxDisplayEx);
            }
            else
            {
                this.mouseHandler = new SelectTargetMouseHandler(
                    this,
                    this.mapDisplay,
                    this.selectionBoxDisplayEx,
                    this.mapObjectDisplayEx.GetMapObjectSprites(this.selectionIndicatorView.LocalPlayer));
            }
            this.mouseHandler.Inactivated += this.CreateMouseHandler;
        }

        #region Panels

        /// <summary>
        /// Reference to the minimap panel.
        /// </summary>
        private readonly RCMinimapPanel minimapPanel;

        /// <summary>
        /// Reference to the details panel.
        /// </summary>
        private readonly RCDetailsPanel detailsPanel;

        /// <summary>
        /// Reference to the command panel.
        /// </summary>
        private readonly RCCommandPanel commandPanel;

        /// <summary>
        /// Reference to the tooltip bar.
        /// </summary>
        private readonly RCTooltipBar tooltipBar;

        /// <summary>
        /// Reference to the resource bar.
        /// </summary>
        private readonly RCResourceBar resourceBar;

        /// <summary>
        /// Reference to the panel that contains the gameplay menu button.
        /// </summary>
        private readonly RCMenuButtonPanel menuButtonPanel;

        #endregion Panels

        #region Map display & extensions

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private readonly RCMapDisplay mapDisplay;

        /// <summary>
        /// The basic part of the map display.
        /// </summary>
        private readonly RCMapDisplayBasic mapDisplayBasic;

        /// <summary>
        /// Extension of the map display that displays the walkability of the map cells.
        /// </summary>
        private readonly RCMapWalkabilityDisplay mapWalkabilityDisplay;

        /// <summary>
        /// Extension of the map display that displays the map objects.
        /// </summary>
        private readonly RCMapObjectDisplay mapObjectDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the suggestion boxes.
        /// </summary>
        private readonly RCSuggestionBoxDisplay suggestionBoxDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the selection indicators of the selected map objects.
        /// </summary>
        private readonly RCSelectionDisplay selectionDisplayEx;

        /// <summary>
        /// Extenation of the map display that displays the actual Fog Of War state of the quadratic tiles.
        /// </summary>
        private readonly RCFogOfWarDisplay fogOfWarDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the object placement boxes.
        /// </summary>
        private readonly RCObjectPlacementDisplay objectPlacementDisplayEx;

        /// <summary>
        /// Extension of the map display that displays the selection box.
        /// </summary>
        private readonly RCSelectionBoxDisplay selectionBoxDisplayEx;

        #endregion Map display & extensions

        /// <summary>
        /// Reference to the game connector object.
        /// </summary>
        private readonly IGameConnector gameConnection;

        /// <summary>
        /// Reference to the currently active mouse handler.
        /// </summary>
        private MouseHandlerBase mouseHandler;

        /// <summary>
        /// Reference to a command view.
        /// </summary>
        private ICommandView commandView;

        /// <summary>
        /// Reference to a selection indicator view.
        /// </summary>
        private ISelectionIndicatorView selectionIndicatorView;
    }
}
