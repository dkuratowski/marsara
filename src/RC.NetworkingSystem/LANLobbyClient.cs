namespace RC.NetworkingSystem
{
    /// <summary>
    /// Represents a LobbyClient on the Local Area Network.
    /// </summary>
    class LANLobbyClient : LobbyClient
    {
        /// <summary>
        /// Constructs a LANLobbyClient object.
        /// </summary>
        public LANLobbyClient(LobbyInfo info, ILobbyListener listener)
            : base(info, listener)
        {
        }

        /// <see cref="LobbyClient.CreateConnection_i"/>
        protected override LobbyConnection CreateConnection_i(System.Net.IPEndPoint serverAddr)
        {
            LobbyConnection conn = new LANLobbyConnection(serverAddr);
            return conn;
        }
    }
}
