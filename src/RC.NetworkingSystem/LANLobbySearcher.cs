using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Represents a LobbySearcher on the Local Area Network.
    /// </summary>
    class LANLobbySearcher : LobbySearcher
    {
        public LANLobbySearcher(ILobbyLocator listener, List<int> wellKnownBroadcastPorts)
            : base(listener)
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
            this.buffer = new byte[NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE];
        }

        protected override bool Prepare_i()
        {
            this.buffer = new byte[NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE];
            this.udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.udpSocket.EnableBroadcast = true;

            /// Try to find and open an UDP port from the wellKnownBroadcastPorts list.
            bool success = false;
            for (int i = 0; i < this.wellKnownBroadcastPorts.Count; i++)
            {
                try
                {
                    this.udpSocket.Bind(new IPEndPoint(IPAddress.Any, this.wellKnownBroadcastPorts[i]));
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    TraceManager.WriteExceptionAllTrace(ex, false);
                    continue;
                }
            }
            if (!success)
            {
                TraceManager.WriteAllTrace("Unable to bind UDP socket for LANLobbySearcher!", NetworkingSystemTraceFilters.INFO);
                this.udpSocket.Close();
            }
            else
            {
                TraceManager.WriteAllTrace(string.Format("LANLobbySearcher successfully bound to port: {0}", ((IPEndPoint)this.udpSocket.LocalEndPoint).Port), NetworkingSystemTraceFilters.INFO);
            }
            return success;
        }

        protected override RCPackage ReadPackage_i(ref IPAddress sender)
        {
            try
            {
                if (this.udpSocket.Available >= NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE)
                {
                    EndPoint rcvFrom = new IPEndPoint(0, 0);
                    int bytesReceived = this.udpSocket.ReceiveFrom(this.buffer,
                                                                   NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE,
                                                                   SocketFlags.None,
                                                                   ref rcvFrom);
                    if (bytesReceived == NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE)
                    {
                        int parsedBytes = 0;
                        RCPackage retPackage = RCPackage.Parse(this.buffer, 0, NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE, out parsedBytes);
                        if (retPackage != null && retPackage.IsCommitted)
                        {
                            IPEndPoint senderEndpoint = (IPEndPoint)rcvFrom;
                            sender = senderEndpoint.Address;
                            return retPackage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
            }

            sender = IPAddress.Any;
            return null;
        }

        protected override void Close_i()
        {
            this.udpSocket.Close();
        }

        /// <summary>
        /// A list of port numbers that this searcher will try to open for reading.
        /// </summary>
        private List<int> wellKnownBroadcastPorts;

        /// <summary>
        /// This socket is used to read the incoming announcements.
        /// </summary>
        private Socket udpSocket;

        /// <summary>
        /// Buffer used to read incoming UDP packets.
        /// </summary>
        private byte[] buffer;
    }
}
