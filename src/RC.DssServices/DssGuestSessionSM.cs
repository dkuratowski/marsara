using System;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.SMC;

namespace RC.DssServices
{
    /// <summary>
    /// This class is a wrapper over the state machine of a guest-side DSS-session.
    /// </summary>
    class DssGuestSessionSM
    {
        /// <summary>
        /// Constructs a DssGuestSessionSM object.
        /// </summary>
        /// <param name="smc">The SM-controller that is used to construct the underlying state machine.</param>
        public DssGuestSessionSM(DssGuestRoot guestRoot)
        {
            ///this.indexOfThisGuest = -1;     /// We don't know the index currently.
            this.guestRoot = guestRoot;
            this.channelProxy = new DssChannelProxy(DssMode.GUEST_SIDE);
            this.opFlagsTmp = null;

            /// Creating the state machine
            this.controller = new StateMachineController();
            this.sm = this.controller.AddStateMachine("Session");

            /// Creating the states
            this.Start = this.sm.AddState("Start", null);
            this.WaitingConnectionACK = this.sm.AddState("WaitingConnectionACK", null);
            this.WaitingSetupStepRQ = this.sm.AddState("WaitingSetupStepRQ", null);
            this.SendingSetupStepAW = this.sm.AddState("SendingSetupStepAW", this.CallClientModuleSetupIface);
            this.Simulating = this.sm.AddState("Simulating", this.BeginSimulationStage);

            /// Setting the initial state
            this.sm.SetInitialState(this.Start);

            /// Creating the external triggers
            this.Start_WaitingConnectionACK = this.Start.AddExternalTrigger(this.WaitingConnectionACK, this.StartConnAckTimeout);
            this.WaitingConnectionACK_WaitingSetupStepRQ = this.WaitingConnectionACK.AddExternalTrigger(this.WaitingSetupStepRQ,
                                                                                                        this.StopConnAckTimeoutStartSetupStepReqTimeout);
            this.WaitingSetupStepRQ_SendingSetupStepAW = this.WaitingSetupStepRQ.AddExternalTrigger(this.SendingSetupStepAW, this.StopSetupStepReqTimeout);
            this.WaitingSetupStepRQ_Simulating = this.WaitingSetupStepRQ.AddExternalTrigger(this.Simulating, this.StopSetupStepReqTimeout);
            this.SendingSetupStepAW_WaitingSetupStepRQ = this.SendingSetupStepAW.AddExternalTrigger(this.WaitingSetupStepRQ, this.StartSetupStepReqTimeout);

            this.controller.CommissionController();
        }

        #region Trigger methods

        /// <summary>
        /// Called when we have successfully connected to the lobby.
        /// </summary>
        public void LobbyIsRunning()
        {
            /// We send a DSS_CTRL_CONN_REQUEST package.
            RCPackage reqPackage = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_CONN_REQUEST);
            reqPackage.WriteInt(0, DssRoot.RC_DSSSERVICES_VERSION.Major);
            reqPackage.WriteInt(1, DssRoot.RC_DSSSERVICES_VERSION.Minor);
            reqPackage.WriteInt(2, DssRoot.RC_DSSSERVICES_VERSION.Build);
            reqPackage.WriteInt(3, DssRoot.RC_DSSSERVICES_VERSION.Revision);
            this.guestRoot.Lobby.SendControlPackage(reqPackage);

            /// Then we fire the corresponding trigger at the session state machine.
            this.Start_WaitingConnectionACK.Fire();
            this.controller.ExecuteFirings();
        }

        /// <summary>
        /// Called when an error occured on guest-side during setup stage (for example: lost connection with the host).
        /// </summary>
        public void SetupStageError()
        {
            if (this.sm.CurrentState == this.WaitingConnectionACK ||
                this.sm.CurrentState == this.SendingSetupStepAW ||
                this.sm.CurrentState == this.WaitingSetupStepRQ ||
                this.sm.CurrentState == this.Start)
            {
                /// Notify the client module, stop the timeout clocks and exit from the event-loop.
                this.guestRoot.SetupIface.HostConnectionLost();
                StopTimeouts();
                this.guestRoot.EventQueue.ExitEventLoop();
            }
            else
            {
                throw new DssException("Illegal call to DssGuestSessionSM.SetupStageError!");
            }
        }

        /// <summary>
        /// Called when a control packages arrived from the host during setup stage.
        /// </summary>
        /// <param name="package">The arrived control package.</param>
        public void SetupStageCtrlPackage(RCPackage package)
        {
            if (this.sm.CurrentState == this.WaitingConnectionACK)
            {
                if (package.PackageFormat.ID == DssRoot.DSS_CTRL_CONN_ACK)
                {
                    int otherMajor = package.ReadInt(0);
                    int otherMinor = package.ReadInt(1);
                    int otherBuild = package.ReadInt(2);
                    int otherRevision = package.ReadInt(3);
                    if (otherMajor >= 0 && otherMinor >= 0 && otherBuild >= 0 && otherRevision >= 0)
                    {
                        Version otherVer = new Version(otherMajor, otherMinor, otherBuild, otherRevision);
                        if (DssRoot.IsCompatibleVersion(otherVer))
                        {
                            this.WaitingConnectionACK_WaitingSetupStepRQ.Fire();
                            this.controller.ExecuteFirings();
                            return;
                        }
                        else
                        {
                            TraceManager.WriteAllTrace(string.Format("Incompatible with host version: {0} (RC.DssServices)", otherVer), DssTraceFilters.SETUP_STAGE_ERROR);
                            SetupStageError();
                            return;
                        }
                    }
                    else
                    {
                        TraceManager.WriteAllTrace("Unable to parse version information!", DssTraceFilters.SETUP_STAGE_ERROR);
                        SetupStageError();
                        return;
                    }
                } /// end-if (package.PackageFormat.ID == DssRoot.DSS_CTRL_CONN_ACK)
                else if (package.PackageFormat.ID == DssRoot.DSS_CTRL_CONN_REJECT)
                {
                    TraceManager.WriteAllTrace(string.Format("Connection request rejected by the host! Reason: {0}", package.ReadString(0)), DssTraceFilters.SETUP_STAGE_ERROR);
                    SetupStageError();
                    return;
                }
                else
                {
                    TraceManager.WriteAllTrace("Unexpected answer from host to connection request!", DssTraceFilters.SETUP_STAGE_ERROR);
                    SetupStageError();
                    return;
                }
            }
            else if (this.sm.CurrentState == this.WaitingSetupStepRQ)
            {
                if (package.PackageFormat.ID == DssRoot.DSS_LEAVE)
                {
                    /// The host left the DSS
                    this.guestRoot.SetupIface.HostLeftDss();
                    StopTimeouts();
                    this.guestRoot.EventQueue.ExitEventLoop();
                    return;
                } /// end-if (package.PackageFormat.ID == DssRoot.DSS_LEAVE)
                else if (package.PackageFormat.ID == DssRoot.DSS_CTRL_DROP_GUEST)
                {
                    /// The current guest has been dropped out of the DSS by the host.
                    this.guestRoot.SetupIface.DroppedByHost();
                    StopTimeouts();
                    this.guestRoot.EventQueue.ExitEventLoop();
                    return;
                } /// end-if (package.PackageFormat.ID == DssRoot.DSS_CTRL_DROP_GUEST)
                else if (package.PackageFormat.ID == DssRoot.DSS_CTRL_START_SIMULATION)
                {
                    int[] leftList = null;
                    int[] lostList = null;
                    bool[] opFlags = null;
                    if (ParseStartSimPackage(package, out leftList, out lostList, out opFlags))
                    {
                        StopTimeouts();
                        CallNotificationMethods(leftList, lostList);
                        this.guestRoot.SetupIface.SimulationStarted();
                        this.opFlagsTmp = opFlags;
                        this.WaitingSetupStepRQ_Simulating.Fire();
                        this.controller.ExecuteFirings();
                        return;
                    }
                    else
                    {
                        SetupStageError();
                        return;
                    }
                } /// end-if (package.PackageFormat.ID == DssRoot.DSS_CTRL_START_SIMULATION)
                else
                {
                    this.guestRoot.Step.IncomingPackage(package);
                    if (this.guestRoot.Step.State == SetupStepState.READY)
                    {
                        /// Setup step request arrived.
                        this.WaitingSetupStepRQ_SendingSetupStepAW.Fire();
                        this.controller.ExecuteFirings();
                        return;
                    }
                    else if (this.guestRoot.Step.State == SetupStepState.ERROR)
                    {
                        /// Setup step request error.
                        SetupStageError();
                        return;
                    }
                    else
                    {
                        /// Setup step request not finished yet, more packages to wait.
                        return;
                    }
                }
            } /// end-if (this.sm.CurrentState == this.WaitingSetupStepRQ)
            else
            {
                /// Go to error state if the package cannot be handled until now.
                SetupStageError();
                return;
            }
        }

        #endregion Trigger methods

        #region Timeout methods

        /// <summary>
        /// This function is called when a CONNECTION_ACKNOWLEDGE_TIMEOUT event occurs.
        /// </summary>
        private void ConnectionAckTimeout(AlarmClock whichClock, object param)
        {
            if (DssConstants.TIMEOUTS_NOT_IGNORED)
            {
                if (whichClock == this.guestRoot.ConnAckTimeoutClock)
                {
                    TraceManager.WriteAllTrace("CONNECTION_ACKNOWLEDGE_TIMEOUT", DssTraceFilters.SETUP_STAGE_ERROR);
                    SetupStageError();
                }
                else
                {
                    throw new DssException("Illegal call to DssGuestSessionSM.ConnectionAckTimeout!");
                }
            }
        }

        /// <summary>
        /// This function is called when a SETUP_STEP_REQUEST_TIMEOUT event occurs.
        /// </summary>
        private void SetupStepReqTimeout(AlarmClock whichClock, object param)
        {
            if (DssConstants.TIMEOUTS_NOT_IGNORED)
            {
                if (whichClock == this.guestRoot.SetupStepReqTimeoutClock)
                {
                    TraceManager.WriteAllTrace("SETUP_STEP_REQUEST_TIMEOUT", DssTraceFilters.SETUP_STAGE_ERROR);
                    SetupStageError();
                }
                else
                {
                    throw new DssException("Illegal call to DssGuestSessionSM.SetupStepReqTimeout!");
                }
            }
        }

        #endregion Timeout methods

        /// <summary>
        /// Gets the current state of this session.
        /// </summary>
        public ISMState CurrentState { get { return this.sm.CurrentState; } }

        /// <summary>
        /// This function is called by the SMC framework when the guest session has to call the setup interface
        /// of the client module.
        /// </summary>
        /// <param name="state"></param>
        private void CallClientModuleSetupIface(ISMState state)
        {
            if (state == this.SendingSetupStepAW)
            {
                TraceManager.WriteAllTrace("Calling IDssGuestSetup.ExecuteNextStep on client module.", DssTraceFilters.SETUP_STAGE_INFO);

                CallNotificationMethods(this.guestRoot.Step.GuestsLeftDss, this.guestRoot.Step.GuestsLost);

                this.channelProxy.UnlockForClient(this.guestRoot.Step.StepPackageList,
                                                  this.guestRoot.Step.ChannelStateList,
                                                  this.guestRoot.IndexOfThisGuest);

                bool result = this.guestRoot.SetupIface.ExecuteNextStep(this.channelProxy);
                this.channelProxy.Lock();

                if (result)
                {
                    ContinueSetup();
                }
                else
                {
                    LeaveDss();
                }
            }
            else
            {
                throw new DssException("Unexpected state!");
            }
        }

        /// <summary>
        /// If a guest left the DSS or the host has lost connection with it, then we have to notify the client
        /// module about these events through the IDssGuestSetup interface just before the channel proxy
        /// is unlocked.
        /// </summary>
        private void CallNotificationMethods(int[] leftList, int[] lostList)
        {
            for (int i = 0; i < leftList.Length; i++)
            {
                this.guestRoot.SetupIface.GuestLeftDss(leftList[i]);
            }

            for (int i = 0; i < lostList.Length; i++)
            {
                this.guestRoot.SetupIface.GuestConnectionLost(lostList[i]);
            }
        }

        /// <summary>
        /// Sends the setup step answer to the host.
        /// </summary>
        private void ContinueSetup()
        {
            /// Send the answer packages.
            SetupStep currStep = this.guestRoot.Step;
            RCPackage[] packagesToSend = currStep.CreateAnswer(this.channelProxy.OutgoingPackages != null ?
                                                                       this.channelProxy.OutgoingPackages :
                                                                       new RCPackage[0] { });
            currStep.Reset();
            foreach (RCPackage pckg in packagesToSend)
            {
                this.guestRoot.Lobby.SendControlPackage(pckg);
            }

            /// Trigger the session state machine.
            this.SendingSetupStepAW_WaitingSetupStepRQ.Fire();
        }

        /// <summary>
        /// Leaves the DSS.
        /// </summary>
        private void LeaveDss()
        {
            /// Send the DSS_LEAVE message to the host.
            RCPackage leavePackage = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_LEAVE);
            leavePackage.WriteString(0, "Guest left DSS!");
            leavePackage.WriteByteArray(1, new byte[0] { });
            this.guestRoot.Lobby.SendControlPackage(leavePackage);

            /// Stop the timeout clocks and exit from the event-loop.
            StopTimeouts();
            this.guestRoot.EventQueue.ExitEventLoop();
        }

        /// <summary>
        /// Internal function to initialize the simulation manager.
        /// </summary>
        /// <remarks>
        /// Called when the session-SM goes to the Simulating stage.
        /// </remarks>
        private void BeginSimulationStage(ISMState state)
        {
            if (state == this.Simulating)
            {
                /// Ask the root to execute the next frame immediately.
                this.guestRoot.SimulationMgr.Reset(this.opFlagsTmp);
                this.guestRoot.SimulationMgr.SetNextFrameExecutionTime(DssRoot.Time);
            }
            else
            {
                throw new DssException("Illegal call to DssGuestSessionSM.BeginSimulationStage!");
            }
        }

        /// <summary>
        /// This function is called when the CONNECTION_ACKNOWLEDGE_TIMEOUT clock has to be started.
        /// </summary>
        private void StartConnAckTimeout(ISMState targetState, ISMState sourceState)
        {
            if (targetState == this.WaitingConnectionACK && sourceState == this.Start)
            {
                this.guestRoot.ConnAckTimeoutClock = this.guestRoot.AlarmClkMgr.SetAlarmClock(
                    DssRoot.Time + DssConstants.CONNECTION_ACKNOWLEDGE_TIMEOUT,
                    this.ConnectionAckTimeout);
            }
            else
            {
                throw new DssException("Illegal call to DssGuestSessionSM.StartConnAckTimeout!");
            }
        }

        /// <summary>
        /// This function is called when the CONNECTION_ACKNOWLEDGE_TIMEOUT clock has to be stopped and
        /// the SETUP_STEP_REQUEST_TIMEOUT clock has to be started.
        /// </summary>
        private void StopConnAckTimeoutStartSetupStepReqTimeout(ISMState targetState, ISMState sourceState)
        {
            if (targetState == this.WaitingSetupStepRQ && sourceState == this.WaitingConnectionACK)
            {
                if (this.guestRoot.ConnAckTimeoutClock != null)
                {
                    this.guestRoot.ConnAckTimeoutClock.Cancel();
                    this.guestRoot.ConnAckTimeoutClock = null;
                }

                this.guestRoot.SetupStepReqTimeoutClock = this.guestRoot.AlarmClkMgr.SetAlarmClock(
                    DssRoot.Time + DssConstants.SETUP_STEP_REQUEST_TIMEOUT,
                    this.SetupStepReqTimeout);
            }
            else
            {
                throw new DssException("Illegal call to DssGuestSessionSM.StopConnAckTimeoutStartSetupStepReqTimeout!");
            }
        }

        /// <summary>
        /// This function is called when the SETUP_STEP_REQUEST_TIMEOUT clock has to be started.
        /// </summary>
        private void StartSetupStepReqTimeout(ISMState targetState, ISMState sourceState)
        {
            if (targetState == this.WaitingSetupStepRQ && sourceState == this.SendingSetupStepAW)
            {
                this.guestRoot.SetupStepReqTimeoutClock = this.guestRoot.AlarmClkMgr.SetAlarmClock(
                    DssRoot.Time + DssConstants.SETUP_STEP_REQUEST_TIMEOUT,
                    this.SetupStepReqTimeout);
            }
            else
            {
                throw new DssException("Illegal call to DssGuestSessionSM.StartSetupStepReqTimeout!");
            }
        }

        /// <summary>
        /// This function is called when the SETUP_STEP_REQUEST_TIMEOUT clock has to be stopped.
        /// </summary>
        private void StopSetupStepReqTimeout(ISMState targetState, ISMState sourceState)
        {
            if (sourceState == this.WaitingSetupStepRQ && (targetState == this.SendingSetupStepAW || targetState == this.Simulating))
            {
                if (this.guestRoot.SetupStepReqTimeoutClock != null)
                {
                    this.guestRoot.SetupStepReqTimeoutClock.Cancel();
                    this.guestRoot.SetupStepReqTimeoutClock = null;
                }
            }
            else
            {
                throw new DssException("Illegal call to DssGuestSessionSM.StartSetupStepReqTimeout!");
            }
        }

        /// <summary>
        /// Stops every setup stage timeout clocks.
        /// </summary>
        private void StopTimeouts()
        {
            if (this.guestRoot.ConnAckTimeoutClock != null)
            {
                this.guestRoot.ConnAckTimeoutClock.Cancel();
                this.guestRoot.ConnAckTimeoutClock = null;
            }

            if (this.guestRoot.SetupStepReqTimeoutClock != null)
            {
                this.guestRoot.SetupStepReqTimeoutClock.Cancel();
                this.guestRoot.SetupStepReqTimeoutClock = null;
            }
        }

        /// <summary>
        /// Internal function for parsing a DSS_CTRL_START_SIMULATION package.
        /// </summary>
        /// <param name="package">The package to parse.</param>
        /// <returns>True if the package has been successfully parsed, false otherwise.</returns>
        private bool ParseStartSimPackage(RCPackage package, out int[] leftList, out int[] lostList, out bool[] opFlags)
        {
            /// Read the left-list
            leftList = package.ReadIntArray(0);
            for (int i = 0; i < leftList.Length; ++i)
            {
                if (leftList[i] < 0)
                {
                    leftList = null;
                    lostList = null;
                    opFlags = null;
                    return false;
                }
            }

            /// Read the lost-list
            lostList = package.ReadIntArray(1);
            for (int i = 0; i < lostList.Length; ++i)
            {
                if (lostList[i] < 0)
                {
                    leftList = null;
                    lostList = null;
                    opFlags = null;
                    return false;
                }
            }

            /// Read the channel-state-list
            byte[] chStateBytes = package.ReadByteArray(2);
            if (chStateBytes.Length == this.guestRoot.OpCount - 1)
            {
                opFlags = new bool[this.guestRoot.OpCount];
                opFlags[0] = true;
                for (int i = 0; i < chStateBytes.Length; ++i)
                {
                    if (chStateBytes[i] == (byte)DssChannelState.CHANNEL_CLOSED ||
                        chStateBytes[i] == (byte)DssChannelState.CHANNEL_OPENED ||
                        chStateBytes[i] == (byte)DssChannelState.GUEST_CONNECTED)
                    {
                        opFlags[i + 1] = (chStateBytes[i] == (byte)DssChannelState.GUEST_CONNECTED);
                    }
                    else
                    {
                        /// Error
                        leftList = null;
                        lostList = null;
                        opFlags = null;
                        return false;
                    }
                }
            } /// end-if (chStateBytes != null && chStateBytes.Length != this.guestRoot.OpCount - 1)
            else
            {
                /// Error
                leftList = null;
                lostList = null;
                opFlags = null;
                return false;
            }

            /// Everything is OK, the start simulation message arrived successfully.
            return true;
        }

        /// <summary>
        /// Reference to the SMC that controls the guest-side session.
        /// </summary>
        private StateMachineController controller;

        /// <summary>
        /// Interface to the state machine of the DSS-session.
        /// </summary>
        private IStateMachine sm;

        /// <summary>
        /// Temporary operator flag array that is used during simulation start. See DssSimulationMgr.operatorFlags for
        /// more informations.
        /// </summary>
        private bool[] opFlagsTmp;

        /// <summary>
        /// Interface to the channel that will be provided to the client module during the setup stage.
        /// </summary>
        private DssChannelProxy channelProxy;

        /// <summary>
        /// Root of the data model at guest-side.
        /// </summary>
        private DssGuestRoot guestRoot;

        /// <summary>
        /// Interface to the external triggers of the DSS-channel.
        /// </summary>
        private ISMTrigger Start_WaitingConnectionACK;
        private ISMTrigger WaitingConnectionACK_WaitingSetupStepRQ;
        private ISMTrigger WaitingSetupStepRQ_SendingSetupStepAW;
        private ISMTrigger WaitingSetupStepRQ_Simulating;
        private ISMTrigger SendingSetupStepAW_WaitingSetupStepRQ;

        /// <summary>
        /// The states of the state machine of the DSS-session.
        /// </summary>
        /// <remarks>
        /// See \docs\model\dss_services\dss_impl.eap for more informations.
        /// </remarks>
        public ISMState Start;
        public ISMState WaitingConnectionACK;
        public ISMState WaitingSetupStepRQ;
        public ISMState SendingSetupStepAW;
        public ISMState Simulating;
    }
}
