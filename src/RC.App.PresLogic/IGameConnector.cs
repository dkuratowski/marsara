using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Enumerates the possible connection states of a game connector.
    /// </summary>
    public enum ConnectionStatusEnum
    {
        Offline = 0,        /// The connector is offline.
        Connecting = 1,     /// The connector is connecting to the currently active game.
        Online = 2,         /// The connector is online.
        Disconnecting = 3   /// The connector is disconnecting from the currently active game.
    }

    /// <summary>
    /// Common interface of UI elements that need to be connected to the currently active game on the backend before use.
    /// </summary>
    public interface IGameConnector
    {
        /// <summary>
        /// Starts connecting this connector to the currently active game on the backend.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the connector is connected or connection is currently in progress.
        /// </exception>
        void Connect();

        /// <summary>
        /// Starts disconnecting this connector from the currently active game on the backend.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the connector is disconnected or disconnection is currently in progress.
        /// </exception>
        void Disconnect();

        /// <summary>
        /// Gets the current status of this game connector.
        /// </summary>
        ConnectionStatusEnum CurrentStatus { get; }

        /// <summary>
        /// This event is raised when the current connection/disconnection operation has been finished. The parameter
        /// is a reference to the connector in which the operation has been finished.
        /// </summary>
        event Action<IGameConnector> ConnectorOperationFinished;
    }
}
