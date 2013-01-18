using System.Collections.Generic;
using RC.Common.SMC;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// This class is a wrapper over the state machine of a DSS-channel.
    /// </summary>
    class DssChannelSM
    {
        /// <summary>
        /// Constructs a DssChannelSM object.
        /// </summary>
        /// <param name="smc">The SM-controller that is used to construct the underlying state machine.</param>
        public DssChannelSM(IStateMachine sm, DssManagerSM manager, DssHostSessionSM session, int idx)
        {
            this.session = session;
            this.channelIndex = idx;
            this.sm = sm;
            this.manager = manager;
            this.externalTriggersCreated = false;
            this.internalTriggersCreated = false;

            /// Creating the states
            this.Start = this.sm.AddState("Start", null);
            this.Opened = this.sm.AddState("Opened", null);
            this.Engaging = this.sm.AddState("Engaging", null);
            this.Engaged = this.sm.AddState("Engaged", this.GuestConnected);
            this.Closing = this.sm.AddState("Closing", null);
            this.Closed = this.sm.AddState("Closed", null);
            this.Opening = this.sm.AddState("Opening", null);
            this.Terminating = this.sm.AddState("Terminating", null);
            this.Simulating = this.sm.AddState("Simulating", null);
            this.PermanentlyClosed = this.sm.AddState("PermanentlyClosed", null);

            /// Setting the initial state
            this.sm.SetInitialState(this.Start);
        }

        /// <summary>
        /// Creates the external triggers of the underlying state machine.
        /// </summary>
        public void CreateExternalTriggers()
        {
            if (!this.externalTriggersCreated)
            {
                this.Start_Opened = this.Start.AddExternalTrigger(this.Opened, null);
                this.Opened_Engaging = this.Opened.AddExternalTrigger(this.Engaging, this.StartConnReqTimeout);
                this.Opened_Closing = this.Opened.AddExternalTrigger(this.Closing, null);
                this.Opened_PermanentlyClosed = this.Opened.AddExternalTrigger(this.PermanentlyClosed, null);
                this.Engaging_Terminating = this.Engaging.AddExternalTrigger(this.Terminating, this.StopConnReqTimeout);
                this.Engaged_Closing = this.Engaged.AddExternalTrigger(this.Closing, null);
                this.Engaged_Terminating = this.Engaged.AddExternalTrigger(this.Terminating, null);
                this.Engaged_Simulating = this.Engaged.AddExternalTrigger(this.Simulating, null);
                this.Engaged_PermanentlyClosed = this.Engaged.AddExternalTrigger(this.PermanentlyClosed, null);
                this.Closing_Closed = this.Closing.AddExternalTrigger(this.Closed, null);
                this.Closed_Opening = this.Closed.AddExternalTrigger(this.Opening, null);
                this.Closed_PermanentlyClosed = this.Closed.AddExternalTrigger(this.PermanentlyClosed, null);
                this.Opening_Opened = this.Opening.AddExternalTrigger(this.Opened, null);
                this.Terminating_Opening = this.Terminating.AddExternalTrigger(this.Opening, null);

                this.externalTriggersCreated = true;
            }
            else
            {
                throw new DssException("External triggers have already been created!");
            }
        }

        /// <summary>
        /// Creates the internal triggers of the underlying state machine.
        /// </summary>
        public void CreateInternalTriggers()
        {
            if (!this.internalTriggersCreated)
            {
                this.Engaging.AddInternalTrigger(this.Engaged,
                                                 this.StopConnReqTimeout,
                                                 new HashSet<ISMState>(new ISMState[1] { this.session.SendingSetupStepRQ }));

                this.internalTriggersCreated = true;
            }
            else
            {
                throw new DssException("Internal triggers have already been created!");
            }
        }

        #region Trigger methods

        /// <summary>
        /// This function is called when the underlying lobby line has moved to the Opened state during setup stage.
        /// </summary>
        public void SetupStageLineOpened()
        {
            if (this.CurrentState == this.Start)
            {
                this.Start_Opened.Fire();
            }
            else if (this.CurrentState == this.Opening)
            {
                this.Opening_Opened.Fire();
            }
            else if (this.CurrentState == this.Engaged)
            {
                this.session.SetupStageError();
                SetupStageError();
            }
            else if (this.CurrentState == this.Engaging)
            {
                this.session.SetupStageError();
                this.Engaging_Terminating.Fire();
            }
            else if (this.CurrentState == this.Terminating)
            {
                /// Ignore
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.SetupStageLineOpened.");
            }
        }

        /// <summary>
        /// This function is called when the underlying lobby line has moved to the Closed state during setup stage.
        /// </summary>
        public void SetupStageLineClosed()
        {
            if (this.CurrentState == this.Closing)
            {
                this.Closing_Closed.Fire();
            }
            else if (this.CurrentState == this.Terminating)
            {
                this.Terminating_Opening.Fire();
                this.manager.HostRoot.Lobby.OpenLine(this.channelIndex + 1);
            }
            else if (this.CurrentState == this.PermanentlyClosed)
            {
                /// Ignore
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.SetupStageLineClosed.");
            }
        }

        /// <summary>
        /// This function is called when the underlying lobby line has moved to the Engaged state during setup stage.
        /// </summary>
        public void SetupStageLineEngaged()
        {
            if (this.CurrentState == this.Opened)
            {
                this.Opened_Engaging.Fire();
            }
            else if (this.CurrentState == this.Closing || this.CurrentState == this.PermanentlyClosed)
            {
                /// Ignore
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.SetupStageLineEngaged.");
                //Console.WriteLine("Warning! Wrong call to SetupStageLineEngaged at a channel");
            }
        }

        /// <summary>
        /// This function is called when the client module decided to start the simulation stage.
        /// </summary>
        public void StartSimulation()
        {
            if (this.CurrentState == this.Engaged)
            {
                this.Engaged_Simulating.Fire();
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.StartSimulation.");
            }
        }

        /// <summary>
        /// Call this function if you want to permanently close this channel (switching to simulation stage).
        /// </summary>
        public void PermanentlyClose()
        {
            if (this.CurrentState == this.Opened)
            {
                this.Opened_PermanentlyClosed.Fire();
                this.manager.HostRoot.Lobby.CloseLine(this.channelIndex + 1);
            }
            else if (this.CurrentState == this.Closed)
            {
                this.Closed_PermanentlyClosed.Fire();
            }
            else if (this.CurrentState == this.Engaged)
            {
                this.session.DropGuest();
                this.Engaged_PermanentlyClosed.Fire();
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.PermanentlyClose.");
            }
        }

        #endregion Trigger methods

        #region Timeout methods

        /// <summary>
        /// This function is called in case of CONNECTION_REQUEST_TIMEOUT.
        /// </summary>
        /// <param name="whichClock">Reference to the AlarmClock that fired the timeout.</param>
        /// <param name="param">An optional parameter.</param>
        private void ConnectionReqTimeout(AlarmClock whichClock, object param)
        {
            if (DssConstants.TIMEOUTS_NOT_IGNORED)
            {
                int sessionIdx = this.manager.HostRoot.FindSessionOfConnReqTimeoutClock(whichClock);
                if (sessionIdx == this.Index)
                {
                    TraceManager.WriteAllTrace(string.Format("CONNECTION_REQUEST_TIMEOUT on session-{0}", sessionIdx), DssTraceFilters.SETUP_STAGE_ERROR);

                    this.manager.HostRoot.Lobby.CloseLine(this.Index + 1);
                    this.session.SetupStageError();
                    this.Engaging_Terminating.Fire();
                    this.manager.SMC.ExecuteFirings();
                }
                else
                {
                    throw new DssException("Illegal call to DssChannelSM.ConnectionReqTimeout.");
                }
            }
        }

        #endregion Timeout methods

        /// <summary>
        /// Gets the current state of this channel.
        /// </summary>
        public ISMState CurrentState { get { return this.sm.CurrentState; } }

        /// <summary>
        /// Gets the index of this channel.
        /// </summary>
        public int Index { get { return this.channelIndex; } }

        /// <summary>
        /// Client module request to close this channel.
        /// </summary>
        public void CloseChannel()
        {
            if (this.CurrentState == this.Opened)
            {
                this.manager.HostRoot.Lobby.CloseLine(this.channelIndex + 1);
                this.Opened_Closing.Fire();
            }
            else
            {
                throw new DssException("Unable to close the channel from current state!");
            }
        }

        /// <summary>
        /// Client module request to open this channel.
        /// </summary>
        public void OpenChannel()
        {
            if (this.CurrentState == this.Closed)
            {
                this.manager.HostRoot.Lobby.OpenLine(this.channelIndex + 1);
                this.Closed_Opening.Fire();
            }
            else
            {
                throw new DssException("Unable to open the channel from current state!");
            }
        }

        /// <summary>
        /// Client module request to drop the guest and close this channel or keep it opened.
        /// </summary>
        /// <param name="keepOpened">True if the channel has to be kept opened, false if it has to be closed.</param>
        public void DropGuest(bool keepOpened)
        {
            if (this.CurrentState == this.Engaged)
            {
                this.session.DropGuest();
                if (keepOpened)
                {
                    this.Engaged_Terminating.Fire();
                }
                else
                {
                    this.Engaged_Closing.Fire();
                }
            }
            else
            {
                throw new DssException("Unable to drop the guest from current state!");
            }
        }

        /// <summary>
        /// Called when a guest has been connected to this channel.
        /// </summary>
        private void GuestConnected(ISMState state)
        {
            this.manager.HostRoot.CreateNewStep(this.channelIndex);
        }

        #region Methods called by the underlying session

        /// <summary>
        /// Called by the session when a DSS_LEAVE message arrived from the corresponding guest during setup stage.
        /// </summary>
        public void GuestLeaveSetupStage()
        {
            if (this.sm.CurrentState == this.Engaged)
            {
                this.manager.HostRoot.GuestLeftDss(this.channelIndex);
                bool keepOpened = this.manager.HostRoot.SetupIface.GuestLeftDss(this.channelIndex);
                if (keepOpened)
                {
                    this.Engaged_Terminating.Fire();
                }
                else
                {
                    this.Engaged_Closing.Fire();
                }
                this.manager.HostRoot.Lobby.CloseLine(this.channelIndex + 1);
            }
            else
            {
                throw new DssException("Illegal call on DssChannelSM.GuestLeaveSetupStage");
            }
        }

        /// <summary>
        /// This function is called when an error occurs on the session corresponding to this channel during setup stage.
        /// </summary>
        public void SetupStageError()
        {
            if (this.sm.CurrentState == this.Engaged)
            {
                /// Guest error during setup stage
                this.manager.HostRoot.GuestConnectionLost(this.channelIndex);
                bool keepOpened = this.manager.HostRoot.SetupIface.GuestConnectionLost(this.channelIndex);
                if (keepOpened)
                {
                    this.Engaged_Terminating.Fire();
                }
                else
                {
                    this.Engaged_Closing.Fire();
                }
                this.manager.HostRoot.Lobby.CloseLine(this.channelIndex + 1);
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.SetupStageError!");
            }
        }

        #endregion Methods called by the underlying session

        /// <summary>
        /// This function is called when the CONNECTION_REQUEST_TIMEOUT clock has to be started.
        /// </summary>
        private void StartConnReqTimeout(ISMState targetState, ISMState sourceState)
        {
            if (sourceState == this.Opened && targetState == this.Engaging)
            {
                AlarmClock connReqTimeoutClk = this.manager.HostRoot.AlarmClkMgr.SetAlarmClock(
                    DssRoot.Time + DssConstants.CONNECTION_REQUEST_TIMEOUT,
                    this.ConnectionReqTimeout);
                this.manager.HostRoot.SetConnReqTimeoutClock(this.Index, connReqTimeoutClk);
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.StartConnReqTimeout");
            }
        }

        /// <summary>
        /// This function is called when the CONNECTION_REQUEST_TIMEOUT clock has to be stopped.
        /// </summary>
        private void StopConnReqTimeout(ISMState targetState, ISMState sourceState)
        {
            if (sourceState == this.Engaging && (targetState == this.Engaged || targetState == this.Terminating))
            {
                AlarmClock connReqTimeoutClk = this.manager.HostRoot.GetConnReqTimeoutClock(this.Index);
                if (connReqTimeoutClk != null)
                {
                    connReqTimeoutClk.Cancel();
                    this.manager.HostRoot.SetConnReqTimeoutClock(this.Index, null);
                }
            }
            else
            {
                throw new DssException("Illegal call to DssChannelSM.StopConnReqTimeout");
            }
        }

        /// <summary>
        /// The manager of the DSS.
        /// </summary>
        private DssManagerSM manager;

        /// <summary>
        /// The underlying session object that will handle incoming RCPackages.
        /// </summary>
        private DssHostSessionSM session;

        /// <summary>
        /// The proxy that belongs to this channel.
        /// </summary>
        private int channelIndex;

        /// <summary>
        /// Interface to the state machine of the DSS-channel.
        /// </summary>
        private IStateMachine sm;

        /// <summary>
        /// Becomes true when the external triggers have been created.
        /// </summary>
        private bool externalTriggersCreated;

        /// <summary>
        /// Becomes true when the internal triggers have been created.
        /// </summary>
        private bool internalTriggersCreated;

        /// <summary>
        /// Interface to the external triggers of the DSS-channel.
        /// </summary>
        private ISMTrigger Start_Opened;
        private ISMTrigger Opened_Engaging;
        private ISMTrigger Opened_Closing;
        private ISMTrigger Opened_PermanentlyClosed;
        private ISMTrigger Engaging_Terminating;
        private ISMTrigger Engaged_Closing;
        private ISMTrigger Engaged_Terminating;
        private ISMTrigger Engaged_Simulating;
        private ISMTrigger Engaged_PermanentlyClosed;
        private ISMTrigger Closing_Closed;
        private ISMTrigger Closed_Opening;
        private ISMTrigger Closed_PermanentlyClosed;
        private ISMTrigger Opening_Opened;
        private ISMTrigger Terminating_Opening;

        /// <summary>
        /// The states of the state machine of the DSS-channel.
        /// </summary>
        /// <remarks>
        /// See \docs\model\dss_services\dss_impl.eap for more informations.
        /// </remarks>
        public ISMState Start;
        public ISMState Opened;
        public ISMState Engaging;
        public ISMState Engaged;
        public ISMState Closing;
        public ISMState Closed;
        public ISMState Opening;
        public ISMState Terminating;
        public ISMState Simulating;
        public ISMState PermanentlyClosed;
    }
}
