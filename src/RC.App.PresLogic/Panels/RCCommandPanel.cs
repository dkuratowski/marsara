using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.App.PresLogic.Controls;
using RC.Common;
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
            this.buttonArray = new RCCommandButton[3, 3];
            for (int row = 0; row < BUTTON_ARRAY_ROWS; row++)
            {
                for (int col = 0; col < BUTTON_ARRAY_COLS; col++)
                {
                    this.buttonArray[row, col] = new RCCommandButton(BUTTON_POSITIONS[row, col], new RCIntVector(row, col).ToString());
                    this.AddControl(this.buttonArray[row, col]);
                }
            }
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        void IGameConnector.Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The command panel has been connected or is currently being connected!"); }

            /// TODO: implement UI-thread connection procedures here!

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

            /// TODO: implement UI-thread disconnection procedures here!

            this.backgroundTask = UITaskManager.StartParallelTask(this.DisconnectBackgroundProc, "RCCommandPanel.Disconnect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.CurrentStatus"/>
        ConnectionStatusEnum IGameConnector.CurrentStatus
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
            this.backgroundTask = null;
            if (!this.isConnected)
            {
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
            /// TODO: implement background connection procedures here!
        }

        /// <summary>
        /// Executes disconnection procedures on a background thread.
        /// </summary>
        private void DisconnectBackgroundProc(object parameter)
        {
            /// TODO: implement background disconnection procedures here!
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
        /// The size of a command button in pixels.
        /// </summary>
        private static readonly RCIntRectangle[,] BUTTON_POSITIONS = new RCIntRectangle[3, 3]
        {
            { new RCIntRectangle(1, 1, 20, 20), new RCIntRectangle(22, 1, 20, 20), new RCIntRectangle(43, 1, 20, 20) },
            { new RCIntRectangle(1, 22, 20, 20), new RCIntRectangle(22, 22, 20, 20), new RCIntRectangle(43, 22, 20, 20) },
            { new RCIntRectangle(1, 43, 20, 20), new RCIntRectangle(22, 43, 20, 20), new RCIntRectangle(43, 43, 20, 20) },
        };
    }
}
