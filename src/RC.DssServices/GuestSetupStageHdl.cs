using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC.Common;

namespace SC
{
    namespace DssServices
    {
        class GuestSetupStageHdl : DssEventHandler
        {
            /// <summary>
            /// Constructs a HostSetupStageHdl object.
            /// </summary>
            /// <param name="root">The root of the guest-side data model.</param>
            public GuestSetupStageHdl(DssGuestRoot root)
            {
                this.guestRoot = root;
                this.session = root.Session;
            }

            #region DssEventHandler overriden methods

            /// <see cref="DssEventHandler.PackageArrivedHdl"/>
            public override bool PackageArrivedHdl(int timestamp, SCPackage package, int senderID)
            {
                /// This event is handled only when the simulation is not running (in setup stage).
                if (this.session.CurrentState != this.session.Simulating)
                {
                    /// TODO: Incoming (non-control) package from another guest during setup stage will be ignored.
                    return true;
                }
                else
                {
                    /// If the session is in Simulating state, then the event will be handled by another DssEventHandler.
                    return false;
                }
            }

            /// <see cref="DssEventHandler.ControlPackageFromServerHdl"/>
            public override bool ControlPackageFromServerHdl(int timestamp, SCPackage package)
            {
                /// This event is handled only when the simulation is not running (in setup stage).
                if (this.session.CurrentState != this.session.Simulating)
                {
                    /// Will be handled by the session.
                    this.session.ControlPackage(timestamp, package);
                    return true;
                }
                else
                {
                    /// If the session is in Simulating state, then the event will be handled by another DssEventHandler.
                    return false;
                }
            }

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool ControlPackageFromClientHdl(int timestamp, SCPackage package, int senderID) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LineOpenedHdl"/>
            public override bool LineOpenedHdl(int timestamp, int lineIdx) { return true; }
            /// ********************************************* IGNORED *********************************************

            /// <see cref="DssEventHandler.LineClosedHdl"/>
            public override bool LineClosedHdl(int timestamp, int lineIdx) { return true; }
            /// ********************************************* IGNORED *********************************************

            /// <see cref="DssEventHandler.LineEngagedHdl"/>
            public override bool LineEngagedHdl(int timestamp, int lineIdx) { return true; }
            /// ********************************************* IGNORED *********************************************

            /// <see cref="DssEventHandler.LobbyLostHdl"/>
            public override bool LobbyLostHdl(int timestamp)
            {
                //this.session.LobbyLost(timestamp);
                this.session.GuestError();
                return true;
            }

            /// <see cref="DssEventHandler.LobbyIsRunningHdl"/>
            public override bool LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount)
            {
                /// The ID of a guest must be greater than 0.
                if (idOfThisPeer > 0)
                {
                    this.guestRoot.InitRoot(idOfThisPeer, opCount);
                    this.session.LobbyIsRunning();//idOfThisPeer - 1);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion

            /// <summary>
            /// The root of the guest-side data model.
            /// </summary>
            private DssGuestRoot guestRoot;

            /// <summary>
            /// The guest-side session.
            /// </summary>
            private DssGuestSessionSM session;
        }
    }
}
