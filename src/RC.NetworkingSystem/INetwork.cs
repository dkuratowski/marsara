namespace RC.NetworkingSystem
{
    /// <summary>
    /// This is the main access interface of the RC.NetworkingSystem. You can create, join, announce or search
    /// lobbies on the network using this interface.
    /// </summary>
    public interface INetwork
    {
        /// <summary>
        /// Starts searching for lobbies announced by other peers on the network.
        /// </summary>
        /// <param name="locator">
        /// A listener object that will be notified about the lobbies announced on the network.
        /// </param>
        /// <returns>True if starting the search was successfully, false otherwise.</returns>
        /// <remarks>
        /// If the search is already in progress then this function has no effect.
        /// </remarks>
        bool StartLocatingLobbies(ILobbyLocator locator);

        /// <summary>
        /// Stops the current search operation.
        /// </summary>
        /// <remarks>If there is no lobby search operation in progress then this function has no effect.</remarks>
        void StopLocatingLobbies();

        /// <summary>
        /// Creates a new lobby on this peer.
        /// </summary>
        /// <param name="maxClients">
        /// Maximum number of the members in the lobby (this value must be at least 1 because the server is always the member
        /// of the lobby it created).
        /// </param>
        /// <param name="listener">
        /// A listener object that will be notified about lobby events.
        /// </param>
        /// <returns>An interface that can be used to control the lobby or null if creating the lobby is failed.</returns>
        /// <remarks>
        /// You can be a member of only one lobby at a time. If you have created or joined to another lobby then you have
        /// to disconnect from it before you call this function.
        /// </remarks>
        ILobbyServer CreateLobby(int maxClients, ILobbyListener listener);

        /// <summary>
        /// Joines to a lobby that is created by another peer.
        /// </summary>
        /// <param name="info">Informations about the lobby you want to join.</param>
        /// <param name="listener">
        /// A listener object that will be notified about lobby events.
        /// </param>
        /// <returns>
        /// An interface that can be used to send message to or disconnect from the lobby or null if joining to the lobby
        /// is failed at client side.
        /// </returns>
        /// <remarks>
        /// You can be a member of only one lobby at a time. If you have created or joined to another lobby then you have
        /// to disconnect from it before you call this function otherwise this function has no effect.
        /// You can use the returned interface only after the ILobbyListener.LineStateReport has been called.
        /// If the connection fails at server side then the function ILobbyListener.LobbyLost function is called.
        /// </remarks>
        ILobbyClient JoinLobby(LobbyInfo info, ILobbyListener listener);

        /// <summary>
        /// Call this function to close the network and stop all of its internal operations.
        /// </summary>
        void ShutdownNetwork();
    }
}
