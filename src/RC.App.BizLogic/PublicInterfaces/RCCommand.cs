using RC.App.BizLogic.Core;
using RC.Common;
using RC.Common.Configuration;
using RC.Common.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// The base class of commands sent by the players to the active scenario.
    /// </summary>
    public abstract class RCCommand
    {
        /// <summary>
        /// Executes this command. Must be overriden in the derived classes.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Creates an RCPackage from this command.
        /// </summary>
        /// <returns>The created RCPackage.</returns>
        public RCPackage ToPackage()
        {
            RCPackage package = RCPackage.CreateCustomDataPackage(RCCommand.COMMAND_PACKAGEFORMAT);
            package.WriteString(0, this.mnemonic);
            package.WriteIntArray(1, this.recipientEntities);
            package.WriteInt(2, this.targetPosition.X.Bits);
            package.WriteInt(3, this.targetPosition.Y.Bits);
            package.WriteInt(4, this.targetEntity);
            package.WriteString(5, this.targetType);
            return package;
        }

        /// <summary>
        /// Creates an RCCommand object from the given RCPackage.
        /// </summary>
        /// <param name="cmdPackage">The RCPackage to be deserialized.</param>
        /// <returns>The created RCCommand.</returns>
        public static RCCommand FromPackage(RCPackage cmdPackage)
        {
            if (cmdPackage == null) { throw new ArgumentNullException("cmdPackage"); }
            if (!cmdPackage.IsCommitted) { throw new ArgumentException("The incoming package is not committed!", "cmdPackage"); }
            if (cmdPackage.PackageFormat.ID != RCCommand.COMMAND_PACKAGEFORMAT) { throw new ArgumentException("The incoming package is not a command package!", "cmdPackage"); }

            /// Create the command instance of the appropriate type.
            RCCommand command = CreateCommandByMnemonic(cmdPackage.ReadString(0));

            /// Fill the attributes of the command.
            command.recipientEntities = cmdPackage.ReadIntArray(1);
            command.targetPosition = new RCNumVector(new RCNumber(cmdPackage.ReadInt(2)), new RCNumber(cmdPackage.ReadInt(3)));
            command.targetEntity = cmdPackage.ReadInt(4);
            command.targetType = cmdPackage.ReadString(5);
            return command;
        }

        /// <summary>
        /// Creates a new RCCommand with the given attributes.
        /// </summary>
        /// <param name="mnemonic">The mnemonic of the command.</param>
        /// <param name="recipientEntities">The recipient entities of the command.</param>
        /// <param name="targetPosition">The target position of the command or RCNumVector.Undefined if the command doesn't have target position.</param>
        /// <param name="targetEntity">The target entity of the command or -1 if the command doesn't have target entity.</param>
        /// <param name="targetType">The target type of the command or null if the command doesn't have target type.</param>
        /// <returns>The created command object.</returns>
        public static RCCommand Create(string mnemonic, int[] recipientEntities, RCNumVector targetPosition, int targetEntity, string targetType)
        {
            if (mnemonic == null) { throw new ArgumentNullException("mnemonic"); }
            if (recipientEntities == null || recipientEntities.Length == 0) { throw new ArgumentNullException("recipientEntities"); }

            /// Create the command instance of the appropriate type.
            RCCommand command = CreateCommandByMnemonic(mnemonic);

            /// Fill the attributes of the command.
            command.recipientEntities = recipientEntities;
            command.targetPosition = targetPosition != RCNumVector.Undefined ? targetPosition : new RCNumVector(0, 0);
            command.targetEntity = targetEntity;
            command.targetType = targetType != null ? targetType : string.Empty;
            return command;
        }

        /// <summary>
        /// Constructs an RCCommand instance.
        /// </summary>
        protected RCCommand() { }

        /// <summary>
        /// Gets the list of the IDs of the command recipients.
        /// </summary>
        protected int[] RecipientEntities { get { return this.recipientEntities; } }

        /// <summary>
        /// Gets the target position of this command.
        /// </summary>
        protected RCNumVector TargetPosition { get { return this.targetPosition; } }

        /// <summary>
        /// Gets the ID of the target entity of the command.
        /// </summary>
        protected int TargetEntity { get { return this.targetEntity; } }

        /// <summary>
        /// Gets the name of the target type of the command.
        /// </summary>
        protected string TargetType { get { return this.targetType; } }

        /// <summary>
        /// Internal method for creating a command instance of the appropriate type by the given mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic of the command.</param>
        /// <returns>The created command instance.</returns>
        /// <exception cref="InvalidOperationException">In case of unexpected mnemonic.</exception>
        private static RCCommand CreateCommandByMnemonic(string mnemonic)
        {
            RCCommand command = null;
            switch (mnemonic)
            {
                case MoveCommand.MNEMONIC:
                    command = new MoveCommand();
                    break;
                default:
                    break;
            }
            if (command == null) { throw new InvalidOperationException(string.Format("Unexpected mnemonic '{0}'!", mnemonic)); }
            command.mnemonic = mnemonic;
            return command;
        }

        /// <summary>
        /// The mnemonic of the command.
        /// </summary>
        private string mnemonic;

        /// <summary>
        /// The list of the IDs of the command recipients.
        /// </summary>
        private int[] recipientEntities;

        /// <summary>
        /// The target position of the command.
        /// </summary>
        private RCNumVector targetPosition;

        /// <summary>
        /// The ID of the target entity of the command.
        /// </summary>
        private int targetEntity;

        /// <summary>
        /// The name of the target type of the command.
        /// </summary>
        private string targetType;

        /// <summary>
        /// The ID of the command package format.
        /// </summary>
        private static int COMMAND_PACKAGEFORMAT = RCPackageFormatMap.Get("RC.App.BizLogic.Command");
    }

    /// <summary>
    /// Represents a move command.
    /// </summary>
    class MoveCommand : RCCommand
    {
        /// <summary>
        /// Constructs a MoveCommand instance.
        /// </summary>
        internal MoveCommand() { }

        #region RCCommand overrides

        /// <see cref="RCCommand.Execute"/>
        public override void Execute()
        {
            TraceManager.WriteAllTrace(string.Format("MOV {0}", this.TargetPosition), BizLogicTraceFilters.INFO);
        }

        #endregion RCCommand overrides

        /// <summary>
        /// The mnemonic of a move command.
        /// </summary>
        public const string MNEMONIC = "MOV";
    }
}
