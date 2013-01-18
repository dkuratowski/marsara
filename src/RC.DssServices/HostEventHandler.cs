using System;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// This class responsible for DSS event handling at host side.
    /// </summary>
    class HostEventHandler : DssEventHandler
    {
        /// <summary>
        /// Constructs a HostEventHandler object.
        /// </summary>
        public HostEventHandler(DssHostRoot root)
            : base(root)
        {
            this.root = root;
        }

        /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
        public override void ControlPackageFromClientHdl(int timestamp, Common.RCPackage package, int senderID)
        {
            if (!IsSimulationRunning_i())
            {
                /// Setup stage handling
                if (this.root.Manager.CurrentState == this.root.Manager.WaitingSetupStepAWs || this.root.Manager.CurrentState == this.root.Manager.LobbyCreated)
                {
                    /// Will be handled by the manager.
                    this.root.Manager.SetupStageCtrlPackage(package, senderID - 1);
                }
                else
                {
                    /// Control packages are only allowed in WaitingSetupStepAWs or LobbyCreated state during setup stage.
                    this.root.Manager.SetupStageError(senderID - 1); /// line n is represented by channel n-1.
                }
            }
            else
            {
                /// Control package during simulation stage is not allowed
                this.root.SimulationMgr.SimulationStageError("Illegal call to HostEventHandler.ControlPackageFromClientHdl during setup stage!",
                                                             new byte[] { });
            }
        }

        /// <see cref="DssEventHandler.LineOpenedHdl"/>
        public override void LineOpenedHdl(int timestamp, int lineIdx)
        {
            if (lineIdx > 0)
            {
                if (!IsSimulationRunning_i())
                {
                    /// Setup stage
                    this.root.Manager.SetupStageLineOpened(lineIdx - 1); /// line n is represented by channel n-1.
                }
                else
                {
                    this.root.SimulationMgr.SimulationStageLineOpened(lineIdx);
                }
            }
        }

        /// <see cref="DssEventHandler.LineClosedHdl"/>
        public override void LineClosedHdl(int timestamp, int lineIdx)
        {
            if (!IsSimulationRunning_i())
            {
                /// Setup stage
                if (lineIdx > 0)
                {
                    this.root.Manager.SetupStageLineClosed(lineIdx - 1); /// line n is represented by channel n-1.
                }
            }
            else
            {
                /// Just ignore
                TraceManager.WriteAllTrace(string.Format("Channel-{0} has been closed successfully.", lineIdx - 1), DssTraceFilters.SIMULATION_INFO);
            }
        }

        /// <see cref="DssEventHandler.LineEngagedHdl"/>
        public override void LineEngagedHdl(int timestamp, int lineIdx)
        {
            if (!IsSimulationRunning_i())
            {
                /// Setup stage
                if (lineIdx > 0)
                {
                    this.root.Manager.SetupStageLineEngaged(lineIdx - 1); /// line n is represented by channel n-1.
                }
            }
            else
            {
                /// Just ignore
                TraceManager.WriteAllTrace(string.Format("WARNING! Channel-{0} engaged during simulation stage!", lineIdx - 1), DssTraceFilters.SIMULATION_ERROR);
            }
        }

        /// <see cref="DssEventHandler.LobbyIsRunningHdl"/>
        public override void LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount)
        {
            if (!IsSimulationRunning_i())
            {
                /// The ID of the host must be 0.
                if (idOfThisPeer == 0)
                {
                    this.root.InitRoot(idOfThisPeer, opCount);
                    this.root.Manager.LobbyIsRunning();
                }
                else
                {
                    throw new DssException("Illegal call to HostEventHandler.LobbyIsRunning!");
                }
            }
            else
            {
                throw new DssException("Illegal call to HostEventHandler.LobbyIsRunningHdl during simulation stage!");
            }
        }

        /// <see cref="DssEventHandler.IsSimulationRunning_i"/>
        protected override bool IsSimulationRunning_i()
        {
            return this.root.Manager.CurrentState == this.root.Manager.SimulationStage;
        }

        /// <see cref="DssEventHandler.PackageArrivedDuringSetupStage_i"/>
        protected override void PackageArrivedDuringSetupStage_i(Common.RCPackage package, int senderID)
        {
            if (senderID <= 0 && senderID >= this.root.OpCount) { throw new ArgumentOutOfRangeException("senderID"); }

            this.root.Manager.SetupStageError(senderID - 1);
        }

        /// <summary>
        /// Reference to the root of the local data-model.
        /// </summary>
        private DssHostRoot root;
    }
}
