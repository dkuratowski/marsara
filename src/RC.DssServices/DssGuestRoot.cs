using RC.NetworkingSystem;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// This is the root class of the guest-side-BL in the RC.DssServices module. This is a singleton class that must be
    /// created by the DssServiceAccess when a DSS connection is being created.
    class DssGuestRoot : DssRoot
    {
        /// <summary>
        /// Constructs a DssGuestRoot object.
        /// </summary>
        public DssGuestRoot(ISimulator simulatorIface, IDssGuestSetup setupIface)
            : base(simulatorIface)
        {
            this.setupIface = setupIface;
            this.lobby = null;
            this.guestSession = new DssGuestSessionSM(this);
            this.setupStep = new SetupStep(DssMode.GUEST_SIDE);

            this.connAckTimeoutClock = null;
            this.setupStepReqTimeoutClock = null;

            TraceManager.WriteAllTrace("DssGuestRoot created", DssTraceFilters.SETUP_STAGE_INFO);
        }

        /// <summary>
        /// Gets or sets the interface to the lobby.
        /// </summary>
        /// <remarks>You can set this interface only once, otherwise you get an exception.</remarks>
        public ILobbyClient Lobby
        {
            get { return this.lobby; }
            set
            {
                if (null == this.lobby) { this.lobby = value; this.lobbyIface = value; }
                else { throw new DssException("Lobby interface already set for DssHostRoot!"); }
            }
        }

        /// <summary>
        /// Gets the step object.
        /// </summary>
        public SetupStep Step { get { return this.setupStep; } }

        /// <summary>
        /// Gets the guest-side session.
        /// </summary>
        public DssGuestSessionSM Session { get { return this.guestSession; } }

        /// <summary>
        /// Gets the setup interface of the client module.
        /// </summary>
        public IDssGuestSetup SetupIface { get { return this.setupIface; } }

        /// <summary>
        /// Gets the index of this guest in the DSS.
        /// </summary>
        public int IndexOfThisGuest { get { return this.IdOfThisPeer - 1; } }

        /// <summary>
        /// Gets or sets the timeout clock of the DSS_CTRL_CONN_ACK message from the host.
        /// </summary>
        public AlarmClock ConnAckTimeoutClock { get { return this.connAckTimeoutClock; } set { this.connAckTimeoutClock = value; } }

        /// <summary>
        /// Gets or sets the clock that measures the timeout between the sending of an answer and receiving the next request during setup stage.
        /// </summary>
        public AlarmClock SetupStepReqTimeoutClock { get { return this.setupStepReqTimeoutClock; } set { this.setupStepReqTimeoutClock = value; } }

        /// <see cref="DssRoot.Dispose_i"/>
        protected override void Dispose_i()
        {
        }

        /// <summary>
        /// Interface to the setup manager in the client module.
        /// </summary>
        private IDssGuestSetup setupIface;

        /// <summary>
        /// Interface to the lobby.
        /// </summary>
        private ILobbyClient lobby;

        /// <summary>
        /// Wrapper of the SMC that controls the whole DSS lifecycle at guest side.
        /// </summary>
        private DssGuestSessionSM guestSession;

        /// <summary>
        /// The setup step parser for parsing messages arrived from the host during setup stage.
        /// </summary>
        private SetupStep setupStep;

        /// <summary>
        /// This clock measures the timeout of the DSS_CTRL_CONN_ACK message from the host.
        /// </summary>
        private AlarmClock connAckTimeoutClock;

        /// <summary>
        /// This clock measures the timeout between the sending of an answer and receiving the next request during setup stage.
        /// </summary>
        private AlarmClock setupStepReqTimeoutClock;
    }
}
