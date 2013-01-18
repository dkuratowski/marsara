using RC.NetworkingSystem;
using System.Threading;
using RC.Common.Diagnostics;

namespace RC.DssServices.TestConsole
{
    class LobbyLocatorImpl : ILobbyLocator
    {
        public LobbyLocatorImpl()
        {
            this.foundLobby = null;
            this.foundEvt = new AutoResetEvent(false);
        }

        public void LobbyFound(LobbyInfo foundLobby)
        {
            this.foundLobby = foundLobby;
            this.foundEvt.Set();
        }

        public void LobbyChanged(LobbyInfo changedLobby)
        {
            /// Do nothing
        }

        public void LobbyVanished(LobbyInfo vanishedLobby)
        {
            /// Do nothing
        }

        public LobbyInfo WaitForFirstLobby()
        {
            TraceManager.WriteAllTrace("Waiting for first lobby!", TestConsoleTraceFilters.TEST_INFO);
            this.foundEvt.WaitOne();
            LobbyInfo ret = this.foundLobby;
            TraceManager.WriteAllTrace(string.Format("Lobby found: {0}", ret.ToString()), TestConsoleTraceFilters.TEST_INFO);
            return ret;
        }

        private LobbyInfo foundLobby;

        private AutoResetEvent foundEvt;
    }
}
