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
        /// Special SimulationStageHdl at guest-side.
        /// </summary>
        class GuestSimStageHdl : SimulationStageHdl
        {
            public GuestSimStageHdl(DssGuestRoot guestRoot) : base(guestRoot)
            {
                this.root = guestRoot;
                this.session = guestRoot.Session;
            }

            #region DssEventHandler overriden methods

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool PackageArrivedHdl(int timestamp, SCPackage package, int senderID) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool ControlPackageFromClientHdl(int timestamp, SCPackage package, int senderID) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            public override bool ControlPackageFromServerHdl(int timestamp, SCPackage package)
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

            //protected override void SimStopTest()
            //{
                /// TODO: delete this function
            //}

            /// <see cref="SimulationStageHdl.IsSimulationRunning_i"/>
            protected override bool IsSimulationRunning_i()
            {
                return this.session.CurrentState == this.session.Simulating;
            }

            /// <summary>
            /// Root of the guest-side data model.
            /// </summary>
            private DssGuestRoot root;

            /// <summary>
            /// The guest-side session.
            /// </summary>
            private DssGuestSessionSM session;
        }
    }
}
