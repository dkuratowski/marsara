using System;
using System.Collections.Generic;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    class LocalAreaNetwork : Network
    {
        /// <see cref="Network.CreateLocalAreaNetwork"/>
        public LocalAreaNetwork(List<int> wellKnownBroadcastPorts)
            : base()
        {
            if (wellKnownBroadcastPorts == null || wellKnownBroadcastPorts.Count == 0)
            {
                throw new ArgumentNullException("wellKnownBroadcastPorts");
            }

            this.wellKnownBroadcastPorts = new List<int>();
            foreach (int port in wellKnownBroadcastPorts)
            {
                this.wellKnownBroadcastPorts.Add(port);
            }
        }

        /// <see cref="Network.CreateSearcher_i"/>
        protected override LobbySearcher CreateSearcher_i(ILobbyLocator listener)
        {
            return new LANLobbySearcher(listener, this.wellKnownBroadcastPorts);
        }

        /// <see cref="Network.CreateLocalLobby_i"/>
        protected override LobbyServer CreateLocalLobby_i(int maxClients, ILobbyListener listener)
        {
            try
            {
                LobbyServer server = new LANLobbyServer(maxClients, listener, this.wellKnownBroadcastPorts);
                return server;
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
                return null;
            }
        }

        /// <see cref="Network.CreateRemoteLobby_i"/>
        protected override LobbyClient CreateRemoteLobby_i(LobbyInfo info, ILobbyListener listener)
        {
            try
            {
                LobbyClient client = new LANLobbyClient(info, listener);
                return client;
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
                return null;
            }
        }

        /// <summary>
        /// A list of port numbers that this LocalAreaNetwork might use for listening broadcast messages (for
        /// example: announced lobby informations).
        /// </summary>
        private List<int> wellKnownBroadcastPorts;
    }
}
