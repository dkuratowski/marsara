using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// This static class is used to access the constants of the RC.DssServices module.
    /// </summary>
    static class DssConstants
    {
        /// <summary>
        /// The initial capacity of the alarm clock manager.
        /// </summary>
        public static readonly int INITIAL_ALARM_CLOCK_CAPACITY = ConstantsTable.Get<int>("RC.DssServices.InitialAlarmClockCapacity");

        /// <summary>
        /// The maximum amount of events in the event-queue.
        /// </summary>
        public static readonly int EVENT_QUEUE_CAPACITY = ConstantsTable.Get<int>("RC.DssServices.EventQueueCapacity");

        /// <summary>
        /// This is the minimum waiting time between two setup steps.
        /// </summary>
        public static readonly int SETUP_STEP_CYCLE_TIME = ConstantsTable.Get<int>("RC.DssServices.SetupStepCycleTime");

        /// <summary>
        /// The length of the vector in an average calculator.
        /// </summary>
        public static readonly int AVG_CALC_VECTOR_LENGTH = ConstantsTable.Get<int>("RC.DssServices.AvgCalcVectorLength");

        /// <summary>
        /// The initial value of the AFT values.
        /// </summary>
        public static readonly int INITIAL_AFT = ConstantsTable.Get<int>("RC.DssServices.InitialAft");

        /// <summary>
        /// The initial value of the APT values.
        /// </summary>
        public static readonly int INITIAL_APT = ConstantsTable.Get<int>("RC.DssServices.InitialApt");

        /// <summary>
        /// The maximum number of frames in a round.
        /// </summary>
        public static readonly int MAX_FRAME_NUM = ConstantsTable.Get<int>("RC.DssServices.MaxFrameNum");

        /// <summary>
        /// The minimum number of frames in a round.
        /// </summary>
        public static readonly int MIN_FRAME_NUM = ConstantsTable.Get<int>("RC.DssServices.MinFrameNum");

        /// <summary>
        /// The maximum target frame time.
        /// </summary>
        public static readonly int MAX_TARGET_FRAME_TIME = ConstantsTable.Get<int>("RC.DssServices.MaxTargetFrameTime");

        /// <summary>
        /// The minimum target frame time.
        /// </summary>
        public static readonly int MIN_TARGET_FRAME_TIME = ConstantsTable.Get<int>("RC.DssServices.MinTargetFrameTime");

        /// <summary>
        /// The timeout value between a commit and the answer of that commit.
        /// </summary>
        public static readonly int COMMIT_ANSWER_TIMEOUT = ConstantsTable.Get<int>("RC.DssServices.CommitAnswerTimeout");

        /// <summary>
        /// The maximum amount of time that the simulation manager is waiting for a missing commit.
        /// </summary>
        public static readonly int COMMIT_TIMEOUT = ConstantsTable.Get<int>("RC.DssServices.CommitTimeout");

        /// <summary>
        /// The timeout value between a setup step request and the answers of that request.
        /// </summary>
        public static readonly int SETUP_STEP_ANSWER_TIMEOUT = ConstantsTable.Get<int>("RC.DssServices.SetupStepAnswerTimeout");

        /// <summary>
        /// The timeout value between the lobby connection of a guest and a DSS_CTRL_CONN_REQUEST message from that guest.
        /// </summary>
        public static readonly int CONNECTION_REQUEST_TIMEOUT = ConstantsTable.Get<int>("RC.DssServices.ConnectionRequestTimeout");

        /// <summary>
        /// The timeout value between sending the DSS_CTRL_CONN_REQUEST to and receiving the DSS_CTRL_CONN_ACK from the host.
        /// </summary>
        public static readonly int CONNECTION_ACKNOWLEDGE_TIMEOUT = ConstantsTable.Get<int>("RC.DssServices.ConnectionAcknowledgeTimeout");

        /// <summary>
        /// The timeout value between sending the setup step answer to and receiving the next request from the host at setup stage.
        /// </summary>
        public static readonly int SETUP_STEP_REQUEST_TIMEOUT = ConstantsTable.Get<int>("RC.DssServices.SetupStepRequestTimeout");

        /// <summary>
        /// This flag is true if the communication timeouts shall not be ignored.
        /// </summary>
        public static readonly bool TIMEOUTS_NOT_IGNORED = ConstantsTable.Get<bool>("RC.DssServices.TimeoutsNotIgnored");
    }

    /// <summary>
    /// This static class is used to access the IDs of the trace filters defined for the RC.DssServices module.
    /// </summary>
    static class DssTraceFilters
    {
        public static readonly int ALARM_CLOCK_MANAGER_INFO = TraceManager.GetTraceFilterID("RC.DssServices.AlarmClockManagerInfo");
        public static readonly int EVENT_QUEUE_INFO = TraceManager.GetTraceFilterID("RC.DssServices.EventQueueInfo");
        public static readonly int SETUP_STAGE_INFO = TraceManager.GetTraceFilterID("RC.DssServices.SetupStageInfo");
        public static readonly int SETUP_STAGE_ERROR = TraceManager.GetTraceFilterID("RC.DssServices.SetupStageError");
        public static readonly int SIMULATION_INFO = TraceManager.GetTraceFilterID("RC.DssServices.SimulationInfo");
        public static readonly int SIMULATION_ERROR = TraceManager.GetTraceFilterID("RC.DssServices.SimulationError");
    }
}
