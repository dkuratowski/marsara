using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using System.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Internal class that acts as a lobby server.
    /// </summary>
    public abstract class LobbyServer : ILobbyServer
    {
        /// <summary>
        /// This event is raised when this LobbyServer has been disposed.
        /// </summary>
        public event Network.DisposedHandler Disposed;

        /// <summary>
        /// Constructs a LobbyServer object.
        /// </summary>
        public LobbyServer(int maxClients, ILobbyListener listener)
        {
            this.id = Guid.NewGuid();
            this.listener = listener;
            this.announcer = null;
            this.tasks = new List<NetworkingTaskType>();
            this.manipulators = new Fifo<LineManipulator>(NetworkingSystemConstants.SERVER_TASK_FIFO_CAPACITY);
            this.manipulatorParams = new Fifo<int>(NetworkingSystemConstants.SERVER_TASK_FIFO_CAPACITY);
            this.outgoingPackages = new Fifo<RCPackage>(NetworkingSystemConstants.SERVER_TASK_FIFO_CAPACITY);
            this.outgoingPackageTargets = new Fifo<int[]>(NetworkingSystemConstants.SERVER_TASK_FIFO_CAPACITY);

            this.connections = new LobbyConnection[maxClients - 1];
            for (int i = 0; i < this.connections.Length; i++)
            {
                this.connections[i] = CreateConnection_i();
            }

            this.stopConnectionManagerThread = new ManualResetEvent(false);
            this.connectionManagerThread = new RCThread(this.ConnectionManagerProc, "Networking");
            this.connectionManagerThread.Start();
        }

        #region ILobbyServer methods

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
                lock (this.tasks)
                {
                    package.Sender = 0; /// Indicate that this package is sent by the server
                    this.tasks.Add(NetworkingTaskType.OUTGOING_MESSAGE);
                    this.outgoingPackages.Push(package);
                    this.outgoingPackageTargets.Push(null);
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
                lock (this.tasks)
                {
                    package.Sender = 0; /// Indicate that this package is sent by the server
                    this.tasks.Add(NetworkingTaskType.OUTGOING_MESSAGE);
                    this.outgoingPackages.Push(package);
                    this.outgoingPackageTargets.Push(targets);
                    return true;
                }
            }
            return false;
        }

        /// <see cref="ILobbyServer.SendControlPackage"/>
        public bool SendControlPackage(RCPackage package, int target)
        {
            if (package == null) { throw new ArgumentNullException("package"); }
            if (target == 0) { throw new ArgumentException("Internal message can only be sent to a client!", "target"); }

            if (package.IsCommitted && package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                package.PackageFormat.ID != Network.FORMAT_DEDICATED_MESSAGE &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_ACK &&
                package.PackageFormat.ID != Network.FORMAT_DISCONNECT_INDICATOR &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO_VANISHED &&
                package.PackageFormat.ID != Network.FORMAT_LOBBY_LINE_STATE_REPORT)
            {
                lock (this.tasks)
                {
                    this.tasks.Add(NetworkingTaskType.OUTGOING_MESSAGE);
                    this.outgoingPackages.Push(package);
                    this.outgoingPackageTargets.Push(new int[1] { target });
                    return true;
                }
            }
            return false;
        }

        /// <see cref="ILobbyServer.CloseLine"/>
        public void CloseLine(int line)
        {
            lock (this.tasks)
            {
                this.tasks.Add(NetworkingTaskType.LINE_MANIPULATION);
                this.manipulators.Push(this.CloseLine_i);
                this.manipulatorParams.Push(line);
            }
        }

        /// <see cref="ILobbyServer.OpenLine"/>
        public void OpenLine(int line)
        {
            lock (this.tasks)
            {
                this.tasks.Add(NetworkingTaskType.LINE_MANIPULATION);
                this.manipulators.Push(this.OpenLine_i);
                this.manipulatorParams.Push(line);
            }
        }

        /// <see cref="ILobbyServer.StartAnnouncing"/>
        public void StartAnnouncing()
        {
            StartAnnouncing(null);
        }

        /// <see cref="ILobbyServer.StartAnnouncing"/>
        public void StartAnnouncing(ILobbyCustomDataProvider customDataProvider)
        {
            if (this.announcer == null)
            {
                this.announcer = CreateAnnouncer_i(customDataProvider);
                bool success = this.announcer.Start();
                if (!success)
                {
                    this.announcer.Stop();
                    this.announcer = null;
                }
            }
        }

        /// <see cref="ILobbyServer.StopAnnouncing"/>
        public void StopAnnouncing()
        {
            if (this.announcer != null)
            {
                this.announcer.Stop();
                this.announcer = null;
            }
        }

        /// <see cref="ILobbyServer.Shutdown"/>
        public void Shutdown()
        {
            StopAnnouncing();
            this.stopConnectionManagerThread.Set();
            this.connectionManagerThread.Join();
            this.stopConnectionManagerThread.Close();
            this.stopConnectionManagerThread = null;
            this.connectionManagerThread = null;
            if (this.Disposed != null) { this.Disposed(this); }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Gets the current state of the lines of this server.
        /// </summary>
        public LobbyLineState[] LineStates
        {
            get
            {
                LobbyLineState[] lineStates = new LobbyLineState[this.connections.Length + 1];
                lineStates[0] = LobbyLineState.Engaged;
                for (int i = 0; i < this.connections.Length; i++)
                {
                    lineStates[i + 1] = this.connections[i].LineState;
                }
                return lineStates;
            }
        }

        /// <summary>
        /// Gets the ID of this lobby server.
        /// </summary>
        public Guid Id { get { return this.id; } }

        /// <summary>
        /// Get the port number where this lobby is waiting for incoming connections.
        /// </summary>
        public abstract int PortNum { get; }

        /// <summary>
        /// Internal function to create a lobby announcer object.
        /// </summary>
        protected abstract LobbyAnnouncer CreateAnnouncer_i(ILobbyCustomDataProvider customDataProvider);

        /// <summary>
        /// Internal function to create a lobby connection.
        /// </summary>
        protected abstract LobbyConnection CreateConnection_i();

        #endregion

        #region Private members

        /// <summary>
        /// The starting function of the connection manager thread.
        /// </summary>
        private void ConnectionManagerProc()
        {
            /// Create the necessary objects.
            Stopwatch cycleStopWatch = new Stopwatch();
            Stopwatch connAcceptStopWatch = new Stopwatch();
            connAcceptStopWatch.Restart();

            /// This call is only for callback the listener.
            SendLineStateReports();

            do
            {
                cycleStopWatch.Restart();

                /// First we execute the tasks
                ExecuteTasks();
                /// Then we accept the next incoming connection if this is the time to do it.
                if (connAcceptStopWatch.ElapsedMilliseconds > NetworkingSystemConstants.CONNECTION_ACCEPT_FREQUENCY)
                {
                    List<LobbyLineState[]> reportsToSend = new List<LobbyLineState[]>();
                    lock (this.tasks)
                    {
                        /// Execute every tasks just before we accept the next connection. It is needed to
                        /// lock the tasks FIFO, because we want to be sure that the FIFO is empty when
                        /// a new connection is accepted.
                        ExecuteTasks(ref reportsToSend);

                        /// We can accept the next connection after we have processed all outgoing messages and the
                        /// FIFO is empty.
                        AcceptNextConnection(ref reportsToSend);
                    }
                    foreach (LobbyLineState[] report in reportsToSend)
                    {
                        SendLineStateReports(report);
                    }
                    connAcceptStopWatch.Restart();
                }
                /// Then we read and process all incoming messages.
                ProcessIncomingMessages();
                /// And finally we send ping messages if necessary.
                for (int i = 0; i < this.connections.Length; i++)
                {
                    if (this.connections[i].ConnectionState == LobbyConnectionState.Connected)
                    {
                        if (!this.connections[i].SendPingIfNecessary())
                        {
                            /// Error --> immediate shutdown.
                            this.connections[i].Shutdown();
                            this.connections[i].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                            SendLineStateReports();
                        }
                    }
                }

            } while (!this.stopConnectionManagerThread.WaitOne(
                Math.Max(NetworkingSystemConstants.SERVER_CONNECTION_MANAGER_CYCLE_TIME - (int)cycleStopWatch.ElapsedMilliseconds, 0)));

            ExecuteTasks();

            /// LobbyServer shutdown is initiated.
            while (true)
            {
                /// Initiate disconnect to every clients.
                for (int i = 0; i < this.connections.Length; i++)
                {
                    if (this.connections[i].ConnectionState == LobbyConnectionState.Connected)
                    {
                        this.connections[i].BeginDisconnect();
                    }
                    else if (this.connections[i].ConnectionState == LobbyConnectionState.Disconnecting)
                    {
                        this.connections[i].ContinueDisconnect();
                    }
                }
                /// Wait for a while.
                RCThread.Sleep(NetworkingSystemConstants.SERVER_CONNECTION_MANAGER_CYCLE_TIME);

                /// Check if everybody has been disconnected or not.
                bool connectedClientExists = false;
                for (int i = 0; i < this.connections.Length; i++)
                {
                    if (this.connections[i].ConnectionState != LobbyConnectionState.Disconnected)
                    {
                        connectedClientExists = true;
                        break;
                    }
                }

                /// If everybody has been disconnected we can finish the thread.
                if (!connectedClientExists)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Enumerates the possible types of tasks that the client module can send to the RC.NetworkingSystem.
        /// </summary>
        private enum NetworkingTaskType
        {
            LINE_MANIPULATION = 0,      /// Closing or opening a lobby line
            OUTGOING_MESSAGE = 1        /// Sending a message to the lobby
        }

        /// <summary>
        /// Delegate functions that can manipulate lines of this server.
        /// </summary>
        /// <param name="param">The index of the line that the function will manipulate.</param>
        /// <return>True if a line state report is necessary, false otherwise.</return>
        private delegate bool LineManipulator(int param);

        /// <summary>
        /// Internal function for performing every tasks created by the client module.
        /// </summary>
        private void ExecuteTasks()
        {
            List<LobbyLineState[]> reportsToSend = new List<LobbyLineState[]>();
            ExecuteTasks(ref reportsToSend);
            foreach (LobbyLineState[] report in reportsToSend)
            {
                SendLineStateReports(report);
            }
        }

        /// <summary>
        /// Internal function for sending every outgoing messages and collecting all line state reports that we
        /// must send after returning this function.
        /// </summary>
        /// <param name="reportsToSend">The collected reports will be added to this list.</param>
        private void ExecuteTasks(ref List<LobbyLineState[]> reportsToSend)
        {
            lock (this.tasks)
            {
                for (int i = 0; i < this.tasks.Count; i++)
                {
                    NetworkingTaskType type = this.tasks[i];
                    if (type == NetworkingTaskType.LINE_MANIPULATION)
                    {
                        ExecuteNextLineManipulator(ref reportsToSend);
                    }
                    else if (type == NetworkingTaskType.OUTGOING_MESSAGE)
                    {
                        SendNextOutgoingMessage(ref reportsToSend);
                    }
                    else
                    {
                        throw new NetworkingSystemException("Unexpected task type!");
                    }
                }
                this.tasks.Clear();
            }
        }

        /// <summary>
        /// Internal function to perform the next line manipulation task.
        /// </summary>
        /// <param name="reportsToSend">The collected reports will be added to this list.</param>
        private void ExecuteNextLineManipulator(ref List<LobbyLineState[]> reportsToSend)
        {
            LineManipulator manipulator = this.manipulators.Get();
            int param = this.manipulatorParams.Get();
            if (manipulator(param))
            {
                /// If a manipulator has changed the line states then we will have to send a report at
                /// the end of this function.
                reportsToSend.Add(this.LineStates);
            }
        }

        /// <summary>
        /// Internal function for sending the next outgoing message.
        /// </summary>
        /// <param name="reportsToSend">The collected reports will be added to this list.</param>
        private void SendNextOutgoingMessage(ref List<LobbyLineState[]> reportsToSend)
        {
            int[] targets = this.outgoingPackageTargets.Get();
            RCPackage package = this.outgoingPackages.Get();

            if (targets != null)
            {
                /// This is a dedicated package --> Send it only to the targets.
                for (int j = 0; j < targets.Length; j++)
                {
                    if (targets[j] != 0 && targets[j] - 1 >= 0 && targets[j] - 1 < this.connections.Length &&
                        this.connections[targets[j] - 1].ConnectionState == LobbyConnectionState.Connected)
                    {
                        if (!this.connections[targets[j] - 1].SendPackage(package))
                        {
                            this.connections[targets[j] - 1].Shutdown();
                            this.connections[targets[j] - 1].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                            //SendLineStateReports(); /// ---> This call can cause deadlock
                            reportsToSend.Add(this.LineStates);
                        }
                    }
                }
            }
            else
            {
                /// This is a simple package --> Send it to every client.
                for (int j = 0; j < this.connections.Length; j++)
                {
                    if (this.connections[j].ConnectionState == LobbyConnectionState.Connected &&
                        !this.connections[j].SendPackage(package))
                    {
                        this.connections[j].Shutdown();
                        this.connections[j].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                        //SendLineStateReports(); /// ---> This call can cause deadlock
                        reportsToSend.Add(this.LineStates);
                    }
                }
            }
        }

        /// <summary>
        /// Process all incoming messages arrived from all connections.
        /// </summary>
        private void ProcessIncomingMessages()
        {
            for (int i = 0; i < this.connections.Length; i++)
            {
                if (this.connections[i].ConnectionState == LobbyConnectionState.Connected)
                {
                    /// The connection is in connected state --> it's incoming messages will be processed by the server
                    List<RCPackage> incomingPackages = new List<RCPackage>();
                    if (this.connections[i].ReceiveIncomingPackages(ref incomingPackages))
                    {
                        /// Process the incoming messages
                        foreach (RCPackage package in incomingPackages)
                        {
                            if (package.PackageType == RCPackageType.NETWORK_CUSTOM_PACKAGE)
                            {
                                /// This is a custom message, forward it to every other clients
                                package.Sender = i + 1;
                                for (int j = 0; j < this.connections.Length; j++)
                                {
                                    if (i != j && this.connections[j].ConnectionState == LobbyConnectionState.Connected)
                                    {
                                        if (!this.connections[j].SendPackage(package))
                                        {
                                            /// Unable to forward the message to a client --> Shutdown the connection
                                            this.connections[j].Shutdown();
                                            this.connections[j].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                                            SendLineStateReports();
                                        }
                                    }
                                }
                                /// Notify the listener object about the arrived package
                                this.listener.PackageArrived(package, i + 1);
                            }
                            else if (package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                                     package.PackageFormat.ID == Network.FORMAT_DEDICATED_MESSAGE)
                            {
                                /// This is a dedicated message, forward only to the targets
                                byte[] targets = package.ReadByteArray(0);  /// List of the targets
                                byte[] theMessageBytes = package.ReadByteArray(1);  /// The embedded message
                                int parsedBytes = 0;
                                RCPackage theMessage = RCPackage.Parse(theMessageBytes, 0, theMessageBytes.Length, out parsedBytes);
                                if (theMessage != null && theMessage.IsCommitted && theMessage.PackageType == RCPackageType.NETWORK_CUSTOM_PACKAGE)
                                {
                                    /// The embedded message is OK --> forward it to the dedicated targets
                                    theMessage.Sender = i + 1;
                                    for (int j = 0; j < targets.Length; j++)
                                    {
                                        int target = targets[j];
                                        if (target == 0)
                                        {
                                            /// This server is the target
                                            this.listener.PackageArrived(theMessage, i + 1);
                                        }
                                        else if (target - 1 >= 0 && target - 1 < this.connections.Length && target - 1 != i)
                                        {
                                            /// Another client is the target --> forward the message to it
                                            LobbyConnection targetConn = this.connections[target - 1];
                                            if (targetConn.ConnectionState == LobbyConnectionState.Connected)
                                            {
                                                if (!targetConn.SendPackage(theMessage))
                                                {
                                                    /// Unable to forward the message to a target --> Shutdown the connection
                                                    targetConn.Shutdown();
                                                    targetConn.LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                                                    SendLineStateReports();
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    /// The embedded message has unexpected format --> Shutdown the connection
                                    this.connections[i].Shutdown();
                                    this.connections[i].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                                    SendLineStateReports();
                                    break;  /// Stop processing the messages of the closed connection
                                }
                            }
                            else if (this.connections[i].ConnectionState == LobbyConnectionState.Connected &&
                                     package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                                     package.PackageFormat.ID == Network.FORMAT_DISCONNECT_INDICATOR)
                            {
                                /// The client at the other side of the connection wants to disconnect.
                                /// Acknowledge this request and shutdown the connection.
                                RCPackage disconnAck = RCPackage.CreateNetworkControlPackage(Network.FORMAT_DISCONNECT_ACK);
                                disconnAck.WriteString(0, string.Empty);
                                disconnAck.WriteByteArray(1, new byte[0] { });
                                this.connections[i].SendPackage(disconnAck);
                                this.connections[i].Shutdown();
                                this.connections[i].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                                SendLineStateReports();
                                break;  /// Stop processing the messages of the closed connection
                            }
                            else if (package.PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                                     package.PackageFormat.ID != Network.FORMAT_DEDICATED_MESSAGE &&
                                     package.PackageFormat.ID != Network.FORMAT_DISCONNECT_ACK &&
                                     package.PackageFormat.ID != Network.FORMAT_DISCONNECT_INDICATOR &&
                                     package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO &&
                                     package.PackageFormat.ID != Network.FORMAT_LOBBY_INFO_VANISHED &&
                                     package.PackageFormat.ID != Network.FORMAT_LOBBY_LINE_STATE_REPORT)
                            {
                                /// Custom internal message from a client --> notify the listener
                                this.listener.ControlPackageArrived(package, i + 1);
                            }
                            else
                            {
                                /// Unexpected message from the current connection
                                this.connections[i].Shutdown();
                                this.connections[i].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                                SendLineStateReports();
                                break;  /// Stop processing the messages of the closed connection
                            }

                        } /// end-foreach (RCPackage package in incomingPackages)
                    }
                    else
                    {
                        /// In case of receive error, we shutdown the connection.
                        this.connections[i].Shutdown();
                        this.connections[i].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                        SendLineStateReports();
                    }
                }
                else if (this.connections[i].ConnectionState == LobbyConnectionState.Disconnecting)
                {
                    /// The connection is about to disconnect --> incoming messages will be handled by the connection itself
                    if (this.connections[i].ContinueDisconnect())
                    {
                        /// This connection remains closed because disconnection is initiated by the server.
                        SendLineStateReports();
                    }
                }

            } /// end-for (int i = 0; i < this.connections.Length; i++)
        }


        /// <summary>
        /// Searches the first Opened connection, and tries to accept the next incoming connection request.
        /// </summary>
        /// <param name="reportsToSend">The necessary line state reports will be added to this list.</param>
        private void AcceptNextConnection(ref List<LobbyLineState[]> reportsToSend)
        {
            bool openedConnectionFound = false;
            for (int i = 0; i < this.connections.Length; i++)
            {
                if (this.connections[i].LineState == LobbyLineState.Opened)
                {
                    openedConnectionFound = true;
                    if (this.connections[i].TryAcceptNextConnection())
                    {
                        /// The state of a line has been changed so we must notify every client about this change
                        //SendLineStateReports(); /// ---> This call can cause deadlock
                        reportsToSend.Add(this.LineStates);
                    }
                    break;
                }
            }

            if (!openedConnectionFound)
            {
                /// If there is no opened communication line, we accept the connection, send a disconnect_indicator
                /// and close the connection immediately.
                LobbyConnection tmpConnection = CreateConnection_i();
                if (null != tmpConnection && tmpConnection.TryAcceptNextConnection())
                {
                    if (tmpConnection.BeginDisconnect()) { tmpConnection.Shutdown(); }
                }
            }
        }

        /// <summary>
        /// Sends reports about the current line states to all connected clients and to the listener of this server.
        /// </summary>
        private void SendLineStateReports()
        {
            LobbyLineState[] lineStates = this.LineStates;
            SendLineStateReports(lineStates);
        }

        /// <summary>
        /// Sends reports about the given line states to all connected clients and to the listener of this server.
        /// </summary>
        private void SendLineStateReports(LobbyLineState[] lineStates)
        {
            for (int i = 0; i < this.connections.Length; i++)
            {
                if (this.connections[i].ConnectionState == LobbyConnectionState.Connected)
                {
                    RCPackage lineStateReport = RCPackage.CreateNetworkControlPackage(Network.FORMAT_LOBBY_LINE_STATE_REPORT);
                    lineStateReport.WriteShort(0, (short)(i + 1));   /// The ID of the client that receives the report.
                    byte[] lineStatesBytes = new byte[lineStates.Length];
                    for (int j = 0; j < lineStatesBytes.Length; j++)
                    {
                        lineStatesBytes[j] = (byte)lineStates[j];
                    }
                    lineStateReport.WriteByteArray(1, lineStatesBytes);     /// The list of the line states on this server.

                    /// Send the package
                    if (!this.connections[i].SendPackage(lineStateReport))
                    {
                        /// In case of error, shutdown the connection and notify the other clients again.
                        this.connections[i].Shutdown();
                        this.connections[i].LineState = LobbyLineState.Opened; /// Keep it opened for other clients.
                        SendLineStateReports();
                    }
                }
            }
            this.listener.LineStateReport(0, lineStates);
        }

        /// <summary>
        /// Manipulator function for closing a line.
        /// </summary>
        /// <return>True if a line state report is necessary, false otherwise.</return>
        private bool CloseLine_i(int line)
        {
            bool lineStateReportNecessary = false;
            if (line - 1 >= 0 && line - 1 < this.connections.Length)
            {
                if (this.connections[line - 1].ConnectionState == LobbyConnectionState.Connected)
                {
                    /// If a client is connected then we begin a disconnect.
                    if (!this.connections[line - 1].BeginDisconnect())
                    {
                        lineStateReportNecessary = true;
                        //SendLineStateReports(); ---> This call can cause deadlock
                    }
                }
                else if (this.connections[line - 1].LineState == LobbyLineState.Opened)
                {
                    /// Else if no client is connected and the state of the line is opened then we simply close it.
                    this.connections[line - 1].LineState = LobbyLineState.Closed;
                    lineStateReportNecessary = true;
                    //SendLineStateReports(); ---> This call can cause deadlock
                }
            }
            return lineStateReportNecessary;
        }

        /// <summary>
        /// Manipulator function for opening a line.
        /// </summary>
        /// <return>True if a line state report is necessary, false otherwise.</return>
        private bool OpenLine_i(int line)
        {
            bool lineStateReportNecessary = false;
            if (line - 1 >= 0 && line - 1 < this.connections.Length)
            {
                /// Open only if it is closed.
                if (this.connections[line - 1].LineState == LobbyLineState.Closed)
                {
                    this.connections[line - 1].LineState = LobbyLineState.Opened;
                    lineStateReportNecessary = true;
                    //SendLineStateReports(); ---> This call can cause deadlock
                }
            }
            return lineStateReportNecessary;
        }

        /// <summary>
        /// List of the tasks of the networking thread.
        /// </summary>
        private List<NetworkingTaskType> tasks;

        /// <summary>
        /// List of the manipulators.
        /// </summary>
        private Fifo<LineManipulator> manipulators;

        /// <summary>
        /// List of the manipulator parameters.
        /// </summary>
        private Fifo<int> manipulatorParams;

        /// <summary>
        /// List of the outgoing packages.
        /// </summary>
        private Fifo<RCPackage> outgoingPackages;

        /// <summary>
        /// List of the targets of the outgoing packages. Null reference is used if we want to send a package to
        /// the whole lobby.
        /// </summary>
        private Fifo<int[]> outgoingPackageTargets;

        /// <summary>
        /// ID of this lobby.
        /// </summary>
        private Guid id;

        /// <summary>
        /// The listener object that will be notified about lobby events.
        /// </summary>
        private ILobbyListener listener;

        /// <summary>
        /// List of the connections to other peers in this lobby.
        /// </summary>
        private LobbyConnection[] connections;

        /// <summary>
        /// This object is used to announce this lobby on the nerwork.
        /// </summary>
        private LobbyAnnouncer announcer;

        /// <summary>
        /// The thread that receives or sends the messages from or to the active connections.
        /// </summary>
        private RCThread connectionManagerThread;

        /// <summary>
        /// This event is being signaled if the connection manager thread should stop.
        /// </summary>
        private ManualResetEvent stopConnectionManagerThread;

        #endregion
    }
}
