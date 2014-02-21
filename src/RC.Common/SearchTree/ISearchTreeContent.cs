using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Prototype of methods that handle property change events of contents.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    public delegate void ContentBoundingBoxChangeHdl(ISearchTreeContent sender);

    /// <summary>
    /// Common interface for anything that can be attached to a search tree as a content.
    /// </summary>
    public interface ISearchTreeContent
    {
        /// <summary>
        /// Gets the bounding box of the content.
        /// </summary>
        RCNumRectangle BoundingBox { get; }

        /// <summary>
        /// This event is raised just before the bounding box of this content changes.
        /// </summary>
        event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <summary>
        /// This event is raised just after the bounding box of this content has been changed.
        /// </summary>
        event ContentBoundingBoxChangeHdl BoundingBoxChanged;
    }
}
