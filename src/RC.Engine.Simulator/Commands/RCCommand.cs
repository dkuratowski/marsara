using RC.Common;
using RC.Common.Configuration;
using RC.Common.Diagnostics;
using System;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Wrapper for encoding/decoding commands sent by the players to the active scenario.
    /// </summary>
    public class RCCommand
    {
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

            return new RCCommand(
                cmdPackage.ReadString(0),
                cmdPackage.ReadIntArray(1),
                new RCNumVector(new RCNumber(cmdPackage.ReadInt(2)), new RCNumber(cmdPackage.ReadInt(3))),
                cmdPackage.ReadInt(4),
                cmdPackage.ReadString(5));
        }

        /// <summary>
        /// Constructs an RCCommand instance.
        /// </summary>
        /// <param name="commandType">The type of the command or null if the type of the command is unknown.</param>
        /// <param name="recipientEntities">The recipient entities of the command.</param>
        /// <param name="targetPosition">The target position of the command or RCNumVector.Undefined if the command doesn't have target position.</param>
        /// <param name="targetEntity">The target entity of the command or -1 if the command doesn't have target entity.</param>
        /// <param name="parameter">The optional parameter of the command or null if the command doesn't have parameter.</param>
        public RCCommand(string commandType, int[] recipientEntities, RCNumVector targetPosition, int targetEntity, string parameter)
        {
            if (recipientEntities == null || recipientEntities.Length == 0) { throw new ArgumentNullException("recipientEntities"); }

            this.commandType = commandType;
            this.recipientEntities = recipientEntities;
            this.targetPosition = targetPosition != RCNumVector.Undefined ? targetPosition : new RCNumVector(0, 0);
            this.targetEntity = targetEntity;
            this.parameter = parameter;
        }

        /// <summary>
        /// Creates an RCPackage from this command.
        /// </summary>
        /// <returns>The created RCPackage.</returns>
        public RCPackage ToPackage()
        {
            RCPackage package = RCPackage.CreateCustomDataPackage(RCCommand.COMMAND_PACKAGEFORMAT);
            package.WriteString(0, this.commandType ?? string.Empty);
            package.WriteIntArray(1, this.recipientEntities);
            package.WriteInt(2, this.targetPosition.X.Bits);
            package.WriteInt(3, this.targetPosition.Y.Bits);
            package.WriteInt(4, this.targetEntity);
            package.WriteString(5, this.parameter ?? string.Empty);
            return package;
        }

        /// <summary>
        /// Gets the type of the command or null if the type of this command is unknown.
        /// </summary>
        public string CommandType { get { return this.commandType; } }

        /// <summary>
        /// Gets the list of the IDs of the command recipients.
        /// </summary>
        public int[] RecipientEntities { get { return this.recipientEntities; } }

        /// <summary>
        /// Gets the target position of this command.
        /// </summary>
        public RCNumVector TargetPosition { get { return this.targetPosition; } }

        /// <summary>
        /// Gets the ID of the target entity of the command.
        /// </summary>
        public int TargetEntity { get { return this.targetEntity; } }

        /// <summary>
        /// Gets the optional parameter of the command or null if the command has no optional parameter.
        /// </summary>
        public string Parameter { get { return this.parameter; } }

        /// <summary>
        /// The type of the command or null if the type of this command is unknown.
        /// </summary>
        private readonly string commandType;

        /// <summary>
        /// The list of the IDs of the command recipients.
        /// </summary>
        private readonly int[] recipientEntities;

        /// <summary>
        /// The target position of the command.
        /// </summary>
        private readonly RCNumVector targetPosition;

        /// <summary>
        /// The ID of the target entity of the command.
        /// </summary>
        private readonly int targetEntity;

        /// <summary>
        /// The optional parameter of the command or null if the command has no optional parameter.
        /// </summary>
        private readonly string parameter;

        /// <summary>
        /// The ID of the command package format.
        /// </summary>
        private static readonly int COMMAND_PACKAGEFORMAT = RCPackageFormatMap.Get("RC.Engine.Simulator.Command");
    }
}
