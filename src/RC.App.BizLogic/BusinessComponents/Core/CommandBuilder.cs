using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// This class is used for building a command package.
    /// </summary>
    class CommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Constructs a CommandBuilder instance.
        /// </summary>
        public CommandBuilder()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.Reset();
        }

        /// <summary>
        /// Creates an RCPackage from the state of this CommandBuilder.
        /// </summary>
        /// <returns>The created RCPackage.</returns>
        /// <exception cref="InvalidOperationException">If RCPackage could not be created.</exception>
        public RCPackage CreatePackage()
        {
            if (this.commandType == null) { throw new InvalidOperationException("Undefined command type!"); }

            HashSet<int> recipientEntities = this.selectionManager.CurrentSelection;
            if (recipientEntities.Count == 0) { throw new InvalidOperationException("No recipient entities!"); }
            /// TODO: check if all the recipient entities belong to the local player!

            RCPackage package = RCPackage.CreateCustomDataPackage(COMMAND_PACKAGEFORMAT);
            package.WriteString(0, this.commandType);
            package.WriteIntArray(1, recipientEntities.ToArray());
            package.WriteInt(2, this.targetPosition != RCNumVector.Undefined ? this.targetPosition.X.Bits : 0);
            package.WriteInt(3, this.targetPosition != RCNumVector.Undefined ? this.targetPosition.Y.Bits : 0);
            package.WriteInt(4, this.targetEntity);
            package.WriteString(5, this.parameter ?? string.Empty);
            return package;
        }

        /// <summary>
        /// Resets the state of this CommandBuilder.
        /// </summary>
        public void Reset()
        {
            this.commandType = null;
            this.targetPosition = RCNumVector.Undefined;
            this.targetEntity = -1;
            this.parameter = null;
            this.isEnabled = false;
        }

        /// <summary>
        /// Enables/disables this CommandBuilder.
        /// </summary>
        /// <param name="isEnabled">True to enable, false to disable.</param>
        public void SetEnabled(bool isEnabled) { this.isEnabled = isEnabled; }

        #region ICommandBuilder members

        /// <see cref="ICommandBuilder.CommandType"/>
        public string CommandType
        {
            get { return this.commandType; }
            set
            {
                if (value == null) { throw new ArgumentNullException("CommandType"); }
                if (!this.isEnabled) { throw new InvalidOperationException("CommandBuilder disabled!"); }
                if (this.commandType != null) { throw new InvalidOperationException("CommandType already defined!"); }
                this.commandType = value;
            }
        }

        /// <see cref="ICommandBuilder.TargetPosition"/>
        public RCNumVector TargetPosition
        {
            get { return this.targetPosition; }
            set
            {
                if (value == RCNumVector.Undefined) { throw new ArgumentNullException("TargetPosition"); }
                if (!this.isEnabled) { throw new InvalidOperationException("CommandBuilder disabled!"); }
                if (this.targetPosition != RCNumVector.Undefined) { throw new InvalidOperationException("TargetPosition already defined!"); }
                this.targetPosition = value;
                this.targetEntity = this.selectionManager.GetEntity(targetPosition);
            }
        }

        /// <see cref="ICommandBuilder.Parameter"/>
        public string Parameter
        {
            get { return this.parameter; }
            set
            {
                if (value == null) { throw new ArgumentNullException("Parameter"); }
                if (!this.isEnabled) { throw new InvalidOperationException("CommandBuilder disabled!"); }
                if (this.parameter != null) { throw new InvalidOperationException("Parameter already defined!"); }
                this.parameter = value;
            }
        }

        #endregion ICommandBuilder members

        /// <summary>
        /// The type of the command.
        /// </summary>
        private string commandType;

        /// <summary>
        /// The target position of the command or RCNumVector.Undefined if the command has no target position.
        /// </summary>
        private RCNumVector targetPosition;

        /// <summary>
        /// The ID of the target entity of the command or -1 if the command has no target entity.
        /// </summary>
        private int targetEntity;

        /// <summary>
        /// The optional parameter of the command or null if the command has no optional parameter.
        /// </summary>
        private string parameter;

        /// <summary>
        /// This flag indicates whether sending inputs to this CommandBuilder is currently enabled or not.
        /// </summary>
        private bool isEnabled;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;

        /// <summary>
        /// The ID of the command package format.
        /// </summary>
        private static int COMMAND_PACKAGEFORMAT = RCPackageFormatMap.Get("RC.App.BizLogic.Command");
    }
}
