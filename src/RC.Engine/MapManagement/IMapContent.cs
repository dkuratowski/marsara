using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Prototype of methods that handle property change events of map contents.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    public delegate void MapContentPropertyChangeHdl(IMapContent sender);

    /// <summary>
    /// Common interface for anything that can be attached to the map as content.
    /// </summary>
    public interface IMapContent
    {
        /// <summary>
        /// Gets the position of the map content.
        /// </summary>
        RCNumRectangle Position { get; }

        /// <summary>
        /// This event is raised just before the position of this map content changes.
        /// </summary>
        event MapContentPropertyChangeHdl PositionChanging;

        /// <summary>
        /// This event is raised just after the position of this map content has been changed.
        /// </summary>
        event MapContentPropertyChangeHdl PositionChanged;
    }
}
