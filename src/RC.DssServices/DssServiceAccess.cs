using System;
using RC.NetworkingSystem;
using System.Threading;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// The client module can access the services of the RC.DssServices module using this static class.
    /// </summary>
    public static class DssServiceAccess
    {
        /// <summary>
        /// Connects to an existing DSS-lobby on the network.
        /// </summary>
        /// <param name="dssLobby">The information package of the lobby that contains data for the connection.</param>
        /// <param name="network">The network interface that is used to connect.</param>
        /// <param name="simulatorIface">Interface of the local simulator implemented by the client module.</param>
        /// <param name="setupIface">Interface of the setup manager object implemented by the client module.</param>
        /// <remarks>The caller thread will be blocked while the peer is connected to the DSS.</remarks>
        public static void ConnectDSS(LobbyInfo dssLobby, INetwork network, ISimulator simulatorIface, IDssGuestSetup setupIface)
        {
            if (dssLobby == null) { throw new ArgumentNullException("dssLobby"); }
            if (network == null) { throw new ArgumentNullException("network"); }
            if (simulatorIface == null) { throw new ArgumentNullException("simulatorIface"); }
            if (setupIface == null) { throw new ArgumentNullException("setupIface"); }

            //throw new Exception("Test exception");
            dssActive.WaitOne();

            try
            {
                DssGuestRoot guestRoot = new DssGuestRoot(simulatorIface, setupIface);
                GuestEventHandler eventHdl = new GuestEventHandler(guestRoot);
                guestRoot.EventQueue.RegisterHandler(eventHdl);

                ILobbyClient client = network.JoinLobby(dssLobby, guestRoot.EventQueue);
                if (client == null) { throw new DssException("Cannot join to the lobby under the DSS!"); }
                guestRoot.Lobby = client;
                guestRoot.EventQueue.EventLoop();
                client.Disconnect();

                guestRoot.Dispose();
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
            }
            finally
            {
                dssActive.Release();
            }
        }

        /// <summary>
        /// Creates a new DSS-lobby with the given maximum number of operators.
        /// </summary>
        /// <param name="opCount">The maximum number of operators in the DSS (including the host itself).</param>
        /// <param name="network">The network interface that is used to connect.</param>
        /// <param name="simulatorIface">Interface of the local simulator implemented by the client module.</param>
        /// <param name="setupIface">Interface of the setup manager object implemented by the client module.</param>
        /// <remarks>The caller thread will be blocked during the whole lifetime of the DSS.</remarks>
        public static void CreateDSS(int opCount, INetwork network, ISimulator simulatorIface, IDssHostSetup setupIface)
        {
            if (opCount < 2) { throw new ArgumentOutOfRangeException("opCount"); }
            if (network == null) { throw new ArgumentNullException("network"); }
            if (simulatorIface == null) { throw new ArgumentNullException("simulatorIface"); }
            if (setupIface == null) { throw new ArgumentNullException("setupIface"); }

            dssActive.WaitOne();

            try
            {
                DssHostRoot hostRoot = new DssHostRoot(simulatorIface, setupIface, opCount);
                HostEventHandler eventHdl = new HostEventHandler(hostRoot);
                hostRoot.EventQueue.RegisterHandler(eventHdl);

                ILobbyServer server = network.CreateLobby(opCount, hostRoot.EventQueue);
                if (server == null) { throw new DssException("Cannot create the lobby under the DSS!"); }
                server.StartAnnouncing(); /// TODO: add ILobbyCustomDataProvider if necessary
                hostRoot.Lobby = server;
                hostRoot.EventQueue.EventLoop();
                server.Shutdown();

                hostRoot.Dispose();
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
            }
            finally
            {
                dssActive.Release();
            }
        }

        /// <summary>Starts announcing the DSS-lobby on the network created by the local operator.</summary>
        /// <remarks>If there is no DSS-lobby created by the local operator then this function has no effect.</remarks>
        public static void StartAnnouncingDSS() { StartAnnouncingDSS(null); }

        /// <summary>
        /// Starts announcing the DSS-lobby on the network created by the local operator.
        /// </summary>
        /// <param name="customDataProvider">
        /// The object that will generate the custom data package into the announcement. If this parameter is null
        /// then no custom data will be placed into the announcement.
        /// </param>
        /// <remarks>If there is no DSS-lobby created by the local operator then this function has no effect.</remarks>
        public static void StartAnnouncingDSS(ILobbyCustomDataProvider customDataProvider)
        {
        }

        /// <summary>
        /// Stops announcing the DSS-lobby on the network created by the local operator.
        /// </summary>
        /// <remarks>
        /// If there is no DSS-lobby created by the local operator or no announcement of such a DSS-lobby is currently in
        /// progress then this function has no effect.
        /// </remarks>
        public static void StopAnnouncingDSS()
        {
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DssServiceAccess()
        {
            dssActive = new Semaphore(1, 1);
        }

        /// <summary>
        /// Semaphore used to accept only one DSS-thread at a time.
        /// </summary>
        private static Semaphore dssActive;
    }
}
