using RC.Common;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Interface for sending messages to the members of a lobby.
    /// </summary>
    public interface ILobby
    {
        /// <summary>
        /// Sends a package to every member of the lobby.
        /// </summary>
        /// <param name="package">The package you want to send.</param>
        /// <returns>
        /// True if the package has been sent successfully, false otherwise.
        /// </returns>
        /// <remarks>
        /// If this function returns true, that only means that the package has been successfully sent from this peer
        /// but you don't get information if it has been delivered to every target or not.
        /// </remarks>
        bool SendPackage(RCPackage package);

        /// <summary>
        /// Sends a package to the given members of the lobby.
        /// </summary>
        /// <param name="package">The package you want to send.</param>
        /// <param name="targets">List of the members you want to send the package to.</param>
        /// <returns>
        /// True if the package has been sent successfully, false otherwise.
        /// </returns>
        /// <remarks>
        /// If this function returns true, that only means that the package has been successfully sent from this peer
        /// but you don't get information if it has been delivered to every target or not.
        /// </remarks>
        bool SendPackage(RCPackage package, int[] targets);
    }

    /// <summary>
    /// Interface for access the lobby at client side.
    /// </summary>
    public interface ILobbyClient : ILobby
    {
        /// <summary>
        /// Sends a control package to the server.
        /// </summary>
        /// <param name="package">The control package you want to send.</param>
        /// <remarks>
        /// The type of the package has to be RCPackageType.NETWORK_CONTROL_PACKAGE. The format of the package has to
        /// be different from the internal package formats used by the RC.NetworkingSystem.
        /// </remarks>
        bool SendControlPackage(RCPackage package);

        /// <summary>
        /// Disconnects this client from the lobby.
        /// </summary>
        void Disconnect();
    }

    /// <summary>
    /// Interface for access the lobby at server side.
    /// </summary>
    public interface ILobbyServer : ILobby
    {
        /// <summary>
        /// Sends a control package to a client.
        /// </summary>
        /// <param name="package">The control package you want to send.</param>
        /// <param name="target">The client you want to send the package to.</param>
        /// <remarks>
        /// The type of the package has to be RCPackageType.NETWORK_CONTROL_PACKAGE. The format of the package has to
        /// be different from the internal package formats used by the RC.NetworkingSystem. The target has to be
        /// different from the server (must be non-zero).
        /// </remarks>
        bool SendControlPackage(RCPackage package, int target);

        /// <summary>
        /// Closes the given communication line of the server.
        /// </summary>
        /// <param name="line">The communication line you want to close.</param>
        /// <remarks>
        /// If there is any client connected to the line, that client will be disconnected.
        /// If the line has been already closed then this function has no effect.
        /// </remarks>
        void CloseLine(int line);

        /// <summary>
        /// Opens the given communication line of the server.
        /// </summary>
        /// <param name="line">The line you want to open.</param>
        /// <remarks>
        /// If the line has been already opened or there is any client connected to the line then this function has no effect.
        /// </remarks>
        void OpenLine(int line);

        /// <summary>
        /// Disconnects every client and stops the lobby server.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Starts announcing the lobby on the network.
        /// </summary>
        /// <remarks>If the lobby is already being announced then this function has no effect.</remarks>
        void StartAnnouncing();

        /// <summary>
        /// Starts announcing the lobby on the network with custom data.
        /// </summary>
        /// <param name="customDataProvider">The object that will provide the custom data for the announcement.</param>
        /// <remarks>If the lobby is already being announced then this function has no effect.</remarks>
        void StartAnnouncing(ILobbyCustomDataProvider customDataProvider);

        /// <summary>
        /// Stops announcing the lobby on the network.
        /// </summary>
        /// <remarks>If the lobby is not being announced then this function has no effect.</remarks>
        void StopAnnouncing();
    }

    /// <summary>
    /// When you want to announce a lobby server, you can give an object implementing this interface if you
    /// want to announce custom lobby data.
    /// </summary>
    public interface ILobbyCustomDataProvider
    {
        RCPackage CustomData { get; }
    }

    /// <summary>
    /// Enumerates the possible states of a line on the server.
    /// </summary>
    public enum LobbyLineState
    {
        Opened = 0x4F,          /// The line is opened and is waiting for client connections (ASCII character "O").
        Closed = 0x43,          /// The line is closed (ASCII character "C").
        Engaged = 0x45          /// The line is engaged because another client is using it (ASCII character "E").
    }

    /// <summary>
    /// Used by the networking system to notify the implementor object about lobby events.
    /// </summary>
    public interface ILobbyListener
    {
        /// <summary>
        /// Called when a package has been arrived from another peer in the lobby.
        /// </summary>
        /// <param name="package">The package that contains the message.</param>
        /// <param name="senderID">The ID of the sender peer.</param>
        void PackageArrived(RCPackage package, int senderID);

        /// <summary>
        /// A control package has been arrived from the server.
        /// </summary>
        /// <param name="package">The arrived control package.</param>
        /// <remarks>This function is called only at client side.</remarks>
        void ControlPackageArrived(RCPackage package);

        /// <summary>
        /// A control package has been arrived from a client.
        /// </summary>
        /// <param name="package">The arrived control package.</param>
        /// <param name="senderID">The ID of the sender.</param>
        /// <remarks>This function is called only at server side.</remarks>
        void ControlPackageArrived(RCPackage package, int senderID);

        /// <summary>
        /// Called when the state of any communication line on the server has been changed.
        /// </summary>
        /// <param name="idOfThisPeer">The ID of this peer.</param>
        /// <param name="lineStates">The current line states on the server.</param>
        /// <remarks>
        /// A client or the server can use the lobby only after this function has been called at least once otherwise
        /// the behaviour is undefined.
        /// </remarks>
        void LineStateReport(int idOfThisPeer, LobbyLineState[] lineStates);

        /// <summary>
        /// Called when the connection with the lobby server has been lost.
        /// </summary>
        /// <remarks>This function is called only at client side.</remarks>
        void LobbyLost();
    }

    /// <summary>
    /// Used by the networking system to notify the implementor object about the lobbies announced on the network.
    /// </summary>
    public interface ILobbyLocator
    {
        /// <summary>
        /// The networking system found a new lobby on the network.
        /// </summary>
        /// <param name="foundLobby">Informations about the new lobby.</param>
        void LobbyFound(LobbyInfo foundLobby);

        /// <summary>
        /// The networking system noticed that the state of an announced lobby has been changed.
        /// </summary>
        /// <param name="changedLobby">Actual informations about the changed lobby.</param>
        void LobbyChanged(LobbyInfo changedLobby);

        /// <summary>
        /// The networking system noticed that an announced lobby has been vanished from the network.
        /// </summary>
        /// <param name="vanishedLobby">The last known informations about the vanished lobby.</param>
        void LobbyVanished(LobbyInfo vanishedLobby);
    }
}
