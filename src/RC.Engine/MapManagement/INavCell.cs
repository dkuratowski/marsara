using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of a navigation cell.
    /// </summary>
    public interface INavCell
    {
        /// <summary>
        /// Gets the data attached to this navigation cell.
        /// </summary>
        CellData Data { get; }
    }
}
