using System;
using System.Collections.Generic;
using System.Net.Sockets;
using RC.Common;
using System.Net;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Represents a LobbyAnnouncer on the Local Area Network.
    /// </summary>
    class LANLobbyAnnouncer : LobbyAnnouncer
    {
        public LANLobbyAnnouncer(LobbyServer lobby, ILobbyCustomDataProvider customDataProvider, List<int> wellKnownBroadcastPorts)
            : base(lobby, customDataProvider)
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

        protected override bool Prepare_i()
        {
            this.buffer = new byte[NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE];
            this.udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.udpSocket.EnableBroadcast = true;

            /// Try to find and open a random UDP port.
            bool success = false;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    this.udpSocket.Bind(new IPEndPoint(IPAddress.Any,
                                                       RandomService.DefaultGenerator.Next(32768, IPEndPoint.MaxPort)));
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
                TraceManager.WriteAllTrace("Unable to bind UDP socket for LANLobbyAnnouncer!", NetworkingSystemTraceFilters.INFO);
                this.udpSocket.Close();
            }
            else
            {
                TraceManager.WriteAllTrace(string.Format("LANLobbyAnnouncer successfully bound to port: {0}", ((IPEndPoint)this.udpSocket.LocalEndPoint).Port), NetworkingSystemTraceFilters.INFO);
            }
            return success;
        }

        protected override void BroadcastPackage_i(RCPackage package)
        {
            try
            {
                package.WritePackageToBuffer(this.buffer, 0);
                foreach (int port in this.wellKnownBroadcastPorts)
                {
                    this.udpSocket.SendTo(buffer, NetworkingSystemConstants.ANNOUNCEMENT_UDP_POCKET_SIZE, SocketFlags.None,
                                          new IPEndPoint(IPAddress.Broadcast, port));
                }
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
            }
        }

        protected override void Close_i()
        {
            this.udpSocket.Close();
        }

        /// <summary>
        /// A list of port numbers that this announcer will broadcast to.
        /// </summary>
        private List<int> wellKnownBroadcastPorts;

        /// <summary>
        /// This socket is used to broadcast the messages.
        /// </summary>
        private Socket udpSocket;

        /// <summary>
        /// The buffer that will be broadcasted.
        /// </summary>
        private byte[] buffer;
    }
}
