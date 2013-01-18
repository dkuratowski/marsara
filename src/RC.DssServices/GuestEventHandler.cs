using RC.Common;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// This class responsible for DSS event handling at guest side.
    /// </summary>
    class GuestEventHandler : DssEventHandler
    {
        /// <summary>
        /// Constructs a GuestEventHandler object.
        /// </summary>
        public GuestEventHandler(DssGuestRoot root)
            : base(root)
        {
            this.root = root;
        }

        /// <see cref="DssEventHandler.ControlPackageFromServerHdl"/>
        public override void ControlPackageFromServerHdl(int timestamp, RCPackage package)
        {
            if (!IsSimulationRunning_i())
            {
                /// Control package during setup stage
                this.root.Session.SetupStageCtrlPackage(package);
            }
            else
            {
                /// Control package during simulation stage is not allowed
                this.root.SimulationMgr.SimulationStageError("Illegal call to GuestEventHandler.ControlPackageFromServerHdl during setup stage!",
                                                             new byte[] { });
            }
        }

        /// <see cref="DssEventHandler.LobbyLostHdl"/>
        public override void LobbyLostHdl(int timestamp)
        {
            if (!IsSimulationRunning_i())
            {
                /// Lost connection during setup stage
                this.root.Session.SetupStageError();
            }
            else
            {
                /// Lost connection during simulation stage
                this.root.SimulationMgr.SimulationStageError("Host connection lost!",
                                                             new byte[] { });
            }
        }

        /// <see cref="DssEventHandler.LobbyIsRunningHdl"/>
        public override void LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount)
        {
            if (!IsSimulationRunning_i())
            {
                /// The ID of a guest must be greater than 0.
                if (idOfThisPeer > 0)
                {
                    this.root.InitRoot(idOfThisPeer, opCount);
                    this.root.Session.LobbyIsRunning();
                }
                else
                {
                    throw new DssException("Illegal call to GuestEventHandler.LobbyIsRunning!");
                }
            }
            else
            {
                throw new DssException("Illegal call to GuestEventHandler.LobbyIsRunningHdl during simulation stage!");
            }
        }

        /// <see cref="DssEventHandler.IsSimulationRunning_i"/>
        protected override bool IsSimulationRunning_i()
        {
            return this.root.Session.CurrentState == this.root.Session.Simulating;
        }

        /// <see cref="DssEventHandler.PackageArrivedDuringSetupStage_i"/>
        protected override void PackageArrivedDuringSetupStage_i(RCPackage package, int senderID)
        {
            /// If the sender of the package is the host --> error
            if (senderID == 0)
            {
                this.root.Session.SetupStageError();
            }

            /// Otherwise we ignore the package
            TraceManager.WriteAllTrace(string.Format("WARNING! Package arrived from operator-{0} during setup stage: {1}. Package will be ignored!", senderID, package.ToString()), DssTraceFilters.SETUP_STAGE_ERROR);
        }

        /// <summary>
        /// Reference to the root of the local data-model.
        /// </summary>
        private DssGuestRoot root;
    }
}
