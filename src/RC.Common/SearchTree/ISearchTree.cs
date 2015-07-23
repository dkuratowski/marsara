using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Interface of search-trees for searching contents of type T.
    /// </summary>
    public interface ISearchTree<T> where T : ISearchTreeContent
    {
        /// <summary>
        /// Gets every content attached to this search tree.
        /// </summary>
        /// <returns>A list that contains every content attached to this search tree.</returns>
        RCSet<T> GetContents();

        /// <summary>
        /// Gets every content at the given position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>A list that contains every content at the given position.</returns>
        RCSet<T> GetContents(RCNumVector position);

        /// <summary>
        /// Gets every content inside the given rectangular area.
        /// </summary>
        /// <param name="area">The rectangular area.</param>
        /// <returns>A list that contains every content inside the given rectangular area.</returns>
        RCSet<T> GetContents(RCNumRectangle area);

        /// <summary>
        /// Attaches the given content to this search tree.
        /// </summary>
        /// <param name="content">The content to be attached.</param>
        void AttachContent(T content);

        /// <summary>
        /// Detaches the given content from this search tree.
        /// </summary>
        /// <param name="content">The content to be detached.</param>
        void DetachContent(T content);

        /// <summary>
        /// Gets whether the given content has been attached to this search tree.
        /// </summary>
        /// <param name="content">The content to be checked.</param>
        /// <returns>True if the given content has been attached to this search tree, otherwise false.</returns>
        bool HasContent(T content);
    }
}
