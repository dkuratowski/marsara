using System;
using System.Net.Sockets;
using System.Net;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Represents a LobbyConnection on the Local Area Network.
    /// </summary>
    class LANLobbyConnection : LobbyConnection
    {
        /// <summary>
        /// Constructs a server side LANLobbyConnection object.
        /// </summary>
        public LANLobbyConnection(LANLobbyServer lanServer)
            : base(LobbyConnectionType.SERVER_SIDE)
        {
            this.lanServer = lanServer;
            this.connection = null;
            this.stream = null;
            this.outBuffer = new byte[NetworkingSystemConstants.TCP_CONNECTION_BUFFER_DEFAULT_LENGTH];
            this.inBuffer = new byte[NetworkingSystemConstants.TCP_CONNECTION_BUFFER_DEFAULT_LENGTH];
            this.lastUncommittedPackage = null;
            this.unparsedOffset = 0;
            this.unparsedCount = 0;
        }

        /// <summary>
        /// Constructs a client side LANLobbyConnection object.
        /// </summary>
        public LANLobbyConnection(IPEndPoint serverAddr)
            : base(LobbyConnectionType.CLIENT_SIDE)
        {
            this.lanServer = null;
            this.connection = null;
            this.stream = null;
            this.outBuffer = new byte[NetworkingSystemConstants.TCP_CONNECTION_BUFFER_DEFAULT_LENGTH];
            this.inBuffer = new byte[NetworkingSystemConstants.TCP_CONNECTION_BUFFER_DEFAULT_LENGTH];
            this.lastUncommittedPackage = null;
            this.unparsedOffset = 0;
            this.unparsedCount = 0;
        }

        /// <see cref="LobbyConnection.TryAcceptNextConnection_i"/>
        protected override bool TryAcceptNextConnection_i()
        {
            try
            {
                TcpListener listener = this.lanServer.TCPListener;
                if (listener.Pending())
                {
                    TcpClient connection = listener.AcceptTcpClient();
                    TraceManager.WriteAllTrace(string.Format("Connection accepted from: {0} to {1}", connection.Client.RemoteEndPoint.ToString(), connection.Client.LocalEndPoint.ToString()), NetworkingSystemTraceFilters.INFO);
                    this.connection = connection;
                    this.connection.NoDelay = true;
                    this.stream = this.connection.GetStream();
                    return true;
                }
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
            }
            return false;
        }

        /// <see cref="LobbyConnection.TryConnectToTheServer_i"/>
        protected override bool TryConnectToTheServer_i(IPEndPoint server)
        {
            this.connection = new TcpClient();
            try
            {
                /// TODO: make it async?
                this.connection.Connect(server);
                TraceManager.WriteAllTrace(string.Format("Connected to server: local-{0} remote-{1}", this.connection.Client.LocalEndPoint.ToString(), this.connection.Client.RemoteEndPoint.ToString()), NetworkingSystemTraceFilters.INFO);
                this.stream = this.connection.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
                if (this.connection != null) { this.connection.Close(); }
                this.connection = null;
                this.stream = null;
                return false;
            }
        }

        /// <see cref="LobbyConnection.SendPackage_i"/>
        protected override bool SendPackage_i(RCPackage packageToSend)
        {
            TraceManager.WriteAllTrace(string.Format("SendPackage: {0} local-{1} remote-{2}", packageToSend.ToString(), this.connection.Client.LocalEndPoint.ToString(), this.connection.Client.RemoteEndPoint.ToString()), NetworkingSystemTraceFilters.INFO);
            return SendPackageToStream_i(packageToSend, this.stream);
        }

        /// <see cref="LobbyConnection.ReceivePackage_i"/>
        protected override bool ReceivePackage_i(out RCPackage receivedPackage)
        {
            receivedPackage = null;
            while (true)
            {
                /// First we parse the unparsed bytes.
                while (this.unparsedCount != 0)
                {
                    if (this.lastUncommittedPackage != null)
                    {
                        /// Continue parse the old package
                        int parsedBytes = 0;
                        bool success = this.lastUncommittedPackage.ContinueParse(this.inBuffer,
                                                                                 this.unparsedOffset,
                                                                                 this.unparsedCount,
                                                                                 out parsedBytes);
                        if (success)
                        {
                            this.unparsedCount -= parsedBytes;
                            this.unparsedOffset += parsedBytes;
                            if (this.lastUncommittedPackage.IsCommitted)
                            {
                                /// Package has been received
                                receivedPackage = this.lastUncommittedPackage;
                                this.lastUncommittedPackage = null;
                                return true;
                            }
                        }
                        else
                        {
                            /// Syntax error
                            return false;
                        }
                    }
                    else
                    {
                        /// Start a new package
                        int parsedBytes = 0;
                        this.lastUncommittedPackage =
                            RCPackage.Parse(this.inBuffer, this.unparsedOffset, this.unparsedCount, out parsedBytes);
                        if (this.lastUncommittedPackage != null)
                        {
                            this.unparsedCount -= parsedBytes;
                            this.unparsedOffset += parsedBytes;
                            if (this.lastUncommittedPackage.IsCommitted)
                            {
                                /// Package has been received
                                receivedPackage = this.lastUncommittedPackage;
                                this.lastUncommittedPackage = null;
                                return true;
                            }
                        }
                        else
                        {
                            /// Syntax error
                            return false;
                        }
                    }
                } /// end-while (this.unparsedCount != 0)

                try
                {
                    /// Now we have parsed all bytes remained from the previous turn. Read from the network.
                    if (this.stream.DataAvailable)
                    {
                        int readBytes = this.stream.Read(this.inBuffer, 0, this.inBuffer.Length);
                        this.unparsedOffset = 0;
                        this.unparsedCount = readBytes;
                    }
                    else
                    {
                        /// No more data available on the network stream.
                        return true;
                    }
                } /// end-try
                catch (Exception ex)
                {
                    TraceManager.WriteExceptionAllTrace(ex, false);
                    return false;
                }
            } /// end-while (true)
        }

        /// <see cref="LobbyConnection.Shutdown_i"/>
        protected override void Shutdown_i()
        {
            if (this.stream != null) { this.stream.Close(); }
            if (this.connection != null) { this.connection.Close(); }

            this.stream = null;
            this.connection = null;
            this.lastUncommittedPackage = null;
            this.unparsedOffset = 0;
            this.unparsedCount = 0;
        }

        /// <summary>
        /// Internal function to send a package to a network stream.
        /// </summary>
        private bool SendPackageToStream_i(RCPackage packageToSend, NetworkStream toWhichStream)
        {
            /// Extend the buffer if necessary.
            if (packageToSend.PackageLength > this.outBuffer.Length)
            {
                this.outBuffer = new byte[packageToSend.PackageLength];
            }
            int writtenBytes = packageToSend.WritePackageToBuffer(this.outBuffer, 0);
            try
            {
                toWhichStream.Write(this.outBuffer, 0, writtenBytes);
                return true;
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
            }
            return false;
        }

        /// <summary>
        /// Reference to the LANLobbyServer which this LANLobbyConnection belongs to.
        /// </summary>
        private LANLobbyServer lanServer;

        /// <summary>
        /// The underlying TCP connection.
        /// </summary>
        private TcpClient connection;

        /// <summary>
        /// The underlying network stream.
        /// </summary>
        private NetworkStream stream;

        /// <summary>
        /// The byte buffer used to write the network stream.
        /// </summary>
        private byte[] outBuffer;

        /// <summary>
        /// The byte buffer used to read the network stream.
        /// </summary>
        private byte[] inBuffer;

        /// <summary>
        /// Offset of the unparsed bytes in the inBuffer.
        /// </summary>
        private int unparsedOffset;

        /// <summary>
        /// Count of the unparsed bytes in the inBuffer or 0 if there is no unparsed bytes.
        /// </summary>
        private int unparsedCount;

        /// <summary>
        /// This is an uncommited package remained from the last receive operation. In the next receive operation
        /// this package will continue parsing the incoming bytes.
        /// </summary>
        private RCPackage lastUncommittedPackage;
    }
}
