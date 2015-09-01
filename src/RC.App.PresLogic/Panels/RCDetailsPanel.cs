using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.App.PresLogic.SpriteGroups;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The details panel on the gameplay page
    /// </summary>
    public class RCDetailsPanel : RCAppPanel, IGameConnector
    {
        /// <summary>
        /// Constructs a details panel.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">Name of the sprite resource that will be the background of this panel or null if there is no background.</param>
        public RCDetailsPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect, string backgroundSprite)
            : base(backgroundRect, contentRect, ShowMode.Appear, HideMode.Disappear, 0, 0, backgroundSprite)
        {
            this.isConnected = false;
            this.backgroundTask = null;
            this.hpIndicatorSprites = new Dictionary<MapObjectConditionEnum, SpriteGroup>();
            this.buttonArray = new RCSelectionButton[MAX_SELECTION_SIZE];
            this.multiplayerService = null;
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        void IGameConnector.Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The details panel has been connected or is currently being connected!"); }

            /// UI-thread connection procedure
            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            IMetadataView metadataView = viewService.CreateView<IMetadataView>();
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Excellent, new HPIconSpriteGroup(metadataView, MapObjectConditionEnum.Excellent));
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Moderate, new HPIconSpriteGroup(metadataView, MapObjectConditionEnum.Moderate));
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Critical, new HPIconSpriteGroup(metadataView, MapObjectConditionEnum.Critical));

            this.backgroundTask = UITaskManager.StartParallelTask(this.ConnectBackgroundProc, "RCDetailsPanel.Connect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.Disconnect"/>
        void IGameConnector.Disconnect()
        {
            if (!this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The command panel has been connected or is currently being connected!"); }

            /// Unsubscribe from the GameUpdated event.
            this.multiplayerService.GameUpdated -= this.OnGameUpdated;

            /// Destroy the selection buttons.
            for (int i = 0; i < MAX_SELECTION_SIZE; i++)
            {
                this.RemoveControl(this.buttonArray[i]); // TODO: remove selection buttons only when more than 1 object is selected!
                this.buttonArray[i].Dispose();
                this.buttonArray[i] = null;
            }

            this.backgroundTask = UITaskManager.StartParallelTask(this.DisconnectBackgroundProc, "RCDetailsPanel.Disconnect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        ConnectionStatusEnum IGameConnector.ConnectionStatus
        {
            get
            {
                if (this.backgroundTask == null) { return this.isConnected ? ConnectionStatusEnum.Online : ConnectionStatusEnum.Offline; }
                else { return this.isConnected ? ConnectionStatusEnum.Disconnecting : ConnectionStatusEnum.Connecting; }
            }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        event Action<IGameConnector> IGameConnector.ConnectorOperationFinished
        {
            add { this.connectorOperationFinished += value; }
            remove { this.connectorOperationFinished -= value; }
        }

        #endregion IGameConnector members

        #region Internal members

        /// <summary>
        /// Called when the currently running background task has been finished.
        /// </summary>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object message)
        {
            this.backgroundTask.Finished -= this.OnBackgroundTaskFinished;
            this.backgroundTask = null;
            if (!this.isConnected)
            {
                /// Create the command buttons.
                for (int i = 0; i < MAX_SELECTION_SIZE; i++)
                {
                    this.buttonArray[i] = new RCSelectionButton(i, this.hpIndicatorSprites);
                    this.AddControl(this.buttonArray[i]); // TODO: add selection buttons only when more than 1 object is selected!
                }

                /// Subscribe to the GameUpdated event.
                this.multiplayerService.GameUpdated += this.OnGameUpdated;

                this.isConnected = true;
                if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
            }
            else
            {
                this.multiplayerService = null;
                this.isConnected = false;
                if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
            }
        }

        /// <summary>
        /// Executes connection procedures on a background thread.
        /// </summary>
        private void ConnectBackgroundProc(object parameter)
        {
            foreach (SpriteGroup spriteGroup in this.hpIndicatorSprites.Values) { spriteGroup.Load(); }
        }

        /// <summary>
        /// Executes disconnection procedures on a background thread.
        /// </summary>
        private void DisconnectBackgroundProc(object parameter)
        {
            foreach (SpriteGroup spriteGroup in this.hpIndicatorSprites.Values) { spriteGroup.Unload(); }
            this.hpIndicatorSprites.Clear();
        }

        /// <summary>
        /// This method is called on each game update.
        /// </summary>
        private void OnGameUpdated()
        {
            
        }

        #endregion Internal members

        /// <summary>
        /// This flag indicates whether this details panel has been connected or not.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// This event is raised when the actual connector operation has been finished.
        /// </summary>
        private event Action<IGameConnector> connectorOperationFinished;

        /// <summary>
        /// An array that stores the selection buttons on the details panel in layout order.
        /// </summary>
        private readonly RCSelectionButton[] buttonArray;

        /// <summary>
        /// Reference to the currently executed connecting/disconnecting task or null if no such a task is under execution.
        /// </summary>
        private IUIBackgroundTask backgroundTask;

        /// <summary>
        /// List of the HP indicator sprite groups for each possible conditions.
        /// </summary>
        private readonly Dictionary<MapObjectConditionEnum, SpriteGroup> hpIndicatorSprites;

        /// <summary>
        /// Reference to the multiplayer service.
        /// </summary>
        private IMultiplayerService multiplayerService;

        /// <summary>
        /// The maximum number of objects can be selected.
        /// </summary>
        private const int MAX_SELECTION_SIZE = 12;
    }
}
