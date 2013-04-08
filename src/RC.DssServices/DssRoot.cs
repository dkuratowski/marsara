using System;
using RC.Common;
using System.Diagnostics;
using RC.NetworkingSystem;
using RC.Common.Diagnostics;
using RC.Common.Configuration;

namespace RC.DssServices
{
    /// <summary>
    /// Enumerates the possible modes of a DSS object
    /// </summary>
    enum DssMode
    {
        HOST_SIDE = 0,       /// The object is running on host-side
        GUEST_SIDE = 1       /// The object is running on guest-side
    }

    /// <summary>
    /// This is the root class of the BL in the RC.DssServices module. This is a singleton class that must be
    /// created by the DssServiceAccess when a DSS or a connection to a DSS is being created.
    /// </summary>
    abstract class DssRoot : IDisposable
    {
        #region Static members

        static DssRoot()
        {
            APPLICATION_VERSION = new Version(ConstantsTable.Get<string>("RC.App.Version"));

            DSS_CTRL_CONN_REQUEST = RCPackageFormatMap.Get("RC.DssServices.DssCtrlConnectionRequest");
            DSS_CTRL_CONN_ACK = RCPackageFormatMap.Get("RC.DssServices.DssCtrlConnectionAcknowledge");
            DSS_CTRL_CONN_REJECT = RCPackageFormatMap.Get("RC.DssServices.DssCtrlConnectionReject");
            DSS_CTRL_SETUP_STEP_RQ_BEGIN = RCPackageFormatMap.Get("RC.DssServices.DssCtrlSetupStepRqBegin");
            DSS_CTRL_SETUP_STEP_AW_BEGIN = RCPackageFormatMap.Get("RC.DssServices.DssCtrlSetupStepAwBegin");
            DSS_CTRL_SETUP_STEP_MSG_END = RCPackageFormatMap.Get("RC.DssServices.DssCtrlSetupStepMsgEnd");
            DSS_CTRL_START_SIMULATION = RCPackageFormatMap.Get("RC.DssServices.DssCtrlStartSimulation");
            DSS_SIM_ERROR = RCPackageFormatMap.Get("RC.DssServices.DssSimulationError");
            DSS_COMMIT = RCPackageFormatMap.Get("RC.DssServices.DssCommit");
            DSS_COMMIT_ANSWER = RCPackageFormatMap.Get("RC.DssServices.DssCommitAnswer");
            DSS_CTRL_DROP_GUEST = RCPackageFormatMap.Get("RC.DssServices.DssCtrlDropGuest");
            DSS_LEAVE = RCPackageFormatMap.Get("RC.DssServices.DssLeave");
            DSS_COMMAND = RCPackageFormatMap.Get("RC.DssServices.DssCommand");
        }

        /// <summary>
        /// Checks whether the version given in the parameter is compatible with the current version of the component.
        /// </summary>
        /// <param name="otherVersion">The other version to check.</param>
        /// <returns>True in case of compatibility, false otherwise.</returns>
        public static bool IsCompatibleVersion(Version otherVersion)
        {
            if (otherVersion == null) { throw new ArgumentNullException("otherVersion"); }
            return otherVersion.CompareTo(APPLICATION_VERSION) == 0;
        }

        /// <summary>
        /// Checks whether the given format is an internal format of the RC.DssServices or not.
        /// </summary>
        /// <param name="format">The format you want to check.</param>
        /// <returns>True in case of internal format, false otherwise.</returns>
        public static bool IsInternalFormat(RCPackageFormat format)
        {
            return format.ID == DSS_CTRL_CONN_REQUEST ||
                   format.ID == DSS_CTRL_CONN_ACK ||
                   format.ID == DSS_CTRL_CONN_REJECT ||
                   format.ID == DSS_CTRL_SETUP_STEP_AW_BEGIN ||
                   format.ID == DSS_CTRL_SETUP_STEP_RQ_BEGIN ||
                   format.ID == DSS_CTRL_SETUP_STEP_MSG_END ||
                   format.ID == DSS_CTRL_START_SIMULATION ||
                   format.ID == DSS_SIM_ERROR ||
                   format.ID == DSS_COMMIT ||
                   format.ID == DSS_COMMIT_ANSWER ||
                   format.ID == DSS_CTRL_DROP_GUEST ||
                   format.ID == DSS_LEAVE ||
                   format.ID == DSS_COMMAND;
        }

        /// <summary>
        /// Internal RCPackageFormat definitions.
        /// </summary>
        public static readonly int DSS_CTRL_CONN_REQUEST;       /// Connection request
        public static readonly int DSS_CTRL_CONN_ACK;           /// Connection acknowledge
        public static readonly int DSS_CTRL_CONN_REJECT;        /// Connection reject
        public static readonly int DSS_CTRL_SETUP_STEP_RQ_BEGIN;  /// Indicates the beginning of a setup step request
        public static readonly int DSS_CTRL_SETUP_STEP_AW_BEGIN;   /// Indicates the beginning of a setup step answer
        public static readonly int DSS_CTRL_SETUP_STEP_MSG_END;     /// Indicates the end of a setup step request/answer

        public static readonly int DSS_CTRL_START_SIMULATION;   /// Simulation start indicator
        public static readonly int DSS_SIM_ERROR;          /// Simulation error indicator
        public static readonly int DSS_COMMIT;                  /// Commit sent by the operators at the end of the rounds
        public static readonly int DSS_COMMIT_ANSWER;      /// Answer to the commits (used to measure APT values)
        public static readonly int DSS_CTRL_DROP_GUEST;         /// Sent to a guest that is being dropped by the host
        public static readonly int DSS_LEAVE;                   /// Sent by an operator that wants to leave the DSS
        public static readonly int DSS_COMMAND;                 /// A command during the simulation stage

        /// <summary>
        /// The current version of the component.
        /// </summary>
        public static readonly Version APPLICATION_VERSION;

        /// <summary>
        /// Gets the singleton instance of this class or null if no instance exists.
        /// </summary>
        //public static DssRoot Instance { get { return instance; } }

        /// <summary>
        /// Reference to the singleton instance of this class or null if no instance exists.
        /// </summary>
        private static DssRoot instance;

        #endregion

        /// <summary>
        /// Constructs a DssRoot object.
        /// </summary>
        /// <param name="simulatorIface">Interface of the simulator in the client module.</param>
        /// <exception cref="DssException">If another instance of this class exists.</exception>
        public DssRoot(ISimulator simulatorIface)
        {
            if (instance != null) { throw new DssException("Another instance of DssRoot already exists!"); }

            this.simulatorIface = simulatorIface;
            this.eventQueue = new DssEventQueue();
            this.alarmClockMgr = new AlarmClockManager(this.eventQueue);
            this.idOfThisPeer = -1;
            this.opCount = -1;
            this.lobbyIface = null;
            this.simulationMgr = new DssSimulationMgr(this);

            this.lifeTimer = new Stopwatch();
            this.lifeTimer.Start();

            instance = this;

            TraceManager.WriteAllTrace("DssRoot successfully created.", DssTraceFilters.SETUP_STAGE_INFO);
        }

        /// <summary>
        /// Initializes the data model.
        /// </summary>
        /// <param name="idOfThisPeer">The ID of this peer in the DSS-lobby.</param>
        /// <param name="opCount">The number of the operators in the DSS including the host.</param>
        public void InitRoot(int idOfThisPeer, int opCount)
        {
            if (this.idOfThisPeer == -1 && this.opCount == -1)
            {
                this.idOfThisPeer = idOfThisPeer;
                this.opCount = opCount;

                /// Initialize the derived class.
                InitRoot_i(idOfThisPeer, opCount);
            }
            else
            {
                throw new DssException("DssRoot already initialized!");
            }
        }

        /// <summary>
        /// Gets the total elapsed time in the life of the DSS (in milliseconds).
        /// </summary>
        public static int Time
        {
            get
            {
                if (instance != null)
                {
                    return (int)instance.lifeTimer.ElapsedMilliseconds;
                }
                else
                {
                    throw new DssException("DssRoot singleton instance doesn't exist!");
                }
            }
        }

        /// <summary>
        /// Gets the event queue of the DSS.
        /// </summary>
        public DssEventQueue EventQueue { get { return this.eventQueue; } }

        /// <summary>
        /// Gets the alarm clock manager of the DSS.
        /// </summary>
        public AlarmClockManager AlarmClkMgr { get { return this.alarmClockMgr; } }

        /// <summary>
        /// Gets the interface of the simulator in the client module.
        /// </summary>
        public ISimulator SimulatorIface { get { return this.simulatorIface; } }

        /// <summary>
        /// Gets the ID of this peer inside the DSS-lobby.
        /// </summary>
        public int IdOfThisPeer
        {
            get
            {
                if (this.idOfThisPeer != -1) { return this.idOfThisPeer; }
                else { throw new DssException("DssRoot.IdOfThisPeer unknown!"); }
            }
        }

        /// <summary>
        /// Gets the number of operators including the host.
        /// </summary>
        public int OpCount
        {
            get
            {
                if (this.opCount != -1) { return this.opCount; }
                else { throw new DssException("DssRoot.OpCount unknown!"); }
            }
        }

        /// <summary>
        /// Gets the simulation manager object.
        /// </summary>
        public DssSimulationMgr SimulationMgr { get { return this.simulationMgr; } }

        /// <summary>
        /// Gets the interface of the lobby.
        /// </summary>
        public ILobby LobbyIface { get { return this.lobbyIface; } }

        /// <summary>
        /// Internal function for indicating if an operator has left the DSS during simulation stage.
        /// </summary>
        /// <param name="senderID">The index of the leaving operator.</param>
        /// <remarks>
        /// This method is only implemented at host side for closing the corresponding lobby line.
        /// </remarks>
        public virtual void OperatorLeftSimulationStage(int senderID) { }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose_i();

            this.simulationMgr.Dispose();
            this.alarmClockMgr.Dispose();
            this.eventQueue.Dispose();
            instance = null;
        }

        #endregion

        /// <summary>
        /// Internal function for initializing the derived classes.
        /// </summary>
        protected virtual void InitRoot_i(int idOfThisPeer, int opCount) { }

        /// <summary>
        /// Internal function for finalizing the derived classes.
        /// </summary>
        protected virtual void Dispose_i() { }

        /// <summary>
        /// Reference to the lobby.
        /// </summary>
        protected ILobby lobbyIface;

        /// <summary>
        /// The event queue that corresponds to the DssRoot object.
        /// </summary>
        private DssEventQueue eventQueue;

        /// <summary>
        /// The interface of the simulator in the client module.
        /// </summary>
        private ISimulator simulatorIface;

        /// <summary>
        /// This object measures the total elapsed time in the life of the DSS.
        /// </summary>
        private Stopwatch lifeTimer;

        /// <summary>
        /// The alarm clock that can be used to call given functions at given times.
        /// </summary>
        private AlarmClockManager alarmClockMgr;

        /// <summary>
        /// The ID of this peer inside the DSS-lobby. If this ID is 0, then this is the host, else if this ID is
        /// N then this is the (N-1)th guest.
        /// </summary>
        private int idOfThisPeer;

        /// <summary>
        /// The total number of operators including the host.
        /// </summary>
        private int opCount;

        /// <summary>
        /// Reference to the simulation manager object.
        /// </summary>
        private DssSimulationMgr simulationMgr;
    }
}
