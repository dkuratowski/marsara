using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Defines an interface for accessing the sprites of a sprite group.
    /// </summary>
    public interface ISpriteGroup
    {
        /// <summary>
        /// Gets the sprite at the given index.
        /// </summary>
        /// <param name="index">The index of the sprite to get.</param>
        /// <returns>The sprite at the given index.</returns>
        UISprite this[int index] { get; }
    }
}
