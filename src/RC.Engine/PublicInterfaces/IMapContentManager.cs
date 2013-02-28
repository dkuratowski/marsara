using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Interface of map content manager for type T.
    /// </summary>
    public interface IMapContentManager<T> where T : IMapContent
    {
        /// <summary>
        /// Gets every map content at the given position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>A list the contains every map content at the given position.</returns>
        HashSet<T> GetContents(RCNumVector position);

        /// <summary>
        /// Gets every map content inside the given selection box.
        /// </summary>
        /// <param name="selectionBox">The selection box.</param>
        /// <returns>A list the contains every map content inside the given selection box.</returns>
        HashSet<T> GetContents(RCNumRectangle selectionBox);

        /// <summary>
        /// Attaches the given content to this content manager.
        /// </summary>
        /// <param name="content">The content to be attached.</param>
        void AttachContent(T content);

        /// <summary>
        /// Detaches the given content from this content manager.
        /// </summary>
        /// <param name="content">The content to be detached.</param>
        void DetachContent(T content);

        /// <summary>
        /// Gets whether the given content has been attached to this content manager.
        /// </summary>
        /// <param name="content">The content to be checked.</param>
        /// <returns>True if the given content has been attached to this content manager, otherwise false.</returns>
        bool HasContent(T content);
    }
}
