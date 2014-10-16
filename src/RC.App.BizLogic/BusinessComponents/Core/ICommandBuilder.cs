using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Internal interface used by the CommandInputListeners for building a command.
    /// </summary>
    interface ICommandBuilder
    {
        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the command type has already been set.</exception>
        string CommandType { get; set; }

        /// <summary>
        /// Gets or sets the target position of the command.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the target position has already been set.</exception>
        RCNumVector TargetPosition { get; set; }

        /// <summary>
        /// Gets or sets the optional parameter of the command.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the optional parameter has already been set.</exception>
        string Parameter { get; set; }
    }
}
