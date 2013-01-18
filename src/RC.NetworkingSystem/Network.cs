using System;
using System.Collections.Generic;
using RC.Common.Diagnostics;
using RC.Common.Configuration;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// This is a singleton class that implements the INetwork interface. This class supports functions to initialize
    /// a concrete network environment (for example: a Local Area Network).
    /// </summary>
    /// <remarks>
    /// WARNING! You have to create the underlying network object before you register any own RCPackageFormat because
    /// the networking system also defines it's internal formats and the formatIDs must be the same on every peer!!!
    /// </remarks>
    public abstract class Network : INetwork
    {
        #region Static members

        /// <summary>
        /// Gets the current network object.
        /// </summary>
        public static INetwork Instance { get { return currentNetwork; } }

        /// <summary>
        /// Creates a LocalAreaNetwork object.
        /// </summary>
        /// <param name="wellKnownBroadcastPorts">
        /// A list of port numbers that the created LocalAreaNetwork might use for listening broadcast messages (for
        /// example: announced lobby informations).
        /// </param>
        /// <returns>The interface of the created LocalAreaNetwork object.</returns>
        /// <exception cref="NetworkingSystemException">
        /// If a Network object already exists.
        /// </exception>
        public static INetwork CreateLocalAreaNetwork(List<int> wellKnownBroadcastPorts)
        {
            if (null == currentNetwork)
            {
                LocalAreaNetwork createdNetwork = null;
                try
                {
                    createdNetwork = new LocalAreaNetwork(wellKnownBroadcastPorts);
                    currentNetwork = createdNetwork;
                    return createdNetwork;
                }
                catch (Exception ex)
                {
                    TraceManager.WriteExceptionAllTrace(ex, false);
                    if (null != createdNetwork)
                    {
                        createdNetwork.ShutdownNetwork();
                        currentNetwork = null;
                    }
                }
                return null;
            }
            else
            {
                throw new NetworkingSystemException("Network object already exists!");
            }
        }

        /// <summary>
        /// Initializes the Network class.
        /// </summary>
        static Network()
        {
            FORMAT_LOBBY_INFO = RCPackageFormatMap.Get("RC.NetworkingSystem.LobbyInfo");
            FORMAT_LOBBY_INFO_VANISHED = RCPackageFormatMap.Get("RC.NetworkingSystem.LobbyInfoVanished");
            FORMAT_LOBBY_LINE_STATE_REPORT = RCPackageFormatMap.Get("RC.NetworkingSystem.LobbyLineStateReport");
            FORMAT_DISCONNECT_INDICATOR = RCPackageFormatMap.Get("RC.NetworkingSystem.DisconnectIndicator");
            FORMAT_DISCONNECT_ACK = RCPackageFormatMap.Get("RC.NetworkingSystem.DisconnectAcknowledge");
            FORMAT_DEDICATED_MESSAGE = RCPackageFormatMap.Get("RC.NetworkingSystem.DedicatedMessage");
        }

        /// <summary>
        /// Internal RCPackageFormat definitions.
        /// </summary>
        public static readonly int FORMAT_LOBBY_INFO;              /// LobbyInfo broadcast
        public static readonly int FORMAT_LOBBY_INFO_VANISHED;     /// Lobby vanished broadcast
        public static readonly int FORMAT_LOBBY_LINE_STATE_REPORT; /// Line state report package format
        public static readonly int FORMAT_DISCONNECT_INDICATOR;    /// Disconnection indicator package format
        public static readonly int FORMAT_DISCONNECT_ACK;          /// Disconnection acknowledgement package format
        public static readonly int FORMAT_DEDICATED_MESSAGE;       /// Dedicated message format

        /// <summary>
        /// Reference to the currently active network.
        /// </summary>
        private static INetwork currentNetwork = null;

        #endregion

        /// <summary>
        /// Delegate for functions that want to handle disposing events.
        /// </summary>
        public delegate void DisposedHandler(object sender);

        #region INetwork methods

        /// <see cref="INetwork.StartLocatingLobbies"/>
        public bool StartLocatingLobbies(ILobbyLocator locator)
        {
            if (this.closed) { throw new ObjectDisposedException("Network"); }
            if (locator == null) { throw new ArgumentNullException("listener"); }

            if (this.searcher == null)
            {
                this.searcher = CreateSearcher_i(locator);
                bool success = this.searcher.Start();
                if (!success)
                {
                    this.searcher.Stop();
                    this.searcher = null;
                }
                return success;
            }
            return false;
        }

        /// <see cref="INetwork.StopLocatingLobbies"/>
        public void StopLocatingLobbies()
        {
            if (this.closed) { throw new ObjectDisposedException("Network"); }

            if (this.searcher != null)
            {
                this.searcher.Stop();
                this.searcher = null;
            }
        }

        /// <see cref="INetwork.CreateLobby"/>
        public ILobbyServer CreateLobby(int maxClients, ILobbyListener listener)
        {
            if (this.closed) { throw new ObjectDisposedException("Network"); }
            if (maxClients < 1) { throw new ArgumentOutOfRangeException("maxClients"); }
            if (listener == null) { throw new ArgumentNullException("listener"); }

            if (this.localLobby == null && this.remoteLobby == null)
            {
                LobbyServer lobbyServer = CreateLocalLobby_i(maxClients, listener);
                lobbyServer.Disposed += this.LobbyDisposedHandler;
                this.localLobby = lobbyServer;
                return this.localLobby;
            }
            else
            {
                return null;
            }
        }

        /// <see cref="INetwork.JoinLobby"/>
        public ILobbyClient JoinLobby(LobbyInfo info, ILobbyListener listener)
        {
            if (this.closed) { throw new ObjectDisposedException("Network"); }
            if (listener == null) { throw new ArgumentNullException("listener"); }
            if (info == null) { throw new ArgumentNullException("info"); }

            if (this.localLobby == null && this.remoteLobby == null)
            {
                LobbyClient lobbyClient = CreateRemoteLobby_i(info, listener);
                lobbyClient.Disposed += this.LobbyDisposedHandler;
                this.remoteLobby = lobbyClient;
                return this.remoteLobby;
            }
            else
            {
                return null;
            }
        }

        /// <see cref="INetwork.CloseNetwork"/>
        public void ShutdownNetwork()
        {
            if (this.closed) { return; }

            this.StopLocatingLobbies();

            if (this.localLobby != null && this.remoteLobby == null)
            {
                /// Shutdown the lobbyserver if exists.
                this.localLobby.Shutdown();
            }
            else if (this.localLobby == null && this.remoteLobby != null)
            {
                /// Disconnect from the remote lobby if exists.
                this.remoteLobby.Disconnect();
            }

            /// Kill the instance
            currentNetwork = null;

            this.closed = true;
        }

        #endregion

        /// <summary>
        /// Constructs a Network object.
        /// </summary>
        protected Network()
        {
            this.closed = false;
            this.localLobby = null;
            this.remoteLobby = null;
            this.searcher = null;
        }

        /// <summary>
        /// Internal function to create a lobby searcher object.
        /// </summary>
        protected abstract LobbySearcher CreateSearcher_i(ILobbyLocator listener);

        /// <summary>
        /// Internal function to create a local lobby.
        /// </summary>
        protected abstract LobbyServer CreateLocalLobby_i(int maxClients, ILobbyListener listener);

        /// <summary>
        /// Internal function to create a remote lobby.
        /// </summary>
        protected abstract LobbyClient CreateRemoteLobby_i(LobbyInfo info, ILobbyListener listener);

        /// <summary>
        /// This event handler function is called by the current LobbyServer (or LobbyClient) if it has been
        /// shutdown (or disconnected).
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        private void LobbyDisposedHandler(object sender)
        {
            if (this.localLobby == sender || this.remoteLobby == sender)
            {
                this.localLobby = null;
                this.remoteLobby = null;
            }
        }

        /// <summary>
        /// The lobby that is created by this peer or null if there is no lobby created by this peer.
        /// </summary>
        private ILobbyServer localLobby;

        /// <summary>
        /// The lobby that is created by another peer and this peer has joined to it or null if there is no such a lobby.
        /// </summary>
        private ILobbyClient remoteLobby;

        /// <summary>
        /// The lobby searcher object or null if there is no searching operation is in progress.
        /// </summary>
        private LobbySearcher searcher;

        /// <summary>
        /// This flag becomes true if you close this network.
        /// </summary>
        private bool closed;
    }
}
