using System;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.SMC;

namespace RC.DssServices
{
    /// <summary>
    /// This class is a wrapper over the state machine that controls the DSS-channels and sessions at host side.
    /// </summary>
    class DssManagerSM
    {
        /// <summary>
        /// Contructs a DssManagerSM object.
        /// </summary>
        /// <param name="channelCount">The number of the channels.</param>
        public DssManagerSM(int channelCount, DssHostRoot hostRoot)
        {
            if (channelCount < 1) { throw new ArgumentOutOfRangeException("channelCount"); }
            if (hostRoot == null) { throw new ArgumentNullException("hostRoot"); }

            this.controller = new StateMachineController();
            this.hostRoot = hostRoot;
            this.channels = new DssChannelSM[channelCount];
            this.sessions = new DssHostSessionSM[channelCount];
            this.externalTriggersCreated = false;
            this.internalTriggersCreated = false;
            this.channelProxies = new DssChannelProxy[channelCount];

            /// Creating the underlying state machines
            this.managerSM = this.controller.AddStateMachine("DSS-manager");
            this.setupStepTimerSM = this.controller.AddStateMachine("SetupStepTimer");
            this.channelStabilityMonitorSM = this.controller.AddStateMachine("ChannelMonitor");

            /// Creating the states of the DSS-manager                
            this.Start = this.managerSM.AddState("Start", null);
            this.LobbyCreated = this.managerSM.AddState("LobbyCreated", null);
            this.SendingSetupStepRQs = this.managerSM.AddState("SendingSetupStepRQs", this.CallClientModuleSetupIface);
            this.WaitingSetupStepAWs = this.managerSM.AddState("WaitingSetupStepAWs", null);
            this.SimulationStage = this.managerSM.AddState("SimulationStage", null);
            this.managerSM.SetInitialState(this.Start);

            /// Creating the states of the setup step timer
            this.SetupStepTimerInactive = this.setupStepTimerSM.AddState("SetupStepTimerInactive", null);
            this.SetupStepTimerRunning = this.setupStepTimerSM.AddState("SetupStepTimerRunning", this.StartSetupStepTimer);
            this.setupStepTimerSM.SetInitialState(this.SetupStepTimerInactive);

            /// Creating the states of the channel stability monitor
            this.TransientChannelStates = this.channelStabilityMonitorSM.AddState("TransientChannelStates", null);
            this.PermanentChannelStates = this.channelStabilityMonitorSM.AddState("PermanentChannelStates", null);
            this.channelStabilityMonitorSM.SetInitialState(this.TransientChannelStates);

            for (int i = 0; i < channelCount; i++)
            {
                this.channelProxies[i] = new DssChannelProxy(DssMode.HOST_SIDE);//, i);
                IStateMachine channelSM = this.controller.AddStateMachine("Channel-" + i);
                IStateMachine sessionSM = this.controller.AddStateMachine("Session-" + i);
                this.sessions[i] = new DssHostSessionSM(sessionSM, this);
                this.sessions[i].CreateExternalTriggers();
                this.channels[i] = new DssChannelSM(channelSM, this, this.sessions[i], i);
                this.channels[i].CreateExternalTriggers();
                this.sessions[i].Channel = this.channels[i];
            }
            CreateExternalTriggers();

            for (int i = 0; i < channelCount; i++)
            {
                this.sessions[i].CreateInternalTriggers();
                this.channels[i].CreateInternalTriggers();
            }
            CreateInternalTriggers();

            this.controller.CommissionController();

            TraceManager.WriteAllTrace("DSS-manager created", DssTraceFilters.SETUP_STAGE_INFO);
        }

        #region Trigger methods

        /// <summary>
        /// This function is called when the lobby is ready to use.
        /// </summary>
        public void LobbyIsRunning()
        {
            this.Start_LobbyCreated.Fire();
            this.controller.ExecuteFirings();
        }

        /// <summary>
        /// This function is called when an error occurs on a session during setup stage.
        /// </summary>
        /// <param name="whichSession">The index of the session that caused the error.</param>
        public void SetupStageError(int whichSession)
        {
            this.sessions[whichSession].SetupStageError();
            this.controller.ExecuteFirings();
        }

        /// <summary>
        /// This function is called when a control package arrives from a channel during setup stage.
        /// </summary>
        /// <param name="package">The arrived package.</param>
        /// <param name="channelIdx">The index of the channel.</param>
        public void SetupStageCtrlPackage(RCPackage package, int channelIdx)
        {
            /// Control package during setup stage.
            this.sessions[channelIdx].SetupStageCtrlPackage(package);
            this.controller.ExecuteFirings();
        }

        /// <summary>
        /// This function is called when the underlying line of a channel has moved to the Opened state during setup stage.
        /// </summary>
        /// <param name="channelIdx">The index of the channel.</param>
        public void SetupStageLineOpened(int channelIdx)
        {
            this.channels[channelIdx].SetupStageLineOpened();
            this.controller.ExecuteFirings();
        }

        /// <summary>
        /// This function is called when the underlying line of a channel has moved to the Closed state during setup stage.
        /// </summary>
        /// <param name="channelIdx">The index of the channel.</param>
        public void SetupStageLineClosed(int channelIdx)
        {
            this.channels[channelIdx].SetupStageLineClosed();
            this.controller.ExecuteFirings();
        }

        /// <summary>
        /// This function is called when the underlying line of a channel has moved to the Engaged state during setup stage.
        /// </summary>
        /// <param name="channelIdx">The index of the channel.</param>
        public void SetupStageLineEngaged(int channelIdx)
        {
            this.channels[channelIdx].SetupStageLineEngaged();
            this.controller.ExecuteFirings();
        }

        #endregion Trigger methods

        #region Timeout methods

        /// <summary>
        /// This function is called when the setup step timer has a timeout.
        /// </summary>
        /// <param name="whichClock">Reference to the AlarmClock that fired the timeout.</param>
        /// <param name="param">An optional parameter.</param>
        private void SetupStepTimeout(AlarmClock whichClock, object param)
        {
            if (this.setupStepTimerSM.CurrentState == this.SetupStepTimerRunning &&
                whichClock == this.hostRoot.SetupStepClock)
            {
                this.SetupStepTimerRunning_Inactive.Fire();
                this.controller.ExecuteFirings();
            }
            else
            {
                throw new DssException("Illegal call to DssManagerSM.SetupStepTimeout.");
            }
        }

        /// <summary>
        /// This function is called in case of SETUP_STEP_ANSWER_TIMEOUT.
        /// </summary>
        /// <param name="whichClock">Reference to the AlarmClock that fired the timeout.</param>
        /// <param name="param">An optional parameter.</param>
        private void SetupStepAwTimeout(AlarmClock whichClock, object param)
        {
            if (DssConstants.TIMEOUTS_NOT_IGNORED)
            {
                if (whichClock == this.hostRoot.SetupStepAwTimeoutClock)
                {
                    bool timeoutErr = false;
                    for (int i = 0; i < this.sessions.Length; i++)
                    {
                        if (this.sessions[i].CurrentState == this.sessions[i].WaitingSetupStepAW)
                        {
                            TraceManager.WriteAllTrace(string.Format("SETUP_STEP_ANSWER_TIMEOUT on session-{0}", i), DssTraceFilters.SETUP_STAGE_ERROR);
                            this.sessions[i].SetupStageError();
                            this.channels[i].SetupStageError();
                            timeoutErr = true;
                        }
                    }
                    if (timeoutErr)
                    {
                        this.controller.ExecuteFirings();
                    }
                }
                else
                {
                    throw new DssException("Illegal call to DssManagerSM.SetupStepAwTimeout.");
                }
            }
        }

        #endregion Timeout methods

        /// <summary>
        /// Gets the current state of the DSS-manager.
        /// </summary>
        public ISMState CurrentState { get { return this.managerSM.CurrentState; } }

        /// <summary>
        /// Gets the state machine controller of the DSS-manager.
        /// </summary>
        public StateMachineController SMC { get { return this.controller; } }

        /// <summary>
        /// Gets the root of the data model at host-side.
        /// </summary>
        public DssHostRoot HostRoot { get { return this.hostRoot; } }

        /// <summary>
        /// Creates the external triggers of the underlying state machine.
        /// </summary>
        private void CreateExternalTriggers()
        {
            if (!this.externalTriggersCreated)
            {
                /// Creating the external triggers of the DSS-manager
                this.Start_LobbyCreated = this.Start.AddExternalTrigger(this.LobbyCreated, null);
                this.SendingSetupStepRQs_SimulationStage = this.SendingSetupStepRQs.AddExternalTrigger(this.SimulationStage, null);
                this.SendingSetupStepRQs_WaitingSetupStepAWs = this.SendingSetupStepRQs.AddExternalTrigger(this.WaitingSetupStepAWs, this.StartSetupStepAwTimer);

                /// Creating the external triggers of the setup step timer
                this.SetupStepTimerInactive_Running = this.SetupStepTimerInactive.AddExternalTrigger(this.SetupStepTimerRunning, null);
                this.SetupStepTimerRunning_Inactive = this.SetupStepTimerRunning.AddExternalTrigger(this.SetupStepTimerInactive, null);

                /// The channel stability monitor has no external triggers
                /// ...

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
        private void CreateInternalTriggers()
        {
            if (!this.internalTriggersCreated)
            {
                /// Creating the operators needed for the internal triggers.
                SMOperator[] channelStabilityOps = new SMOperator[this.channels.Length];
                for (int i = 0; i < this.channels.Length; i++)
                {
                    /// The channel stability operator of channel[i] is satisfied in 2 cases:
                    ///     Channel is engaged and the underlying session has been connected OR...
                    SMOperator sessionIsStable = new SMOperator(SMOperatorType.AND, new ISMState[2] { this.channels[i].Engaged,
                                                                                                          this.sessions[i].SendingSetupStepRQ });
                    /// ... the channel is opened or closed and the underlying session has reached the inactive state.
                    SMOperator channelNotEngaged = new SMOperator(SMOperatorType.OR, new ISMState[2] { this.channels[i].Opened,
                                                                                                           this.channels[i].Closed });
                    SMOperator noSession = new SMOperator(SMOperatorType.AND, new ISMState[1] { this.sessions[i].Inactive },
                                                                              new SMOperator[1] { channelNotEngaged });
                    /// Here we create the channel stability operator for the channel[i].
                    channelStabilityOps[i] = new SMOperator(SMOperatorType.OR, new SMOperator[2] { sessionIsStable, noSession });
                }

                /// The channels are in permanent states when all channel stability operators are satisfied.
                SMOperator permanentChannelStatesOp = new SMOperator(SMOperatorType.AND, channelStabilityOps);
                SMOperator transientChannelStatesOp = new SMOperator(SMOperatorType.NOT, new SMOperator[1] { permanentChannelStatesOp });

                /// The setup can continue when all channels are stable and the setup step timer is over.
                SMOperator setupCanContinueOp = new SMOperator(SMOperatorType.AND, new ISMState[1] { this.SetupStepTimerInactive },
                                                                                   new SMOperator[1] { permanentChannelStatesOp });

                /// Creating the internal triggers of the DSS-manager
                this.LobbyCreated.AddInternalTrigger(this.SendingSetupStepRQs, null, permanentChannelStatesOp);
                this.WaitingSetupStepAWs.AddInternalTrigger(this.SendingSetupStepRQs, this.StopSetupStepAwTimer, setupCanContinueOp);

                /// Creating the internal triggers of the channel stability monitor
                this.TransientChannelStates.AddInternalTrigger(this.PermanentChannelStates, null, permanentChannelStatesOp);
                this.PermanentChannelStates.AddInternalTrigger(this.TransientChannelStates, null, transientChannelStatesOp);

                this.internalTriggersCreated = true;
            }
            else
            {
                throw new DssException("Internal triggers have already been created!");
            }
        }

        /// <summary>
        /// This function is called by the SMC framework when the DSS-manager has to call the setup interface
        /// of the client module.
        /// </summary>
        /// <param name="state"></param>
        private void CallClientModuleSetupIface(ISMState state)
        {
            if (state == this.SendingSetupStepRQs)
            {
                TraceManager.WriteAllTrace("Calling IDssHostSetup.ExecuteNextStep on client module.", DssTraceFilters.SETUP_STAGE_INFO);

                /// Unlock the channel interfaces for the client module.
                IDssGuestChannel[] channelIfaces = this.DssChannelIfaces;
                for (int i = 0; i < this.channels.Length; i++)
                {
                    if (this.channels[i].CurrentState == this.channels[i].Opened)
                    {
                        this.channelProxies[i].UnlockForClient(null, DssChannelState.CHANNEL_OPENED);
                    }
                    else if (this.channels[i].CurrentState == this.channels[i].Closed)
                    {
                        this.channelProxies[i].UnlockForClient(null, DssChannelState.CHANNEL_CLOSED);
                    }
                    else if (this.channels[i].CurrentState == this.channels[i].Engaged)
                    {
                        SetupStep currStep = this.hostRoot.GetStep(i);
                        this.channelProxies[i].UnlockForClient(currStep.StepPackageList, DssChannelState.GUEST_CONNECTED);
                    }
                    else
                    {
                        throw new DssException("Unexpected channel state!");
                    }
                }

                /// Calling the client module for setup step execution.
                DssSetupResult result = this.hostRoot.SetupIface.ExecuteNextStep(channelIfaces);

                /// Lock the channel interfaces.
                for (int i = 0; i < this.channels.Length; i++)
                {
                    this.channelProxies[i].Lock();
                }

                /// Evaluate the result of the setup step execution.
                if (result == DssSetupResult.CONTINUE_SETUP)
                {
                    ContinueSetup();
                }
                else if (result == DssSetupResult.LEAVE_DSS)
                {
                    LeaveDss();
                }
                else if (result == DssSetupResult.START_SIMULATION)
                {
                    StartSimulation();
                }
                else
                {
                    throw new DssException("Unexpected answer from client module!");
                }
            }
            else
            {
                throw new DssException("Unexpected state!");
            }
        }

        /// <summary>
        /// Gets a copy of the list of the DssChannelProxies.
        /// </summary>
        private IDssGuestChannel[] DssChannelIfaces
        {
            get
            {
                IDssGuestChannel[] retList = new IDssGuestChannel[this.channelProxies.Length];
                for (int i = 0; i < this.channelProxies.Length; i++)
                {
                    retList[i] = this.channelProxies[i];
                }
                return retList;
            }
        }

        /// <summary>
        /// Sends the setup step requests to the guests, perform the tasks initiated by the client module and
        /// send the SMC to waiting state.
        /// </summary>
        private void ContinueSetup()
        {
            this.SetupStepTimerInactive_Running.Fire();
            this.SendingSetupStepRQs_WaitingSetupStepAWs.Fire();

            /// First handle the channels where drop happened.
            for (int i = 0; i < this.channelProxies.Length; i++)
            {
                if (this.channelProxies[i].TaskToPerform == DssChannelTask.DROP_AND_OPEN ||
                    this.channelProxies[i].TaskToPerform == DssChannelTask.DROP_AND_CLOSE)
                {
                    /// Drop guest and close channel if necessary.
                    this.channels[i].DropGuest(this.channelProxies[i].TaskToPerform == DssChannelTask.DROP_AND_OPEN);
                    this.hostRoot.GuestLeftDss(i);
                }
            }

            /// Then handle any other channels.
            for (int i = 0; i < this.sessions.Length; i++)
            {
                if (this.channelProxies[i].TaskToPerform == DssChannelTask.CLOSE)
                {
                    /// Close the channel.
                    this.channels[i].CloseChannel();
                }
                else if (this.channelProxies[i].TaskToPerform == DssChannelTask.OPEN)
                {
                    /// Open the channel.
                    this.channels[i].OpenChannel();
                }
                else if (this.channelProxies[i].TaskToPerform != DssChannelTask.DROP_AND_OPEN &&
                         this.channelProxies[i].TaskToPerform != DssChannelTask.DROP_AND_CLOSE)
                {
                    /// No task to perform on the channel just send the setup step request.
                    if (this.channels[i].CurrentState == this.channels[i].Engaged)
                    {
                        SetupStep currStep = this.hostRoot.GetStep(i);
                        if (currStep.State == SetupStepState.READY || currStep.State == SetupStepState.ERROR)
                        {
                            currStep.Reset();
                        }
                        DssChannelState[] chStates = new DssChannelState[this.channels.Length];
                        for (int j = 0; j < channels.Length; j++)
                        {
                            if (this.channelProxies[j].TaskToPerform == DssChannelTask.OPEN ||
                                this.channelProxies[j].TaskToPerform == DssChannelTask.DROP_AND_OPEN)
                            {
                                chStates[j] = DssChannelState.CHANNEL_OPENED;
                            }
                            else if (this.channelProxies[j].TaskToPerform == DssChannelTask.CLOSE ||
                                     this.channelProxies[j].TaskToPerform == DssChannelTask.DROP_AND_CLOSE)
                            {
                                chStates[j] = DssChannelState.CHANNEL_CLOSED;
                            }
                            else
                            {
                                if (this.channels[j].CurrentState == this.channels[j].Opened)
                                {
                                    chStates[j] = DssChannelState.CHANNEL_OPENED;
                                }
                                else if (this.channels[j].CurrentState == this.channels[j].Closed)
                                {
                                    chStates[j] = DssChannelState.CHANNEL_CLOSED;
                                }
                                else if (this.channels[j].CurrentState == this.channels[j].Engaged)
                                {
                                    chStates[j] = DssChannelState.GUEST_CONNECTED;
                                }
                                else
                                {
                                    throw new DssException("Unexpected channel state!");
                                }
                            }
                        }

                        int[] leftList = null;
                        int[] lostList = null;
                        this.hostRoot.GetGuestEvents(out leftList, out lostList);
                        RCPackage[] packagesToSend = currStep.CreateRequest(this.channelProxies[i].OutgoingPackages != null ?
                                                                                    this.channelProxies[i].OutgoingPackages :
                                                                                    new RCPackage[0] { },
                                                                            leftList,
                                                                            lostList,
                                                                            chStates);
                        foreach (RCPackage pckg in packagesToSend)
                        {
                            this.hostRoot.Lobby.SendControlPackage(pckg, i + 1);
                        }
                        this.sessions[i].SetupStepRequestSent();
                    }
                }
            } /// end-for (int i = 0; i < this.channelProxies.Length; i++)
        }

        /// <summary>
        /// Sends a DSS_LEAVE message to all active sessions and exits from the event loop.
        /// </summary>
        private void LeaveDss()
        {
            for (int i = 0; i < this.sessions.Length; ++i)
            {
                this.sessions[i].SendLeaveMsg("Host left the DSS!", new byte[0] { });
            }

            this.hostRoot.EventQueue.ExitEventLoop();
        }

        /// <summary>
        /// Starts the simulation.
        /// </summary>
        private void StartSimulation()
        {
            /// Collect opFlags and channel state informations.
            byte[] chStates = new byte[this.channelProxies.Length];
            bool[] opFlags = new bool[this.hostRoot.OpCount];
            opFlags[0] = true;

            for (int i = 0; i < chStates.Length; i++)
            {
                if (this.channels[i].CurrentState == this.channels[i].Opened)
                {
                    /// If the channel is opened then it will be closed.
                    //this.channels[i].CloseChannel();
                    this.channels[i].PermanentlyClose();
                    chStates[i] = (byte)DssChannelState.CHANNEL_CLOSED;//CHANNEL_OPENED;
                    opFlags[i + 1] = false;
                }
                else if (this.channels[i].CurrentState == this.channels[i].Closed)
                {
                    /// If the channel is closed then it will remain closed.
                    this.channels[i].PermanentlyClose();
                    chStates[i] = (byte)DssChannelState.CHANNEL_CLOSED;
                    opFlags[i + 1] = false;
                }
                else if (this.channels[i].CurrentState == this.channels[i].Engaged)
                {
                    /// If the channel is engaged then we have to check whether the guest will be dropped or not.
                    if (this.channelProxies[i].TaskToPerform == DssChannelTask.DROP_AND_OPEN ||
                        this.channelProxies[i].TaskToPerform == DssChannelTask.DROP_AND_CLOSE)
                    {
                        /// Close channel permanently.
                        this.channels[i].PermanentlyClose();
                        this.hostRoot.GuestLeftDss(i);
                        chStates[i] = (byte)DssChannelState.CHANNEL_CLOSED;
                        opFlags[i + 1] = false;
                    }
                    else
                    {
                        /// Don't drop guest, it will participate in the simulation stage.
                        chStates[i] = (byte)DssChannelState.GUEST_CONNECTED;
                        opFlags[i + 1] = true;
                    }
                }
                else
                {
                    throw new DssException("Unexpected channel state!");
                }
            }
            int[] leftList = null;
            int[] lostList = null;
            this.hostRoot.GetGuestEvents(out leftList, out lostList);

            RCPackage startSimMsg = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_START_SIMULATION);
            startSimMsg.WriteIntArray(0, leftList);
            startSimMsg.WriteIntArray(1, lostList);
            startSimMsg.WriteByteArray(2, chStates);

            /// Send the DSS_CTRL_START_SIMULATION message to every engaged channels
            for (int i = 0; i < this.sessions.Length; i++)
            {
                if (opFlags[i + 1])
                {
                    /// Push the corresponding session and channel to the Simulating state
                    this.channels[i].StartSimulation();
                    this.sessions[i].StartSimulation();

                    /// Send the start message to the corresponding guest
                    this.hostRoot.Lobby.SendControlPackage(startSimMsg, i + 1);
                }
            }

            /// Reset the simulation manager and ask it to execute the next frame immediately.
            this.hostRoot.SimulationMgr.Reset(opFlags);
            this.hostRoot.SimulationMgr.SetNextFrameExecutionTime(DssRoot.Time);

            /// And finally go to the SimulationStage.
            this.SendingSetupStepRQs_SimulationStage.Fire();
        }

        /// <summary>
        /// Internal function to reset the SetupStep objects of all channels.
        /// </summary>
        private void ResetSteps()
        {
            for (int i = 0; i < this.sessions.Length; i++)
            {
                SetupStep currStep = this.hostRoot.GetStep(i);
                if (currStep.State == SetupStepState.READY || currStep.State == SetupStepState.ERROR)
                {
                    currStep.Reset();
                }
            }
        }

        /// <summary>
        /// Internal function to start the setup step timer.
        /// </summary>
        private void StartSetupStepTimer(ISMState state)
        {
            if (state == this.SetupStepTimerRunning)
            {
                this.hostRoot.SetupStepClock = this.hostRoot.AlarmClkMgr.SetAlarmClock(
                                                    DssRoot.Time + DssConstants.SETUP_STEP_CYCLE_TIME,
                                                    this.SetupStepTimeout);
            }
            else
            {
                throw new DssException("Illegal call to DssManagerSM.StartSetupStepTimer");
            }
        }

        /// <summary>
        /// This function is called when the SETUP_STEP_ANSWER_TIMEOUT clock has to be started.
        /// </summary>
        private void StartSetupStepAwTimer(ISMState targetState, ISMState sourceState)
        {
            if (targetState == this.WaitingSetupStepAWs && sourceState == this.SendingSetupStepRQs)
            {
                this.hostRoot.SetupStepAwTimeoutClock = this.hostRoot.AlarmClkMgr.SetAlarmClock(
                    DssRoot.Time + DssConstants.SETUP_STEP_ANSWER_TIMEOUT,
                    this.SetupStepAwTimeout);
            }
            else
            {
                throw new DssException("Illegal call to DssManagerSM.StartSetupStepAwTimer");
            }
        }

        /// <summary>
        /// This function is called when the SETUP_STEP_ANSWER_TIMEOUT clock has to be stopped.
        /// </summary>
        private void StopSetupStepAwTimer(ISMState targetState, ISMState sourceState)
        {
            if (targetState == this.SendingSetupStepRQs && sourceState == this.WaitingSetupStepAWs)
            {
                if (this.hostRoot.SetupStepAwTimeoutClock != null)
                {
                    this.hostRoot.SetupStepAwTimeoutClock.Cancel();
                    this.hostRoot.SetupStepAwTimeoutClock = null;
                }
            }
            else
            {
                throw new DssException("Illegal call to DssManagerSM.StopSetupStepAwTimer");
            }
        }

        /// <summary>
        /// List of the channel controller state machines.
        /// </summary>
        private DssChannelSM[] channels;

        /// <summary>
        /// List of the session controller state machines.
        /// </summary>
        private DssHostSessionSM[] sessions;

        /// <summary>
        /// Becomes true when the external triggers have been created.
        /// </summary>
        private bool externalTriggersCreated;

        /// <summary>
        /// Becomes true when the internal triggers have been created.
        /// </summary>
        private bool internalTriggersCreated;

        /// <summary>
        /// The SMC that controls the whole DSS at host-side.
        /// </summary>
        private StateMachineController controller;

        /// <summary>
        /// Root of the data model at host-side.
        /// </summary>
        private DssHostRoot hostRoot;

        /// <summary>
        /// Interface to the channels that will be provided to the client module during the setup stage.
        /// </summary>
        private DssChannelProxy[] channelProxies;

        /// <summary>
        /// Interface to the state machine of the DSS-manager.
        /// </summary>
        private IStateMachine managerSM;

        /// <summary>
        /// Interface to the state machine of the setup step timer.
        /// </summary>
        private IStateMachine setupStepTimerSM;

        /// <summary>
        /// Interface to the state machine of the channel stability monitor.
        /// </summary>
        private IStateMachine channelStabilityMonitorSM;

        /// <summary>
        /// The states of the state machine of the DSS-manager.
        /// </summary>
        /// <remarks>
        /// See \docs\model\dss_services\dss_impl.eap for more informations.
        /// </remarks>
        public ISMState Start;
        public ISMState LobbyCreated;
        public ISMState SendingSetupStepRQs;
        public ISMState SimulationStage;
        public ISMState WaitingSetupStepAWs;

        /// <summary>
        /// The states of the state machine of the setup step timer.
        /// </summary>
        /// <remarks>
        /// See \docs\model\dss_services\dss_impl.eap for more informations.
        /// </remarks>
        public ISMState SetupStepTimerInactive;
        public ISMState SetupStepTimerRunning;

        /// <summary>
        /// The states of the state machine of the channel stability monitor.
        /// </summary>
        /// <remarks>
        /// See \docs\model\dss_services\dss_impl.eap for more informations.
        /// </remarks>
        public ISMState PermanentChannelStates;
        public ISMState TransientChannelStates;

        /// <summary>
        /// Interface to the external triggers of the DSS-channel.
        /// </summary>
        private ISMTrigger Start_LobbyCreated;
        private ISMTrigger SendingSetupStepRQs_SimulationStage;
        private ISMTrigger SendingSetupStepRQs_WaitingSetupStepAWs;

        /// <summary>
        /// Interface to the external triggers of the setup step timer.
        /// </summary>
        private ISMTrigger SetupStepTimerInactive_Running;
        private ISMTrigger SetupStepTimerRunning_Inactive;
    }
}
