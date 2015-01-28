using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The command panel on the gameplay page
    /// </summary>
    public class RCCommandPanel : RCAppPanel, IGameConnector
    {
        /// <summary>
        /// Constructs a command panel.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">Name of the sprite resource that will be the background of this panel or null if there is no background.</param>
        public RCCommandPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect, string backgroundSprite)
            : base(backgroundRect, contentRect, ShowMode.Appear, HideMode.Disappear, 0, 0, backgroundSprite)
        {
            this.isConnected = false;
            this.backgroundTask = null;
            this.commandButtonSprites = new Dictionary<CommandButtonStateEnum, SpriteGroup>();
            this.buttonArray = new RCCommandButton[BUTTON_ARRAY_ROWS, BUTTON_ARRAY_COLS];
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        void IGameConnector.Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The command panel has been connected or is currently being connected!"); }

            /// UI-thread connection procedure
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            ICommandView commandPanelView = viewService.CreateView<ICommandView>();
            this.commandButtonSprites.Add(CommandButtonStateEnum.Disabled, new CmdButtonSpriteGroup(commandPanelView, CommandButtonStateEnum.Disabled));
            this.commandButtonSprites.Add(CommandButtonStateEnum.Enabled, new CmdButtonSpriteGroup(commandPanelView, CommandButtonStateEnum.Enabled));
            this.commandButtonSprites.Add(CommandButtonStateEnum.Highlighted, new CmdButtonSpriteGroup(commandPanelView, CommandButtonStateEnum.Highlighted));

            this.backgroundTask = UITaskManager.StartParallelTask(this.ConnectBackgroundProc, "RCCommandPanel.Connect");
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

            /// Destroy the command buttons.
            for (int row = 0; row < BUTTON_ARRAY_ROWS; row++)
            {
                for (int col = 0; col < BUTTON_ARRAY_COLS; col++)
                {
                    this.RemoveControl(this.buttonArray[row, col]);
                    this.buttonArray[row, col].Dispose();
                    this.buttonArray[row, col] = null;
                }
            }

            this.backgroundTask = UITaskManager.StartParallelTask(this.DisconnectBackgroundProc, "RCCommandPanel.Disconnect");
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
                for (int row = 0; row < BUTTON_ARRAY_ROWS; row++)
                {
                    for (int col = 0; col < BUTTON_ARRAY_COLS; col++)
                    {
                        this.buttonArray[row, col] = new RCCommandButton(new RCIntVector(row, col), this.commandButtonSprites);
                        this.AddControl(this.buttonArray[row, col]);
                    }
                }

                this.isConnected = true;
                if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
            }
            else
            {
                this.isConnected = false;
                if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
            }
        }

        /// <summary>
        /// Executes connection procedures on a background thread.
        /// </summary>
        private void ConnectBackgroundProc(object parameter)
        {
            foreach (SpriteGroup spriteGroup in this.commandButtonSprites.Values) { spriteGroup.Load(); }
        }

        /// <summary>
        /// Executes disconnection procedures on a background thread.
        /// </summary>
        private void DisconnectBackgroundProc(object parameter)
        {
            foreach (SpriteGroup spriteGroup in this.commandButtonSprites.Values) { spriteGroup.Unload(); }
            this.commandButtonSprites.Clear();
        }

        #endregion Internal members

        /// <summary>
        /// This flag indicates whether this command panel has been connected or not.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// This event is raised when the actual connector operation has been finished.
        /// </summary>
        private event Action<IGameConnector> connectorOperationFinished;

        /// <summary>
        /// A 2D array that stores the buttons on the command panel. The first coordinate in this array defines the row and
        /// the second coordinate defines the column in which the button is located. The first row is the row at the top,
        /// the first column is the column at the left side of the panel.
        /// </summary>
        private RCCommandButton[,] buttonArray;

        /// <summary>
        /// The size of the command button array.
        /// </summary>
        private const int BUTTON_ARRAY_ROWS = 3;
        private const int BUTTON_ARRAY_COLS = 3;

        /// <summary>
        /// Reference to the currently executed connecting/disconnecting task or null if no such a task is under execution.
        /// </summary>
        private IUIBackgroundTask backgroundTask;

        /// <summary>
        /// List of the command button sprite groups mapped by the appropriate button state.
        /// </summary>
        private Dictionary<CommandButtonStateEnum, SpriteGroup> commandButtonSprites;
    }
}
