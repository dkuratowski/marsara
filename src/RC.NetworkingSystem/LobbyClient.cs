using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Net;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Internal class that acts as a lobby client.
    /// </summary>
    public abstract class LobbyClient : ILobbyClient
    {
        /// <summary>
        /// This event is raised when this LobbyClient has been disposed.
        /// </summary>
        public event Network.DisposedHandler Disposed;

        /// <summary>
        /// Constructs a LobbyClient object.
        /// </summary>
        public LobbyClient(LobbyInfo info, ILobbyListener listener)
        {
            this.id = info.ID;
            this.listener = listener;
            this.memberCount = -1;
            this.clientID = -1;
            this.outgoingPackages = new List<RCPackage>();
            this.outgoingPackageTargets = new List<int[]>();

            IPAddress ipAddress;
            if (!IPAddress.TryParse(info.IPAddress, out ipAddress))
            {
                throw new NetworkingSystemException("Unable to parse server IP address: " + info.IPAddress);
            }
            this.serverEndpoint = new IPEndPoint(ipAddress, info.PortNumber);
            this.connection = CreateConnection_i(this.serverEndpoint);

            this.stopConnectionManagerThread = new ManualResetEvent(false);
            this.connectionManagerThread = new RCThread(this.ConnectionManagerProc, "Networking");
            this.connectionManagerThread.Start();
        }

        #region ILobbyClient members

        /// <see cref="ILobby.SendPackage"/>
        public bool SendPackage(RCPackage package)
        {
            if (package == null) { throw new ArgumentNullException("package"); }

            if (package.IsCommitted && package.PackageType == RCPackageType.NETWORK_CUSTOM_PACKAGE &&
                package.PackageFormat.ID != Network.FORMAT_DEDICATED_MESSAGE &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_ACK &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_INDICATOR &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO_VANISHED &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_LINE_STATE_REPORT)
            {
                lock (this.outgoingPackages)
                {
                    this.outgoingPackages.Add(package);
                    this.outgoingPackageTargets.Add(null);
                    return true;
                }
            }
            return false;
        }

        /// <see cref="ILobby.SendPackage"/>
        public bool SendPackage(RCPackage package, int[] targets)
        {
            if (package == null) { throw new ArgumentNullException("package"); }
            if (targets == null || targets.Length == 0) { throw new ArgumentNullException("targets"); }

            if (package.IsCommitted && package.PackageType == RCPackageType.NETWORK_CUSTOM_PACKAGE &&
                package.PackageFormat.ID != Network.FORMAT_DEDICATED_MESSAGE &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_ACK &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_INDICATOR &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO_VANISHED &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_LINE_STATE_REPORT)
            {
                lock (this.outgoingPackages)
                {
                    this.outgoingPackages.Add(package);
                    this.outgoingPackageTargets.Add(targets);
                    return true;
                }
            }
            return false;
        }

        /// <see cref="ILobbyClient.SendControlPackage"/>
        public bool SendControlPackage(RCPackage package)
        {
            if (package == null) { throw new ArgumentNullException("package"); }

            if (package.IsCommitted && package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                package.PackageFormat.ID != Network.FORMAT_DEDICATED_MESSAGE &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_ACK &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_INDICATOR &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO_VANISHED &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_LINE_STATE_REPORT)
            {
                lock (this.outgoingPackages)
                {
                    this.outgoingPackages.Add(package);
                    this.outgoingPackageTargets.Add(null);
                    return true;
                }
            }
            return false;
        }

        /// <see cref="ILobbyClient.Disconnect"/>
        public void Disconnect()
        {
            this.stopConnectionManagerThread.Set();
            this.connectionManagerThread.Join();
            this.stopConnectionManagerThread.Close();
            this.stopConnectionManagerThread = null;
            this.connectionManagerThread = null;
            if (this.Disposed != null) { this.Disposed(this); }
        }

        #endregion

        /// <summary>
        /// Internal function to create a lobby connection for this client.
        /// </summary>
        protected abstract LobbyConnection CreateConnection_i(IPEndPoint serverAddr);

        /// <summary>
        /// This is the starting function of the connection manager thread.
        /// </summary>
        private void ConnectionManagerProc()
        {
            Stopwatch stopWatch = new Stopwatch();

            if (!this.connection.BeginConnectToTheServer(this.serverEndpoint))
            {
                /// Connection failed at the beginning --> Stop the connection manager thread.
                this.connection.Shutdown();
                if (this.Disposed != null) { this.Disposed(this); }
                this.listener.LobbyLost();
                return;
            }

            do
            {
                stopWatch.Restart();

                if (this.connection.ConnectionState == LobbyConnectionState.Connecting)
                {
                    /// We must wait for the first line state report.
                    if (!ContinueConnectToTheServer())
                    {
                        /// There was an error, so finish the connection manager thread.
                        return;
                    }
                }
                else if (this.connection.ConnectionState == LobbyConnectionState.Connected)
                {
                    /// Normal message processing.
                    if (!ProcessIncomingMessages())
                    {
                        /// Connection manager thread has to stop.
                        return;
                    }

                    /// Sending outgoing messages
                    if (!SendOutgoingMessages())
                    {
                        /// Connection manager thread has to stop.
                        return;
                    }

                    if (!this.connection.SendPingIfNecessary())
                    {
                        /// Connection manager thread has to stop.
                        this.connection.Shutdown();
                        if (this.Disposed != null) { this.Disposed(this); }
                        this.listener.LobbyLost();
                        return;
                    }
                }
                else
                {
                    /// Unexpected state --> FATAL ERROR
                    throw new NetworkingSystemException("Unexpected connection state!");
                }

            } while (!this.stopConnectionManagerThread.WaitOne(
                Math.Max(NetworkingSystemConstants.CLIENT_CONNECTION_MANAGER_CYCLE_TIME - (int)stopWatch.ElapsedMilliseconds, 0)));

            /// Send the remaining outgoing messages to the server.
            SendOutgoingMessages();

            /// LobbyClient shutdown is initiated.
            while (true)
            {
                if (this.connection.ConnectionState == LobbyConnectionState.Connected)
                {
                    /// Initiate disconnect from the server
                    this.connection.BeginDisconnect();
                }
                else if (this.connection.ConnectionState == LobbyConnectionState.Disconnecting)
                {
                    if (this.connection.ContinueDisconnect())
                    {
                        /// Disconnect finished --> Stop the connection manager thread.
                        return;
                    }
                }

                /// Wait for a while.
                RCThread.Sleep(NetworkingSystemConstants.CLIENT_CONNECTION_MANAGER_CYCLE_TIME);
            }
        }

        /// <summary>
        /// Continues the connection procedure to the server.
        /// </summary>
        /// <returns>
        /// In case of error this function automatically shuts down the connection, notifies the listener and
        /// returns false. Otherwise it returns true. If the line state report arrives from the server, this
        /// function automatically notifies the listener.
        /// </returns>
        private bool ContinueConnectToTheServer()
        {
            RCPackage lineStateReport = null;
            if (this.connection.ContinueConnectToTheServer(out lineStateReport))
            {
                if (lineStateReport != null)
                {
                    short clientID = lineStateReport.ReadShort(0);
                    byte[] lineStateBytes = lineStateReport.ReadByteArray(1);
                    LobbyLineState[] lineStates = new LobbyLineState[lineStateBytes.Length];
                    bool lineStatesOK = true;
                    for (int i = 0; i < lineStateBytes.Length; i++)
                    {
                        if (lineStateBytes[i] == (byte)LobbyLineState.Closed ||
                            lineStateBytes[i] == (byte)LobbyLineState.Engaged ||
                            lineStateBytes[i] == (byte)LobbyLineState.Opened)
                        {
                            lineStates[i] = (LobbyLineState)lineStateBytes[i];
                        }
                        else
                        {
                            lineStatesOK = false;
                        }
                    }
                    lineStatesOK = lineStatesOK && clientID > 0 && clientID < lineStateBytes.Length;
                    if (!lineStatesOK)
                    {
                        /// Line state report error --> shutdown.
                        this.connection.Shutdown();
                        if (this.Disposed != null) { this.Disposed(this); }
                        this.listener.LobbyLost();
                        return false;
                    }
                    else
                    {
                        /// Connection procedure finished --> send a line state report to the listener.
                        this.clientID = clientID;
                        this.memberCount = lineStates.Length;
                        this.listener.LineStateReport(clientID, lineStates);
                        return true;
                    }
                }
                /// No error but connection procedure not finished.
                return true;
            }
            else
            {
                /// Connection rejected by the server or an error occured --> shutdown.
                this.connection.Shutdown();
                if (this.Disposed != null) { this.Disposed(this); }
                this.listener.LobbyLost();
                return false;
            }
        }

        /// <summary>
        /// This function reads and processes every incoming message arriving from the server.
        /// </summary>
        /// <returns>False, if the connection manager thread has to stop, true otherwise.</returns>
        private bool ProcessIncomingMessages()
        {
            List<RCPackage> incomingPackages = new List<RCPackage>();
            if (this.connection.ReceiveIncomingPackages(ref incomingPackages))
            {
                foreach (RCPackage package in incomingPackages)
                {
                    if (package.PackageType == RCPackageType.NETWORK_CUSTOM_PACKAGE &&
                        package.Sender >= 0 && package.Sender < this.memberCount && package.Sender != this.clientID)
                    {
                        /// Custom message from a member --> notify the listener.
                        this.listener.PackageArrived(package, package.Sender);
                    }
                    else if (package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                             package.PackageFormat.ID == Network.FORMAT_LOBBY_LINE_STATE_REPORT)
                    {
                        /// Line state report from the server.
                        short clientID = package.ReadShort(0);
                        byte[] lineStateBytes = package.ReadByteArray(1);
                        LobbyLineState[] lineStates = new LobbyLineState[lineStateBytes.Length];
                        bool lineStatesOK = true;
                        for (int i = 0; i < lineStateBytes.Length; i++)
                        {
                            if (lineStateBytes[i] == (byte)LobbyLineState.Closed ||
                                lineStateBytes[i] == (byte)LobbyLineState.Engaged ||
                                lineStateBytes[i] == (byte)LobbyLineState.Opened)
                            {
                                lineStates[i] = (LobbyLineState)lineStateBytes[i];
                            }
                            else
                            {
                                lineStatesOK = false;
                            }
                        }
                        lineStatesOK = lineStatesOK && (clientID == this.clientID) && (lineStateBytes.Length == this.memberCount);
                        if (!lineStatesOK)
                        {
                            /// Line state report error --> shutdown.
                            this.connection.Shutdown();
                            if (this.Disposed != null) { this.Disposed(this); }
                            this.listener.LobbyLost();
                            return false;
                        }
                        else
                        {
                            /// Line state report arrived --> notify the listener
                            this.listener.LineStateReport(clientID, lineStates);
                            ///return true;
                        }
                    }
                    else if (package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                             package.PackageFormat.ID == Network.FORMAT_DISCONNECT_INDICATOR)
                    {
                        /// Disconnection indicator from the server.
                        RCPackage disconnectAck = RCPackage.CreateNetworkControlPackage(Network.FORMAT_DISCONNECT_ACK);
                        disconnectAck.WriteString(0, string.Empty);
                        disconnectAck.WriteByteArray(1, new byte[0] { });
                        this.connection.SendPackage(disconnectAck);
                        this.connection.Shutdown();
                        if (this.Disposed != null) { this.Disposed(this); }
                        this.listener.LobbyLost();
                        return false;
                    }
                    else if (package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                             package.PackageFormat.ID != Network.FORMAT_DEDICATED_MESSAGE &&
                             package.PackageFormat.ID != Network.FORMAT_DISCONNECT_ACK &&
                             package.PackageFormat.ID != Network.FORMAT_DISCONNECT_INDICATOR &&
                             package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO &&
                             package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO_VANISHED &&
                             package.PackageFormat.ID != Network.FORMAT_LOBBY_LINE_STATE_REPORT)
                    {
                        /// Custom internal message from the server --> notify the listener
                        TraceManager.WriteAllTrace(string.Format("Incoming package: {0}", package.ToString()), NetworkingSystemTraceFilters.INFO);
                        this.listener.ControlPackageArrived(package);
                    }
                    else
                    {
                        /// Unexpected package format and type --> immediate shutdown
                        this.connection.Shutdown();
                        if (this.Disposed != null) { this.Disposed(this); }
                        this.listener.LobbyLost();
                        return false;
                    }
                } /// end-foreach (RCPackage package in incomingPackages)

                /// Incoming packages has been processed.
                return true;
            }
            else
            {
                /// Receive error --> immediate shutdown
                this.connection.Shutdown();
                if (this.Disposed != null) { this.Disposed(this); }
                this.listener.LobbyLost();
                return false;
            }
        }

        /// <summary>
        /// The function sends every outgoing messages.
        /// </summary>
        /// <returns>False if the connection manager thread has to stop, true otherwise.</returns>
        private bool SendOutgoingMessages()
        {
            bool errorOccured = false;

            lock (this.outgoingPackages)
            {
                if (this.outgoingPackages.Count == this.outgoingPackageTargets.Count)
                {
                    if (this.connection.ConnectionState == LobbyConnectionState.Connected)
                    {
                        for (int i = 0; i < this.outgoingPackages.Count; i++)
                        {
                            int[] targets = this.outgoingPackageTargets[i];
                            if (targets != null)
                            {
                                /// This is a dedicated message --> Send it as an embedded message.
                                /// First collect the targets.
                                List<byte> targetBytesList = new List<byte>();
                                for (int j = 0; j < targets.Length; j++)
                                {
                                    if (targets[j] != this.clientID && targets[j] >= 0 && targets[j] < this.memberCount)
                                    {
                                        targetBytesList.Add((byte)targets[j]);
                                    }
                                }
                                /// Then create the dedicated message and send it to the server.
                                byte[] targetBytes = targetBytesList.ToArray();
                                if (targetBytes != null && targetBytes.Length > 0)
                                {
                                    byte[] packageBytes = new byte[this.outgoingPackages[i].PackageLength];
                                    this.outgoingPackages[i].WritePackageToBuffer(packageBytes, 0);
                                    RCPackage dedicatedPackage = RCPackage.CreateNetworkControlPackage(Network.FORMAT_DEDICATED_MESSAGE);
                                    dedicatedPackage.WriteByteArray(0, targetBytes);    /// The targets of the message.
                                    dedicatedPackage.WriteByteArray(1, packageBytes);   /// The message itself.
                                    if (!this.connection.SendPackage(dedicatedPackage))
                                    {
                                        /// Error when try to send --> immediate shutdown.
                                        this.connection.Shutdown();
                                        //if (this.Disposed != null) { this.Disposed(this); }
                                        //this.listener.LobbyLost();
                                        errorOccured = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                /// This is a simple message.
                                if (!this.connection.SendPackage(this.outgoingPackages[i]))
                                {
                                    /// Error when try to send --> immediate shutdown.
                                    this.connection.Shutdown();
                                    //if (this.Disposed != null) { this.Disposed(this); }
                                    //this.listener.LobbyLost();
                                    errorOccured = true;
                                    break;
                                }
                            }
                        }
                    }

                    /// Clear the FIFO
                    this.outgoingPackages.Clear();
                    this.outgoingPackageTargets.Clear();

                    /// Outgoing messages sent successfully.
                    ///return true;
                }
                else
                {
                    throw new NetworkingSystemException("Inconsistence in the outgoing package FIFO!");
                }
            }

            if (errorOccured)
            {
                /// An error happened during the send.
                if (this.Disposed != null) { this.Disposed(this); }
                this.listener.LobbyLost();
                return false;
            }
            else
            {
                /// Outgoing messages sent successfully.
                return true;
            }
        }

        /// <summary>
        /// List of the outgoing packages.
        /// </summary>
        private List<RCPackage> outgoingPackages;

        /// <summary>
        /// List of the targets of the outgoing packages. Null reference is used if we want to send a package to
        /// the whole lobby.
        /// </summary>
        private List<int[]> outgoingPackageTargets;

        /// <summary>
        /// ID of this lobby.
        /// </summary>
        private Guid id;

        /// <summary>
        /// The total number of members in the lobby.
        /// </summary>
        private int memberCount;

        /// <summary>
        /// The ID of this member in the lobby.
        /// </summary>
        private int clientID;

        /// <summary>
        /// The IP endpoint of the server.
        /// </summary>
        private IPEndPoint serverEndpoint;

        /// <summary>
        /// The listener object that will be notified about lobby events.
        /// </summary>
        private ILobbyListener listener;

        /// <summary>
        /// The connection of this peer to the server in this lobby.
        /// </summary>
        private LobbyConnection connection;

        /// <summary>
        /// The thread that receives or sends the messages from or to the active connection.
        /// </summary>
        private RCThread connectionManagerThread;

        /// <summary>
        /// This event is signaled if the connection manager thread should stop.
        /// </summary>
        private ManualResetEvent stopConnectionManagerThread;
    }
}
