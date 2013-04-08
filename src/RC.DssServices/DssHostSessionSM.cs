using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.SMC;

namespace RC.DssServices
{
    /// <summary>
    /// This class is a wrapper over the state machine of a host-side DSS-session.
    /// </summary>
    class DssHostSessionSM
    {
        /// <summary>
        /// Constructs a DssHostSessionSM object.
        /// </summary>
        public DssHostSessionSM(IStateMachine sm, DssManagerSM manager)//, SetupStep step)
        {
            this.sm = sm;
            this.channel = null;                    /// will be set later
            this.manager = manager;
            this.externalTriggersCreated = false;
            this.internalTriggersCreated = false;

            /// Creating the states
            this.Inactive = this.sm.AddState("Inactive", null);
            this.WaitingConnectionRQ = this.sm.AddState("WaitingConnectionRQ", null);
            this.SendingSetupStepRQ = this.sm.AddState("SendingSetupStepRQ", null);
            this.WaitingSetupStepAW = this.sm.AddState("WaitingSetupStepAW", null);
            this.Simulating = this.sm.AddState("Simulating", null);

            /// Setting the initial state
            this.sm.SetInitialState(this.Inactive);
        }

        /// <summary>
        /// Creates the external triggers of the underlying state machine.
        /// </summary>
        public void CreateExternalTriggers()
        {
            if (!this.externalTriggersCreated)
            {
                this.WaitingConnectionRQ_SendingSetupStepRQ = this.WaitingConnectionRQ.AddExternalTrigger(this.SendingSetupStepRQ, null);
                this.WaitingConnectionRQ_Inactive = this.WaitingConnectionRQ.AddExternalTrigger(this.Inactive, null);
                this.SendingSetupStepRQ_WaitingSetupStepAW = this.SendingSetupStepRQ.AddExternalTrigger(this.WaitingSetupStepAW, null);
                this.SendingSetupStepRQ_Simulating = this.SendingSetupStepRQ.AddExternalTrigger(this.Simulating, null);
                this.SendingSetupStepRQ_Inactive = this.SendingSetupStepRQ.AddExternalTrigger(this.Inactive, null);
                this.WaitingSetupStepAW_SendingSetupStepRQ = this.WaitingSetupStepAW.AddExternalTrigger(this.SendingSetupStepRQ, null);
                this.WaitingSetupStepAW_Inactive = this.WaitingSetupStepAW.AddExternalTrigger(this.Inactive, null);

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
                if (this.channel != null)
                {
                    this.Inactive.AddInternalTrigger(this.WaitingConnectionRQ,
                                                   null,
                                                   new HashSet<ISMState>(new ISMState[1] { this.channel.Engaging }));

                    this.internalTriggersCreated = true;
                }
                else
                {
                    throw new DssException("Corresponding channel was not set!");
                }
            }
            else
            {
                throw new DssException("Internal triggers have already been created!");
            }
        }

        #region Trigger methods

        /// <summary>
        /// This function is called when an error occurs on this session during setup stage.
        /// </summary>
        public void SetupStageError()
        {
            if (this.sm.CurrentState == this.WaitingConnectionRQ)
            {
                this.WaitingConnectionRQ_Inactive.Fire();
            }
            else if (this.sm.CurrentState == this.SendingSetupStepRQ)
            {
                this.SendingSetupStepRQ_Inactive.Fire();
            }
            else if (this.sm.CurrentState == this.WaitingSetupStepAW)
            {
                this.WaitingSetupStepAW_Inactive.Fire();
            }
            else if (this.sm.CurrentState == this.Inactive)
            {
                /// Ignore
            }
            else
            {
                throw new DssException("Illegal call to SetupStageError at a session");
            }
        }

        /// <summary>
        /// This function is called when the client module decided to start the simulation stage.
        /// </summary>
        public void StartSimulation()
        {
            if (this.sm.CurrentState == this.SendingSetupStepRQ)
            {
                this.SendingSetupStepRQ_Simulating.Fire();
            }
            else
            {
                throw new DssException("Illegal call to StartSimulation at a session");
            }
        }

        /// <summary>
        /// This function is called when a control package arrives at setup stage.
        /// </summary>
        /// <param name="package">The arrived control package.</param>
        public void SetupStageCtrlPackage(RCPackage package)
        {
            bool error = false;

            if (this.sm.CurrentState == this.WaitingConnectionRQ)
            {
                if (package.PackageFormat.ID == DssRoot.DSS_CTRL_CONN_REQUEST)
                {
                    RCPackage rejPackage = null;
                    int otherMajor = package.ReadInt(0);
                    int otherMinor = package.ReadInt(1);
                    int otherBuild = package.ReadInt(2);
                    int otherRevision = package.ReadInt(3);
                    if (otherMajor >= 0 && otherMinor >= 0 && otherBuild >= 0 && otherRevision >= 0)
                    {
                        Version otherVer = new Version(otherMajor, otherMinor, otherBuild, otherRevision);
                        if (DssRoot.IsCompatibleVersion(otherVer))
                        {
                            /// We send back a DSS_CTRL_CONN_ACK package.
                            RCPackage ackPackage = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_CONN_ACK);
                            ackPackage.WriteInt(0, DssRoot.APPLICATION_VERSION.Major);
                            ackPackage.WriteInt(1, DssRoot.APPLICATION_VERSION.Minor);
                            ackPackage.WriteInt(2, DssRoot.APPLICATION_VERSION.Build);
                            ackPackage.WriteInt(3, DssRoot.APPLICATION_VERSION.Revision);

                            this.manager.HostRoot.Lobby.SendControlPackage(ackPackage, this.channel.Index + 1);
                            this.WaitingConnectionRQ_SendingSetupStepRQ.Fire();
                        }
                        else
                        {
                            /// We send back a DSS_CTRL_CONN_REJECT package
                            rejPackage = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_CONN_REJECT);
                            string reason = string.Format("Incompatible with host version: {0} (RC.DssServices)",
                                                          DssRoot.APPLICATION_VERSION.ToString());
                            rejPackage.WriteString(0, reason);
                            rejPackage.WriteByteArray(1, new byte[0]);
                        }
                    }
                    else
                    {
                        /// We create a DSS_CTRL_CONN_REJECT package
                        rejPackage = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_CONN_REJECT);
                        rejPackage.WriteString(0, "Unable to parse version information!");
                        rejPackage.WriteByteArray(1, new byte[0]);
                    }


                    /// We send back a DSS_CTRL_CONN_REJECT package if necessary.
                    if (rejPackage != null && rejPackage.IsCommitted)
                    {
                        this.manager.HostRoot.Lobby.SendControlPackage(rejPackage, this.channel.Index + 1);

                        error = true;
                    }
                } /// end-if (package.PackageFormat.ID == DssRoot.DSS_CTRL_CONN_REQUEST)
            } /// end-if (this.sm.CurrentState == this.WaitingConnectionRQ)
            else if (this.sm.CurrentState == this.WaitingSetupStepAW)
            {
                if (package.PackageFormat.ID == DssRoot.DSS_LEAVE)
                {
                    string leaveReason = package.ReadString(0);
                    string trcMsg = string.Format("Guest-{0} has left the DSS. Reason: {1}",
                                                  this.channel.Index,
                                                  leaveReason.Length != 0 ? leaveReason : "-");
                    TraceManager.WriteAllTrace(trcMsg, DssTraceFilters.SETUP_STAGE_INFO);
                    this.WaitingSetupStepAW_Inactive.Fire();
                    this.channel.GuestLeaveSetupStage();
                } /// end-if (package.PackageFormat.ID == DssRoot.DSS_LEAVE)
                else
                {
                    SetupStep currentStep = this.manager.HostRoot.GetStep(this.channel.Index);
                    currentStep.IncomingPackage(package);
                    if (currentStep.State == SetupStepState.READY)
                    {
                        /// Setup step answer arrived.
                        this.WaitingSetupStepAW_SendingSetupStepRQ.Fire();
                    }
                    else if (currentStep.State == SetupStepState.ERROR)
                    {
                        /// Setup step answer error.
                        error = true;
                    }
                    else
                    {
                        /// Setup step answer not finished yet, more packages to wait.
                    }
                }
            } /// end-if (this.sm.CurrentState == this.WaitingSetupStepAW)

            if (error)
            {
                /// Go to error state if the package cannot be handled until now.
                SetupStageError();
                this.channel.SetupStageError();
                return;
            }
        }

        /// <summary>
        /// Sends a DSS_LEAVE message to the other side of this session.
        /// </summary>
        /// <param name="reason">The reason of leaving the DSS.</param>
        /// <param name="customData">Custom data about leaving the DSS.</param>
        public void SendLeaveMsg(string reason, byte[] customData)
        {
            if (this.sm.CurrentState == this.SendingSetupStepRQ)
            {
                RCPackage leaveMsg = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_LEAVE);
                leaveMsg.WriteString(0, reason);
                leaveMsg.WriteByteArray(1, customData);
                this.manager.HostRoot.Lobby.SendControlPackage(leaveMsg, this.channel.Index + 1);
            }
        }

        #endregion Trigger methods

        /// <summary>
        /// Sets the corresponding channel of this session. This property can be called only once.
        /// </summary>
        public DssChannelSM Channel
        {
            set
            {
                if (this.channel == null)
                {
                    if (value != null) { this.channel = value; } else { throw new ArgumentNullException("Channel"); }
                }
                else
                {
                    throw new DssException("DssHostSessionSM.Channel already set!");
                }
            }
        }

        /// <summary>
        /// Indicates that outgoing packages have been sent by the manager to the guest.
        /// </summary>
        public void SetupStepRequestSent()
        {
            if (this.sm.CurrentState == this.SendingSetupStepRQ)
            {
                this.SendingSetupStepRQ_WaitingSetupStepAW.Fire();
            }
            else
            {
                throw new DssException("Illegal call to DssHostSessionSM.SetupStepRequestSent");
            }
        }

        /// <summary>
        /// Internal function to send a DSS_CTRL_DROP_GUEST message to the other side of this session.
        /// </summary>
        public void DropGuest()
        {
            if (this.sm.CurrentState == this.SendingSetupStepRQ)
            {
                RCPackage dropGuestMsg = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_DROP_GUEST);
                dropGuestMsg.WriteString(0, "Dropped from the DSS by the host.");
                dropGuestMsg.WriteByteArray(1, new byte[0] { });

                /// Send the DSS_CTRL_DROP_GUEST message to the guest and close the line immediately
                this.manager.HostRoot.Lobby.SendControlPackage(dropGuestMsg, this.channel.Index + 1);
                this.manager.HostRoot.Lobby.CloseLine(this.channel.Index + 1);

                /// Move the session back to inactive state.
                this.SendingSetupStepRQ_Inactive.Fire();
            }
            else
            {
                throw new DssException("Illegal call to DssHostSessionSM.DropGuest");
            }
        }

        /// <summary>
        /// Gets the current state of this session.
        /// </summary>
        public ISMState CurrentState { get { return this.sm.CurrentState; } }

        /// <summary>
        /// The manager of the DSS.
        /// </summary>
        private DssManagerSM manager;

        /// <summary>
        /// The corresponding DSS-channel.
        /// </summary>
        private DssChannelSM channel;

        /// <summary>
        /// Interface to the state machine of the DSS-session.
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
        private ISMTrigger WaitingConnectionRQ_SendingSetupStepRQ;
        private ISMTrigger WaitingConnectionRQ_Inactive;
        private ISMTrigger SendingSetupStepRQ_WaitingSetupStepAW;
        private ISMTrigger SendingSetupStepRQ_Inactive;
        private ISMTrigger SendingSetupStepRQ_Simulating;
        private ISMTrigger WaitingSetupStepAW_SendingSetupStepRQ;
        private ISMTrigger WaitingSetupStepAW_Inactive;

        /// <summary>
        /// The states of the state machine of the DSS-session.
        /// </summary>
        /// <remarks>
        /// See \docs\model\dss_services\dss_impl.eap for more informations.
        /// </remarks>
        public ISMState Inactive;
        public ISMState WaitingConnectionRQ;
        public ISMState SendingSetupStepRQ;
        public ISMState WaitingSetupStepAW;
        public ISMState Simulating;
    }
}
