using System.Threading;
using RC.Common;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Internal class for announcing a local lobby.
    /// </summary>
    public abstract class LobbyAnnouncer
    {
        /// <summary>
        /// Constructs a new lobby announcer object.
        /// </summary>
        /// <param name="lobby">The lobby that this object will announce.</param>
        /// <param name="customDataProvider">The object that provides additional informations to the announcement.</param>
        public LobbyAnnouncer(LobbyServer lobby, ILobbyCustomDataProvider customDataProvider)
        {
            if (lobby == null) { throw new NetworkingSystemException("lobby"); }

            this.announcedLobby = lobby;
            this.customDataProvider = customDataProvider;
            this.announcerThread = null;
            this.announcementFinished = null;
        }

        /// <summary>
        /// Starts announcing the lobby.
        /// </summary>
        public bool Start()
        {
            if (this.announcerThread == null)
            {
                bool prepared = Prepare_i();
                if (prepared)
                {
                    this.announcementFinished = new ManualResetEvent(false);
                    this.announcerThread = new RCThread(AnnounceProc, "LobbyAnnouncer");
                    this.announcerThread.Start();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Stops the announcer.
        /// </summary>
        public void Stop()
        {
            if (this.announcerThread != null)
            {
                this.announcementFinished.Set();
                this.announcerThread.Join();
                Close_i();
                this.announcerThread = null;
                this.announcementFinished.Close();
                this.announcementFinished = null;
            }
        }

        /// <summary>
        /// Prepares the underlying network environment to announce a lobby.
        /// </summary>
        /// <returns>True in case of success, false otherwise.</returns>
        protected abstract bool Prepare_i();

        /// <summary>
        /// Broadcasts the given package on the underlying network.
        /// </summary>
        /// <param name="package">The package to broadcast.</param>
        protected abstract void BroadcastPackage_i(RCPackage package);

        /// <summary>
        /// Closes any necessary resources on the underlying network environment.
        /// </summary>
        protected abstract void Close_i();

        /// <summary>
        /// The starting function of the announcer thread.
        /// </summary>
        private void AnnounceProc()
        {
            while (!this.announcementFinished.WaitOne(NetworkingSystemConstants.ANNOUNCEMENT_FREQUENCY))
            {
                LobbyInfo lobbyInfo =
                    new LobbyInfo(this.announcedLobby.Id, "", this.announcedLobby.PortNum);

                if (null != this.customDataProvider)
                {
                    lobbyInfo.CustomData = this.customDataProvider.CustomData;
                }

                RCPackage lobbyInfoPackage = lobbyInfo.Package;
                BroadcastPackage_i(lobbyInfoPackage);
            }

            /// Announce finished so we broadcast a FORMAT_LOBBY_INFO_VANISHED message.
            RCPackage lobbyVanishedPackage = RCPackage.CreateCustomDataPackage(Network.FORMAT_LOBBY_INFO_VANISHED);
            lobbyVanishedPackage.WriteString(0, this.announcedLobby.Id.ToString());
            for (int i = 0; i < NetworkingSystemConstants.ANNOUNCEMENT_BROADCAST_MULTIPLICITY; i++)
            {
                /// Broadcast more than once because of possible package lost on UDP.
                BroadcastPackage_i(lobbyVanishedPackage);
            }
        }

        /// <summary>
        /// The lobby that is being announced by this object.
        /// </summary>
        private LobbyServer announcedLobby;

        /// <summary>
        /// Reference to an object that can provide custom data for the lobby announcement.
        /// </summary>
        private ILobbyCustomDataProvider customDataProvider;

        /// <summary>
        /// The thread that announces the lobby.
        /// </summary>
        private RCThread announcerThread;

        /// <summary>
        /// This event will be signaled when the announcer thread should stop working.
        /// </summary>
        private ManualResetEvent announcementFinished;
    }
}
