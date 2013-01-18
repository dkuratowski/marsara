using System;
using System.Collections.Generic;
using RC.NetworkingSystem;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// This is the root class of the host-side-BL in the RC.DssServices module. This is a singleton class that must be
    /// created by the DssServiceAccess when a DSS is being created.
    class DssHostRoot : DssRoot
    {
        /// <summary>
        /// Constructs a DssHostRoot object.
        /// </summary>
        public DssHostRoot(ISimulator simulatorIface, IDssHostSetup setupIface, int opCount)
            : base(simulatorIface)
        {
            this.setupSteps = new SetupStep[opCount - 1];
            this.connReqTimeoutClocks = new AlarmClock[opCount - 1];
            for (int i = 0; i < this.setupSteps.Length; i++)
            {
                this.setupSteps[i] = new SetupStep(DssMode.HOST_SIDE);
                this.connReqTimeoutClocks[i] = null;
            }

            this.setupStepAwTimeoutClock = null;
            this.setupStepClock = null;

            this.leftGuests = new List<int>();
            this.lostGuests = new List<int>();

            this.setupIface = setupIface;
            this.lobby = null;
            this.dssManager = new DssManagerSM(opCount - 1, this);
            TraceManager.WriteAllTrace("DssHostRoot created", DssTraceFilters.SETUP_STAGE_INFO);
        }

        /// <summary>
        /// Gets the step object that belongs to the given guest.
        /// </summary>
        /// <param name="guestIdx">The index of the corresponding setup step.</param>
        /// <returns>The SetupStep object of the given guest.</returns>
        public SetupStep GetStep(int guestIdx)
        {
            if (guestIdx < 0 || guestIdx >= this.setupSteps.Length) { throw new ArgumentOutOfRangeException("guestIdx"); }

            return this.setupSteps[guestIdx];
        }

        /// <summary>
        /// Use this function to create a new setup step parser object for the given guest.
        /// </summary>
        /// <param name="guestIdx">The index of the guest.</param>
        /// <remarks>This function must be used when a channel becomes engaged.</remarks>
        public void CreateNewStep(int guestIdx)
        {
            if (guestIdx < 0 || guestIdx >= this.setupSteps.Length) { throw new ArgumentOutOfRangeException("guestIdx"); }

            this.setupSteps[guestIdx] = new SetupStep(DssMode.HOST_SIDE);
        }

        /// <summary>
        /// Registers in the temporary list that the given guest has left the DSS during the current setup step.
        /// </summary>
        /// <param name="guestIdx">The index of the guest.</param>
        public void GuestLeftDss(int guestIdx)
        {
            if (guestIdx < 0 || guestIdx >= this.OpCount - 1) { throw new ArgumentOutOfRangeException("guestIdx"); }
            this.leftGuests.Add(guestIdx);
        }

        /// <summary>
        /// Registers in the temporary list that the host has lost connection with the given guest during the
        /// current setup step.
        /// </summary>
        /// <param name="guestIdx">The index of the guest.</param>
        public void GuestConnectionLost(int guestIdx)
        {
            if (guestIdx < 0 || guestIdx >= this.OpCount - 1) { throw new ArgumentOutOfRangeException("guestIdx"); }
            this.lostGuests.Add(guestIdx);
        }

        /// <summary>
        /// Gets and resets the list of the guests that left the DSS and the list of the guests that the host has
        /// lost connection with during the current setup step.
        /// </summary>
        /// <param name="leftList">This list will contain the guests that left the DSS.</param>
        /// <param name="lostList">This list will contain the guests that the host has lost connection with.</param>
        public void GetGuestEvents(out int[] leftList, out int[] lostList)
        {
            leftList = this.leftGuests.ToArray();
            lostList = this.lostGuests.ToArray();

            this.leftGuests.Clear();
            this.lostGuests.Clear();
        }

        /// <see cref="DssRoot.OperatorLeftSimulationStage"/>
        public override void OperatorLeftSimulationStage(int senderID)
        {
            if (senderID < 1 || senderID >= this.OpCount) { throw new ArgumentOutOfRangeException("senderID"); }

            this.lobby.CloseLine(senderID);
        }

        /// <summary>
        /// Gets or sets the interface to the lobby.
        /// </summary>
        /// <remarks>You can set this interface only once, otherwise you get an exception.</remarks>
        public ILobbyServer Lobby
        {
            get { return this.lobby; }
            set
            {
                if (null == this.lobby) { this.lobby = value; this.lobbyIface = value; }
                else { throw new DssException("Lobby interface already set for DssHostRoot!"); }
            }
        }

        /// <summary>
        /// Gets the setup interface of the client module.
        /// </summary>
        public IDssHostSetup SetupIface { get { return this.setupIface; } }

        /// <summary>
        /// Gets the manager SMC.
        /// </summary>
        public DssManagerSM Manager { get { return this.dssManager; } }

        /// <summary>
        /// Gets or sets the setup step clock reference.
        /// </summary>
        public AlarmClock SetupStepClock { get { return this.setupStepClock; } set { this.setupStepClock = value; } }

        /// <summary>
        /// Gets or sets the SETUP_STEP_ANSWER_TIMEOUT clock reference.
        /// </summary>
        public AlarmClock SetupStepAwTimeoutClock { get { return this.setupStepAwTimeoutClock; } set { this.setupStepAwTimeoutClock = value; } }

        /// <summary>
        /// Gets the CONNECTION_REQUEST_TIMEOUT clock of the given session.
        /// </summary>
        /// <param name="sessionIdx">The index of the session.</param>
        /// <returns>The CONNECTION_REQUEST_TIMEOUT clock of the given session.</returns>
        public AlarmClock GetConnReqTimeoutClock(int sessionIdx)
        {
            if (sessionIdx < 0 || sessionIdx >= this.OpCount) { throw new ArgumentOutOfRangeException("sessionIdx"); }
            return this.connReqTimeoutClocks[sessionIdx];
        }

        /// <summary>
        /// Sets the CONNECTION_REQUEST_TIMEOUT clock of the given session.
        /// </summary>
        /// <param name="sessionIdx">The index of the session.</param>
        /// <param name="newClock">The clock you want to set for the given session.</param>
        public void SetConnReqTimeoutClock(int sessionIdx, AlarmClock newClock)
        {
            if (sessionIdx < 0 || sessionIdx >= this.OpCount) { throw new ArgumentOutOfRangeException("sessionIdx"); }
            this.connReqTimeoutClocks[sessionIdx] = newClock;
        }

        /// <summary>
        /// Finds the index of the session that has the given AlarmClock as CONNECTION_REQUEST_TIMEOUT clock.
        /// </summary>
        /// <param name="whichClock">The clock you are searching session index for.</param>
        /// <returns>
        /// The index of the session that has the given AlarmClock as CONNECTION_REQUEST_TIMEOUT clock or -1 if no such session exists.
        /// </returns>
        public int FindSessionOfConnReqTimeoutClock(AlarmClock whichClock)
        {
            if (whichClock == null) { throw new ArgumentNullException("whichClock"); }

            for (int i = 0; i < this.connReqTimeoutClocks.Length; i++)
            {
                if (this.connReqTimeoutClocks[i] == whichClock)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <see cref="DssRoot.Dispose_i"/>
        protected override void Dispose_i()
        {
        }

        /// <summary>
        /// Interface to the setup manager in the client module.
        /// </summary>
        private IDssHostSetup setupIface;

        /// <summary>
        /// Interface to the lobby.
        /// </summary>
        private ILobbyServer lobby;

        /// <summary>
        /// Wrapper of the SMC that controls the whole DSS lifecycle at host side.
        /// </summary>
        private DssManagerSM dssManager;

        /// <summary>
        /// List of the setup step parsers for each channels for parsing messages arrived from the guests during setup stage.
        /// </summary>
        private SetupStep[] setupSteps;

        /// <summary>
        /// This temporary list contains the guests that have left the DSS during the current setup step.
        /// </summary>
        private List<int> leftGuests;

        /// <summary>
        /// This temporary list contains the guests that the host has lost connection with during the current setup step.
        /// </summary>
        private List<int> lostGuests;

        /// <summary>
        /// The Nth element of this array measures the timeout between the lobby connection and the incoming
        /// connection request message of guest-N.
        /// </summary>
        private AlarmClock[] connReqTimeoutClocks;

        /// <summary>
        /// This clock measures the timeout between the setup step request and the answer in the current setup step.
        /// </summary>
        private AlarmClock setupStepAwTimeoutClock;

        /// <summary>
        /// This clock is used to wait the necessary amount of time between setup steps during the setup stage.
        /// </summary>
        private AlarmClock setupStepClock;
    }
}
