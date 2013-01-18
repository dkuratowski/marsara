using System;
using System.Collections.Generic;
using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// Base class of objects that perform DSS-event handling.
    /// </summary>
    abstract class DssEventHandler
    {
        /// <summary>
        /// Constructs a DssEventHandler object.
        /// </summary>
        public DssEventHandler(DssRoot root)
        {
            this.root = root;
        }

        /// <summary>
        /// Called when a NETWORK_CUSTOM_PACKAGE has arrived from the lobby.
        /// </summary>
        /// <remarks>Must be implemented at both server and client side.</remarks>
        public virtual void PackageArrivedHdl(int timestamp, RCPackage package, int senderID)
        {
            if (IsSimulationRunning_i())
            {
                /// Package arrived during simulation stage must be handled.
                if (package.PackageFormat.ID == DssRoot.DSS_COMMAND)
                {
                    if (!CommandPackageArrived(package, senderID))
                    {
                        this.root.SimulationMgr.SimulationStageError(string.Format("Processing incoming DSS_COMMAND package failed. Sender: {0}. Package: {1}", senderID, package.ToString()),
                                                                     new byte[] { });
                    }
                }
                else if (package.PackageFormat.ID == DssRoot.DSS_COMMIT)
                {
                    if (!this.root.SimulationMgr.RegisterCommit(package, senderID))
                    {
                        this.root.SimulationMgr.SimulationStageError(string.Format("Processing incoming DSS_COMMIT package failed. Sender: {0}. Package: {1}", senderID, package.ToString()),
                                                                     new byte[] { });
                    }
                }
                else if (package.PackageFormat.ID == DssRoot.DSS_COMMIT_ANSWER)
                {
                    if (!this.root.SimulationMgr.RegisterCommitAnswer(package, senderID))
                    {
                        this.root.SimulationMgr.SimulationStageError(string.Format("Processing incoming DSS_COMMIT_ANSWER package failed. Sender: {0}. Package: {1}", senderID, package.ToString()),
                                                                     new byte[] { });
                    }
                }
                else if (package.PackageFormat.ID == DssRoot.DSS_LEAVE)
                {
                    if (this.root.SimulationMgr.RegisterLeaveMessage(package, senderID))
                    {
                        this.root.OperatorLeftSimulationStage(senderID);
                    }
                    else
                    {
                        this.root.SimulationMgr.SimulationStageError(string.Format("Processing incoming DSS_LEAVE package failed. Sender: {0}. Package: {1}", senderID, package.ToString()),
                                                                     new byte[] { });
                    }
                }
                else if (package.PackageFormat.ID == DssRoot.DSS_SIM_ERROR)
                {
                    string errorDescr = package.ReadString(0);
                    byte[] customData = package.ReadByteArray(1);
                    this.root.SimulationMgr.SimulationStageErrorReceived(string.Format("DSS_SIM_ERROR received from operator-{0}: {1}", senderID, errorDescr), customData);
                }
                else
                {
                    this.root.SimulationMgr.SimulationStageError(string.Format("Unexpected package arrived from operator-{0}: {1}", senderID, package.ToString()),
                                                                 new byte[] { });
                }
            }
            else
            {
                /// Package arrived during setup stage is not allowed.
                PackageArrivedDuringSetupStage_i(package, senderID);
            }
        }

        /// <summary>
        /// Called when a NETWORK_CONTROL_PACKAGE has arrived from the lobby-server.
        /// </summary>
        /// <remarks>Must be implemented at client side.</remarks>
        public virtual void ControlPackageFromServerHdl(int timestamp, RCPackage package)
        {
            throw new DssException("DssEventHandler.ControlPackageFromServerHdl not implemented!");
        }

        /// <summary>
        /// Called when a NETWORK_CONTROL_PACKAGE has arrived from a lobby-client.
        /// </summary>
        /// <remarks>Must be implemented at server side.</remarks>
        public virtual void ControlPackageFromClientHdl(int timestamp, RCPackage package, int senderID)
        {
            throw new DssException("DssEventHandler.ControlPackageFromClientHdl not implemented!");
        }

        /// <summary>
        /// Called when a line has become opened at the lobby-server.
        /// </summary>
        /// <remarks>Must be implemented at server side.</remarks>
        public virtual void LineOpenedHdl(int timestamp, int lineIdx) { }

        /// <summary>
        /// Called when a line has become closed at the lobby-server.
        /// </summary>
        /// <remarks>Must be implemented at server side.</remarks>
        public virtual void LineClosedHdl(int timestamp, int lineIdx) { }

        /// <summary>
        /// Called when a line has become engaged at the lobby-server.
        /// </summary>
        /// <remarks>Must be implemented at server side.</remarks>
        public virtual void LineEngagedHdl(int timestamp, int lineIdx) { }

        /// <summary>
        /// Called when the connection has been lost with the lobby-server.
        /// </summary>
        /// <remarks>Must be implemented at client side.</remarks>
        public virtual void LobbyLostHdl(int timestamp) { }

        /// <summary>
        /// Called when the connection has been established between the lobby-server and a lobby-client.
        /// </summary>
        /// <remarks>Must be implemented at both server and client side.</remarks>
        public virtual void LobbyIsRunningHdl(int timestamp, int idOfThisPeer, int opCount) { }

        /// <summary>
        /// Internal function to check whether we are in the simulation stage or not.
        /// </summary>
        protected abstract bool IsSimulationRunning_i();

        /// <summary>
        /// Internal function to handle NETWORK_CUSTOM_PACKAGE arrived during setup stage.
        /// </summary>
        protected abstract void PackageArrivedDuringSetupStage_i(RCPackage package, int senderID);

        /// <summary>
        /// Internal function to handle command packages and send them to the simulation manager.
        /// </summary>
        /// <param name="commandPackage">The package that contains the commands.</param>
        /// <param name="senderID">The ID of the sender peer.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        private bool CommandPackageArrived(RCPackage commandPackage, int senderID)
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

            List<RCPackage> cmds = new List<RCPackage>();

            int offset = 0;
            int remainingBytes = cmdBuffer.Length;
            while (offset < cmdBuffer.Length)
            {
                /// Parse and check the next command package.
                int parsedBytes = 0;
                RCPackage currPackage = RCPackage.Parse(cmdBuffer, offset, remainingBytes, out parsedBytes);
                if (currPackage != null && currPackage.IsCommitted)
                {
                    cmds.Add(currPackage);
                    offset += parsedBytes;
                    remainingBytes -= parsedBytes;
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
        /// Reference to the root of the local data-model.
        /// </summary>
        private DssRoot root;
    }
}
