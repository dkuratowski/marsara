using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// This static class is used to access the constants of the RC.NetworkingSystem module.
    /// </summary>
    static class NetworkingSystemConstants
    {
        /// <summary>
        /// Length of the UDP pockets sent by the lobby announcer.
        /// </summary>
        public static readonly int ANNOUNCEMENT_UDP_POCKET_SIZE = ConstantsTable.Get<int>("RC.NetworkingSystem.AnnouncementUdpPocketSize");

        /// <summary>
        /// Default length of the buffers used to read or write the network.
        /// </summary>
        public static readonly int TCP_CONNECTION_BUFFER_DEFAULT_LENGTH = ConstantsTable.Get<int>("RC.NetworkingSystem.TcpConnectionBufferDefaultLength");

        /// <summary>
        /// The elapsed time between announcements in milliseconds.
        /// </summary>
        public static readonly int ANNOUNCEMENT_FREQUENCY = ConstantsTable.Get<int>("RC.NetworkingSystem.AnnouncementFrequency");

        /// <summary>
        /// The number of announcement packages to send on UDP.
        /// </summary>
        public static readonly int ANNOUNCEMENT_BROADCAST_MULTIPLICITY = ConstantsTable.Get<int>("RC.NetworkingSystem.AnnouncementBroadcastMultiplicity");

        /// <summary>
        /// The cycle time of the client connection manager thread in milliseconds.
        /// </summary>
        public static readonly int CLIENT_CONNECTION_MANAGER_CYCLE_TIME = ConstantsTable.Get<int>("RC.NetworkingSystem.ClientConnectionManagerCycleTime");

        /// <summary>
        /// The cycle time of the server connection manager thread in milliseconds.
        /// </summary>
        public static readonly int SERVER_CONNECTION_MANAGER_CYCLE_TIME = ConstantsTable.Get<int>("RC.NetworkingSystem.ServerConnectionManagerCycleTime");

        /// <summary>
        /// If more than this time elapses without receiving a disconnect_ack message from the other side then it is
        /// considered that the other side has a problem and an immediate shutdown needed. This time is in milliseconds.
        /// </summary>
        public static readonly int DISCONNECT_ACK_TIMEOUT = ConstantsTable.Get<int>("RC.NetworkingSystem.DisconnectAckTimeout");

        /// <summary>
        /// If more than this time elapses without receiving a line state report message from the other side then it is
        /// considered that the other side has a problem and an immediate shutdown needed. This time is in milliseconds.
        /// </summary>
        public static readonly int CONNECT_ACK_TIMEOUT = ConstantsTable.Get<int>("RC.NetworkingSystem.ConnectAckTimeout");

        /// <summary>
        /// If more than this time elapses without receiving at least a ping message from the other side then the connection
        /// is considered to be lost. This time is in milliseconds.
        /// </summary>
        public static readonly int CONNECTION_PING_TIMEOUT = ConstantsTable.Get<int>("RC.NetworkingSystem.ConnectionPingTimeout");

        /// <summary>
        /// If more than this time elapses without sending a message to the other side then the connection automatically
        /// sends a ping message. This time is in milliseconds.
        /// </summary>
        public static readonly int CONNECTION_PING_FREQUENCY = ConstantsTable.Get<int>("RC.NetworkingSystem.ConnectionPingFrequency");

        /// <summary>
        /// You can turn off the lobby connection pinging mechanism using this flag.
        /// </summary>
        /// <remarks>Use this flag only for debugging purposes.</remarks>
        public static readonly bool CONNECTION_PING_NOT_IGNORED = ConstantsTable.Get<bool>("RC.NetworkingSystem.ConnectionPingNotIgnored");

        /// <summary>
        /// The elapsed time between the searcher thread tries to receive announcements from the network.
        /// </summary>
        public static readonly int LOBBY_INFO_RECEIVE_FREQUENCY = ConstantsTable.Get<int>("RC.NetworkingSystem.LobbyInfoReceiveFrequency");

        /// <summary>
        /// A registered LobbyInfo will be deleted if no confirmation arrives from the network for a time
        /// given in this timeout parameter (in milliseconds).
        /// </summary>
        public static readonly int LOBBY_INFO_TIMEOUT = ConstantsTable.Get<int>("RC.NetworkingSystem.LobbyInfoTimeout");

        /// <summary>
        /// The connection manager thread tries to accept incoming connection requests with this frequency
        /// in time (milliseconds).
        /// </summary>
        public static readonly int CONNECTION_ACCEPT_FREQUENCY = ConstantsTable.Get<int>("RC.NetworkingSystem.ConnectionAcceptFrequency");

        /// <summary>
        /// The maximum amount of incompleted server tasks.
        /// </summary>
        public static readonly int SERVER_TASK_FIFO_CAPACITY = ConstantsTable.Get<int>("RC.NetworkingSystem.ServerTaskFifoCapacity");
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.NetworkingSystem module.
    /// </summary>
    static class NetworkingSystemTraceFilters
    {
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.NetworkingSystem.Info");
    }
}
