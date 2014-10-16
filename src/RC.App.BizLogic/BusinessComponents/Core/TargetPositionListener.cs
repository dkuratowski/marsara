using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a command input listener that receives target position from the presentation layer.
    /// </summary>
    class TargetPositionListener : CommandInputListener, ITargetPositionListener
    {
        /// <see cref="CommandInputListener.TryComplete"/>
        public override bool TryComplete()
        {
            return this.CommandBuilder.TargetPosition != RCNumVector.Undefined;
        }

        #region ITargetPositionListener members

        /// <see cref="ITargetPositionListener.SelectedBuildingType"/>
        public string SelectedBuildingType
        {
            get
            {
                return this.CommandBuilder.CommandType == TargetPositionListener.BUILD_COMMAND
                     ? this.CommandBuilder.Parameter
                     : null;
            }
        }

        /// <see cref="ITargetPositionListener.SelectTargetPosition"/>
        public void SelectTargetPosition(RCNumVector targetPosition)
        {
            this.CommandBuilder.TargetPosition = targetPosition;
        }

        #endregion ITargetPositionListener members

        /// <summary>
        /// The name of the "Build" command.
        /// </summary>
        private const string BUILD_COMMAND = "Build";
    }
}
