using System.Collections.Generic;
using RC.Common;
using System.Diagnostics;
using System.Net;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Enumerates the possible types of a LobbyConnection.
    /// </summary>
    public enum LobbyConnectionType
    {
        CLIENT_SIDE = 0,        /// Indicates the client side of a LobbyConnection.
        SERVER_SIDE = 1         /// Indicates the server side of a LobbyConnection.
    }

    /// <summary>
    /// Enumerates the possible states of the connection between a client and the server.
    /// </summary>
    public enum LobbyConnectionState
    {
        Disconnected = 0,       /// The connection is inactive (default state).
        Connecting = 1,         /// The connection is being activated.
        Connected = 2,          /// The connection has been activated.
        Disconnecting = 3       /// The connection is being deactivated.
    }

    /// <summary>
    /// Internal class that represents a connection between this peer and another.
    /// </summary>
    public abstract class LobbyConnection
    {
        /// <summary>
        /// Constructs a LobbyConnection object.
        /// </summary>
        public LobbyConnection(LobbyConnectionType side)
        {
            this.lineState = LobbyLineState.Opened;
            this.connectionState = LobbyConnectionState.Disconnected;
            this.side = side;
            this.lastIncomingMsgTimer = new Stopwatch();
            this.lastOutgoingMsgTimer = new Stopwatch();
            this.disconnectAckMsgTimer = new Stopwatch();
            this.connectAckMsgTimer = new Stopwatch();
        }

        /// <summary>
        /// Tries to accept the next incoming connection.
        /// </summary>
        /// <remarks>
        /// Calling this function is only allowed on opened, disconnected, server-side connections.
        /// This function is called by the connection manager thread from the corresponding LobbyServer.
        /// </remarks>
        public bool TryAcceptNextConnection()
        {
            if (this.side != LobbyConnectionType.SERVER_SIDE)
            {
                throw new NetworkingSystemException("Accepting connections is only allowed on server side LobbyConnections!");
            }
            if (this.lineState != LobbyLineState.Opened)
            {
                throw new NetworkingSystemException("Accepting connections is only allowed on opened LobbyConnections!");
            }
            if (this.connectionState != LobbyConnectionState.Disconnected)
            {
                throw new NetworkingSystemException("Accepting connections is only allowed on disconnected LobbyConnections!");
            }

            bool accepted = TryAcceptNextConnection_i();
            if (accepted)
            {
                this.lineState = LobbyLineState.Engaged;
                this.connectionState = LobbyConnectionState.Connected;
                this.lastIncomingMsgTimer.Start();
                this.lastOutgoingMsgTimer.Start();
            }
            return accepted;
        }

        /// <summary>
        /// Begins the connection procedure to the given server.
        /// </summary>
        /// <returns>True if the connection procedure has been successfully started, false otherwise.</returns>
        /// <remarks>
        /// Calling this function is only allowed on disconnected, client-side connections.
        /// This function is called by the connection manager thread from the corresponding LobbyClient.
        /// </remarks>
        public bool BeginConnectToTheServer(IPEndPoint server)
        {
            if (this.side != LobbyConnectionType.CLIENT_SIDE)
            {
                throw new NetworkingSystemException("Connect to server is only allowed on client side LobbyConnections!");
            }
            if (this.connectionState != LobbyConnectionState.Disconnected)
            {
                throw new NetworkingSystemException("Connect to server is only allowed on disconnected LobbyConnections!");
            }

            if (!TryConnectToTheServer_i(server))
            {
                return false;
            }

            this.connectionState = LobbyConnectionState.Connecting;
            this.connectAckMsgTimer.Start();
            return true;
        }

        /// <summary>
        /// Continues the connection procedure to the server.
        /// </summary>
        /// <returns>False in case of any error or timeout, true otherwise.</returns>
        /// <remarks>
        /// Calling this function is only allowed on connecting, client-side connections.
        /// This function is called by the connection manager thread from the corresponding LobbyClient.
        /// After calling this function you have to check whether the connection procedure has been finished.
        /// You can do this by reading the out parameter lineStateReport. If this is not null then the
        /// connection procedure has been finished, otherwise you have to wait.
        /// </remarks>
        public bool ContinueConnectToTheServer(out RCPackage lineStateReport)
        {
            if (this.side != LobbyConnectionType.CLIENT_SIDE)
            {
                throw new NetworkingSystemException("Continue connect to server is only allowed on client side LobbyConnections!");
            }
            if (this.connectionState != LobbyConnectionState.Connecting)
            {
                throw new NetworkingSystemException("Continue connect to server is only allowed on connecting LobbyConnections!");
            }

            lineStateReport = null;
            RCPackage incomingMessage = null;
            do
            {
                if (ReceivePackage_i(out incomingMessage))
                {
                    if (incomingMessage != null)
                    {
                        if (incomingMessage.IsCommitted && incomingMessage.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                            incomingMessage.PackageFormat.ID == Network.FORMAT_LOBBY_LINE_STATE_REPORT)
                        {
                            lineStateReport = incomingMessage;
                            this.connectAckMsgTimer.Stop();
                            this.connectAckMsgTimer.Reset();
                            this.connectionState = LobbyConnectionState.Connected;
                            this.lastIncomingMsgTimer.Start();
                            this.lastOutgoingMsgTimer.Start();
                            return true;
                        }
                        else if (incomingMessage.IsCommitted && incomingMessage.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                                 incomingMessage.PackageFormat.ID == Network.FORMAT_DISCONNECT_INDICATOR)
                        {
                            /// Connection request is rejected by the server.
                            this.connectAckMsgTimer.Stop();
                            this.connectAckMsgTimer.Reset();
                            this.connectionState = LobbyConnectionState.Disconnected;
                            return false;
                        }
                    }

                }
                else
                {
                    /// Syntax error --> immediate shutdown
                    return false;
                }

            } while (incomingMessage != null);

            if (this.connectAckMsgTimer.ElapsedMilliseconds > NetworkingSystemConstants.CONNECT_ACK_TIMEOUT)
            {
                /// Connection timeout --> immedate shutdown
                return false;
            }
            else
            {
                /// Continue the connection procedure a bit later.
                return true;
            }
        }

        /// <summary>
        /// Receives all incoming packages from the network.
        /// </summary>
        /// <param name="incomingPackages">This list will contain the incoming packages.</param>
        /// <returns>
        /// True if everything was OK or false if the connection has been timed out or there was a syntax or any
        /// other unexpected error in the incoming byte stream.
        /// </returns>
        /// <remarks>
        /// In case of any error the LobbyConnection automatically shuts down itself.
        /// </remarks>
        public bool ReceiveIncomingPackages(ref List<RCPackage> incomingPackages)
        {
            incomingPackages.Clear();

            RCPackage incomingPackage = null;
            bool error = false;
            do
            {
                if (ReceivePackage_i(out incomingPackage))
                {
                    if (incomingPackage != null)
                    {
                        if (incomingPackage.PackageType == RCPackageType.NETWORK_PING_PACKAGE)
                        {
                            /// Eat the ping message
                            this.lastIncomingMsgTimer.Restart();
                        }
                        else if (incomingPackage.PackageType == RCPackageType.NETWORK_CUSTOM_PACKAGE ||
                                 incomingPackage.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE)
                        {
                            /// This is a custom or an internal message --> OK
                            incomingPackages.Add(incomingPackage);
                            this.lastIncomingMsgTimer.Restart();
                        }
                        else
                        {
                            /// Unexpected package type
                            error = true;
                        }
                    }
                    else
                    {
                        if (this.lastIncomingMsgTimer.ElapsedMilliseconds > NetworkingSystemConstants.CONNECTION_PING_TIMEOUT &&
                            NetworkingSystemConstants.CONNECTION_PING_NOT_IGNORED)
                        {
                            /// Timeout
                            error = true;
                        }
                    }
                }
                else
                {
                    /// Syntax error in the byte sequence
                    error = true;
                }
            } while (incomingPackage != null);

            return !error;
        }

        /// <summary>
        /// Sends a package through this connection.
        /// </summary>
        /// <param name="package">The package you want to send.</param>
        /// <returns>True if the send was successful, false in case of any error.</returns>
        public bool SendPackage(RCPackage package)
        {
            if (this.connectionState == LobbyConnectionState.Connected)
            {
                if (SendPackage_i(package))
                {
                    this.lastOutgoingMsgTimer.Restart();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This function sends a ping to the other side of this connection if there was no outgoing message
        /// for a given time.
        /// </summary>
        /// <returns>
        /// In case of any error, this function returns false. In any other case this function returns true.
        /// </returns>
        public bool SendPingIfNecessary()
        {
            if (this.connectionState == LobbyConnectionState.Connected)
            {
                /// Send a ping if necessary
                if (this.lastOutgoingMsgTimer.ElapsedMilliseconds > NetworkingSystemConstants.CONNECTION_PING_FREQUENCY)
                {
                    RCPackage pingPackage = RCPackage.CreateNetworkPingPackage();
                    if (SendPackage_i(pingPackage))
                    {
                        this.lastOutgoingMsgTimer.Restart();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Shuts down this connection immediately (in case of error).
        /// </summary>
        public void Shutdown()
        {
            Shutdown_i();
            this.connectionState = LobbyConnectionState.Disconnected;
            this.lineState = LobbyLineState.Closed;
            this.lastIncomingMsgTimer.Stop();
            this.lastIncomingMsgTimer.Reset();
            this.lastOutgoingMsgTimer.Stop();
            this.lastOutgoingMsgTimer.Reset();
            this.disconnectAckMsgTimer.Stop();
            this.disconnectAckMsgTimer.Reset();
        }

        /// <summary>
        /// Starts the disconnection procedure of this connection.
        /// </summary>
        /// <returns>True if the disconnection procedure has been successfully started, false otherwise.</returns>
        /// <remarks>
        /// If this function returns false that means that an underlying network error occured and the connection
        /// has been shutdown immediately.
        /// </remarks>
        public bool BeginDisconnect()
        {
            if (this.connectionState == LobbyConnectionState.Connected)
            {
                /// Send a disconnect indicator message to the other side.
                RCPackage disconnectIndicator = RCPackage.CreateNetworkControlPackage(Network.FORMAT_DISCONNECT_INDICATOR);
                disconnectIndicator.WriteString(0, string.Empty);
                disconnectIndicator.WriteByteArray(1, new byte[0] { });
                if (!SendPackage(disconnectIndicator))
                {
                    /// Shutdown the connection immediately in case of any error.
                    Shutdown();
                    return false;
                }
                this.connectionState = LobbyConnectionState.Disconnecting;
                this.disconnectAckMsgTimer.Start();
                return true;
            }
            else
            {
                throw new NetworkingSystemException("Unexpected call to LobbyConnection.BeginDisconnect()!");
            }
        }

        /// <summary>
        /// Continues the disconnection procedure of this connection.
        /// </summary>
        /// <returns>
        /// True if the disconnection procedure has been finished, false otherwise.
        /// </returns>
        public bool ContinueDisconnect()
        {
            if (this.connectionState == LobbyConnectionState.Disconnecting)
            {
                List<RCPackage> incomingPackages = new List<RCPackage>();
                if (ReceiveIncomingPackages(ref incomingPackages))
                {
                    foreach (RCPackage package in incomingPackages)
                    {
                        if (package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                            (package.PackageFormat.ID == Network.FORMAT_DISCONNECT_ACK ||
                             package.PackageFormat.ID == Network.FORMAT_DISCONNECT_INDICATOR))
                        {
                            /// Disconnect ack received --> Normal shutdown.
                            Shutdown();
                            return true;
                        }
                    }
                    if (this.disconnectAckMsgTimer.ElapsedMilliseconds > NetworkingSystemConstants.DISCONNECT_ACK_TIMEOUT)
                    {
                        /// Disconnect ack timeout --> Finish with an immediate shutdown.
                        Shutdown();
                        return true;
                    }

                    /// Disconnection has not yet been finished.
                    return false;
                }
                else
                {
                    /// Receive error --> Finish with an immediate shutdown.
                    Shutdown();
                    return true;
                }
            }
            else
            {
                throw new NetworkingSystemException("Unexpected call to LobbyConnection.ContinueDisconnect()!");
            }
        }

        /// <summary>
        /// Gets or sets the state of the line represented by this connection.
        /// </summary>
        public LobbyLineState LineState
        {
            get
            {
                return this.lineState;
            }
            set
            {
                this.lineState = value;
            }
        }

        /// <summary>
        /// Gets the state of this connection.
        /// </summary>
        public LobbyConnectionState ConnectionState { get { return this.connectionState; } }

        /// <summary>
        /// Internal function to accept the next incoming connection. If there is no incoming connection, then this function
        /// has no effect.
        /// </summary>
        protected abstract bool TryAcceptNextConnection_i();

        /// <summary>
        /// Internal function to connect to an IPEndPoint.
        /// </summary>
        protected abstract bool TryConnectToTheServer_i(IPEndPoint server);

        /// <summary>
        /// Internal function to send the message out to the network.
        /// </summary>
        protected abstract bool SendPackage_i(RCPackage packageToSend);

        /// <summary>
        /// Internal function to receive a package from the network.
        /// </summary>
        /// <param name="receivedPackage">
        /// The package that has been received or null if no incoming package is available on the network.
        /// </param>
        /// <returns>True if everything was OK, false in case of syntax error.</returns>
        protected abstract bool ReceivePackage_i(out RCPackage receivedPackage);

        /// <summary>
        /// Internal function to shutdown this connection.
        /// </summary>
        protected abstract void Shutdown_i();

        /// <summary>
        /// Indicates whether this is the server or the client side of the LobbyConnection.
        /// </summary>
        private LobbyConnectionType side;

        /// <summary>
        /// State of the line that is represented by this connection.
        /// </summary>
        private LobbyLineState lineState;

        /// <summary>
        /// State of this connection.
        /// </summary>
        private LobbyConnectionState connectionState;

        /// <summary>
        /// This timer is used to measure the time elapsed since the last message has been arrived from the other
        /// side of this connection.
        /// </summary>
        private Stopwatch lastIncomingMsgTimer;

        /// <summary>
        /// This timer is used to measure the time elapsed since the last message has been sent to the other
        /// side of this connection.
        /// </summary>
        private Stopwatch lastOutgoingMsgTimer;

        /// <summary>
        /// This timer is used to measure the time elapsed since the disconnect indicator message has been
        /// sent to the other side of this connection.
        /// </summary>
        private Stopwatch disconnectAckMsgTimer;

        /// <summary>
        /// This timer is used to measure the time elapsed since the connect request has been sent to the other
        /// side of this connection.
        /// </summary>
        private Stopwatch connectAckMsgTimer;
    }
}
