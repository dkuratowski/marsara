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
        /// This class is responsible for handling events during the simulation stage.
        /// </summary>
        abstract class SimulationStageHdl : DssEventHandler
        {
            /// <summary>
            /// Constructs a SimulationStageHdl object.
            /// </summary>
            public SimulationStageHdl(DssRoot root)
            {
                this.root = root;
                //this.root.SimulationMgr.FrmExecutorMethod = this.ExecuteNextSimulationFrame;
            }

            #region DssEventHandler overriden methods

            /// <see cref="DssEventHandler.PackageArrivedHdl"/>
            public override bool PackageArrivedHdl(int timestamp, SCPackage package, int senderID)
            {
                if (IsSimulationRunning_i())
                {
                    if (package.PackageFormat.ID == DssRoot.DSS_COMMAND)
                    {
                        if (!CommandPackageArrived(package, senderID))
                        {
                            this.root.SimulationMgr.SimulationStageError();
                        }
                    }
                    else if (package.PackageFormat.ID == DssRoot.DSS_COMMIT)
                    {
                        if (!this.root.SimulationMgr.RegisterCommit(package, senderID))
                        {
                            this.root.SimulationMgr.SimulationStageError();
                        }
                    }
                    else if (package.PackageFormat.ID == DssRoot.DSS_CTRL_COMMIT_ANSWER)
                    {
                        if (!this.root.SimulationMgr.RegisterCommitAnswer(package, senderID))
                        {
                            this.root.SimulationMgr.SimulationStageError();
                        }
                    }
                    else if (package.PackageFormat.ID == DssRoot.DSS_LEAVE)
                    {
                        if (this.root.SimulationMgr.RegisterLeaveMessage(package, senderID))
                        {
                            /// Indicate the leave of an operator to the root (for closing the corresponding channel at host side).
                            this.root.OperatorLeftDuringSim(senderID);
                        }
                        else
                        {
                            this.root.SimulationMgr.SimulationStageError();
                        }
                    }
                    return true;
                }
                else
                {
                    /// If we are not in the simulation stage, then the event will be handled by another DssEventHandler.
                    return false;
                }
            }

            /// <see cref="DssEventHandler.ControlPackageFromServerHdl"/>
            /// public override bool ControlPackageFromServerHdl(int timestamp, SCPackage package) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.ControlPackageFromClientHdl"/>
            /// public override bool ControlPackageFromClientHdl(int timestamp, SCPackage package, int senderID) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LineOpenedHdl"/>
            /// public override bool LineOpenedHdl(int timestamp, int lineIdx) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LineClosedHdl"/>
            /// public override bool LineClosedHdl(int timestamp, int lineIdx) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LineEngagedHdl"/>
            /// public override bool LineEngagedHdl(int timestamp, int lineIdx) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LobbyLostHdl"/>
            /// public override bool LobbyLostHdl(int timestamp) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            /// <see cref="DssEventHandler.LobbyIsRunningHdl"/>
            /// public override bool LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount) { return false; }
            /// ***************************************** NOT IMPLEMENTED *****************************************

            #endregion
            
            /// <summary>
            /// Internal function to check whether we are in the simulation stage or not.
            /// </summary>
            protected abstract bool IsSimulationRunning_i();

            /// <summary>
            /// Internal function to handle command packages and send them to the simulation manager.
            /// </summary>
            /// <param name="commandPackage">The package that contains the commands.</param>
            /// <param name="senderID">The ID of the sender peer.</param>
            /// <returns>True in case of success, false otherwise.</returns>
            private bool CommandPackageArrived(SCPackage commandPackage, int senderID)
            {
                if (commandPackage == null) { throw new ArgumentNullException("commandPackage"); }
                if (!commandPackage.IsCommitted ||
                    DssRoot.DSS_COMMAND != commandPackage.PackageFormat.ID) { throw new ArgumentException("commandPackage"); }
                if (senderID < 0 || senderID >= this.root.OpCount) { throw new ArgumentException("senderID"); }
                if (senderID == this.root.IdOfThisPeer) { throw new ArgumentException("senderID"); }

                int roundIdx = commandPackage.ReadInt(0);
                int frameIdx = commandPackage.ReadInt(1);
                byte[] cmdBuffer = commandPackage.ReadByteArray(2);
                if (cmdBuffer.Length == 0) { return false; }

                List<SCPackage> cmds = new List<SCPackage>();

                int offset = 0;
                while (offset < cmdBuffer.Length)
                {
                    /// Parse and check the next command package.
                    int parsedBytes = 0;
                    SCPackage currPackage = SCPackage.Parse(cmdBuffer, offset, cmdBuffer.Length, out parsedBytes);
                    if (currPackage != null && currPackage.IsCommitted)
                    {
                        cmds.Add(currPackage);
                        offset += parsedBytes;
                    }
                    else
                    {
                        /// Uncommitted command package.
                        return false;
                    }
                }

                return this.root.SimulationMgr.RegisterCommands(cmds.ToArray(), roundIdx, frameIdx, senderID);
            }

            /// <summary>
            /// The root of the local data-model.
            /// </summary>
            private DssRoot root;
        }
    }
}
