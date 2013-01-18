using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC.Common;

namespace SC
{
    namespace DssServices
    {
        /// <summary>
        /// This class is responsible for event handling during the setup stage of the DSS.
        /// </summary>
        class HostSetupStageHdl : DssEventHandler
        {
            /// <summary>
            /// Constructs a HostSetupStageHdl object.
            /// </summary>
            /// <param name="root">The root of the host-side data model.</param>
            public HostSetupStageHdl(DssHostRoot root)
            {
                this.hostRoot = root;
                this.manager = root.Manager;
            }

            #region DssEventHandler overriden methods

            /// <see cref="DssEventHandler.PackageArrivedHdl"/>
            public override bool PackageArrivedHdl(int timestamp, SCPackage package, int senderID)
            {
                /// This event is handled only when the simulation is not running (in setup stage).
                if (this.manager.CurrentState != this.manager.RunningSimulation)
                {
                    if (this.manager.CurrentState == this.manager.Waiting)
                    {
                        /// TODO: Incoming (non-control) package from a guest is only allowed between a stop_sim and the first
                        ///       setup_step_answer packages. Those incoming (non-control) packages will be ignored even if
                        ///       the DSS-manager is in Waiting state.
                    }
                    else
                    {
                        this.manager.GuestError(senderID - 1); /// line n is represented by channel n-1.
                    }
                    return true;
                }
                else
                {
                    /// If the DSS-manager is in RunningSimulation state, then the event will be handled by another DssEventHandler.
                    return false;
                }
            }

            /// <see cref="DssEventHandler.ControlPackageFromServerHdl"/>
            /// public override bool ControlPackageFromServerHdl(int timestamp, SCPackage package) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            public override bool ControlPackageFromClientHdl(int timestamp, SCPackage package, int senderID)
            {
                /// This event is handled only when the simulation is not running (in setup stage).
                if (this.manager.CurrentState != this.manager.RunningSimulation)
                {
                    if (this.manager.CurrentState == this.manager.Waiting || this.manager.CurrentState == this.manager.Uninitialized)
                    {
                        /// Will be handled by the manager.
                        this.manager.ControlPackage(timestamp, package, senderID - 1);
                    }
                    else
                    {
                        /// Control packages are only allowed in Waiting or Uninitialized state during setup stage.
                        this.manager.GuestError(senderID - 1); /// line n is represented by channel n-1.
                    }
                    return true;
                }
                else
                {
                    /// If the DSS-manager is in RunningSimulation state, then the event will be handled by another DssEventHandler.
                    return false;
                }
            }

            /// <see cref="DssEventHandler.LineOpenedHdl"/>
            public override bool LineOpenedHdl(int timestamp, int lineIdx)
            {
                if (lineIdx > 0)
                {
                    this.manager.LineOpened(timestamp, lineIdx - 1); /// line n is represented by channel n-1.
                }
                return true;
            }

            /// <see cref="DssEventHandler.LineClosedHdl"/>
            public override bool LineClosedHdl(int timestamp, int lineIdx)
            {
                if (lineIdx > 0)
                {
                    this.manager.LineClosed(timestamp, lineIdx - 1); /// line n is represented by channel n-1.
                }
                return true;
            }

            /// <see cref="DssEventHandler.LineEngagedHdl"/>
            public override bool LineEngagedHdl(int timestamp, int lineIdx)
            {
                if (lineIdx > 0)
                {
                    this.manager.LineEngaged(timestamp, lineIdx - 1); /// line n is represented by channel n-1.
                }
                return true;
            }

            /// <see cref="DssEventHandler.LobbyLostHdl"/>
            /// public override bool LobbyLostHdl(int timestamp) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LobbyIsRunningHdl"/>
            public override bool LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount)
            {
                /// The ID of the host must be 0.
                if (idOfThisPeer == 0)
                {
                    this.hostRoot.InitRoot(idOfThisPeer, opCount);
                    this.manager.LobbyIsRunning();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion

            /// <summary>
            /// The root of the host-side data model.
            /// </summary>
            private DssHostRoot hostRoot;

            /// <summary>
            /// The DSS-manager SMC.
            /// </summary>
            private DssManagerSM manager;
        }
    }
}
