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
    /// The command panel on the gameplay page
    /// </summary>
    public class RCCommandPanel : RCAppPanel, IGameConnector
    {
        /// <summary>
        /// Constructs a command panel.
        /// </summary>
        /// <param name="commandButtonSprites">List of the command button sprite groups mapped by the appropriate button state.</param>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">Name of the sprite resource that will be the background of this panel or null if there is no background.</param>
        public RCCommandPanel(Dictionary<CommandButtonStateEnum, ISpriteGroup> commandButtonSprites, RCIntRectangle backgroundRect, RCIntRectangle contentRect, string backgroundSprite)
            : base(backgroundRect, contentRect, ShowMode.Appear, HideMode.Disappear, 0, 0, backgroundSprite)
        {
            if (commandButtonSprites == null) { throw new ArgumentNullException("commandButtonSprites"); }

            this.commandButtonSprites = commandButtonSprites;
            this.isConnected = false;
            this.buttonArray = new RCCommandButton[BUTTON_ARRAY_ROWS, BUTTON_ARRAY_COLS];
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        void IGameConnector.Connect()
        {
            if (this.isConnected) { throw new InvalidOperationException("The command panel has been connected or is currently being connected!"); }

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

        /// <see cref="IGameConnector.Disconnect"/>
        void IGameConnector.Disconnect()
        {
            if (!this.isConnected) { throw new InvalidOperationException("The command panel has been connected or is currently being connected!"); }

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

            this.isConnected = false;
            if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        ConnectionStatusEnum IGameConnector.ConnectionStatus
        {
            get { return this.isConnected ? ConnectionStatusEnum.Online : ConnectionStatusEnum.Offline; }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        event Action<IGameConnector> IGameConnector.ConnectorOperationFinished
        {
            add { this.connectorOperationFinished += value; }
            remove { this.connectorOperationFinished -= value; }
        }

        #endregion IGameConnector members

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
        private readonly RCCommandButton[,] buttonArray;

        /// <summary>
        /// List of the command button sprite groups mapped by the appropriate button state.
        /// </summary>
        private readonly Dictionary<CommandButtonStateEnum, ISpriteGroup> commandButtonSprites;

        /// <summary>
        /// The size of the command button array.
        /// </summary>
        private const int BUTTON_ARRAY_ROWS = 3;
        private const int BUTTON_ARRAY_COLS = 3;
    }
}
