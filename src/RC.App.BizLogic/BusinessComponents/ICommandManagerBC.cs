using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the command manager business component.
    /// </summary>
    [ComponentInterface]
    interface ICommandManagerBC
    {
        /// <summary>
        /// Gets the sprite palettes defined for the command workflows.
        /// </summary>
        IEnumerable<ISpritePalette> CommandWorkflowSpritePalettes { get; }
    }
}
