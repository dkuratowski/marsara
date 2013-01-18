using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Net;
using RC.Common;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Internal class that is used to search lobbies on the network announced by other peers.
    /// </summary>
    public abstract class LobbySearcher
    {
        /// <summary>
        /// Constructs a LobbyListener object.
        /// </summary>
        /// <param name="locator">
        /// A listener object that will be notified about the lobbies announced on the network.
        /// </param>
        public LobbySearcher(ILobbyLocator locator)
        {
            if (locator == null) { throw new ArgumentNullException("locator"); }

            this.locator = locator;
            this.collectedInfos = new Dictionary<Guid, LobbyInfo>();
            this.timers = new Dictionary<Guid, Stopwatch>();
            this.searcherThread = null;
            this.searchFinished = null;
        }

        /// <summary>
        /// Starts searching lobbies on the network.
        /// </summary>
        public bool Start()
        {
            if (this.searcherThread == null)
            {
                bool prepared = Prepare_i();
                if (prepared)
                {
                    this.searchFinished = new ManualResetEvent(false);
                    this.searcherThread = new RCThread(SearchProc, "LobbySearcher");
                    this.searcherThread.Start();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Stops the searcher.
        /// </summary>
        public void Stop()
        {
            if (this.searcherThread != null)
            {
                this.searchFinished.Set();
                this.searcherThread.Join();
                Close_i();
                this.searcherThread = null;
                this.searchFinished.Close();
                this.searchFinished = null;
            }
        }

        /// <summary>
        /// Prepares the underlying network environment to the search operation.
        /// </summary>
        /// <returns>True in case of success, false otherwise.</returns>
        protected abstract bool Prepare_i();

        /// <summary>
        /// Try to read an incoming RCPackage from the underlying network.
        /// </summary>
        /// <param name="sender">The IP address of the sender.</param>
        /// <returns>
        /// The RCPackage has been read or null if no RCPackage has been arrived.
        /// </returns>
        protected abstract RCPackage ReadPackage_i(ref IPAddress sender);

        /// <summary>
        /// Closes any necessary resources on the underlying network environment.
        /// </summary>
        protected abstract void Close_i();

        /// <summary>
        /// The starting function of the searcher thread.
        /// </summary>
        private void SearchProc()
        {
            while (!this.searchFinished.WaitOne(NetworkingSystemConstants.LOBBY_INFO_RECEIVE_FREQUENCY))
            {
                /// Read an RCPackage from the network.
                IPAddress sender = IPAddress.Any;
                RCPackage announcementPackage = ReadPackage_i(ref sender);
                while (announcementPackage != null)
                {
                    if (announcementPackage.PackageType == RCPackageType.CUSTOM_DATA_PACKAGE)
                    {
                        if (announcementPackage.PackageFormat.ID == Network.FORMAT_LOBBY_INFO)
                        {
                            /// Try to create a LobbyInfo object from the package.
                            LobbyInfo info = LobbyInfo.FromRCPackage(announcementPackage);
                            if (info != null && info.ID != Guid.Empty)
                            {
                                info.IPAddress = sender.ToString();
                                /// Search in the local registry.
                                if (this.collectedInfos.ContainsKey(info.ID) && this.timers.ContainsKey(info.ID))
                                {
                                    /// If found we check if it has been changed since last check.
                                    bool changed = this.collectedInfos[info.ID].IPAddress.CompareTo(info.IPAddress) != 0 ||
                                                   this.collectedInfos[info.ID].PortNumber != info.PortNumber;

                                    /// Check for differences in the custom data.
                                    if (info.CustomData != null)
                                    {
                                        if (this.collectedInfos[info.ID].CustomData != null)
                                        {
                                            if (!RCPackage.IsEqual(this.collectedInfos[info.ID].CustomData, info.CustomData))
                                            {
                                                changed = true;
                                            }
                                        }
                                        else
                                        {
                                            /// Custom data appeared.
                                            changed = true;
                                        }
                                    }
                                    else
                                    {
                                        if (this.collectedInfos[info.ID].CustomData != null)
                                        {
                                            /// Custom data disappeared.
                                            changed = true;
                                        }
                                    }

                                    this.collectedInfos[info.ID] = info;    /// save the info
                                    this.timers[info.ID].Restart();         /// restarts it's timer
                                    /// notify the listener that the lobby has changed
                                    if (changed) { this.locator.LobbyChanged(info); }
                                }
                                else
                                {
                                    /// If not found...
                                    this.collectedInfos.Add(info.ID, info);     /// save the info
                                    this.timers.Add(info.ID, new Stopwatch());  /// create timer for the info
                                    this.timers[info.ID].Start();               /// start timer for the info
                                    /// notify the listener that a new lobby has been found
                                    this.locator.LobbyFound(info);
                                }
                            }
                        }
                        else if (announcementPackage.PackageFormat.ID == Network.FORMAT_LOBBY_INFO_VANISHED)
                        {
                            string idStr = announcementPackage.ReadString(0);
                            Guid id;
                            if (Guid.TryParse(idStr, out id) && id != Guid.Empty)
                            {
                                /// Search in the local registry.
                                if (this.collectedInfos.ContainsKey(id) && this.timers.ContainsKey(id))
                                {
                                    LobbyInfo vanishedLobby = this.collectedInfos[id];
                                    this.collectedInfos.Remove(id);
                                    this.timers.Remove(id);
                                    this.locator.LobbyVanished(vanishedLobby);
                                }
                            }
                        }
                    }

                    announcementPackage = ReadPackage_i(ref sender);
                } /// end-while

                /// Check the timers.
                List<Guid> removedKeys = new List<Guid>();
                foreach (KeyValuePair<Guid, Stopwatch> timer in this.timers)
                {
                    if (timer.Value.ElapsedMilliseconds > NetworkingSystemConstants.LOBBY_INFO_TIMEOUT)
                    {
                        removedKeys.Add(timer.Key);
                    }
                }
                foreach (Guid key in removedKeys)
                {
                    LobbyInfo vanishedLobby = this.collectedInfos[key];
                    this.collectedInfos.Remove(key);
                    this.timers.Remove(key);
                    this.locator.LobbyVanished(vanishedLobby);
                }
            }
        }

        /// <summary>
        /// This object will be notified about the lobbies announced on the network.
        /// </summary>
        private ILobbyLocator locator;

        /// <summary>
        /// The LobbyInfos that has been collected from the network.
        /// </summary>
        private Dictionary<Guid, LobbyInfo> collectedInfos;

        /// <summary>
        /// Timers that are used to notice if a lobby is no longer announced on the network.
        /// </summary>
        private Dictionary<Guid, Stopwatch> timers;

        /// <summary>
        /// The thread that searches the lobbies.
        /// </summary>
        private RCThread searcherThread;

        /// <summary>
        /// This event will be signaled when the searcher thread should stop working.
        /// </summary>
        private ManualResetEvent searchFinished;
    }
}
