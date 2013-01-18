using System;
using System.Collections.Generic;
using System.Net.Sockets;
using RC.Common;
using System.Net;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Represents a LobbyServer on the Local Area Network.
    /// </summary>
    class LANLobbyServer : LobbyServer
    {
        /// <summary>
        /// Constructs a LANLobbyServer object.
        /// </summary>
        public LANLobbyServer(int maxClients, ILobbyListener listener, List<int> wellKnownBroadcastPorts)
            : base(maxClients, listener)
        {
            /// Copy the list of the well-known broadcast ports
            this.wellKnownBroadcastPorts = new List<int>();
            foreach (int p in wellKnownBroadcastPorts)
            {
                this.wellKnownBroadcastPorts.Add(p);
            }

            int port;
            bool listenerStarted = false;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    port = RandomService.DefaultGenerator.Next(32768, IPEndPoint.MaxPort);
                    this.connListener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
                    this.connListener.ExclusiveAddressUse = false;
                    this.connListener.Start(maxClients);
                    listenerStarted = true;
                    break;
                }
                catch (Exception ex)
                {
                    TraceManager.WriteExceptionAllTrace(ex, false);
                }
            }
            if (!listenerStarted)
            {
                throw new NetworkingSystemException("Unable to start listener 10 times.");
            }
        }

        /// <summary>
        /// Gets the TCPListener object of this LANLobbyServer.
        /// </summary>
        public TcpListener TCPListener { get { return this.connListener; } }

        /// <see cref="LobbyServer.PortNum"/>
        public override int PortNum
        {
            get { return ((IPEndPoint)this.connListener.LocalEndpoint).Port; }
        }

        /// <see cref="LobbyServer.CreateAnnouncer_i"/>
        protected override LobbyAnnouncer CreateAnnouncer_i(ILobbyCustomDataProvider customDataProvider)
        {
            LobbyAnnouncer ann = new LANLobbyAnnouncer(this, customDataProvider, this.wellKnownBroadcastPorts);
            return ann;
        }

        /// <see cref="LobbyServer.CreateConnection_i"/>
        protected override LobbyConnection CreateConnection_i()
        {
            LobbyConnection conn = new LANLobbyConnection(this);
            return conn;
        }

        /// <summary>
        /// Listens for incoming connections.
        /// </summary>
        private TcpListener connListener;

        /// <summary>
        /// A list of port numbers that this LANLobbyServer might use as a target of broadcast messages (for
        /// example: announce lobby informations).
        /// </summary>
        private List<int> wellKnownBroadcastPorts;
    }
}
