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
        /// Special SimulationStageHdl at host-side.
        /// </summary>
        class HostSimStageHdl : SimulationStageHdl
        {
            public HostSimStageHdl(DssHostRoot hostRoot) : base(hostRoot)
            {
                this.root = hostRoot;
                this.manager = hostRoot.Manager;
            }

            #region DssEventHandler overriden methods

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool PackageArrivedHdl(int timestamp, SCPackage package, int senderID) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            public override bool ControlPackageFromClientHdl(int timestamp, SCPackage package, int senderID)
            {
                if (IsSimulationRunning_i())
                {
                    /// TODO: implement this function
                    return true;
                }
                else
                {
                    /// If we are not in the simulation stage, then the event will be handled by another DssEventHandler.
                    return false;
                }
            }

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool ControlPackageFromServerHdl(int timestamp, SCPackage package) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool LineOpenedHdl(int timestamp, int lineIdx) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool LineClosedHdl(int timestamp, int lineIdx) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool LineEngagedHdl(int timestamp, int lineIdx) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool LobbyLostHdl(int timestamp) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            #endregion

            /// <see cref="SimulationStageHdl.SimStopTest"/>
            //protected override void SimStopTest()
            //{
                //this.root.SimulationError(0);
            //    this.manager.GuestError(0);
            //}

            /// <see cref="SimulationStageHdl.IsSimulationRunning_i"/>
            protected override bool IsSimulationRunning_i()
            {
                return this.manager.CurrentState == this.manager.RunningSimulation;
            }

            /// <summary>
            /// Root of the host-side data model.
            /// </summary>
            DssHostRoot root;

            /// <summary>
            /// The DSS-manager SMC.
            /// </summary>
            DssManagerSM manager;
        }
    }
}
